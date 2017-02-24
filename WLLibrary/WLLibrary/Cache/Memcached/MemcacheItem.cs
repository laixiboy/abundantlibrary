using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using WLLibrary.Cache;
using System.Configuration;
using System.IO;
using WLLibrary.Log;

namespace WLLibrary.Cache.Memcached
{
    public class MemcacheItem
    {
        private static Dictionary<string, MemcachedClient> _clientPool = null;

        /// <summary>
        /// @brief:获取MemcachedClient对象
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns>
        /// null:poolName不存在或者没有有效MemcachedClient对象
        /// 非null:正常
        /// </returns>
        protected static MemcachedClient GetInstance(string poolName)
        {
            if(_clientPool==null)
                return null;

            if (_clientPool.ContainsKey(poolName))
                return _clientPool[poolName];

            return null;
        }

        /// <summary>
        /// @brief:永不过期=30天
        /// </summary>
        private const int UnExpired = 30 * 24 * 60 * 60;

        /// <summary>
        /// 读取键值key下的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static object Get(string poolName, string key, bool valueIsStr=false,bool compress = false)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = compress;
                mc.PoolName = poolName;
                return mc.Get(key,valueIsStr);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Get ex:", ex.ToString());
                return null;
            }
        }

        public static byte[] Get_Bytes(string poolName, string key, bool compress = false)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = compress;
                mc.PoolName = poolName;
                return mc.Get_Bytes(key);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Get_Bytes ex:", ex.ToString());
                return null;
            }
        }

        public static Dictionary<string, string> Gets(string poolName, ref Dictionary<string, long> cass, params string[] keys)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = false;
                mc.PoolName = poolName;
                return mc.Gets(keys,ref cass);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Gets ex:", ex.ToString());
                return null;
            }
        }

        public static Dictionary<string,byte[]> Gets_Bytes(string poolName, ref Dictionary<string,long> cass, params string[] keys)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = false;
                mc.PoolName = poolName;
                return mc.Gets_Bytes(keys,ref cass);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Gets_Bytes ex:", ex.ToString());
                return null;
            }
        }

        public static Dictionary<string, byte[]> Gets_Bytes(string poolName, ref Dictionary<string, long> cass, List<string> keys)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = false;
                mc.PoolName = poolName;
                return mc.Gets_Bytes(keys.ToArray(), ref cass);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Gets_Bytes ex:", ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// @brief：以键值Key保存value数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="liveSecs">默认0：永不过期</param>
        /// <returns></returns>
        public static bool Set(string poolName, string key, object value, int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if(liveSecs<0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;

                return mc.Set(key,value,expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Set ex:", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// @brief:以键值Key保存类型为string的value数据
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="liveSecs"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static bool Set(string poolName, string key, string value, int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if (liveSecs < 0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;

                return mc.Set(key, value, expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Set ex:", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="liveSecs">0:永不过期 >0:有效时间</param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static bool Set_Bytes(string poolName, string key, byte[] value, int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if (liveSecs < 0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;
               
                return mc.Set_Bytes(key, value, expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Set_Bytes ex:", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="liveSecs">0:永不过期 >0:有效时间</param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static bool Cas_Bytes(string poolName, string key, byte[] value,long cas,int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if (liveSecs < 0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;

                return mc.Cas_Bytes(key, value, cas,expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Cas_Bytes ex:", ex.ToString());
                return false;
            }
        }

        public static bool Append(string poolName, string key, string value, int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if (liveSecs < 0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;

                return mc.Append(key, value, expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Append ex:", ex.ToString());
                return false;
            }
        }

        public static bool Prepend(string poolName, string key, string value, int liveSecs = UnExpired, bool compress = false)
        {
            try
            {
                if (liveSecs < 0)
                    return false;

                if (liveSecs == 0)
                    liveSecs = UnExpired;

                DateTime expired = DateTime.Now.AddSeconds(liveSecs);
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                mc.EnableCompression = compress;

                return mc.Prepend(key, value, expired);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Prepend ex:", ex.ToString());
                return false;
            }
        }

        public static bool Delete(string poolName, string key)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                return mc.Delete(key);
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem Delete ex:", ex.ToString());
                return false;
            }
        }

        public static bool DeleteAferExpire(string poolName, string key,int expireSecs)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.PoolName = poolName;
                return mc.Delete(key,DateTime.Now.AddSeconds(expireSecs));
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "MemcacheItem DeleteAferExpire ex:", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverList">ip:port的集合，与poolNames是一对一关系</param>
        /// <param name="poolNames">poolname的集合，与serverList是一对一关系</param>
        /// <param name="connInit">初始连接数(=最小连接数)</param>
        /// <param name="connMax">最大连接数</param>
        /// <returns></returns>
        public static bool InitMemcachePool(ArrayList serverList,ArrayList poolNames,
            int connInit,int connMax)
        {
            if (serverList == null || poolNames == null)
            {
                throw new ArgumentNullException("Argument Must Not Null");
            }

            if (connInit < 1 || connInit > connMax)
            {
                throw new ArgumentNullException("connX Parameters Invalid");
            }

            if (serverList.Count != poolNames.Count)
            {
                throw new ArgumentException("serverList.Count Must Equals poolNames.Count");
            }

            #region 初始化池

            MemcacheItem._clientPool = new Dictionary<string, MemcachedClient>(serverList.Count);
            for (int i = 0; i < serverList.Count; i++)
            {
                SockIOPool pool = SockIOPool.GetInstance(poolNames[i].ToString());
                if (pool == null)
                    return false;

                ArrayList servers = new ArrayList();
                servers.Add(serverList[i].ToString());
                pool.SetServers(servers);
                pool.InitConnections = connInit;
                pool.MinConnections = connInit;
                pool.MaxConnections = connMax;

                pool.SocketConnectTimeout = 500;//设置连接的套接字超时
                pool.SocketTimeout = 500;//设置套接字超时读取

                pool.MaintenanceSleep = 60*1000;//设置维护线程运行的睡眠时间。如果设置为0，那么维护线程将不会启动
                pool.Failover = true;

                pool.Nagle = false;
                if (!pool.Initialize())
                    return false;

                MemcachedClient mc = new MemcachedClient();
                mc.PoolName = poolNames[i].ToString();
                mc.EnableCompression = false;//是否启用压缩数据
                MemcacheItem._clientPool[poolNames[i].ToString()] = mc;
            }
            
            #endregion
            
            return true;
        }

        /// <summary>
        /// @brief:终止serverList中的Memcached的客户端连接池
        /// </summary>
        /// <param name="serverList"></param>
        /// <param name="poolNames"></param>
        /// <returns></returns>
        public static bool StopMemcachePool(ArrayList serverList, ArrayList poolNames)
        {
            if (serverList == null || poolNames == null)
            {
                throw new ArgumentNullException("Argument Must Not Null");
            }

            if (serverList.Count != poolNames.Count)
            {
                throw new ArgumentException("serverList.Count Must Equals poolNames.Count");
            }

            for (int i = 0; i < serverList.Count; i++)
            {
                SockIOPool pool = SockIOPool.GetInstance(poolNames[i].ToString());
                if (pool != null)
                {
                    pool.Shutdown();
                }                
            }

            return true;
        }

        /// <summary>
        /// 重生所有缓存
        /// </summary>
        public static void ReSetMemcache()
        {
            MemcachedClient mc = new MemcachedClient();
            mc.FlushAll();
        }      
    }
}
