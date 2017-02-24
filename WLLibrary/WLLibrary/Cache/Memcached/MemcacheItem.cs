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
        /// @brief:��ȡMemcachedClient����
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns>
        /// null:poolName�����ڻ���û����ЧMemcachedClient����
        /// ��null:����
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
        /// @brief:��������=30��
        /// </summary>
        private const int UnExpired = 30 * 24 * 60 * 60;

        /// <summary>
        /// ��ȡ��ֵkey�µ�����
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
        /// @brief���Լ�ֵKey����value����
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="liveSecs">Ĭ��0����������</param>
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
        /// @brief:�Լ�ֵKey��������Ϊstring��value����
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
        /// <param name="liveSecs">0:�������� >0:��Чʱ��</param>
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
        /// <param name="liveSecs">0:�������� >0:��Чʱ��</param>
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
        /// <param name="serverList">ip:port�ļ��ϣ���poolNames��һ��һ��ϵ</param>
        /// <param name="poolNames">poolname�ļ��ϣ���serverList��һ��һ��ϵ</param>
        /// <param name="connInit">��ʼ������(=��С������)</param>
        /// <param name="connMax">���������</param>
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

            #region ��ʼ����

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

                pool.SocketConnectTimeout = 500;//�������ӵ��׽��ֳ�ʱ
                pool.SocketTimeout = 500;//�����׽��ֳ�ʱ��ȡ

                pool.MaintenanceSleep = 60*1000;//����ά���߳����е�˯��ʱ�䡣�������Ϊ0����ôά���߳̽���������
                pool.Failover = true;

                pool.Nagle = false;
                if (!pool.Initialize())
                    return false;

                MemcachedClient mc = new MemcachedClient();
                mc.PoolName = poolNames[i].ToString();
                mc.EnableCompression = false;//�Ƿ�����ѹ������
                MemcacheItem._clientPool[poolNames[i].ToString()] = mc;
            }
            
            #endregion
            
            return true;
        }

        /// <summary>
        /// @brief:��ֹserverList�е�Memcached�Ŀͻ������ӳ�
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
        /// �������л���
        /// </summary>
        public static void ReSetMemcache()
        {
            MemcachedClient mc = new MemcachedClient();
            mc.FlushAll();
        }      
    }
}
