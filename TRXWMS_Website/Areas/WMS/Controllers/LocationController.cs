using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [CheckLoginFilter]
    public class LocationController : Controller
    {
        wms_base_locationService service = new wms_base_locationService();
        wms_base_zoneService zoneservice = new wms_base_zoneService();
        wms_base_warehouseService warehouseservice = new wms_base_warehouseService();
        wms_bill_inventorybookService inventoryservice = new wms_bill_inventorybookService();
        // GET: WMS/TEST
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRowData(wms_base_location WMS_BASE_LOCATION)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_base_location> lists = null;
            try
            {
                //根据用户名显示内容
                string uid = Session["username"].ToString().ToLower();
                if (uid == "ycl")
                {
                    WMS_BASE_LOCATION.ZONECODE = "101-1";

                }
                if (uid == "bcp")
                {
                    WMS_BASE_LOCATION.ZONECODE = "201-1";

                }
                if (uid == "cp")
                {
                    WMS_BASE_LOCATION.ZONECODE = "301-1";

                }
                if (uid == "lck")
                {
                    WMS_BASE_LOCATION.ZONECODE = "401-1";

                }

                lists = service.QueryPage(x =>
                     ((WMS_BASE_LOCATION.ZONECODE == null || x.ZONECODE.Contains(WMS_BASE_LOCATION.ZONECODE)))
                     && (WMS_BASE_LOCATION.TYPE == null || x.TYPE.Contains(WMS_BASE_LOCATION.TYPE))
                     && (WMS_BASE_LOCATION.LOCATIONCODE == null || x.LOCATIONCODE.Contains(WMS_BASE_LOCATION.LOCATIONCODE))
                     && (WMS_BASE_LOCATION.CONTAINER == null || x.CONTAINER.Contains(WMS_BASE_LOCATION.CONTAINER))
                     //&& (WMS_BASE_LOCATION.CONTAINERTYPE == null || x.CONTAINERTYPE.Equals(WMS_BASE_LOCATION.CONTAINERTYPE))
                     && (WMS_BASE_LOCATION.SOURCEORDER == null || x.SOURCEORDER.Contains(WMS_BASE_LOCATION.SOURCEORDER))
                     && (WMS_BASE_LOCATION.ROWNUM == 0 || x.ROWNUM.Equals(WMS_BASE_LOCATION.ROWNUM))
                     && (WMS_BASE_LOCATION.COLUMNNUM == 0 || x.COLUMNNUM.Equals(WMS_BASE_LOCATION.COLUMNNUM))
                     && (WMS_BASE_LOCATION.LAYERNUM == 0 || x.LAYERNUM.Equals(WMS_BASE_LOCATION.LAYERNUM))
                     && (WMS_BASE_LOCATION.TUNNELNUM == 0 || x.TUNNELNUM.Equals(WMS_BASE_LOCATION.TUNNELNUM))
                     && (WMS_BASE_LOCATION.LIMITHEIGHT == 0 || x.LIMITHEIGHT.Equals(WMS_BASE_LOCATION.LIMITHEIGHT))
                     && (WMS_BASE_LOCATION.ENABLED == -10 || x.ENABLED.Equals(WMS_BASE_LOCATION.ENABLED))

                     && (WMS_BASE_LOCATION.ISAVIALABLE == -10 || x.ISAVIALABLE.Equals(WMS_BASE_LOCATION.ISAVIALABLE))
                     ,
                     ref TotalCount, pageId, pageSize, null).ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetExportlist(wms_base_location WMS_BASE_LOCATION)
        {
            int TotalCount = 0;
            //int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            //int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_base_location> lists = null;
            try
            {
                //根据用户名显示内容
                string uid = Session["username"].ToString();
                if (uid == "ycl")
                {
                    WMS_BASE_LOCATION.ZONECODE = "101-1";

                }
                if (uid == "bcp")
                {
                    WMS_BASE_LOCATION.ZONECODE = "201-1";

                }
                if (uid == "cp")
                {
                    WMS_BASE_LOCATION.ZONECODE = "301-1";

                }

                lists = service.QueryPage(x =>
                     ((WMS_BASE_LOCATION.ZONECODE == null || x.ZONECODE.Contains(WMS_BASE_LOCATION.ZONECODE)))
                     && (WMS_BASE_LOCATION.TYPE == null || x.TYPE.Contains(WMS_BASE_LOCATION.TYPE))
                     && (WMS_BASE_LOCATION.LOCATIONCODE == "undefined" || x.LOCATIONCODE.Contains(WMS_BASE_LOCATION.LOCATIONCODE))
                     && (WMS_BASE_LOCATION.CONTAINER == "undefined" || x.CONTAINER.Contains(WMS_BASE_LOCATION.CONTAINER))
                     && (WMS_BASE_LOCATION.SOURCEORDER == "undefined" || x.SOURCEORDER.Contains(WMS_BASE_LOCATION.SOURCEORDER))
                     && (WMS_BASE_LOCATION.ROWNUM == 0 || x.ROWNUM.Equals(WMS_BASE_LOCATION.ROWNUM))
                     && (WMS_BASE_LOCATION.COLUMNNUM == 0 || x.COLUMNNUM.Equals(WMS_BASE_LOCATION.COLUMNNUM))
                     && (WMS_BASE_LOCATION.LAYERNUM == 0 || x.LAYERNUM.Equals(WMS_BASE_LOCATION.LAYERNUM))
                     && (WMS_BASE_LOCATION.TUNNELNUM == 0 || x.TUNNELNUM.Equals(WMS_BASE_LOCATION.TUNNELNUM))
                     && (WMS_BASE_LOCATION.LIMITHEIGHT == 0 || x.LIMITHEIGHT.Equals(WMS_BASE_LOCATION.LIMITHEIGHT))
                     && (WMS_BASE_LOCATION.ENABLED == -10 || x.ENABLED.Equals(WMS_BASE_LOCATION.ENABLED))
                     && (WMS_BASE_LOCATION.ISAVIALABLE == -10 || x.ISAVIALABLE.Equals(WMS_BASE_LOCATION.ISAVIALABLE)), ref TotalCount, 1, 9999999, "LOCATIONCODE");



            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOrEdit()
        {
            return View();
        }


        [HttpPost]
        public JsonResult AddProduct(string ZONECODE, string LOCATIONCODE, string TYPE, int TUNNELNUM, int LIMITHEIGHT, int ENABLED, int ISAVIALABLE, string REMARK, string CONTAINER, string CONTAINERTYPE, string SOURCEORDER)
        {
            wms_base_location model = new wms_base_location();
            try
            {
                //编码不能重复
                var codeid = ZONECODE + "-" + LOCATIONCODE;
                var codecount = service.Query(t => t.ID == codeid).Count;
                if (codecount > 0)
                {
                    return Json(new { result = "库区库位信息重复" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = codeid;

                    model.ZONECODE = ZONECODE;
                    model.LOCATIONCODE = LOCATIONCODE;
                    model.TYPE = TYPE;
					string[] parts = LOCATIONCODE.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    model.ROWNUM = int.Parse(parts[0]);
                    model.COLUMNNUM = int.Parse(parts[1]);
                    model.LAYERNUM = int.Parse(parts[2]);
                    model.TUNNELNUM = TUNNELNUM;
                    model.LIMITHEIGHT = LIMITHEIGHT;

                    //model.PICKORDER = PICKORDER;
                    // model.SYMBOL = SYMBOL;
                    //  model.SORTTYPE = SORTTYPE;
                    // model.EXITOROUT = EXITOROUT;
                    model.CONTAINER = CONTAINER;
                    model.CONTAINERTYPE = CONTAINERTYPE;


                    model.SOURCEORDER = SOURCEORDER;
                    model.ENABLED = ENABLED;
                    model.ISAVIALABLE = ISAVIALABLE;
                    model.REMARK = REMARK;
                    // model.XYZ = XYZ;
                    //  model.CHECKXYZ = CHECKXYZ;
                    // model.SPECS = SPECS;
                    //model.IFMIXING = IFMIXING;
                    model.CREATOR = Session["truename"].ToString();
                    model.CREATETIME = DateTime.Now;
                    model.LASTEDITOR = Session["truename"].ToString();
                    model.LASTEDITTIME = DateTime.Now;
                    var results = (service.InsertReturnIdentity(model) == 1);
                    return Json(new { result = results }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult addalllocation()
        {
            for (int i = 1; i <= 8; i++)  //8排
            {

                //144列  17层 （48列  28层）
                for (int j = 1; j < 145; j++)
                {
                    for (int k = 1; k <= 17; k++)
                    {
                        wms_base_location model = new wms_base_location();
                        string LOCATIONCODE = "";
                        if (j <= 9)
                        {
                            if (k <= 9)
                            {
                                LOCATIONCODE = "0" + i.ToString() + "-" + "0" + j.ToString() + "-" + "0" + k.ToString();
                            }
                            else
                            {
                                LOCATIONCODE = "0" + i.ToString() + "-" + "0" + j.ToString() + "-" + k.ToString();
                            }

                        }
                        else
                        {

                            if (k <= 9)
                            {
                                LOCATIONCODE = "0" + i.ToString() + "-" + j.ToString() + "-" + "0" + k.ToString();
                            }
                            else
                            {
                                LOCATIONCODE = "0" + i.ToString() + "-" + j.ToString() + "-" + k.ToString();
                            }

                        }
                        model.ID = "101" + LOCATIONCODE.Replace("-", "");
                        model.ZONECODE = "101";
                        model.LOCATIONCODE = LOCATIONCODE;
                        model.TYPE = "ST";
                        string[] parts = LOCATIONCODE.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        model.ROWNUM = int.Parse(parts[0]);
                        model.COLUMNNUM = int.Parse(parts[1]);
                        model.LAYERNUM = int.Parse(parts[2]);
                        if (i <= 2)
                        {
                            model.TUNNELNUM = 1;
                        }
                        else if (i > 2 & i <= 4)
                        {
                            model.TUNNELNUM = 2;
                        }
                        else if (i > 4 & i <= 6)
                        {
                            model.TUNNELNUM = 3;
                        }
                        else
                        {
                            model.TUNNELNUM = 4;
                        }

                        // 计算 REGION：每20列为一个区域，区域序号从1开始，区域码示例：1巷道第1区域 = 1001，2巷道第1区域 = 2001
                        int regionIndex = ((j - 1) / 8) + 1;
                        model.LIMITHEIGHT = regionIndex;
                        //model.TUNNELNUM = 1;
                        //model.LIMITHEIGHT = 800; //限高

                        //model.PICKORDER = PICKORDER;
                        // model.SYMBOL = SYMBOL;
                        //  model.SORTTYPE = SORTTYPE;
                        // model.EXITOROUT = EXITOROUT;
                        model.CONTAINER = "";
                        model.SOURCEORDER = "";
                        model.ENABLED = 1;
                        model.ISAVIALABLE = 1;
                        model.REMARK = "";
                        model.CONTAINERTYPE = "";
                        //if (k == 6)
                        //{
                        //	model.CONTAINERTYPE = "0,1";
                        //}
                        //else if (k == 7)
                        //{
                        //	model.CONTAINERTYPE = "2";
                        //}
                        //else
                        //{
                        //	model.CONTAINERTYPE = "";
                        //}

                        // model.XYZ = XYZ;
                        //  model.CHECKXYZ = CHECKXYZ;
                        // model.SPECS = SPECS;
                        //model.IFMIXING = IFMIXING;
                        model.CREATOR = HttpContextHelper.GetCurrentUserName();
                        model.CREATETIME = DateTime.Now;
                        model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                        model.LASTEDITTIME = DateTime.Now;
                        service.InsertReturnIdentity(model);
                    }

                }


            }
            return Json(new { result = 0 }, JsonRequestBehavior.AllowGet);
        }
/*
        [HttpPost]
        public JsonResult addalllocation()
        {
            for (int i = 1; i <= 6; i++)  //6排
            {

                //48列  28层
                for (int j = 1; j < 49; j++)
                {
                    for (int k = 1; k <= 28; k++)
                    {
                        wms_base_location model = new wms_base_location();
                        string LOCATIONCODE = "";
                        if (j <= 9)
                        {
                            if (k <= 9)
                            {
                                LOCATIONCODE = "0" + i.ToString() + "0" + j.ToString() + "0" + k.ToString();
                            }
                            else
                            {
                                LOCATIONCODE = "0" + i.ToString() + "0" + j.ToString() + k.ToString();
                            }

                        }
                        else
                        {

                            if (k <= 9)
                            {
                                LOCATIONCODE = "0" + i.ToString() + j.ToString() + "0" + k.ToString();
                            }
                            else
                            {
                                LOCATIONCODE = "0" + i.ToString() + j.ToString() + k.ToString();
                            }

                        }
                        model.ID = "101" + LOCATIONCODE;
                        model.ZONECODE = "101";
                        model.LOCATIONCODE = LOCATIONCODE;
                        model.TYPE = "ST";
                        model.ROWNUM = int.Parse(LOCATIONCODE.Substring(0, 2));
                        model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                        model.LAYERNUM = int.Parse(LOCATIONCODE.Substring(4, 2));
                        if (i <= 2)
                        {
                            model.TUNNELNUM = 1;
                        }
                        else if (i > 2 & i <= 4)
                        {
                            model.TUNNELNUM = 2;
                        }
                        else if (i > 4 & i <= 6)
                        {
                            model.TUNNELNUM = 3;
                        }
                        else
                        {
                            model.TUNNELNUM = 4;
                        }
                        //model.TUNNELNUM = 1;
                        model.LIMITHEIGHT = 800;

                        //model.PICKORDER = PICKORDER;
                        // model.SYMBOL = SYMBOL;
                        //  model.SORTTYPE = SORTTYPE;
                        // model.EXITOROUT = EXITOROUT;
                        model.CONTAINER = "";
                        model.SOURCEORDER = "";
                        model.ENABLED = 1;
                        model.ISAVIALABLE = 1;
                        model.OUTENABLED = 0;
                        model.REMARK = "";
                        model.CONTAINERTYPE = "";
                        //if (k == 6)
                        //{
                        //	model.CONTAINERTYPE = "0,1";
                        //}
                        //else if (k == 7)
                        //{
                        //	model.CONTAINERTYPE = "2";
                        //}
                        //else
                        //{
                        //	model.CONTAINERTYPE = "";
                        //}

                        // model.XYZ = XYZ;
                        //  model.CHECKXYZ = CHECKXYZ;
                        // model.SPECS = SPECS;
                        //model.IFMIXING = IFMIXING;
                        model.CREATOR = Session["truename"].ToString();
                        model.CREATETIME = DateTime.Now;
                        model.LASTEDITOR = Session["truename"].ToString();
                        model.LASTEDITTIME = DateTime.Now;
                        service.InsertReturnIdentity(model);
                    }

                }


            }
            return Json(new { result = 0 }, JsonRequestBehavior.AllowGet);
        }
*/
        [HttpPost]
        public JsonResult EditProduct(string ID, string ZONECODE, string LOCATIONCODE, string TYPE, int TUNNELNUM, int LIMITHEIGHT, int ENABLED, int ISAVIALABLE, string REMARK, string CONTAINER, string CONTAINERTYPE, string SOURCEORDER)
        {
            wms_base_location model = new wms_base_location();
            wms_base_location oldmodel = new wms_base_location();
            oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
            try
            {
                //var codeid = ZONECODE +"-"+ LOCATIONCODE;
                //var codecount = service.Query(t => t.ID == codeid&t.ID!=oldmodel.ID).Count;
                //if (codecount > 0)
                //{
                //    return Json(new { result = "库区库位信息重复" }, JsonRequestBehavior.AllowGet);
                //}
                model.ID = ID;

                model.ZONECODE = oldmodel.ZONECODE;

                model.LOCATIONCODE = LOCATIONCODE;
                model.TYPE = TYPE;
                model.ROWNUM = int.Parse(LOCATIONCODE.Substring(0, 2));
                model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                model.LAYERNUM = int.Parse(LOCATIONCODE.Substring(4, 2));
                model.TUNNELNUM = TUNNELNUM;
                model.LIMITHEIGHT = LIMITHEIGHT;

                model.CONTAINER = CONTAINER;
                model.CONTAINERTYPE = CONTAINERTYPE;
                model.SOURCEORDER = SOURCEORDER;

                model.ENABLED = ENABLED;
                model.ISAVIALABLE = ISAVIALABLE;
                model.REMARK = REMARK;
                if (REMARK != "")
                {
                    model.REMARK = REMARK;
                }
                else
                {
                    model.REMARK = null;
                }

                model.CREATOR = oldmodel.CREATOR;
                model.CREATETIME = oldmodel.CREATETIME;
                model.LASTEDITOR = Session["truename"].ToString();
                model.LASTEDITTIME = DateTime.Now;
                var results = (service.Update(model));
                return Json(new { result = results }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [HttpPost]
        public JsonResult DeleteProduct(string ID)
        {
            wms_base_location model = new wms_base_location();
            var result = true;
            try
            {
                //无货且无托盘可删除
                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                if (!string.IsNullOrEmpty(oldmodel.CONTAINER) || !string.IsNullOrEmpty(oldmodel.SOURCEORDER))
                {
                    return Json(new { result = "库位有订单信息或空托盘，不可删除！" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = ID;
                    result = (service.Delete(model));
                    return Json(new { result = result }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 复位库位
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Resetlocation(string ID)
        {

            string result = "";
            try
            {

                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                if (oldmodel.ENABLED == 0 & oldmodel.ISAVIALABLE == 0)
                {
                    result = "此库位已处于冻结状态，不可复位！";

                }
                else if (!string.IsNullOrEmpty(oldmodel.CONTAINER))
                {
                    result = "当前库位含有托盘信息，不可复位！";

                }
                //else if (oldmodel.ENABLED == 1 & oldmodel.ISAVIALABLE == 0)
                //{
                //	result = "库位已被占用，请重新选择冻结库位！";

                //}
                else
                {
                    wms_base_location model = new wms_base_location();
                    model.ID = ID;
                    model.ZONECODE = oldmodel.ZONECODE;

                    model.LOCATIONCODE = oldmodel.LOCATIONCODE;
                    model.TYPE = oldmodel.TYPE;
                    model.ROWNUM = oldmodel.ROWNUM;
                    model.COLUMNNUM = oldmodel.COLUMNNUM;
                    model.LAYERNUM = oldmodel.LAYERNUM;
                    model.TUNNELNUM = oldmodel.TUNNELNUM;
                    model.LIMITHEIGHT = oldmodel.LIMITHEIGHT;

                    model.CONTAINER = "";
                    model.CONTAINERTYPE = "";
                    model.SOURCEORDER = "";
                    model.ENABLED = 1;
                    model.ISAVIALABLE = 1;
                    model.OUTENABLED = 0;
                    model.REMARK = "";
                    model.CONTAINERTYPE = "";

                    model.CREATOR = oldmodel.CREATOR;
                    model.CREATETIME = oldmodel.CREATETIME;
                    model.LASTEDITOR = Session["truename"].ToString();
                    model.LASTEDITTIME = DateTime.Now;
                    result = service.Update(model) ? "true" : "更新失败，WMS内部错误!"; ;

                }
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                result = "更新失败，WMS内部错误!";
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 清空库位
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Clearlocation(string ID)
        {

            var result = true;
            try
            {

                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                var list = inventoryservice.Query(t => t.SOURCEORDER == oldmodel.SOURCEORDER & t.BARCODE == oldmodel.CONTAINER);
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        inventoryservice.Delete(item);
                    }
                }

                service.Update(t => t.CONTAINER == "", t => t.ID == ID);
                service.Update(t => t.SOURCEORDER == "", t => t.ID == ID);
                service.Update(t => t.REMARK == "", t => t.ID == ID);
                service.Update(t => t.ENABLED == 1, t => t.ID == ID);
                service.Update(t => t.ISAVIALABLE == 1, t => t.ID == ID);
                service.Update(t => t.OUTENABLED == 0, t => t.ID == ID);
                service.Update(t => t.CONTAINERTYPE == "", t => t.ID == ID);
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 清空托盘物料
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Clearmatter(string ID)
        {


            try
            {

                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                var list = inventoryservice.Query(t => t.SOURCEORDER == oldmodel.SOURCEORDER & t.BARCODE == oldmodel.CONTAINER);
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        inventoryservice.Delete(item);
                    }
                }
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 冻结库位
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DJlocation(string ID)
        {

            string result = "";
            try
            {

                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                if (oldmodel.ENABLED == 0 & oldmodel.ISAVIALABLE == 0)
                {
                    result = "此库位已处于冻结状态，请勿重复操作！";

                }
                else if (!string.IsNullOrEmpty(oldmodel.CONTAINER))
                {
                    result = "库位为非空状态，请查看实际库位状态，清空库位再重试！";

                }
                else if (oldmodel.ENABLED == 1 & oldmodel.ISAVIALABLE == 0)
                {
                    result = "库位已被占用，请重新选择冻结库位！";

                }
                else
                {
                    wms_base_location model = new wms_base_location();
                    model.ID = ID;
                    model.ZONECODE = oldmodel.ZONECODE;

                    model.LOCATIONCODE = oldmodel.LOCATIONCODE;
                    model.TYPE = oldmodel.TYPE;
                    model.ROWNUM = oldmodel.ROWNUM;
                    model.COLUMNNUM = oldmodel.COLUMNNUM;
                    model.LAYERNUM = oldmodel.LAYERNUM;
                    model.TUNNELNUM = oldmodel.TUNNELNUM;
                    model.LIMITHEIGHT = oldmodel.LIMITHEIGHT;

                    model.CONTAINER = "";
                    model.SOURCEORDER = "";
                    model.ENABLED = 0;
                    model.ISAVIALABLE = 0;
                    model.REMARK = oldmodel.REMARK;
                    model.CONTAINERTYPE = "";

                    model.CREATOR = oldmodel.CREATOR;
                    model.CREATETIME = oldmodel.CREATETIME;
                    model.LASTEDITOR = Session["truename"].ToString();
                    model.LASTEDITTIME = DateTime.Now;
                    result = service.Update(model) ? "true" : "更新失败，WMS内部错误!"; ;

                }
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                result = "更新失败，WMS内部错误!";
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 解封库位
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult JFlocation(string ID)
        {

            string result = "";
            try
            {

                wms_base_location oldmodel = new wms_base_location();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                if (oldmodel.ENABLED == 1)
                {
                    result = "此库位非冻结状态，无需解封库位！";

                }
                else
                {
                    wms_base_location model = new wms_base_location();
                    model.ID = ID;
                    model.ZONECODE = oldmodel.ZONECODE;

                    model.LOCATIONCODE = oldmodel.LOCATIONCODE;
                    model.TYPE = oldmodel.TYPE;
                    model.ROWNUM = oldmodel.ROWNUM;
                    model.COLUMNNUM = oldmodel.COLUMNNUM;
                    model.LAYERNUM = oldmodel.LAYERNUM;
                    model.TUNNELNUM = oldmodel.TUNNELNUM;
                    model.LIMITHEIGHT = oldmodel.LIMITHEIGHT;

                    model.CONTAINER = oldmodel.CONTAINER;
                    model.SOURCEORDER = oldmodel.SOURCEORDER;
                    model.ENABLED = 1;
                    model.ISAVIALABLE = 1;
                    model.REMARK = oldmodel.REMARK;
                    model.CONTAINERTYPE = oldmodel.CONTAINERTYPE;

                    model.CREATOR = oldmodel.CREATOR;
                    model.CREATETIME = oldmodel.CREATETIME;
                    model.LASTEDITOR = Session["truename"].ToString();
                    model.LASTEDITTIME = DateTime.Now;
                    result = service.Update(model) ? "true" : "更新失败，WMS内部错误!"; ;

                }
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                result = "更新失败，WMS内部错误!";
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUsertData()
        {

            string user = "";
            user = Session["username"].ToString().ToLower();

            return Json(user, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取库区下拉列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult GetSearchSelectListByZone(string code, string name)
        {

            var lists = zoneservice.Query(t => t.ENABLED == 1);
            var Platforms = "<option value=''>" + name + "</option>";
            foreach (var item in lists)
            {
                Platforms += "<option value = '" + item.ZONECODE + "'> " + item.ZONENAME + " </option> ";
            }
            return Json(Platforms, JsonRequestBehavior.AllowGet);
        }
    }
}