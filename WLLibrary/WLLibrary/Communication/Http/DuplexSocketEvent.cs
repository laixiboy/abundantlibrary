
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using WLLibrary.DataStructure;
using WLLibrary.Log;

namespace WLLibrary.Communication.Http
{
    internal class DuplexSocketEvent
    {       
        #region private 
        /// <summary>
        /// 创建时间
        /// </summary>
        private long _ticksBorn = 0;

        /// <summary>
        /// 接收缓冲区
        /// add.wl.20131114
        /// </summary>
        private byte[] _RecvBuffer = null;

        /// <summary>
        /// Received Data's End Index's Next Index In Buffer's Cache Range
        /// </summary>
        private int _idxRecvEnd = 0;

        /// <summary>
        /// Profile容器
        /// </summary>
        private StringBuilder _profileContain = new StringBuilder(100);

        /// <summary>
        /// 等待HttpResponse
        /// </summary>
        //private bool _waitHttpResponse = false;

        private bool _isBreak = false;

        private int _portListen = 0;

        /// <summary>
        /// @brief:标记有没有上送过数据
        /// </summary>
        private volatile bool _isReceived = false;

        #endregion

        #region public

        /// <summary>
        /// 接收数据后的回调
        /// param1：suid
        /// param2：socketId
        /// param3:receive buff
        /// param4:index for read
        /// param5:request body's data length
        /// </summary>
        public Func<CommuEngine.CommuParam, CommuEngine.ReceiveBuffParam, bool> AfterReceiveCallBack = null;
        /// <summary>
        /// 发送数据之前的处理
        /// </summary>
        public Func<byte[], byte[]> BeforeSendCallBack = null;

        /// <summary>
        /// 出生时间
        /// </summary>
        public long ticksBorn
        {
            get { return _ticksBorn; }
        }

        public SocketAsyncEventArgs eSend = null;
        public SocketAsyncEventArgs eReceive = null;

        /// <summary>
        /// the core info for exception log
        /// </summary>
        public string Profile
        {
            get 
            {
                try
                {
                    _profileContain.Append(this.commuParam.ip).Append(":").Append(this.commuParam.port.ToString())
                        .Append(",").Append(this.commuParam.socketId.ToString())
                        .Append("/s");

                    return _profileContain.ToString();
                }
                catch
                {
                    return string.Empty;
                }
                finally
                {
                    _profileContain.Remove(0, _profileContain.Length);
                }                
            }
        }

        /// <summary>
        /// @brief 通讯参数
        /// </summary>
        public CommuEngine.CommuParam commuParam = new CommuEngine.CommuParam();

        /// <summary>
        /// 发送数据缓冲池
        /// </summary>
        public AsyncQueue<byte[]> SendPool = new AsyncQueue<byte[]>();

        /// <summary>
        /// Socket发送事件缓冲区
        /// 多线程共享 SEND_STATUS控制锁
        /// </summary>
        public byte[] SendBuffer = null;

        /// <summary>
        /// SendBuffer中数据长度
        /// 多线程共享 SEND_STATUS控制锁
        /// </summary>
        public int waitSendLen = 0;

        /// <summary>
        /// 发送开始的Ticks
        /// </summary>
        public long sendBeginTicks = DateTime.Now.Ticks;

        /// <summary>
        /// @brief:是否是断开状态
        /// </summary>
        public bool IsBreak
        {
            get { return this._isBreak; }
        }
   
        #endregion

        #region Const

        public const int SOCKETID_INVALID = -1;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socketID">对应一维在线数组的索引位置</param>
        public DuplexSocketEvent(int sndBufSize,int revBufSize,int socketID,int portListen)
        {
            this.SendBuffer = new byte[sndBufSize];
            this._RecvBuffer = new byte[revBufSize*2];
            this._idxRecvEnd = 0;
            this.waitSendLen = 0;
            this.commuParam.socketId = socketID;
            this._portListen = portListen;

            this.eReceive = new SocketAsyncEventArgs();
            this.eSend = new SocketAsyncEventArgs();
            this.eReceive.UserToken = this.eSend.UserToken = this;
            this.eReceive.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceiveCompleted);
            this.eSend.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnSendCompleted);
            this.eReceive.SetBuffer(this._RecvBuffer,revBufSize, revBufSize);

