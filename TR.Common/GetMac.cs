using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TR.Common
{
    public class GetMac
    {

        /// <summary>
        /// 通过NetworkInterface获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacByNetworkInterface()
        {
            string strLocalIP = string.Empty;
            try
            {
                
                strLocalIP =  HttpContext.Current.Request.UserHostAddress;

               
                return strLocalIP;
                //NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                //foreach (NetworkInterface ni in interfaces)
                //{
                //    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()) + ",";
                //}
            }
            catch (Exception)
            {
                return "unknown";
            }
           
        }

    }
}
