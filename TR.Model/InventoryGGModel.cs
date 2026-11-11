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
        /// <summary>
        /// 库存针数（无单针记录的历史盒按 50 支折算）
        /// </summary>
        public long NEEDLENUM { get; set; }
    }
}
