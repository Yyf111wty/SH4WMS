using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public class MasterInverntoryModel
    {
        public MasterInverntoryModel()
        {


        }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string ITEMCODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ITEMNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ITEMTYPE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //public string ITEMPEC { get; set; }

       

        /// <summary>
        /// 
        /// </summary>
        public int MINSTORAGE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int MAXSTORAGE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NOWSTORAGE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int STATE { get; set; }

        
    }
}
