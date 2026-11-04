using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("堆垛机报警日志")]
    public class WmsBaseDeviceErrorLog : EntityBase<string>
    {
        [Description("设备编号")]
        public string DeviceCode { get; set; }


        [Description("设备类型")]
        public string DeviceType { get; set; }

        [Description("设备属性")]
        public string DeviceProperty { get; set; }

        [Description("设备状态")]
        public string DeviceAtion { get; set; }

        [Description("设备报警码")]
        public string ErrorCode { get; set; }

        [Description("库区编码")]
        public string ZoonCode { get; set; }

        [Description("设备ip")]
        public string Ip { get; set; }


    }
}