
using TR.IDAL;
using TR.IService;
using TR.ORM.Model;

namespace TR.Service
{
    public partial class jc_bidding_detailService : BaseService<jc_bidding_detail>, Ijc_bidding_detailService
    {
        protected override IBaseDal<jc_bidding_detail> GetDal()
        {
            return dbSession.Getjc_bidding_detailDal();
        }
    }
    public partial class jc_bidding_historyService : BaseService<jc_bidding_history>, Ijc_bidding_historyService
    {
        protected override IBaseDal<jc_bidding_history> GetDal()
        {
            return dbSession.Getjc_bidding_historyDal();
        }
    }
    public partial class jc_bidding_projectService : BaseService<jc_bidding_project>, Ijc_bidding_projectService
    {
        protected override IBaseDal<jc_bidding_project> GetDal()
        {
            return dbSession.Getjc_bidding_projectDal();
        }
    }
    public partial class jc_customer_levelService : BaseService<jc_customer_level>, Ijc_customer_levelService
    {
        protected override IBaseDal<jc_customer_level> GetDal()
        {
            return dbSession.Getjc_customer_levelDal();
        }
    }
    public partial class sys_exceptionService : BaseService<sys_exception>, Isys_exceptionService
    {
        protected override IBaseDal<sys_exception> GetDal()
        {
            return dbSession.Getsys_exceptionDal();
        }
    }
    public partial class sys_groupsService : BaseService<sys_groups>, Isys_groupsService
    {
        protected override IBaseDal<sys_groups> GetDal()
        {
            return dbSession.Getsys_groupsDal();
        }
    }
    public partial class sys_logService : BaseService<sys_log>, Isys_logService
    {
        protected override IBaseDal<sys_log> GetDal()
        {
            return dbSession.Getsys_logDal();
        }
    }
    public partial class sys_permissionsService : BaseService<sys_permissions>, Isys_permissionsService
    {
        protected override IBaseDal<sys_permissions> GetDal()
        {
            return dbSession.Getsys_permissionsDal();
        }
    }
    public partial class sys_rolepermissionService : BaseService<sys_rolepermission>, Isys_rolepermissionService
    {
        protected override IBaseDal<sys_rolepermission> GetDal()
        {
            return dbSession.Getsys_rolepermissionDal();
        }
    }
    public partial class sys_rolesService : BaseService<sys_roles>, Isys_rolesService
    {
        protected override IBaseDal<sys_roles> GetDal()
        {
            return dbSession.Getsys_rolesDal();
        }
    }
    public partial class sys_settingService : BaseService<sys_setting>, Isys_settingService
    {
        protected override IBaseDal<sys_setting> GetDal()
        {
            return dbSession.Getsys_settingDal();
        }
    }
    public partial class sys_userroleService : BaseService<sys_userrole>, Isys_userroleService
    {
        protected override IBaseDal<sys_userrole> GetDal()
        {
            return dbSession.Getsys_userroleDal();
        }
    }
    public partial class sys_usersService : BaseService<sys_users>, Isys_usersService
    {
        protected override IBaseDal<sys_users> GetDal()
        {
            return dbSession.Getsys_usersDal();
        }
    }
    public partial class sys_wcsrequestService : BaseService<sys_wcsrequest>, Isys_wcsrequestService
    {
        protected override IBaseDal<sys_wcsrequest> GetDal()
        {
            return dbSession.Getsys_wcsrequestDal();
        }
    }
    public partial class wms_agv_taskService : BaseService<wms_agv_task>, Iwms_agv_taskService
    {
        protected override IBaseDal<wms_agv_task> GetDal()
        {
            return dbSession.Getwms_agv_taskDal();
        }
    }
    public partial class wms_agv_taxiService : BaseService<wms_agv_taxi>, Iwms_agv_taxiService
    {
        protected override IBaseDal<wms_agv_taxi> GetDal()
        {
            return dbSession.Getwms_agv_taxiDal();
        }
    }
    public partial class wms_base_deviceerrorlogService : BaseService<wms_base_deviceerrorlog>, Iwms_base_deviceerrorlogService
    {
        protected override IBaseDal<wms_base_deviceerrorlog> GetDal()
        {
            return dbSession.Getwms_base_deviceerrorlogDal();
        }
    }
    public partial class wms_base_locationService : BaseService<wms_base_location>, Iwms_base_locationService
    {
        protected override IBaseDal<wms_base_location> GetDal()
        {
            return dbSession.Getwms_base_locationDal();
        }
    }
    public partial class wms_base_masterService : BaseService<wms_base_master>, Iwms_base_masterService
    {
        protected override IBaseDal<wms_base_master> GetDal()
        {
            return dbSession.Getwms_base_masterDal();
        }
    }
    public partial class wms_base_warehouseService : BaseService<wms_base_warehouse>, Iwms_base_warehouseService
    {
        protected override IBaseDal<wms_base_warehouse> GetDal()
        {
            return dbSession.Getwms_base_warehouseDal();
        }
    }
    public partial class wms_base_zoneService : BaseService<wms_base_zone>, Iwms_base_zoneService
    {
        protected override IBaseDal<wms_base_zone> GetDal()
        {
            return dbSession.Getwms_base_zoneDal();
        }
    }
    public partial class wms_bill_countbodyService : BaseService<wms_bill_countbody>, Iwms_bill_countbodyService
    {
        protected override IBaseDal<wms_bill_countbody> GetDal()
        {
            return dbSession.Getwms_bill_countbodyDal();
        }
    }
    public partial class wms_bill_countdetailService : BaseService<wms_bill_countdetail>, Iwms_bill_countdetailService
    {
        protected override IBaseDal<wms_bill_countdetail> GetDal()
        {
            return dbSession.Getwms_bill_countdetailDal();
        }
    }
    public partial class wms_bill_countheaderService : BaseService<wms_bill_countheader>, Iwms_bill_countheaderService
    {
        protected override IBaseDal<wms_bill_countheader> GetDal()
        {
            return dbSession.Getwms_bill_countheaderDal();
        }
    }
    public partial class wms_bill_inventorybookService : BaseService<wms_bill_inventorybook>, Iwms_bill_inventorybookService
    {
        protected override IBaseDal<wms_bill_inventorybook> GetDal()
        {
            return dbSession.Getwms_bill_inventorybookDal();
        }
    }
    public partial class wms_bill_putinService : BaseService<wms_bill_putin>, Iwms_bill_putinService
    {
        protected override IBaseDal<wms_bill_putin> GetDal()
        {
            return dbSession.Getwms_bill_putinDal();
        }
    }
    public partial class wms_bill_putinbindingbodyService : BaseService<wms_bill_putinbindingbody>, Iwms_bill_putinbindingbodyService
    {
        protected override IBaseDal<wms_bill_putinbindingbody> GetDal()
        {
            return dbSession.Getwms_bill_putinbindingbodyDal();
        }
    }
    public partial class wms_bill_putinbindingheaderService : BaseService<wms_bill_putinbindingheader>, Iwms_bill_putinbindingheaderService
    {
        protected override IBaseDal<wms_bill_putinbindingheader> GetDal()
        {
            return dbSession.Getwms_bill_putinbindingheaderDal();
        }
    }
    public partial class wms_bill_putoutbodyService : BaseService<wms_bill_putoutbody>, Iwms_bill_putoutbodyService
    {
        protected override IBaseDal<wms_bill_putoutbody> GetDal()
        {
            return dbSession.Getwms_bill_putoutbodyDal();
        }
    }
    public partial class wms_bill_putoutheaderService : BaseService<wms_bill_putoutheader>, Iwms_bill_putoutheaderService
    {
        protected override IBaseDal<wms_bill_putoutheader> GetDal()
        {
            return dbSession.Getwms_bill_putoutheaderDal();
        }
    }
    public partial class wms_bill_weighService : BaseService<wms_bill_weigh>, Iwms_bill_weighService
    {
        protected override IBaseDal<wms_bill_weigh> GetDal()
        {
            return dbSession.Getwms_bill_weighDal();
        }
    }
    public partial class wms_conveyor_taskService : BaseService<wms_conveyor_task>, Iwms_conveyor_taskService
    {
        protected override IBaseDal<wms_conveyor_task> GetDal()
        {
            return dbSession.Getwms_conveyor_taskDal();
        }
    }
    public partial class wms_jc_tbdetailService : BaseService<wms_jc_tbdetail>, Iwms_jc_tbdetailService
    {
        protected override IBaseDal<wms_jc_tbdetail> GetDal()
        {
            return dbSession.Getwms_jc_tbdetailDal();
        }
    }
    public partial class wms_logService : BaseService<wms_log>, Iwms_logService
    {
        protected override IBaseDal<wms_log> GetDal()
        {
            return dbSession.Getwms_logDal();
        }
    }
    public partial class wms_needle_inventoryService : BaseService<wms_needle_inventory>, Iwms_needle_inventoryService
    {
        protected override IBaseDal<wms_needle_inventory> GetDal()
        {
            return dbSession.Getwms_needle_inventoryDal();
        }
    }
    public partial class wms_needle_transactionService : BaseService<wms_needle_transaction>, Iwms_needle_transactionService
    {
        protected override IBaseDal<wms_needle_transaction> GetDal()
        {
            return dbSession.Getwms_needle_transactionDal();
        }
    }
    public partial class wms_operation_logService : BaseService<wms_operation_log>, Iwms_operation_logService
    {
        protected override IBaseDal<wms_operation_log> GetDal()
        {
            return dbSession.Getwms_operation_logDal();
        }
    }
    public partial class wms_report_workService : BaseService<wms_report_work>, Iwms_report_workService
    {
        protected override IBaseDal<wms_report_work> GetDal()
        {
            return dbSession.Getwms_report_workDal();
        }
    }
    public partial class wms_stocker_taskService : BaseService<wms_stocker_task>, Iwms_stocker_taskService
    {
        protected override IBaseDal<wms_stocker_task> GetDal()
        {
            return dbSession.Getwms_stocker_taskDal();
        }
    }
    public partial class wms_transfer_taskService : BaseService<wms_transfer_task>, Iwms_transfer_taskService
    {
        protected override IBaseDal<wms_transfer_task> GetDal()
        {
            return dbSession.Getwms_transfer_taskDal();
        }
    }
}

