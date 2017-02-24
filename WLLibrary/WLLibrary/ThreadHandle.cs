using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WLLibrary
{
    public class ThreadHandle
    {
        /// <summary>
        /// @brief:启动后台线程
        /// @author:wolan
        /// </summary>
        /// <param name="start"></param>
        /// <param name="threadPools"></param>
        /// <returns></returns>
        public static Thread StartBackgroundThread(ThreadStart start, List<Thread> threadPools)
        {
            Thread thread = new Thread(start);
            thread.IsBackground = true;
            thread.Start();

            if (threadPools != null)
            {
                threadPools.Add(thread);
            }

            return thread;
        }

        /// <summary>
        /// @brief:启动带参数的后台线程
        /// @author:wolan
        /// </summary>
        /// <param name="start"></param>
        /// <param name="threadPools"></param>
        /// <returns></returns>
        public static Thread StartBackgroundParamerterizedThread(ParameterizedThreadStart start, object parameter, List<Thread> threadPools)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(start));
            thread.IsBackground = true;
            thread.Start(parameter);

            if (threadPools != null)
            {
                threadPools.Add(thread);
            }

            return thread;
        }

        /// <summary>
        /// @brief:关闭后台线程
        /// @author:wolan
        /// </summary>
        /// <param name="threadPools"></param>
        public static void StopBackgroundThread(List<Thread> threadPools)
        {
            if (threadPools != null && threadPools.Count > 0)
            {
                for (int i = threadPools.Count - 1; i >= 0; i--)
                {
                    if (threadPools[i] != null && threadPools[i].ThreadState == ThreadState.Running)
                        threadPools[i].Abort();

                    threadPools.RemoveAt(i);
                }
            }
        }
    }
}
