using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Collections;

/**
 * @func:DuplexSocketAsyncEventArgs池
 * @author:wolan
 * @date:2012/03/26
 **/
namespace WLLibrary.Communication.Tcp
{
    internal sealed class DuplexSocketEventPool
    {
        private Int32 _maxsocketcount = 0;
        //这里不再考虑缓存命中问题，而是规避同样SID
        private Queue<DuplexSocketEvent> _freesocketpool = null;
        private object _lock = null;

        public DuplexSocketEventPool(Int32 nMaxSocketCount)
        {
            _maxsocketcount = nMaxSocketCount;
            _freesocketpool = new Queue<DuplexSocketEvent>(nMaxSocketCount);
            _lock = ((ICollection)_freesocketpool).SyncRoot;
        }

        /// <summary>
        /// [Safe]获取剩余空闲连接数
        /// </summary>
        /// <returns></returns>
        public int GetFreeNum()
        {
            int nRet = -1;

            lock (_lock)
            {
                nRet = _freesocketpool.Count;
            }

            return nRet;
        }

        /// <summary>
        /// [UnSafe]获取剩余空闲连接数的近似数
        /// </summary>
        /// <returns></returns>
        public int GetFreeNum_NoLock()
        {
            try
            {
                return _freesocketpool.Count;
            }
            catch 
            {
                return 0;
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="e"></param>
        public void Push(DuplexSocketEvent e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            else
            {
                if (e.eReceive.AcceptSocket != null)
                {
                    e.eReceive.AcceptSocket = null;
                }

                if (e.eSend.AcceptSocket != null)
                {
                    e.eSend.AcceptSocket = null;
                }

                lock (_lock)
                {
                    _freesocketpool.Enqueue(e);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <returns></returns>
        public DuplexSocketEvent Pop()
        {
            DuplexSocketEvent e = default(DuplexSocketEvent);

            lock (_lock)
            {
                if (_freesocketpool.Count > 0)
                {
                    e = _freesocketpool.Dequeue();
                    e.Initialize();
                }
            }

            return e;
        }
    }
}
