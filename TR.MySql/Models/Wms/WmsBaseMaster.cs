using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("物料基础信息")]
    public class WmsBaseMaster : EntityBase<string>
    {
        /// <summary>
        ///  
        /// </summary>
        [Description("工厂编号")]
        public string FactoryCode { get; set; }

        [Description("物料编号")]
        public string ItemCode { get; set; }

        [Description("物料名称")]
        public string ItemName { get; set; }

        [Description("物料类型")]
        public string ItemType { get; set; }

        [Description("物料规格")]
        public string ItemPec { get; set; }

        [Description("包装长度")]
        public int? PackageLength { get; set; }

        [Description("包装宽度")]
        public int? PackageWidth { get; set; }

        [Description("包装高度")]
        public int? PackageHeight { get; set; }

        [Description("包装重量")]
        public int? PackageWeight { get; set; }

        [Description("包装重量误差")]
        public int? PackageOffset{ get; set; }

        [Description("存储期限")]
        public string StoraGetime { get; set; }

        [Description("失效日期")]
        public DateTime? UnuSetime { get; set; }

        [Description("最低库存")]
        public int? MinStorage { get; set; }

        [Description("最高库存")]
        public int? MaxStorage { get; set; }

        [Description("是否有效")]
        public int Ifuse { get; set; }

        [Description("是否允许绑多个")]
        public int Ifbindmul { get; set; }

        [Description("批次")]
        public string Batch { get; set; }

        [Description("入库口")]
        public string EntRance { get; set; }

        [Description("出库口")]
        public string OutErmode { get; set; }

        [Description("存储区域")]
        public string StorgeAreas { get; set; }

        [Description("拣选区域")]
        public string PickPosition { get; set; }

        [Description("拉动标识")]
        public int? ThresholdEnable { get; set; }

        [Description("拉动阈值")]
        public string Threshold { get; set; }

        [Description("拣选优先级")]
        public int? PriorityLevel { get; set; }

        [Description("包装数量")]
        public int PackageQty { get; set; }

    }
}