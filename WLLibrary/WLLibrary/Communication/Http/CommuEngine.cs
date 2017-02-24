/**
 * @brief:Processing the Communication
 * @author:wolan email:khyusj@163.com
 * @brief:none
 **/

//#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using WLLibrary.DataStructure;
using WLLibrary.Log;

#pragma warning disable 420
namespace WLLibrary.Communication.Http
{
    public sealed class CommuEngine
    {
        public struct CommuParam
        {
            /// <summary>
            /// @brief:本次TCP会话唯一标示
            /// @attention:不会复用
            /// </summary>
            public string suId;
            /// <summary>
            /// @brief:通讯通道唯一标示
            /// @attention:会复用
            /// </summary>
            public int socketId;

            public string ip;
            public int port;
        }

        public struct ReceiveBuffParam
        {
            /// <summary>
            /// @brief:接收缓存
            /// </summary>
            public byte[] recvBuffer;
            /// <summary>
            /// @brief:RequestBody所在的缓冲区起始位置
            /// </summary>
            public int idxRequestBody;
            /// <summary>
            /// @brief:RequestBody的长度
            /// </summary>
            public int lengthRequestBody;
        }

        #region Private

        /// <summary>
        /// 系统运行标记
        /// </summary>
        private volatile bool _systemRun = true;

        /// <summary>
        /// 提供新连接的连接事件管理类对象池
        /// Listener,CommuThread共用
        /// add.wl.20131114
        /// </summary>
        private DuplexSocketEventPool _DuplexSocketEventPool = null;
        /// <summary>
        /// @brief:Engine线程数
        /// </summary>
        private int _EngineNum = Environment.ProcessorCount*2;//CommuThread's AutoLoad Num
        /// <summary>
        /// @brief:每个Engine线程负责的链接数
        /// </summary>
        private int _ConnNumPerEngine = 1000;
        /// <summary>
        /// @brief:通讯模块的最大连接数
        /// </summary>
        private int _MaxConnectNum = 8000;//同时在线的最大人数

        /// <summary>
        /// 定时器
        /// </summary>
        private MinHeap<OnTimer>[] _timerHeaps = null;

        /// <summary>
        /// @brief 白名单
        /// </summary>
        private HashSet<string> _whiteIps = null;

        #endregion

        #region Public

        /// <summary>
        /// @brief:获取最大连接数量
        /// </summary>
        public int MaxConnectNum
        {
            get { return this._MaxConnectNum; }
        }

        /// <summary>
        /// @brief:Get Current Connect Num
        /// </summary>
        /// <returns></returns>
        public int CurConnectNum
        {
            get
            {
                return this._MaxConnectNum - this._DuplexSocketEventPool.GetFreeNum();
            }
        }

        /// <summary>
        /// @brief 白名单
        /// </summary>
        public HashSet<string> WhiteIps
        {
            get { return this._whiteIps; }
        }

        #endregion

        #region Static\Const

        private static CommuEngine _s_instance = null;
        /// <summary>
        /// @brief:DuplexSocketEvent列表
        /// </summary>
        private static DuplexSocketEvent[] _s_DuplexSocketEvents = null;
        /// <summary>
        /// @brief:缓存其他模块对CommuEngine的命令
        /// </summary>
        private static AsyncQueue<UInt64>[] _s_CommuEngineEvents = null;
        /// <summary>
        /// @brief:DuplexSocketEvents事件记录
        /// </summary>
        private static int[] _s_EventsPerSocket = null;
        private const int MTU_SEND = 1472;

        /// <summary>
        /// @brief:Buffer缓存大小
        /// @variant:需要兼顾文件发送的上限
        /// </summary>
        internal const int SIZE_SEND_BUFF = 1024 * 256;
        internal const int SIZE_RECV_BUFF = 1024 * 10;

        /// <summary>
        /// @brief:单次发送的最长容忍时间（单位：ticks）
        /// </summary>
        internal const int MAX_WAIT_SEND_COMPLETE_TIME = 100000000;

        public const int s_MaxConnNum = 8000;//同时在线的最大人数

