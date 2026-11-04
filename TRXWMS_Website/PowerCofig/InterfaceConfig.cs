using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.Lib
{
    public class InterfaceConfig
    {
        public static string Url = System.Configuration.ConfigurationManager.AppSettings["apiUrl"].ToString();
        public static string ApiActionUrl = System.Configuration.ConfigurationManager.AppSettings["apiActionUrl"].ToString();

        #region 权限接口
        /// <summary>
        /// 登录接口
        /// </summary>
        public static string LoginApi_url = Url + "/Login/TRWCS_LoginApi";
        /// <summary>
        /// 登录获取用户权限
        /// </summary>
        public static string LoginApi_GetUserPermission_url = Url + "/Login/TRWCS_GetUserPermissionDataApi";
        /// <summary>
        /// 获取组别信息接口
        /// </summary>
        public static string GetGroupsApi_url = Url + "/Groups/TRWCS_GetGroupsDataApi";
        ///<summary>
        ///新增组别接口
        ///</summary>
        public static string AddGroupsApi_url = Url + "/Groups/TRWCS_AddGroupsApi";
        ///<summary>
        ///编辑组别接口
        ///</summary>
        public static string EditGroupsApi_url = Url + "/Groups/TRWCS_EditGroupsApi";
        ///<summary>
        ///编辑组别接口
        ///</summary>
        public static string DeleteGroupsApi_url = Url + "/Groups/TRWCS_DeleteGroupsApi";
        ///<summary>
        ///查询用户信息接口
        ///</summary>
        public static string GetUsersApi_url = Url + "/User/TRWCS_GetUsersDataApi";
        ///<summary>
        ///新增用户信息接口
        ///</summary>
        public static string AddUsersApi_url = Url + "/User/TRWCS_AddUsersApi";
        ///<summary>
        ///编辑用户信息接口
        ///</summary>
        public static string EditUsersApi_url = Url + "/User/TRWCS_EditUsersApi";
        ///<summary>
        ///删除用户信息接口
        ///</summary>
        public static string DeleteUsersApi_url = Url + "/User/TRWCS_DeleteUsersApi";
        ///<summary>
        ///设置用户角色接口
        ///</summary>
        public static string SetUsersRoleApi_url = Url + "/User/TRWCS_SetUserRoleApi";
        ///<summary>
        ///重置用户密码接口
        ///</summary>
        public static string ResetPasswordApi_url = Url + "/User/TRWCS_ResetPassword";
        ///<summary>
        ///查询用户角色接口
        ///</summary>
        public static string GetUserRolesApi_url = Url + "/User/TRWCS_GetUserRoleApi";
        ///<summary>
        ///查询角色接口
        ///</summary>
        public static string GetRolesApi_url = Url + "/Role/TRWCS_GetRolesDataApi";
        ///<summary>
        ///新增角色接口
        ///</summary>
        public static string AddRolesApi_url = Url + "/Role/TRWCS_AddRolesApi";
        ///<summary>
        ///编辑角色接口
        ///</summary>
        public static string EditRolesApi_url = Url + "/Role/TRWCS_EditRolesApi";
        ///<summary>
        ///删除角色接口
        ///</summary>
        public static string DeleteRolesApi_url = Url + "/Role/TRWCS_DeleteRolesApi";
        ///<summary>
        ///角色查询用户接口
        ///</summary>
        public static string GetRoleUsersApi_url = Url + "/Role/TRWCS_GetRoleUsersApi";
        ///<summary>
        ///查询权限列表接口
        ///</summary>
        public static string GetPermissionsApi_url = Url + "/Permission/TRWCS_GetPermissionsDataApi";
        ///<summary>
        ///新增模块接口
        ///</summary>
        public static string AddModularApi_url = Url + "/Permission/TRWCS_AddModularApi";
        ///<summary>
        ///新增页面接口
        ///</summary>
        public static string AddPageApi_url = Url + "/Permission/TRWCS_AddPageApi";
        ///<summary>
        ///新增按钮接口
        ///</summary>
        public static string AddBtnApi_url = Url + "/Permission/TRWCS_AddBtnApi";
        ///<summary>
        ///编辑模块接口
        ///</summary>
        public static string EditModularApi_url = Url + "/Permission/TRWCS_EditModularApi";
        ///<summary>
        ///编辑页面接口
        ///</summary>
        public static string EditPageApi_url = Url + "/Permission/TRWCS_EditPageApi";
        ///<summary>
        ///编辑按钮接口
        ///</summary>
        public static string EditBtnApi_url = Url + "/Permission/TRWCS_EditBtnApi";
        ///<summary>
        ///删除模块、页面、按钮接口
        ///</summary>
        public static string DeletePermissionApi_url = Url + "/Permission/TRWCS_DeleteByIdApi";

        public static string Positonup_url = Url + "/Permission/TRWCS_PositionUp";

        public static string PositonDown_url = Url + "/Permission/TRWCS_PositionDown";
        ///<summary>
        ///保存角色分配权限接口
        ///</summary>
        public static string SetRolePermissionsApi_url = Url + "/RolePermission/TRWCS_SetRolePermissionApi";
        ///<summary>
        ///查询角色分配权限接口
        ///</summary>
        public static string GetRolePermissionsApi_url = Url + "/RolePermission/TRWCS_GetRolePermissionApi";
        ///<summary>
        ///查询操作日志接口
        ///</summary>
        public static string GetActionLogApi_url = Url + "/Log/TRWCS_GetActionLogApi";
        ///<summary>
        ///查询异常日志接口
        ///</summary>
        public static string GetExceptionLogApi_url = Url + "/Log/TRWCS_GetExceptionLogApi";
        #endregion

        #region MES接口  
        /// <summary>
        /// 获取作业班信息接口
        /// </summary>
        public static string GetClasssApi_url = Url + "/Groups/TRWCS_GetClasssApi";
        /// <summary>
        /// 获取作业组信息接口
        /// </summary>
        public static string GetGroupByPidApi_url = Url + "/Groups/TRWCS_GetGroupByPidApi";
        #endregion
        
        #region MES工位操作接口
        public static string GetMessageInfo = ApiActionUrl + "/Trx/Api_Page_11_GetMessageInfo";

        public static string UpdateMessageStatus = ApiActionUrl + "/Trx/Api_Page_12_UpdateMessageStatus";

        public static string CheckSNStateReturnMOInfo = ApiActionUrl + "/Trx/Api_Page_01_CheckSNStateReturnMOInfo";

        public static string LockOrUnlockStep = ApiActionUrl + "/Trx/Api_Page_02_LockOrUnlockStep";

        public static string PassStep = ApiActionUrl + "/Trx/Api_Page_03_PassStep";

        public static string GetSNPassStepMsg = ApiActionUrl + "/Trx/Api_Page_04_GetSNPassStepMsg";

        public static string UploadQualityTestingInfo = ApiActionUrl + "/Trx/Api_Page_05_UploadQualityTestingInfo";

        public static string GetStepErrorinfo = ApiActionUrl + "/Trx/Api_Page_08_GetStepErrorinfo";

        public static string GetSNQCStep = ApiActionUrl + "/Trx/Api_Page_07_GetSNQCStep";

        public static string GetSNPassStep = ApiActionUrl + "/Trx/Api_Page_09_GetSNPassStep";

        public static string UploadRepairInfo = ApiActionUrl + "/Trx/Api_Page_06_UploadRepairInfo";

        public static string GetSNPrint = ApiActionUrl + "/Trx/Api_Page_13_GetSNPrint";

        public static string SNPrintPassStep = ApiActionUrl + "/Trx/Api_Page_14_SNPrintPassStep";

        public static string EquipmentEnablingStart = ApiActionUrl + "/Trx/Api_Page_15_EquipmentEnablingStart";

        public static string GetEquipmentEnablingValue = ApiActionUrl + "/Trx/Api_Page_16_GetEquipmentEnablingValue";

        public static string GetEquipmentEnablingValueDetial = ApiActionUrl + "/Trx/Api_Page_17_GetEquipmentEnablingValueDetial";

        public static string EquipmentEnablingRedo = ApiActionUrl + "/Trx/Api_Page_18_EquipmentEnablingRedo";

        public static string CheckSecretKey = ApiActionUrl + "/Trx/Api_Page_10_CheckSecretKey";

        public static string EquipmentEnablingErrorConform = ApiActionUrl + "/Trx/Api_Page_19_EquipmentEnablingErrorConform";

        public static string OffLineRepairGetBaseInfo = ApiActionUrl + "/Trx/Api_Page_20_OffLineRepairGetBaseInfo";

        public static string SpecialStepGoPass = ApiActionUrl + "/Trx/Api_Page_21_SpecialStepGoPass";

        public static string RePrintLabel = ApiActionUrl + "/Trx/Api_Page_22_RePrintLabel";

        public static string GetSNUpDate = ApiActionUrl + "/Trx/Api_MES_01_UploadSN";

        public static string OvertimeWarning = ApiActionUrl + "/Trx/Api_Page_23_OvertimeWarning";

        #endregion
    }
}