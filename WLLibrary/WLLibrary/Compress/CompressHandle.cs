using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WLLibrary.Compress.Zlib;

namespace WLLibrary
{
    public class CompressHandle
    {
        /// <summary>
        /// @brief：压缩解压
        /// </summary>
        /// <param name="boolIsCompress">true压缩 false解压</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Compress(bool isCompress, byte[] input)
        {
            return Compress(isCompress, input, 0, input.Length);
        }
        public static byte[] Compress(bool isCompress, byte[] input, int idxBg, int length)
        {
            byte[] ret = null;
            try
            {
                if (isCompress)//压缩
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        ZOutputStream zos = new ZOutputStream(memoryStream, zlibConst.Z_DEFAULT_COMPRESSION);
                        zos.Write(input, idxBg, length);
                        zos.Dispose();

                        ret = memoryStream.ToArray();
                        memoryStream.Flush();
                    }
                }
                else
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        ZOutputStream zos = new ZOutputStream(memoryStream);
                        //解压缩时间，调用Write_NoException，防止攻击
                        if (!zos.Write_NoException(input, idxBg, length))
                        {
                            zos.Dispose();
                            return null;
                        }
                        zos.Dispose();

                        ret = memoryStream.ToArray();
                        memoryStream.Flush();
                    }
                }
            }
            catch (Exception)
            {
                //LogEngine.Write(LOGTYPE.ERROR, ex.ToString());
                ret = null;
            }
            return ret;
        }
    }
}
