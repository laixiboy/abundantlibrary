
//#define V1_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Collections.Concurrent;

/**
 * @func：带锁的Byte[]的QUEUE 
 * @author:wolan
 * @attention:不支持快速遍历
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Writes-Readers
    /// Write Lock,Read Lock
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class AsyncBytesQueue
    {
#if V1_0
        private Queue<byte[]> m_Queue = null;
        private object _lock = null;
        private int _capacity = 0;

        public AsyncBytesQueue()
        {
            this.m_Queue = new Queue<byte[]>();
            this._lock = ((ICollection)this.m_Queue).SyncRoot;
        }

        public AsyncBytesQueue(int nCapacity)
        {
            this._capacity = nCapacity;
            this.m_Queue = new Queue<byte[]>(nCapacity);
            this._lock = ((ICollection)this.m_Queue).SyncRoot;
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public Int32 Count
        {
            get
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    return this.m_Queue.Count;
                }
                finally
                {
                    System.Threading.Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public void Clear()
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                this.m_Queue.Clear();
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <returns></returns>
        public byte[] Dequeue()
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                if (this.m_Queue.Count > 0)
                {
                    return this.m_Queue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// [Safe] 批量弹出队列
        /// </summary>
        public byte[][] DequeueToArray()
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                if (this.m_Queue.Count > 0)
                {
                    return this.m_Queue.ToArray();
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                this.m_Queue.Clear();
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// [Safe] 批量弹出队列
        /// </summary>
        /// <param name="nMaxNum"></param>
        /// <param name="queGet">0：queGet为null；>0：queGet调用者申请</param>
        public void MultiDequeue(int nMaxNum,Queue<byte[]> queGet)
        {
            if (nMaxNum < 1)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    queGet = new Queue<byte[]>(this.m_Queue.Count);
                    while(this.m_Queue.Count>0)
                    {
                        queGet.Enqueue(this.m_Queue.Dequeue());
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_lock);
                }
            }
            else
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    nMaxNum = this.m_Queue.Count > nMaxNum ? nMaxNum : this.m_Queue.Count;
                    while (nMaxNum>0)
                    {
                        queGet.Enqueue(this.m_Queue.Dequeue());
                        nMaxNum--;
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_lock);
                }
            }
        }

        public int DequeueToList(int nMaxNum, byte[][] aryGet)
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                nMaxNum = this.m_Queue.Count > nMaxNum ? nMaxNum : this.m_Queue.Count;
                int nBeginIdx = 0;
                while (nMaxNum > 0)
                {
                    aryGet[nBeginIdx++] = this.m_Queue.Dequeue();
                    nMaxNum--;
                }
                return nBeginIdx;
            }
            catch { return 0; }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(byte[] item)
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                this.m_Queue.Enqueue(item);
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// [Safe]加入队列时间限制总条数，若总条数超限则不会添加
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nMaxNum"></param>
        /// <returns>true:加入成功 false:加入失败</returns>
        public bool EnqueueInLimit(byte[] item,int nMaxNum)
        {
            bool blnRet = true;
            System.Threading.Monitor.Enter(_lock);
            try
            {
                if (this.m_Queue.Count < nMaxNum)
                    this.m_Queue.Enqueue(item);
                else
                    blnRet = false;
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }

            return blnRet;
        }
        public bool EnqueueInLimit(byte[] item)
        {
            bool blnRet = true;
            System.Threading.Monitor.Enter(_lock);
            try
            {
                if (this.m_Queue.Count < this._capacity)
                    this.m_Queue.Enqueue(item);
                else
                    blnRet = false;
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }

            return blnRet;
        }

        /// <summary>
        /// [Safe] 批量数据入队列
        /// </summary>
        /// <param name="item"></param>
        public void MultiEnqueue(ref List<byte[]> listItem)
        {
            if (listItem != null && listItem.Count > 0)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    foreach (byte[] item in listItem)
                    {
                        this.m_Queue.Enqueue(item);
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// [Safe] 批量数据入队列
        /// </summary>
        /// <param name="item"></param>
        public void MultiEnqueue(ref byte[][] listItem)
        {
            if (listItem != null && listItem.Length > 0)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    foreach (byte[] item in listItem)
                    {
                        this.m_Queue.Enqueue(item);
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// 添加元素进队列，若元素已在队列中，则不再重复添加
        /// </summary>
        /// <param name="item"></param>
        public void EnqueueWithNoRepeat(byte[] item)
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                if (!this.m_Queue.Contains(item))
                {
                    this.m_Queue.Enqueue(item);
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// 判断是否达到最大容量
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            System.Threading.Monitor.Enter(_lock);
            try
            {
                return this.m_Queue.Count >= this._capacity;
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// @brief:[Safe]将不超过capacity容量的数据存入dstBuffer
        /// @invariant:此函数仅适用于TValue是byte[]时
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="idxBg">开始位置</param>
        /// <param name="capacity"></param>
        /// <param name="lastCount">容器内的剩余条数</param>
        /// <returns>本次得到的字节数</returns>
        public int DequeuToByteArray(byte[] dstBuffer, int idxBg,int capacity,ref int lastCount)
        {
            int ret = 0;

            byte[] temp = null;
            System.Threading.Monitor.Enter(_lock);
            try
            {
                while (this.m_Queue.Count > 0 && capacity>0)
                {
                    temp = this.m_Queue.First();
                    if (temp.Length > capacity)
                    {
                        break;
                    }
                    else
                    {
                        temp = this.m_Queue.Dequeue();
                        Buffer.BlockCopy(temp, 0, dstBuffer, idxBg + ret, temp.Length);
                        ret += temp.Length;
                        capacity -= temp.Length;
                    }                        
                }
                lastCount = this.m_Queue.Count;
            }
            finally
            {
                System.Threading.Monitor.Exit(_lock);
            }

            return ret;
        }
#else
        private ConcurrentQueue<byte[]> m_Queue = null;

        public AsyncBytesQueue()
        {
            this.m_Queue = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public Int32 Count
        {
            get
            {
                return this.m_Queue.Count;
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <returns></returns>
        public byte[] Dequeue()
        {
            byte[] value = null;
            this.m_Queue.TryDequeue(out value);
            return value;
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(byte[] item)
        {
            this.m_Queue.Enqueue(item);
        }
    
        /// <summary>
        /// [Safe] 批量数据入队列
        /// </summary>
        /// <param name="item"></param>
        public void MultiEnqueue(ref List<byte[]> listItem)
        {
            if (listItem != null && listItem.Count > 0)
            {
                foreach (byte[] item in listItem)
                {
                    this.m_Queue.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// @brief:[Safe]将不超过capacity容量的数据存入dstBuffer
        /// @invariant:此函数仅适用于TValue是byte[]时
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="idxBg">开始位置</param>
        /// <param name="capacity"></param>
        /// <param name="lastCount">容器内的剩余条数</param>
        /// <returns>本次得到的字节数</returns>
        public int DequeuToByteArray(byte[] dstBuffer, int idxBg, int capacity, ref int lastCount)
        {
            int ret = 0;

            byte[] temp = null;

            while (this.m_Queue.Count > 0 && capacity > 0)
            {
                if (this.m_Queue.TryPeek(out temp))
                {
                    if (temp.Length > capacity)
                    {
                        break;
                    }
                    else
                    {
                        if (this.m_Queue.TryDequeue(out temp))
                        {
                            Buffer.BlockCopy(temp, 0, dstBuffer, idxBg + ret, temp.Length);
                            ret += temp.Length;
                            capacity -= temp.Length;
                        }
                        else
                            break;
                    }
                }
                else
                    break;
            }
            lastCount = this.m_Queue.Count;

            return ret;
        }
#endif
    }
}
