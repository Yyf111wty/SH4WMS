using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TRWMS_BillApi.Models.Request;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.EXTENSIONS;

namespace TRXWMS.BILL.MES
{
    public static class MesDrillBitService
    {
        public static MesIssueDrillBitRequest BuildRequest(string pallet, string materialCode, string spec, IEnumerable<string> containers, DateTime createTime)
        {
            var boxContent = BuildBoxContent(spec, ApiConfig.MesDrillBitContentCount);
            return new MesIssueDrillBitRequest
            {
                FactoryInfo = ApiConfig.MesFactoryInfo,
                PalletQRCode = pallet,
                MaterialCode = materialCode,
                CreateTime = createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                BoxInfoList = containers
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(container => new MesDrillBitBoxInfo
                    {
                        QRCode = container.Trim(),
                        Content = boxContent
                    })
                    .ToList()
            };
        }

        public static string BuildBoxContent(string spec, int count)
        {
            if (count <= 0)
            {
                return spec ?? string.Empty;
            }

            var values = Enumerable.Repeat(spec ?? string.Empty, count);
            return string.Join(", ", values);
        }

        public static async Task<MesDrillBitCallResult> IssueDrillBitFromWarehouseAsync(MesIssueDrillBitRequest request)
        {
            var result = new MesDrillBitCallResult
            {
                RequestUrl = ApiConfig.IssueDrillBitFromWarehouseUrl,
                RequestJson = JsonConvert.SerializeObject(request)
            };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(ApiConfig.MesHttpTimeoutSeconds);
                    using (var content = new StringContent(result.RequestJson, Encoding.UTF8, "application/json"))
                    {
                        var response = await client.PostAsync(result.RequestUrl, content).ConfigureAwait(false);
                        result.HttpStatusCode = (int)response.StatusCode;
                        result.ResponseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(result.ResponseJson))
                        {
                            result.Response = new MesIssueDrillBitResponse
                            {
                                IsSuccess = false,
                                Message = "MES返回空响应"
                            };
                            result.Success = false;
                            result.ErrorMessage = "MES返回空响应";
                            return result;
                        }

                        result.Response = JsonConvert.DeserializeObject<MesIssueDrillBitResponse>(result.ResponseJson)
                            ?? new MesIssueDrillBitResponse
                            {
                                IsSuccess = false,
                                Message = "MES响应解析失败"
                            };
                        result.Success = result.Response.IsSuccess;
                        if (!result.Success)
                        {
                            result.ErrorMessage = result.Response.ErrorMsg ?? result.Response.Message ?? "MES上报失败";
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
    }
}
