
using TR.DAL;
using TR.IDAL;

namespace TR.DalFactory
{
    public partial class DbSession : IDbSession
    {
        public Ijc_bidding_detailDal Getjc_bidding_detailDal()
        {
            return new jc_bidding_detailDal();
        }
        public Ijc_bidding_historyDal Getjc_bidding_historyDal()
        {
            return new jc_bidding_historyDal();
        }
        public Ijc_bidding_projectDal Getjc_bidding_projectDal()
        {
            return new jc_bidding_projectDal();
        }
        public Ijc_customer_levelDal Getjc_customer_levelDal()
        {
            return new jc_customer_levelDal();
        }
        public Isys_exceptionDal Getsys_exceptionDal()
        {
            return new sys_exceptionDal();
        }
        public Isys_groupsDal Getsys_groupsDal()
        {
            return new sys_groupsDal();
        }
        public Isys_logDal Getsys_logDal()
        {
            return new sys_logDal();
        }
        public Isys_permissionsDal Getsys_permissionsDal()
        {
            return new sys_permissionsDal();
        }
        public Isys_rolepermissionDal Getsys_rolepermissionDal()
        {
            return new sys_rolepermissionDal();
        }
        public Isys_rolesDal Getsys_rolesDal()
        {
            return new sys_rolesDal();
        }
        public Isys_settingDal Getsys_settingDal()
        {
            return new sys_settingDal();
        }
        public Isys_userroleDal Getsys_userroleDal()
        {
            return new sys_userroleDal();
        }
        public Isys_usersDal Getsys_usersDal()
        {
            return new sys_usersDal();
        }
        public Isys_wcsrequestDal Getsys_wcsrequestDal()
        {
            return new sys_wcsrequestDal();
        }
        public Iwms_agv_taskDal Getwms_agv_taskDal()
        {
            return new wms_agv_taskDal();
        }
        public Iwms_agv_taxiDal Getwms_agv_taxiDal()
        {
            return new wms_agv_taxiDal();
        }
        public Iwms_base_deviceerrorlogDal Getwms_base_deviceerrorlogDal()
        {
            return new wms_base_deviceerrorlogDal();
        }
        public Iwms_base_locationDal Getwms_base_locationDal()
        {
            return new wms_base_locationDal();
        }
        public Iwms_base_masterDal Getwms_base_masterDal()
        {
            return new wms_base_masterDal();
        }
        public Iwms_base_warehouseDal Getwms_base_warehouseDal()
        {
            return new wms_base_warehouseDal();
        }
        public Iwms_base_zoneDal Getwms_base_zoneDal()
        {
            return new wms_base_zoneDal();
        }
        public Iwms_bill_countbodyDal Getwms_bill_countbodyDal()
        {
            return new wms_bill_countbodyDal();
        }
        public Iwms_bill_countdetailDal Getwms_bill_countdetailDal()
        {
            return new wms_bill_countdetailDal();
        }
        public Iwms_bill_countheaderDal Getwms_bill_countheaderDal()
        {
            return new wms_bill_countheaderDal();
        }
        public Iwms_bill_inventorybookDal Getwms_bill_inventorybookDal()
        {
            return new wms_bill_inventorybookDal();
        }
        public Iwms_bill_putinDal Getwms_bill_putinDal()
        {
            return new wms_bill_putinDal();
        }
        public Iwms_bill_putinbindingbodyDal Getwms_bill_putinbindingbodyDal()
        {
            return new wms_bill_putinbindingbodyDal();
        }
        public Iwms_bill_putinbindingheaderDal Getwms_bill_putinbindingheaderDal()
        {
            return new wms_bill_putinbindingheaderDal();
        }
        public Iwms_bill_putoutbodyDal Getwms_bill_putoutbodyDal()
        {
            return new wms_bill_putoutbodyDal();
        }
        public Iwms_bill_putoutheaderDal Getwms_bill_putoutheaderDal()
        {
            return new wms_bill_putoutheaderDal();
        }
        public Iwms_bill_weighDal Getwms_bill_weighDal()
        {
            return new wms_bill_weighDal();
        }
        public Iwms_conveyor_taskDal Getwms_conveyor_taskDal()
        {
            return new wms_conveyor_taskDal();
        }
        public Iwms_jc_tbdetailDal Getwms_jc_tbdetailDal()
        {
            return new wms_jc_tbdetailDal();
        }
        public Iwms_logDal Getwms_logDal()
        {
            return new wms_logDal();
        }
        public Iwms_operation_logDal Getwms_operation_logDal()
        {
            return new wms_operation_logDal();
        }
        public Iwms_report_workDal Getwms_report_workDal()
        {
            return new wms_report_workDal();
        }
        public Iwms_stocker_taskDal Getwms_stocker_taskDal()
        {
            return new wms_stocker_taskDal();
        }
        public Iwms_transfer_taskDal Getwms_transfer_taskDal()
        {
            return new wms_transfer_taskDal();
        }
    }
}

