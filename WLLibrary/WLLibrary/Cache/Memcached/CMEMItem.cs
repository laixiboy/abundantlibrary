using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using WLLibrary.Cache;
using System.Configuration;
using System.IO;

/**
 * @brief:
 * 
 */
namespace WLLibrary.Cache.Memcached
{
    public class CMEMItem:MemcacheItem
    {
        public static Dictionary<string, byte[]> Gets_Ext_Bytes(ref int error,ref string errorDetail,
            string poolName,ref Dictionary<string,long> cas,params string[] keys)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = false;
                mc.PoolName = poolName;
                return mc.Gets_Ext_Bytes(keys, null,ref cas,ref error,ref errorDetail);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Dictionary<string, string> Gets_Ext(ref int error, ref string errorDetail,
            string poolName, ref Dictionary<string, long> cas, params string[] keys)
        {
            try
            {
                MemcachedClient mc = MemcacheItem.GetInstance(poolName);
                mc.EnableCompression = false;
                mc.PoolName = poolName;
                return mc.Gets_Ext(keys, null, ref cas,ref error, ref errorDetail);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
