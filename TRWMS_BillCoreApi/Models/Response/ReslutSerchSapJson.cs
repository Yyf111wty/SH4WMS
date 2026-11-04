using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Response
{
	public class ReslutSerchSapJson
	{
		/// <summary>
		/// 操作结果
		/// </summary>
		public string SUCCESS { get; set; }
		/// <summary>
		/// 操作消息
		/// </summary>
		public string MESSAGE { get; set; }
		/// <summary>
		/// 返回列表
		/// </summary>
		public List<SModel> ROWS { get; set; }

	}
	public class SModel
	{
		public string WAREHOUSE_ID { get; set; }
		public string LOCATION_CODE { get; set; }
		public string STATUS { get; set; }
		public string LENUM { get; set; }
		public string CHARG { get; set; }
		public string MATNR { get; set; }
	}
}