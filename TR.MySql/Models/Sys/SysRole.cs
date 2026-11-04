using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("角色")]
    public partial class SysRole : EntityBase<int>
    {
        [Description("角色编码")]
        public int RoleId { get; set; }

        [Description("角色名称")]
        public string RoleName {get;set;}

        [Description("是否启用")]
        public bool? Enabled {get;set;}

        [Description("角色描述")]
        public string Description { get;set;}

        [Description("角色水平")]
        public int? RoleLevel {get;set;}

        public virtual ICollection<SysRolePermission> BodyRole { get; set; }
        public virtual ICollection<SysUserRole> Body { get; set; }
    }
}
