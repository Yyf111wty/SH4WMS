using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    //[CheckLoginFilter]
    public class ReportWorkController : Controller
    {
        wms_report_workService service = new wms_report_workService();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetRowData(wms_report_work WMS_REPORT_WORK)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_report_work> lists = null;
            try
            {
                lists = service.QueryPage(x =>
                     (WMS_REPORT_WORK.SOURCEORDER == null || x.SOURCEORDER.Contains(WMS_REPORT_WORK.SOURCEORDER))
                     && (WMS_REPORT_WORK.WAREHOUSECODE == null || x.WAREHOUSECODE.Contains(WMS_REPORT_WORK.WAREHOUSECODE))
                     && (WMS_REPORT_WORK.BARCODE == null || x.BARCODE.Contains(WMS_REPORT_WORK.BARCODE))
                     //&& (WMS_REPORT_WORK.ITEMCODE == null || x.ITEMCODE.Contains(WMS_REPORT_WORK.ITEMCODE))
                     //&& (WMS_REPORT_WORK.ITEMNAME == null || x.ITEMNAME.Contains(WMS_REPORT_WORK.ITEMNAME))
                     && (WMS_REPORT_WORK.TYPE == -10 || x.TYPE.Equals(WMS_REPORT_WORK.TYPE))
                     && (WMS_REPORT_WORK.STATE == "state" || WMS_REPORT_WORK.STATE ==null || x.STATE.Equals(WMS_REPORT_WORK.STATE))
                     ,
                     ref TotalCount, pageId, pageSize, "CREATETIME DESC").ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public FileResult Export()
        {
            List<wms_report_work> list = new List<wms_report_work>();
            list = service.Query(a => true);
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { "载具编号", "单据编号", "库位编号", "成功标记",  "创建时间" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            for (int i = 0; i < list.Count; i++)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].BARCODE);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].SOURCEORDER);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].WAREHOUSECODE);
                //sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].QTY);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].TYPE);
                //sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].ITEMCODE);
                //sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].ITEMNAME);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", list[i].CREATETIME);
            }
            sbHtml.Append("</table>");
            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());
            return File(fileContents, "application/ms-excel", DateTime.Now.ToShortDateString() + "报工信息" + ".xls");
        }
    }
}
