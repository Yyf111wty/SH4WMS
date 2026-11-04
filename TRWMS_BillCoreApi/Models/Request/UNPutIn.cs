using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Request
{
	public class UNPutIn
	{
		/// <summary>
		/// 
		/// </summary>
		public string SOURCEORDER { get; set; }

		

		/// <summary>
		/// 
		/// </summary>
		public string CONTAINER { get; set; }

		
		/// <summary>
		/// 
		/// </summary>
		public int STATE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ZONECODE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string LOCATIONCODE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int ENABLED { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string REMARK { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CREATOR { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CREATETIME { get; set; }

		
	}
}