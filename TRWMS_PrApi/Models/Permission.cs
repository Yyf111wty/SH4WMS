using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class Permission
    {
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int IsEnabled { get; set; }
        public string ImageUrl { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
        public string LinkUrl { get; set; }
        public string ControllerName{ get; set; }
        public string ActionName { get; set; }
        public string KeyName { get; set; }
        public int _parentId { get; set; }
        public string ParentName { get; set; }
        public int SortNo { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateOperator{ get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
        public string State { get; set; }
        public string NodeType { get; set; }
    }
}