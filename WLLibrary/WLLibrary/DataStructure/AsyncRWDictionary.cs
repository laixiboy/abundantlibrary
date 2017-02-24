#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

/**
 * @func：带读写s锁的Dictionary 
 * @author:wolan
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Readers-Writers
    /// Reader Lock,Writer Lock
    /// </summary>
    public class AsyncRWDictionary<TKey, TValue>
    {
        /// <summary>
        /// [UnSafe]
        /// </summary>
        internal Dictionary<TKey, TValue> m_Dic = null;
        internal ReaderWriterLockSlim m_RWLock = null;

        public AsyncRWDictionary()
        {
            this.m_Dic = new Dictionary<TKey, TValue>();
            this.m_RWLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// [Safe]存在该键则返回，否则返回default(object:null,int:0)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get_S(TKey key)
        {
            this.m_RWLock.EnterReadLock();
            try
            {
                if (this.m_Dic.ContainsKey(key))
                {
                    return this.m_Dic[key];
                }
                else
                    return default(TValue);
            }
            finally
            {
                this.m_RWLock.ExitReadLock();
            }
        }

        /// <summary>
        /// [Safe]返回Key-Value键值对的数量
        /// </summary>
        /// <returns></returns>
        public int Count_S()
        {
            this.m_RWLock.EnterReadLock();
            try
            {
                return this.m_Dic.Count;
            }
            finally
            {
                this.m_RWLock.ExitReadLock();
            }
        }

        /// <summary>
        /// [Safe]清理
        /// </summary>
        /// <returns></returns>
        public void Clear_S()
        {
            this.m_RWLock.EnterWriteLock();
            try
            {
                this.m_Dic.Clear();
            }
            finally
            {
                this.m_RWLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// [UnSafe]存在该键则返回，否则返回default(object:null,int:0)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            if (this.m_Dic.ContainsKey(key))
            {
                return this.m_Dic[key];
            }
            else
                return default(TValue);
        }

        /// <summary>
        /// [Safe]存在该键则更新，否则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set_S(TKey key, TValue value)
        {
            this.m_RWLock.EnterWriteLock();
            try
            {
                if (this.m_Dic.ContainsKey(key))
                {
                    this.m_Dic[key] = value;
                }
                else
                {
                    this.m_Dic.Add(key, value);
                }
            }
            finally
            {
                this.m_RWLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// [UnSafe]存在该键则更新，否则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value)
        {
            if (this.m_Dic.ContainsKey(key))
            {
                this.m_Dic[key] = value;
            }
            else
            {
                this.m_Dic.Add(key, value);
            }
        }

        /// <summary>
        /// [Safe]移除指定键
        /// </summary>
        /// <param name="key"></param>
        public void Remove_S(TKey key)
        {
            this.m_RWLock.EnterWriteLock();
            try
            {
                if (this.m_Dic.ContainsKey(key))
                {
                    this.m_Dic.Remove(key);
                }
            }
            finally
            {
                this.m_RWLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// [UnSafe]移除指定键
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            if (this.m_Dic.ContainsKey(key))
            {
                this.m_Dic.Remove(key);
            }
        }

        /// <summary>
        /// [Safe]获取读锁
        /// </summary>
        public void AcquireReadLock()
        {
            this.m_RWLock.EnterReadLock();                  
        }

        /// <summary>
        /// [Safe]释放读锁
        /// </summary>
        public void ReleaseReadLock()
        {
            this.m_RWLock.ExitReadLock();
        }

        /// <summary>
        /// [Safe]获取读锁
        /// </summary>
        public void AcquireWriteLock()
        {
            this.m_RWLock.EnterWriteLock();
        }

        /// <summary>
        /// [Safe]释放读锁
        /// </summary>
        public void ReleaseWriteLock()
        {
            this.m_RWLock.ExitWriteLock();
        }
    }
}
