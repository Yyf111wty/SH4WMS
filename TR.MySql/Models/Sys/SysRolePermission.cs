using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("角色权限")]
    public partial class SysRolePermission : EntityBase<int>
    {
       
        public int? RoleId {get;set;}
        public int? PermissionId {get;set;}

        public virtual SysRole Role { get; set; }
        public virtual SysPermission Permission { get; set; }

    }
}