        /// <summary>
        /// @brief:请求的最大等待回复时间（秒）
        /// </summary>
        public static int s_RequestTimeOutTicks = 10;

        /// <summary>
        /// 单次循环次数
        /// </summary>
        public const int POOL_MAX_LOOP = 500;

        #endregion

        public CommuEngine()
        {

        }

        #region Static Function

        public static CommuEngine Instance
        {
            get
            {
                if (CommuEngine._s_instance == null)
                    CommuEngine._s_instance = new CommuEngine();

                return CommuEngine._s_instance;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numConnectMax">最大连接数</param>
        /// <param name="threads">记录系统内开启的线程</param>
        /// <param name="portListen">监听端口</param>
        /// <param name="whiteIps">白名单，哈希表存储的是IP，为null时，关闭白名单过滤</param>
        /// <param name="afterReceiveCallBack">接收到完整的http报文后，系统会调用此函数，在函数内可进行Http Request Body的读取</param>
        /// <param name="beforeSendCallBack">发送报文之前的回调函数，在函数内可进行数据发送前的封装</param>
        public void Start(int numConnectMax,List<Thread> threads,
                int portListen,
                HashSet<string> whiteIps,
                Func<CommuParam,ReceiveBuffParam, bool> afterReceiveCallBack, 
                Func<byte[], byte[]> beforeSendCallBack)
        {
            this._MaxConnectNum = numConnectMax;
            this._whiteIps = whiteIps;
            CommuEngine._s_DuplexSocketEvents = new DuplexSocketEvent[this._MaxConnectNum];
            CommuEngine._s_EventsPerSocket = new int[this._MaxConnectNum];
            this._ConnNumPerEngine = this._MaxConnectNum / this._EngineNum;
            if ((this._MaxConnectNum % this._EngineNum) > 0)
            {
                this._ConnNumPerEngine++;
            }

            #region Init DuplexSocket's DataStructrue

            this._DuplexSocketEventPool = new DuplexSocketEventPool(this._MaxConnectNum);
            for (int i = 1; i < this._MaxConnectNum; i++)
            {
                DuplexSocketEvent duplexSocket = new DuplexSocketEvent(CommuEngine.SIZE_SEND_BUFF, CommuEngine.SIZE_RECV_BUFF, i, portListen);
                duplexSocket.AfterReceiveCallBack = afterReceiveCallBack;
                duplexSocket.BeforeSendCallBack = beforeSendCallBack;
                this._DuplexSocketEventPool.Push(duplexSocket);
                CommuEngine._s_DuplexSocketEvents[i] = duplexSocket;
            }

            #endregion

            CommuEngine._s_CommuEngineEvents = new AsyncQueue<UInt64>[this._EngineNum];
            this._timerHeaps = new MinHeap<OnTimer>[this._EngineNum];
            for (int i = 0; i < this._EngineNum; i++)
            {
                //OnTimer
                this._timerHeaps[i] = new MinHeap<OnTimer>(this._ConnNumPerEngine);
                CommuEngine._s_CommuEngineEvents[i] = new AsyncQueue<UInt64>();
                WLLibrary.ThreadHandle.StartBackgroundParamerterizedThread(CommuEngine.Instance.MainLoop, i, threads);
            }

            WLLibrary.Communication.Http.TCPListener.Start(portListen, threads);
        }

        internal DuplexSocketEvent GetFreeDuplexSocket()
        {
            if (this._DuplexSocketEventPool == null)
                return null;

            return this._DuplexSocketEventPool.Pop();
        }

        public void EnqueueCommuEngineDataPool(byte[] data, int socketId)
        {
            int idx = socketId / this._ConnNumPerEngine;
            DuplexSocketEvent socketEvent = CommuEngine._s_DuplexSocketEvents[socketId];
            data = socketEvent.BeforeSendCallBack(data);
            socketEvent.SendPool.Enqueue(data);
            this.AddEvent(socketId, EventType.Event_Send);
        }

        public void StopSystem()
        {
            this._systemRun = false;
        }

        public void AddEvent(int socketId, EventType events)
        {
            UInt64 eventsData = (UInt64)events << 32;
            eventsData |= (UInt32)socketId;
            int idxAutoLoad = socketId / this._ConnNumPerEngine;
            CommuEngine._s_CommuEngineEvents[idxAutoLoad].Enqueue(eventsData);
        }

        /// <summary>
        /// 接纳新连接，启动接收，发送，断开处理线程主体
        /// </summary>
        public void MainLoop(object state)
        {
            int idxEngine = Convert.ToInt32(state);
            int numCurLoop = 0, numMaxLoop = 0;
            Socket socket = null;
            byte[] sockSendBuf = null;
            DuplexSocketEvent sockDup = null;
            UInt64 eventsOuterData = 0uL;
            int socketId = 0;
            int events = 0;
            byte[] sendEachBuffer = null;

            AsyncQueue<UInt64> eventsOuter = CommuEngine._s_CommuEngineEvents[idxEngine];
            UInt64[] dataPoolOnce = new UInt64[CommuEngine.POOL_MAX_LOOP];
            MinHeap<OnTimer> timerHeap = this._timerHeaps[idxEngine];

            int idxBegin = idxEngine * this._ConnNumPerEngine;
            if (idxBegin == 0)
                idxBegin = 1;

            int idxEnd = (idxEngine + 1) * this._ConnNumPerEngine - 1;
            if (idxEnd > CommuEngine.s_MaxConnNum - 1)
                idxEnd = CommuEngine.s_MaxConnNum - 1;

            while (this._systemRun)
            {
                socket = null;

                try
                {
                    numCurLoop = 0;
                    numMaxLoop = eventsOuter.DequeueToArray(dataPoolOnce);
                    while (numCurLoop < numMaxLoop)
                    {
                        eventsOuterData = dataPoolOnce[numCurLoop++];
                        socketId = (int)(eventsOuterData & 0x00000000FFFFFFFFL);
                        CommuEngine._s_EventsPerSocket[socketId] |= (int)(eventsOuterData >> 32);
                    }

                    for(int i=idxBegin;i<=idxEnd;i++)
                    {
                        events = CommuEngine._s_EventsPerSocket[i];
                        if (events < 1)
                            continue;

#if PROFILE
                        LogEngine.Write(LOGTYPE.INFO, "Client Have Event:", i.ToString(),",EventCode:",events.ToString("X"));
#endif

                        socketId = i;
                        CommuEngine._s_EventsPerSocket[i] = 0;
                        sockDup = CommuEngine._s_DuplexSocketEvents[socketId];

                        if ((events & (int)EventType.Event_New) > 0)
                        {
                            #region New Connect
                            
                            socket = sockDup.eReceive.AcceptSocket;

                            try
                            {
                                if (socket != null && !socket.ReceiveAsync(sockDup.eReceive))
                                {
                                    sockDup.ProcessReceive(sockDup.eReceive);
                                }
                            }
                            catch (System.InvalidOperationException)
                            {
                                //LogEngine.Write(LOGTYPE.ERROR, sockDup.SocketID.ToString(), "/s,",idxEngine.ToString(), "/idx,", sockDup.eReceive.LastOperation.ToString());
                            }
                            catch (Exception)
                            {
                                //LogEngine.Write(LOGTYPE.ERROR, sockDup.SocketID.ToString(), "/s,",idxEngine.ToString(), "/idx,", ex2.Message);
                            }

                            OnTimer timer = new OnTimer(OnTimer.TimerHeapType.ONCE, 10 * 1000, sockDup.ConnectOnTimer);
                            timerHeap.Push(timer);

                            #endregion
                        }
                        else if ((events & (int)EventType.Event_StopCommu) > 0)
                        {
                            #region Break

                            if (sockDup.IsBreak)
                            {
#if PROFILE
                                LogEngine.Write(LOGTYPE.ERROR, "Recycle Repeat:", socketId.ToString());
#endif
                                continue;
                            }

                            try
                            {
                                socket = sockDup.eReceive.AcceptSocket;
                                if (socket != null)
                                {
                                    //socket.Shutdown(SocketShutdown.Both);
                                    socket.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                LogEngine.Write(LOGTYPE.ERROR, "MGR_BREAK:", ex.ToString());
                            }
                            finally
                            {
                                sockDup.eReceive.AcceptSocket = sockDup.eSend.AcceptSocket = null;
                                sockDup.SetBreak();
                                this._DuplexSocketEventPool.Push(sockDup);//second

#if PROFILE
                                LogEngine.Write(LOGTYPE.INFO, "Recycle:", socketId.ToString(), "/sid");
#endif
                            }
                            #endregion
                        }
                        else 
                        {
                            if (sockDup.IsBreak)
                            {
#if PROFILE
                                LogEngine.Write(LOGTYPE.ERROR, "Send After Recycle:", socketId.ToString());
#endif
                                continue;
                            }

                            //if ((events & Event_Receive) > 0)
                            //{
                            //    #region Receive

                            //    #endregion
                            //}
                            if ((events & (int)EventType.Event_SendComplet) > 0)
                            {
                                #region SendComplet

                                if (sockDup.SendPool.Count > 0)
                                    events |= (int)EventType.Event_Send;

                                #endregion
                            }
                            if ((events & (int)EventType.Event_Send) > 0)
                            {
                                #region 处理发放
                                sockSendBuf = sockDup.SendBuffer;
                                while (sockDup.SendPool.Count > 0)
                                {
                                    sendEachBuffer = sockDup.SendPool.First();

                                    #region Concat bytes
                                    if (sendEachBuffer.Length <= sockSendBuf.Length)
                                    {
                                        if (sendEachBuffer.Length <= (sockSendBuf.Length - sockDup.waitSendLen))
                                        {
                                            sendEachBuffer = sockDup.SendPool.Dequeue();
                                            Buffer.BlockCopy(sendEachBuffer, 0, sockSendBuf, sockDup.waitSendLen, sendEachBuffer.Length);
                                            sockDup.waitSendLen += sendEachBuffer.Length;
                                            if (sockDup.waitSendLen >= CommuEngine.MTU_SEND)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                            break;
                                    }
                                    else
                                    {
                                        sockDup.SendPool.Dequeue();//drop data
                                        //LogEngine.Write(LOGTYPE.ERROR, "ProtocolData TooLong Max:", sockSendBuf.Length.ToString(), ",", sockDup.Profile);
                                    }
                                    #endregion
                                }

                                #region ProcessSend

                                if (sockDup.waitSendLen > 0)
                                {
                                    sockDup.eSend.SetBuffer(sockSendBuf, 0, sockDup.waitSendLen);
                                    sockDup.sendBeginTicks = DateTime.Now.Ticks;
                                    socket = sockDup.eSend.AcceptSocket;
                                    try
                                    {
                                        if (socket != null && socket.Connected && !socket.SendAsync(sockDup.eSend))
                                        {
                                            sockDup.ProcessSend(sockDup.eSend);
                                        }
                                    }
                                    catch (SocketException)
                                    {
                                        if (sockDup.eSend.SocketError == SocketError.MessageSize)
                                        {
                                            sockDup.Break(CommuBreak.SERVER_SEND_DATA_SIZE);
                                        }
                                        else
                                        {
                                            sockDup.Break(CommuBreak.SERVER_SEND_EXPECT);
                                        }    
                                    }
                                }
                                #endregion

                                #endregion
                            }
                        }
                    }

                    //OnTimer
                    while (timerHeap.Count > 0 && timerHeap.Root.Weight <= DateTime.Now.Ticks)
                    {
                        if (timerHeap.Root.TimeOutCallBack != null)
                            timerHeap.Root.TimeOutCallBack(null);

                        if (timerHeap.Root.Type == OnTimer.TimerHeapType.ALWAYS)
                        {
                            timerHeap.ChangeWeight(timerHeap.Root.HeapIndex, timerHeap.Root.RefreshWeight());
                        }
                        else
                        {
                            timerHeap.PopRoot();
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    LogEngine.Write(LOGTYPE.ERROR, "CommuEngine MainLoop:", ex.ToString());
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}