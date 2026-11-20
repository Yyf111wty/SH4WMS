using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TRXWMS.Models.Response;

namespace TRXWMS.BILL.EXTENSIONS
{
    public class HttpRequestHelper
    {
        /// <summary>
        /// 访问Http Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public  static HttpResponseModel HttpPostMethod(string url, object content)
        {
            HttpResponseModel hm = new HttpResponseModel();
            var requestJson = JsonConvert.SerializeObject(content);
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (httpContent)
            {
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    var response = httpClient.PostAsync(url, httpContent).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    var responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    hm = JsonConvert.DeserializeObject<HttpResponseModel>(responseJson);
                }
            }
            return hm;
        }      
    }
}