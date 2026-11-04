using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class StepProcessModel
    {
        public StepProcessModel()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int id { get; set; }

        /// <summary>
        /// Desc:工艺ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string process_id { get; set; }
        public string steptype { get; set; }
        public string parent_id { get; set; }
        public string step_sop_url { get; set; }
        public string qty { get; set; }
        /// <summary>
        /// Desc:工序ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string step_id { get; set; }
        /// <summary>
        /// Desc:料号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string itemcode { get; set; }

        /// <summary>
        /// Desc:工序名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string step_name { get; set; }

        /// <summary>
        /// Desc:工序料号名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string stepprocess_name { get; set; }

        /// <summary>
        /// Desc:加工顺序
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string sequnce { get; set; }

        /// <summary>
        /// Desc:是否连续工艺
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? is_continued { get; set; }

        /// <summary>
        /// Desc:是否连续工艺
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string is_continuedname { get; set; }
       

        /// <summary>
        /// Desc:是否有效
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string enabled { get; set; }

       

        /// <summary>
        /// Desc:创建人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string create_id { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? create_time { get; set; }

        /// <summary>
        /// Desc:修改人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string update_id { get; set; }

        /// <summary>
        /// Desc:修改时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? update_time { get; set; }

    }
}
