using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS.BILL.EXTENSIONS
{
    public class ApiConfig
    {
        private static string GetAppSetting(string key, string defaultValue)
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        #region WMS调用WCS任务接口
        public static string WcsUrl = GetAppSetting("WcsApiUrl", "");
        /// <summary>
        /// 堆垛机出库下发接口
        /// </summary>
        public static string StockTaskUrl = WcsUrl + "/Devices/A01_NewTask";
        /// <summary>
        /// 输送搬运任务下发
        /// </summary>
        public static string ConveryTaskUrl = WcsUrl + "/Devices/B01_NewTransportTask";
        /// <summary>
        /// AGV搬运任务下发
        /// </summary>
        public static string AgvTaskUrl = WcsUrl + "/Devices/C01_NewAgvTask";
        #endregion


        #region WMS调用MES任务接口
        public static string MesUrl = GetAppSetting("MesApiUrl", "");
        /// <summary>
        /// 条码是否返修
        /// </summary>
        public static string method07 = MesUrl + "Devices/method07";
        /// <summary>
        /// 返修反馈
        /// </summary>
        public static string method08 = MesUrl + "Devices/method08";

        #endregion
    }
}
