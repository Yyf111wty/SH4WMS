using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class PointLacationModel
    {
        public PointLacationModel()
        {


        }
        /// <summary>
        /// Desc:点位号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string location_code { get; set; }

        /// <summary>
        /// Desc:点位类型
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string location_type { get; set; }
        /// <summary>
        /// Desc:点位类型
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string location_typename { get; set; }


        /// <summary>
        /// Desc:所属设备ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_id { get; set; }
        /// <summary>
        /// Desc:所属设备ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_idname { get; set; }

        /// <summary>
        /// Desc:所属设备名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string machine_name { get; set; }

        /// <summary>
        /// Desc:点位任务状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string location_task { get; set; }

        /// <summary>
        /// Desc:点位任务状态
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string location_taskname { get; set; }

        /// <summary>
        /// Desc:是否启用
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string enabled { get; set; }

        /// <summary>
        /// Desc:容器条码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string container_code { get; set; }

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
