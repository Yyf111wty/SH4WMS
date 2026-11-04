using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("权限")]
    public partial class SysPermission : EntityBase<int>
    {
        [Description("权限编码")]
        public int PermissionId { get; set; }

        [Description("父母模块ID")]
        public int? ParentId { get; set; }

        [Description("权限名称")]
        public string PermissionName { get;set;}

        [Description("是否启用")]
        public bool? Enabled {get;set;}

        [Description("图标")]
        public string Icon {get;set;}

        [Description("类型")]
        public int Type {get;set;}

        [Description("链接")]
        public string LinkUrl {get;set;}

        [Description("分区")]
        public string Area { get; set; }

        [Description("控制器")]
        public string Controller { get; set; }

        [Description("动作")]
        public string Action { get; set; }

        [Description("排序")]
        public int? SortNo {get;set;}

        [Description("是否是菜单")]
        public bool IsLast { get; set; }

        [Description("描述")]
        public string Description { get; set; }

        public virtual ICollection<SysRolePermission> BodyPermission { get; set; }
    }
}
