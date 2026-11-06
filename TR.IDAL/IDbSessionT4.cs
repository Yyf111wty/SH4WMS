
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.IDAL
{
    public partial interface IDbSession
    {
        Ijc_bidding_detailDal Getjc_bidding_detailDal();
        Ijc_bidding_historyDal Getjc_bidding_historyDal();
        Ijc_bidding_projectDal Getjc_bidding_projectDal();
        Ijc_customer_levelDal Getjc_customer_levelDal();
        Isys_exceptionDal Getsys_exceptionDal();
        Isys_groupsDal Getsys_groupsDal();
        Isys_logDal Getsys_logDal();
        Isys_permissionsDal Getsys_permissionsDal();
        Isys_rolepermissionDal Getsys_rolepermissionDal();
        Isys_rolesDal Getsys_rolesDal();
        Isys_settingDal Getsys_settingDal();
        Isys_userroleDal Getsys_userroleDal();
        Isys_usersDal Getsys_usersDal();
        Isys_wcsrequestDal Getsys_wcsrequestDal();
        Iwms_agv_taskDal Getwms_agv_taskDal();
        Iwms_agv_taxiDal Getwms_agv_taxiDal();
        Iwms_base_deviceerrorlogDal Getwms_base_deviceerrorlogDal();
        Iwms_base_locationDal Getwms_base_locationDal();
        Iwms_base_masterDal Getwms_base_masterDal();
        Iwms_base_warehouseDal Getwms_base_warehouseDal();
        Iwms_base_zoneDal Getwms_base_zoneDal();
        Iwms_bill_countbodyDal Getwms_bill_countbodyDal();
        Iwms_bill_countdetailDal Getwms_bill_countdetailDal();
        Iwms_bill_countheaderDal Getwms_bill_countheaderDal();
        Iwms_bill_inventorybookDal Getwms_bill_inventorybookDal();
        Iwms_bill_putinDal Getwms_bill_putinDal();
        Iwms_bill_putinbindingbodyDal Getwms_bill_putinbindingbodyDal();
        Iwms_bill_putinbindingheaderDal Getwms_bill_putinbindingheaderDal();
        Iwms_bill_putoutbodyDal Getwms_bill_putoutbodyDal();
        Iwms_bill_putoutheaderDal Getwms_bill_putoutheaderDal();
        Iwms_bill_weighDal Getwms_bill_weighDal();
        Iwms_conveyor_taskDal Getwms_conveyor_taskDal();
        Iwms_jc_tbdetailDal Getwms_jc_tbdetailDal();
        Iwms_logDal Getwms_logDal();
        Iwms_needle_inventoryDal Getwms_needle_inventoryDal();
        Iwms_needle_transactionDal Getwms_needle_transactionDal();
        Iwms_operation_logDal Getwms_operation_logDal();
        Iwms_report_workDal Getwms_report_workDal();
        Iwms_stocker_taskDal Getwms_stocker_taskDal();
        Iwms_transfer_taskDal Getwms_transfer_taskDal();
    }
}

