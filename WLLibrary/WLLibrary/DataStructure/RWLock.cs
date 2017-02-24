#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

/**
 * @func：RW锁(单线程写，多线程读)的基类
 * @author:wolan
 * @date:2012/02/17
 **/
namespace WLLibrary.DataStructure
{
    /// <summary>
    /// Multi Readers,Single Writer base class
    /// </summary>
    public class RWLock
    {
        private long write_wish = 0;//写入期望
        private long write = 0;//写入计数
        private long read = 0;//读取计数

        public RWLock()
        {

        }

        public bool AcquireReadLock()
        {
#if PROFILE
            int loopMaxNum = 0;
#endif 
            while (true)
            {
                try
                {
                    if (Interlocked.Read(ref write_wish) != 1)
                    {
                        Interlocked.Increment(ref read);
                        return true;
                    }
#if PROFILE
                    if (++loopMaxNum > 5000)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, "RWLock::AcquireReadLock 请求超限5000");
                        return false;
                    }
#endif
                }
                catch (System.Exception)
                {
                    //LogEngine.Write(LOGTYPE.ERROR, "RWLock::AcquireReadLock:" + ex.ToString());
                    return false;                    
                }
            }
        }

        public bool ReleaseReadLock()
        {
            try
            {
                if (Interlocked.Read(ref read) > 0)
                {
                    Interlocked.Decrement(ref read);

#if PROFILE
                    if (Interlocked.Read(ref read) <1)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, "RWLock::ReleaseReadLock read<1");
                    }
#endif
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, "RWLock::ReleaseReadLock:" + ex.ToString());
                return false;
            }  
        }

        public bool AcquireWriteLock()
        {
#if PROFILE
            int loopMaxNum = 0;
#endif
            while (true)
            {
                try
                {
                    if (Interlocked.Read(ref read) == 0)
                    {
                        Interlocked.Increment(ref write);

#if PROFILE
                        if(Interlocked.Read(ref write)>1)
                            //LogEngine.Write(LOGTYPE.ERROR, "RWLock::AcquireWriteLock write>1");
#endif
                        Interlocked.Exchange(ref write_wish, 0);
                        return true;
                    }
                    else
                        Interlocked.Exchange(ref write_wish, 1);

#if PROFILE
                    if (++loopMaxNum > 5000)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, "RWLock::AcquireWriteLock 请求超限5000");
                        return false;
                    }
#endif
                }
                catch (System.Exception)
                {
                    //LogEngine.Write(LOGTYPE.ERROR, "RWLock::AcquireWriteLock:" + ex.ToString());
                    return false;
                }
            }
        }

        public bool ReleaseWriteLock()
        {
            try
            {
                if (Interlocked.Read(ref write) > 0)
                {
                    Interlocked.Decrement(ref write);

#if PROFILE
                    if (Interlocked.Read(ref write) !=0)
                    {
                        //LogEngine.Write(LOGTYPE.ERROR, "RWLock::ReleaseWriteLock");
                    }
#endif
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, "RWLock::ReleaseWriteLock:" + ex.ToString());
                return false;
            }
        }
    }
}
