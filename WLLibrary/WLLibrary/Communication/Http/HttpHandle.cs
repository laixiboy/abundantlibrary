using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WLLibrary.Log;
using System.IO;

namespace WLLibrary.Communication.Http
{
    public class HttpHandle
    {
        /// <summary>
        /// @brief 将指定的二进制数据放在request中Post到指定的uri中
        /// @return 
        ///     非null:已二进制数组方式返回response body中的数据
        ///     null:response body没有返回数据
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="requestData"></param>
        /// <param name="keepAlive">
        /// true:Connection:Keep-Alive 
        /// false:Connection:Close
        /// </param>
        /// <returns></returns>
        public static byte[] PostHttpRequest(string uri,byte[] requestData,bool keepAlive = false)
        {
            if (string.IsNullOrEmpty(uri)||requestData==null||requestData.Length<1)
                throw new Exception("uri\requestData must not be null");

            System.Net.HttpWebRequest request = null;
            byte[] ret = null;
            try
            {
                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                request.Method = "POST";
                request.KeepAlive = keepAlive;
                request.ContentType = "application/octet-stream";
                request.ServicePoint.Expect100Continue = false;
                request.UserAgent = "WLLibrary";
                request.ContentLength = requestData.Length;
                System.IO.Stream streamRequest = request.GetRequestStream();
                streamRequest.Write(requestData, 0, requestData.Length);
                streamRequest.Close();

                //回复
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                BinaryReader binaryReader = new BinaryReader(responseStream);
                if(response.ContentLength>0)
                {
                    ret = new byte[response.ContentLength];
                    responseStream.Read(ret, 0, ret.Length);
                }
                responseStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogEngine.Write(LOGTYPE.ERROR, "PostHttpRequest:", ex.ToString());
            }
            finally
            {
                if (request != null)
                    request.Abort();
            }

            return ret;
        }
    }
}
