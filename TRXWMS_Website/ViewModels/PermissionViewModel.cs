using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.ViewModels
{
    public class PermissionViewModel
    {
        public string total{get;set;}
        public List<Rows> rows { get; set; }

    }
    public class Rows
    {
        public int RoleId { get; set; }
        public int NodeId { get; set; }
        public int Id{get;set;}
        public string NodeName { get; set; }
        public string KeyName { get; set; }
        public int IsEnabled { get; set; }
        public string Type { get; set; }
        public string Linkurl { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public int SortNo { get; set; }
        public string icon { get; set; }
        public string Imageurl { get; set; }
        public int _parentId { get; set; }
        public string  ParentName { get; set; }
        public string state { get; set; }
        public string NodeType { get; set; }
    }
   
}