using System.Collections.Generic;

namespace TR.Model
{
    public class InventoryCallBoxItem
    {
        public string LOCATIONCODE { get; set; }
        public string CONTAINER { get; set; }
        public string SOURCEORDER { get; set; }
        public string ZONECODE { get; set; }
    }

    public class InventoryCallBoxRequest
    {
        public string target { get; set; }
        public List<InventoryCallBoxItem> items { get; set; }
    }

    public class OutTargetOption
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class CallBoxSuccessDetail
    {
        public string CONTAINER { get; set; }
        public string LOCATIONCODE { get; set; }
        public string SOURCEORDER { get; set; }
        public string TASKID { get; set; }
    }

    public class CallBoxSkipDetail
    {
        public string CONTAINER { get; set; }
        public string LOCATIONCODE { get; set; }
        public string REASON { get; set; }
    }
}
