using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TR.Common;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Helper
{
    public static class WmsOperationLogWriter
    {
        private static readonly wms_operation_logService LogService = new wms_operation_logService();

        public static void Write(WmsOperationLogEntry entry)
        {
            try
            {
                var log = new wms_operation_log
                {
                    ID = ResultHelper.NewId,
                    PageId = entry.PageId ?? 0,
                    PageName = entry.PageName,
                    ButtonKey = entry.ButtonKey,
                    ButtonName = entry.ButtonName,
                    Module = "WMS",
                    Controller = entry.Controller,
                    Action = entry.Action,
                    OperationType = entry.OperationType,
                    Result = entry.Success ? "成功" : "失败",
                    ErrorMessage = Truncate(entry.ErrorMessage, 500),
                    RequestData = entry.RequestData,
                    DataBefore = entry.DataBefore,
                    DataAfter = entry.DataAfter,
                    OperatorName = entry.OperatorName,
                    ClientIp = entry.ClientIp,
                    OperateTime = entry.OperateTime,
                    DurationMs = (int)entry.DurationMs
                };

                LogService.InsertReturnIdentity(log);
            }
            catch
            {
                // 写日志失败不影响业务
            }
        }

        public static bool TryParseSuccess(object data, out string errorMessage)
        {
            errorMessage = null;
            if (data == null)
            {
                return true;
            }

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict == null)
                {
                    return true;
                }

                object value;
                if (dict.TryGetValue("success", out value))
                {
                    if (value is bool boolValue)
                    {
                        if (!boolValue && dict.TryGetValue("message", out value))
                        {
                            errorMessage = Convert.ToString(value);
                        }

                        return boolValue;
                    }

                    var successText = Convert.ToString(value);
                    if (string.Equals(successText, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dict.TryGetValue("message", out value))
                        {
                            errorMessage = Convert.ToString(value);
                        }

                        return false;
                    }

                    return string.Equals(successText, "true", StringComparison.OrdinalIgnoreCase)
                           || successText == "1";
                }

                if (dict.TryGetValue("result", out value))
                {
                    if (value is bool)
                    {
                        var ok = (bool)value;
                        if (!ok)
                        {
                            errorMessage = "操作失败";
                        }

                        return ok;
                    }

                    var text = Convert.ToString(value);
                    if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = "操作失败";
                        return false;
                    }

                    if (bool.TryParse(text, out var boolResult))
                    {
                        if (!boolResult)
                        {
                            errorMessage = "操作失败";
                        }

                        return boolResult;
                    }

                    if (text == "0" && dict.Count == 1)
                    {
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(text) && !string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = text;
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength);
        }

        public static string ReadHeaderValue(string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }

            if (headerValue.IndexOf('%') < 0)
            {
                return headerValue;
            }

            try
            {
                return Uri.UnescapeDataString(headerValue);
            }
            catch
            {
                return headerValue;
            }
        }
    }

    public sealed class WmsOperationLogEntry
    {
        public int? PageId { get; set; }
        public string PageName { get; set; }
        public string ButtonKey { get; set; }
        public string ButtonName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string OperationType { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string RequestData { get; set; }
        public string DataBefore { get; set; }
        public string DataAfter { get; set; }
        public string OperatorName { get; set; }
        public string ClientIp { get; set; }
        public DateTime OperateTime { get; set; }
        public int? DurationMs { get; set; }
    }
}
