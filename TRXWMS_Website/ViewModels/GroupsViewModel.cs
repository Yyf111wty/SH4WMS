using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.ViewModels
{
    public class GroupsViewModel
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int ParentGroupId { get; set; }
        public string ParentGroupName { get; set; }
        public int IsEnabled { get; set; }
        public string Describes { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateOperator { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
        public string state { get; set; }
        public string GroupLevlel { get; set; }


    }
}