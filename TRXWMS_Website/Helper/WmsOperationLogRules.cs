using System;
using System.Collections.Generic;

namespace TRXWMS_Website.Helper
{
    public enum WmsOperationSnapshotMode
    {
        None,
        RequestOnly,
        BeforeOnly,
        BeforeAndAfter
    }

    public sealed class WmsOperationLogRule
    {
        public string OperationType { get; set; }
        public WmsOperationSnapshotMode SnapshotMode { get; set; }
        public string DefaultButtonName { get; set; }
        public string SnapshotKey { get; set; }
    }

    public static class WmsOperationLogRules
    {
        private static readonly HashSet<string> ExcludedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Index", "AddOrEdit", "Canvas", "Detail",
            "GetRowData", "GetHeadData", "GetBodyData", "Getlist", "GetGGlist", "GetLane",
            "GetRowMatrix", "GetCellDetail", "GetExportlistold", "GetDetail",
            "GetUsertData", "GetSearchSelectListByZone", "GetSearchSelectListByWarehouse", "GetSearchSelectListByCodeName"
        };

        private static readonly Dictionary<string, WmsOperationLogRule> Rules =
            new Dictionary<string, WmsOperationLogRule>(StringComparer.OrdinalIgnoreCase);

        static WmsOperationLogRules()
        {
            RegisterCrud("Warehouse");
            RegisterCrud("Zone");
            RegisterCrud("Master");
            RegisterCrud("Location");
            Register("Location", "addalllocation", "新增", "新增", WmsOperationSnapshotMode.RequestOnly, null);
            Register("Location", "Resetlocation", "复位", "业务", WmsOperationSnapshotMode.BeforeAndAfter, "wms_base_location");
            Register("Location", "Clearlocation", "清空", "业务", WmsOperationSnapshotMode.BeforeAndAfter, "wms_base_location");
            Register("Location", "Clearmatter", "清空托盘物料", "业务", WmsOperationSnapshotMode.BeforeAndAfter, "wms_base_location");
            Register("Location", "DJlocation", "冻结", "业务", WmsOperationSnapshotMode.BeforeAndAfter, "wms_base_location");
            Register("Location", "JFlocation", "解封", "业务", WmsOperationSnapshotMode.BeforeAndAfter, "wms_base_location");
            Register("Location", "GetExportlist", "导出", "导出", WmsOperationSnapshotMode.RequestOnly, null);

            Register("PutIn", "updateHead", "修改", "修改", WmsOperationSnapshotMode.BeforeAndAfter, "wms_bill_putinbindingheader");
            Register("PutIn", "addalllocation", "批量生成", "新增", WmsOperationSnapshotMode.RequestOnly, null);
            Register("PutIn", "DeleteHead", "删除", "删除", WmsOperationSnapshotMode.BeforeOnly, "wms_bill_putinbindingheader");

            Register("PutOut", "DeleteHead", "删除", "删除", WmsOperationSnapshotMode.BeforeOnly, "wms_bill_putoutheader_with_body");

            Register("Inventory", "GetExportlist", "导出", "导出", WmsOperationSnapshotMode.RequestOnly, null);
            Register("Inventory", "GetExportlistold", "导出", "导出", WmsOperationSnapshotMode.RequestOnly, null);

            Register("StockerTask", "DeleteProduct", "删除", "删除", WmsOperationSnapshotMode.BeforeOnly, "wms_stocker_task");

            Register("ReportWork", "Export", "导出", "导出", WmsOperationSnapshotMode.RequestOnly, null);

            Register("LocationMap", "RegisterMenu", "注册菜单", "业务", WmsOperationSnapshotMode.RequestOnly, null);
        }

        private static void RegisterCrud(string controller)
        {
            Register(controller, "AddProduct", "新增", "新增", WmsOperationSnapshotMode.RequestOnly, null);
            Register(controller, "EditProduct", "编辑", "修改", WmsOperationSnapshotMode.BeforeAndAfter, controllerEntity(controller));
            Register(controller, "DeleteProduct", "删除", "删除", WmsOperationSnapshotMode.BeforeOnly, controllerEntity(controller));
        }

        private static string controllerEntity(string controller)
        {
            switch (controller)
            {
                case "Warehouse": return "wms_base_warehouse";
                case "Zone": return "wms_base_zone";
                case "Master": return "wms_base_master";
                case "Location": return "wms_base_location";
                default: return null;
            }
        }

        private static void Register(string controller, string action, string buttonName, string operationType,
            WmsOperationSnapshotMode mode, string snapshotKey)
        {
            Rules[BuildKey(controller, action)] = new WmsOperationLogRule
            {
                DefaultButtonName = buttonName,
                OperationType = operationType,
                SnapshotMode = mode,
                SnapshotKey = snapshotKey
            };
        }

        public static bool ShouldLog(string area, string controller, string action, out WmsOperationLogRule rule)
        {
            rule = null;
            if (!string.Equals(area, "WMS", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.Equals(controller, "Log", StringComparison.OrdinalIgnoreCase)
                || string.Equals(controller, "OperationLog", StringComparison.OrdinalIgnoreCase)
                || string.Equals(controller, "SystemLog", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ExcludedActions.Contains(action))
            {
                return false;
            }

            return Rules.TryGetValue(BuildKey(controller, action), out rule);
        }

        public static string BuildKey(string controller, string action)
        {
            return controller + "|" + action;
        }
    }
}
