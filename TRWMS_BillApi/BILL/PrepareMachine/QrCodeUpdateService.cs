using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using TRWMS_BillApi.Models.Request;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.EXTENSIONS;

namespace TRXWMS.BILL.PrepareMachine
{
    /// <summary>
    /// 换料成功后向备刀机下发二维码绑定（POST /api/updateQRCodeList）。
    /// </summary>
    public static class QrCodeUpdateService
    {
        public static string ResolveSwapContent(string abnormalContainer, string fallbackContent, Func<string, string> wmsContentLookup = null)
        {
            abnormalContainer = (abnormalContainer ?? string.Empty).Trim();
            fallbackContent = (fallbackContent ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(abnormalContainer))
            {
                string fromMachine = TrySearchContentFromPrepareMachine(abnormalContainer);
                if (!string.IsNullOrWhiteSpace(fromMachine))
                {
                    return fromMachine;
                }

                if (wmsContentLookup != null)
                {
                    string fromWms = (wmsContentLookup(abnormalContainer) ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(fromWms))
                    {
                        return fromWms;
                    }
                }
            }

            return fallbackContent;
        }

        public static QrCodeUpdateCallResult UpdateQRCodeList(string[] qrCodes, string[] contents)
        {
            var result = new QrCodeUpdateCallResult();
            qrCodes = (qrCodes ?? new string[0]).Select(x => (x ?? string.Empty).Trim()).Where(x => x.Length > 0).ToArray();
            contents = (contents ?? new string[0]).Select(x => x ?? string.Empty).ToArray();

            if (qrCodes.Length == 0)
            {
                result.Success = false;
                result.ErrorMessage = "QRCodes 不能为空";
                return result;
            }

            if (contents.Length != qrCodes.Length)
            {
                result.Success = false;
                result.ErrorMessage = "QRCodes 与 Contents 数量必须一致";
                return result;
            }

            string requestUrl = ApiConfig.UpdateQRCodeListUrl;
            result.RequestUrl = requestUrl;
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                result.Success = false;
                result.ErrorMessage = "未配置备刀机地址（bdjWarehouseApiUrl）";
                return result;
            }

            var request = new QrCodeInfoListRequest
            {
                QRCodes = qrCodes,
                Contents = contents
            };
            result.RequestJson = JsonConvert.SerializeObject(request);
            result.ContentUsed = contents.Length > 0 ? contents[0] : string.Empty;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(ApiConfig.MesHttpTimeoutSeconds);
                    using (var httpContent = new StringContent(result.RequestJson, Encoding.UTF8, "application/json"))
                    {
                        var response = client.PostAsync(requestUrl, httpContent).ConfigureAwait(false).GetAwaiter().GetResult();
                        result.HttpStatusCode = (int)response.StatusCode;
                        result.ResponseJson = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                        if (string.IsNullOrWhiteSpace(result.ResponseJson))
                        {
                            result.Success = false;
                            result.ErrorMessage = "备刀机返回空响应";
                            return result;
                        }

                        result.Response = JsonConvert.DeserializeObject<QrCodeUpdateResponse>(result.ResponseJson)
                            ?? new QrCodeUpdateResponse
                            {
                                Success = false,
                                Message = "备刀机响应解析失败"
                            };
                        result.Success = result.Response.Success;
                        if (!result.Success)
                        {
                            result.ErrorMessage = result.Response.Message ?? "备刀机二维码更新失败";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        private static string TrySearchContentFromPrepareMachine(string qrCode)
        {
            qrCode = (qrCode ?? string.Empty).Trim();
            if (qrCode.Length == 0)
            {
                return string.Empty;
            }
            //验证绑定是否成功
            string searchUrl = ApiConfig.SearchQRCodeUrl;
            if (string.IsNullOrWhiteSpace(searchUrl))
            {
                return string.Empty;
            }

            string url = searchUrl + "?input=" + HttpUtility.UrlEncode(qrCode);
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(ApiConfig.MesHttpTimeoutSeconds);
                    string responseJson = client.GetStringAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        return string.Empty;
                    }

                    var token = JToken.Parse(responseJson);
                    string content = token.SelectToken("Result.Content")?.ToString()
                        ?? token.SelectToken("Result.content")?.ToString()
                        ?? token.SelectToken("Content")?.ToString()
                        ?? token.SelectToken("content")?.ToString();
                    return (content ?? string.Empty).Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
