using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TRXWMS_Website.Helper
{
  

    public static class TokenHelper
    {
        // 密钥（随便改）
        private static readonly string SECRET = "Jichen_Bidding_2025_@#$";

        /// <summary>
        /// 创建Token（只包含用户名，无依赖）
        /// </summary>
        public static string CreateToken(string userName)
        {
            var expire = DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
            var str = $"{userName}|{expire}";
            return Encode(str);
        }

        /// <summary>
        /// 从Token获取用户名
        /// </summary>
        public static string GetUserName(string token)
        {
            try
            {
                var str = Decode(token);
                var arr = str.Split('|');
                if (arr.Length != 2) return null;

                var expire = DateTime.Parse(arr[1]);
                if (expire < DateTime.Now) return null;

                return arr[0];
            }
            catch
            {
                return null;
            }
        }

        #region 加密解密（无依赖）
        private static string Encode(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var md5 = MD5.Create())
            {
                var key = md5.ComputeHash(Encoding.UTF8.GetBytes(SECRET));
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }
            return Convert.ToBase64String(bytes);
        }

        private static string Decode(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);
            using (var md5 = MD5.Create())
            {
                var key = md5.ComputeHash(Encoding.UTF8.GetBytes(SECRET));
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion
    }
}