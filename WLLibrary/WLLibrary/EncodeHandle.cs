using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLLibrary
{
    public sealed class EncodeHandle
    {
        //Encode Change
        /// <summary>
        /// 功能：字符串编码转换
        /// 作者：qt
        /// 创建时间：2011-5-30
        /// </summary>
        /// <param name="srcEncode">旧编码</param>
        /// <param name="dstEncode">新编码</param>
        /// <param name="src">转换的字符串</param>
        /// <returns></returns>
        public static byte[] CodingChange(string srcEncode, string dstEncode, string src)
        {
            byte[] ret = null;
            try
            {
                if (!String.IsNullOrEmpty(srcEncode) && !String.IsNullOrEmpty(dstEncode) && !String.IsNullOrEmpty(src))
                {
                    Encoding srcEncoding = Encoding.GetEncoding(srcEncode);
                    Encoding dstEncoding = Encoding.GetEncoding(dstEncode);
                    ret = Encoding.Convert(srcEncoding, dstEncoding, srcEncoding.GetBytes(src));
                }
            }
            catch
            {
                ret = null;
            }
            return ret;
        }

        #region BASE64

        /// <summary>
        /// @brief Base64编码后的文本进行UrlSafe处理的转换对应字符
        /// </summary>
        private static string[][] Base64_UrlSafe_Encode = new string[][]{
            new string[]{"+","_"},
            new string[]{"/","-"},
        };

        /// <summary>
        /// @bref base64加密
        /// @author wolan(khyusj@163.com)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="urlSafe">
        /// true:使用"_"代替"+"，使用"-"代替"/"
        /// false:使用标准Base64编码
        /// </param>
        /// <returns></returns>
        public static string Base64Encode(string src,bool urlSafe=false)
        {
            byte[] srcBytes = Encoding.UTF8.GetBytes(src);
            string ret = Convert.ToBase64String(srcBytes);

            if (urlSafe)
            {
                for (int i = 0; i < Base64_UrlSafe_Encode.GetLength(0); i++)
                {
                    ret = ret.Replace(Base64_UrlSafe_Encode[i][0], Base64_UrlSafe_Encode[i][1]);
                }
            }

            return ret;
        }

        /// <summary>
        /// @brief base64解密
        /// @author wolan(khyusj@163.com)
        /// </summary>
        /// <param name="srcBase64"></param>
        /// <param name="urlSafe">
        /// true:表明srcBase64是使用"_"代替"+"，使用"-"代替"/"编码后的内容
        /// false:表明srcBase64是使用标准Base64编码后的内容
        /// </param>
        /// <returns></returns>
        public static string Base64Decode(string srcBase64, bool urlSafe = false)
        {
            if (urlSafe)
            {
                for (int i = 0; i < Base64_UrlSafe_Encode.GetLength(0); i++)
                {
                    srcBase64 = srcBase64.Replace(Base64_UrlSafe_Encode[i][1], Base64_UrlSafe_Encode[i][0]);
                }
            }

            byte[] srcBase64Bytes = Convert.FromBase64String(srcBase64);
            return Encoding.UTF8.GetString(srcBase64Bytes);
        }

        /// <summary>
        /// @brief Base64的标准编码
        /// </summary>
        private static readonly HashSet<char> BASE64_STRANDARD = new HashSet<char>(){ 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/', 
            '='};

        /// <summary>
        /// @brief 判断src是否只用了Base64标准中的字符
        /// @author wolan(khyusj@163.com)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="extendChars">在标准Base64上，允许指定扩展字符进行检测</param>
        /// <returns></returns>
        public static bool IsBase64Char(string src,char[] extendChars=null)
        {
            Base64ExtentCharCompare compare = new Base64ExtentCharCompare();

            if (string.IsNullOrEmpty(src))
                return false;
            else if (src.Any(c => !BASE64_STRANDARD.Contains(c)
                                    &&(extendChars==null||(extendChars!=null&&!extendChars.Contains(c,compare)))))
            {
                return false;
            }

            return true;
        }

        public class Base64ExtentCharCompare : IEqualityComparer<char>
        {
            public bool Equals(char data1, char data2)
            {
                return data1.Equals(data2);
            }

            public int GetHashCode(char data)
            {
                return data.GetHashCode();
            }
        }


        #endregion

        #region Url Encode

        /// <summary>
        /// @brief 将+号替换成%2B
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FixPlusFlagBug(string src)
        {
            return src.Replace("+", "%2B");
        }

        /// <summary>
        /// @brief 将*号替换成%2A
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FixStarFlagBug(string src)
        {
            return src.Replace("*", "%2A");
        }

        /// <summary>
        /// @brief 将空格替换成+号
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string TransSpaceFlagToPlusFlag(string src)
        {
            return src.Replace(" ", "+");
        }

        /// <summary>
        /// @brief 解决.net HttpUtility.UrlEncode将=编码为%3d问题，将%3d替换成%3D
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FixDotnetEqualFlagBug(string src)
        {
            return src.Replace("=", "%3D");
        }

        /// <summary>
        /// @brief 内部调用FixPlusFlagBug，FixStarFlagBug，FixDotnetEqualFlagBug
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string FixUrlEncodeBug(string src)
        {
            src = FixPlusFlagBug(src);
            src = FixStarFlagBug(src);
            src = FixDotnetEqualFlagBug(src);
            return src;
        }

        #endregion
    }
}
