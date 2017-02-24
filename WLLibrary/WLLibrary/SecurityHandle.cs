using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace WLLibrary
{
    public class SecurityHandle
    {
        #region DES

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="key">128 bits</param>
        /// <param name="iv">8 bytes</param>
        /// <param name="plainStr">明文字符串</param>
        /// <returns>密文</returns>
        public static string DESEncrypt(string key,string iv,string plainStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);
            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);

            string encrypt = null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();

            return encrypt;
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public static string DESDecrypt(string key,string iv,string encryptStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);
            byte[] byteArray = Convert.FromBase64String(encryptStr);

            string decrypt = null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();

            return decrypt;
        }

        #endregion

        #region AES

        /// <summary>
        /// @brief Base64编码后的文本进行UrlSafe处理的转换对应字符
        /// </summary>
        private static string[][] AES_UrlSafe_Encode = new string[][]{
            new string[]{"+","_"},
            new string[]{"/","-"},
        };

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="key">128/192/256 bits</param>
        /// <param name="iv">向量 16bytes</param>
        /// <param name="plainStr">明文字符串</param>
        /// <param name="urlSafe">
        /// true:使用"_"代替"+"，使用"-"代替"/"
        /// false:使用标准Base64编码
        /// </param>
        /// <returns>密文，异常时返回null</returns>
        public static string AESEncrypt(string key,string iv, string plainStr,bool urlSafe=false)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);
            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);

            string encrypt = null;
            Rijndael aes = Rijndael.Create();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }

                if (urlSafe)
                {
                    for (int i = 0; i < AES_UrlSafe_Encode.GetLength(0); i++)
                    {
                        encrypt = encrypt.Replace(AES_UrlSafe_Encode[i][0], AES_UrlSafe_Encode[i][1]);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            aes.Clear();

            return encrypt;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <param name="urlSafe">
        /// true:表明encryptStr是使用"_"代替"+"，使用"-"代替"/"编码后的内容
        /// false:表明encryptStr是使用标准Base64编码后的内容
        /// </param>
        /// <returns>明文，异常时返回null</returns>
        public static string AESDecrypt(string key, string iv, string encryptStr, bool urlSafe = false)
        {
            if (urlSafe)
            {
                for (int i = 0; i < AES_UrlSafe_Encode.GetLength(0); i++)
                {
                    encryptStr = encryptStr.Replace(AES_UrlSafe_Encode[i][1], AES_UrlSafe_Encode[i][0]);
                }
            }

            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);
            byte[] byteArray = Convert.FromBase64String(encryptStr);

            string decrypt = null;
            Rijndael aes = Rijndael.Create();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch(Exception ex) 
            {

            }
            aes.Clear();

            return decrypt;
        }
        
        #endregion
    }
}
