using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public class InventorystatisticsModel
    {
        public InventorystatisticsModel()
        {
        }
      
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
		public string LOCATIONSTATUS { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CONTAINER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SOURCEORDER { get; set; }

       
        /// <summary>
        /// 
        /// </summary>
        public string ROWNUM { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string COLUMNNUM { get; set; }



		/// <summary>
		/// 
		/// </summary>
		public string LAYERNUM { get; set; }
        /// <summary>
		/// 
		/// </summary>
		public string TUNNELNUM { get; set; }

        public DateTime LASTEDITTIME { get; set; }

        /// <summary>
        /// 出库锁定：0=占用，1=可用
        /// </summary>
        public int OUTENABLED { get; set; }

        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }


    }
}
