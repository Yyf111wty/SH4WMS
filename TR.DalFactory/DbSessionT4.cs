
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.DAL;
using TR.IDAL;

namespace TR.DalFactory
{
    public partial class DbSession:IDbSession
    {
        
		public Isys_GroupsDal Getsys_GroupsDal()
        {
            return  new sys_GroupsDal();
        }
		public Isys_PermissionsDal Getsys_PermissionsDal()
        {
            return  new sys_PermissionsDal();
        }
		public Isys_RolesDal Getsys_RolesDal()
        {
            return  new sys_RolesDal();
        }
		public Isys_UsersDal Getsys_UsersDal()
        {
            return  new sys_UsersDal();
        }
		public IWMS_BAS_CODESETDal GetWMS_BAS_CODESETDal()
        {
            return  new WMS_BAS_CODESETDal();
        }
		public IWMS_BAS_DEVICEDal GetWMS_BAS_DEVICEDal()
        {
            return  new WMS_BAS_DEVICEDal();
        }
		public IWMS_BAS_LOCATIONDal GetWMS_BAS_LOCATIONDal()
        {
            return  new WMS_BAS_LOCATIONDal();
        }
		public IWMS_BAS_MASTERDal GetWMS_BAS_MASTERDal()
        {
            return  new WMS_BAS_MASTERDal();
        }
		public IWMS_BAS_ReplenishSetDal GetWMS_BAS_ReplenishSetDal()
        {
            return  new WMS_BAS_ReplenishSetDal();
        }
		public IWMS_BILL_BODYDal GetWMS_BILL_BODYDal()
        {
            return  new WMS_BILL_BODYDal();
        }
		public IWMS_BILL_COUNTBODYDal GetWMS_BILL_COUNTBODYDal()
        {
            return  new WMS_BILL_COUNTBODYDal();
        }
		public IWMS_BILL_COUNTHEADDal GetWMS_BILL_COUNTHEADDal()
        {
            return  new WMS_BILL_COUNTHEADDal();
        }
		public IWMS_BILL_DETALIDal GetWMS_BILL_DETALIDal()
        {
            return  new WMS_BILL_DETALIDal();
        }
		public IWMS_BILL_HEADDal GetWMS_BILL_HEADDal()
        {
            return  new WMS_BILL_HEADDal();
        }
		public IWMS_BILL_PUTINBODYDal GetWMS_BILL_PUTINBODYDal()
        {
            return  new WMS_BILL_PUTINBODYDal();
        }
		public IWMS_BILL_PUTINHEADDal GetWMS_BILL_PUTINHEADDal()
        {
            return  new WMS_BILL_PUTINHEADDal();
        }
		public IWMS_BILL_TYPEDal GetWMS_BILL_TYPEDal()
        {
            return  new WMS_BILL_TYPEDal();
        }
		public IWMS_BILL_WEIGHDal GetWMS_BILL_WEIGHDal()
        {
            return  new WMS_BILL_WEIGHDal();
        }
		public IWMS_BOOKING_TASKDal GetWMS_BOOKING_TASKDal()
        {
            return  new WMS_BOOKING_TASKDal();
        }
		public IWMS_CONTAINERDal GetWMS_CONTAINERDal()
        {
            return  new WMS_CONTAINERDal();
        }
		public IWMS_OPERATINGLOGDal GetWMS_OPERATINGLOGDal()
        {
            return  new WMS_OPERATINGLOGDal();
        }
		public IWMS_SUPPLIER_CUSTOMERDal GetWMS_SUPPLIER_CUSTOMERDal()
        {
            return  new WMS_SUPPLIER_CUSTOMERDal();
        }
		public IWMS_TASK_STOCKERDal GetWMS_TASK_STOCKERDal()
        {
            return  new WMS_TASK_STOCKERDal();
        }
		public Isys_RolePermissionDal Getsys_RolePermissionDal()
        {
            return  new sys_RolePermissionDal();
        }
		public Isys_UserRoleDal Getsys_UserRoleDal()
        {
            return  new sys_UserRoleDal();
        }
		public IWMS_BILL_HEIGHDal GetWMS_BILL_HEIGHDal()
        {
            return  new WMS_BILL_HEIGHDal();
        }
	}

}