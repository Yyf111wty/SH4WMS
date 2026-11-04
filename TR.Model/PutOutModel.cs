using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
	public class PutOutModel
	{
		public PutOutModel()
		{


		}		
		/// <summary>
		/// 
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string SOURCEORDER { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string ZONECODE { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string ITEMCODE { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string LOT { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string TASKTYPE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CONTAINER { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CONTAINERTYPE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int QUESTFLAG { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? QUESTTIME { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? FINISHTIME { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int STATE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string TARGET { get; set; }

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
		public DateTime? CREATETIME { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string LASTEDITOR { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? LASTEDITTIME { get; set; }

        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
    }
}
