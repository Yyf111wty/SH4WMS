using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TRWMS_Website.Helper
{
    public static class HttpRequestPost
    {
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            return client;
        }

        /// <summary>
        /// 访问Http Post
        /// </summary>
        public static string HttpPostMethod(string url, object content, bool ignoreNullValues = false)
        {
            var settings = ignoreNullValues
                ? new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                : null;
            var requestJson = JsonConvert.SerializeObject(content, settings);
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var responseJson = SharedHttpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
            return responseJson;
        }
    }
}
