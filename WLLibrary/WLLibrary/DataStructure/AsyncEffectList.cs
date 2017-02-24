using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/**
 * @brief：多线程安全的链表，支持快速遍历，不支持容量扩展 
 * @author:wolan
 * @attention:必须指定容量，后期使用中不能扩容,插入，删除效率略低于AsyncQueue
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Readers-Writers
    /// Reader Lock,Writer Lock
    /// </summary>
    public class AsyncEffectList<TValue>
    {
        /// <summary>
        /// unsafe
        /// 数据可直接删除
        /// </summary>
        public LinkedList<TValue> m_list = null;
        private object objLock = null;
        private int _capacity = 0;
        private int _readidx = 0;
        private int _writeidx = 0;
        private int _bg_idx_init = 0;
        private int _ed_idx_init = 0;

        /// <summary>
        /// unsafe
        /// 数据不可直接删除，操作_arraybeginidx,_arrayendidx
        /// </summary>
        public TValue[] ArrayValue = null;

        /// <summary>
        /// ArrayValue的开始索引
        /// 从0开始
        /// </summary>
        public int ReadIdx
        {
            get { return this._readidx; }
        }

        /// <summary>
        /// ArrayValue的结束索引
        /// 从0开始
        /// </summary>
        public int WriteIdx
        {
            get { return this._writeidx; }
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            get { return this._capacity; }
        }

        public int Bg_Idx_Init
        {
            get { return this._bg_idx_init; }
        }

        public int Ed_Idx_Init
        {
            get { return this._ed_idx_init; }
        }

        public AsyncEffectList(int nCapacity)
        {
            if (nCapacity < 1)
            {
                throw new ArgumentException("Initial Capacity must bigger than zero");
            }

            this._capacity = nCapacity;
            this._ed_idx_init = nCapacity - 1;
            this.m_list = new LinkedList<TValue>();
            ArrayValue = new TValue[nCapacity];

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
        public bool Add(TValue value)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                if (this.m_list.Count >= this._capacity)
                {                    
                    return false;
                }

                this.m_list.AddLast(value);
                ArrayValue[this._writeidx++] = value;              
                if (this._writeidx > this._capacity - 1)
                    this._writeidx = 0;

                return true;
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
                this._readidx = 0;
                this._writeidx = 0;
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
        /// <param name="liGet"></param>
        /// <returns>实际长度</returns>
        public int DequeueToList(int nMaxNum, TValue[] aryGet)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                int nAllNum = this.m_list.Count;
                if (nMaxNum < 1)
                {
                    nMaxNum = nAllNum;
                }
                else
                {
                    nMaxNum = nAllNum > nMaxNum ? nMaxNum : nAllNum;
                }

                TValue node = default(TValue);
                int nBeginIdx = 0;
                while (nBeginIdx < nMaxNum)
                {
                    node = this.m_list.First.Value;
                    aryGet[nBeginIdx++] = node;                    
                    this.m_list.RemoveFirst();

                    this._readidx++;
                    if (this._readidx > this._capacity - 1)
                        this._readidx = 0;
                }

                return nMaxNum;
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }
    }
}
