using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace TRWMS_BillApi.Models.Request
{
	public static class HttpRequestPost
	{
		/// <summary>
		/// 访问Http Post
		/// </summary>
		/// <param name="url"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string HttpPostMethod(string url, object content)
		{
			string id = Guid.NewGuid().ToString();
			var requestJson = JsonConvert.SerializeObject(content);
			HttpContent httpContent = new StringContent(requestJson);
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			httpContent.Headers.Add("requestId",id);
			httpContent.Headers.Add("sourceSystem", "TRWMS");
			httpContent.Headers.Add("serviceName", "S_TRWMS_SAP_001S");
			var httpClient = new HttpClient();
			var responseJson = httpClient.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
			return responseJson;

		}
	}
}