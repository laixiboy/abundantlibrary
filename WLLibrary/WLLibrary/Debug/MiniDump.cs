using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

/**
 * @brief:Dump
 * @author:wolan mail:khyusj@163.com
 * @attention:none
 * */
namespace WLLibrary.Debug
{
    /// <summary>
    /// Visible For Version>=Windows5.1
    /// </summary>
    public static class MiniDump
    {
        [DllImport("DbgHelp.dll")]
        private static extern Boolean MiniDumpWriteDump(
                                    IntPtr hProcess,
                                    uint processId,
                                    IntPtr fileHandle,
                                    MiniDumpType dumpType,
                                    ref MINIDUMP_EXCEPTION_INFORMATION excepInfo,
                                    IntPtr userInfo,
                                    IntPtr extInfo);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct MINIDUMP_EXCEPTION_INFORMATION
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            public Boolean ClientPointers;
        }

        /// <summary>
        /// 输出Dump文件
        /// </summary>
        /// <param name="dmpPath"></param>
        /// <param name="dmpType"></param>
        /// <returns></returns>
        public static Boolean Dump(String filename, MiniDumpType dmpType)
        {
            string assemblyPath = Assembly.GetEntryAssembly().Location;
            string[] aryPath = assemblyPath.Split('\\');
            int nFileIdx = assemblyPath.IndexOf(aryPath[aryPath.Length - 1]);
            string dumpFileName = assemblyPath.Substring(0, nFileIdx) + @"dump\" + filename;
            if (!Directory.Exists(assemblyPath.Substring(0, nFileIdx) + @"dump\"))
            {
                Directory.CreateDirectory(assemblyPath.Substring(0, nFileIdx) + @"dump\");
            }

            Boolean ret = false;
            using (FileStream file = new FileStream(dumpFileName, FileMode.Create))
            {
                MINIDUMP_EXCEPTION_INFORMATION info = new MINIDUMP_EXCEPTION_INFORMATION();
                info.ClientPointers = true;
                info.ExceptionPointers = Marshal.GetExceptionPointers();
                info.ThreadId = GetCurrentThreadId();

                ret = MiniDumpWriteDump(GetCurrentProcess(),
                    GetCurrentProcessId(),
                    file.SafeFileHandle.DangerousGetHandle(),
                    dmpType,
                    ref info,
                    IntPtr.Zero,
                    IntPtr.Zero);

                //file.Close();
            }

            return ret;
        }

        public enum MiniDumpType
        {
            None = 0x00010000,
            Normal = 0x00000000,
            WithDataSegs = 0x00000001,
            WithFullMemory = 0x00000002,
            WithHandleData = 0x00000004,
            FilterMemory = 0x00000008,
            ScanMemory = 0x00000010,
            WithUnloadedModules = 0x00000020,
            WithIndirectlyReferencedMemory = 0x00000040,
            FilterModulePaths = 0x00000080,
            WithProcessThreadData = 0x00000100,
            WithPrivateReadWriteMemory = 0x00000200,
            WithoutOptionalData = 0x00000400,
            WithFullMemoryInfo = 0x00000800,
            WithThreadInfo = 0x00001000,
            WithCodeSegs = 0x00002000
        }
    }

}
