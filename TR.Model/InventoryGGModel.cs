using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public class InventoryGGModel
    {
        public InventoryGGModel() { }
        /// <summary>
        /// 
        /// </summary>
        public string ZONECODE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SOURCEORDER { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NUM { get; set; }
        /// <summary>
        /// 当前占用盒数（OUTENABLED=0）
        /// </summary>
        public string OCCUPYNUM { get; set; }
    }
}
