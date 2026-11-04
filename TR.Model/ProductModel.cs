using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Model
{
    public class ProductModel
    {
        public ProductModel()
        {


        }
        /// <summary>
        /// Desc:产品ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int prod_id { get; set; }
        /// <summary>
        /// Desc:作业组
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string group_id { get; set; }
        public string group_idname { get; set; }
        public string p_laser_location { get; set; }

        public string p_laser { get; set; }
        public string p_print_tips { get; set; }
        public string p_print_nums { get; set; }
        public string p_print_templates { get; set; }
        public string p_camera_location { get; set; }
        /// <summary>
        /// Desc:产品名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_name { get; set; }

        /// <summary>
        /// Desc:产品简称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_name_short { get; set; }

        /// <summary>
        /// Desc:客户产品名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_name_cust { get; set; }

        /// <summary>
        /// Desc:客户显示名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string p_name_disp { get; set; }

        /// <summary>
        /// Desc:客户产品编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_code { get; set; }

        /// <summary>
        /// Desc:创建人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string create_id { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:DateTime.Now
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
        /// Desc:是否有效
        /// Default:
        /// Nullable:True
        /// </summary>           
        public bool? isenabled { get; set; }

        /// <summary>
        /// Desc:是否为其它物料
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string prod_type { get; set; }

    }
}