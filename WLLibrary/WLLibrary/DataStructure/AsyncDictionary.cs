
//#define V1_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

/**
 * @func：带锁的Dictionary 
 * @author:wolan
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Readers-Writers
    /// Reader Lock,Writer Lock
    /// </summary>
    public class AsyncDictionary<TKey, TValue>
    {
#if V1_0

        /// <summary>
        /// [UnSafe]
        /// </summary>
        internal Dictionary<TKey, TValue> m_Dic = null;
        internal object objLock = null;

        public AsyncDictionary()
        {
            this.m_Dic = new Dictionary<TKey, TValue>();
            this.objLock = ((ICollection)this.m_Dic).SyncRoot;
        }

        /// <summary>
        /// [Safe] 获取此字典表在函数调用时的数据副本
        /// 在此函数运行期间是线程安全的
        /// </summary>
        public Dictionary<TKey, TValue> DicCopy
        {
            get 
            {
                System.Threading.Monitor.Enter(objLock);
                try
                {                    
                    return this.m_Dic.ToDictionary(p => p.Key, p => p.Value);
                }
                finally
                {
                    System.Threading.Monitor.Exit(objLock);
                }
            }
        }

        /// <summary>
        /// [Safe]存在该键则返回，否则返回default(object:null,int:0)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            System.Threading.Monitor.Enter(objLock);
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
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]存在该键则返回，否则返回default(object:null,int:0)
        /// 返回数据之后将此键位Clear
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetAndClear(TKey key)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {
                if (this.m_Dic.ContainsKey(key))
                {
                    TValue ret = this.m_Dic[key];
                    this.m_Dic[key] = default(TValue);
                    return ret;
                }
                else
                    return default(TValue);
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]存在该键则更新，否则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value)
        {
            System.Threading.Monitor.Enter(objLock);
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
                System.Threading.Monitor.Exit(objLock);
            }
        }

        /// <summary>
        /// [Safe]移除指定键
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            System.Threading.Monitor.Enter(objLock);
            try
            {                
                if (this.m_Dic.ContainsKey(key))
                {
                    this.m_Dic.Remove(key);
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(objLock);
            }
        }
#else

        private ConcurrentDictionary<TKey, TValue> m_Dic = null;

        public AsyncDictionary()
        {
            this.m_Dic = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// [Safe]存在该键则返回，否则返回default(object:null,int:0)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            TValue value = default(TValue);
            this.m_Dic.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// [Safe]存在该键则更新，否则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value)
        {
            //不支持更新
            this.m_Dic.TryAdd(key, value);
        }

        /// <summary>
        /// [Safe]移除指定键
        /// </summary>
        /// <param name="key"></param>
        public TValue Remove(TKey key)
        {
            TValue value = default(TValue);
            this.m_Dic.TryRemove(key, out value);

            return value;
        }
#endif
    }
}
