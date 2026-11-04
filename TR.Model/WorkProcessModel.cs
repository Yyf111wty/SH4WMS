using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class WorkProcessModel
    {
        public WorkProcessModel()
        {


        }
        /// <summary>
        /// Desc:生产工单ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string job_id { get; set; }

        /// <summary>
        /// Desc:生产任务ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string work_id { get; set; }

        /// <summary>
        /// Desc:工序ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string step_id { get; set; }
        /// <summary>
        /// Desc:工序名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string step_name { get; set; }
        /// <summary>
        /// Desc:加工顺序
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string sequnce { get; set; }

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
        /// Desc:工单状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string work_status { get; set; }
        /// <summary>
        /// Desc:工单状态名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string work_statusname { get; set; }

        /// <summary>
        /// Desc:所属班
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string class_id { get; set; }
        /// <summary>
        /// Desc:所属班
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string class_idname { get; set; }

        /// <summary>
        /// Desc:所属组
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_id { get; set; }
        /// <summary>
        /// Desc:所属组
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_idname { get; set; }

        /// <summary>
        /// Desc:设备ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_ids { get; set; }
        /// <summary>
        /// Desc:设备ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_idsname { get; set; }

        /// <summary>
        /// Desc:操作员
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string user_ids { get; set; }
        /// <summary>
        /// Desc:操作员
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string user_idsname { get; set; }

        /// <summary>
        /// Desc:派工时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? plan_time { get; set; }

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