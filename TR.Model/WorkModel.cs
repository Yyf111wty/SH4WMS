using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class WorkModel
    {
        public WorkModel()
        {


        }
        /// <summary>
        /// Desc:生产任务ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string work_id { get; set; }

        /// <summary>
        /// Desc:生产任务ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string group_id { get; set; }
       
        /// <summary>
        /// Desc:产品ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_id { get; set; }
        /// <summary>
        /// Desc:产品
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_name { get; set; }

        /// <summary>
        /// Desc:计划总数量
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? work_qty_plan { get; set; }

        /// <summary>
        /// Desc:实际入库数量
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? work_qty { get; set; }

        /// <summary>
        /// Desc:开始时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? work_start { get; set; }

        /// <summary>
        /// Desc:结束时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? work_end { get; set; }

        /// <summary>
        /// Desc:工艺版本
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string process_ver { get; set; }

        /// <summary>
        /// Desc:工单状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string work_status { get; set; }
        /// <summary>
        /// Desc:工单状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string work_statusname { get; set; }

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
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string remark { get; set; }
        public string itemfrom { get; set; }
        public string xlsurl { get; set; }

    }
}