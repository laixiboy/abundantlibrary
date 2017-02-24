/**
 * @brief:日志写入线程
 * @author:wolan email:khyusj@163.com
 * @attention:none
 **/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WLLibrary.DataStructure;

namespace WLLibrary.Log
{
    /// <summary>
    /// 日志类型
    /// 级别必须由小到大
    /// </summary>
    public enum LOGTYPE
    {
        /// <summary>
        /// 运行信息
        /// </summary>
        INFO = 0x0001,
        /// <summary>
        /// 调试信息
        /// </summary>
        DEBUG = 0x0002,
        /// <summary>
        /// 异常
        /// </summary>
        ERROR = 0x0004,
    }

    public class LogEngine
    {
        #region Static Private

        private const int LOG_EACH_MAXLEN = 500;

        private static LogEngine _instance = null;

        /// <summary>
        /// 承载多个线程的线程安全日志池（不需单独加锁）
        /// 共享资源，禁止对此变量进行过长的独占操作
        /// 所有线程共用
        /// add.wl.20131114
        /// </summary>
        private static AsyncQueue<string> _s_logPool = null;

        /// <summary>
        /// log4net
        /// </summary>
        private static log4net.ILog _s_log = null;

        /// <summary>
        /// 日志线程对象
        /// </summary>
        private static Thread _s_Thread = null;

        /// <summary>
        /// @brief:线程运行标记
        /// </summary>
        private static volatile bool _run = true;

        /// <summary>
        /// 输出的日志类型
        /// 支持LOGTYPE.INFO|LOGTYPE.DEBUG形式
        /// </summary>
        private static int _logType = 0;

        #endregion

        public LogEngine()
        {
            
        }

        /// <summary>
        /// @brief 初始化日志系统
        /// </summary>
        /// <param name="log"></param>
        /// <param name="logTypes">输出的日志类型 支持同时输出多个日志类型</param>
        public static void InitLogSystem(log4net.ILog log,params LOGTYPE[] logTypes)
        {
            if (log == null || logTypes == null)
            {
                throw new NullReferenceException("log、logTypes Must Not Null");
            }
            LogEngine._run = true;
            LogEngine._s_log = log;

            foreach (LOGTYPE logType in logTypes)
            {
                LogEngine._logType |= (int)logType;
            }

            if(LogEngine._s_logPool==null)
                LogEngine._s_logPool = new AsyncQueue<string>();
            
            if(LogEngine._s_Thread==null)
                LogEngine._s_Thread = ThreadHandle.StartBackgroundThread(LogEngine.Instance.MainLoop, null);
        }

        public static void ExitLogSystem()
        {
            LogEngine._run = false;
            Thread.Sleep(1000);
        }

        /// <summary>
        /// @brief:获取CLogThread对象
        /// </summary>
        /// <returns></returns>
        public static LogEngine Instance
        {
            get
            {
                if (LogEngine._instance == null)
                    LogEngine._instance = new LogEngine();

                return LogEngine._instance;
            }
        }

        /// <summary>
        /// @func:将日志根据类型记录入日志池
        /// @author:wolan
        /// @brief:内部根据enmLogType进行日志内容区分
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logContent"></param>
        public static void Write(LOGTYPE logType, params string[] content)
        {
            try
            {
                if ((LogEngine._logType & (int)logType) > 0)
                {
                    StringBuilder logInfo = new StringBuilder(LOG_EACH_MAXLEN + 50);

                    logInfo.Append(DateTime.Now.ToString("dd HH:mm:ss "));
                    switch (logType)
                    {
                        case LOGTYPE.ERROR:
                            logInfo.Append("● ");
                            break;
                        case LOGTYPE.DEBUG:
                            logInfo.Append("○ ");
                            break;
                    }

                    foreach (string each in content)
                    {
                        logInfo.Append(each);
                    }

                    LogEngine._s_logPool.Enqueue(logInfo.ToString());
                    logInfo.Remove(0, logInfo.Length);
                }
            }
            catch { }
        }

        public static void Write(LOGTYPE logType, StringBuilder content)
        {
            Write(logType, content.ToString());
            content.Remove(0, content.Length);
        }

        #region 日志处理线程

        /// <summary>
        /// 线程化日志处理的主函数
        /// add.wl.20131114
        /// </summary>
        public void MainLoop()
        {
            string[] listLog = new string[100];
            int nWriteNum = 0;
            int nWriteNumMax = 5;
            int nWriteGroupNum = 0;
            StringBuilder logInfo = new StringBuilder(1000);
            int nLogNum = 0;
            int nLogIdx = 0;

            while (LogEngine._run)
            {
                try
                {
                    nWriteNum = 0;
                    nWriteGroupNum = 0;
                    nLogIdx = 0;
                    nLogNum = LogEngine._s_logPool.DequeueToArray(listLog);
                    while (nLogIdx < nLogNum)
                    {
                        if (nWriteNum >= nWriteNumMax)
                        {
                            LogEngine._s_log.Error(logInfo.ToString());
                            nWriteGroupNum++;
                            nWriteNum = 0;
                            logInfo.Remove(0, logInfo.Length);                                
                        }
                        else
                        {
                            logInfo.Append(listLog[nLogIdx]);
                            nWriteNum++;
                            nLogIdx = nWriteGroupNum * nWriteNumMax + nWriteNum;

                            //增加\r\n的情形
                            //只有一条不能加
                            //达到写入阀值（规定阀值或者不足阀值的现有数据总量）不能加
                            if (nLogNum>1 
                                && (nWriteNum < nWriteNumMax && nWriteNum<nLogNum)
                                && nLogIdx<nLogNum)
                            {
                                logInfo.Append("\r\n");
                            }
                        }                            
                    }

                    if (logInfo.Length > 0)
                    {
                        LogEngine._s_log.Error(logInfo.ToString());
                        logInfo.Remove(0, logInfo.Length);                
                    }
                }
                catch { }
                finally
                {
                    Thread.Sleep(500);
                }
            }

            LogEngine._s_log.Error(DateTime.Now.ToString("dd HH:mm:ss ")+"LogEngine Exit Success");
        }
        #endregion
    }
}
