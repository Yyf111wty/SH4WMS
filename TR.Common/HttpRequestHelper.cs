using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TR.Model;
namespace TR.Common
{
    public class HttpRequestHelper
    {

        /// <summary>
        /// 访问Http Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static HttpResponseModel HttpPostMethod(string url, object content)
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

        public static ReslutJson ReslutJsonMethod(string url, object content)
        {
            ReslutJson hm = new ReslutJson();
            var requestJson = JsonConvert.SerializeObject(content);
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var httpClient = new HttpClient();
            var responseJson = httpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
            hm = JsonConvert.DeserializeObject<ReslutJson>(responseJson);
            return hm;
        }



    }
}