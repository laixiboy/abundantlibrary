//add.wl.20140314 调试期标记
#define PROFILE
#define WRITE_LOG


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

namespace WLLibrary.Communication.Tcp
{
    internal class DuplexSocketEvent
    {       
        #region private 
        /// <summary>
        /// 创建时间
        /// </summary>
        private long _ticksBorn = 0;

        /// <summary>
        /// Recv Data's Begin Index Of RecvBuffer 
        /// </summary>
        private int _offsetRecv = 0;

        /// <summary>
        /// Buffer's max length
        /// </summary>
        private int _idxFreeRecv = 0;

        private int _numTotalRecv = 0;

        /// <summary>
        /// Profile容器
        /// </summary>
        private StringBuilder _profileContain = new StringBuilder(100);

        private bool _isBreak = false;

        private int _portListen = 0;

        private Random _rnd = RandomHandle.GenerateRandom();

        /// <summary>
        /// @brief:标记有没有上送过数据
        /// </summary>
        private volatile bool _isReceived = false;

        private ReceiveBuffParam _recvBufferParam = new ReceiveBuffParam();

        #endregion

        #region public

        public CommuParam commuParam = new CommuParam();
        /// <summary>
        /// 接收数据后的回调
        /// return:
        ///     >0:each msg's length
        ///     =0:error
        ///     <0:need more data
        /// </summary>
        public Func<CommuParam,ReceiveBuffParam,int> AfterReceiveCallBack = null;
        /// <summary>
        /// @brief:发送数据之前的处理
        /// param1:object by user oneself defined,eg clientmsg
        /// </summary>
        public Func<object,CommuParam,byte[]> BeforeSendCallBack = null;
        /// <summary>
        /// @brief:通讯底层断开的回调，通常用于逻辑层断开
        /// </summary>
        public Action<CommuParam> BreakCallBack = null;

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
        /// 对应一维在线数组的索引位置
        /// </summary>
        public int SocketID
        {
            get { return this.commuParam.socketId; }
        }

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
        /// 发送数据缓冲池
        /// </summary>
        public AsyncQueue<byte[]> SendPool = new AsyncQueue<byte[]>();

        public string suId
        {
            get { return this.commuParam.suId; }
        }

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
        public DuplexSocketEvent(int sndBufSize,int revBufSize,int ecgBufSize,int socketID,int portListen)
        {
            this.SendBuffer = new byte[sndBufSize];
            this._recvBufferParam.recvBuffer = new byte[ecgBufSize + revBufSize];
            this._offsetRecv = ecgBufSize;
            this._idxFreeRecv = 0;
            this.commuParam.socketId = socketID;
            this._portListen = portListen;

            this.eReceive = new SocketAsyncEventArgs();
            this.eSend = new SocketAsyncEventArgs();
            this.eReceive.UserToken = this.eSend.UserToken = this;
            this.eReceive.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceiveCompleted);
            this.eSend.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnSendCompleted);
            this.eReceive.SetBuffer(this._recvBufferParam.recvBuffer, this._offsetRecv, revBufSize);

            this.commuParam.encyptCode = Convert.ToByte(1 + RandomHandle.RandGet(255,this._rnd));
            this.Initialize();
        }

        public void Initialize()
        {
            this.SendPool.Clear();
            this._ticksBorn = DateTime.Now.Ticks;
            this.commuParam.ip = string.Empty;
            this.waitSendLen = 0;
            this._idxFreeRecv = 0;
            this.sendBeginTicks = DateTime.Now.Ticks;
            this.eSend.SetBuffer(null, 0, 0);
            this.commuParam.suId = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(this.commuParam.socketId.ToString() + this._portListen.ToString() + DateTime.Now.Ticks.ToString(), "MD5");
            this._isBreak = false;
        }

        /// <summary>
        /// @brief:关闭Socket
        /// @invariant:内部对Socket==null,Connected进行判断
        /// </summary>
        public void Break(CommuBreak offline)
        {
            CommuEngine.Instance.AddEvent(this.commuParam.socketId, EventType.Event_StopCommu);

#if WRITE_LOG
            {
                //LogEngine.Write(LOGTYPE.INFO, "Break Because:", offline.ToString(), ",", this._socketid.ToString(), "/sid");
            }
#endif
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
                    }
                }
                else
                {
                    if (!this._isReceived)
                        this._isReceived = true;

                    if ((this._idxFreeRecv + e.BytesTransferred) > this._offsetRecv)
                    {
                        this.Break(CommuBreak.SERVER_PROC_SLOW);
                    }
                    else
                    {
                        if (AfterReceive(e.BytesTransferred))
                        {
                            if (e.AcceptSocket != null && !e.AcceptSocket.ReceiveAsync(e))
                            {
                                this.ProcessReceive(e);
                            }
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
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, ex.ToString());
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
            this._recvBufferParam.idxRead = 0;
            if(this._idxFreeRecv<1)
            {
                this._recvBufferParam.idxRead = this._offsetRecv;
                this._idxFreeRecv = this._offsetRecv + bytesTransferred;
                this._numTotalRecv = bytesTransferred;
            }
            else
            {
                Buffer.BlockCopy(this._recvBufferParam.recvBuffer, this._offsetRecv, this._recvBufferParam.recvBuffer, this._idxFreeRecv, bytesTransferred);
                this._idxFreeRecv += bytesTransferred;
                this._numTotalRecv = this._idxFreeRecv;
            }

            this._recvBufferParam.recvLength = 0;//剩余等待分析的报文长度
            try
            {
                if (this._idxFreeRecv > 0)
                {
                    while (this._idxFreeRecv > this._recvBufferParam.idxRead)
                    {
                        this._recvBufferParam.recvLength = _idxFreeRecv - this._recvBufferParam.idxRead;
                        if (this._recvBufferParam.recvLength > 0)
                        {                           
                            #region Build MessageData

                            try
                            {
                                int analyseLen = AfterReceiveCallBack(this.commuParam,this._recvBufferParam);
                                if (analyseLen ==0)
                                {
                                    this.Break(CommuBreak.SERVER_ANALYSE_EXPECT);
                                    break;
                                }
                                else if (analyseLen >0)
                                {
                                    this._recvBufferParam.idxRead += analyseLen;
                                }                               
                            }
                            catch
                            {
                                this.Break(CommuBreak.SERVER_ANALYSE_EXPECT);
                                return false;
                            }

                            #endregion
                        }
                        else
                            break;                        
                    }

                    if (this._recvBufferParam.idxRead < this._idxFreeRecv)
                    {
                        Buffer.BlockCopy(this._recvBufferParam.recvBuffer, this._recvBufferParam.idxRead, 
                            this._recvBufferParam.recvBuffer, 0, this._idxFreeRecv - this._recvBufferParam.idxRead);
                    }
                    this._idxFreeRecv -= this._recvBufferParam.idxRead;

                    if (this._idxFreeRecv < 0)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, "DuplexSocket Analyse Idx:",this._recvBufferParam.idxRead.ToString(),",Ed:",this._idxFreeRecv.ToString());
                        this._idxFreeRecv = 0;
                    }
                }
            }
            catch
            {
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