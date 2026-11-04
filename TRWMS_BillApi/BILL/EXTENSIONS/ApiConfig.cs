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

        private static int GetAppSettingInt(string key, int defaultValue)
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[key];
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
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
        public static string MesUrl = GetAppSetting("MesApiUrl", "http://172.20.217.246:808/");
        /// <summary>
        /// 小钻装盘完成上报MES
        /// </summary>
        public static string IssueDrillBitFromWarehouseUrl = GetAppSetting("MesIssueDrillBitUrl", "http://172.20.217.246:808/api/drillBit/issueDrillBitFromWarehouse");
        /// <summary>
        /// MES厂房信息
        /// </summary>
        public static string MesFactoryInfo = GetAppSetting("MesFactoryInfo", "厂房4");
        /// <summary>
        /// 小盒Content规格重复次数
        /// </summary>
        public static int MesDrillBitContentCount = GetAppSettingInt("MesDrillBitContentCount", 50);
        /// <summary>
        /// 调用MES超时时间(秒)
        /// </summary>
        public static int MesHttpTimeoutSeconds = GetAppSettingInt("MesHttpTimeoutSeconds", 30);
        /// <summary>
        /// 条码是否返修
        /// </summary>
        public static string method07 = MesUrl + "Devices/method07";
        /// <summary>
        /// 返修反馈
        /// </summary>
        public static string method08 = MesUrl + "Devices/method08";

        #endregion

        #region WMS调用备刀机接口

        private static string NormalizeBaseUrl(string url)
        {
            url = (url ?? string.Empty).Trim();
            if (url.Length == 0)
            {
                return string.Empty;
            }

            return url.TrimEnd('/');
        }

        /// <summary>
        /// 备刀机 WebApi 基址（与 Website Web.config 的 bdjWarehouseApiUrl 一致）。
        /// </summary>
        public static string PrepareMachineBaseUrl = NormalizeBaseUrl(GetAppSetting("bdjWarehouseApiUrl", GetAppSetting("PrepareMachineBaseUrl", "http://localhost:8081")));

        /// <summary>
        /// 二维码批量更新：POST /api/updateQRCodeList
        /// </summary>
        public static string UpdateQRCodeListUrl = string.IsNullOrEmpty(PrepareMachineBaseUrl)
            ? string.Empty
            : PrepareMachineBaseUrl + "/api/updateQRCodeList";

        /// <summary>
        /// 按单个二维码查询绑定：GET /api/search?input=
        /// </summary>
        public static string SearchQRCodeUrl = string.IsNullOrEmpty(PrepareMachineBaseUrl)
            ? string.Empty
            : PrepareMachineBaseUrl + "/api/search";

        #endregion
    }
}

