using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Configuration;
using System.Net;
using System.IO;
using System.Threading;
using WLLibrary.Log;

#pragma warning disable 420
namespace WLLibrary.Communication.Tcp
{
    internal class TCPListener
    {
        private Socket _listener = null;
        private int _port = 0;

        #region Static Public

        public static SocketAsyncEventArgs s_ListenClient = null;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strThreadName"></param>
        public TCPListener(int port)
        {
            this._port = port;
        }

        #region Static Function

        public static void Start(int port,List<Thread> threads)
        {
            TCPListener tcpl = new TCPListener(port);
            WLLibrary.ThreadHandle.StartBackgroundThread(tcpl.Listen, threads);
        }

        #endregion

        /// <summary>
        /// 开始监听
        /// </summary>
        public void Listen()
        {
            try
            {
                this._listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this._listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                this._listener.NoDelay = true;
                this._listener.ReceiveBufferSize = CommuEngine.SIZE_RECV_BUFF;
                this._listener.SendBufferSize = CommuEngine.SIZE_SEND_BUFF;
                this._listener.Bind(new IPEndPoint(IPAddress.Any, this._port));

                this._listener.Listen(0xFFFF);
                this.StartAccept(ref TCPListener.s_ListenClient);
            }
            catch (Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, "【严重】初始化监听失败,原因:" + ex.ToString());
            }
        }

        /// <summary>
        /// 开始接受连接
        /// </summary>
        /// <param name="e"></param>
        private void StartAccept(ref SocketAsyncEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    e.AcceptSocket = null;
                }
                else
                {
                    e = new SocketAsyncEventArgs();
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                }

                if (!this._listener.AcceptAsync(e))
                {
                    this.ProcessAccept(e);
                }
            }
            catch (Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, "【严重】启动监听失败,原因:" + ex.ToString());
            }
        }

        /// <summary>
        /// 接受连接IOCP回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.AcceptSocket != null && e.AcceptSocket.Connected)
                {
                    try
                    {
                        DuplexSocketEvent duplexSocket = CommuEngine.Instance.GetFreeDuplexSocket();
                        if (duplexSocket != null)
                        {
                            IPEndPoint ipepAcceptAddress = (IPEndPoint)e.AcceptSocket.RemoteEndPoint;
                            duplexSocket.eReceive.AcceptSocket = duplexSocket.eSend.AcceptSocket = e.AcceptSocket;

                            duplexSocket.commuParam.ip = ipepAcceptAddress.Address.ToString();
                            duplexSocket.commuParam.port = ipepAcceptAddress.Port;

                            CommuEngine.Instance.AddEvent(duplexSocket.SocketID,EventType.Event_New);
                        }
                        else
                        {
                            //e.AcceptSocket.Shutdown(SocketShutdown.Both);
                            e.AcceptSocket.Close();
                        }

                        e.AcceptSocket = null;
                    }
                    catch (Exception)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, ex.ToString());
                    }        
                }
            }
            catch (Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, "【严重】监听处理异常，原因:" + ex.ToString());
            }
            finally
            {
                this.StartAccept(ref e);
            }
        }
    }
}