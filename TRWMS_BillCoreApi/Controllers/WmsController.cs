using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.LOG;
using TRXWMS.BILL.WCS;
using System.Web;
using TRXWMS.Models.Response;

namespace TRWMS_BillCoreApi.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class WmsController : ControllerBase
	{
       // /3.4/3.9/3.12
		//3.6/3.11
		//3.14/3.7

		/// <summary>
		/// 测试
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpGet, Route("api/wms/GetLogin")]
		public ReslutJson Get(JObject json)
		{

			string ip = HttpContext.Request.PathBase;
			LOGService.WriteOperateLogT(string.Format("用户名:{0}", "乔闯"), "移动设备用户登录验证", ip);
			ReslutJson reslutJson = new ReslutJson()
			{
				success = true,
				msg = "",
				data =
					new Dictionary<string, object>{
						{ "A","Adddd"}
					}
			};
			var DAD = JObject.FromObject(reslutJson.data);
			var dad = DAD["A"].ToString();

			return reslutJson;
		}

		#region WCS和WMS交互
		/// <summary>
		/// WCS->WMS请求库位 3.1
		/// </summary>
		/// <param name="json">参数</param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/grnLocation")]
		public ReslutJson grnLocation(JObject json)
		{

			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.grnLocationTranspor(json, ip);
		}

		/// <summary>
		/// WCS->WMS堆垛机执行
		/// </summary>
		/// <param name="json"></param>
		[HttpPost, Route("api/wms/ImplementStock")]
		public ReslutJson ImplementStock(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.ImplementStock(json, ip);
		}
		/// <summary>
		/// WCS->WMS堆垛机执行测试
		/// </summary>
		/// <param name="json"></param>
		[HttpPost, Route("api/wms/ImplementStocktest")]
		public ReslutJson ImplementStocktest(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.ImplementStocktest(json, ip);
		}
		/// <summary>
		/// WCS->WMS申请巷道 3.6/3.11
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		//[HttpPost, Route("api/wms/grnTransport")]
		//public ReslutJson grnTransport(JObject json)
		//{
		//    string ip =  HttpContext.HttpContext.Request.PathBase;
		//    WCSService cSService = new WCSService();
		//    return cSService.grnTransport(json, ip);

		//}



		/// <summary>
		/// WCS->WMS堆垛机入库任务完成3.2
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/completeGrnRecord")]
		public ReslutJson completeGrnRecord(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.completeGrnRecord(json, ip);
		}

		/// <summary>
		/// WCS->WMS堆垛机入库任务完成3.2  冷藏库
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/AGVcompleteGrnRecord")]
		public ReslutJson AGVcompleteGrnRecord(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.completeGrnRecordtest(json, ip);
		}
		/// <summary>
		/// WCS->WMS堆垛机入库任务完成3.2
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/completeGrnRecordTEST")]
		public ReslutJson completeGrnRecordTEST(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.completeGrnRecordtest(json, ip);
		}
		///// <summary>
		///// WCS->WMS堆垛机出库任务完成3.3
		///// </summary>
		///// <param name="json"></param>
		///// <returns></returns>
		//[HttpPost, Route("api/wms/completeGdnRecord")]
		//public ReslutJson completeGdnRecord(JObject json)
		//{
		//    string ip =  HttpContext.HttpContext.Request.PathBase;
		//    WCSService cSService = new WCSService();
		//    return cSService.completeGdnRecord(json, ip);
		//}

		///// <summary>
		///// WCS->WMS设备叫料 3.4/3.9/3.12 
		///// </summary>
		///// <param name="json"></param>
		///// <returns></returns>
		//[HttpPost, Route("api/wms/agvAutomatic")]
		//public ReslutJson agvAutomatic(JObject json)
		//{
		//    ReslutJson reslutJson = new ReslutJson();
		//    return reslutJson;
		//}

		/// <summary>
		/// WCS->WMS获取agv任务 3.5
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/agvTaskState")]
		public ReslutJson agvTaskState(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.agvTaskState(json, ip);
		}

		/// <summary>
		/// WCS->WMS输送线任务完成 3.8
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/transportTaskState")]
		public ReslutJson transportTaskState(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.transportTaskState(json, ip);
		}

		///// <summary>
		///// WCS->WMS输送线下料 3.10
		///// </summary>
		///// <param name="json"></param>
		///// <returns></returns>
		//[HttpPost, Route("api/wms/outMatReq")]
		//public ReslutJson outMatReq(JObject json)
		//{
		//    string ip =  HttpContext.HttpContext.Request.PathBase;
		//    WCSService cSService = new WCSService();
		//    return cSService.outMatReq(json, ip);

		//}


		/// <summary>
		/// 下发堆垛机任务
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/A01_NewTask")]
		public ReslutJson A01_NewTask(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.A01_NewTask(json, ip);
		}

		/// <summary>
		/// 获取冷藏库AGV任务
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/GetColdTask")]
		public ReslutJson GetColdTask(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.GetAgvTask(json, ip);
		}

		[HttpPost, Route("api/wms/A02_NewTask")]
		public ReslutJson A02_NewTask(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.A02_NewTask(json, ip);
		}
		/// <summary>
		/// WCS生成AGV任务
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/robatConvery")]
		public ReslutJson robatConvery(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.robatConvery(json, ip);
		}
		#endregion


		#region SAP和WMS交互

		/// <summary>
		/// 请求物料空盘或满盘入库
		/// </summary>
		/// <param name="strxml"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_INBOUND")]
		public ReslutSapJson SET_TASK_INBOUND(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskinbound(json, ip);
		}
		/// <summary>
		/// 测试请求物料空盘或满盘入库
		/// </summary>
		/// <param name="strxml"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_INBOUNDTEST")]
		public ReslutSapJson SET_TASK_INBOUNDTEST(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskinboundtest(json, ip);
		}
		/// <summary>
		/// 请求物料空盘或满盘入库(冷藏库)
		/// </summary>
		/// <param name="strxml"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_INBOUND_NEW")]
		public ReslutSapJson SET_TASK_INBOUND_NEW(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskinboundforcold(json, ip);
		}
		/// <summary>
		/// 请求物料空盘或满盘出库
		/// </summary>		
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_OUTBOUND")]
		public ReslutSapJson SET_TASK_OUTBOUND(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskoutbound(json, ip);
		}
		/// <summary>
		/// 请求物料空盘或满盘出库
		/// </summary>		
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_OUTBOUNDFORTEST")]
		public ReslutSapJson SET_TASK_OUTBOUNDFORTEST(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskoutboundfortest(json, ip);
		}
		/// <summary>
		/// 测试请求物料空盘或满盘出库
		/// </summary>		
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_TASK_OUTBOUNDTEST")]
		public ReslutSapJson SET_TASK_OUTBOUNDTEST(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.settaskoutboundtest(json, ip);
		}

		/// <summary>
		/// 请求清除指定任务号的任无
		/// </summary>
		/// <param name="strxml"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/SET_CLEAR_TASK")]
		public ReslutSapJson SET_CLEAR_TASK(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.setcleartask(json, ip);
		}

		/// <summary>
		/// 请求按一定条件查询库位信息
		/// </summary>
		/// <param name="strxml"></param>
		/// <returns></returns>
		[HttpPost, Route("api/wms/GET_LOCATION_INFO")]
		public ReslutSerchSapJson GET_LOCATION_INFO(JObject json)
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.getlocationinfo(json, ip);
		}

		#endregion

		#region   看板接口
		/// <summary>
		/// 请求看板信息
		/// </summary>		
		/// <returns></returns>
		[HttpPost, Route("api/wms/GET_KANBAN_INFO")]
		public ReslutJson GET_KANBAN_INFO()
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.getkanbaninfo(ip);
		}
		/// <summary>
		/// 请求待执行入库信息
		/// </summary>		
		/// <returns></returns>
		[HttpPost, Route("api/wms/GET_UNPUTIN_INFO")]
		public ReslutJson GET_UNPUTIN_INFO()
		{
			string ip =  HttpContext.Request.PathBase;
			WCSService cSService = new WCSService();
			return cSService.getunputininfo(ip);
		}

		#endregion
		#region WMS定时任务
		//[HttpPost, Route("api/wms/inoroutFinishStockerTask")]
		//public ReslutSapJson inoroutFinishStockerTask()
		//{
		//	ReslutJson reslutJson = new ReslutJson();
		//	string ip =  HttpContext.HttpContext.Request.PathBase;
		//	return WMSService.finishstockertask(ip);
		//}

		//      [HttpPost, Route("api/wms/DeleteLog")]
		//      public void DeleteLog()
		//      {
		//          WMSService cSService = new WMSService();
		//          cSService.DeleteLog();
		//      }


		#endregion
	
}
}
