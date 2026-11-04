using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public  class MachineRepairModel
    {
        public MachineRepairModel()
        {

        }
        /// <summary>
        /// Desc:主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int id { get; set; }

        /// <summary>
        /// Desc:设备ID
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? machine_id { get; set; }

        /// <summary>
        /// Desc:设备名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string   machine_name { get; set; }

        /// <summary>
        /// Desc:故障原因编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string error_type { get; set; }

        /// <summary>
        /// Desc:故障原因
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string error_typename { get; set; }

        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string remark { get; set; }

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

    }
}
