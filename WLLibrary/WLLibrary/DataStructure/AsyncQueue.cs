
//#define V1_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

/**
 * @func：带锁的QUEUE 
 * @author:wolan
 * @attention:不支持快速遍历
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
#if V1_0
    /// <summary>
    /// Writes-Readers
    /// Write Lock,Read Lock
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class AsyncQueue<TValue>
    {
        private Queue<TValue> m_Queue = null;
        private object _lock = null;
        private int _capacity = 0;

        public AsyncQueue()
        {
            this.m_Queue = new Queue<TValue>();
            this._lock = ((ICollection)this.m_Queue).SyncRoot;
        }

        public AsyncQueue(int nCapacity)
        {
            this._capacity = nCapacity;
            this.m_Queue = new Queue<TValue>(nCapacity);
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
        public TValue Dequeue()
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
                    return default(TValue);
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
        public TValue[] DequeueToArray()
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
        public void MultiDequeue(int nMaxNum,Queue<TValue> queGet)
        {
            if (nMaxNum < 1)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    queGet = new Queue<TValue>(this.m_Queue.Count);
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

        public int DequeueToList(int nMaxNum, TValue[] aryGet)
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
        public void Enqueue(TValue item)
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
        public bool EnqueueInLimit(TValue item,int nMaxNum)
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

        /// <summary>
        /// [Safe] 批量数据入队列
        /// </summary>
        /// <param name="item"></param>
        public void MultiEnqueue(ref List<TValue> listItem)
        {
            if (listItem != null && listItem.Count > 0)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    foreach(TValue item in listItem)
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
        public void MultiEnqueue(ref TValue[] listItem)
        {
            if (listItem != null && listItem.Length > 0)
            {
                System.Threading.Monitor.Enter(_lock);
                try
                {
                    foreach (TValue item in listItem)
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
        public void EnqueueWithNoRepeat(TValue item)
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
#else
    /// <summary>
    /// Writes-Readers
    /// Write Lock,Read Lock
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class AsyncQueue<TValue>
    {
        private ConcurrentQueue<TValue> m_Queue = null;

        public AsyncQueue()
        {
            this.m_Queue = new ConcurrentQueue<TValue>();
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

        public TValue First()
        {
            return this.m_Queue.ElementAt(0);
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <returns></returns>
        public TValue Dequeue()
        {
            TValue value = default(TValue);
            this.m_Queue.TryDequeue(out value);
            return value;
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(TValue item)
        {
            this.m_Queue.Enqueue(item);
        }

        /// <summary>
        /// [Safe]加入队列时间限制总条数，若总条数超限则不会添加
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nMaxNum"></param>
        /// <returns>true:加入成功 false:加入失败</returns>
        public bool EnqueueInLimit(TValue item, int numMax)
        {
            if (this.m_Queue.Count < numMax)
                this.m_Queue.Enqueue(item);
            else
                return false;

            return true;
        }

        /// <summary>
        /// [Safe] 批量弹出队列
        /// </summary>
        /// <returns>实际返回长度</returns>
        public int DequeueToArray(TValue[] array)
        {
            int numReal = 0;
            int numMax = array.Length;
            TValue value = default(TValue);
            while (numReal<numMax && this.m_Queue.TryDequeue(out value))
            {
                array[numReal++]=value;
            }
            return numReal;
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public void Clear()
        {
            TValue value = default(TValue);
            while (this.m_Queue.TryDequeue(out value))
            {}
        }
#endif
    }
}
