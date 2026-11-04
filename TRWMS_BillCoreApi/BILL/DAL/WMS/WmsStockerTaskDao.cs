using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TR.ORM.Model;
using TR.Service;
namespace TRXWMS.BILL.DAL.WMS
{
    public class WmsStockerTaskDao
    {
        public static wms_stocker_taskService WMS_STOCKER_TASKService = new wms_stocker_taskService();

        public static int insert(wms_stocker_task wMS_STOCKER_TASK)
        {
            wMS_STOCKER_TASK.CREATETIME = DateTime.Now;
            wMS_STOCKER_TASK.CREATOR = "WMS";
            wMS_STOCKER_TASK.LASTEDITOR = "WMS";
            wMS_STOCKER_TASK.LASTEDITTIME = DateTime.Now;
            //wMS_STOCKER_TASK.CODE = System.DateTime.Now.ToString("yyyyMMddhh")+;
           // wMS_STOCKER_TASK.CONTAINER = System.DateTime.Now.ToString("yyyyMMddhh");
            return WMS_STOCKER_TASKService.InsertReturnIdentity(wMS_STOCKER_TASK);
        }
    }
}