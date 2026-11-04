using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class MachineModel
    {
        public MachineModel()
        {


        }
        /// <summary>
        /// Desc:设备ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int id { get; set; }
        /// <summary>
        /// Desc:设备ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int machine_id { get; set; }

        /// <summary>
        /// Desc:设备编号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_code { get; set; }

        /// <summary>
        /// Desc:设备名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_name { get; set; }

        /// <summary>
        /// Desc:设备简称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_name_short { get; set; }

        /// <summary>
        /// Desc:所属组ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_id { get; set; }

        /// <summary>
        /// Desc:所属组ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_idname { get; set; }

        /// <summary>
        /// Desc:设备状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_status { get; set; }

        /// <summary>
        /// Desc:设备状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_statusname { get; set; }

        /// <summary>
        /// Desc:上料方式
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string in_type { get; set; }

        /// <summary>
        /// Desc:上料AGV模板
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string in_agv { get; set; }
        /// <summary>
        /// Desc:上料AGV模板
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string in_agvname { get; set; }

        /// <summary>
        /// Desc:下料方式
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string out_type { get; set; }

        /// <summary>
        /// Desc:下料AGV模板
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string out_agv { get; set; }
        /// <summary>
        /// Desc:下料AGV模板
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string out_agvname { get; set; }
        /// <summary>
        /// Desc:是否允许绑定多个工单
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ifworks { get; set; }
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