            this.Initialize();
        }

        public void Initialize()
        {
            this.SendPool.Clear();
            this._ticksBorn = DateTime.Now.Ticks;
            this.commuParam.ip = string.Empty;
            this.commuParam.port = 0;
            this.waitSendLen = 0;
            this._idxRecvEnd = 0;
            this.sendBeginTicks = DateTime.Now.Ticks;
            this.eSend.SetBuffer(null, 0, 0);
            this.commuParam.suId = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(this.commuParam.socketId.ToString() + this._portListen.ToString() + DateTime.Now.Ticks.ToString(), "MD5");
            //this._waitHttpResponse = false;
            this._isBreak = false;
            this._isReceived = false;
        }

        /// <summary>
        /// @brief:关闭Socket
        /// @invariant:内部对Socket==null,Connected进行判断
        /// </summary>
        public void Break(CommuBreak offline)
        {
            CommuEngine.Instance.AddEvent(this.commuParam.socketId, EventType.Event_StopCommu);

            LogEngine.Write(LOGTYPE.DEBUG, "Break Because:", offline.ToString(), ",", this.commuParam.socketId.ToString(), "/sid");
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="e"></param>
        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success || e.BytesTransferred < 1)
                {
                    if (e.SocketError == SocketError.ConnectionReset)
                    {
                        this.Break(CommuBreak.CLIENT_RST);
                    }
                    else if (e.SocketError == SocketError.OperationAborted)
                    {
                        //do nothing
                    }
                    else
                    {
                        this.Break(CommuBreak.CLIENT);
                        LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") Receive Close:", e.SocketError.ToString());
                    }
                }
                else
                {
                    if (!this._isReceived)
                        this._isReceived = true;

                    if (AfterReceive(e.BytesTransferred))
                    {
                        if (e.AcceptSocket != null && !e.AcceptSocket.ReceiveAsync(e))
                        {
                            this.ProcessReceive(e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, ex.ToString());
            }
        }

        /// <summary>
        /// 发送完毕后处理
        /// </summary>
        /// <param name="e"></param>
        public void ProcessSend(SocketAsyncEventArgs e)
        {
            try
            {
                e.SetBuffer(null, 0, 0);// 清理发送缓冲区
                this.waitSendLen = 0;

                #region 发送速度检测

                long timeSpanTicks = DateTime.Now.Ticks - this.sendBeginTicks;
                if (timeSpanTicks > CommuEngine.MAX_WAIT_SEND_COMPLETE_TIME)//10s
                {
                    this.Break(CommuBreak.CLIENT_PROC_SLOW);
                }
                else
                {
                    CommuEngine.Instance.AddEvent(this.commuParam.socketId, EventType.Event_SendComplet);
                }

                #endregion
            }
            catch (Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, ex.ToString());
            }
        }

        /// <summary>
        /// 接受数据回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (this.eReceive != null)
                this.ProcessReceive(e);
        }

        /// <summary>
        /// 发送数据回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(this.eSend!=null)
                this.ProcessSend(e);
        }

        /// <summary>
        /// 解析接收的数据,提取核心数据和关键属性，加入缓冲
        /// </summary>
        public bool AfterReceive(int bytesTransferred)
        {
            LogEngine.Write(LOGTYPE.DEBUG, "HTTP Request Come,SocketId:", this.commuParam.socketId.ToString(), ",bytesTransfer:", bytesTransferred.ToString());

            int idxProcBg = 0,idxProcEnd = 0;
            if (this._idxRecvEnd > 0)
            {
                LogEngine.Write(LOGTYPE.DEBUG, "HTTP Request Come Again,SocketId:", this.commuParam.socketId.ToString(), ",bytesTransfer:", bytesTransferred.ToString());

                idxProcBg = 0;
                Buffer.BlockCopy(this._RecvBuffer,this._RecvBuffer.Length/2, this._RecvBuffer, this._idxRecvEnd, bytesTransferred);
                this._idxRecvEnd += bytesTransferred;
                idxProcEnd = this._idxRecvEnd;
            }
            else
            {
                idxProcBg = this._RecvBuffer.Length / 2;
                idxProcEnd = idxProcBg+bytesTransferred;
            }

            int lenLast = 0;//剩余等待分析的报文长度

            try
            {
                if (idxProcEnd <= idxProcBg)
                {
                    LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") Receive Trigger,But Not Find Data,idxBg:",
                            idxProcBg.ToString(),
                            ",idxEd:", idxProcEnd.ToString());
                    return true;
                }

                lenLast = idxProcEnd - idxProcBg;
                if (lenLast >= this._RecvBuffer.Length / 2)
                {
                    LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") Receive Datagram Too Long Than:",
                            (this._RecvBuffer.Length / 2).ToString());

                    this.Break(CommuBreak.CLIENT_DATAGRAM_TOOLONG);
                    return false;
                }
                         
                #region Analyse ContentLength

                string httpRequest = Encoding.UTF8.GetString(this._RecvBuffer, idxProcBg, lenLast);
                //Check \r\n\r\n
                int idxSplit = httpRequest.IndexOf("\r\n\r\n");
                if (idxSplit >= 0)
                {
                    //header may be not complet,wait for next receive
                    string[] headerItems = httpRequest.Split(new char[] { '\r', '\n' });
                    int contentLength = -1;

                    foreach (string headerItem in headerItems)
                    {
                        //分析出ContentType,ContentLength
                        if (!headerItem.Equals(string.Empty))
                        {
                            if ((contentLength = headerItem.ToLower().IndexOf("content-length")) > -1)
                            {
                                string[] contentLengths = headerItem.Split(':');
                                if (contentLengths.Length > 1)
                                {
                                    contentLength = Convert.ToInt32(contentLengths[1].Trim());

                                    if (contentLength > 0 && contentLength > this._RecvBuffer.Length / 2)
                                    {
                                        //LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") Content-Length Too Long Than:",
                                        //            (this._RecvBuffer.Length / 2).ToString());


                                        this.Break(CommuBreak.CLIENT_DATAGRAM_TOOLONG);
                                        return false;
                                    }
                                }
                                else
                                {
                                    //LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") AfterReceive Content-Length Not Complete:", httpRequest);
                                }
                                break;
                            }
                        }
                    }

                    if (contentLength == -1)
                    {
                        //can't find content-length sign,set length=0
                        contentLength = 0;
                    }

                #endregion

                    if (contentLength >= 0)
                    {
                        int idxRequestBodyEnd = idxProcBg+idxSplit + 3 +contentLength;//point to end index of request body                        
                        if(idxRequestBodyEnd<idxProcEnd)
                        {
                            //LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") After Receive:", httpRequest, ",Content-Length:", contentLength.ToString());

                            #region Build MessageData

                            try
                            {
                                CommuEngine.ReceiveBuffParam receiveBuffParam = new CommuEngine.ReceiveBuffParam();
                                receiveBuffParam.recvBuffer = this._RecvBuffer;
                                receiveBuffParam.idxRequestBody = idxProcBg + idxSplit + 4;
                                receiveBuffParam.lengthRequestBody = contentLength;

                                //this._waitHttpResponse = true;
                                if (!AfterReceiveCallBack(this.commuParam,receiveBuffParam))
                                {
                                    this.Break(CommuBreak.SERVER_ANALYSE_EXPECT);
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogEngine.Write(LOGTYPE.ERROR, "AfterReceive BuildMessageData:", ex.ToString());

                                this.Break(CommuBreak.SERVER_ANALYSE_EXPECT);
                                return false;
                            }

                            #endregion

                            this._idxRecvEnd = 0;
                            lenLast = 0;
                        }
                        else
                        {
                            //LogEngine.Write(LOGTYPE.DEBUG, "Http(", this.commuParam.socketId.ToString(), ") AfterReceive Request Data Body Not Complete:", httpRequest);
                        }
                    }
                }

                //移动不完整的已接收数据
                if (this._idxRecvEnd < 1 && lenLast>0)
                {
                    //Data Copy To RecvBuff's Cache Range
                    Buffer.BlockCopy(this._RecvBuffer, idxProcBg, this._RecvBuffer, this._idxRecvEnd, lenLast);
                    this._idxRecvEnd += lenLast;
                }                    
            }
            catch(Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "AfterReceive:", ex.ToString());

                this.Break(CommuBreak.SERVER_ANALYSE_EXPECT);         
                return false;
            }

            return true;
        }

        /// <summary>
        /// @brief:设置断开，调用后不接收任何Send，Receive操作
        /// </summary>
        public void SetBreak()
        {
            this._isBreak = true;
        }

        /// <summary>
        /// @brief:第一次连接以后默认X秒后自动回调的处理
        /// </summary>
        /// <param name="param"></param>
        public void ConnectOnTimer(object param)
        {
            if (!this._isReceived)
            {
                this.Break(CommuBreak.CLIENT_VALID_CONNECT);
            }
        }
    }
}