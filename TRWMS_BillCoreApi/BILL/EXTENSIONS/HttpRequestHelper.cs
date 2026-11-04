using Newtonsoft.Json;
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
            var httpClient = new HttpClient();
            var responseJson = httpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
            hm = JsonConvert.DeserializeObject<HttpResponseModel>(responseJson);
            return hm;
        }      
    }
}