using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class StepModel
    {
        public StepModel()
        {


        }
        /// <summary>
        /// Desc:工序ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int step_id { get; set; }

        /// <summary>
        /// Desc:工序编号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? step_code { get; set; }

        /// <summary>
        /// Desc:工序名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string step_name { get; set; }

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

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string is_enable { get; set; }

        /// <summary>
        /// Desc:作业班
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string class_id { get; set; }

        /// <summary>
        /// Desc:作业组
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_id { get; set; }



        /// <summary>
        /// Desc:默认操作员
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string emp_id { get; set; }
        /// <summary>
        /// Desc:默认设备
        /// Desc:默认设备
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_id { get; set; }
        public string part_id { get; set; }
        public string part_name_specifications { get; set; }
        public string is_keypart { get; set; }
        public string drawing_no { get; set; }
        public string drawing_url { get; set; }
        public string type { get; set; }
        /// <summary>
        /// Desc:默认设备
        /// Desc:默认设备
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string typename { get; set; }
        public string work_classname { get; set; }
        public string work_groupname { get; set; }
        public string user_idsname { get; set; }
        public string machine_idsname { get; set; }
    

    }
}
