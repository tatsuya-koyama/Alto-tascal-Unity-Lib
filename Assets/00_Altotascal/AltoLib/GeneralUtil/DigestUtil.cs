using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AltoLib
{
    public class DigestUtil
    {
        /// <summary>
        /// 文字列から MD5 ハッシュ（32 文字の 16 進形式文字列）を返す
        /// <example>
        ///   DigestUtil.GetMD5("hoge"); // => "ea703e7aa1efda0064eaa507d9e8ab7e"
        /// </example>
        /// </summary>
        public static string GetMD5(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                var strBuilder = new StringBuilder();
                foreach (var dataByte in data)
                {
                    strBuilder.Append(dataByte.ToString("x2"));
                }
                return strBuilder.ToString();
            }
        }

        /// <summary>
        /// MD5 ハッシュ生成 : Stream を引数にとる版
        /// </summary>
        public static string GetMD5(Stream inputStream)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(inputStream);
                var strBuilder = new StringBuilder();
                foreach (var dataByte in data)
                {
                    strBuilder.Append(dataByte.ToString("x2"));
                }
                return strBuilder.ToString();
            }
        }

        /// <summary>
        /// 文字列から MD5 ハッシュを求め、Base64 形式（22 文字の文字列）で返す
        /// <example>
        ///   DigestUtil.GetMD5AsBase64("hoge"); // => "6nA+eqHv2gBk6qUH2eirfg=="
        /// </example>
        /// </summary>
        public static string GetMD5AsBase64(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                return Convert.ToBase64String(data);
            }
        }
    }
}
