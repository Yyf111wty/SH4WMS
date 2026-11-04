using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Helper
{
    public static class WmsOperationSnapshotLoader
    {
        private const int MaxJsonLength = 65536;

        public static string LoadBefore(string snapshotKey, string id)
        {
            if (string.IsNullOrWhiteSpace(snapshotKey) || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            try
            {
                object data = null;
                switch (snapshotKey)
                {
                    case "wms_base_warehouse":
                        data = new wms_base_warehouseService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_base_zone":
                        data = new wms_base_zoneService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_base_master":
                        data = new wms_base_masterService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_base_location":
                        data = new wms_base_locationService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_stocker_task":
                        data = new wms_stocker_taskService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_bill_putinbindingheader":
                        data = new wms_bill_putinbindingheaderService().Query(t => t.ID == id).FirstOrDefault();
                        break;
                    case "wms_bill_putoutheader_with_body":
                        data = LoadPutOutDeleteSnapshot(id);
                        break;
                }

                return ToJson(data);
            }
            catch
            {
                return null;
            }
        }

        private static object LoadPutOutDeleteSnapshot(string id)
        {
            var headerService = new wms_bill_putoutheaderService();
            var bodyService = new wms_bill_putoutbodyService();
            var header = headerService.Query(t => t.ID == id).FirstOrDefault();
            if (header == null)
            {
                return null;
            }

            var bodies = bodyService.Query(t => t.HEADERID == id).ToList();
            return new { header, bodies };
        }

        public static string ToJson(object data)
        {
            if (data == null)
            {
                return null;
            }

            var json = JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, DateFormatString = "yyyy-MM-dd HH:mm:ss" });
            return Truncate(json);
        }

        public static string BuildRequestJson(IDictionary<string, object> actionParameters)
        {
            if (actionParameters == null || actionParameters.Count == 0)
            {
                return null;
            }

            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in actionParameters)
            {
                if (pair.Key.StartsWith("__", StringComparison.Ordinal))
                {
                    continue;
                }

                dict[pair.Key] = pair.Value;
            }

            return Truncate(JsonConvert.SerializeObject(dict, Formatting.None,
                new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" }));
        }

        public static string ExtractId(IDictionary<string, object> actionParameters)
        {
            if (actionParameters == null)
            {
                return null;
            }

            object value;
            if (actionParameters.TryGetValue("ID", out value) && value != null)
            {
                return Convert.ToString(value);
            }

            if (actionParameters.TryGetValue("id", out value) && value != null)
            {
                return Convert.ToString(value);
            }

            return null;
        }

        private static string Truncate(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length <= MaxJsonLength)
            {
                return json;
            }

            return json.Substring(0, MaxJsonLength) + "...[truncated]";
        }
    }
}
