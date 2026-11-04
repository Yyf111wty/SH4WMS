using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.ViewModels
{
    public class GroupsTreeViewModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string level { get; set; }
        public string field { get; set; }
       public bool lazy { get; set; }
    }

}