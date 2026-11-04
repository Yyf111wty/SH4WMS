using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
	public class BiddingProjectDetailModel
	{
		public BiddingProjectDetailModel()
		{


		}
		

        /// <summary>
        /// 物料编号
        /// </summary>
        public string COMPANYNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PROJECTCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PROJECTNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PROJECTTYPE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CUSTOMERS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int STATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string USERNAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TrueName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int MYPRICE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DESCRIBE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BIDDINGTYPE { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime BIDDTIME { get; set; }
        
    }
}
