using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("用户")]
    public partial class SysUser : EntityBase<int>
    {
        public SysUser()
        {
            this.SysUserConfig = new List<SysUserConfig>();
            this.SysRole = new List<SysRole>();
        }
        [Description("用户名")]
        public string UserName {get;set;}

        [Description("密码")]
        public string Password {get;set;}

        [Description("真实姓名")]
        public string TrueName {get;set;}

        [Description("用户组别")]
        public int UserGroupId {get;set;}

        [Description("通讯地址")]
        public string Mobile {get;set;}

        [Description("电子邮箱")]
        public string Email {get;set;}

        [Description("通讯地址")]
        public string Address { get; set; }

        [Description("移动客户端在线标识")]
        public bool CSOnlineFlag { get; set; }

        [Description("移动客户端登陆时间")]
        public DateTime? CSLoginTime { get; set; }

        [Description("有效标识")]
        public bool Enabled { get; set; }
        public virtual ICollection<SysUserRole> Body { get; set; }
        public virtual ICollection<SysUserConfig> SysUserConfig { get; set; }
        public virtual ICollection<SysRole> SysRole { get; set; }
        public virtual SysGroup SysGroup { get; set; }     
    }
}
