using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace TR.Common
{
    public class MD5Encrypt
    {
        private static string desKey = "???#v?B7";
        public static void SetKey(string key)
        {
            desKey = key;
        }
        public static string Encrypt(string encryptStr)
        {
            StringBuilder ret = new StringBuilder();
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = Encoding.Default.GetBytes(encryptStr);
                    des.Key = Encoding.ASCII.GetBytes(desKey);
                    des.IV = Encoding.ASCII.GetBytes(desKey);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByteArray, 0, inputByteArray.Length);
                            cs.FlushFinalBlock();

                            foreach (byte b in ms.ToArray())
                            {
                                ret.AppendFormat("{0:X2}", b);
                            }
                            ret.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //log
            }

            return ret.ToString();

        }
        public static string Decrypt(string decryptStr)
        {
            string result = "";
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = new byte[decryptStr.Length / 2];
                    for (int x = 0; x < decryptStr.Length / 2; x++)
                    {
                        int i = (Convert.ToInt32(decryptStr.Substring(x * 2, 2), 16));
                        inputByteArray[x] = (byte)i;
                    }
                    des.Key = Encoding.ASCII.GetBytes(desKey);
                    des.IV = Encoding.ASCII.GetBytes(desKey);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByteArray, 0, inputByteArray.Length);
                            cs.FlushFinalBlock();
                            result = Encoding.Default.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {
                //log
            }
            return result;
        }
    }
}
