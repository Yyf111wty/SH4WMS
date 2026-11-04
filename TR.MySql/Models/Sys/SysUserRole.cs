using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("用户角色")]
    public partial class SysUserRole : EntityBase<int>
    {
        public int? UserId {get;set;}         
        public int? RoleId {get;set;}

        public virtual SysUser SysUser { get; set; }
        public virtual SysRole SysRole { get; set; }
    }
}
