using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/**
 * @func：带锁的List 
 * @author:wolan
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Readers-Writers
    /// Reader Lock,Writer Lock
    /// 适合不多于100个数据的小规模数据量
    /// </summary>
    public class AsyncList<TValue>
    {
        /// <summary>
        /// unsafe
        /// </summary>
        public List<TValue> m_list = null;
        private object objLock = null;
        private int _capacity = 0;

        public AsyncList()
        {
            m_list = new List<TValue>();
            objLock = ((ICollection)this.m_list).SyncRoot;
        }

        public AsyncList(int nCapacity)
        {
            this._capacity = nCapacity;
            m_list = new List<TValue>(nCapacity);
            objLock = ((ICollection)this.m_list).SyncRoot;
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public Int32 Count
        {
            get
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    return this.m_list.Count;
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="value"></param>
        public bool IsFull()
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                return this.m_list.Count >= this._capacity;
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="value"></param>
        public void Add(TValue value)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                this.m_list.Add(value);
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]限制最大容量下的增加，若满，则顶掉开头的数据
        /// </summary>
        /// <param name="value"></param>
        public void Add_LimitCapacity(TValue value)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                if (this.m_list.Count >= this._capacity)
                {
                    this.m_list.RemoveAt(0);
                }
                this.m_list.Add(value);
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="value"></param>
        public void Clear()
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                this.m_list.Clear();
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="nMaxNum"></param>
        /// <param name="liGet">0：liGet为null；>0：liGet调用者申请</param>
        public void DequeueToList(int nMaxNum,List<TValue> liGet)
        {
            if (nMaxNum < 1)
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    liGet = this.m_list.ToList();
                }
                finally
                {
                    this.m_list.Clear();
                    System.Threading.Monitor.Exit(objLock);
                }
            }
            else
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    nMaxNum = this.m_list.Count > nMaxNum ? nMaxNum : this.m_list.Count;
                    while (nMaxNum > 0)
                    {
                        liGet.Add(this.m_list[0]);
                        m_list.RemoveAt(0);
                        nMaxNum--;
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="nMaxNum"></param>
        /// <param name="liGet">0：liGet为null；>0：liGet调用者申请</param>
        public void CopyToList(int nMaxNum, ref List<TValue> liGet)
        {
            if (nMaxNum < 1)
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    liGet = this.m_list.ToList();
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
            else
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    nMaxNum = this.m_list.Count > nMaxNum ? nMaxNum : this.m_list.Count;
                    for (int i = 0; i < nMaxNum;i++ )
                    {
                        liGet.Add(this.m_list[i]);
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        public TValue Dequeue()
        {
            TValue value = default(TValue);
            System.Threading.Monitor.Enter(objLock);
            try
            {
                if (this.m_list.Count > 0)
                {
                    value = this.m_list[0];
                    this.m_list.RemoveAt(0);
                }

                return value;
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="listValue"></param>
        public void MultiAdd(List<TValue> listValue)
        {
            if(listValue!=null && listValue.Count>0)
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {
                    this.m_list.AddRange(listValue);
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="value"></param>
        public void Remove(TValue value)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                this.m_list.Remove(value);
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]
        /// </summary>
        /// <param name="value"></param>
        public void RemoveAt(int idx)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                this.m_list.RemoveAt(idx);
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]加入队列时间限制总条数，若总条数超限则不会添加
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nMaxNum"></param>
        /// <returns>true:加入成功 false:加入失败</returns>
        public bool EnqueueInLimit(TValue item, int nMaxNum)
        {
            bool blnRet = true;
            System.Threading.Monitor.Enter(objLock);
            try
            {
                if (this.m_list.Count < nMaxNum)
                    this.m_list.Add(item);
                else
                    blnRet = false;
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }

            return blnRet;
        }
    }
}
