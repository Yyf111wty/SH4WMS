using IdGen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using SqlSugar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using TR.ORM.Model;
using TR.Service;
using TRWMS_BillApi.Models.Request;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.DAL;
using TRXWMS.BILL.DAL.WMS;
using TRXWMS.BILL.EXTENSIONS;
using TRXWMS.BILL.LOG;
using TRXWMS.BILL.MES;
using TRXWMS.Models.Response;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;


namespace TRXWMS.BILL.WCS
{
    public class WCSService
    {


        //public static DbContext context = new DbContext();

        public static DbContext dbcontext = new DbContext();
        public static MESService mesService = new MESService();
        wms_base_locationService service = new wms_base_locationService();
        wms_bill_putinbindingheaderService servicespec = new wms_bill_putinbindingheaderService();
        wms_bill_putinbindingbodyService servicespecbody = new wms_bill_putinbindingbodyService();
        wms_bill_putoutheaderService serviceputoutheader = new wms_bill_putoutheaderService();
        wms_bill_inventorybookService serviceinventorybook = new wms_bill_inventorybookService();
        wms_stocker_taskService servicetask = new wms_stocker_taskService();
        public HttpContext context = HttpContext.Current;

        #region   暂时作废
        /// <summary>
        /// 半成品申请库位(单伸位)参数：托盘号、工位、库区、实际重量，返回值：任务号、任务类型、托盘号、目标库位号、起始工位；string  startcode, string container, string zonecode, string realweight
        /// </summary>
        /// <param name="boxNums"></param>
        /// <param name="startPoint"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        //		public ReslutJson grnLocationTranspor(JObject json, string ip)
        //		{
        //			ReslutJson reslutJson = new ReslutJson();
        //			string startcode = json["startcode"].ToString();
        //			string container = json["container"].ToString();
        //			string zonecode = json["zonecode"].ToString();
        //			string realweight = json["realweight"].ToString();
        //			if (container.Length > 0)
        //			{
        //				//查询入库绑定记录
        //				string sql = @"
        //SELECT DISTINCT A.ID AS ID,A.SOURCEORDER AS SOURCEORDER,A.CONTAINER AS CONTAINER,A.REMARK AS REMARK 
        //FROM wms_bill_putinbindingheader A left JOIN wms_bill_putinbindingbody B ON A.ID=B.HEADERID   
        //WHERE A.CONTAINER=@CONTAINER AND A.ENABLED=1 AND A.QUESTFLAG=0  AND  A.STATE=0 ORDER BY A.CREATETIME DESC ";
        //				var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

        //				  new SugarParameter("@CONTAINER",container)
        //				});
        //				if (bingID != null && bingID.Rows.Count > 0)
        //				{
        //					string ID = bingID.Rows[0]["ID"].ToString();
        //					//int TUNNELNUM = bingID.Rows[0]["TUNNELNUM"].ObjToInt();
        //					//string ITMECODE = bingID.Rows[0]["ITMECODE"].ToString();
        //					string REMARK = bingID.Rows[0]["REMARK"].ToString();
        //					string SOURCEORDER = bingID.Rows[0]["SOURCEORDER"].ToString();
        //					string CONTAINER = bingID.Rows[0]["CONTAINER"].ToString();

        //					//获取仓库配置

        //					sql = @"SELECT  A.ENTPRINCIPLE,A.OUTPRINCIPLE,B.ROWNUM,B.COLUMNNUM,B.SIGINNUM ,B.LAYERNUM   FROM wms_base_warehouse A   JOIN wms_base_zone B  ON  A.WAREHOUSECODE=B.WAREHOUSECODE WHERE A.ENABLED = 1 AND B.ENABLED = 1  AND B.ZONECODE = @ZONECODE  LIMIT 1";
        //					var warehouse = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //					{
        //						  new SugarParameter("@ZONECODE",zonecode)
        //					});
        //					if (warehouse != null && warehouse.Rows.Count > 0)
        //					{
        //						var ENTPRINCIPLE = warehouse.Rows[0]["ENTPRINCIPLE"].ObjToInt();
        //						var ROWNUM = warehouse.Rows[0]["ROWNUM"].ObjToInt();
        //						var COLUMNNUM = warehouse.Rows[0]["COLUMNNUM"].ObjToInt();
        //						var SIGINNUM = warehouse.Rows[0]["SIGINNUM"].ObjToInt();
        //						var LAYERNUM = warehouse.Rows[0]["LAYERNUM"].ObjToInt();
        //						if (SIGINNUM.ObjToInt() == 0)//单伸位
        //						{
        //							if (ENTPRINCIPLE == 4)
        //							{
        //								string code = "";
        //								if (zonecode == "201-1")
        //								{
        //									#region 库位分配  半成品库

        //									//按排列层托盘类型分配
        //									//托盘类型和入库口判断是否
        //									//1～27  1号  其他2号    托盘类型切入具体范围     先判断未放满时执行一套，放满时执行周转库位流程

        //									if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
        //										 startcode == "2024" || startcode == "2026") && (container.Substring(0, 2) == "21" || container.Substring(0, 2) == "22"))//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});

        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{

        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}

        //									else if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
        //										 startcode == "2024" || startcode == "2026") && (container.Substring(0, 2) == "25"))//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=7
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完需要周转时，申请前查看是否有未完成的周转任务
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=7
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//													//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}
        //									else if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
        //									   startcode == "2024" || startcode == "2026") && container.Substring(0, 2) != "25" && container.Substring(0, 2) != "21" && container.Substring(0, 2) != "22")//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM!=7 and LAYERNUM!=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM!=7 and LAYERNUM!=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}

        //									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
        //									   startcode == "2032" || startcode == "2034") && (container.Substring(0, 2) == "21" || container.Substring(0, 2) == "22"))//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														//													string sid = Guid.NewGuid().ToString("D");
        //														//													string sinsertsql = @"
        //														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //														//                       VALUES
        //														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //														//";
        //														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
        //														//					{

        //														//						new SugarParameter("@ID",sid),
        //														//						new SugarParameter("@CODE",sid),
        //														//						new SugarParameter("@ENDTAXI",code),
        //														//						new SugarParameter("@CONTAINER",CONTAINER),
        //														//						new SugarParameter("@STARTTAXI","022704"),
        //														//						new SugarParameter("@ZONECODE",zonecode),
        //														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //														//					});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}
        //									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
        //									   startcode == "2032" || startcode == "2034") && (container.Substring(0, 2) == "25"))//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=7
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=7
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														//													string sid = Guid.NewGuid().ToString("D");
        //														//													string sinsertsql = @"
        //														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //														//                       VALUES
        //														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //														//";
        //														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
        //														//					{

        //														//						new SugarParameter("@ID",sid),
        //														//						new SugarParameter("@CODE",sid),
        //														//						new SugarParameter("@ENDTAXI",code),
        //														//						new SugarParameter("@CONTAINER",CONTAINER),
        //														//						new SugarParameter("@STARTTAXI","022704"),
        //														//						new SugarParameter("@ZONECODE",zonecode),
        //														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //														//					});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}
        //									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
        //									   startcode == "2032" || startcode == "2034") && container.Substring(0, 2) != "25" && container.Substring(0, 2) != "21" && container.Substring(0, 2) != "22")//1号堆垛机范围
        //									{

        //										sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM!=7 and LAYERNUM!=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											//1库位用完申请前查看是否有未完成的周转任务
        //											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
        //											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											  new SugarParameter("@ZONECODE",zonecode)
        //										});
        //											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
        //											{
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = "立库周转库位正在执行任务，请稍候尝试！!",
        //												};
        //												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //											}
        //											else
        //											{
        //												sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM!=7 and LAYERNUM!=6
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";

        //												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //												if (flocation != null && flocation.Rows.Count > 0)
        //												{
        //													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
        //													//需要周转时
        //													try
        //													{
        //														//更新库位信息   入库信息   生成堆垛机任务  
        //														dbcontext.Db.Ado.BeginTran();
        //														sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //														sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //					;
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //														string id = Guid.NewGuid().ToString("D");
        //														string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI","022704"),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});
        //														//更新周转库位为使用状态
        //														string upsql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
        //";
        //														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

        //										new SugarParameter("@LOCATIONCODE","022704"),
        //										new SugarParameter("@ZONECODE",zonecode)

        //									});
        //														//更新入库单为正在处理		
        //														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //														//													string sid = Guid.NewGuid().ToString("D");
        //														//													string sinsertsql = @"
        //														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //														//                       VALUES
        //														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //														//";
        //														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
        //														//					{

        //														//						new SugarParameter("@ID",sid),
        //														//						new SugarParameter("@CODE",sid),
        //														//						new SugarParameter("@ENDTAXI",code),
        //														//						new SugarParameter("@CONTAINER",CONTAINER),
        //														//						new SugarParameter("@STARTTAXI","022704"),
        //														//						new SugarParameter("@ZONECODE",zonecode),
        //														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //														//					});
        //														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //														dbcontext.Db.Ado.CommitTran();

        //														reslutJson = new ReslutJson()
        //														{
        //															success = true,
        //															msg = "WCS请求成功!",
        //															data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", "022704" }
        //												}
        //														};

        //													}
        //													catch (Exception ex)
        //													{
        //														dbcontext.Db.Ado.RollbackTran();
        //														reslutJson = new ReslutJson()
        //														{
        //															success = false,
        //															msg = ex.Message,
        //														};
        //														LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //													}
        //												}
        //												else
        //												{
        //													reslutJson = new ReslutJson()
        //													{
        //														success = false,
        //														msg = "立库无可用库位！!",
        //													};
        //													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //												}
        //											}

        //										}

        //									}
        //									else
        //									{
        //										reslutJson = new ReslutJson()
        //										{
        //											success = false,
        //											msg = "站台号不存在!",
        //										};
        //										LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

        //									}


        //									#endregion
        //								}
        //								else if (zonecode == "301-1")
        //								{
        //									#region 库位分配  成品库时

        //									//按排列层托盘类型分配

        //									if (startcode != "")//1号堆垛机范围
        //									{
        //										//大托盘入库
        //										if (container.Substring(0, 2) == "32")
        //										{
        //											sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND (LAYERNUM<=3 OR (LAYERNUM=4 AND 
        //COLUMNNUM<=40) OR (LAYERNUM>=4 AND 
        //COLUMNNUM=53))  
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";
        //										}
        //										else
        //										{
        //											sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND ((LAYERNUM>3  AND COLUMNNUM!=54) OR (LAYERNUM=4 AND 
        //COLUMNNUM>40) )
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";
        //										}


        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});

        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											reslutJson = new ReslutJson()
        //											{
        //												success = false,
        //												msg = "立库无可用库位!",
        //											};
        //											LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

        //										}

        //									}

        //									else
        //									{
        //										reslutJson = new ReslutJson()
        //										{
        //											success = false,
        //											msg = "站台号不存在!",
        //										};
        //										LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

        //									}


        //									#endregion
        //								}
        //								else
        //								{
        //									#region 库位分配  原材料时

        //									//按排列层托盘类型分配

        //									if (startcode != "")//1号堆垛机范围
        //									{
        //										if (REMARK == "空托盘入库")
        //										{
        //											sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND (COLUMNNUM<62 AND COLUMNNUM>1)  
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";
        //										}
        //										else
        //										{
        //											sql = @"
        //SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
        //WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  
        //ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
        //";
        //										}



        //										var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //										{
        //											 new SugarParameter("@ZONECODE",zonecode)
        //										});

        //										if (location != null && location.Rows.Count > 0)
        //										{
        //											code = location.Rows[0]["LOCATIONCODE"].ObjToString();
        //											//不需要周转时
        //											try
        //											{
        //												//更新库位信息   入库信息   生成堆垛机任务  
        //												dbcontext.Db.Ado.BeginTran();
        //												sql = @"
        //UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //											new SugarParameter("@REMARK",CONTAINER),
        //											new SugarParameter("@ZONECODE",zonecode),
        //											new SugarParameter("@LOCATIONCODE",code)
        //										});
        //												sql = @"
        //UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //WHERE   ID=@ID"
        //			;
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //												   new SugarParameter("@LOCATIONCODE",code),
        //													new SugarParameter("@ID",ID)

        //												});
        //												string id = Guid.NewGuid().ToString("D");
        //												string insertsql = @"
        //INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
        //                       VALUES
        //                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
        //";
        //												dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
        //					{

        //						new SugarParameter("@ID",id),
        //						new SugarParameter("@CODE",id),
        //						new SugarParameter("@ENDTAXI",code),
        //						new SugarParameter("@CONTAINER",CONTAINER),
        //						new SugarParameter("@STARTTAXI",startcode),
        //						new SugarParameter("@ZONECODE",zonecode),
        //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

        //					});

        //												//更新入库单为正在处理		
        //												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
        //												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //													new SugarParameter("@ID",id)

        //												});
        //												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //												dbcontext.Db.Ado.CommitTran();

        //												reslutJson = new ReslutJson()
        //												{
        //													success = true,
        //													msg = "WCS请求成功!",
        //													data = new Dictionary<string, string>() {
        //													{"taskId", id },
        //													{"taskType", "1" },
        //													{ "conTainer", CONTAINER },
        //													{ "startTaxi", startcode },
        //													{ "endTaxi", code }
        //												}
        //												};

        //											}
        //											catch (Exception ex)
        //											{
        //												dbcontext.Db.Ado.RollbackTran();
        //												reslutJson = new ReslutJson()
        //												{
        //													success = false,
        //													msg = ex.Message,
        //												};
        //												LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //											}
        //										}
        //										else
        //										{
        //											reslutJson = new ReslutJson()
        //											{
        //												success = false,
        //												msg = "立库无可用库位!",
        //											};
        //											LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

        //										}

        //									}

        //									else
        //									{
        //										reslutJson = new ReslutJson()
        //										{
        //											success = false,
        //											msg = "站台号不存在!",
        //										};
        //										LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

        //									}


        //									#endregion
        //								}


        //								#region   作废
        //								//								if (code == "")
        //								//								{
        //								//									reslutJson = new ReslutJson()
        //								//									{
        //								//										success = false,
        //								//										msg = "立库无可用库位!",
        //								//									};
        //								//									LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
        //								//								}
        //								//								else
        //								//								{
        //								//									try
        //								//									{
        //								//										//更新库位信息   入库信息   生成堆垛机任务  
        //								//										dbcontext.Db.Ado.BeginTran();
        //								//										sql = @"
        //								//UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
        //								//WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
        //								//";
        //								//										int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

        //								//											new SugarParameter("@REMARK",CONTAINER),
        //								//											new SugarParameter("@ZONECODE",zonecode),
        //								//											new SugarParameter("@LOCATIONCODE",code)
        //								//										});
        //								//										sql = @"
        //								//UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
        //								//WHERE   ID=@ID"
        //								//	;
        //								//										int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //								//												   new SugarParameter("@LOCATIONCODE",code),
        //								//													new SugarParameter("@ID",ID)

        //								//												});
        //								//										//                                        sql = @"
        //								//										//UPDATE WMS_BILL_PUTINBINDINGBODY SET ENABLED=1 ,LASTEDITTIME=GETDATE() WHERE HEADERID=@ID AND BARCODE IN(" + sb.ToString().Substring(0, sb.Length - 1) + ")";
        //								//										//                                        int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //								//										//                                                    new SugarParameter("@ID",ID),
        //								//										//                                                });
        //								//										//                                        sql = @"
        //								//										//DELETE WMS_BILL_PUTINBINDINGBODY WHERE HEADERID=@ID AND BARCODE NOT IN(" + sb.ToString().Substring(0, sb.Length - 1) + ")";
        //								//										//                                        int d = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
        //								//										//                                                    new SugarParameter("@ID",ID),
        //								//										//                                                });
        //								//										dbcontext.Db.Ado.CommitTran();
        //								//										if (a > 0 && b > 0)
        //								//										{
        //								//											string id = Guid.NewGuid().ToString("D");
        //								//											wms_stocker_task wmsStocker = new wms_stocker_task();
        //								//											wmsStocker.ID = id;
        //								//											wmsStocker.CODE = id;
        //								//											wmsStocker.ENDTAXI = code;
        //								//											wmsStocker.CONTAINER = CONTAINER;
        //								//											wmsStocker.STARTTAXI = startcode;//起始点

        //								//											wmsStocker.TYPE = 1;
        //								//											wmsStocker.STARTTIME = DateTime.Now;
        //								//											wmsStocker.ZONECODE = zonecode;
        //								//											wmsStocker.SOURCEORDER = SOURCEORDER;
        //								//											wmsStocker.PRIORITY = 0;
        //								//											wmsStocker.STATE = 0;
        //								//											WmsStockerTaskDao.insert(wmsStocker);
        //								//											LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

        //								//											reslutJson = new ReslutJson()
        //								//											{
        //								//												success = true,
        //								//												msg = "WCS请求成功!",
        //								//												data = new Dictionary<string, string>() {
        //								//													{"taskId", id },
        //								//													{"taskType", "1" },
        //								//													{ "conTainer", CONTAINER },
        //								//													{ "startTaxi", startcode },
        //								//													{ "endTaxi", code }
        //								//												}
        //								//											};
        //								//										}
        //								//										else
        //								//										{
        //								//											dbcontext.Db.Ado.RollbackTran();
        //								//											reslutJson = new ReslutJson()
        //								//											{
        //								//												success = false,
        //								//												msg = "WMS内部错误",
        //								//											};
        //								//										}
        //								//									}

        //								//									catch (Exception ex)
        //								//									{
        //								//										dbcontext.Db.Ado.RollbackTran();
        //								//										reslutJson = new ReslutJson()
        //								//										{
        //								//											success = false,
        //								//											msg = ex.Message,
        //								//										};
        //								//										LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
        //								//									}
        //								//								}
        //								#endregion
        //							}
        //						}
        //						else
        //						{
        //							reslutJson = new ReslutJson()
        //							{
        //								success = false,
        //								msg = "立库配置单双伸类型不对!",
        //							};
        //							LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "托盘立库请求库位", ip);
        //						}

        //					}
        //					else
        //					{
        //						reslutJson = new ReslutJson()
        //						{
        //							success = false,
        //							msg = "请查看立库仓库配置!",
        //						};
        //						LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "托盘立库请求库位", ip);
        //					}
        //				}

        //				else
        //				{
        //					reslutJson = new ReslutJson()
        //					{
        //						success = false,
        //						msg = "错误:无入库单或入库单非新建状态!"
        //					};
        //				}
        //			}
        //			else
        //			{
        //				reslutJson = new ReslutJson()
        //				{
        //					success = false,
        //					msg = "WCS上传的条码为空!"
        //				};
        //			}

        //			return reslutJson;


        //		}

        #endregion
        /// <summary>
        /// 半成品申请库位(单伸位)参数：托盘号、工位、库区、实际重量，返回值：任务号、任务类型、托盘号、目标库位号、起始工位；string  startcode, string container, string zonecode, string realweight
        /// </summary>
        /// <param name="boxNums"></param>
        /// <param name="startPoint"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson grnLocationTransportest(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            string startcode = json["startcode"].ToString();//入库口
            string container = json["container"].ToString();
            string zonecode = json["zonecode"].ToString();
            string inorout = json["inorout"].ToString();//是出库还是入库




            #region 库位分配  
            //按排列层托盘类型分配
            string sql = "";
            string code = "";
            //string code = "";
            if (inorout == "out")
            {
                //出库任务  生成输送线任务

                string gsql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,LOCATIONCODE  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE
";
                var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",container),
                        new SugarParameter("@ZONECODE",zonecode)

                    });

                if (glist != null && glist.Rows.Count > 0)
                {   //获取出库起点
                    string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                    string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                    string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();

                    #region  执行出库   1.插入入库单和入库明细 2.生成堆垛机出库任务


                    {
                        dbcontext.Db.Ado.BeginTran();

                        //同区不需要周转时
                        string taskid = Guid.NewGuid().ToString("D");
                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",startcode),
                        new SugarParameter("@CONTAINER",container),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",SOURCEORDER)

                    });

                        dbcontext.Db.Ado.CommitTran();
                        reslutJson = new ReslutJson()
                        {
                            success = true,
                            msg = "WCS请求成功!",
                            data = new Dictionary<string, string>() {
                                                    {"taskId", taskid },
                                                    {"taskType", "1" },
                                                    { "conTainer", container },
                                                    { "startTaxi", LOCATIONCODE },
                                                    { "endTaxi", startcode }
                                                }
                        };
                    }
                    #endregion


                }


                //				string id = Guid.NewGuid().ToString("D");
                //				string insertsql = @"
                //INSERT INTO wms_conveyor_task (ID,STARTTAXI,ENDTAXI,CONTAINER,TYPE,ZONECODE,SOURCEORDER,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                //                       VALUES
                //                       (@ID,@STARTTAXI, @ENDTAXI,@CONTAINER,2,@ZONECODE,@SOURCEORDER,0,sysdate(),'WMS','WMS',sysdate()) 
                //";
                //				dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                //					{

                //						new SugarParameter("@ID",id),						
                //						new SugarParameter("@ENDTAXI","1022"),
                //						new SugarParameter("@CONTAINER",container),
                //						new SugarParameter("@STARTTAXI",startcode),
                //						new SugarParameter("@ZONECODE",zonecode),
                //						new SugarParameter("@SOURCEORDER","test001")

                //					});

                //更新入库单为正在处理		
                //sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
                //dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                //									new SugarParameter("@ID",id)

                //								});

            }
            else
            {

                if (startcode != "")//1号堆垛机范围
                {
                    //更新库位信息   入库信息   生成堆垛机任务  
                    dbcontext.Db.Ado.BeginTran();
                    sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location  
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1  FOR UPDATE 
";

                    var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                        {
                                             new SugarParameter("@ZONECODE",zonecode)
                                        });

                    if (location != null && location.Rows.Count > 0)
                    {
                        code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                        //不需要周转时
                        try
                        {

                            sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
                            dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                            new SugarParameter("@REMARK",container),
                                            new SugarParameter("@ZONECODE",zonecode),
                                            new SugarParameter("@LOCATIONCODE",code)
                                        });

                            string id = Guid.NewGuid().ToString("D");
                            string insertsql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                            dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@CODE",id),
                        new SugarParameter("@ENDTAXI",code),
                        new SugarParameter("@CONTAINER",container),
                        new SugarParameter("@STARTTAXI",startcode),
                        new SugarParameter("@ZONECODE",zonecode),
                        new SugarParameter("@SOURCEORDER","test001")

                    });

                            //更新入库单为正在处理		
                            //sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
                            //dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                            //						new SugarParameter("@ID",id)

                            //					});
                            LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

                            dbcontext.Db.Ado.CommitTran();

                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS请求成功!",
                                data = new Dictionary<string, string>() {
                                                    {"taskId", id },
                                                    {"taskType", "1" },
                                                    { "conTainer", container },
                                                    { "startTaxi", startcode },
                                                    { "endTaxi", code }
                                                }
                            };

                        }
                        catch (Exception ex)
                        {
                            dbcontext.Db.Ado.RollbackTran();
                            reslutJson = new ReslutJson()
                            {
                                success = false,
                                msg = ex.Message,
                            };
                            LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
                        }
                    }
                    else
                    {
                        reslutJson = new ReslutJson()
                        {
                            success = false,
                            msg = "立库无可用库位!",
                        };
                        LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

                    }

                }

                else
                {
                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "站台号不存在!",
                    };
                    LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);

                }
            }
            #endregion


            return reslutJson;


        }

        /// <summary>
        ///堆垛机任务执行测试
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        public ReslutJson ImplementStocktest(JObject json, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            #region 
            try
            {
                string id = json["taskId"].ToString();

                //SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
                dbcontext.Db.Ado.ExecuteCommand("SET TRANSACTION ISOLATION LEVEL REPEATABLE READ");

                dbcontext.Db.Ado.BeginTran();
                string sql = @"
SELECT SOURCEORDER,ZONECODE,ENDTAXI,TYPE,STARTTAXI FROM wms_stocker_task WHERE ID=@ID AND STATE=0  for  update
";
                var stockerTask = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {
                    new SugarParameter("@ID",id)
                });
                if (stockerTask != null && stockerTask.Rows.Count > 0)
                {
                    string sourceOrder = stockerTask.Rows[0]["SOURCEORDER"].ToString();
                    string zoneCode = stockerTask.Rows[0]["ZONECODE"].ToString();
                    string endTaxi = stockerTask.Rows[0]["ENDTAXI"].ToString();
                    int type = stockerTask.Rows[0]["TYPE"].ObjToInt();
                    string startTaxi = stockerTask.Rows[0]["STARTTAXI"].ToString();

                    {

                        //WCS上报正在执行的任务
                        sql = @"
UPDATE wms_stocker_task SET IMPLEMENTTIME=sysdate(),LASTEDITOR='WCS',LASTEDITTIME=sysdate(), STATE=1 WHERE ID=@ID AND TYPE=@TYPE AND STATE=0
";
                        int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                            new SugarParameter("@ID",id),
                            new SugarParameter("@TYPE",type)
                        });
                        dbcontext.Db.Ado.CommitTran();
                        if (b > 0)
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS通知成功"
                            };
                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", sourceOrder), "WCS通知WMS任务执行(入库移库)", ip);
                        }
                        else
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = false,
                                msg = "WCS通知失败"
                            };
                        }
                    }

                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", id), "WCS通知WMS任务执行", ip);
                }
            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message
                };
                //throw;
            }

            return reslutJson;
            #endregion
        }



        /// <summary>
        ///堆垛机任务执行
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        public ReslutJson ImplementStock(JObject json, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            #region 
            try
            {
                string id = json["taskId"].ToString();
                string sql = @"
SELECT SOURCEORDER,ZONECODE,ENDTAXI,TYPE,STARTTAXI,STATE FROM wms_stocker_task WHERE ID=@ID
";
                var stockerTask = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {
                    new SugarParameter("@ID",id)
                });
                if (stockerTask != null && stockerTask.Rows.Count > 0)
                {
                    int taskState = stockerTask.Rows[0]["STATE"].ObjToInt();
                    if (taskState != 0)
                    {
                        return new ReslutJson()
                        {
                            success = true,
                            msg = taskState == 2 ? "任务已完成" : "任务已在执行中"
                        };
                    }

                    string sourceOrder = stockerTask.Rows[0]["SOURCEORDER"].ToString();
                    string zoneCode = stockerTask.Rows[0]["ZONECODE"].ToString();
                    string endTaxi = stockerTask.Rows[0]["ENDTAXI"].ToString();
                    int type = stockerTask.Rows[0]["TYPE"].ObjToInt();
                    string startTaxi = stockerTask.Rows[0]["STARTTAXI"].ToString();
                    if (type == 1 || type == 3)//入库 或回库
                    {
                        dbcontext.Db.Ado.BeginTran();
                        //WCS上报正在执行的任务
                        sql = @"
UPDATE wms_stocker_task SET IMPLEMENTTIME=sysdate(),LASTEDITOR='WCS',LASTEDITTIME=sysdate(), STATE=1 WHERE ID=@ID AND TYPE=@TYPE AND STATE=0
";
                        int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                            new SugarParameter("@ID",id),
                            new SugarParameter("@TYPE",type)
                        });
                        dbcontext.Db.Ado.CommitTran();
                        if (b > 0)
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS通知成功"
                            };
                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", sourceOrder), "WCS通知WMS任务执行(入库移库)", ip);
                        }
                        else
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = false,
                                msg = "WCS通知失败"
                            };
                        }
                    }
                    else
                    {
                        dbcontext.Db.Ado.BeginTran();
                        //                        sql = @"
                        // SELECT ID,BARCODE,GROUPCODE AS GROUPCODE ,STARTTAXI,ENDTAXI,PRIORITY FROM WMS_AGV_TASK WHERE SOURCEORDER =@SOURCEORDER AND WAREHOUSECODE=@WAHOUSECODE AND STATE=0
                        // ORDER BY PRIORITY ASC
                        //";
                        //                        var AgvTask = dbcontext.Db.Ado.GetDataTable(sql,new List<SugarParameter>() {
                        //                            new SugarParameter("@SOURCEORDER",sourceOrder),
                        //                            new SugarParameter("@WAHOUSECODE",warehouseCode),
                        //                        });
                        //给WCS
                        sql = @"
UPDATE wms_stocker_task SET IMPLEMENTTIME=sysdate(),LASTEDITOR='WCS',LASTEDITTIME=sysdate(), STATE=1 WHERE ID=@ID AND TYPE=@TYPE AND STATE=0
";
                        int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                            new SugarParameter("@ID",id),
                              new SugarParameter("@TYPE",type)
                        });
                        //                        sql = @"
                        //INSERT INTO WMS_CONVEYOR_TASK(ID,SOURCEORDER,BARCODE,ITEMCODE,ITEMNAME,
                        //TYPE,STATE,CREATOR,CREATETIME,LASTEDITOR,LASTEDITTIME,STARTTAXI,ENDTAXI,QTY,BATCHQTY,CONTAINER,WAREHOUSECODE)
                        //SELECT NEWID(),SOURCEORDER,BARCODE,ITEMCODE,ITEMNAME,1,0,'WMS',GETDATE(),'WMS',GETDATE(),@CODE,@ENDTAXI,QTY,
                        //LOT,SYSBOLCODE,WAREHOUSECODE FROM WMS_BILL_INVENTORYBOOK WHERE WAREHOUSECODE=@WAREHOUSECODE
                        //AND LOCATIONCODE= @CODE
                        //";
                        //                        int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                        //                            new SugarParameter("@WAREHOUSECODE",warehouseCode),
                        //                            new SugarParameter("@CODE",startTaxi),
                        //                            new SugarParameter("@ENDTAXI",endTaxi),
                        //                        });
                        dbcontext.Db.Ado.CommitTran();
                        if (b > 0)
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS通知成功"
                            };
                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", sourceOrder), "WCS通知WMS任务执行(出库)", ip);
                        }
                        else
                        {
                            reslutJson = new ReslutJson()
                            {
                                success = false,
                                msg = "WCS通知失败"
                            };
                            dbcontext.Db.Ado.RollbackTran();
                        }
                    }
                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", id), "WCS通知WMS任务执行", ip);
                }
                else
                {
                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "任务号无任务"
                    };
                }
            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message
                };
                //throw;
            }
            return reslutJson;
            #endregion
        }





        /// <summary>
        /// 堆垛机任务完成  参数：任务号、任务类型、托盘号，返回值：成功失败
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson completeGrnRecord(JObject json, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string id = json["taskId"].ToString();
                //查询堆垛机任务情况
                string sql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,STARTTAXI,ENDTAXI,TYPE,STATE FROM wms_stocker_task WHERE ID=@ID 
";
                var stockerTask = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {
                    new SugarParameter("@ID",id)
                });
                if (stockerTask != null && stockerTask.Rows.Count > 0)
                {
                    int taskState = stockerTask.Rows[0]["STATE"].ObjToInt();
                    if (taskState == 2)
                    {
                        return new ReslutJson()
                        {
                            success = true,
                            msg = "任务已完成"
                        };
                    }

                    string sourceOrder = stockerTask.Rows[0]["SOURCEORDER"].ToString();
                    string warehouseCode = stockerTask.Rows[0]["ZONECODE"].ToString();
                    string endTaxi = stockerTask.Rows[0]["ENDTAXI"].ToString();
                    int type = stockerTask.Rows[0]["TYPE"].ObjToInt();
                    string startTaxi = stockerTask.Rows[0]["STARTTAXI"].ToString();
                    string container = stockerTask.Rows[0]["CONTAINER"].ToString();
                    if (type == 1 || type == 3)
                    {
                        //冷藏库
                        if (warehouseCode != "401-1")
                        {
                            reslutJson = completeEnterKu(sourceOrder, warehouseCode, container, endTaxi, id, ip);
                        }
                        //else
                        //{
                        //	//入到冷藏库时
                        //	if (endTaxi.Length == 6)
                        //	{
                        //		reslutJson = completeEnterKu(sourceOrder, warehouseCode, container, endTaxi, id, ip);
                        //	}
                        //}

                    }
                    else if (type == 2)
                    {
                        reslutJson = completeOutKu(sourceOrder, warehouseCode, container, startTaxi, endTaxi, id, ip);
                    }



                }
                else
                {
                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "任务号无任务"
                    };
                }

                LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "堆垛机任务完成上报");
                return reslutJson;

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteOperateLogF(string.Format("堆垛机任务号:{0},原因:{1}", json["taskId"].ToString(), ex.ToString()), "堆垛机任务完成", ip);
                LOGService.WriteWcsRequestLog(context, json, string.Format("堆垛机任务号:{0},原因:{1}", json["taskId"].ToString(), ex.ToString()), "堆垛机任务完成上报");
            }
            return reslutJson;
        }



        /// <summary>
        /// 堆垛机任务完成  参数：任务号、任务类型、托盘号，返回值：成功失败
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson completeGrnRecordtest(JObject json, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string id = json["taskId"].ToString();
                string mymes = "";
                //查询堆垛机任务情况
                string sql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,STARTTAXI,ENDTAXI,TYPE FROM wms_stocker_task WHERE ID=@ID 
";
                var stockerTask = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {
                    new SugarParameter("@ID",id)
                });
                if (stockerTask != null && stockerTask.Rows.Count > 0)
                {
                    string sourceOrder = stockerTask.Rows[0]["SOURCEORDER"].ToString();
                    string warehouseCode = stockerTask.Rows[0]["ZONECODE"].ToString();
                    string endTaxi = stockerTask.Rows[0]["ENDTAXI"].ToString();
                    int type = stockerTask.Rows[0]["TYPE"].ObjToInt();
                    string startTaxi = stockerTask.Rows[0]["STARTTAXI"].ToString();
                    string container = stockerTask.Rows[0]["CONTAINER"].ToString();

                    if (type == 1 || type == 3)
                    {
                        //fei冷藏库
                        if (warehouseCode != "401-1")
                        {
                            reslutJson = completeEnterKu(sourceOrder, warehouseCode, container, endTaxi, id, ip);
                        }


                    }
                    else if (type == 2)
                    {
                        //reslutJson = new ReslutJson()
                        //{
                        //	success = true,
                        //	msg = "已确认出库已成功!",
                        //};
                        reslutJson = completeOutKu(sourceOrder, warehouseCode, container, startTaxi, endTaxi, id, ip);
                    }
                    //if (reslutJson.success == true)
                    //{
                    //	if (type != 2)
                    //	{
                    //		if (warehouseCode != "401-1")
                    //		{
                    //			string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
                    //			//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
                    //			var content = new
                    //			{
                    //				TASK_NO = sourceOrder,
                    //				TASK_TYPE = "IN",
                    //				LENUM = container,
                    //				BOUND_SUCCESS = 1,
                    //				MESSAGE = "入库成功" + mymes
                    //			};
                    //			HttpRequestPost.HttpPostMethod(url, content);
                    //			//交互成功执行的操作
                    //		}
                    //		else
                    //		{
                    //			if (endTaxi.Length == 6)
                    //			{
                    //				string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
                    //				//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
                    //				var content = new
                    //				{
                    //					TASK_NO = sourceOrder,
                    //					TASK_TYPE = "IN",
                    //					LENUM = container,
                    //					BOUND_SUCCESS = 1,
                    //					MESSAGE = "入库成功" + mymes
                    //				};
                    //				HttpRequestPost.HttpPostMethod(url, content);
                    //			}


                    //		}


                    //	}
                    //	else
                    //	{
                    //		string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
                    //		//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
                    //		var content = new
                    //		{
                    //			TASK_NO = sourceOrder,
                    //			TASK_TYPE = "OUT",
                    //			LENUM = container,
                    //			BOUND_SUCCESS = 1,
                    //			MESSAGE = "出库成功" + mymes
                    //		};

                    //		HttpRequestPost.HttpPostMethod(url, content);


                    //	}
                    //}




                }
                else
                {
                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "任务号无任务"
                    };
                }
                return reslutJson;

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteOperateLogF(string.Format("堆垛机任务号:{0},原因:{1}", json["taskId"].ToString(), ex.ToString()), "堆垛机任务完成", ip);
            }
            return reslutJson;
        }


        /// <summary>
        /// 堆垛机入库任务完成   
        /// </summary>
        /// <param name="sourceOrder"></param>
        /// <param name="warehouseCode"></param>
        /// <param name="endTaxi"></param>
        /// <param name="id"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson completeEnterKu(string sourceOrder, string warehouseCode, string container, string endTaxi, string id, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string sql = "";
                string exmes = "";
                try
                {
                    int b = 0;
                    dbcontext.Db.Ado.BeginTran();
                    var containertype = "1";
                    //半成品库时

                    {

                        sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=0,OUTENABLED=1,CONTAINERTYPE=@CONTAINERTYPE,
LASTEDITTIME = sysdate() WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
                        b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                        new SugarParameter("@SOURCEORDER",sourceOrder),
                                        new SugarParameter("@CONTAINERTYPE",containertype),
                                        new SugarParameter("@CONTAINER",container),
                                        new SugarParameter("@LOCATIONCODE",endTaxi),
                                        new SugarParameter("@ZONECODE",warehouseCode)

                                    });

                        //更新堆垛机状态
                        sql = @"
UPDATE wms_stocker_task SET STATE=2 ,FINISHTIME=sysdate(),LASTEDITOR='WCS' WHERE ID=@ID
";
                        int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                    new SugarParameter("@ID",id),

                });
                        //更新库存状态 空托盘入库不需更新库存
                        sql = @"
SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE,TASKTYPE
FROM wms_bill_putinbindingheader WHERE  CONTAINER=@CONTAINER AND LOCATIONCODE=@LOCATIONCODE  AND  QUESTFLAG=1 AND STATE=1";
                        var Header = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                new SugarParameter("@CONTAINER",container),
                                new SugarParameter("@LOCATIONCODE",endTaxi),
                });
                        var headid = Header.Rows[0][0].ToString();
                        var SUPPLIER = Header.Rows[0]["TASKTYPE"].ToString();
                        //更新入库单状态
                        sql = @"UPDATE wms_bill_putinbindingheader SET STATE =2,FINISHTIME=sysdate() WHERE ID = @ID";
                        int cc = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {


                                 new SugarParameter("@ID",headid)
                });
                        //                        //更新入库单状态
                        //                        sql = @"
                        //UPDATE wms_bill_putinbindingheader SET STATE =2,FINISHTIME=sysdate() WHERE  CONTAINER=@CONTAINER AND LOCATIONCODE=@LOCATIONCODE  AND  QUESTFLAG=1 AND STATE=1
                        //";
                        //                        int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {


                        //                                 new SugarParameter("@CONTAINER",container),
                        //                                new SugarParameter("@LOCATIONCODE",endTaxi),
                        //                });

                        //                        //更新库存状态 空托盘入库不需更新库存
                        //                        sql = @"
                        //SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE 
                        //FROM wms_bill_putinbindingheader 
                        //WHERE CONTAINER=@CONTAINER   ";
                        //                        var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

                        //                                 new SugarParameter("@CONTAINER",container)
                        //                });
                        //                        var headid = bingID.Rows[0][0].ToString();

                        sql = @"
SELECT *
FROM wms_bill_putinbindingbody 
WHERE HEADERID=@HEADERID   ";
                        var bodylist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

                                 new SugarParameter("@HEADERID",headid)
                });

                        // ===== 单针改造：入库完成，本盒针 INBOUND_PENDING→AVAILABLE（历史盒无针数据自动跳过，返回 0） =====
                        // needleQty = 本盒实际翻转针数，用于同步库存台账 QTY 与入库单明细 QTY
                        int needleQty = NeedleService.MarkInboundComplete(dbcontext.Db, container, "WCS");
                        if (needleQty > 0)
                        {
                            // 同步入库单明细 QTY = 盒内实际针数
                            dbcontext.Db.Ado.ExecuteCommand("UPDATE wms_bill_putinbindingbody SET QTY=@QTY, LASTEDITTIME=sysdate() WHERE HEADERID=@HEADERID",
                                new List<SugarParameter>() {
                                    new SugarParameter("@QTY", needleQty),
                                    new SugarParameter("@HEADERID", headid)
                                });
                        }

                        //若有明细则添加
                        if (bodylist != null && bodylist.Rows.Count > 0)
                        {
                            foreach (DataRow dr in bodylist.Rows)
                            {
                                string myid = Guid.NewGuid().ToString("D");
                                //string SOURCEORDER = dr["SOURCEORDER"].ToString();
                                //string CONTAINER = dr["CONTAINER"].ToString();
                                string ITEMCODE = dr["ITEMCODE"].ToString();
                                string LOT = dr["LOT"].ToString();
                                string TASK_NO_SN = dr["TASK_NO_SN"].ToString();
                                //string QTY = dr["QTY"].ToString();
                                // ===== 单针改造：库存台账 QTY 写入本盒实际针数（needleQty=0 为历史盒，QTY 维持 NULL 与旧行为一致） =====
                                string insertsql = @"
INSERT INTO wms_bill_inventorybook (ID,SOURCEORDER,BARCODE,ITEMNAME,ITEMCODE,LOT,QTY,REMARK,CREATOR,CREATETIME,SUPPLIERID)
                       VALUES
                       (@ID,@SOURCEORDER, @BARCODE,@ITEMNAME,@ITEMCODE,@LOT,@QTY,'','SAP',sysdate(),@SUPPLIERID)
";
                                dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",myid),
                        new SugarParameter("@SOURCEORDER",sourceOrder),
                        new SugarParameter("@BARCODE",container),
                        new SugarParameter("@ITEMNAME",TASK_NO_SN),
                        new SugarParameter("@ITEMCODE",ITEMCODE),
                        new SugarParameter("@LOT",LOT),
                        new SugarParameter("@QTY", needleQty > 0 ? (object)needleQty : DBNull.Value),
                        new SugarParameter("@SUPPLIERID",SUPPLIER)
                    });
                            }

                        }


                    }


                    dbcontext.Db.Ado.CommitTran();
                    //报工完成入库

                    reslutJson = new ReslutJson()
                    {
                        success = true,
                        msg = "已确认入库已成功!" + exmes
                    };
                    //LOGService.WriteOperateLogT(string.Format("任务号:{0},库位:{1}", sourceOrder, warehouseCode + "|" + endTaxi), "入库完成", ip);


                }

                catch (Exception)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    throw;
                }

            }
            catch (Exception ex)
            {

                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteOperateLogF(string.Format("任务号:{0},原因:{1}", sourceOrder, warehouseCode + "|" + reslutJson.msg), "入库完成", ip);
            }


            return reslutJson;
        }


        /// <summary>
        /// 出库任务完成
        /// </summary>
        /// <param name="sourceOrder"></param>
        /// <param name="warehouseCode"></param>
        /// <param name="startTaxi"></param>
        /// <param name="endTaxi"></param>
        /// <param name="id"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson completeOutKu(string sourceOrder, string warehouseCode, string container, string startTaxi, string endTaxi, string id, string ip)
        {

            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string sql = "";
                try
                {
                    int b = 0;

                    //出库完成  1.更新出库单状态 2.更新堆垛机任务  3.删除库存，更新库位信息4.上报sap
                    dbcontext.Db.Ado.BeginTran();
                    #region  注释
                    //查询是否是盘点出库
                    //string IFCOUNT = "";
                    //                string sql1 = @"
                    //SELECT FREEZEREASON FROM WMS_BILL_INVENTORYBOOK WHERE WAREHOUSECODE = @WAREHOUSECODE AND LOCATIONCODE = @LOCATIONCODE
                    //";
                    //                var FREEZEREASON = dbcontext.Db.Ado.GetDataTable(sql1, new List<SugarParameter>() {
                    //                    new SugarParameter("@WAREHOUSECODE",warehouseCode),
                    //                    new SugarParameter("@LOCATIONCODE",startTaxi)
                    //                });
                    //                if(FREEZEREASON != null && FREEZEREASON.Rows.Count > 0)
                    //                {
                    //                    IFCOUNT = FREEZEREASON.Rows[0]["FREEZEREASON"].ObjToString();
                    //                }

                    #endregion

                    //其他库位报完成时

                    {

                        //更新库区绑定关系
                        sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=1,OUTENABLED=0,CONTAINERTYPE=@CONTAINERTYPE,REMARK=@REMARK  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
";
                        b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                     new SugarParameter("@SOURCEORDER",""),
                                        new SugarParameter("@CONTAINER",""),
                                        new SugarParameter("@CONTAINERTYPE",""),
                                        new SugarParameter("@REMARK",""),
                                        new SugarParameter("@LOCATIONCODE",startTaxi),
                                        new SugarParameter("@ZONECODE","101")

                                    });

                        //更新堆垛机任务状态
                        sql = @"
UPDATE wms_stocker_task SET STATE=2 ,FINISHTIME=sysdate(),LASTEDITOR='WCS' WHERE ID=@ID
";
                        int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                    new SugarParameter("@ID",id),

                });
                        //更新出库单状态
                        sql = @"
UPDATE wms_bill_putoutheader SET STATE =2,FINISHTIME=sysdate() WHERE SOURCEORDER=@SOURCEORDER AND CONTAINER=@CONTAINER AND LOCATIONCODE=@LOCATIONCODE  AND  TARGET=@TARGET
";
                        int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                new SugarParameter("@SOURCEORDER",sourceOrder),
                                 new SugarParameter("@CONTAINER",container),
                                new SugarParameter("@LOCATIONCODE",startTaxi),
                                  new SugarParameter("@TARGET",endTaxi)
                });
                        //删除库存
                        sql = @"
DELETE FROM wms_bill_inventorybook  WHERE  BARCODE=@BARCODE  
";
                        dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {


                                 new SugarParameter("@BARCODE",container)

                });


                    }

                    // ===== 单针改造：出库完成，本盒针 RESERVED/AVAILABLE→OUTBOUND，记录出库口 END-taxi =====
                    NeedleService.MarkOutboundComplete(dbcontext.Db, container, sourceOrder, endTaxi, "WCS");

                    dbcontext.Db.Ado.CommitTran();

                    reslutJson = new ReslutJson()
                    {
                        success = true,
                        msg = "已确认出库已成功!",
                    };
                }
                catch (Exception e)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = e.Message,
                    };
                }
            }
            catch (Exception ex)
            {

                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteOperateLogF(string.Format("任务号:{0},原因:{1}", sourceOrder, warehouseCode + "|" + reslutJson.msg), "出库完成", ip);
            }

            return reslutJson;
        }


        /// <summary>
        /// 输送线任务
        /// </summary>
        /// <returns></returns>
        public ReslutJson transportTaskState(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string id = json["taskId"].ToString();
                string sql = @"
 UPDATE WMS_CONVEYOR_TASK SET STATE=2 WHERE ID=@ID
";
                int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                new SugarParameter("@ID",id),
            });
                if (a > 0)
                {
                    reslutJson = new ReslutJson()
                    {
                        success = true,
                        msg = "输送线任务完成",
                    };
                    LOGService.WriteOperateLogT(string.Format("任务号:{0},原因:{1}", id, reslutJson.msg), "输送线任务完成", ip);
                }
            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
            }
            return reslutJson;
        }

        /// <summary>
        /// agv任务
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson agvTaskState(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            try
            {
                string taskId = json["taskId"].ToString();
                int taskState = json["taskState"].ObjToInt();
                string sql = "";
                if (taskState == 100)
                {
                    sql = @"
  UPDATE WMS_AGV_TASK SET STATE=2 WHERE ID=@ID
";
                }
                else
                {
                    sql = @"
  UPDATE WMS_AGV_TASK SET STATE=3 WHERE ID=@ID
";
                }
                int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                new SugarParameter("@ID",taskId),
            });
                if (a > 0)
                {
                    reslutJson = new ReslutJson()
                    {
                        success = true,
                        msg = "AGV任务完成",
                    };
                    LOGService.WriteOperateLogT(string.Format("任务号:{0},原因:{1}", taskId, reslutJson.msg), "AGV任务完成", ip);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return reslutJson;
        }



        /// <summary>
        /// WMS->wcs下发堆垛机任务
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson A01_NewTask(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            //查询正在执行以及下发WCS任务数
            try
            {
                string warehouse = json["warehouse"].ToString();
                //string warehtunnelNum = json["tunnelNum"].ToString();
                reslutJson = A01_NewTask(warehouse, ip, "");
                if (reslutJson.success)
                {
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "WMS->wcs下发堆垛机任务");
                }
            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
                LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "WMS->wcs下发堆垛机任务");
                //LOGService.WriteExceptionLog(ex);
                //            throw;
            }

            return reslutJson;
        }


        /// <summary>
        /// 厚达->WMS申请AGV任务
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson GetAgvTask(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            //查询正在执行以及下发WCS任务数
            try
            {
                string warehouse = json["warehouse"].ToString();
                reslutJson = AGV_NewTask(warehouse, ip, 1);

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
                //LOGService.WriteExceptionLog(ex);
                //            throw;
            }

            return reslutJson;
        }

        /// <summary>
        /// WMS->wcs下发堆垛机任务
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson A02_NewTask(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            //查询正在执行以及下发WCS任务数
            try
            {
                string warehouse = json["warehouse"].ToString();
                reslutJson = A02_NewTask(warehouse, ip, 1);

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
                //LOGService.WriteExceptionLog(ex);
                //            throw;
            }

            return reslutJson;
        }
        public ReslutJson A02_NewTask(string warehouse, string ip, int tunnelNum)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {

                string sql = @"
				SELECT A.ID
				FROM wms_stocker_task  A   
				WHERE A.ZONECODE = @ZONECODE AND A.STATE=0  
				";
                var TaskCount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE",warehouse),
                                });
                if (TaskCount != null && TaskCount.Rows.Count > 0)
                {
                    //获取最早出库任务，优先级，创建时间
                    sql = @"
				SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
				FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 
				ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    var Count = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE",warehouse),
                                });
                    if (Count != null && Count.Rows.Count > 0)
                    {
                        string id = Count.Rows[0]["ID"].ObjToString();
                        string container = Count.Rows[0]["CONTAINER"].ObjToString();
                        string priority = Count.Rows[0]["PRIORITY"].ObjToString();
                        string startTaxi = Count.Rows[0]["STARTTAXI"].ObjToString();
                        string type = Count.Rows[0]["TYPE"].ObjToString();
                        string endTaxi = Count.Rows[0]["ENDTAXI"].ObjToString();
                        sql = @"
				UPDATE wms_stocker_task SET STATE=1,STARTTIME=sysdate()  WHERE ID=@ID AND ZONECODE=@ZONECODE AND STATE=0
				";
                        int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                        new SugarParameter("@ZONECODE",warehouse),
                                         new SugarParameter("@ID",id),
                                    });
                        //		sql = @"
                        //UPDATE WMS_BASE_LOCATION SET ISAVIALABLE=0 WHERE CODE=@CODE AND WAREHOUSECODE = @WAREHOUSECODE AND ISAVIALABLE=1
                        //";
                        //		int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                        //						new SugarParameter("@WAREHOUSECODE",warehouse),
                        //						 new SugarParameter("@CODE",startTaxi),
                        //});
                        if (a > 0)
                        {

                            //JObject jObject = new JObject() {
                            //				{"taskId",id},
                            //				{"taskType","SRM_OUT"},
                            //				{"placeFrom",startTaxi},
                            //				{"placeTo",endTaxi},
                            //				{"containerId",container},
                            //				{"priority",priority},
                            //				//{"doDevice",doDevice},
                            //				{"outTo",endTaxi},
                            //				{"c_01",""}
                            //			};
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "AGV请求成功!",
                                data = new Dictionary<string, string>()
                                {
                                                    {"taskId", id },
                                                    {"taskType", type },
                                                    {"prioRity", priority },
                                                    { "conTainer", container },
                                                    { "startTaxi", startTaxi },
                                                    { "endTaxi", endTaxi }
                                }
                            };
                            //var reslut = HttpRequestHelper.HttpPostMethod(ApiConfig.StockTaskUrl,jObject);
                            //reslutJson = new ReslutJson()
                            //{
                            //    success = true,
                            //    msg=reslut.msg,
                            //};
                            //LOGService.WriteOperateLogT(string.Format("任务号:{0},原因:{1}", id, reslut.msg), "WMS向WCS下发堆垛机任务", ip);
                        }
                    }
                    else
                    {

                        reslutJson = new ReslutJson()
                        {
                            success = false,
                            msg = "暂无可执行出库任务!"
                        };

                    }

                }
                else
                {

                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "暂无可执行出库任务!"
                    };

                }

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message
                };
            }
            return reslutJson;
        }
        /// <summary>
        /// AGV任务
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="ip"></param>
        /// <param name="tunnelNum"></param>
        /// <returns></returns>
        public ReslutJson AGV_NewTask(string warehouse, string ip, int tunnelNum)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {


                string sql = @"
				SELECT A.ID
				FROM wms_stocker_task  A   
				WHERE A.ZONECODE = @ZONECODE AND A.STATE=0 
				";
                var TaskCount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE",warehouse),
                                });
                if (TaskCount != null && TaskCount.Rows.Count > 0)
                {
                    //获取最早出库任务，优先级，创建时间
                    sql = @"
				SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
				FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 
				ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    var Count = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE",warehouse),
                                });
                    if (Count != null && Count.Rows.Count > 0)
                    {
                        string id = Count.Rows[0]["ID"].ObjToString();
                        string container = Count.Rows[0]["CONTAINER"].ObjToString();
                        string priority = Count.Rows[0]["PRIORITY"].ObjToString();
                        string startTaxi = Count.Rows[0]["STARTTAXI"].ObjToString();
                        string type = Count.Rows[0]["TYPE"].ObjToString();
                        string endTaxi = Count.Rows[0]["ENDTAXI"].ObjToString();
                        sql = @"
				UPDATE wms_stocker_task SET STATE=1,STARTTIME=sysdate()  WHERE ID=@ID AND ZONECODE=@ZONECODE AND STATE=0
				";
                        int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                        new SugarParameter("@ZONECODE",warehouse),
                                         new SugarParameter("@ID",id),
                                    });
                        //		sql = @"
                        //UPDATE WMS_BASE_LOCATION SET ISAVIALABLE=0 WHERE CODE=@CODE AND WAREHOUSECODE = @WAREHOUSECODE AND ISAVIALABLE=1
                        //";
                        //		int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                        //						new SugarParameter("@WAREHOUSECODE",warehouse),
                        //						 new SugarParameter("@CODE",startTaxi),
                        //});
                        if (a > 0)
                        {

                            //JObject jObject = new JObject() {
                            //				{"taskId",id},
                            //				{"taskType","SRM_OUT"},
                            //				{"placeFrom",startTaxi},
                            //				{"placeTo",endTaxi},
                            //				{"containerId",container},
                            //				{"priority",priority},
                            //				//{"doDevice",doDevice},
                            //				{"outTo",endTaxi},
                            //				{"c_01",""}
                            //			};
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "AGV任务请求成功!",
                                data = new Dictionary<string, string>()
                                {
                                                    {"taskId", id },
                                                    {"taskType", type },
                                                    {"prioRity", priority },
                                                    { "conTainer", container },
                                                    { "startTaxi", startTaxi },
                                                    { "endTaxi", endTaxi }
                                }
                            };
                            //var reslut = HttpRequestHelper.HttpPostMethod(ApiConfig.StockTaskUrl,jObject);
                            //reslutJson = new ReslutJson()
                            //{
                            //    success = true,
                            //    msg=reslut.msg,
                            //};
                            //LOGService.WriteOperateLogT(string.Format("任务号:{0},原因:{1}", id, reslut.msg), "WMS向WCS下发堆垛机任务", ip);
                        }
                    }
                    else
                    {

                        reslutJson = new ReslutJson()
                        {
                            success = false,
                            msg = "暂无可执行出入库任务!"
                        };

                    }

                }
                else
                {

                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "暂无可执行出入库任务!"
                    };

                }

            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message
                };
                LOGService.WriteOperateLogF(string.Format("原因:{0}", warehouse + "|" + reslutJson.msg), "获取AGV任务失败", ip);
            }
            return reslutJson;
        }


        public ReslutJson A01_NewTask(string warehouse, string ip, string tunnelNum)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {

                {
                    string sql = "";
                    if (warehouse == "1")
                    {
                        //获取最早出库任务，优先级，创建时间
                        sql = @"
					SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE,ORDERINFO
	                    FROM wms_stocker_task WHERE TYPE=2 AND STATE=0 AND (
                            ORDERINFO IS NULL
                            OR ORDERINFO = ''
                            OR TASKMODE = 'NORMAL'
                        ) AND LEFT(STARTTAXI,2) IN('01','02')
	                    ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    }
                    else if (warehouse == "2")
                    {
                        //获取最早出库任务，优先级，创建时间
                        sql = @"
					SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE,ORDERINFO
	FROM wms_stocker_task WHERE TYPE=2 AND (
        ORDERINFO IS NULL
        OR ORDERINFO = ''
        OR TASKMODE = 'NORMAL'
    )  AND STATE=0  AND LEFT(STARTTAXI,2) IN('03','04')
	ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    }
                    else if (warehouse == "3")
                    {
                        //获取最早出库任务，优先级，创建时间
                        sql = @"
					SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE,ORDERINFO
	FROM wms_stocker_task WHERE TYPE=2 AND (
        ORDERINFO IS NULL
        OR ORDERINFO = ''
        OR TASKMODE = 'NORMAL'
    )  AND STATE=0  AND LEFT(STARTTAXI,2) IN('05','06')
	ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    }
                    else
                    {
                        sql = @"
					SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE,ORDERINFO
	FROM wms_stocker_task WHERE TYPE=2 AND (
        ORDERINFO IS NULL
        OR ORDERINFO = ''
        OR TASKMODE = 'NORMAL'
    )  AND STATE=0  AND LEFT(STARTTAXI,2) IN('07','08')
	ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
                    }

                    //
                    var Count = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                    {

                    });
                    if (Count != null && Count.Rows.Count > 0)
                    {
                        string id = Count.Rows[0]["ID"].ObjToString();
                        string container = Count.Rows[0]["CONTAINER"].ObjToString();
                        string priority = Count.Rows[0]["PRIORITY"].ObjToString();
                        string startTaxi = Count.Rows[0]["STARTTAXI"].ObjToString();
                        string type = Count.Rows[0]["TYPE"].ObjToString();
                        string endTaxi = Count.Rows[0]["ENDTAXI"].ObjToString();
                        string orderInfo = Count.Rows[0]["ORDERINFO"].ObjToString();
                        sql = @"
				UPDATE wms_stocker_task SET STATE=1,STARTTIME=sysdate()  WHERE ID=@ID AND STATE=0
				";
                        int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                         new SugarParameter("@ID",id)
                                    });

                        if (a > 0)
                        {

                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS请求成功!",
                                data = new Dictionary<string, string>()
                                {
                                                    {"taskId", id },
                                                    {"taskType", type },
                                                    {"prioRity", priority },
                                                    { "conTainer", container },
                                                    { "startTaxi", startTaxi },
                                                    { "endTaxi", endTaxi },
                                                    { "orderInfo", orderInfo }


                                }
                            };

                        }
                    }
                    else
                    {

                        reslutJson = new ReslutJson()
                        {
                            success = false,
                            msg = "暂无可执行任务!"
                        };

                    }

                }


            }
            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message
                };
                LOGService.WriteOperateLogF(string.Format("原因:{0}", warehouse + "|" + reslutJson.msg), "获取堆垛机任务失败", ip);
            }
            return reslutJson;
        }

        /// <summary>
        /// WMS->WCS下发AGV任务
        /// </summary>
        /// <param name="warehouse"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson C01_NewAgvTask(string warehouse, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            return reslutJson;
        }

        /// <summary>
        /// WMS->WCS下发输送线任务
        /// </summary>
        /// <returns></returns>
        public ReslutJson B01_NewTransportTask(string sourceorder, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            string sql = @"
SELECT TOP 1 ID,BARCODE,ENDTAXI,QTY,BATCHQTY,CONTAINER
FROM WMS_CONVEYOR_TASK
WHERE SOURCEORDER = @SOURCEORDRE AND STATE=0
ORDER BY CREATETIME DESC
";
            var ConveryTask = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                   new SugarParameter("@SOURCEORDRE",sourceorder),
            });
            if (ConveryTask != null && ConveryTask.Rows.Count > 1)
            {
                string id = ConveryTask.Rows[0]["ID"].ObjToString();
                string barcode = ConveryTask.Rows[0]["BARCODE"].ObjToString();
                string endTaxi = ConveryTask.Rows[0]["ENDTAXI"].ObjToString();
                string qty = ConveryTask.Rows[0]["QTY"].ObjToString();
                string batchQty = ConveryTask.Rows[0]["BATCHQTY"].ObjToString();
                string container = ConveryTask.Rows[0]["CONTAINER"].ObjToString();
                JObject json = new JObject() {

                    { "taskId",id},
                    { "taskCode",id},
                    { "taskType",1},
                    { "placeFrom",1},
                    { "placeTo",endTaxi},
                    { "containerId",container},
                    { "stationId",barcode},
                    { "outTo",batchQty},
                    { "c_01",qty},
                };
                var reslut = HttpRequestHelper.HttpPostMethod(ApiConfig.ConveryTaskUrl, json);
                if (reslut.success == true)
                {
                    sql = @"
UPDATE WMS_CONVEYOR_TASK SET STATE=1 WHERE SOURCEORDER = @SOURCEORDER AND STATE=0
";
                    dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                        new SugarParameter("@SOURCEORDER",sourceorder)
                    });
                }
                reslutJson = new ReslutJson()
                {
                    success = true,
                    msg = reslut.msg,
                };

                LOGService.WriteOperateLogT(string.Format("任务号:{0},原因:{1}", id, reslut.msg), "下发输送线任务", ip);
            }
            return reslutJson;
        }

        /// <summary>
        /// 机架下线生成agv任务
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson robatConvery(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string ConveryID = json["ConveryID"].ToString();
                string boxId = json["boxId"].ToString();
                reslutJson = new ReslutJson()
                {
                    success = true,
                    code = "200",//标识正常
                    msg = "WCS向WMS请求AGV任务",
                };
                LOGService.WriteOperateLogT(string.Format("箱号:{0},设备:{1},原因:{2}", boxId, ConveryID, reslutJson.msg), "WCS向WMS请求AGV任务", ip);
            }
            catch (Exception)
            {

                throw;
            }
            return reslutJson;
        }

        #region  sap交互
        /// <summary>
        /// 出入库完成后向SAP报工
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        //		public ReslutSapJson finishstockertask( string ip)
        //		{
        //			ReslutSapJson reslutJson = new ReslutSapJson();
        //			//查询未报工的堆垛机任务

        //		string	 sql = @"
        //SELECT ID,SOURCEORDER,CONTAINER 
        //FROM wms_bill_putinbindingheader 
        //WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
        //			var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
        //								new SugarParameter("@SOURCEORDER",sourceOrder),
        //								 new SugarParameter("@CONTAINER",container)
        //				});
        //			string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
        //			var content = new
        //			{
        //				TASK_NO = sourceOrder,
        //				TASK_TYPE = "IN",
        //				LENUM = container,
        //				BOUND_SUCCESS = 1,
        //				MESSAGE = "入库成功"
        //			};
        //			reslutJson = JsonConvert.DeserializeObject<ReslutSapJson>(HttpRequestPost.HttpPostMethod(url, content).ToString());
        //			//reslutJson = new ReslutSapJson()
        //			//{
        //			//	SUCCESS = "0",
        //			//	MESSAGE = "失败:物料类型未识别"

        //			//};
        //			LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);
        //		}
        /// <summary>
        /// sap请求物料空盘或满盘入库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskinbound(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值
                string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                string TARGET = json["TARGET"].ToString();//目标地址
                                                          //空托盘入库和满盘入库
                                                          //1.空托盘入库  
                                                          //重复任务过滤，若已经存在该条码的入库任务则返回失败
                                                          //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                #region 解析库区 托盘类型										
                string target = "";

                if (LENUM.Substring(0, 1) == "1")
                {
                    target = "101-1";

                }
                else if (LENUM.Substring(0, 1) == "2")
                {
                    target = "201-1";
                }
                else if (LENUM.Substring(0, 1) == "3")
                {
                    target = "301-1";
                }
                else
                {
                    target = "";
                }
                //托盘类型
                string CONTAINERTYPE = "";

                string sn = LENUM.Substring(0, 2);
                if (sn == "21")
                {
                    CONTAINERTYPE = "0";
                }
                if (sn == "22")
                {
                    CONTAINERTYPE = "1";
                }
                if (sn == "23")
                {
                    CONTAINERTYPE = "3";
                }
                if (sn == "24")
                {
                    CONTAINERTYPE = "4";
                }
                if (sn == "25")
                {
                    CONTAINERTYPE = "2";
                }
                if (sn == "11")
                {
                    CONTAINERTYPE = "0";
                }
                if (sn == "31")
                {
                    CONTAINERTYPE = "0";
                }
                if (sn == "32")
                {
                    CONTAINERTYPE = "1";
                }


                #endregion
                //校验托盘是否在库中存在
                string gsql = @"
SELECT CONTAINER  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE    
";
                var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });
                if (glist != null && glist.Rows.Count > 0)
                {
                    reslutJson = new ReslutSapJson()
                    {
                        SUCCESS = "0",
                        MESSAGE = "失败:当前托盘已经在库中存在，请联系SAP人员！"

                    };
                    LOGService.WriteOperateLogF(string.Format("任务号:{0} 当前托盘已经在库中存在，入库失败", TASK_NO), "空盘或满盘入库", ip);
                }
                else
                {
                    //校验入库信息是否重复
                    string msql = @"
SELECT SOURCEORDER,CONTAINER  FROM wms_bill_putinbindingheader WHERE CONTAINER=@CONTAINER  and TARGET=@ZONECODE   and STATE<2  
";
                    var mlist = dbcontext.Db.Ado.GetDataTable(msql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });

                    if (mlist != null && mlist.Rows.Count > 0)
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:当前托盘已经生成入库在执行，请勿重复操作！"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0} 重复扫描入库，入库失败", TASK_NO + "|" + target), "空盘或满盘入库", ip);
                    }
                    else
                    {
                        //
                        if (IS_EMPTY == "1")
                        {
                            //生成入库任务

                            string id = Guid.NewGuid().ToString("D");
                            //string id = TASK_NO;
                            #region  
                            //获取库区
                            //string target = "";

                            //if (MA_TYPE == "101")
                            //{
                            //	target = "101-1";

                            //}
                            //else if (MA_TYPE == "301")
                            //{
                            //	target = "301-1";
                            //}
                            //else if (MA_TYPE == "201" || MA_TYPE == "202"|| MA_TYPE == "203")
                            //{
                            //	target = "201-1";
                            //}
                            //else
                            //{
                            //	target = "";
                            //}
                            ////托盘类型
                            //string CONTAINERTYPE = "";
                            //if (MA_TYPE == "101")
                            //{

                            //	CONTAINERTYPE = "0";
                            //}
                            //else if (MA_TYPE == "301")
                            //{
                            //	CONTAINERTYPE = IS_LARGE;
                            //}
                            //else if (MA_TYPE == "201")
                            //{
                            //	CONTAINERTYPE = "0";
                            //}
                            //else if (MA_TYPE == "202")
                            //{
                            //	CONTAINERTYPE = "1";
                            //}
                            //else if (MA_TYPE == "203")
                            //{
                            //	CONTAINERTYPE = "2";
                            //}
                            //else
                            //{
                            //	CONTAINERTYPE = "";
                            //}
                            #endregion
                            if (target != "")
                            {
                                dbcontext.Db.Ado.BeginTran();
                                string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'空托盘入库','SAP',sysdate()) 
";
                                dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),

                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });
                                dbcontext.Db.Ado.CommitTran();
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "成功"

                                };
                                LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO + "|" + target), "空盘或满盘入库", ip);
                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:物料类型未识别"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO + "|" + target), "空盘或满盘入库", ip);
                            }
                        }
                        else   //满盘入库或回库
                        {
                            //生成入库任务

                            string id = Guid.NewGuid().ToString("D");
                            //string id = TASK_NO;

                            if (target != "")
                            {

                                //半成品和成品库时
                                if (target != "")
                                {
                                    dbcontext.Db.Ado.BeginTran();
                                    string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER, CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'入库','SAP',sysdate()) 
";
                                    dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });
                                    //插入物料明细
                                    JToken data = json["DATA"];//list列表
                                    for (int i = 0; i < data.Count(); i++)
                                    {
                                        string lid = Guid.NewGuid().ToString("D");
                                        string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                        string CHARG = data[i]["CHARG"].ToString();
                                        string MATNR = data[i]["MATNR"].ToString();
                                        string WEIGHT = data[i]["WEIGHT"].ToString();

                                        string listsql = @"
INSERT INTO wms_bill_putinbindingbody (ID,HEADERID,ITEMCODE, LOT,WEIGHT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@WEIGHT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
                        new SugarParameter("@WEIGHT",WEIGHT),
                        new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                    }

                                    dbcontext.Db.Ado.CommitTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO + "|" + target), "空盘或满盘入库", ip);


                                }
                                else//原材料库
                                {
                                    //暂不调试
                                }

                            }
                            else
                            {
                                //dbcontext.Db.Ado.RollbackTran();
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:物料类型未识别"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO + "|" + target), "空盘或满盘入库", ip);
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteOperateLogF(ex.ToString(), "空盘或满盘入库", ip);
            }
            return reslutJson;
        }

        /// <summary>
        /// sap请求物料空盘或满盘入库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskinboundtest(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值
                string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                string TARGET = json["TARGET"].ToString();//目标地址
                                                          //空托盘入库和满盘入库
                                                          //1.空托盘入库  
                                                          //重复任务过滤，若已经存在该条码的入库任务则返回失败
                                                          //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                                                          //获取库区
                string target = "";

                if (MA_TYPE == "101")
                {
                    target = "101-1";

                }
                else if (MA_TYPE == "301")
                {
                    target = "301-1";
                }
                else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203")
                {
                    target = "201-1";
                }
                //else if (MA_TYPE == "401" )
                //{
                //	target = "401-1";
                //}
                else
                {
                    target = "";
                }
                //托盘类型
                string CONTAINERTYPE = "";
                if (MA_TYPE == "101")
                {

                    CONTAINERTYPE = "0";
                }
                else if (MA_TYPE == "301")
                {
                    CONTAINERTYPE = IS_LARGE;
                }
                //else if (MA_TYPE == "401")
                //{
                //	CONTAINERTYPE = "0";
                //}
                //else if (MA_TYPE == "201")
                //{
                //	CONTAINERTYPE = "0";
                //}
                //else if (MA_TYPE == "202")
                //{
                //	CONTAINERTYPE = "1";
                //}
                //else if (MA_TYPE == "203")
                //{
                //	CONTAINERTYPE = "2";
                //}
                else
                {
                    string sn = LENUM.Substring(0, 2);
                    if (sn == "21")
                    {
                        CONTAINERTYPE = "1";
                    }
                    if (sn == "22")
                    {
                        CONTAINERTYPE = "0";
                    }
                    if (sn == "23")
                    {
                        CONTAINERTYPE = "2";
                    }
                }

                //校验托盘是否在库中存在
                string gsql = @"
SELECT CONTAINER  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE    
";
                var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });
                if (glist != null && glist.Rows.Count > 0)
                {
                    reslutJson = new ReslutSapJson()
                    {
                        SUCCESS = "0",
                        MESSAGE = "失败:当前托盘已经在库中存在，请联系SAP人员！"

                    };
                    LOGService.WriteOperateLogF(string.Format("任务号:{0} 当前托盘已经在库中存在，入库失败", TASK_NO), "空盘或满盘入库", ip);
                }
                else
                {
                    //校验入库信息是否重复
                    string msql = @"
SELECT SOURCEORDER,CONTAINER  FROM wms_bill_putinbindingheader WHERE CONTAINER=@CONTAINER  and TARGET=@ZONECODE   and STATE<2  
";
                    var mlist = dbcontext.Db.Ado.GetDataTable(msql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });

                    if (mlist != null && mlist.Rows.Count > 0)
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:当前托盘已经生成入库在执行，请勿重复操作！"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0} 重复扫描入库，入库失败", TASK_NO), "空盘或满盘入库", ip);
                    }
                    else
                    {
                        //
                        if (IS_EMPTY == "1")
                        {
                            //生成入库任务

                            string id = Guid.NewGuid().ToString("D");
                            //string id = TASK_NO;
                            #region  
                            //获取库区
                            //string target = "";

                            //if (MA_TYPE == "101")
                            //{
                            //	target = "101-1";

                            //}
                            //else if (MA_TYPE == "301")
                            //{
                            //	target = "301-1";
                            //}
                            //else if (MA_TYPE == "201" || MA_TYPE == "202"|| MA_TYPE == "203")
                            //{
                            //	target = "201-1";
                            //}
                            //else
                            //{
                            //	target = "";
                            //}
                            ////托盘类型
                            //string CONTAINERTYPE = "";
                            //if (MA_TYPE == "101")
                            //{

                            //	CONTAINERTYPE = "0";
                            //}
                            //else if (MA_TYPE == "301")
                            //{
                            //	CONTAINERTYPE = IS_LARGE;
                            //}
                            //else if (MA_TYPE == "201")
                            //{
                            //	CONTAINERTYPE = "0";
                            //}
                            //else if (MA_TYPE == "202")
                            //{
                            //	CONTAINERTYPE = "1";
                            //}
                            //else if (MA_TYPE == "203")
                            //{
                            //	CONTAINERTYPE = "2";
                            //}
                            //else
                            //{
                            //	CONTAINERTYPE = "";
                            //}
                            #endregion
                            if (target != "")
                            {
                                dbcontext.Db.Ado.BeginTran();
                                string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'空托盘入库','SAP',sysdate()) 
";
                                dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),

                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });
                                dbcontext.Db.Ado.CommitTran();
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "成功"

                                };
                                LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);
                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:物料类型未识别"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);
                            }
                        }
                        else   //满盘入库或回库
                        {
                            //生成入库任务

                            string id = Guid.NewGuid().ToString("D");
                            //string id = TASK_NO;

                            if (target != "")
                            {

                                //半成品和成品库时
                                if (target != "")
                                {
                                    dbcontext.Db.Ado.BeginTran();
                                    string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER, CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'入库','SAP',sysdate()) 
";
                                    dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });
                                    //插入物料明细
                                    JToken data = json["DATA"];//list列表
                                    for (int i = 0; i < data.Count(); i++)
                                    {
                                        string lid = Guid.NewGuid().ToString("D");
                                        string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                        string CHARG = data[i]["CHARG"].ToString();
                                        string MATNR = data[i]["MATNR"].ToString();
                                        string WEIGHT = data[i]["WEIGHT"].ToString();

                                        string listsql = @"
INSERT INTO wms_bill_putinbindingbody (ID,HEADERID,ITEMCODE, LOT,WEIGHT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@WEIGHT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
                        new SugarParameter("@WEIGHT",WEIGHT),
                        new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                    }

                                    dbcontext.Db.Ado.CommitTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);


                                }
                                else//原材料库
                                {
                                    //暂不调试
                                }

                            }
                            else
                            {
                                //dbcontext.Db.Ado.RollbackTran();
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:物料类型未识别"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteOperateLogF(ex.ToString(), "空盘或满盘入库", ip);
            }
            return reslutJson;
        }
        /// <summary>
        /// 冷藏库sap请求物料空盘或满盘入库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskinboundforcold(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                string startcode = json["OTARGET"].ToString();//起始点位
                string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值  9.99为直接到出库口
                string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                string TARGET = json["TARGET"].ToString();//目标地址
                                                          //空托盘入库和满盘入库
                                                          //1.空托盘入库  
                                                          //重复任务过滤，若已经存在该条码的入库任务则返回失败
                                                          //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                                                          //获取库区
                string target = "401-1";
                //托盘类型
                string CONTAINERTYPE = "0";
                //1.先查询是否能申请到空库位 2.再生成入库单 更新对应信息
                if (MA_TYPE == "401")
                {
                    CONTAINERTYPE = "0";
                }
                else
                {
                    CONTAINERTYPE = "1";
                }
                string sql = "";
                //获取仓库配置

                sql = @"SELECT  A.ENTPRINCIPLE,A.OUTPRINCIPLE,B.ROWNUM,B.COLUMNNUM,B.SIGINNUM ,B.LAYERNUM   FROM wms_base_warehouse A   JOIN wms_base_zone B  ON  A.WAREHOUSECODE=B.WAREHOUSECODE WHERE A.ENABLED = 1 AND B.ENABLED = 1  AND B.ZONECODE = @ZONECODE  LIMIT 1";
                var warehouse = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                    {
                          new SugarParameter("@ZONECODE",target)
                    });
                if (warehouse != null && warehouse.Rows.Count > 0)
                {
                    var ENTPRINCIPLE = warehouse.Rows[0]["ENTPRINCIPLE"].ObjToInt();
                    var ROWNUM = warehouse.Rows[0]["ROWNUM"].ObjToInt();
                    var COLUMNNUM = warehouse.Rows[0]["COLUMNNUM"].ObjToInt();
                    var SIGINNUM = warehouse.Rows[0]["SIGINNUM"].ObjToInt();
                    var LAYERNUM = warehouse.Rows[0]["LAYERNUM"].ObjToInt();
                    if (SIGINNUM.ObjToInt() == 0)//单伸位
                    {
                        if (ENTPRINCIPLE == 4)
                        {

                            //分配库位，生成agv任务
                            if (TARGET == "0")//入冷藏库
                            {
                                //根据卷数查询分配库位  
                                string code = "";

                                try
                                {
                                    //更新库位信息   入库信息   生成堆垛机任务  
                                    dbcontext.Db.Ado.BeginTran();
                                    string id = Guid.NewGuid().ToString("D");
                                    int c = 0;

                                    //冷藏库的库位分配逻辑：
                                    //	第一，有料木托盘的数据都只放一层；(目前放一层，后续可放一二层)
                                    //	第二，铁的空托盘数据只放二层和三层。
                                    //	第三，有料铁托盘数据只放二层和三层，同时要区分批次个数，
                                    //当小于等于7个批次时优先放三层，当大于7个批次时只放二层。
                                    //（备注：当有料木托不够位置放置时，联系冷藏库系统管理员冻结二层的库位状态给予仓库员工周转使用）
                                    if (IS_EMPTY == "1")
                                    {

                                        //铁托盘时
                                        if (CONTAINERTYPE == "0")
                                        {
                                            sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1  AND ENABLED=1 AND ROWNUM=2
														ORDER BY LAYERNUM ASC,COLUMNNUM ASC, ROWNUM ASC LIMIT 1
														";

                                            var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

                                              new SugarParameter("@ZONECODE",target)
                                         });
                                            if (location != null && location.Rows.Count > 0)
                                            {
                                                code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                                            }
                                            else
                                            {
                                                sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1  AND ENABLED=1 AND ROWNUM=1 AND LAYERNUM>=2
														ORDER BY LAYERNUM ASC,COLUMNNUM ASC, ROWNUM ASC LIMIT 1
														";

                                                var seclocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                            new SugarParameter("@COLUMNNUM",COLUMNNUM),
                                              new SugarParameter("@ZONECODE",target)
                                         });
                                                if (seclocation != null && seclocation.Rows.Count > 0)
                                                {
                                                    code = seclocation.Rows[0]["LOCATIONCODE"].ObjToString();
                                                }
                                            }
                                        }
                                        else//木托盘时
                                        {
                                            sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1 AND ROWNUM=@ROWNUM AND ENABLED=1 AND LAYERNUM=1
														ORDER BY LAYERNUM ASC,COLUMNNUM ASC, ROWNUM DESC LIMIT 1
														";

                                            var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                              new SugarParameter("@ZONECODE",target)
                                         });
                                            if (location != null && location.Rows.Count > 0)
                                            {
                                                code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                                            }

                                        }


                                        if (code != "")
                                        {
                                            string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER, CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'入库','SAP',sysdate()) 
";
                                            c = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });

                                            sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                            new SugarParameter("@REMARK",LENUM),
                                            new SugarParameter("@ZONECODE",target),
                                            new SugarParameter("@LOCATIONCODE",code)
                                        });
                                            sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
        ;
                                            int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                                   new SugarParameter("@LOCATIONCODE",code),
                                                    new SugarParameter("@ID",id)

                                                });

                                            dbcontext.Db.Ado.CommitTran();
                                            if (a > 0 && b > 0 && c > 0)
                                            {
                                                //打包区1202  1201 不生成
                                                string ids = Guid.NewGuid().ToString("D");
                                                wms_stocker_task wmsStocker = new wms_stocker_task();
                                                wmsStocker.ID = ids;
                                                wmsStocker.CODE = ids;
                                                wmsStocker.ENDTAXI = code;
                                                wmsStocker.CONTAINER = LENUM;
                                                wmsStocker.STARTTAXI = startcode;//起始点

                                                wmsStocker.TYPE = 1;
                                                wmsStocker.STARTTIME = DateTime.Now;
                                                wmsStocker.ZONECODE = target;
                                                wmsStocker.SOURCEORDER = TASK_NO;
                                                wmsStocker.PRIORITY = 0;
                                                wmsStocker.STATE = 0;
                                                WmsStockerTaskDao.insert(wmsStocker);
                                                //LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, code), "立库请求库位", ip);

                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "1",
                                                    MESSAGE = "生成AGV任务成功!"

                                                };
                                            }
                                            else
                                            {
                                                dbcontext.Db.Ado.RollbackTran();
                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "0",
                                                    MESSAGE = "wms数据执行错误!"

                                                };
                                            }
                                        }
                                        else
                                        {
                                            string msg = "";

                                            msg = "失败：立库无可用库位！";

                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "0",
                                                MESSAGE = msg

                                            };
                                        }
                                    }
                                    else
                                    {
                                        //根据卷数分配库位
                                        JToken jsdata = json["DATA"];//list列表
                                        int jscount = jsdata.Count();
                                        //木托盘无论多少卷均放一层
                                        if (CONTAINERTYPE == "1")
                                        {

                                            sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1 AND ROWNUM=@ROWNUM AND ENABLED=1 AND LAYERNUM=1
														ORDER BY LAYERNUM ASC,COLUMNNUM ASC, ROWNUM DESC LIMIT 1
														";

                                            var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                                 new SugarParameter("@ROWNUM",1*1),
                                            new SugarParameter("@COLUMNNUM",COLUMNNUM),
                                              new SugarParameter("@ZONECODE",target)
                                         });
                                            if (location != null && location.Rows.Count > 0)
                                            {
                                                code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                                            }
                                        }
                                        else//铁托盘时
                                        {

                                            if (jscount > 7)//大于7卷放2层库位
                                            {
                                                //查询2层可用库位  
                                                sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1 AND ROWNUM=@ROWNUM AND ENABLED=1 AND LAYERNUM=2
														ORDER BY LAYERNUM ASC,COLUMNNUM ASC, ROWNUM DESC LIMIT 1
														";

                                                var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                                 new SugarParameter("@ROWNUM",1*1),
                                            new SugarParameter("@COLUMNNUM",COLUMNNUM),
                                              new SugarParameter("@ZONECODE",target)
                                         });
                                                if (location != null && location.Rows.Count > 0)
                                                {
                                                    code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                                                }


                                            }
                                            else
                                            {
                                                ////查询23层可用库位  优先放2层
                                                sql = @"
														SELECT LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM  FROM wms_base_location
														WHERE ZONECODE=@ZONECODE AND TYPE='ST' AND ISAVIALABLE=1 AND ROWNUM=@ROWNUM AND ENABLED=1 AND LAYERNUM>=2
														ORDER BY LAYERNUM DESC,COLUMNNUM ASC, ROWNUM DESC LIMIT 1
														";

                                                var location = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                                 new SugarParameter("@ROWNUM",1*1),
                                            new SugarParameter("@COLUMNNUM",COLUMNNUM),
                                              new SugarParameter("@ZONECODE",target)
                                         });
                                                if (location != null && location.Rows.Count > 0)
                                                {
                                                    code = location.Rows[0]["LOCATIONCODE"].ObjToString();
                                                }

                                            }
                                        }

                                        if (code != "")
                                        {
                                            string insertsql = @"
INSERT INTO wms_bill_putinbindingheader (ID,SOURCEORDER, CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @CONTAINER,@CONTAINERTYPE,0,0,@TARGET,1,'入库','SAP',sysdate()) 
";
                                            c = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@TARGET",target)

                    });
                                            //插入物料明细
                                            JToken data = json["DATA"];//list列表
                                            for (int i = 0; i < data.Count(); i++)
                                            {
                                                string lid = Guid.NewGuid().ToString("D");
                                                string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                                string CHARG = data[i]["CHARG"].ToString();
                                                string MATNR = data[i]["MATNR"].ToString();
                                                string WEIGHT = data[i]["WEIGHT"].ToString();

                                                string listsql = @"
INSERT INTO wms_bill_putinbindingbody (ID,HEADERID,ITEMCODE, LOT,WEIGHT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@WEIGHT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                                dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
                        new SugarParameter("@WEIGHT",WEIGHT),
                        new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                            }

                                            sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                            new SugarParameter("@REMARK",LENUM),
                                            new SugarParameter("@ZONECODE",target),
                                            new SugarParameter("@LOCATIONCODE",code)
                                        });
                                            sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
        ;
                                            int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                                   new SugarParameter("@LOCATIONCODE",code),
                                                    new SugarParameter("@ID",id)

                                                });

                                            dbcontext.Db.Ado.CommitTran();
                                            if (a >= 0 && b >= 0 && c >= 0)
                                            {
                                                string ids = Guid.NewGuid().ToString("D");
                                                wms_stocker_task wmsStocker = new wms_stocker_task();
                                                wmsStocker.ID = ids;
                                                wmsStocker.CODE = ids;
                                                wmsStocker.ENDTAXI = code;
                                                wmsStocker.CONTAINER = LENUM;
                                                wmsStocker.STARTTAXI = startcode;//起始点

                                                wmsStocker.TYPE = 1;
                                                wmsStocker.STARTTIME = DateTime.Now;
                                                wmsStocker.ZONECODE = target;
                                                wmsStocker.SOURCEORDER = TASK_NO;
                                                wmsStocker.PRIORITY = 0;
                                                wmsStocker.STATE = 0;
                                                WmsStockerTaskDao.insert(wmsStocker);
                                                //LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, code), "立库请求库位", ip);

                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "1",
                                                    MESSAGE = "生成AGV任务成功!"

                                                };
                                            }
                                            else
                                            {
                                                dbcontext.Db.Ado.RollbackTran();
                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "0",
                                                    MESSAGE = "wms数据执行错误!"

                                                };
                                            }
                                        }
                                        else
                                        {
                                            string msg = "";
                                            if (jscount > 7)
                                            {
                                                msg = "失败：立库1、2层无可用库位！";
                                            }
                                            else
                                            {
                                                msg = "失败：立库3层无可用库位！";
                                            }
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "0",
                                                MESSAGE = msg

                                            };
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    dbcontext.Db.Ado.RollbackTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "0",
                                        MESSAGE = ex.Message.ToString()

                                    };
                                    //LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
                                }

                            }
                            else
                            {
                                //只生成AGV任务

                                string ids = Guid.NewGuid().ToString("D");
                                wms_stocker_task wmsStocker = new wms_stocker_task();
                                wmsStocker.ID = ids;
                                wmsStocker.CODE = ids;
                                wmsStocker.ENDTAXI = TARGET;
                                wmsStocker.CONTAINER = LENUM;
                                wmsStocker.STARTTAXI = startcode;//起始点

                                wmsStocker.TYPE = 1;
                                wmsStocker.STARTTIME = DateTime.Now;
                                wmsStocker.ZONECODE = target;
                                wmsStocker.SOURCEORDER = TASK_NO;
                                wmsStocker.PRIORITY = 0;
                                wmsStocker.STATE = 0;
                                WmsStockerTaskDao.insert(wmsStocker);

                                //LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, code), "立库请求库位", ip);

                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "生成AGV任务成功!"

                                };
                            }


                        }
                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败：立库配置不对"

                        };
                        //LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, reslutJson.msg), "托盘立库请求库位", ip);
                    }

                }
                else
                {
                    reslutJson = new ReslutSapJson()
                    {
                        SUCCESS = "0",
                        MESSAGE = "失败：立库配置不对"

                    };
                    //LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, reslutJson.msg), "托盘立库请求库位", ip);
                }

            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                //LOGService.WriteExceptionLog(ex);
            }
            return reslutJson;
        }
        /// <summary>
        /// sap请求物料空盘或满盘出库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskoutbound(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString().Trim();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                                                            //string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                                                              //string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值
                                                              //string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                                                              //string TARGET = json["TARGET"].ToString();//目标地址
                                                              // 提取List数据
                                                              //JArray hobbiesArray = (JArray)json["BARCODE"];
                                                              //List<string> hobbies = new List<string>();
                                                              //foreach (JToken hobby in hobbiesArray)
                                                              //{
                                                              //	hobbies.Add(hobby.ToString());
                                                              //}											  //空托盘入库和满盘出库
                                                              //1.空托盘出库  
                if (IS_EMPTY != "1")
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301" || MA_TYPE == "302")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203" || MA_TYPE == "204" || MA_TYPE == "205")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }
                    if (target != "")
                    {
                        //
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表

                        if (data.Count() > 0)
                        {

                            string LENUM = data[0]["LENUM"].ToString();
                            string TARGET = data[0]["TARGET"].ToString();
                            string IS_LARGE = data[0]["IS_LARGE"].ToString();
                            //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                            string msql = @"
SELECT SOURCEORDER,CONTAINER  FROM wms_stocker_task WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE and TYPE=2  and STATE<2  
";
                            var mlist = dbcontext.Db.Ado.GetDataTable(msql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });
                            if (mlist != null && mlist.Rows.Count > 0)
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:当前托盘正在出库，请勿重复下架！"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0} 重复下架，下架失败", TASK_NO), "空盘或满盘出库", ip);
                            }
                            else
                            {
                                string gsql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,LOCATIONCODE  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE
";
                                var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });

                                if (glist != null && glist.Rows.Count > 0)
                                {   //获取出库起点
                                    string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                                    string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                                    string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                                    #region   作废
                                    //比对出库信息与库存信息是否一致
                                    //								int inventcount = 0;
                                    //								for (int i = 0; i < data.Count(); i++)
                                    //								{

                                    //									string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                    //									string CHARG = data[i]["CHARG"].ToString();
                                    //									string MATNR = data[i]["MATNR"].ToString();
                                    //									string sql = @"
                                    //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  and BARCODE=@BARCODE   and ITEMCODE=@ITEMCODE  and LOT=@LOT
                                    //";
                                    //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                    //					{

                                    //						new SugarParameter("@SOURCEORDER",SOURCEORDER),
                                    //						new SugarParameter("@BARCODE",TASK_NO_SN),
                                    //						new SugarParameter("@ITEMCODE",MATNR),
                                    //						new SugarParameter("@LOT",CHARG)

                                    //					});
                                    //									if (list.Rows.Count > 0)
                                    //										inventcount++;
                                    //								}

                                    //								if (inventcount == data.Count())//库存信息一致
                                    //{
                                    #endregion
                                    #region  执行出库   1.插入入库单和入库明细 2.生成堆垛机出库任务

                                    if (ZONECODE == "201-1")
                                    {

                                        int start = int.Parse(LOCATIONCODE.Substring(2, 2));
                                        int end = int.Parse(GetLoclation(TARGET).Substring(2, 2));
                                        if (((start >= 28 & end >= 28) || (start <= 27 & end <= 27)))//非周转任务时
                                        {
                                            dbcontext.Db.Ado.BeginTran();
                                            string id = Guid.NewGuid().ToString("D");
                                            string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                            for (int i = 0; i < data.Count(); i++)
                                            {
                                                string lid = Guid.NewGuid().ToString("D");
                                                string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                                string CHARG = data[i]["CHARG"].ToString();
                                                string MATNR = data[i]["MATNR"].ToString();
                                                //string WEIGHT = data[i]["WEIGHT"].ToString();

                                                string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                                dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                            }

                                            //同区不需要周转时
                                            string taskid = Guid.NewGuid().ToString("D");
                                            string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                            dbcontext.Db.Ado.CommitTran();
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "1",
                                                MESSAGE = "成功"

                                            };
                                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                        }
                                        else
                                        {
                                            //1库位用完申请前查看是否有未完成的周转任务
                                            string sssql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
                                            var scount = dbcontext.Db.Ado.GetDataTable(sssql, new List<SugarParameter>()
                                        {
                                              new SugarParameter("@ZONECODE",ZONECODE)
                                        });
                                            if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "1")
                                            {
                                                dbcontext.Db.Ado.BeginTran();

                                                string id = Guid.NewGuid().ToString("D");
                                                string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                                int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                                for (int i = 0; i < data.Count(); i++)
                                                {
                                                    string lid = Guid.NewGuid().ToString("D");
                                                    string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                                    string CHARG = data[i]["CHARG"].ToString();
                                                    string MATNR = data[i]["MATNR"].ToString();
                                                    //string WEIGHT = data[i]["WEIGHT"].ToString();

                                                    string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                                    dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                                }

                                                //同区需要周转时
                                                string taskid = Guid.NewGuid().ToString("D");
                                                string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                                dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI","022704"),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });
                                                //更新周转库位为使用状态
                                                string ksql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
                                                dbcontext.Db.Ado.ExecuteCommand(ksql, new List<SugarParameter>() {

                                        new SugarParameter("@LOCATIONCODE","022704"),
                                        new SugarParameter("@ZONECODE",ZONECODE)

                                    });

                                                dbcontext.Db.Ado.CommitTran();
                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "1",
                                                    MESSAGE = "成功"

                                                };
                                                LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                            }
                                            else
                                            {
                                                reslutJson = new ReslutSapJson()
                                                {
                                                    SUCCESS = "0",
                                                    MESSAGE = "失败:当前有周转任务正在执行请稍候尝试！"

                                                };
                                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                            }

                                        }


                                    }
                                    else if (ZONECODE == "301-1")
                                    {
                                        //判定出库任务是否特殊站台
                                        if (LENUM.Substring(0, 2) == "32" & TARGET == "3007")
                                        {
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "0",
                                                MESSAGE = "失败:当前出库口不可出大托盘"

                                            };
                                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                        }
                                        else
                                        {
                                            dbcontext.Db.Ado.BeginTran();
                                            string id = Guid.NewGuid().ToString("D");
                                            string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                            for (int i = 0; i < data.Count(); i++)
                                            {
                                                string lid = Guid.NewGuid().ToString("D");
                                                string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                                string CHARG = data[i]["CHARG"].ToString();
                                                string MATNR = data[i]["MATNR"].ToString();
                                                //string WEIGHT = data[i]["WEIGHT"].ToString();

                                                string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                                dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                            }

                                            //同区不需要周转时
                                            string taskid = Guid.NewGuid().ToString("D");
                                            string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                            dbcontext.Db.Ado.CommitTran();
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "1",
                                                MESSAGE = "成功"

                                            };
                                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);


                                        }



                                    }
                                    else
                                    {


                                        dbcontext.Db.Ado.BeginTran();
                                        string id = Guid.NewGuid().ToString("D");
                                        string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                        int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                        for (int i = 0; i < data.Count(); i++)
                                        {
                                            string lid = Guid.NewGuid().ToString("D");
                                            string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                            string CHARG = data[i]["CHARG"].ToString();
                                            string MATNR = data[i]["MATNR"].ToString();
                                            //string WEIGHT = data[i]["WEIGHT"].ToString();

                                            string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                        }

                                        //同区不需要周转时
                                        string taskid = Guid.NewGuid().ToString("D");
                                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                        dbcontext.Db.Ado.CommitTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);



                                    }
                                    #endregion


                                }
                                else
                                {
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "0",
                                        MESSAGE = "失败:当前库存未查询到该托盘信息"

                                    };
                                    LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                }
                            }


                        }
                        else
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:未获取到托盘列表信息"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                        }

                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }
                else
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301" || MA_TYPE == "302")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203" || MA_TYPE == "204" || MA_TYPE == "205")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }



                    if (target != "")
                    {
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表

                        if (data.Count() > 0)
                        {
                            string LENUM = data[0]["LENUM"].ToString();//空
                            string TARGET = data[0]["TARGET"].ToString();//有
                            string IS_LARGE = data[0]["IS_LARGE"].ToString();//有
                                                                             //托盘类型
                            string CONTAINERTYPE = "";
                            if (MA_TYPE == "101")
                            {
                                CONTAINERTYPE = "0";
                            }
                            else if (MA_TYPE == "302")
                            {
                                CONTAINERTYPE = "1";
                            }
                            else if (MA_TYPE == "301")
                            {
                                CONTAINERTYPE = "0";
                            }
                            else if (MA_TYPE == "201")
                            {
                                CONTAINERTYPE = "0";
                            }
                            else if (MA_TYPE == "202")
                            {
                                CONTAINERTYPE = "3";
                            }
                            else if (MA_TYPE == "203")
                            {
                                CONTAINERTYPE = "2";
                            }
                            else if (MA_TYPE == "204")
                            {
                                CONTAINERTYPE = "4";
                            }
                            else if (MA_TYPE == "205")
                            {
                                CONTAINERTYPE = "1";
                            }

                            else
                            {
                                CONTAINERTYPE = "";
                            }
                            //获取空托盘信息
                            string gsql = @"
SELECT a.SOURCEORDER,a.CONTAINER,a.ZONECODE,a.LOCATIONCODE  FROM wms_base_location  a    WHERE a.ZONECODE=@ZONECODE   AND  ''!=a.CONTAINER  AND a.CONTAINERTYPE=@CONTAINERTYPE 
AND  CONTAINER  NOT IN(SELECT  CONTAINER  from wms_stocker_task  where ZONECODE=@ZONECODE and  TYPE=2 and  STATE<=1)
AND  (SELECT  COUNT(*)  from  wms_bill_inventorybook  b  where b.SOURCEORDER=a.SOURCEORDER  and  b.BARCODE=a.CONTAINER)=0  LIMIT 1 
";
                            var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@ZONECODE",target)

                    });
                            //有空托盘信息时
                            if (glist != null && glist.Rows.Count > 0)
                            {   //获取出库起点
                                string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                                string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                                string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                                string CONTAINER = glist.Rows[0]["CONTAINER"].ToString();


                                string TASK_NO_SN = data[0]["TASK_NO_SN"].ToString();
                                string CHARG = data[0]["CHARG"].ToString();
                                string MATNR = data[0]["MATNR"].ToString();
                                //									string sql = @"
                                //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  
                                //";
                                //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                //					{

                                //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

                                //					});



                                #region  执行出库   1.插入出库单 2.生成堆垛机出库任务

                                if (ZONECODE == "201-1")
                                {

                                    int start = int.Parse(LOCATIONCODE.Substring(2, 2));
                                    int end = int.Parse(GetLoclation(TARGET).Substring(2, 2));
                                    if (((start >= 28 & end >= 28) || (start <= 27 & end <= 27)))//非周转任务时
                                    {
                                        dbcontext.Db.Ado.BeginTran();
                                        string id = Guid.NewGuid().ToString("D");
                                        string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                        int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                        string taskid = Guid.NewGuid().ToString("D");
                                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                        dbcontext.Db.Ado.CommitTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    }
                                    else
                                    {
                                        //1库位用完申请前查看是否有未完成的周转任务
                                        string sssql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
                                        var scount = dbcontext.Db.Ado.GetDataTable(sssql, new List<SugarParameter>()
                                        {
                                              new SugarParameter("@ZONECODE",ZONECODE)
                                        });
                                        if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "1")
                                        {
                                            dbcontext.Db.Ado.BeginTran();

                                            string id = Guid.NewGuid().ToString("D");
                                            string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate())   
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                            //同区需要周转时
                                            string taskid = Guid.NewGuid().ToString("D");
                                            string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI","022704"),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });
                                            //更新周转库位为使用状态
                                            string ksql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
                                            dbcontext.Db.Ado.ExecuteCommand(ksql, new List<SugarParameter>() {

                                        new SugarParameter("@LOCATIONCODE","022704"),
                                        new SugarParameter("@ZONECODE",ZONECODE)

                                    });

                                            dbcontext.Db.Ado.CommitTran();
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "1",
                                                MESSAGE = "成功"

                                            };
                                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                        }
                                        else
                                        {
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "0",
                                                MESSAGE = "失败:当前有周转任务正在执行请稍候尝试！"

                                            };
                                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                        }

                                    }


                                }
                                else if (ZONECODE == "301-1")
                                {
                                    //原材料和半成品时
                                    if (LENUM.Substring(0, 2) == "32" & TARGET == "3007")
                                    {
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "0",
                                            MESSAGE = "失败:当前出库口不可出大托盘"

                                        };
                                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                    }
                                    else
                                    {
                                        dbcontext.Db.Ado.BeginTran();
                                        string id = Guid.NewGuid().ToString("D");
                                        string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                        int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                        string taskid = Guid.NewGuid().ToString("D");
                                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                        dbcontext.Db.Ado.CommitTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                    }



                                }

                                else
                                {
                                    //原材料和半成品时


                                    dbcontext.Db.Ado.BeginTran();
                                    string id = Guid.NewGuid().ToString("D");
                                    string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                    int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                    string taskid = Guid.NewGuid().ToString("D");
                                    string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                    dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                    dbcontext.Db.Ado.CommitTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);


                                }




                                #endregion
                                //}
                                //else//库存信息不一致
                                //{

                                //	reslutJson = new ReslutSapJson()
                                //	{
                                //		SUCCESS = "0",
                                //		MESSAGE = "失败:库存信息与输入列表信息不一致"

                                //	};
                                //	LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                //}

                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:当前库存未查询到空托盘"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                            }


                        }
                        else
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:未获取到列表信息"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                        }

                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }


            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteOperateLogF(ex.ToString(), "空盘或满盘出库", ip);
            }
            return reslutJson;
        }

        /// <summary>
        /// sap请求物料空盘或满盘出库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskoutboundfortest(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString().Trim();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                                                            //string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                                                              //string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值
                                                              //string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                                                              //string TARGET = json["TARGET"].ToString();//目标地址
                                                              //空托盘入库和满盘出库
                                                              //1.空托盘出库  
                if (IS_EMPTY != "1")
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301" || MA_TYPE == "302")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203" || MA_TYPE == "204" || MA_TYPE == "205")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }
                    if (target != "")
                    {
                        //
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表					

                        string LENUM = json["LENUM"].ToString();
                        string TARGET = json["TARGET"].ToString();
                        string IS_LARGE = json["IS_LARGE"].ToString();
                        //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                        string msql = @"
SELECT SOURCEORDER,CONTAINER  FROM wms_stocker_task WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE and TYPE=2  and STATE<2  
";
                        var mlist = dbcontext.Db.Ado.GetDataTable(msql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });
                        if (mlist != null && mlist.Rows.Count > 0)
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:当前托盘正在出库，请勿重复下架！"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0} 重复下架，下架失败", TASK_NO), "空盘或满盘出库", ip);
                        }
                        else
                        {
                            string gsql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,LOCATIONCODE  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE
";
                            var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });

                            if (glist != null && glist.Rows.Count > 0)
                            {   //获取出库起点
                                string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                                string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                                string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                                #region   作废
                                //比对出库信息与库存信息是否一致
                                //								int inventcount = 0;
                                //								for (int i = 0; i < data.Count(); i++)
                                //								{

                                //									string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                //									string CHARG = data[i]["CHARG"].ToString();
                                //									string MATNR = data[i]["MATNR"].ToString();
                                //									string sql = @"
                                //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  and BARCODE=@BARCODE   and ITEMCODE=@ITEMCODE  and LOT=@LOT
                                //";
                                //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                //					{

                                //						new SugarParameter("@SOURCEORDER",SOURCEORDER),
                                //						new SugarParameter("@BARCODE",TASK_NO_SN),
                                //						new SugarParameter("@ITEMCODE",MATNR),
                                //						new SugarParameter("@LOT",CHARG)

                                //					});
                                //									if (list.Rows.Count > 0)
                                //										inventcount++;
                                //								}

                                //								if (inventcount == data.Count())//库存信息一致
                                //{
                                #endregion
                                #region  执行出库   1.插入入库单和入库明细 2.生成堆垛机出库任务

                                if (ZONECODE == "201-1")
                                {

                                    int start = int.Parse(LOCATIONCODE.Substring(2, 2));
                                    int end = int.Parse(GetLoclation(TARGET).Substring(2, 2));
                                    if (((start >= 28 & end >= 28) || (start <= 27 & end <= 27)))//非周转任务时
                                    {
                                        dbcontext.Db.Ado.BeginTran();
                                        string id = Guid.NewGuid().ToString("D");
                                        string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                        int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                        for (int i = 0; i < data.Count(); i++)
                                        {
                                            string lid = Guid.NewGuid().ToString("D");
                                            string TASK_NO_SN = json["TASK_NO_SN"].ToString();
                                            string CHARG = json["CHARG"].ToString();
                                            string MATNR = json["MATNR"].ToString();
                                            //string WEIGHT = data[i]["WEIGHT"].ToString();

                                            string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                        }

                                        //同区不需要周转时
                                        string taskid = Guid.NewGuid().ToString("D");
                                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                        dbcontext.Db.Ado.CommitTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    }
                                    else
                                    {
                                        //1库位用完申请前查看是否有未完成的周转任务
                                        string sssql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
                                        var scount = dbcontext.Db.Ado.GetDataTable(sssql, new List<SugarParameter>()
                                        {
                                              new SugarParameter("@ZONECODE",ZONECODE)
                                        });
                                        if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "1")
                                        {
                                            dbcontext.Db.Ado.BeginTran();

                                            string id = Guid.NewGuid().ToString("D");
                                            string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                            int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                            for (int i = 0; i < data.Count(); i++)
                                            {
                                                string lid = Guid.NewGuid().ToString("D");
                                                string TASK_NO_SN = json["TASK_NO_SN"].ToString();
                                                string CHARG = json["CHARG"].ToString();
                                                string MATNR = json["MATNR"].ToString();
                                                //string WEIGHT = data[i]["WEIGHT"].ToString();

                                                string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                                dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                            }

                                            //同区需要周转时
                                            string taskid = Guid.NewGuid().ToString("D");
                                            string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                            dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI","022704"),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });
                                            //更新周转库位为使用状态
                                            string ksql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
                                            dbcontext.Db.Ado.ExecuteCommand(ksql, new List<SugarParameter>() {

                                        new SugarParameter("@LOCATIONCODE","022704"),
                                        new SugarParameter("@ZONECODE",ZONECODE)

                                    });

                                            dbcontext.Db.Ado.CommitTran();
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "1",
                                                MESSAGE = "成功"

                                            };
                                            LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                        }
                                        else
                                        {
                                            reslutJson = new ReslutSapJson()
                                            {
                                                SUCCESS = "0",
                                                MESSAGE = "失败:当前有周转任务正在执行请稍候尝试！"

                                            };
                                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                        }

                                    }


                                }
                                else
                                {


                                    dbcontext.Db.Ado.BeginTran();
                                    string id = Guid.NewGuid().ToString("D");
                                    string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                    int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                    for (int i = 0; i < data.Count(); i++)
                                    {
                                        string lid = Guid.NewGuid().ToString("D");
                                        string TASK_NO_SN = json["TASK_NO_SN"].ToString();
                                        string CHARG = json["CHARG"].ToString();
                                        string MATNR = json["MATNR"].ToString();
                                        //string WEIGHT = data[i]["WEIGHT"].ToString();

                                        string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                    }

                                    //同区不需要周转时
                                    string taskid = Guid.NewGuid().ToString("D");
                                    string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                    dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                    dbcontext.Db.Ado.CommitTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);



                                }
                                #endregion


                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:当前库存未查询到该托盘信息"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                            }
                        }





                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }
                else
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301" || MA_TYPE == "302")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203" || MA_TYPE == "204" || MA_TYPE == "205")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }



                    if (target != "")
                    {
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表


                        string LENUM = json["LENUM"].ToString();//空
                        string TARGET = json["TARGET"].ToString();//有
                        string IS_LARGE = json["IS_LARGE"].ToString();//有
                                                                      //托盘类型
                        string CONTAINERTYPE = "";
                        if (MA_TYPE == "101")
                        {
                            CONTAINERTYPE = "0";
                        }
                        else if (MA_TYPE == "302")
                        {
                            CONTAINERTYPE = "1";
                        }
                        else if (MA_TYPE == "301")
                        {
                            CONTAINERTYPE = "0";
                        }
                        else if (MA_TYPE == "201")
                        {
                            CONTAINERTYPE = "0";
                        }
                        else if (MA_TYPE == "202")
                        {
                            CONTAINERTYPE = "3";
                        }
                        else if (MA_TYPE == "203")
                        {
                            CONTAINERTYPE = "1";
                        }
                        else if (MA_TYPE == "204")
                        {
                            CONTAINERTYPE = "4";
                        }
                        else if (MA_TYPE == "205")
                        {
                            CONTAINERTYPE = "2";
                        }

                        else
                        {
                            CONTAINERTYPE = "";
                        }
                        //获取空托盘信息
                        string gsql = @"
SELECT a.SOURCEORDER,a.CONTAINER,a.ZONECODE,a.LOCATIONCODE  FROM wms_base_location  a    WHERE a.ZONECODE=@ZONECODE   AND  ''!=a.CONTAINER  AND a.CONTAINERTYPE=@CONTAINERTYPE 
AND  CONTAINER  NOT IN(SELECT  CONTAINER  from wms_stocker_task  where ZONECODE=@ZONECODE and  TYPE=2 and  STATE<=1)
AND  (SELECT  COUNT(*)  from  wms_bill_inventorybook  b  where b.SOURCEORDER=a.SOURCEORDER  and  b.BARCODE=a.CONTAINER)=0  LIMIT 1 
";
                        var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@ZONECODE",target)

                    });
                        //有空托盘信息时
                        if (glist != null && glist.Rows.Count > 0)
                        {   //获取出库起点
                            string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                            string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                            string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                            string CONTAINER = glist.Rows[0]["CONTAINER"].ToString();


                            string TASK_NO_SN = json["TASK_NO_SN"].ToString();
                            string CHARG = json["CHARG"].ToString();
                            string MATNR = json["MATNR"].ToString();
                            //									string sql = @"
                            //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  
                            //";
                            //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                            //					{

                            //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

                            //					});



                            #region  执行出库   1.插入出库单 2.生成堆垛机出库任务

                            if (ZONECODE == "201-1")
                            {

                                int start = int.Parse(LOCATIONCODE.Substring(2, 2));
                                int end = int.Parse(GetLoclation(TARGET).Substring(2, 2));
                                if (((start >= 28 & end >= 28) || (start <= 27 & end <= 27)))//非周转任务时
                                {
                                    dbcontext.Db.Ado.BeginTran();
                                    string id = Guid.NewGuid().ToString("D");
                                    string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                    int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                    string taskid = Guid.NewGuid().ToString("D");
                                    string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                    dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                    dbcontext.Db.Ado.CommitTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                }
                                else
                                {
                                    //1库位用完申请前查看是否有未完成的周转任务
                                    string sssql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
                                    var scount = dbcontext.Db.Ado.GetDataTable(sssql, new List<SugarParameter>()
                                        {
                                              new SugarParameter("@ZONECODE",ZONECODE)
                                        });
                                    if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "1")
                                    {
                                        dbcontext.Db.Ado.BeginTran();

                                        string id = Guid.NewGuid().ToString("D");
                                        string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                        int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });


                                        //同区需要周转时
                                        string taskid = Guid.NewGuid().ToString("D");
                                        string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI","022704"),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });
                                        //更新周转库位为使用状态
                                        string ksql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
                                        dbcontext.Db.Ado.ExecuteCommand(ksql, new List<SugarParameter>() {

                                        new SugarParameter("@LOCATIONCODE","022704"),
                                        new SugarParameter("@ZONECODE",ZONECODE)

                                    });

                                        dbcontext.Db.Ado.CommitTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                    }
                                    else
                                    {
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "0",
                                            MESSAGE = "失败:当前有周转任务正在执行请稍候尝试！"

                                        };
                                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    }

                                }


                            }
                            else
                            {
                                //原材料和半成品时


                                dbcontext.Db.Ado.BeginTran();
                                string id = Guid.NewGuid().ToString("D");
                                string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                string taskid = Guid.NewGuid().ToString("D");
                                string tasksql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
";
                                dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",taskid),
                        new SugarParameter("@CODE",taskid),
                        new SugarParameter("@ENDTAXI",TARGET),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@STARTTAXI",LOCATIONCODE),
                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@SOURCEORDER",TASK_NO)

                    });

                                dbcontext.Db.Ado.CommitTran();
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "成功"

                                };
                                LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);


                            }




                            #endregion
                            //}
                            //else//库存信息不一致
                            //{

                            //	reslutJson = new ReslutSapJson()
                            //	{
                            //		SUCCESS = "0",
                            //		MESSAGE = "失败:库存信息与输入列表信息不一致"

                            //	};
                            //	LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                            //}

                        }
                        else
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:当前库存未查询到空托盘"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                        }



                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }


            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteOperateLogF(ex.ToString(), "空盘或满盘出库", ip);
            }
            return reslutJson;
        }

        /// <summary>
        /// sap请求物料空盘或满盘出库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson settaskoutboundtest(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {
                string MA_TYPE = json["MA_TYPE"].ToString();//物料类型
                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                                                            //string LENUM = json["LENUM"].ToString();//托盘条码
                string IS_EMPTY = json["IS_EMPTY"].ToString();//是否为空托盘
                                                              //string WEIGHT_ERRORS = json["WEIGHT_ERRORS"].ToString();//托盘重量误差值
                                                              //string IS_LARGE = json["IS_LARGE"].ToString();//盘具类型（大、小）
                                                              //string TARGET = json["TARGET"].ToString();//目标地址
                                                              //空托盘入库和满盘出库
                                                              //1.空托盘出库  
                if (IS_EMPTY != "1")
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "401")
                    {
                        target = "401-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }
                    if (target != "")
                    {
                        //
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表

                        if (data.Count() > 0)
                        {

                            string LENUM = data[0]["LENUM"].ToString();
                            string TARGET = data[0]["TARGET"].ToString();
                            string IS_LARGE = data[0]["IS_LARGE"].ToString();
                            //同一时间堆垛机只能执行一条该托盘出入库信息，查询该托盘的出库信息，如果已经有未完成的任务则提示错误信息
                            string msql = @"
SELECT SOURCEORDER,CONTAINER  FROM wms_stocker_task WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE and TYPE=2  and STATE<2  
";
                            var mlist = dbcontext.Db.Ado.GetDataTable(msql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });
                            if (mlist != null && mlist.Rows.Count > 0)
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:当前托盘正在出库，请勿重复下架！"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0} 重复下架，下架失败", TASK_NO), "空盘或满盘出库", ip);
                            }
                            else
                            {
                                string gsql = @"
SELECT SOURCEORDER,CONTAINER,ZONECODE,LOCATIONCODE  FROM wms_base_location WHERE CONTAINER=@CONTAINER  and ZONECODE=@ZONECODE
";
                                var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@ZONECODE",target)

                    });

                                if (glist != null && glist.Rows.Count > 0)
                                {   //获取出库起点
                                    string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                                    string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                                    string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                                    #region   作废
                                    //比对出库信息与库存信息是否一致
                                    //								int inventcount = 0;
                                    //								for (int i = 0; i < data.Count(); i++)
                                    //								{

                                    //									string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                    //									string CHARG = data[i]["CHARG"].ToString();
                                    //									string MATNR = data[i]["MATNR"].ToString();
                                    //									string sql = @"
                                    //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  and BARCODE=@BARCODE   and ITEMCODE=@ITEMCODE  and LOT=@LOT
                                    //";
                                    //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                    //					{

                                    //						new SugarParameter("@SOURCEORDER",SOURCEORDER),
                                    //						new SugarParameter("@BARCODE",TASK_NO_SN),
                                    //						new SugarParameter("@ITEMCODE",MATNR),
                                    //						new SugarParameter("@LOT",CHARG)

                                    //					});
                                    //									if (list.Rows.Count > 0)
                                    //										inventcount++;
                                    //								}

                                    //								if (inventcount == data.Count())//库存信息一致
                                    //{
                                    #endregion
                                    #region  执行出库   1.插入入库单和入库明细 2.生成堆垛机出库任务
                                    dbcontext.Db.Ado.BeginTran();
                                    string id = Guid.NewGuid().ToString("D");
                                    string insertsql = @"
    INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())   
";
                                    int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@CONTAINERTYPE",IS_LARGE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });

                                    for (int i = 0; i < data.Count(); i++)
                                    {
                                        string lid = Guid.NewGuid().ToString("D");
                                        string TASK_NO_SN = data[i]["TASK_NO_SN"].ToString();
                                        string CHARG = data[i]["CHARG"].ToString();
                                        string MATNR = data[i]["MATNR"].ToString();
                                        //string WEIGHT = data[i]["WEIGHT"].ToString();

                                        string listsql = @"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
";
                                        dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",lid),
                        new SugarParameter("@HEADERID",id),
                        new SugarParameter("@ITEMCODE",MATNR),
                        new SugarParameter("@LOT",CHARG),
						//new SugarParameter("@WEIGHT",WEIGHT),
						new SugarParameter("@TASK_NO_SN",TASK_NO_SN)

                    });
                                    }
                                    dbcontext.Db.Ado.CommitTran();
                                    if (a > 0)
                                    {
                                        //生成堆垛机任务  
                                        //半成品库单独逻辑 ，其他标准逻辑

                                        string taskid = Guid.NewGuid().ToString("D");
                                        wms_stocker_task wmsStocker = new wms_stocker_task();
                                        wmsStocker.ID = taskid;
                                        wmsStocker.CODE = taskid;
                                        wmsStocker.ENDTAXI = TARGET;
                                        wmsStocker.CONTAINER = LENUM;
                                        wmsStocker.STARTTAXI = LOCATIONCODE;//起始点

                                        wmsStocker.TYPE = 2;
                                        wmsStocker.STARTTIME = DateTime.Now;
                                        wmsStocker.ZONECODE = ZONECODE;
                                        wmsStocker.SOURCEORDER = TASK_NO;
                                        wmsStocker.PRIORITY = 0;
                                        wmsStocker.STATE = 0;
                                        WmsStockerTaskDao.insert(wmsStocker);

                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "1",
                                            MESSAGE = "成功"

                                        };
                                        LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    }
                                    else
                                    {
                                        dbcontext.Db.Ado.RollbackTran();
                                        reslutJson = new ReslutSapJson()
                                        {
                                            SUCCESS = "0",
                                            MESSAGE = "失败:WMS内部错误"

                                        };
                                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    }
                                    #endregion
                                    //}
                                    //else//库存信息不一致
                                    //{

                                    //	reslutJson = new ReslutSapJson()
                                    //	{
                                    //		SUCCESS = "0",
                                    //		MESSAGE = "失败:库存信息与输入列表信息不一致"

                                    //	};
                                    //	LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                    //}

                                }
                                else
                                {
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "0",
                                        MESSAGE = "失败:当前库存未查询到该托盘信息"

                                    };
                                    LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                                }
                            }


                        }
                        else
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:未获取到托盘列表信息"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                        }

                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }
                else
                {
                    //生成出库任务


                    //string id = TASK_NO;
                    //获取库区
                    string target = "";
                    if (MA_TYPE == "101")
                    {
                        target = "101-1";
                    }
                    else if (MA_TYPE == "301")
                    {
                        target = "301-1";
                    }
                    else if (MA_TYPE == "401")
                    {
                        target = "401-1";
                    }
                    else if (MA_TYPE == "201" || MA_TYPE == "202" || MA_TYPE == "203")
                    {
                        target = "201-1";
                    }
                    else
                    {
                        target = "";
                    }



                    if (target != "")
                    {
                        //查询库存信息 
                        JToken data = json["DATA"];//list列表

                        if (data.Count() > 0)
                        {
                            string LENUM = data[0]["LENUM"].ToString();//空
                            string TARGET = data[0]["TARGET"].ToString();//有
                            string IS_LARGE = data[0]["IS_LARGE"].ToString();//有
                                                                             //托盘类型
                            string CONTAINERTYPE = "";
                            if (MA_TYPE == "101")
                            {

                                CONTAINERTYPE = "0";
                            }
                            if (MA_TYPE == "401")
                            {

                                CONTAINERTYPE = "0";
                            }
                            else if (MA_TYPE == "301")
                            {
                                CONTAINERTYPE = IS_LARGE;
                            }
                            else if (MA_TYPE == "201")
                            {
                                CONTAINERTYPE = "0";
                            }
                            else if (MA_TYPE == "202")
                            {
                                CONTAINERTYPE = "1";
                            }
                            else if (MA_TYPE == "203")
                            {
                                CONTAINERTYPE = "2";
                            }
                            else
                            {
                                CONTAINERTYPE = "";
                            }
                            //获取空托盘信息
                            string gsql = @"
SELECT a.SOURCEORDER,a.CONTAINER,a.ZONECODE,a.LOCATIONCODE  FROM wms_base_location  a    WHERE a.ZONECODE=@ZONECODE   AND  ''!=a.CONTAINER  AND a.CONTAINERTYPE=@CONTAINERTYPE 
AND  CONTAINER  NOT IN(SELECT  CONTAINER  from wms_stocker_task  where ZONECODE=@ZONECODE and  TYPE=2 and  STATE<=1)
AND  (SELECT  COUNT(*)  from  wms_bill_inventorybook  b  where b.SOURCEORDER=a.SOURCEORDER  and  b.BARCODE=a.CONTAINER)=0  LIMIT 1 
";
                            var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@ZONECODE",target)

                    });
                            //有空托盘信息时
                            if (glist != null && glist.Rows.Count > 0)
                            {   //获取出库起点
                                string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
                                string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
                                string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
                                string CONTAINER = glist.Rows[0]["CONTAINER"].ToString();


                                string TASK_NO_SN = data[0]["TASK_NO_SN"].ToString();
                                string CHARG = data[0]["CHARG"].ToString();
                                string MATNR = data[0]["MATNR"].ToString();
                                //									string sql = @"
                                //SELECT ID  FROM wms_bill_inventorybook WHERE SOURCEORDER=@SOURCEORDER  
                                //";
                                //									var list = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                //					{

                                //						new SugarParameter("@SOURCEORDER",SOURCEORDER)

                                //					});



                                #region  执行出库   1.插入出库单 2.生成堆垛机出库任务
                                dbcontext.Db.Ado.BeginTran();
                                string id = Guid.NewGuid().ToString("D");
                                string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
                                int a = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@ZONECODE",target),
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",CONTAINER),
                        new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
                        new SugarParameter("@TARGET",TARGET),
                        new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

                    });
                                dbcontext.Db.Ado.CommitTran();
                                if (a > 0)
                                {
                                    //生成堆垛机任务
                                    string taskid = Guid.NewGuid().ToString("D");
                                    wms_stocker_task wmsStocker = new wms_stocker_task();
                                    wmsStocker.ID = taskid;
                                    wmsStocker.CODE = taskid;
                                    wmsStocker.ENDTAXI = TARGET;
                                    wmsStocker.CONTAINER = CONTAINER;
                                    wmsStocker.STARTTAXI = LOCATIONCODE;//起始点

                                    wmsStocker.TYPE = 2;
                                    wmsStocker.STARTTIME = DateTime.Now;
                                    wmsStocker.ZONECODE = ZONECODE;
                                    wmsStocker.SOURCEORDER = TASK_NO;
                                    wmsStocker.PRIORITY = 0;
                                    wmsStocker.STATE = 0;
                                    WmsStockerTaskDao.insert(wmsStocker);
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "1",
                                        MESSAGE = "成功"

                                    };
                                    LOGService.WriteOperateLogT(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                }
                                else
                                {
                                    dbcontext.Db.Ado.RollbackTran();
                                    reslutJson = new ReslutSapJson()
                                    {
                                        SUCCESS = "0",
                                        MESSAGE = "失败:WMS内部错误"

                                    };
                                    LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                }

                                #endregion
                                //}
                                //else//库存信息不一致
                                //{

                                //	reslutJson = new ReslutSapJson()
                                //	{
                                //		SUCCESS = "0",
                                //		MESSAGE = "失败:库存信息与输入列表信息不一致"

                                //	};
                                //	LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                                //}

                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:当前库存未查询到空托盘"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);

                            }


                        }
                        else
                        {
                            reslutJson = new ReslutSapJson()
                            {
                                SUCCESS = "0",
                                MESSAGE = "失败:未获取到列表信息"

                            };
                            LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                        }

                    }
                    else
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:物料类型未识别"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘出库", ip);
                    }
                }


            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteOperateLogF(ex.ToString(), "空盘或满盘出库", ip);
            }
            return reslutJson;
        }
        /// <summary>
        /// 请求清除指定任务号的任
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSapJson setcleartask(JObject json, string ip)
        {
            ReslutSapJson reslutJson = new ReslutSapJson();
            try
            {

                string TASK_NO = json["TASK_NO"].ToString();//任务标识
                string LENUM = json["LENUM"].ToString();//托盘条码
                                                        //查询是否满足取消任务条件   堆垛机任务未执行
                string gsql = @"
SELECT ID ,ZONECODE,TYPE  FROM wms_stocker_task WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER  and  STATE=0
";
                var a = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                //				string hsql = @"
                //SELECT ID ,ZONECODE FROM wms_conveyor_task WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER
                //";
                //				var b = dbcontext.Db.Ado.GetDataTable(hsql, new List<SugarParameter>()
                //					{

                //						new SugarParameter("@SOURCEORDER",TASK_NO),
                //						new SugarParameter("@CONTAINER",LENUM)

                //					});
                if (a.Rows.Count > 0)
                {
                    //出入库任务清除
                    if (a.Rows[0]["ZONECODE"].ToString() == "101-1")
                    {
                        reslutJson = new ReslutSapJson()
                        {
                            SUCCESS = "0",
                            MESSAGE = "失败:无法清除指定任务，不满足取消条件"

                        };
                        LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "取消指定任务", ip);
                    }
                    else
                    {
                        dbcontext.Db.Ado.BeginTran();
                        var ZONECODE = a.Rows[0]["ZONECODE"].ToString();
                        var TYPE = a.Rows[0]["TYPE"].ToString();
                        if (TYPE == "2")//出库
                        {

                            string outsql = @"
SELECT ID  FROM wms_bill_putoutheader WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER 
";
                            var gg = dbcontext.Db.Ado.GetDataTable(outsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                            if (gg != null && gg.Rows.Count > 0)
                            {
                                var headid = gg.Rows[0]["ID"].ToString();
                                //删除出库单及明细  删除堆垛机任务
                                string delheadsql = @"
DELETE FROM    wms_bill_putoutbody WHERE HEADERID=@HEADERID 
";
                                dbcontext.Db.Ado.ExecuteCommand(delheadsql, new List<SugarParameter>()
                    {
                        new SugarParameter("@HEADERID",headid)

                    });
                                string delbodysql = @"
DELETE FROM    wms_bill_putoutheader WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER
";
                                dbcontext.Db.Ado.ExecuteCommand(delbodysql, new List<SugarParameter>()
                    {
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                                // ===== 单针改造：出库取消，释放预占针 RESERVED→AVAILABLE =====
                                NeedleService.UnreserveContainer(dbcontext.Db, LENUM, "取消出库");
                                string delsql = @"
DELETE FROM    wms_stocker_task WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER 
";
                                dbcontext.Db.Ado.ExecuteCommand(delsql, new List<SugarParameter>()
                    {
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "取消成功"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "取消指定任务", ip);
                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:无查询到出入库记录"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "取消指定任务", ip);
                            }

                        }
                        else
                        {
                            string outsql = @"
SELECT ID  FROM wms_bill_putinbindingheader WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER 
";
                            var gg = dbcontext.Db.Ado.GetDataTable(outsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                            if (gg != null && gg.Rows.Count > 0)
                            {
                                var headid = gg.Rows[0]["ID"].ToString();
                                //删除出库单及明细  删除堆垛机任务
                                string delheadsql = @"
DELETE FROM    wms_bill_putinbindingbody  WHERE HEADERID=@HEADERID 
";
                                dbcontext.Db.Ado.ExecuteCommand(delheadsql, new List<SugarParameter>()
                    {
                        new SugarParameter("@HEADERID",headid)

                    });
                                string delbodysql = @"
DELETE FROM    wms_bill_putinbindingheader WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER
";
                                dbcontext.Db.Ado.ExecuteCommand(delbodysql, new List<SugarParameter>()
                    {
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                                // ===== 单针改造：入库绑定取消，本盒待入库针 INBOUND_PENDING→BIND_CANCELLED =====
                                NeedleService.MarkBindCancelled(dbcontext.Db, LENUM, "取消入库");
                                string delsql = @"
DELETE FROM    wms_stocker_task WHERE SOURCEORDER=@SOURCEORDER  and CONTAINER=@CONTAINER 
";
                                dbcontext.Db.Ado.ExecuteCommand(delsql, new List<SugarParameter>()
                    {
                        new SugarParameter("@SOURCEORDER",TASK_NO),
                        new SugarParameter("@CONTAINER",LENUM)

                    });
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "1",
                                    MESSAGE = "取消成功"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "取消指定任务", ip);
                            }
                            else
                            {
                                reslutJson = new ReslutSapJson()
                                {
                                    SUCCESS = "0",
                                    MESSAGE = "失败:无查询到出入库记录"

                                };
                                LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "取消指定任务", ip);
                            }

                        }

                        // ===== 单针改造：补齐原实现缺失的 CommitTran（修复前事务悬挂在共享连接上，由后续任意请求间接提交） =====
                        dbcontext.Db.Ado.CommitTran();
                    }

                }
                else
                {

                    reslutJson = new ReslutSapJson()
                    {
                        SUCCESS = "0",
                        MESSAGE = "失败:无法清除指定任务，不满足取消条件"

                    };
                    LOGService.WriteOperateLogF(string.Format("任务号:{0}", TASK_NO), "空盘或满盘入库", ip);

                }


            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误"

                };
                LOGService.WriteExceptionLog(ex);
            }
            return reslutJson;
        }

        /// <summary>
        /// 请求按一定条件查询库位信息
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutSerchSapJson getlocationinfo(JObject json, string ip)
        {
            ReslutSerchSapJson reslutJson = new ReslutSerchSapJson();
            try
            {
                string WAREHOUSE_ID = json["WAREHOUSE_ID"].ToString();//仓库标识
                string LENUM = json["LENUM"].ToString();//托盘条码
                string CHARG = json["CHARG"].ToString();//物料批次
                string MATNR = json["MATNR"].ToString();//物料号
                string WEIGHT = json["WEIGHT"].ToString();//托盘重量误差值

                string ZONECODE = ""; // 611  原材料     612半成品    613成品库
                if (WAREHOUSE_ID == "711")
                {
                    ZONECODE = "101-1";
                }
                else if (WAREHOUSE_ID == "712")
                {
                    ZONECODE = "201-1";
                }
                else if (WAREHOUSE_ID == "713")
                {
                    ZONECODE = "301-1";
                }

                else
                {
                    ZONECODE = "";
                }
                if (ZONECODE != "")
                {

                    string gsql = @"
SELECT  CASE a.ZONECODE WHEN '101-1' THEN '711'  when '201-1' THEN '712' WHEN '301-1' THEN '713'      ELSE '' END  as WAREHOUSE_ID ,a.LOCATIONCODE as LOCATION_CODE,a.CONTAINER as LENUM,b.LOT as CHARG ,  b.ITEMCODE as MATNR  from   wms_base_location  a  INNER JOIN   wms_bill_inventorybook  b  on  a.CONTAINER=b.BARCODE  and  a.SOURCEORDER=b.SOURCEORDER
WHERE  a.ZONECODE=@ZONECODE  and  (a.CONTAINER=@CONTAINER  or @CONTAINER='')   and (b.LOT=@LOT or @LOT ='')   and (b.ITEMCODE=@ITEMCODE  or @ITEMCODE='')   
";
                    var dt = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ZONECODE",ZONECODE),
                        new SugarParameter("@CONTAINER",LENUM),
                        new SugarParameter("@LOT",CHARG),
                        new SugarParameter("@ITEMCODE",MATNR)

                    });

                    List<SModel> rows = new List<SModel>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        SModel model = new SModel();
                        model.WAREHOUSE_ID = dr["WAREHOUSE_ID"].ToString();
                        model.LOCATION_CODE = dr["LOCATION_CODE"].ToString();
                        model.STATUS = "4";
                        model.LENUM = dr["LENUM"].ToString();
                        model.CHARG = dr["CHARG"].ToString();
                        model.MATNR = dr["MATNR"].ToString();
                        rows.Add(model);
                    }
                    reslutJson = new ReslutSerchSapJson()
                    {
                        SUCCESS = "1",
                        MESSAGE = "查询成功",
                        ROWS = rows

                    };
                    LOGService.WriteOperateLogT(string.Format("查询成功"), "查询库位信息", ip);
                }
                else
                {
                    reslutJson = new ReslutSerchSapJson()
                    {
                        SUCCESS = "0",
                        MESSAGE = "查询失败，仓库标识未识别",
                        ROWS = null

                    };
                    LOGService.WriteOperateLogF(string.Format("查询失败"), "查询库位信息", ip);
                }
            }
            catch (Exception ex)
            {

                reslutJson = new ReslutSerchSapJson()
                {
                    SUCCESS = "0",
                    MESSAGE = "失败:WMS内部错误",
                    ROWS = null
                };
                LOGService.WriteOperateLogF(string.Format("查询失败" + reslutJson.MESSAGE), "查询库位信息", ip);
            }
            return reslutJson;
        }



        #endregion


        #region   江西联益交互

        //获取巷道，请求库位 ，任务完成，入库，出库

        /// <summary>
        /// 请求获取巷道（空库位>=20的巷道优先，空位多的优先，同规格集中为辅）
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson LYgetlane(JObject json, string ip)
        {
            ReslutJson reslutJson;

            try
            {
                #region 参数
                string barcodeone = json["barcodeone"]?.ToString();

                if (string.IsNullOrEmpty(barcodeone))
                {
                    return new ReslutJson
                    {
                        success = false,
                        msg = "barcodeone不能为空"
                    };
                }
                #endregion

                #region 1. 查询绑定物料
                string sql = @"
            SELECT a.CONTAINER, b.ITEMCODE
            FROM wms_bill_putinbindingheader a
            INNER JOIN wms_bill_putinbindingbody b ON a.ID = b.HEADERID
            WHERE a.CONTAINER = @CONTAINER AND a.STATE < 1
        ";

                var glist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>
        {
            new SugarParameter("@CONTAINER", barcodeone)
        });

                if (glist == null || glist.Rows.Count == 0)
                {
                    return new ReslutJson
                    {
                        success = false,
                        msg = "未找到绑定物料",
                        data = new Dictionary<string, string> { { "laneone", "-1" } }
                    };
                }

                string itemCode = glist.Rows[0]["ITEMCODE"]?.ToString();
                #endregion

                #region 2. 统一巷道分配（空库位>=20优先，空位多的优先，同规格集中为辅）
                const int minPreferredEmptyLocations = 20;
                sql = @"
            SELECT
                L.TUNNELNUM,
                L.empty_cnt,
                IFNULL(S.same_sku_cnt, 0) AS same_sku_cnt
            FROM (
                SELECT TUNNELNUM, COUNT(*) AS empty_cnt
                FROM wms_base_location
                WHERE ENABLED = 1 AND ISAVIALABLE = 1
                GROUP BY TUNNELNUM
                HAVING COUNT(*) > 0
            ) L
            LEFT JOIN (
                SELECT TUNNELNUM, COUNT(*) AS same_sku_cnt
                FROM wms_base_location
                WHERE ENABLED = 1 AND SOURCEORDER = @ITEMCODE
                GROUP BY TUNNELNUM
            ) S ON S.TUNNELNUM = L.TUNNELNUM
            ORDER BY
                CASE WHEN L.empty_cnt >= @MIN_EMPTY THEN 1 ELSE 0 END DESC,
                L.empty_cnt DESC,
                IFNULL(S.same_sku_cnt, 0) ASC,
                RAND()
            LIMIT 1";

                var laneList = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>
                {
                    new SugarParameter("@ITEMCODE", itemCode),
                    new SugarParameter("@MIN_EMPTY", minPreferredEmptyLocations)
                });
                #endregion

                #region 3. 巷道选择

                string lane1 = "-1";
                string laneLogDetail = string.Empty;
                if (laneList != null && laneList.Rows.Count > 0)
                {
                    var row = laneList.Rows[0];
                    lane1 = row["TUNNELNUM"]?.ToString() ?? "-1";
                    int emptyCnt = Convert.ToInt32(row["empty_cnt"]);
                    int sameSkuCnt = Convert.ToInt32(row["same_sku_cnt"]);
                    string tier = emptyCnt >= minPreferredEmptyLocations ? "A级(>=20)" : "B级(<20兜底)";
                    laneLogDetail = string.Format("TUNNELNUM={0}, empty_cnt={1}, same_sku_cnt={2}, tier={3}",
                        lane1, emptyCnt, sameSkuCnt, tier);
                }

                #endregion

                #region 4. 巷道映射（避免 if-else）

                var laneMap = new Dictionary<string, string>
        {
            { "1", "1001" },
            { "2", "1002" },
            { "3", "1003" },
            { "4", "1004" }
        };

                if (laneMap.ContainsKey(lane1))
                    lane1 = laneMap[lane1];

                #endregion

                #region 5. 返回结果

                reslutJson = new ReslutJson
                {
                    success = true,
                    msg = "WCS请求成功!",
                    data = new Dictionary<string, string>
            {
                { "laneone", lane1 }
            }
                };

                string logMsg = string.IsNullOrEmpty(laneLogDetail)
                    ? "巷道分配成功：" + lane1
                    : "巷道分配成功：" + lane1 + "，" + laneLogDetail;
                LOGService.WriteWcsRequestLog(context, json, logMsg, "入库分配巷道");

                return reslutJson;

                #endregion
            }
            catch (Exception ex)
            {
                LOGService.WriteExceptionLog(ex);

                return new ReslutJson
                {
                    success = false,
                    msg = ex.Message
                };
            }
        }
        ///// <summary>
        ///// 请求获取巷道
        ///// </summary>
        ///// <param name="json"></param>
        ///// <param name="ip"></param>
        ///// <returns></returns>
        //public ReslutJson LYgetlane(JObject json, string ip)
        //{
        //    ReslutJson reslutJson = new ReslutJson();




        //    // 提取List数据
        //    //JArray hobbiesArray = (JArray)json["BARCODE"];
        //    //List<string> hobbies = new List<string>();
        //    //foreach (JToken hobby in hobbiesArray)
        //    //{
        //    //	hobbies.Add(hobby.ToString());
        //    //}
        //    //查询规格  未查询到绑定数据或库位满打回  查询到则依据均衡原则入库


        //    #region 巷道分配
        //    string sql = "";
        //    string lane1 = "0";
        //    string lane2 = "0";
        //    try
        //    {
        //        string barcodeone = json["barcodeone"].ToString();//托盘条码1
        //        string barcodetwo = json["barcodetwo"].ToString();//托盘条码2
        //        if (barcodeone != "")
        //        {


        //            sql = @"
        //SELECT a.CONTAINER  ,  b.ITEMCODE   from   wms_bill_putinbindingheader  a  
        //INNER JOIN   wms_bill_putinbindingbody  b  on  a.ID=b.HEADERID  
        //WHERE  a.CONTAINER=@CONTAINER    AND  a.STATE<1 
        //";
        //            var glist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //            {
        //                new SugarParameter("@CONTAINER",barcodeone)

        //            });

        //            if (glist != null && glist.Rows.Count > 0)
        //            {
        //                string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
        //                sql = @"
        //SELECT TUNNELNUM FROM (  SELECT t.TUNNELNUM ,IFNULL(COUNT(I.TUNNELNUM),0) AS  cnt    from  (SELECT 1  AS  TUNNELNUM  UNION  ALL  SELECT 2 UNION  ALL  SELECT 3 UNION ALL SELECT 4) AS  t LEFT JOIN   wms_base_location  I
        //ON  I.SOURCEORDER=@CONTAINER   and I.TUNNELNUM=t.TUNNELNUM  GROUP BY t.TUNNELNUM)g  ORDER BY cnt  ASC,RAND() LIMIT 1   
        //";
        //                var gglist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //            {
        //                new SugarParameter("@CONTAINER",ITEMCODE)

        //            });
        //                //获取可用库位
        //                sql = @"
        //SELECT  TUNNELNUM,count(*) as NUM  from  wms_base_location  where  ENABLED=1  and ISAVIALABLE=1    GROUP BY TUNNELNUM HAVING count(*)>0  
        //";
        //                var mmlist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //                {


        //                });
        //                int lan = int.Parse(gglist.Rows[0]["TUNNELNUM"].ToString());
        //                var hastun = mmlist.Select($"TUNNELNUM={lan}").Length;
        //                if (hastun > 0)
        //                {
        //                    lane1 = lan.ToString();
        //                }
        //                else
        //                {
        //                    lane1 = mmlist.Rows[0]["TUNNELNUM"].ToString();
        //                    if (mmlist != null && mmlist.Rows.Count > 0)
        //                    {
        //                        var minRow = mmlist.AsEnumerable()
        //                            .OrderBy(r => Convert.ToInt32(r["NUM"]))
        //                            .ThenBy(_ => Guid.NewGuid())
        //                            .First();

        //                        lane1 = minRow["TUNNELNUM"].ToString();
        //                    }
        //                    else
        //                    {
        //                        lane1 = "-1";
        //                    }
        //                }

        //                //LOGService.WriteOperateLogT(string.Format("1号请求成功"), "立库请求巷道", ip);

        //            }
        //            else
        //            {
        //                lane1 = "-1";

        //            }

        //        }


        //        if (barcodetwo != "")
        //        {


        //            sql = @"
        //SELECT a.CONTAINER  ,  b.ITEMCODE   from   wms_bill_putinbindingheader  a  
        //INNER JOIN   wms_bill_putinbindingbody  b  on  a.ID=b.HEADERID  
        //WHERE  a.CONTAINER=@CONTAINER    AND  a.STATE<2 
        //";
        //            var glist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //            {
        //                new SugarParameter("@CONTAINER",barcodetwo)

        //            });

        //            if (glist != null && glist.Rows.Count > 0)
        //            {
        //                string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
        //                sql = @"
        //SELECT TUNNELNUM FROM (  SELECT t.TUNNELNUM ,IFNULL(COUNT(I.TUNNELNUM),0) AS  cnt    from  (SELECT 1  AS  TUNNELNUM  UNION  ALL  SELECT 2 UNION  ALL  SELECT 3 UNION ALL SELECT 4) AS  t LEFT JOIN   wms_base_location  I
        //ON  I.SOURCEORDER=@CONTAINER   and I.TUNNELNUM=t.TUNNELNUM  GROUP BY t.TUNNELNUM)g  ORDER BY cnt  ASC LIMIT 1   
        //";
        //                var gglist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //            {
        //                new SugarParameter("@CONTAINER",ITEMCODE)

        //            });

        //                //获取可用库位
        //                sql = @"
        //SELECT  TUNNELNUM,count(*) as NUM  from  wms_base_location  where  ENABLED=1  and ISAVIALABLE=1    GROUP BY TUNNELNUM HAVING count(*)>0  
        //";
        //                var mmlist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
        //                {


        //                });
        //                int lan = int.Parse(gglist.Rows[0]["TUNNELNUM"].ToString());
        //                var hastun = mmlist.Select($"TUNNELNUM={lan}").Length;
        //                if (hastun > 0)
        //                {
        //                    lane2 = lan.ToString();
        //                }
        //                else
        //                {
        //                    lane2 = mmlist.Rows[0]["TUNNELNUM"].ToString();
        //                }
        //                //LOGService.WriteOperateLogT(string.Format("2号请求成功"), "立库请求巷道", ip);


        //            }
        //            else
        //            {
        //                lane2 = "-1";

        //            }

        //        }
        //        if (lane1 == "1")
        //        {
        //            lane1 = "1001";
        //        }
        //        else if (lane1 == "2")
        //        {
        //            lane1 = "1002";
        //        }
        //        else if (lane1 == "3")
        //        {
        //            lane1 = "1003";
        //        }
        //        else if (lane1 == "4")
        //        {
        //            lane1 = "1004";
        //        }

        //        if (lane2 == "1")
        //        {
        //            lane2 = "1001";
        //        }
        //        else if (lane2 == "2")
        //        {
        //            lane2 = "1002";
        //        }
        //        else if (lane2 == "3")
        //        {
        //            lane2 = "1003";
        //        }
        //        else if (lane2 == "4")
        //        {
        //            lane2 = "1004";
        //        }



        //        #endregion
        //        reslutJson = new ReslutJson()
        //        {
        //            success = true,
        //            msg = "WCS请求成功!",
        //            data = new Dictionary<string, string>() {
        //                                            {"laneone", lane1 },
        //                                            {"lanetwo", lane2 }
        //                                        }
        //        };
        //        LOGService.WriteWcsRequestLog(context, json, reslutJson.msg + lane1 + lane2, "入库分配巷道");
        //    }
        //    catch (Exception ex)
        //    {
        //        reslutJson = new ReslutJson()
        //        {
        //            success = false,
        //            msg = ex.Message,
        //        };
        //        LOGService.WriteExceptionLog(ex);
        //        LOGService.WriteWcsRequestLog(context, json, "巷道请求失败：" + ex, "入库分配巷道");
        //    }

        //    // LOGService.WriteOperateLogT(string.Format("1巷道请求成功"), "立库请求巷道", ip);
        //    return reslutJson;


        //}

        /// <summary>
        /// sap请求物料空盘或满盘出库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutLYCodeJson lygetcodelist(JObject json, string ip)
        {
            ReslutLYCodeJson reslutJson = new ReslutLYCodeJson();
            JArray hobbiesArray = (JArray)json["QRCodeList"];
            List<string> hobbies = new List<string>();
            List<CodeResult> result = new List<CodeResult>();
            try
            {

                //JToken data = json["dictionary"];
                //list列表
                // 提取List数据


                foreach (JToken hobby in hobbiesArray)
                {
                    hobbies.Add(hobby.ToString().Trim());
                }
                string sql = "";
                foreach (var item in hobbies)
                {
                    sql = @"
									SELECT  b.ITEMCODE  from wms_bill_putinbindingheader a INNER JOIN wms_bill_putinbindingbody  b on a.ID=b.HEADERID  WHERE a.CONTAINER=@CONTAINER   and  a.STATE=2 ORDER BY a.CREATETIME  DESC
									";
                    var glist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                                {

                                    new SugarParameter("@CONTAINER",item)

                                });
                    CodeResult s = new CodeResult();

                    if (glist != null && glist.Rows.Count > 0)
                    {
                        s.QRCode = item;
                        s.Content = glist.Rows[0][0].ToString();
                    }
                    else
                    {
                        s.QRCode = item;
                        s.Content = "未查询到数据";
                    }
                    result.Add(s);
                }

                reslutJson = new ReslutLYCodeJson()
                {
                    success = true,
                    code = 0,
                    message = "成功",
                    result = result
                };
                LOGService.WriteWcsRequestLog(context, json, reslutJson.message, "二维码查询接口");
            }
            catch (Exception ex)
            {

                reslutJson = new ReslutLYCodeJson()
                {
                    success = false,
                    code = 1,
                    message = "查询条码异常",
                    result = result
                };
                LOGService.WriteOperateLogF(ex.ToString(), "查询条码", ip);
                LOGService.WriteWcsRequestLog(context, json, reslutJson.message, "二维码查询接口");
            }
            return reslutJson;
        }

        /// <summary>
        /// 备刀机查询小刀组盘出库托盘内盒子及工单信息
        /// </summary>
        public ReslutKnifePalletJson lygetpalletoutboundinfo(JObject json, string ip)
        {
            const string logTitle = "备刀机查询组盘出库托盘信息";
            ReslutKnifePalletJson reslutJson;
            try
            {
                string pallet = json["pallet"]?.ToString().Trim() ?? string.Empty;
                string container = json["container"]?.ToString().Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(pallet) && string.IsNullOrEmpty(container))
                {
                    reslutJson = new ReslutKnifePalletJson
                    {
                        Success = false,
                        Code = 1,
                        Message = "参数 pallet 或 container 至少填写一项",
                        Result = null
                    };
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, logTitle);
                    return reslutJson;
                }

                string batchPallet = string.Empty;
                string batchOrderInfo = string.Empty;
                if (!string.IsNullOrEmpty(pallet))
                {
                    string batchSql = @"
SELECT PALLET, ORDERINFO FROM wms_stocker_task
WHERE PALLET = @PALLET AND TYPE = 2 AND ENDTAXI = '1002' AND STATE >= 2
  AND PALLET IS NOT NULL AND PALLET != ''
ORDER BY LASTEDITTIME DESC, CREATETIME DESC
LIMIT 1";
                    DataTable batchDt = dbcontext.Db.Ado.GetDataTable(batchSql, new List<SugarParameter>
                    {
                        new SugarParameter("@PALLET", pallet)
                    });
                    if (batchDt != null && batchDt.Rows.Count > 0)
                    {
                        batchPallet = batchDt.Rows[0]["PALLET"]?.ToString() ?? string.Empty;
                        batchOrderInfo = batchDt.Rows[0]["ORDERINFO"]?.ToString() ?? string.Empty;
                    }
                }
                else
                {
                    string batchSql = @"
SELECT PALLET, ORDERINFO FROM wms_stocker_task
WHERE CONTAINER = @CONTAINER AND TYPE = 2 AND ENDTAXI = '1002' AND STATE >= 2
ORDER BY LASTEDITTIME DESC, CREATETIME DESC
LIMIT 1";
                    DataTable batchDt = dbcontext.Db.Ado.GetDataTable(batchSql, new List<SugarParameter>
                    {
                        new SugarParameter("@CONTAINER", container)
                    });
                    if (batchDt != null && batchDt.Rows.Count > 0)
                    {
                        batchPallet = batchDt.Rows[0]["PALLET"]?.ToString() ?? string.Empty;
                        batchOrderInfo = batchDt.Rows[0]["ORDERINFO"]?.ToString() ?? string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(batchOrderInfo))
                {
                    reslutJson = new ReslutKnifePalletJson
                    {
                        Success = false,
                        Code = 1,
                        Message = "未查询到组盘出库数据",
                        Result = null
                    };
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, logTitle);
                    return reslutJson;
                }

                // 托盘会重复使用，仅返回最新一次组盘批次（PALLET + ORDERINFO）
                string sql = @"
SELECT t.CONTAINER, t.SOURCEORDER, t.WORKNAME, t.PALLET, h.ORDERID
FROM wms_stocker_task t
LEFT JOIN wms_bill_putoutheader h
    ON h.CONTAINER = t.CONTAINER AND h.SOURCEORDER = t.SOURCEORDER
    AND h.REMARK = '组盘出库' AND h.TARGET = '1002'
WHERE t.TYPE = 2 AND t.ENDTAXI = '1002' AND t.STATE >= 2
  AND t.ORDERINFO = @ORDERINFO";
                var parameters = new List<SugarParameter>
                {
                    new SugarParameter("@ORDERINFO", batchOrderInfo)
                };
                if (!string.IsNullOrEmpty(batchPallet))
                {
                    sql += " AND t.PALLET = @PALLET";
                    parameters.Add(new SugarParameter("@PALLET", batchPallet));
                }
                else
                {
                    sql += " AND t.CONTAINER = @CONTAINER";
                    parameters.Add(new SugarParameter("@CONTAINER", container));
                }
                sql += " ORDER BY t.SKUSEQ ASC, t.CONTAINER ASC";

                DataTable dt = dbcontext.Db.Ado.GetDataTable(sql, parameters);
                if (dt == null || dt.Rows.Count == 0)
                {
                    reslutJson = new ReslutKnifePalletJson
                    {
                        Success = false,
                        Code = 1,
                        Message = "未查询到组盘出库数据",
                        Result = null
                    };
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, logTitle);
                    return reslutJson;
                }

                var boxes = new List<KnifePalletBoxInfo>();
                string workname = string.Empty;
                string orderId = string.Empty;
                string resultPallet = !string.IsNullOrEmpty(batchPallet) ? batchPallet : pallet;
                foreach (DataRow row in dt.Rows)
                {
                    if (string.IsNullOrEmpty(workname))
                    {
                        workname = row["WORKNAME"]?.ToString() ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(orderId))
                    {
                        orderId = row["ORDERID"]?.ToString() ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(resultPallet))
                    {
                        resultPallet = row["PALLET"]?.ToString() ?? string.Empty;
                    }
                    boxes.Add(new KnifePalletBoxInfo
                    {
                        container = row["CONTAINER"]?.ToString() ?? string.Empty,
                        specc = row["SOURCEORDER"]?.ToString() ?? string.Empty
                    });
                }

                if (string.IsNullOrEmpty(workname))
                {
                    workname = orderId;
                }

                reslutJson = new ReslutKnifePalletJson
                {
                    Success = true,
                    Code = 0,
                    Message = "查询成功",
                    Result = new KnifePalletOutboundResult
                    {
                        workname = workname,
                        orderId = orderId,
                        pallet = resultPallet,
                        boxCount = boxes.Count,
                        boxes = boxes
                    }
                };
                LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, logTitle);
            }
            catch (Exception ex)
            {
                reslutJson = new ReslutKnifePalletJson
                {
                    Success = false,
                    Code = 1,
                    Message = "查询异常：" + ex.Message,
                    Result = null
                };
                LOGService.WriteOperateLogF(ex.ToString(), logTitle, ip);
                LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, logTitle);
            }
            return reslutJson;
        }

        private static bool TryParseGrindSequence(string sourceOrder, out string specBeforeGrind, out int targetMc)
        {
            specBeforeGrind = string.Empty;
            targetMc = 0;
            if (string.IsNullOrWhiteSpace(sourceOrder))
            {
                return false;
            }

            sourceOrder = sourceOrder.Trim();
            Match match = Regex.Match(sourceOrder, @"\*M(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                targetMc = int.Parse(match.Groups[1].Value);
                if (targetMc < 1)
                {
                    return false;
                }

                specBeforeGrind = sourceOrder.Substring(0, match.Index);
                return true;
            }

            char lastChar = sourceOrder[sourceOrder.Length - 1];
            if (!char.IsDigit(lastChar))
            {
                return false;
            }

            targetMc = lastChar - '0';
            if (targetMc < 1 || targetMc > 9)
            {
                return false;
            }

            specBeforeGrind = sourceOrder.Substring(0, sourceOrder.Length - 1);
            return true;
        }

        private int CountAvailableOutboundLocations(string sourceOrder)
        {
            const string sql = @"
SELECT COUNT(*)
FROM wms_base_location
WHERE SOURCEORDER = @SOURCEORDER
  AND ENABLED = 1
  AND OUTENABLED = 1
  AND CONTAINER != ''
  AND ISAVIALABLE = 0";
            object scalar = dbcontext.Db.Ado.GetScalar(sql, new SugarParameter("@SOURCEORDER", sourceOrder));
            return scalar == null ? 0 : Convert.ToInt32(scalar);
        }

        /// <summary>
        /// 按目标磨次优先、不足则依次降级(M5→M4→…→M1)统计可用库存，不允许使用更高磨次。
        /// </summary>
        private int CountAvailableOutboundByGrindFallback(string requestedSourceOrder)
        {
            requestedSourceOrder = requestedSourceOrder?.Trim() ?? string.Empty;
            if (TryParseGrindSequence(requestedSourceOrder, out _, out int targetMc))
            {
                const string sql = @"
SELECT COUNT(*)
FROM wms_base_location
WHERE LEFT(SOURCEORDER, CHAR_LENGTH(@req) - 1) = LEFT(@req, CHAR_LENGTH(@req) - 1)
  AND SOURCEORDER REGEXP '[0-9]$'
  AND CAST(RIGHT(SOURCEORDER, 1) AS UNSIGNED) <= @targetMc
  AND ENABLED = 1
  AND OUTENABLED = 1
  AND CONTAINER != ''
  AND ISAVIALABLE = 0";
                object scalar = dbcontext.Db.Ado.GetScalar(sql, new List<SugarParameter>
                {
                    new SugarParameter("@req", requestedSourceOrder),
                    new SugarParameter("@targetMc", targetMc)
                });
                return scalar == null ? 0 : Convert.ToInt32(scalar);
            }

            return CountAvailableOutboundLocations(requestedSourceOrder);
        }

        private DataTable SelectAvailableOutboundLocations(string sourceOrder, int limit)
        {
            if (limit <= 0)
            {
                return dbcontext.Db.Ado.GetDataTable(@"
SELECT LOCATIONCODE, CONTAINER, SOURCEORDER, TUNNELNUM
FROM wms_base_location
WHERE 1 = 0");
            }

            string sql = $@"
SELECT LOCATIONCODE, CONTAINER, SOURCEORDER, TUNNELNUM
FROM wms_base_location
WHERE SOURCEORDER = @SOURCEORDER
  AND ENABLED = 1
  AND OUTENABLED = 1
  AND CONTAINER != ''
  AND ISAVIALABLE = 0
ORDER BY LASTEDITTIME ASC, LOCATIONCODE ASC
LIMIT {limit}";
            return dbcontext.Db.Ado.GetDataTable(sql, new SugarParameter("@SOURCEORDER", sourceOrder));
        }

        /// <summary>
        /// 按目标磨次优先、不足则依次降级(M5→M4→…→M1)选取库位，各磨次内 FIFO。
        /// </summary>
        private DataTable SelectAvailableOutboundByGrindFallback(string requestedSourceOrder, int quantity)
        {
            requestedSourceOrder = requestedSourceOrder?.Trim() ?? string.Empty;
            if (quantity <= 0)
            {
                return SelectAvailableOutboundLocations(string.Empty, 0);
            }

            if (TryParseGrindSequence(requestedSourceOrder, out _, out int targetMc))
            {
                string sql = $@"
SELECT LOCATIONCODE, CONTAINER, SOURCEORDER, TUNNELNUM
FROM wms_base_location
WHERE LEFT(SOURCEORDER, CHAR_LENGTH(@req) - 1) = LEFT(@req, CHAR_LENGTH(@req) - 1)
  AND SOURCEORDER REGEXP '[0-9]$'
  AND CAST(RIGHT(SOURCEORDER, 1) AS UNSIGNED) <= @targetMc
  AND ENABLED = 1
  AND OUTENABLED = 1
  AND CONTAINER != ''
  AND ISAVIALABLE = 0
ORDER BY CAST(RIGHT(SOURCEORDER, 1) AS UNSIGNED) DESC, LASTEDITTIME ASC, LOCATIONCODE ASC
LIMIT {quantity}";
                return dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>
                {
                    new SugarParameter("@req", requestedSourceOrder),
                    new SugarParameter("@targetMc", targetMc)
                });
            }

            return SelectAvailableOutboundLocations(requestedSourceOrder, quantity);
        }

        private static bool IsValidSmallDrillSets(int sets)
        {
            return (sets >= 1 && sets <= 6) || (sets >= 12 && sets % 12 == 0);
        }

        private static Dictionary<string, int> GetInvalidSmallDrillQuantities(Dictionary<string, int> request, int sets)
        {
            var invalid = new Dictionary<string, int>();
            foreach (var item in request)
            {
                if (item.Value <= 0 || item.Value % sets != 0)
                {
                    invalid.Add(item.Key, item.Value);
                }
            }
            return invalid;
        }

        private static ReslutKnifeJson BuildPutoutboundValidateFail(string workname, string message, Dictionary<string, int> dictionary)
        {
            return new ReslutKnifeJson()
            {
                Success = false,
                Code = 0,
                Message = message,
                Result = new ReslutresJson
                {
                    workname = workname,
                    sets = dictionary == null ? 0 : dictionary.Count,
                    dictionary = dictionary
                }
            };
        }

        /// <summary>
        /// 本盘应装盒数。normalTotal 是当前规格未标异常盒数（出库单头 STATE≠5，含已装盘和换料补配）。
        /// 一台（sets≤6）：每盘最多 12，尾数单独一盘。
        /// 两台（sets大于6，现场最多 12）：未标异常总数正好 12 时拆成两盘各 6；
        /// 总数大于 12 时仍每盘最多 12，剩下的尾数单独一盘。已装走 12 后再剩 12，不再对半分。
        /// </summary>
        private static int GetNextSmallDrillPackingQty(int sets, int normalTotal, int remaining)
        {
            if (remaining <= 0 || sets <= 0)
            {
                return 0;
            }

            bool twoMachines = sets > 6;
            if (twoMachines && normalTotal == 12)
            {
                return Math.Min(6, remaining);
            }

            return Math.Min(12, remaining);
        }

        private static string BuildSmallDrillOrderInfo(string orderId, string spec, int sets, int skuSeq)
        {
            return orderId + "-" + spec + "-" + sets + "-" + skuSeq;
        }

        private static bool TryParseSmallDrillOrderInfo(string orderInfo, out string orderId, out string spec, out int sets, out int skuSeq)
        {
            orderId = string.Empty;
            spec = string.Empty;
            sets = 0;
            skuSeq = 0;
            if (string.IsNullOrWhiteSpace(orderInfo))
            {
                return false;
            }
            string[] parts = orderInfo.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                return false;
            }
            orderId = parts[0];
            spec = string.Join("-", parts.Skip(1).Take(parts.Length - 3));
            if (!int.TryParse(parts[parts.Length - 2], out sets))
            {
                return false;
            }
            string skuPart = parts[parts.Length - 1];
            int dot = skuPart.IndexOf('.');
            if (dot >= 0)
            {
                skuPart = skuPart.Substring(0, dot);
            }
            return int.TryParse(skuPart, out skuSeq) && !string.IsNullOrEmpty(orderId);
        }

        /// <summary>
        /// 小钻出库-1、申请出库任务
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutKnifeJson putoutbound(JObject json, string ip)
        {
            ReslutKnifeJson reslutJson = new ReslutKnifeJson();

            try
            {
                string orderId = json["orderId"].ToString();
                string workname = json["workname"].ToString();
                int sets = Convert.ToInt32(json["sets"]);
                JObject dicObj = json["dictionary"] as JObject;

                // ===== 单针改造：可选机台号（备刀机请求带 machineno 时写入 OUTMACHINENO，不带则不动） =====
                string machineno = json["machineno"] != null ? json["machineno"].ToString().Trim() : null;


                Dictionary<string, int> request = dicObj.ToObject<Dictionary<string, int>>();
                if (!IsValidSmallDrillSets(sets))
                {
                    reslutJson = BuildPutoutboundValidateFail(workname, "sets必须为1-6或12的倍数", request);
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, "备刀向WMS请求小钻出库");
                    return reslutJson;
                }

                Dictionary<string, int> invalidQty = GetInvalidSmallDrillQuantities(request, sets);
                if (invalidQty.Count > 0)
                {
                    reslutJson = BuildPutoutboundValidateFail(workname, "规格数量必须是sets的倍数", invalidQty);
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, "备刀向WMS请求小钻出库");
                    return reslutJson;
                }

                dbcontext.Db.Ado.BeginTran();
                #region 1.库存检查

                Dictionary<string, int> newdir =
                    new Dictionary<string, int>();


                foreach (var item in request)
                {
                    string sku = item.Key;
                    int qty = item.Value;

                    int stock = CountAvailableOutboundByGrindFallback(sku);

                    if (stock < qty)
                    {
                        newdir.Add(sku, qty - stock);
                    }

                }

                //库存不足直接返回
                if (newdir.Count > 0)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    LOGService.WriteWcsRequestLog(context, json, "库存不足", "备刀向WMS请求小钻出库");

                    return new ReslutKnifeJson()
                    {
                        Success = false,
                        Code = 0,
                        Message = "库存不足",
                        Result = new ReslutresJson
                        {
                            workname = workname,
                            sets = newdir.Count,
                            dictionary = newdir
                        }
                    };
                }
                List<QRCodeList> qRCodeLists = new List<QRCodeList>();
                #endregion
                //查询是否有未完成的小钻出库任务，如果有则把出库任务设为预取模式，避免堆垛机任务阻塞
                string sql = @"SELECT * FROM wms_stocker_task WHERE TYPE = '2' AND ENDTAXI='1002' AND STATE <=2";
                bool hasPendingTasks = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()).Rows.Count > 0;
                if (hasPendingTasks)
                {
                    LOGService.WriteWcsRequestLog(context, json, "存在未完成的小钻出库任务，此工单设为等待状态，等待上一工单完成", "备刀向WMS请求小钻出库");
                }


                #region 2.生成任务
                int skuSeq = 1;
                foreach (var item in request)
                {
                    string sku = item.Key;
                    int qty = item.Value;

                    /*
                     查询实际库存托盘：优先目标磨次，不足则 M5→M4→…→M1 降级
                    */
                    DataTable list = SelectAvailableOutboundByGrindFallback(sku, qty);
                    for (int i = 0; i < list.Rows.Count; i++)
                    {
                        string actualSourceOrder = list.Rows[i]["SOURCEORDER"]?.ToString()?.Trim() ?? sku;

                        string location = list.Rows[i]["LOCATIONCODE"].ToString();
                        string container = list.Rows[i]["CONTAINER"].ToString();

                        //锁定库存
                        string updateSql = @"UPDATE wms_base_location SET OUTENABLED=0 WHERE LOCATIONCODE=@LOCATION";
                        dbcontext.Db.Ado.ExecuteCommand(updateSql, new SugarParameter("@LOCATION", location));

                        string headerId = Guid.NewGuid().ToString("D");
                        //出库单头
                        string headerSql = @"INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER,CONTAINER,CONTAINERTYPE,QUESTFLAG,
                STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME,SKUSEQ,ORDERID)
                VALUES( @ID,'101',@SOURCEORDER,@CONTAINER,'0',0,0,'1002',@LOCATION,1,'组盘出库','WMS',SYSDATE(),@SKUSEQ,@ORDERID)";

                        dbcontext.Db.Ado.ExecuteCommand(headerSql, new List<SugarParameter>(){
                                    new SugarParameter("@ID",headerId),
                                    new SugarParameter("@SOURCEORDER",actualSourceOrder),
                                    new SugarParameter("@CONTAINER",container),
                                    new SugarParameter("@LOCATION",location),
                                    new SugarParameter("@SKUSEQ",skuSeq),
                                    new SugarParameter("@ORDERID",orderId)
                            });

                        //出库明细
                        string bodySql = @"INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE,LOT,TASK_NO_SN,CREATOR,CREATETIME)
                                           VALUES(@ID,@HEADERID,@ITEMCODE,'','','WMS',SYSDATE())";

                        dbcontext.Db.Ado.ExecuteCommand(bodySql, new List<SugarParameter>(){
                                    new SugarParameter("@ID",Guid.NewGuid().ToString("D")),
                                    new SugarParameter("@HEADERID",headerId),
                                    new SugarParameter("@ITEMCODE",actualSourceOrder)
                            });


                        //堆垛机任务
                        string taskId =
                        Guid.NewGuid().ToString("D");
                        string taskSql = @"INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,
                SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME,SKUSEQ,TASKMODE,ORDERINFO,WORKNAME)
                VALUES(@ID,@CODE,'1002',@CONTAINER,@START,2,SYSDATE(),'101',@SOURCEORDER,@PRIORITY,0,SYSDATE(),'WMS','WMS',SYSDATE(),
                @SEQ,@MODE,@ORDERINFO,@WORKNAME)";
                        string mode = (skuSeq == 1 && !hasPendingTasks) ? "NORMAL" : "PRE_FETCH";
                        dbcontext.Db.Ado.ExecuteCommand(taskSql, new List<SugarParameter>(){
                                new SugarParameter("@ID",taskId),
                                new SugarParameter("@CODE",taskId),
                                new SugarParameter("@CONTAINER",container),
                                new SugarParameter("@START",location),
                                new SugarParameter("@SOURCEORDER",actualSourceOrder),
                                new SugarParameter("@PRIORITY",2),
                                new SugarParameter("@SEQ",skuSeq),
                                new SugarParameter("@MODE",mode),
                                new SugarParameter("@ORDERINFO",BuildSmallDrillOrderInfo(orderId, sku, sets, skuSeq)),
                                new SugarParameter("@WORKNAME",workname)
                            });

                        qRCodeLists.Add(new QRCodeList
                        {
                            QRCode = container,
                            Content = actualSourceOrder
                        });

                        // ===== 单针改造：出库建单预占本盒针 AVAILABLE→RESERVED（预占单号=备刀工单orderId；历史盒无针数据自动跳过） =====
                        NeedleService.ReserveContainer(dbcontext.Db, container, orderId, machineno, "备刀机");
                    }
                    skuSeq++;
                }
                #endregion

                dbcontext.Db.Ado.CommitTran();
                reslutJson = new ReslutKnifeJson()
                {
                    Success = true,
                    Code = 0,
                    Message = "出库任务创建成功",
                    Result = new ReslutresJson
                    {
                        workname = workname,
                        sets = request.Count,
                        dictionary = newdir,
                        QRCodeList = qRCodeLists
                    }
                };


            }
            catch (Exception ex)
            {

                dbcontext.Db.Ado.RollbackTran();


                reslutJson = new ReslutKnifeJson()
                {
                    Success = false,
                    Code = 0,
                    Message = "创建出库任务异常:" + ex.Message
                };

                LOGService.WriteOperateLogF(ex.ToString(), "出库", ip);
            }
            LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, "备刀向WMS请求小钻出库");


            return reslutJson;

        }
        /// <summary>
        /// sap请求物料空盘或满盘出库（创建出库单）备刀机请求出库
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutLYJson lysettaskoutbound(JObject json, string ip)
        {
            ReslutLYJson reslutJson = new ReslutLYJson();
            try
            {
                int length = int.Parse(json["length"].ToString());//长度
                                                                  // string dictionary = json["dictionary"].ToString();//
                JToken data = json["dictionary"];
                //list列表
                // 提取List数据
                //JArray hobbiesArray = (JArray)json["BARCODE"];
                //List<string> hobbies = new List<string>();
                //foreach (JToken hobby in hobbiesArray)
                //{
                //	hobbies.Add(hobby.ToString());
                //}											  
                //
                if (length == 0)
                {

                    reslutJson = new ReslutLYJson()
                    {
                        Success = true,
                        Code = 1,
                        Message = "出库单创建成功",
                        Result = new Result { length = 0, dictionary = null }
                    };
                }
                else
                {
                    //错误数据
                    if (length != data.Count())
                    {
                        reslutJson = new ReslutLYJson()
                        {
                            Success = false,
                            Code = 0,
                            Message = "出库单创建失败，length与实际的数据不符",
                            Result = new Result { length = 0, dictionary = null }
                        };
                    }
                    else
                    {
                        Dictionary<string, int> getdir = data.ToObject<Dictionary<string, int>>();
                        Dictionary<string, int> newdir = new Dictionary<string, int>();
                        string sql = "";
                        dbcontext.Db.Ado.BeginTran();

                        //出库已确认不会出现重复下单20260323
                        //第二次
                        foreach (var item in getdir)
                        {
                            string key = item.Key.Trim();
                            int value = item.Value;

                            int tt = CountAvailableOutboundByGrindFallback(key);
                            int use = 0;
                            if (tt >= value)
                            {
                                use = value;
                            }
                            else
                            {
                                newdir.Add(key, value - tt);
                                use = tt;
                            }

                            if (use > 0)
                            {
                                // 按目标磨次优先，不足则 M5→M4→…→M1 降级，各磨次内 FIFO
                                DataTable nowlist = SelectAvailableOutboundByGrindFallback(key, use);
                                //插入数据
                                for (int i = 0; i < nowlist.Rows.Count; i++)
                                {
                                    string locationCode = nowlist.Rows[i]["LOCATIONCODE"].ToString();
                                    string container = nowlist.Rows[i]["CONTAINER"].ToString();
                                    string actualSourceOrder = nowlist.Rows[i]["SOURCEORDER"]?.ToString()?.Trim() ?? string.Empty;
                                    if (string.IsNullOrEmpty(actualSourceOrder))
                                    {
                                        actualSourceOrder = key;
                                    }

                                    //更新库位待出库
                                    sql = @"
											UPDATE wms_base_location  SET OUTENABLED=0 WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
											";
                                    dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                       new SugarParameter("@ZONECODE",101),
                                       new SugarParameter("@LOCATIONCODE",locationCode)

                                    });


                                    string id = Guid.NewGuid().ToString("D");
                                    sql = @"
											INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,
                                    QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                                    VALUES(@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'出库','SAP',sysdate())";
                                    dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>()
                                {

                                    new SugarParameter("@ID",id),
                                    new SugarParameter("@ZONECODE",101),
                                    new SugarParameter("@SOURCEORDER",actualSourceOrder),
                                    new SugarParameter("@CONTAINER",container),
                                    new SugarParameter("@CONTAINERTYPE","0"),
                                    new SugarParameter("@TARGET",1001),
                                    new SugarParameter("@LOCATIONCODE",locationCode)

                                });
                                    string lid = Guid.NewGuid().ToString("D");
                                    string listsql = @"
								INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE, LOT,TASK_NO_SN,CREATOR,CREATETIME)
													   VALUES
													   (@ID,@HEADERID, @ITEMCODE,@LOT,@TASK_NO_SN,'SAP',sysdate()) 
								";
                                    dbcontext.Db.Ado.ExecuteCommand(listsql, new List<SugarParameter>()
                                                    {

                                                        new SugarParameter("@ID",lid),
                                                        new SugarParameter("@HEADERID",id),
                                                        new SugarParameter("@ITEMCODE",actualSourceOrder),
                                                        new SugarParameter("@LOT",""),
														//new SugarParameter("@WEIGHT",WEIGHT),
														new SugarParameter("@TASK_NO_SN","")

                                                    });

                                    string taskid = Guid.NewGuid().ToString("D");
                                    string tasksql = @"
													INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME,SKUSEQ,TASKMODE)
													 VALUES (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,1,0,sysdate(),'WMS','WMS',sysdate(),@SKUSEQ,@TASKMODE)";
                                    dbcontext.Db.Ado.ExecuteCommand(tasksql, new List<SugarParameter>()
                                                                        {

                                                                            new SugarParameter("@ID",taskid),
                                                                            new SugarParameter("@CODE",taskid),
                                                                            new SugarParameter("@ENDTAXI","1001"),
                                                                            new SugarParameter("@CONTAINER",container),
                                                                            new SugarParameter("@STARTTAXI",locationCode),
                                                                            new SugarParameter("@ZONECODE","101"),
                                                                            new SugarParameter("@SOURCEORDER",actualSourceOrder),
                                                                            new SugarParameter("@SKUSEQ",0),
                                                                            new SugarParameter("@TASKMODE","NORMAL")

                                                                        });

                                    // ===== 单针改造：出库建单预占本盒针 AVAILABLE→RESERVED（预占单号=出库单头ID） =====
                                    NeedleService.ReserveContainer(dbcontext.Db, container, id, null, "SAP");

                                }



                            }

                        }
                        dbcontext.Db.Ado.CommitTran();
                        //返回数据
                        reslutJson = new ReslutLYJson()
                        {
                            Success = true,
                            Code = 1,
                            Message = "出库单创建成功!",
                            Result = new Result { length = newdir.Count, dictionary = newdir }
                        };
                        LOGService.WriteWcsRequestLog(context, json, reslutJson.Message + "条数：" + newdir.Count, "备刀向WMS请求出库");


                    }


                }

            }
            catch (Exception ex)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutLYJson()
                {
                    Success = false,
                    Code = 0,
                    Message = "出库单创建失败异常",
                    Result = new Result { }
                };
                LOGService.WriteOperateLogF(ex.ToString(), "出库", ip);
                LOGService.WriteWcsRequestLog(context, json, reslutJson.Message, "备刀向WMS请求出库");
            }
            return reslutJson;
        }
        /// <summary>
        /// 半成品申请库位(单伸位)参数：托盘号、工位、库区、实际重量，返回值：任务号、任务类型、托盘号、目标库位号、起始工位；string  startcode, string container, string zonecode, string realweight
        /// </summary>
        /// <param name="boxNums"></param>
        /// <param name="startPoint"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson grnLocationTranspor(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            string barcode = json["barcode"].ToString().Trim();//
            string start = json["start"].ToString().Trim();//入库口
            int tune = 1;
            if (start == "6100" || start == "6120")
            {
                tune = 1;
            }
            else if (start == "6140" || start == "6160")
            {
                tune = 2;
            }
            else
            {
                tune = 3;
            }
            try

            {
                string sql = "";

                //校验是否已经有对应任务生成
                sql = @"SELECT  * FROM wms_stocker_task  WHERE CONTAINER=@CONTAINER  AND  STATE<2";

                var checkdata = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {
                     new SugarParameter("@CONTAINER",barcode)
                });
                if (checkdata != null && checkdata.Rows.Count > 0)
                {

                    reslutJson = new ReslutJson()
                    {
                        success = false,
                        msg = "重复请求",
                    };
                    //LOGService.WriteOperateLogF(string.Format("原因{0}", e.Message), "托盘立库请求库位", ip);
                }
                else
                {
                    dbcontext.Db.Ado.BeginTran();
                    sql = @"
								SELECT a.ID, a.CONTAINER  ,  b.ITEMCODE   from   wms_bill_putinbindingheader  a  
								INNER JOIN   wms_bill_putinbindingbody  b  on  a.ID=b.HEADERID  
								WHERE  a.CONTAINER=@CONTAINER   and a.QUESTFLAG=0 and a.STATE=0
								";
                    var glist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                    {
                        new SugarParameter("@CONTAINER",barcode)

                    });
                    if (glist != null && glist.Rows.Count > 0)
                    {
                        string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
                        string ID = glist.Rows[0]["ID"].ToString();
                        dbcontext.Db.Ado.BeginTran();
                        //更新库位信息，插入库存，更新入库单信息 ，插入堆垛机任务
                        sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK,SOURCEORDER=@SOURCEORDER  
WHERE  TYPE='ST'    AND ENABLED=1 AND TUNNELNUM=@TUNNELNUM   AND ISAVIALABLE = 1  AND REMARK = ''    AND SOURCEORDER = ''  ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC LIMIT 1
";  //乐观锁解决并发
                        int ss = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

                                                                        new SugarParameter("@REMARK",barcode),
                                                                       new SugarParameter("@TUNNELNUM",tune),
                                                                        new SugarParameter("@SOURCEORDER",ITEMCODE)
                                                                    });
                        sql = @"
SELECT  LOCATIONCODE  FROM wms_base_location  
WHERE  TYPE='ST'  AND ISAVIALABLE=0  AND ENABLED=1 AND REMARK=@REMARK  AND SOURCEORDER=@SOURCEORDER  AND TUNNELNUM=@TUNNELNUM
  ";

                        var inlocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                    {
                                                                      new SugarParameter("@REMARK",barcode),
                                                                       new SugarParameter("@TUNNELNUM",tune),
                                                                        new SugarParameter("@SOURCEORDER",ITEMCODE)
                    });
                        if (inlocation != null && inlocation.Rows.Count > 0)
                        {
                            string LOCATIONCODE = inlocation.Rows[0]["LOCATIONCODE"].ToString();

                            sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
                ;
                            dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
                                                   new SugarParameter("@LOCATIONCODE",LOCATIONCODE),
                                                    new SugarParameter("@ID",ID)

                                                });


                            //插入入库信息
                            string id = Guid.NewGuid().ToString("D");
                            string insertsql = @"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
                       VALUES
                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,1,sysdate(),'WMS','WMS',sysdate()) 
";
                            dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
                    {

                        new SugarParameter("@ID",id),
                        new SugarParameter("@CODE",id),
                        new SugarParameter("@ENDTAXI",LOCATIONCODE),
                        new SugarParameter("@CONTAINER",barcode),
                        new SugarParameter("@STARTTAXI",start),
                        new SugarParameter("@ZONECODE","101"),
                        new SugarParameter("@SOURCEORDER",ITEMCODE)

                    });

                            dbcontext.Db.Ado.CommitTran();

                            //LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", barcode, LOCATIONCODE + "|" + LOCATIONCODE), "立库请求库位", ip);
                            reslutJson = new ReslutJson()
                            {
                                success = true,
                                msg = "WCS请求成功!",
                                data = new Dictionary<string, string>() {
                                                    {"taskId", id },
                                                    {"taskType", "1" },
                                                    { "conTainer", barcode },
                                                    { "startTaxi", start },
                                                    { "endTaxi", LOCATIONCODE }
                                                }
                            };



                        }
                        else
                        {
                            dbcontext.Db.Ado.RollbackTran();
                            reslutJson = new ReslutJson()
                            {
                                success = false

                            };
                            LOGService.WriteOperateLogF(string.Format("原因{0}", "未查询到库位"), "托盘立库请求库位", ip);
                        }

                    }
                    else
                    {
                        dbcontext.Db.Ado.RollbackTran();
                        reslutJson = new ReslutJson()
                        {
                            success = false

                        };
                        LOGService.WriteOperateLogF(string.Format("原因{0}", "未查询到入库清单"), "托盘立库请求库位", ip);
                    }

                }

            }

            catch (Exception e)
            {
                dbcontext.Db.Ado.RollbackTran();
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = e.Message,
                };
                LOGService.WriteOperateLogF(string.Format("原因{0}", e.Message), "托盘立库请求库位", ip);
            }



            return reslutJson;


        }


        public ReslutJson lygrnLocationTranspor(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            // 1. 参数校验（不变，无事务）
            if (json == null)
            {
                reslutJson.success = false;
                reslutJson.msg = "参数不能为空";
                return reslutJson;
            }

            string barcode = string.Empty;
            string start = string.Empty;

            try
            {
                if (json["barcode"] != null)
                    barcode = json["barcode"].ToString().Trim();

                if (json["start"] != null)
                    start = json["start"].ToString().Trim();
            }
            catch
            {
                reslutJson.success = false;
                reslutJson.msg = "参数格式错误";
                return reslutJson;
            }

            if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(start))
            {
                reslutJson.success = false;
                reslutJson.msg = "条码/入库口不能为空";
                return reslutJson;
            }

            // 2. 巷道判断（不变）
            int tune = 3;
            if (start == "6100" || start == "6120")
            {
                tune = 1;
            }
            else if (start == "6140" || start == "6160")
            {
                tune = 2;
            }
            else
            {
                tune = 3;
            }

            // ==============================================
            // ✅ 修改1：增加死锁重试（解决死锁最有效）
            // ==============================================
            int retryCount = 0;
            const int MaxRetry = 3;
            while (retryCount < MaxRetry)
            {
                // ✅ 修改2：事务标记，防止重复回滚
                bool isTransactionOpen = false;
                try
                {
                    // 3. 开启事务（不变）
                    dbcontext.Db.Ado.BeginTran();
                    isTransactionOpen = true;

                    // 4. 防重复请求（不变）
                    string checkSql = @"
SELECT 1 FROM wms_stocker_task WHERE CONTAINER=@CONTAINER AND STATE<2 LIMIT 1";

                    DataTable checkdata = dbcontext.Db.Ado.GetDataTable(checkSql, new List<SugarParameter>()
            {
                new SugarParameter("@CONTAINER", barcode)
            });

                    if (checkdata != null && checkdata.Rows.Count > 0)
                    {
                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran(); // ✅ 修改3：安全回滚
                        reslutJson.success = false;
                        reslutJson.msg = "重复请求";
                        return reslutJson;
                    }

                    // 5. 查询入库单（不变）
                    string bindSql = @"
SELECT a.ID, a.CONTAINER, b.ITEMCODE
FROM wms_bill_putinbindingheader a
INNER JOIN wms_bill_putinbindingbody b ON a.ID=b.HEADERID
WHERE a.CONTAINER=@CONTAINER AND a.QUESTFLAG=0 AND a.STATE=0 LIMIT 1";

                    DataTable glist = dbcontext.Db.Ado.GetDataTable(bindSql, new List<SugarParameter>()
            {
                new SugarParameter("@CONTAINER", barcode)
            });

                    if (glist == null || glist.Rows.Count <= 0)
                    {
                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran(); // ✅ 修改3
                        reslutJson.success = false;
                        reslutJson.msg = "未查询到入库清单";
                        return reslutJson;
                    }

                    string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
                    string ID = glist.Rows[0]["ID"].ToString();

                    // 1. 先查询并锁定一条可用库位（FOR UPDATE 行锁，高并发绝对不重复）
                    string selectLockSql = @"
    SELECT LOCATIONCODE
    FROM wms_base_location
    WHERE TYPE='ST'
      AND ENABLED=1
      AND TUNNELNUM=@TUNNELNUM
      AND ISAVIALABLE=1
      AND REMARK=''
      AND SOURCEORDER=''
    ORDER BY ROWNUM ASC, LAYERNUM ASC, COLUMNNUM ASC
    LIMIT 1
    FOR UPDATE;  -- 关键：行锁，其他事务必须等待
";

                    // 2. 更新锁定的这条库位
                    string updateSql = @"
    UPDATE wms_base_location
    SET ISAVIALABLE = 0,
        REMARK = @REMARK,
        SOURCEORDER = @SOURCEORDER
    WHERE LOCATIONCODE = @LOCATIONCODE;
";


                    // ====================== 第一步：查询并锁定库位 ======================
                    var locationCode = dbcontext.Db.Ado.SqlQuery<string>(selectLockSql, new
                    {
                        TUNNELNUM = tune
                    }).FirstOrDefault();

                    // 没查到可用库位
                    if (string.IsNullOrEmpty(locationCode))
                    {
                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran();
                        reslutJson.success = false;
                        reslutJson.msg = "库位分配失败(无可用库位)";
                        return reslutJson;
                    }

                    // ====================== 第二步：更新这条锁定的库位 ======================
                    int updateRows = dbcontext.Db.Ado.ExecuteCommand(updateSql, new
                    {
                        REMARK = barcode,
                        SOURCEORDER = ITEMCODE,
                        LOCATIONCODE = locationCode
                    });

                    if (updateRows <= 0)
                    {
                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran();
                        reslutJson.success = false;
                        reslutJson.msg = "库位分配失败(更新失败)";
                        return reslutJson;
                    }
                    //                        string updateSql = @"
                    //UPDATE wms_base_location l
                    //INNER JOIN (
                    //    SELECT LOCATIONCODE FROM wms_base_location
                    //    WHERE TYPE='ST' AND ENABLED=1 AND TUNNELNUM=@TUNNELNUM
                    //      AND ISAVIALABLE=1 AND REMARK='' AND SOURCEORDER=''
                    //    ORDER BY ROWNUM ASC, LAYERNUM ASC, COLUMNNUM ASC
                    //    LIMIT 1
                    //) AS temp ON l.LOCATIONCODE = temp.LOCATIONCODE
                    //SET l.ISAVIALABLE=0, l.REMARK=@REMARK, l.SOURCEORDER=@SOURCEORDER";



                    //					int updateRows = dbcontext.Db.Ado.ExecuteCommand(updateSql, new List<SugarParameter>()
                    //            {
                    //                new SugarParameter("@REMARK", barcode),
                    //                new SugarParameter("@TUNNELNUM", tune),
                    //                new SugarParameter("@SOURCEORDER", ITEMCODE)
                    //            });

                    //                    if (updateRows <= 0)
                    //                    {
                    //                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran(); // ✅ 修改3
                    //                        reslutJson.success = false;
                    //                        reslutJson.msg = "库位分配失败(无可用库位/并发冲突)";
                    //                        return reslutJson;
                    //                    }

                    //                    // 查询已分配库位（不变）
                    //                    string locSql = @"
                    //SELECT LOCATIONCODE FROM wms_base_location
                    //WHERE ISAVIALABLE=0 AND REMARK=@REMARK AND SOURCEORDER=@SOURCEORDER AND TUNNELNUM=@TUNNELNUM LIMIT 1";

                    //                    DataTable inlocation = dbcontext.Db.Ado.GetDataTable(locSql, new List<SugarParameter>()
                    //            {
                    //                new SugarParameter("@REMARK", barcode),
                    //                new SugarParameter("@TUNNELNUM", tune),
                    //                new SugarParameter("@SOURCEORDER", ITEMCODE)
                    //            });

                    //                    if (inlocation == null || inlocation.Rows.Count <= 0)
                    //                    {
                    //                        if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran(); // ✅ 修改3
                    //                        reslutJson.success = false;
                    //                        reslutJson.msg = "未查询到库位";
                    //                        return reslutJson;
                    //                    }

                    string LOCATIONCODE = locationCode;

                    // 更新入库单（不变）
                    string updateHeader = @"
UPDATE wms_bill_putinbindingheader
SET QUESTFLAG=1,QUESTTIME=NOW(),LOCATIONCODE=@LOCATIONCODE,STATE=1
WHERE ID=@ID";

                    dbcontext.Db.Ado.ExecuteCommand(updateHeader, new List<SugarParameter>()
            {
                new SugarParameter("@LOCATIONCODE", LOCATIONCODE),
                new SugarParameter("@ID", ID)
            });

                    // 生成任务（不变）
                    string taskId = Guid.NewGuid().ToString("D");
                    string insertSql = @"
INSERT INTO wms_stocker_task
(ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
VALUES
(@ID,@CODE,@ENDTAXI,@CONTAINER,@STARTTAXI,1,NOW(),@ZONECODE,@SOURCEORDER,0,1,NOW(),'WMS','WMS',NOW())";

                    dbcontext.Db.Ado.ExecuteCommand(insertSql, new List<SugarParameter>()
            {
                new SugarParameter("@ID", taskId),
                new SugarParameter("@CODE", taskId),
                new SugarParameter("@ENDTAXI", LOCATIONCODE),
                new SugarParameter("@CONTAINER", barcode),
                new SugarParameter("@STARTTAXI", start),
                new SugarParameter("@ZONECODE", "101"),
                new SugarParameter("@SOURCEORDER", ITEMCODE)
            });

                    // 提交事务
                    dbcontext.Db.Ado.CommitTran();
                    isTransactionOpen = false;

                    // 返回结果
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("taskId", taskId);
                    data.Add("taskType", "1");
                    data.Add("conTainer", barcode);
                    data.Add("startTaxi", start);
                    data.Add("endTaxi", LOCATIONCODE);

                    reslutJson.success = true;
                    reslutJson.msg = "WCS请求成功!";
                    reslutJson.data = data;
                    return reslutJson;
                }
                catch (Exception e)
                {
                    // ✅ 修改6：捕获死锁异常，自动重试
                    if (e.Message.Contains("Deadlock") || e.Message.Contains("死锁"))
                    {
                        retryCount++;
                        if (isTransactionOpen)
                        {
                            try { dbcontext.Db.Ado.RollbackTran(); } catch { }
                        }
                        System.Threading.Thread.Sleep(50 * retryCount); // 重试等待
                        continue;
                    }

                    // 其他异常直接返回
                    if (isTransactionOpen)
                    {
                        try { dbcontext.Db.Ado.RollbackTran(); } catch { }
                    }
                    reslutJson.success = false;
                    reslutJson.msg = e.Message;
                    LOGService.WriteOperateLogF("原因：" + e.Message, "托盘立库请求库位", ip);
                    return reslutJson;
                }
            }

            // 重试次数用完
            reslutJson.success = false;
            reslutJson.msg = "库位分配失败，并发死锁多次重试无效";
            LOGService.WriteOperateLogF("死锁重试耗尽", "托盘立库请求库位", ip);
            return reslutJson;
        }

        /// <summary>
        /// 获取出库任务的终点站信息（巷道出库口），WCS调用，参数：托盘条码，返回值：终点站信息
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson GetPutoutEndTaxi(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            try
            {
                string container = json["container"].ToString().Trim();//
                string sql = @"SELECT ENDTAXI FROM wms_stocker_task WHERE TYPE='2' AND CONTAINER=@CONTAINER ORDER BY CREATETIME DESC";
                var endTaxi = dbcontext.Db.Ado.SqlQuery<string>(sql, new List<SugarParameter>()
                {
                    new SugarParameter("@CONTAINER", container)
                }).FirstOrDefault();
                if (string.IsNullOrEmpty(endTaxi))
                {
                    reslutJson.success = false;
                    reslutJson.msg = "未查询到任务";
                    LOGService.WriteWcsRequestLog(context, json, "未查询到任务", "获取出库任务终点");
                    return reslutJson;
                }

                reslutJson.success = true;
                reslutJson.msg = "查询成功";
                reslutJson.data = new Dictionary<string, string>() { { "endTaxi", endTaxi } };
                LOGService.WriteWcsRequestLog(context, json, "查询成功", "获取出库任务终点");
                return reslutJson;
            }
            catch (Exception e)
            {
                reslutJson.success = false;
                reslutJson.msg = e.Message;
                LOGService.WriteOperateLogF("原因：" + e.Message, "查询任务终点", ip);
                return reslutJson;
            }
        }

        //public ReslutJson UpdateContainer(JObject json, string ip)
        //{
        //    ReslutJson reslutJson = new ReslutJson();
        //    try
        //    {
        //        string newCode = json["newCode"].ToString().Trim();
        //        string oldCode = json["oldCode"].ToString().Trim();
        //        string sql = @"UPDATE wms_bill_putinbindingheader SET CONTAINER=@CONTAINER,LASTEDITTIME = sysdate() WHERE SOURCEORDER=@SOURCEORDER AND STATE<2";
        //        int rows = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>()
        //        {
        //            new SugarParameter("@SOURCEORDER", oldCode),
        //            new SugarParameter("@CONTAINER", newCode)
        //        });
        //        if (rows <= 0)
        //        {
        //            reslutJson.success = false;
        //            reslutJson.msg = "未更新到任务";
        //            LOGService.WriteWcsRequestLog(context, json, "未更新到任务", "翻盒机修改容器");
        //            return reslutJson;
        //        }
        //        reslutJson.success = true;
        //        reslutJson.msg = "更新成功";
        //        LOGService.WriteWcsRequestLog(context, json, "更新成功", "翻盒机修改容器");
        //        return reslutJson;
        //    }
        //    catch (Exception e)
        //    {
        //        reslutJson.success = false;
        //        reslutJson.msg = e.Message;
        //        LOGService.WriteOperateLogF("原因：" + e.Message, "更新任务SourceOrder", ip);
        //        return reslutJson;
        //    }
        //}

        public void updateItemcode()
        {
            JObject json = new JObject();
            LOGService.WriteWcsRequestLog(context, json, "开始时间" + new DateTime().ToString("G"), "开始修改规格信息");
            //判断规格是否为空，规格为空不做处理，截取规格的第一位小于0.55并大于0.5
            List<wms_bill_putinbindingheader> list = servicespec.Query(s => s.Specifications != null && s.Specifications != "").ToList();

            var result = list
                .GroupBy(x => x.CONTAINER)
                .Where(g => g.Count() >= 2)
                .Select(g => g.OrderByDescending(x => x.CREATETIME).First())
                .ToList();
            //var result = list.Where(x =>
            //{
            //    if (string.IsNullOrEmpty(x.Specifications))
            //        return false;

            //    var firstValue = x.Specifications.Split('-')[0];

            //    decimal value;

            //    return decimal.TryParse(firstValue, out value)
            //           && value >= 0.5m
            //           && value <= 0.55m;

            //}).ToList();
            List<string> strings1 = new List<string>();
            List<string> strings2 = new List<string>();
            foreach (var item in result)
            {
                string[] parts = item.Specifications.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string resultstr = parts[0].ToString();
                string type = parts[1].ToString();
                string series = parts[2].ToString();
                string SUPPLIERID = parts[3].ToString();
                string blade = parts[4].ToString();
                int mc = int.Parse(parts[5]);
                //转换钻咀二维码
                string code = ConvertCode(item.Specifications);
                //判断直径(resultstr)小于0.5，需要拼接为<厂商_系列-直径(类型)/刃长*M磨次>格式 DT_UCN-0.500B/4.0*M5
                if (double.Parse(resultstr) < 0.5)
                {
                    code = $"{SUPPLIERID}_{series}-{code}/{blade}*M{mc}";
                }
                else if (type == "SD" || type == "EA")      //槽刀需要加类型以及刃长0.703/7.0*M5
                {
                    code = $"{code}/{blade}*M{mc}";
                }
                else
                {
                    code = resultstr + $"*M{mc}";
                }
                //wms_bill_putinbindingbody bodymodel = servicespecbody.Query(x => x.HEADERID == item.ID).FirstOrDefault();
                //if (bodymodel == null)
                //{
                //    continue;
                //}
                //else
                //{
                //    bodymodel.ITEMCODE = code;
                //    var results = servicespecbody.Update(bodymodel);
                //}
                List<wms_base_location> locationsList = service.Query(x => x.CONTAINER == item.CONTAINER).ToList();
                wms_base_location locations = locationsList.FirstOrDefault();
                if (locations != null && locations.SOURCEORDER != code)
                {
                    locations.SOURCEORDER = code;
                    var results1 = service.Update(locations);
                    strings1.Add(item.CONTAINER);
                    //strings1.Add(item.CONTAINER);
                    //wms_stocker_task task = servicetask.Query(x => x.CONTAINER == item.CONTAINER && x.TYPE==1).OrderByDescending(s => s.CREATETIME).FirstOrDefault();
                    //if (task.SOURCEORDER==code)
                    //{
                    //locations.SOURCEORDER = task.SOURCEORDER;
                    //var results1 = service.Update(locations);
                    //}
                    //else
                    //{
                    //    strings1.Add(item.CONTAINER);
                    //}
                }

                List<wms_bill_inventorybook> inventorybooklist = serviceinventorybook.Query(x => x.BARCODE == item.CONTAINER).OrderByDescending(s => s.CREATETIME).ToList();
                if (inventorybooklist.Count() == 1)
                {
                    wms_bill_inventorybook inventorybook = inventorybooklist.FirstOrDefault();
                    if (inventorybook.SOURCEORDER != code || inventorybook.ITEMCODE != code)
                    {
                        inventorybook.SOURCEORDER = code;
                        inventorybook.ITEMCODE = code;
                        var results2 = serviceinventorybook.Update(inventorybook);
                        strings2.Add(item.CONTAINER);
                    }
                }
                if (inventorybooklist.Count() >= 2)
                {
                    wms_bill_inventorybook inventorybook = inventorybooklist.OrderBy(s => s.CREATETIME).FirstOrDefault();
                    var results2 = serviceinventorybook.Delete(inventorybook);
                    strings2.Add(item.CONTAINER);
                }

            }
            if (strings1.Count >= 1 || strings2.Count() >= 1)
            {
                LOGService.WriteWcsRequestLog(context, json, "库位异常数据：" + string.Join(",", strings1) + "台账异常数据：" + string.Join(",", strings2), "修改规格结束");
            }
            else
            {

                LOGService.WriteWcsRequestLog(context, json, "结束时间" + new DateTime().ToString("G"), "修改规格结束");
            }
        }

        /// <summary>
        /// 当前规格未标异常盒数：同一 orderId + skuSeq、终点 1002、出库单头 STATE≠5。
        /// 已装盘（STATE=3）和换料补配计入，已标异常的旧盒不计入。
        /// </summary>
        private int CountNormalSmallDrillBoxes(string orderId, int skuSeq)
        {
            return serviceputoutheader.Query(s =>
                s.ORDERID == orderId
                && s.SKUSEQ == skuSeq
                && s.TARGET == "1002"
                && s.STATE != 5).Count();
        }

        /// <summary>
        /// 小钻组盘前：按当前规格未标异常总数和剩余未装盘任务计算本盘应装数量，供 WCS 写入 PLC。
        /// </summary>
        public ReslutJson GetPackingQty(JObject json)
        {
            const string logTitle = "WCS查询小钻装盘数量";
            string orderInfo = json["orderInfo"]?.ToString()?.Trim() ?? string.Empty;
            string orderId;
            string specc;
            int sets;
            int skuSeq;
            if (!TryParseSmallDrillOrderInfo(orderInfo, out orderId, out specc, out sets, out skuSeq))
            {
                var bad = new ReslutJson()
                {
                    success = false,
                    msg = "orderInfo格式不正确：" + orderInfo
                };
                LOGService.WriteWcsRequestLog(context, json, bad.msg, logTitle);
                return bad;
            }

            try
            {
                int candidateCount;
                JArray taskEvaluations;
                var pendingTasks = QuerySmallDrillPackingTasksForQty(orderId, orderInfo, skuSeq, out candidateCount, out taskEvaluations);
                int remaining = pendingTasks.Count;
                int normalTotal = CountNormalSmallDrillBoxes(orderId, skuSeq);
                int plating = GetNextSmallDrillPackingQty(sets, normalTotal, remaining);
                var result = new ReslutJson()
                {
                    success = true,
                    msg = remaining == 0 ? "没有待装盘任务" : "查询成功",
                    data = new SmallDrillPackingQtyResult
                    {
                        orderInfo = orderInfo,
                        orderId = orderId,
                        specc = specc,
                        sets = sets,
                        skuSeq = skuSeq,
                        remaining = remaining,
                        plating = plating,
                        candidateCount = candidateCount,
                        taskEvaluations = taskEvaluations
                    }
                };
                LOGService.WriteWcsRequestLog(
                    context,
                    json,
                    result.msg + " normalTotal=" + normalTotal + " remaining=" + remaining + " plating=" + plating + " candidateCount=" + candidateCount,
                    logTitle);
                return result;
            }
            catch (Exception ex)
            {
                var result = new ReslutJson()
                {
                    success = false,
                    msg = "查询装盘数量失败：" + ex.Message
                };
                LOGService.WriteWcsRequestLog(context, json, result.msg, logTitle);
                return result;
            }
        }

        /// <summary>
        /// 查询小钻待装盘任务：先按 SKUSEQ+STATE 查库，再在内存中过滤，避免 SqlSugar 对 StartsWith 翻译异常。
        /// </summary>
        private List<wms_stocker_task> QuerySmallDrillPackingTasksForQty(
            string orderId,
            string orderInfo,
            int skuSeq,
            out int candidateCount,
            out JArray taskEvaluations)
        {
            var candidates = servicetask.Query(s =>
                s.SKUSEQ == skuSeq
                && s.STATE >= 0
                && s.STATE <= 2).ToList();
            candidateCount = candidates.Count;
            taskEvaluations = new JArray(candidates.Select(t => BuildPackingQtyTaskEval(t, orderId, orderInfo, skuSeq)));

            return candidates
                .Where(t => IsSmallDrillPackingTask(t, orderId, orderInfo, skuSeq))
                .ToList();
        }

        private static JObject BuildPackingQtyTaskEval(wms_stocker_task task, string orderId, string orderInfo, int skuSeq)
        {
            string orderInfoValue = (task.ORDERINFO ?? string.Empty).Trim();
            string endTaxi = (task.ENDTAXI ?? string.Empty).Trim();
            orderId = (orderId ?? string.Empty).Trim();
            orderInfo = (orderInfo ?? string.Empty).Trim();
            string orderPrefix = orderId + "-";

            bool typeOk = task.TYPE == 2;
            bool endTaxiOk = string.Equals(endTaxi, "1002", StringComparison.Ordinal);
            bool stateOk = task.STATE >= 0 && task.STATE <= 2;
            bool skuOk = skuSeq <= 0 || task.SKUSEQ == skuSeq;
            bool orderInfoOk = orderInfoValue.Length > 0
                && (string.Equals(orderInfoValue, orderInfo, StringComparison.OrdinalIgnoreCase)
                    || orderInfoValue.StartsWith(orderPrefix, StringComparison.Ordinal));
            bool matched = typeOk && endTaxiOk && stateOk && skuOk && orderInfoOk;

            return new JObject
            {
                ["taskId"] = task.ID,
                ["container"] = task.CONTAINER,
                ["state"] = task.STATE,
                ["skuSeq"] = task.SKUSEQ,
                ["type"] = task.TYPE,
                ["endTaxi"] = task.ENDTAXI,
                ["orderInfo"] = task.ORDERINFO,
                ["typeOk"] = typeOk,
                ["endTaxiOk"] = endTaxiOk,
                ["stateOk"] = stateOk,
                ["skuOk"] = skuOk,
                ["orderInfoOk"] = orderInfoOk,
                ["matched"] = matched
            };
        }

        private static bool IsSmallDrillPackingTask(wms_stocker_task task, string orderId, string orderInfo, int skuSeq)
        {
            if (task == null || task.TYPE != 2)
            {
                return false;
            }

            string endTaxi = (task.ENDTAXI ?? string.Empty).Trim();
            if (!string.Equals(endTaxi, "1002", StringComparison.Ordinal))
            {
                return false;
            }

            string orderInfoValue = (task.ORDERINFO ?? string.Empty).Trim();
            if (orderInfoValue.Length == 0)
            {
                return false;
            }

            orderId = (orderId ?? string.Empty).Trim();
            orderInfo = (orderInfo ?? string.Empty).Trim();
            string orderPrefix = orderId + "-";

            if (string.Equals(orderInfoValue, orderInfo, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!orderInfoValue.StartsWith(orderPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            return skuSeq <= 0 || task.SKUSEQ == skuSeq;
        }

        private static JObject BuildPackingTaskEval(wms_stocker_task task, string orderId, string orderInfo, int skuSeq, List<string> containers)
        {
            string container = (task.CONTAINER ?? string.Empty).Trim();
            string orderInfoValue = (task.ORDERINFO ?? string.Empty).Trim();
            string endTaxi = (task.ENDTAXI ?? string.Empty).Trim();
            orderId = (orderId ?? string.Empty).Trim();
            orderInfo = (orderInfo ?? string.Empty).Trim();
            string orderPrefix = orderId + "-";

            bool typeOk = task.TYPE == 2;
            bool endTaxiOk = string.Equals(endTaxi, "1002", StringComparison.Ordinal);
            bool stateOk = task.STATE >= 0 && task.STATE <= 2;
            bool containerOk = containers.Any(c => string.Equals(c, container, StringComparison.OrdinalIgnoreCase));
            bool skuOk = skuSeq <= 0 || task.SKUSEQ == skuSeq;
            bool orderInfoOk = orderInfoValue.Length > 0
                && (string.Equals(orderInfoValue, orderInfo, StringComparison.OrdinalIgnoreCase)
                    || orderInfoValue.StartsWith(orderPrefix, StringComparison.Ordinal));

            return new JObject
            {
                ["taskId"] = task.ID,
                ["container"] = task.CONTAINER,
                ["state"] = task.STATE,
                ["skuSeq"] = task.SKUSEQ,
                ["type"] = task.TYPE,
                ["endTaxi"] = task.ENDTAXI,
                ["orderInfo"] = task.ORDERINFO,
                ["typeOk"] = typeOk,
                ["endTaxiOk"] = endTaxiOk,
                ["stateOk"] = stateOk,
                ["containerOk"] = containerOk,
                ["skuOk"] = skuOk,
                ["orderInfoOk"] = orderInfoOk,
                ["matched"] = typeOk && endTaxiOk && stateOk && containerOk && skuOk && orderInfoOk
            };
        }

        private List<wms_stocker_task> FindPackingTasksForComplete(
            string orderId,
            string orderInfo,
            int skuSeq,
            List<string> containerList,
            List<wms_bill_putoutheader> headersToUpdate,
            JArray steps)
        {
            var containers = (containerList ?? new List<string>())
                .Select(c => (c ?? string.Empty).Trim())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 先仅按容器查库，避免 SqlSugar 对 StartsWith/组合条件翻译异常
            var candidates = servicetask.Query(s => containers.Contains(s.CONTAINER)).ToList();
            var evaluations = candidates
                .Select(t => BuildPackingTaskEval(t, orderId, orderInfo, skuSeq, containers))
                .ToList();

            steps.Add(new JObject
            {
                ["step"] = "packingTaskLookupEval",
                ["containerFilter"] = new JArray(containers),
                ["candidateCount"] = candidates.Count,
                ["evaluations"] = new JArray(evaluations)
            });

            var matched = candidates
                .Where(t => IsSmallDrillPackingTask(t, orderId, orderInfo, skuSeq))
                .GroupBy(t => (t.CONTAINER ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(t => t.STATE).ThenByDescending(t => t.LASTEDITTIME).First())
                .ToList();

            if (matched.Count == 0 && headersToUpdate != null && headersToUpdate.Count > 0)
            {
                foreach (var header in headersToUpdate)
                {
                    string headerContainer = (header.CONTAINER ?? string.Empty).Trim();
                    var task = candidates
                        .Where(t => string.Equals((t.CONTAINER ?? string.Empty).Trim(), headerContainer, StringComparison.OrdinalIgnoreCase))
                        .Where(t => t.TYPE == 2 && string.Equals((t.ENDTAXI ?? string.Empty).Trim(), "1002", StringComparison.Ordinal))
                        .Where(t => t.STATE >= 0 && t.STATE <= 2)
                        .Where(t => skuSeq <= 0 || t.SKUSEQ == header.SKUSEQ)
                        .OrderByDescending(t => t.STATE)
                        .ThenByDescending(t => t.LASTEDITTIME)
                        .FirstOrDefault();

                    if (task != null && matched.All(x => !string.Equals(x.ID, task.ID, StringComparison.OrdinalIgnoreCase)))
                    {
                        matched.Add(task);
                    }
                }

                if (matched.Count > 0)
                {
                    steps.Add(new JObject
                    {
                        ["step"] = "packingTaskLookupByHeader",
                        ["note"] = "按已更新的出库单头 CONTAINER/SKUSEQ 回退匹配堆垛机任务",
                        ["matchedCount"] = matched.Count
                    });
                }
            }

            if (matched.Count == 0)
            {
                steps.Add(new JObject
                {
                    ["step"] = "packingTaskLookupMiss",
                    ["note"] = "未找到可更新的堆垛机任务"
                });
            }

            return matched;
        }

        //小钻出库2-装盘完成WCS上报WMS
        public ReslutJson ReportPackingComplete(JObject json)
        {
            var trace = new JObject
            {
                ["method"] = "ReportPackingComplete",
                ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            };
            var steps = new JArray();
            trace["steps"] = steps;

            string pallet = json["pallet"].ToString().Trim();//大托盘号
            string orderInfo = json["orderInfo"].ToString().Trim();//订单号信息
            string containers = json["containers"].ToString().Trim();//小托盘号集合
            int taskState = int.Parse(json["taskState"].ToString());//任务状态（0正常，1异常状态，强制结束，将未完成的装盘任务标记为异常）
            trace["request"] = new JObject
            {
                ["pallet"] = pallet,
                ["orderInfo"] = orderInfo,
                ["containers"] = containers,
                ["taskState"] = taskState
            };

            string orderId;
            string specc;
            int sets;
            int skuseq;
            if (!TryParseSmallDrillOrderInfo(orderInfo, out orderId, out specc, out sets, out skuseq))
            {
                steps.Add(new JObject
                {
                    ["step"] = "parseOrderInfo",
                    ["success"] = false,
                    ["orderInfo"] = orderInfo
                });
                trace["result"] = new JObject { ["success"] = false, ["msg"] = "orderInfo格式不正确" };
                LOGService.WriteWcsRequestLog(context, json, trace.ToString(Newtonsoft.Json.Formatting.None), "小钻装盘完成上报WMS-诊断");
                return new ReslutJson()
                {
                    success = false,
                    msg = "orderInfo格式不正确：" + orderInfo
                };
            }

            trace["parsed"] = new JObject
            {
                ["orderId"] = orderId,
                ["specc"] = specc,
                ["sets"] = sets,
                ["skuseq"] = skuseq
            };

            List<string> containerList = containers
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .ToList();
            trace["request"]["containerCount"] = containerList.Count;
            trace["request"]["containerList"] = new JArray(containerList);

            try
            {
                #region
                var headersToUpdate = serviceputoutheader.Query(s =>
                    s.ORDERID == orderId
                    && containerList.Contains(s.CONTAINER)
                    && s.STATE == 2).ToList();
                steps.Add(new JObject
                {
                    ["step"] = "updatePutoutHeaders",
                    ["matchedCount"] = headersToUpdate.Count,
                    ["matchedContainers"] = new JArray(headersToUpdate.Select(h => h.CONTAINER)),
                    ["beforeStates"] = new JArray(headersToUpdate.Select(h => new JObject
                    {
                        ["container"] = h.CONTAINER,
                        ["state"] = h.STATE,
                        ["skuSeq"] = h.SKUSEQ
                    }))
                });
                headersToUpdate.ForEach(header =>
                {
                    header.STATE = 3;
                    header.LASTEDITTIME = DateTime.Now;
                    header.LASTEDITOR = "WCS";
                    serviceputoutheader.Update(header);
                });

                var packingTasks = FindPackingTasksForComplete(orderId, orderInfo, skuseq, containerList, headersToUpdate, steps);
                string workname = packingTasks.Select(t => t.WORKNAME).FirstOrDefault(w => !string.IsNullOrWhiteSpace(w)) ?? orderId;
                steps.Add(new JObject
                {
                    ["step"] = "updateStockerTasks",
                    ["matchedCount"] = packingTasks.Count,
                    ["workname"] = workname,
                    ["matchedTasks"] = new JArray(packingTasks.Select(t => new JObject
                    {
                        ["taskId"] = t.ID,
                        ["container"] = t.CONTAINER,
                        ["state"] = t.STATE,
                        ["taskMode"] = t.TASKMODE,
                        ["orderInfo"] = t.ORDERINFO
                    }))
                });
                packingTasks.ForEach(task =>
                {
                    task.STATE = 3;
                    task.LASTEDITTIME = DateTime.Now;
                    task.LASTEDITOR = "WCS";
                    task.PALLET = pallet;
                    servicetask.Update(task);
                });

                // 按工单+规格序号统计整规格是否装完；已标记异常(state=5)的托盘不参与计算
                List<wms_bill_putoutheader> headerList = serviceputoutheader.Query(s => s.ORDERID == orderId && s.SKUSEQ == skuseq && s.TARGET == "1002").ToList();
                List<wms_bill_putoutheader> effectiveHeaders = headerList.Where(h => h.STATE != 5).ToList();
                int headercount = effectiveHeaders.Count;
                int taskCount = effectiveHeaders.Count(h => h.STATE == 3);
                bool allSpecDone = headercount == 0 || taskCount == headercount;
                var stateSummary = headerList
                    .GroupBy(h => h.STATE)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());
                var incompleteHeaders = effectiveHeaders
                    .Where(h => h.STATE != 3)
                    .Select(h => new JObject
                    {
                        ["container"] = h.CONTAINER,
                        ["state"] = h.STATE,
                        ["remark"] = h.REMARK
                    })
                    .ToList();

                steps.Add(new JObject
                {
                    ["step"] = "specCompletionCheck",
                    ["headercount"] = headercount,
                    ["excludedAbnormalCount"] = headerList.Count(h => h.STATE == 5),
                    ["taskCount"] = taskCount,
                    ["allSpecDone"] = allSpecDone,
                    ["stateSummary"] = JObject.FromObject(stateSummary),
                    ["incompleteHeaders"] = new JArray(incompleteHeaders)
                });

                if (taskState == 1)
                {
                    steps.Add(new JObject
                    {
                        ["step"] = "taskStateIgnored",
                        ["taskState"] = taskState,
                        ["note"] = "taskState=1 已忽略，异常标记请通过调度页处理"
                    });
                }

                // 仅全部正常装盘完成时自动开启下一规格/下一工单；异常标记改由调度页手动处理
                var advanceStep = new JObject
                {
                    ["step"] = "advanceDecision",
                    ["allSpecDone"] = allSpecDone,
                    ["willAdvance"] = allSpecDone
                };
                if (allSpecDone)
                {
                    int nextSkuSeq = skuseq + 1;
                    bool hasNextSkuInCurrentOrder = serviceputoutheader.Query(s => s.ORDERID == orderId && s.SKUSEQ == nextSkuSeq).Any();
                    advanceStep["nextSkuSeq"] = nextSkuSeq;
                    advanceStep["hasNextSkuInCurrentOrder"] = hasNextSkuInCurrentOrder;

                    if (hasNextSkuInCurrentOrder)
                    {
                        var pendingNextSpecTaskList = QueryPendingPrefetchTasksForOrder(orderId, nextSkuSeq);
                        advanceStep["action"] = "activateNextSpec";
                        advanceStep["pendingNextSpecTasksBefore"] = pendingNextSpecTaskList.Count;

                        int activatedNextSpecCount = ActivatePendingTasksToNormal(orderId, nextSkuSeq);
                        advanceStep["activatedNextSpecTasksAfter"] = activatedNextSpecCount;
                    }
                    else
                    {
                        var pendingNextOrderTasks = servicetask.Query(s =>
                            s.ORDERINFO != null
                            && s.ORDERINFO != ""
                            && s.TYPE == 2
                            && s.ENDTAXI == "1002"
                            && s.SKUSEQ == 1
                            && s.STATE == 0).ToList()
                            .Where(t => IsPreFetchTaskMode(t))
                            .ToList();
                        string nextOrderId = pendingNextOrderTasks
                            .Select(t => new { Task = t, OrderId = ExtractOrderIdFromOrderInfo(t.ORDERINFO) })
                            .Where(x => !string.IsNullOrWhiteSpace(x.OrderId) && x.OrderId != orderId)
                            .GroupBy(x => x.OrderId)
                            .OrderBy(g => g.Min(x => x.Task.CREATETIME))
                            .Select(g => g.Key)
                            .FirstOrDefault();

                        advanceStep["action"] = "activateNextOrder";
                        advanceStep["pendingNextOrderCandidates"] = pendingNextOrderTasks.Count;
                        advanceStep["nextOrderId"] = nextOrderId ?? string.Empty;
                        advanceStep["pendingNextOrderTaskIds"] = new JArray(
                            pendingNextOrderTasks.Select(t => new JObject
                            {
                                ["taskId"] = t.ID,
                                ["orderInfo"] = t.ORDERINFO,
                                ["orderId"] = ExtractOrderIdFromOrderInfo(t.ORDERINFO)
                            }));

                        int activatedNextOrderCount = ActivateNextWorkOrderTasks(orderId);
                        advanceStep["activatedNextOrderTasksAfter"] = activatedNextOrderCount;
                    }
                }
                else
                {
                    advanceStep["action"] = "none";
                    advanceStep["reason"] = string.Format("规格未全部完成 taskCount={0}, headercount={1}", taskCount, headercount);
                }
                steps.Add(advanceStep);
                #endregion

                NotifyMesIssueDrillBitAsync(pallet, FormatWorknameForMes(workname), specc, containerList, workname);

                var result = new ReslutJson()
                {
                    success = true,
                    msg = "上报成功"
                };
                trace["result"] = new JObject
                {
                    ["success"] = true,
                    ["msg"] = result.msg
                };
                LOGService.WriteWcsRequestLog(context, json, result.msg, "小钻装盘完成上报WMS");
                LOGService.WriteWcsRequestLog(context, json, trace.ToString(Newtonsoft.Json.Formatting.None), "小钻装盘完成上报WMS-诊断");
                return result;
            }
            catch (Exception ex)
            {
                trace["result"] = new JObject
                {
                    ["success"] = false,
                    ["msg"] = ex.Message,
                    ["exception"] = ex.ToString()
                };
                LOGService.WriteWcsRequestLog(context, json, trace.ToString(Newtonsoft.Json.Formatting.None), "小钻装盘完成上报WMS-诊断");

                var result = new ReslutJson()
                {
                    success = false,
                    msg = "上报失败：" + ex.Message
                };
                LOGService.WriteWcsRequestLog(context, json, result.msg, "小钻装盘完成上报WMS");
                return result;
            }
        }

        private static string ExtractOrderIdFromOrderInfo(string orderInfo)
        {
            if (string.IsNullOrWhiteSpace(orderInfo))
            {
                return string.Empty;
            }

            int idx = orderInfo.IndexOf('-');
            return idx > 0 ? orderInfo.Substring(0, idx) : orderInfo;
        }

        private int ActivatePendingTasksToNormal(string orderId, int skuSeq, string editor = "WCS")
        {
            orderId = (orderId ?? string.Empty).Trim();
            var tasks = QueryPendingPrefetchTasksForOrder(orderId, skuSeq);

            foreach (var task in tasks)
            {
                task.TASKMODE = "NORMAL";
                task.LASTEDITTIME = DateTime.Now;
                task.LASTEDITOR = editor;
                servicetask.Update(task);
            }

            return tasks.Count;
        }

        private List<wms_stocker_task> QueryPendingPrefetchTasksForOrder(string orderId, int skuSeq)
        {
            orderId = (orderId ?? string.Empty).Trim();
            return servicetask.Query(s =>
                    s.TYPE == 2
                    && s.ENDTAXI == "1002"
                    && s.SKUSEQ == skuSeq
                    && s.STATE == 0
                    && s.ORDERINFO != null
                    && s.ORDERINFO != "")
                .Where(t => MatchesSmallDrillOrderInfoPrefix(t, orderId))
                .Where(t => IsPreFetchTaskMode(t))
                .ToList();
        }

        private static bool MatchesSmallDrillOrderInfoPrefix(wms_stocker_task task, string orderId)
        {
            orderId = (orderId ?? string.Empty).Trim();
            if (orderId.Length == 0)
            {
                return false;
            }

            string orderInfo = (task.ORDERINFO ?? string.Empty).Trim();
            return orderInfo.Length > 0 && orderInfo.StartsWith(orderId + "-", StringComparison.Ordinal);
        }

        private static bool IsPreFetchTaskMode(wms_stocker_task task)
        {
            return string.Equals((task.TASKMODE ?? string.Empty).Trim(), "PRE_FETCH", StringComparison.OrdinalIgnoreCase);
        }

        private int ActivateNextWorkOrderTasks(string currentOrderId, string editor = "WCS")
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            var pendingTasks = servicetask.Query(s =>
                    s.ORDERINFO != null
                    && s.ORDERINFO != ""
                    && s.TYPE == 2
                    && s.ENDTAXI == "1002"
                    && s.SKUSEQ == 1
                    && s.STATE == 0)
                .Where(t => IsPreFetchTaskMode(t))
                .ToList();

            var nextOrderId = pendingTasks
                .Select(t => new { Task = t, OrderId = ExtractOrderIdFromOrderInfo(t.ORDERINFO) })
                .Where(x => !string.IsNullOrWhiteSpace(x.OrderId) && x.OrderId != currentOrderId)
                .GroupBy(x => x.OrderId)
                .OrderBy(g => g.Min(x => x.Task.CREATETIME))
                .Select(g => g.Key)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(nextOrderId))
            {
                return 0;
            }

            return ActivatePendingTasksToNormal(nextOrderId, 1, editor);
        }

        private static string FormatWorknameForMes(string workname)
        {
            if (string.IsNullOrWhiteSpace(workname))
            {
                return string.Empty;
            }

            var parts = workname.Split('_');
            string result = parts.Length == 3 ? parts[1] : workname;
            return result.ToUpperInvariant();
        }

        private void NotifyMesIssueDrillBitAsync(string pallet, string materialCode, string spec, List<string> containerList, string rawWorkname = null)
        {
            if (containerList == null || containerList.Count == 0)
            {
                return;
            }

            var httpContext = context ?? HttpContext.Current;
            var triggerData = JsonConvert.SerializeObject(new
            {
                pallet,
                materialCode,
                spec,
                rawWorkname,
                mesUrl = ApiConfig.IssueDrillBitFromWarehouseUrl,
                containers = containerList
            });

            OperationLogWriter.WriteMesUpload("MES上报-触发", true, triggerData, null, null, 0, httpContext);

            try
            {
                var mesRequest = MesDrillBitService.BuildRequest(pallet, materialCode, spec, containerList, DateTime.Now);
                var requestJson = JsonConvert.SerializeObject(mesRequest);
                var requestMeta = JsonConvert.SerializeObject(new
                {
                    url = ApiConfig.IssueDrillBitFromWarehouseUrl,
                    timeoutSeconds = ApiConfig.MesHttpTimeoutSeconds,
                    body = mesRequest
                });

                OperationLogWriter.WriteMesUpload(
                    "MES上报-发送请求",
                    true,
                    requestMeta,
                    null,
                    null,
                    0,
                    httpContext);

                Task.Run(async () =>
                {
                    try
                    {
                        var callResult = await MesDrillBitService.IssueDrillBitFromWarehouseAsync(mesRequest).ConfigureAwait(false);
                        var responseMeta = JsonConvert.SerializeObject(new
                        {
                            url = callResult.RequestUrl,
                            httpStatusCode = callResult.HttpStatusCode,
                            durationMs = callResult.DurationMs,
                            request = requestJson,
                            response = callResult.ResponseJson
                        });

                        OperationLogWriter.WriteMesUpload(callResult.Success ? "MES上报-收到响应" : "MES上报-响应失败",callResult.Success,requestJson,responseMeta,callResult.ErrorMessage,callResult.DurationMs,httpContext);

                        var logMessage = callResult.Success
                            ? "MES上报成功：" + (callResult.Response?.Message ?? string.Empty)
                            : "MES上报失败：" + (callResult.ErrorMessage ?? callResult.Response?.Message ?? "未知错误");

                        LOGService.WriteWcsRequestLog(httpContext, JObject.Parse(requestJson), logMessage + "；响应：" + callResult.ResponseJson, "装盘完成上报MES");
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = "MES上报异常：" + ex.Message + "；请求：" + requestJson;
                        OperationLogWriter.WriteMesUpload(
                            "MES上报-异常",
                            false,
                            requestJson,
                            null,
                            ex.ToString(),
                            0,
                            httpContext);
                        LOGService.WriteWcsRequestLog(httpContext, JObject.Parse(requestJson), errorMessage, "装盘完成上报MES");
                    }
                });
            }
            catch (Exception ex)
            {
                var errorMessage = "MES上报初始化失败：" + ex.Message;
                OperationLogWriter.WriteMesUpload(
                    "MES上报-初始化失败",
                    false,
                    triggerData,
                    null,
                    ex.ToString(),
                    0,
                    httpContext);
                LOGService.WriteWcsRequestLog(httpContext, null, errorMessage, "装盘完成上报MES");
            }
        }


        /// <summary>
        /// 创建入库单，参数：托盘号、规格
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson CreatePutInfo(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            string barlist = json["Barcode"].ToString().Trim();     //托盘号
            string specc = json["Spec"].ToString().Trim(); //规格
            if (string.IsNullOrEmpty(barlist) || string.IsNullOrEmpty(specc))
            {
                reslutJson.success = false;
                reslutJson.msg = "入库单生成失败：条码或规格均不能为空!";
            }
            else
            {
                //符合规则则提示绑定成功，否则绑定失败
                //
                // string pattern = @"^(0\.(1[5-9][0-9]|[2-9][0-9]{2})|[1-5]\.[0-9]{3}|6\.([0-4][0-9]{2}|500)([AZ][0-6]$";
                string pattern = @"^(0\.(1[5-9][0-9]|[2-9][0-9]{2})|[1-5]\.[0-9]{3}|6\.([0-4][0-9]{2}|500))([A-Z])?$";
                //ItemInfo itemInfo = GetItemInfo(specc);
                string[] parts = specc.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6)
                {
                    reslutJson.success = false;
                    reslutJson.msg = "入库单生成失败" + specc + "数据信息缺失，请检查后重新入库";
                    LOGService.WriteWcsRequestLog(context, json, specc + "数据信息缺失，请检查后重新入库", "翻盒机扫码生成入库单");
                    return reslutJson;
                }
                string resultstr = parts[0].ToString();
                string type = parts[1].ToString();
                string series = parts[2].ToString();
                string SUPPLIERID = parts[3].ToString();
                string blade = parts[4].ToString();
                int mc = int.Parse(parts[5]);
                bool isValid = Regex.IsMatch(resultstr, pattern);
                //转换钻咀二维码
                string code = ConvertCode(specc);
                //判断直径(resultstr)小于0.5，需要拼接为<厂商_系列-直径(类型)/刃长*M磨次>格式 DT_UCN-0.500B/4.0*M5
                if (double.Parse(resultstr) < 0.525)
                {
                    code = $"{SUPPLIERID}_{series}-{code}/{blade}*M{mc}";
                }
                else if (type == "SD" || type == "EA")      //槽刀需要加类型以及刃长0.703/7.0*M5
                {
                    code = $"{code}/{blade}*M{mc}";
                }
                else
                {
                    code = code + $"*M{mc}";
                }



                //bool isValid = true;
                if (isValid)
                {
                    try
                    {
                        //string mc = json["cboSpec"].ToString().Trim(); //磨次
                        // ===== 单针改造：可选针清单参数 Needles（不传则完全维持旧行为）；单据与针清单同事务 =====
                        JToken needlesToken = json["Needles"] != null ? json["Needles"] : json["needles"];
                        JArray needleArr = needlesToken as JArray;
                        bool bindOkThis = false;
                        dbcontext.Db.Ado.BeginTran();
                        string[] list = null;
                        if (barlist.Substring(barlist.Length - 1, 1) != "/")
                        {
                            list = new string[] { barlist };
                        }
                        else
                        {

                            list = barlist.Substring(0, barlist.Length - 1).Split('/');

                        }
                        foreach (string s in list)
                        {
                            bindOkThis = false;
                            // string[] list = txtBarcode.Text.Trim().Split(',');
                            //判定是否重复绑定
                            var barcode = service.Query(x => x.CONTAINER == s);
                            var spec = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2);

                            if (!s.StartsWith("W", StringComparison.OrdinalIgnoreCase))
                            {
                                reslutJson.success = false;
                                reslutJson.msg = s + "入库单生成失败：托盘码不符合规则，请更换托盘重新入库!";
                                dbcontext.Db.Ado.RollbackTran(); // 单针改造：事务已开启，返回前必须回滚
                                return reslutJson;
                            }

                            if (barcode.Count > 0)
                            {
                                reslutJson.success = false;
                                reslutJson.msg = s + "入库单生成失败：库中已存在该条码!";
                            }
                            else
                            {

                                if (spec != null && spec.Count > 0)//已绑定则覆盖
                                {
                                    wms_bill_putinbindingheader oldheadmodel = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2).FirstOrDefault();
                                    wms_bill_putinbindingbody oldbodymodel = servicespecbody.Query(x => x.HEADERID == oldheadmodel.ID).FirstOrDefault();
                                    // wms_bill_putinbindingheader headmodel = new wms_bill_putinbindingheader();
                                    wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();
                                    //headmodel.ID = oldheadmodel.ID;
                                    //headmodel.SOURCEORDER = oldheadmodel.SOURCEORDER;
                                    //headmodel.CONTAINER = oldheadmodel.CONTAINER;
                                    //headmodel.CONTAINERTYPE = oldheadmodel.CONTAINERTYPE;
                                    //headmodel.QUESTFLAG = oldheadmodel.QUESTFLAG;
                                    //headmodel.STATE = oldheadmodel.STATE;
                                    //headmodel.TARGET = oldheadmodel.TARGET;
                                    //headmodel.ENABLED = oldheadmodel.ENABLED;
                                    //headmodel.REMARK = oldheadmodel.REMARK;
                                    //headmodel.CREATOR = oldheadmodel.CREATOR;
                                    //headmodel.CREATETIME = DateTime.Now;
                                    bodymodel.ID = oldbodymodel.ID;
                                    bodymodel.HEADERID = oldbodymodel.HEADERID;
                                    bodymodel.ITEMCODE = code;
                                    bodymodel.CREATOR = "手持扫描";
                                    bodymodel.CREATETIME = DateTime.Now;
                                    var results = servicespecbody.Update(bodymodel);
                                    if (results)
                                    {
                                        reslutJson.success = true;
                                        reslutJson.msg = code + "入库单生成成功!";
                                        bindOkThis = true; // 单针改造：覆盖绑定成功，允许写针清单
                                    }
                                    else
                                    {
                                        reslutJson.success = false;
                                        reslutJson.msg = "入库单生成失败：导入数据错误!";
                                    }
                                }
                                else
                                {

                                    //插入数据，提示绑定成功  
                                    string id = Guid.NewGuid().ToString("D");
                                    wms_bill_putinbindingheader headmodel = new wms_bill_putinbindingheader();
                                    wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();
                                    //         INSERT INTO wms_bill_putinbindingbody(ID, HEADERID,ITEMCODE,LOTWEIGHT, TASK_NO_SN, CREATOR, CREATETIME)
                                    //VALUES
                                    //(@ID, @HEADERID, @ITEMCODE, @LOT, @WEIGHT, @TASK_NO_SN, 'SAP', sysdate())

                                    headmodel.ID = id;
                                    headmodel.SOURCEORDER = s;
                                    headmodel.CONTAINER = s;
                                    headmodel.CONTAINERTYPE = "0";
                                    headmodel.QUESTFLAG = 0;
                                    headmodel.STATE = 0;
                                    headmodel.TARGET = "101";
                                    headmodel.ENABLED = 1;
                                    headmodel.REMARK = "入库";
                                    headmodel.CREATOR = "手持扫描";
                                    headmodel.CREATETIME = DateTime.Now;
                                    headmodel.TASKTYPE = SUPPLIERID;
                                    headmodel.Specifications = specc;
                                    var results = (servicespec.InsertReturnIdentity(headmodel) == 1);

                                    string bodyid = Guid.NewGuid().ToString("D");
                                    bodymodel.ID = bodyid;
                                    bodymodel.HEADERID = id;
                                    bodymodel.ITEMCODE = code;
                                    bodymodel.CREATOR = "手持扫描";
                                    bodymodel.CREATETIME = DateTime.Now;
                                    var results1 = (servicespecbody.InsertReturnIdentity(bodymodel) == 1);
                                    if (results & results1)
                                    {

                                        reslutJson.success = true;
                                        reslutJson.msg = code + "入库单生成成功!";
                                        bindOkThis = true; // 单针改造：新绑定成功，允许写针清单
                                    }
                                    else
                                    {
                                        reslutJson.success = false;
                                        reslutJson.msg = "入库单生成失败：导入数据错误!";

                                    }
                                }


                            }

                            // ===== 单针改造：绑定/换绑本盒针清单（Needles 为 null/空时自动生成 50 个 T-随机字符-位置号 临时针号） =====
                            if (bindOkThis)
                            {
                                var needlesForBox = NeedleService.FilterByContainer(needleArr, s);
                                if (needlesForBox == null || needlesForBox.Count == 0)
                                {
                                    needlesForBox = NeedleService.GenerateTempNeedles(NeedleService.TEMP_NEEDLE_COUNT);
                                }
                                NeedleService.BindNeedles(dbcontext.Db, s, code, SUPPLIERID, needlesForBox, "手持扫描");
                            }

                        }
                        dbcontext.Db.Ado.CommitTran(); // 单针改造：单据+针清单同事务提交
                    }
                    catch (Exception ee)
                    {
                        dbcontext.Db.Ado.RollbackTran(); // 单针改造：任一环节异常整体回滚
                        reslutJson.success = false;
                        reslutJson.msg = "入库单生成失败：" + ee.Message;
                    }
                }
                else
                {
                    reslutJson.success = false;
                    reslutJson.msg = "入库单生成失败：规格格式错误!";
                }

            }
            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "翻盒机扫码生成入库单");
            return reslutJson;
        }
        public async Task<ReslutJson> CreatePutInfoByLookupAsync(JObject json, string ip)
        {
            LOGService.WriteWcsRequestLog(context, json, "开始修改巷道状态", "WCS巷道管理");
            ReslutJson reslutJson = new ReslutJson();

            string lane = json["lane"].ToString().Trim();
            int enabled = int.Parse(json["enabled"].ToString().Trim());
            //调用研磨机接口获取数据
            HttpClient client = new HttpClient();
            Console.WriteLine("开始执行，修改巷道状态");
            int enabled1 = enabled == 1 ? 0 : 1;


            string url = $"http://172.20.63.181:9902/api/wcs/WMSToWcsLangEnable" + $"?wmsLangNum={lane}&wmsLangEnabled={enabled1}";

            string json1 = await client.GetStringAsync(url);

            //string result2 = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(json1);

            bool success = obj["IsSuccess"]?.Value<bool>() ?? false;
            Console.WriteLine("调用WCS接口，反馈：" + success);
            if (success)
            {
                reslutJson.success = true;
                reslutJson.msg = "WCS返回成功" + obj;
            }
            else
            {

                reslutJson.success = false;
                reslutJson.msg = "WCS返回失败" + obj;
            }
            return reslutJson;
        }
        //研磨机生成入库单
        public async Task<ReslutJson> UpdateContainer(JObject json, string ip)
        {
            LOGService.WriteWcsRequestLog(context, json, "开始执行生成入库单", "获取研磨机数据生成入库单");
            ReslutJson reslutJson = new ReslutJson();

            string newCode = json["newCode"].ToString().Trim();
            string oldCode = json["oldCode"].ToString().Trim();
            //string oldCode = json["Barcode"].ToString().Trim();     //托盘号
            string barlist;     //托盘号
            string code; //规格
            //调用研磨机接口获取数据
            HttpClient client = new HttpClient();

            string json1 = await client.GetStringAsync("http://172.20.63.188:5131/api/qrcode/lookup?qr=" + oldCode);

            JObject obj = JObject.Parse(json1);

            bool success = obj["success"]?.Value<bool>() ?? false;

            if (success)
            {
                oldCode = obj["result"]?["qrCode"]?.ToString();
                barlist = newCode;

                code = obj["result"]?["content"]?.ToString();
            }
            else
            {
                reslutJson.success = false;
                reslutJson.msg = "入库单生成失败：未查到托盘数据!";
                LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "翻盒机扫码生成入库单");
                return reslutJson;
            }

            LOGService.WriteWcsRequestLog(context, json, $"获取数据成功：托盘号：{barlist},规格：{code}", "获取研磨机数据生成入库单");
            if (string.IsNullOrEmpty(barlist) || string.IsNullOrEmpty(code))
            {
                reslutJson.success = false;
                reslutJson.msg = "入库单生成失败：条码或规格均不能为空!";
            }
            else
            {
                try
                {
                    // ===== 单针改造：研磨机换码入库，可选针清单参数 Needles（不传则完全维持旧行为）；单据与针清单同事务 =====
                    JToken needlesToken = json["Needles"] != null ? json["Needles"] : json["needles"];
                    JArray needleArr = needlesToken as JArray;
                    bool bindOkThis = false;
                    dbcontext.Db.Ado.BeginTran();
                    string[] list = null;
                    if (barlist.Substring(barlist.Length - 1, 1) != "/")
                    {
                        list = new string[] { barlist };
                    }
                    else
                    {

                        list = barlist.Substring(0, barlist.Length - 1).Split('/');

                    }
                    foreach (string s in list)
                    {
                        bindOkThis = false;
                        //判定是否重复绑定
                        var barcode = service.Query(x => x.CONTAINER == s);
                        var spec = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2);
                        if (barcode.Count > 0)
                        {
                            reslutJson.success = false;
                            reslutJson.msg = s + "入库单生成失败：库中已存在该条码!";
                        }
                        else
                        {

                            if (spec != null && spec.Count > 0)//已绑定则覆盖
                            {
                                wms_bill_putinbindingheader oldheadmodel = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2).FirstOrDefault();
                                wms_bill_putinbindingbody oldbodymodel = servicespecbody.Query(x => x.HEADERID == oldheadmodel.ID).FirstOrDefault();
                                wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();
                                bodymodel.ID = oldbodymodel.ID;
                                bodymodel.HEADERID = oldbodymodel.HEADERID;
                                bodymodel.ITEMCODE = code;
                                bodymodel.CREATOR = "手持扫描";
                                bodymodel.CREATETIME = DateTime.Now;
                                var results = servicespecbody.Update(bodymodel);
                                if (results)
                                {
                                    reslutJson.success = true;
                                    reslutJson.msg = code + "入库单生成成功!";
                                    bindOkThis = true; // 单针改造：覆盖绑定成功，允许写针清单
                                }
                                else
                                {
                                    reslutJson.success = false;
                                    reslutJson.msg = "入库单生成失败：导入数据错误!";
                                }
                            }
                            else
                            {

                                //插入数据，提示绑定成功  
                                string id = Guid.NewGuid().ToString("D");
                                wms_bill_putinbindingheader headmodel = new wms_bill_putinbindingheader();
                                wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();

                                headmodel.ID = id;
                                headmodel.SOURCEORDER = s;
                                headmodel.CONTAINER = s;
                                headmodel.CONTAINERTYPE = "0";
                                headmodel.QUESTFLAG = 0;
                                headmodel.STATE = 0;
                                headmodel.TARGET = "101";
                                headmodel.ENABLED = 1;
                                headmodel.REMARK = "入库";
                                headmodel.CREATOR = "手持扫描";
                                headmodel.CREATETIME = DateTime.Now;
                                headmodel.Specifications = code;
                                var results = (servicespec.InsertReturnIdentity(headmodel) == 1);

                                string bodyid = Guid.NewGuid().ToString("D");
                                bodymodel.ID = bodyid;
                                bodymodel.HEADERID = id;
                                bodymodel.ITEMCODE = code;
                                bodymodel.CREATOR = "手持扫描";
                                bodymodel.CREATETIME = DateTime.Now;
                                var results1 = (servicespecbody.InsertReturnIdentity(bodymodel) == 1);
                                if (results & results1)
                                {

                                    reslutJson.success = true;
                                    reslutJson.msg = code + "入库单生成成功!";
                                    bindOkThis = true; // 单针改造：新绑定成功，允许写针清单
                                }
                                else
                                {
                                    reslutJson.success = false;
                                    reslutJson.msg = "入库单生成失败：导入数据错误!";

                                }
                            }


                        }

                        // ===== 单针改造：绑定/换绑本盒针清单（Needles 为 null/空时自动生成 50 个 T-随机字符-位置号 临时针号） =====
                        if (bindOkThis)
                        {
                            var needlesForBox = NeedleService.FilterByContainer(needleArr, s);
                            if (needlesForBox == null || needlesForBox.Count == 0)
                            {
                                needlesForBox = NeedleService.GenerateTempNeedles(NeedleService.TEMP_NEEDLE_COUNT);
                            }
                            NeedleService.BindNeedles(dbcontext.Db, s, code, null, needlesForBox, "手持扫描");
                        }

                    }
                    dbcontext.Db.Ado.CommitTran(); // 单针改造：单据+针清单同事务提交
                }
                catch (Exception ee)
                {
                    dbcontext.Db.Ado.RollbackTran(); // 单针改造：任一环节异常整体回滚
                    reslutJson.success = false;
                    reslutJson.msg = "入库单生成失败：" + ee.Message;
                }
            }
            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "翻盒机扫码生成入库单");
            return reslutJson;
        }
        public string ConvertCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return string.Empty;

            var parts = code.Split('-');
            if (parts.Length < 2)
                return code;

            string baseValue = parts[0];      // 5.650
            string type = parts[1];           // N / A / EA / SD / B...
            //string last = parts[5];          // 最后一段（5）

            string resultBase = baseValue;

            // EA 规则：末位为 0 时替换最后一位，否则在末尾拼接
            if (type == "EA")
            {
                if (baseValue.EndsWith("0"))
                    resultBase = baseValue.Substring(0, baseValue.Length - 1) + "3";
                else
                    resultBase = baseValue + "3";
            }
            // SD 规则：末位为 0 时替换最后一位，否则在末尾拼接
            else if (type == "SD")
            {
                if (baseValue.EndsWith("0"))
                    resultBase = baseValue.Substring(0, baseValue.Length - 1) + "1";
                else
                    resultBase = baseValue + "1";
            }
            // A/B/C/D 规则
            else if (type == "A" || type == "B" || type == "C" || type == "D" || type == "R")
            {
                if (double.Parse(baseValue) > 0.5 && type == "B")
                {
                    resultBase = baseValue;
                }
                else
                {
                    resultBase = baseValue + type;
                }
            }

            return resultBase;
        }
        public ReslutJson ErrStockerTask(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();
            string id = json["taskId"].ToString();
            //更新任务状态为异常
            string sql = @"UPDATE wms_stocker_task SET STATE=3,LASTEDITTIME=sysdate() WHERE ID=@ID";
            int rows = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>()
            {
                new SugarParameter("@ID", id)
            });
            if (rows > 0)
            {
                reslutJson.success = true;
                reslutJson.msg = "任务状态更新为异常";
            }
            else
            {
                reslutJson.success = false;
                reslutJson.msg = "任务状态更新失败";
            }
            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "任务异常处理");
            return reslutJson;
        }

        /// <summary>
        /// 入库请求库位
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson scgrnLocationTranspor(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            // 1. 参数校验（不变，无事务）
            if (json == null)
            {
                reslutJson.success = false;
                reslutJson.msg = "参数不能为空";
                return reslutJson;
            }

            string barcode = string.Empty;
            string start = string.Empty;
            //string region = string.Empty;
            string lang = string.Empty;

            try
            {
                if (json["barcode"] != null)
                    barcode = json["barcode"].ToString().Trim();

                if (json["start"] != null)
                    start = json["start"].ToString().Trim();

                if (json["lang"] != null)
                    lang = json["lang"].ToString().Trim();
            }
            catch
            {
                reslutJson.success = false;
                reslutJson.msg = "参数格式错误";
                return reslutJson;
            }

            if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(start))
            {
                reslutJson.success = false;
                reslutJson.msg = "条码/入库口不能为空";
                return reslutJson;
            }

            // 2. 巷道判断（不变）
            int tune = 1;
            //if (start == "6100" || start == "6120")
            //{
            //    tune = 1;
            //}
            //else if (start == "6140" || start == "6160")
            //{
            //    tune = 2;
            //}
            //else
            //{
            //    tune = 3;
            //}
            if (lang == "T1")
            {
                tune = 1;
            }
            else if (lang == "T2")
            {
                tune = 2;
            }
            else if (lang == "T3")
            {
                tune = 3;
            }
            else
            {
                tune = 4;
            }




            // ==============================================
            // ✅ 修改1：增加死锁重试（解决死锁最有效）
            // ==============================================
            int retryCount = 0;
            const int MaxRetry = 3;
            while (retryCount < MaxRetry)
            {
                // ✅ 修改2：事务标记，防止重复回滚
                bool isTransactionOpen = false;

                void CloseTransaction(bool commit)
                {
                    if (!isTransactionOpen) return;
                    try
                    {
                        if (commit)
                            dbcontext.Db.Ado.CommitTran();
                        else
                            dbcontext.Db.Ado.RollbackTran();
                    }
                    catch { }
                    isTransactionOpen = false;
                }

                try
                {
                    // 3. 开启事务（不变）
                    dbcontext.Db.Ado.BeginTran();
                    isTransactionOpen = true;

                    // 4. 防重复请求（不变）
                    string checkSql = @"
SELECT ID,STATE FROM wms_stocker_task WHERE CONTAINER=@CONTAINER AND TYPE=1 AND (STATE<2 OR STATE=3) LIMIT 1";

                    DataTable checkdata = dbcontext.Db.Ado.GetDataTable(checkSql, new List<SugarParameter>()
            {
                new SugarParameter("@CONTAINER", barcode)
            });

                    //if (checkdata != null && checkdata.Rows.Count > 0)
                    //{
                    //    if (isTransactionOpen) dbcontext.Db.Ado.RollbackTran(); // ✅ 修改3：安全回滚
                    //    reslutJson.success = false;
                    //    reslutJson.msg = "重复请求";
                    //    return reslutJson;

                    //}
                    //                    string checkSql1 = @"
                    //SELECT ID FROM wms_stocker_task WHERE CONTAINER=@CONTAINER AND STATE=3 LIMIT 1";

                    //                    DataTable checkdata1 = dbcontext.Db.Ado.GetDataTable(checkSql1, new List<SugarParameter>()
                    //            {
                    //                new SugarParameter("@CONTAINER", barcode)
                    //            });

                    if (checkdata != null && checkdata.Rows.Count > 0)
                    {
                        wms_base_location wms_Base_Location = service.Query(x => x.REMARK == barcode).FirstOrDefault();
                        if (wms_Base_Location == null)
                        {
                            CloseTransaction(false);
                            reslutJson.success = false;
                            reslutJson.msg = "存在未完成任务但未找到对应库位，请检查任务与库位数据";
                            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "入库请求库位");
                            return reslutJson;
                        }
                        if (wms_Base_Location.TUNNELNUM != tune)
                        {
                            wms_Base_Location.REMARK = "";
                            wms_Base_Location.ISAVIALABLE = 1;
                            wms_Base_Location.SOURCEORDER = "";
                            service.Update(wms_Base_Location);
                            string updateHeader2 = @"
                                        UPDATE wms_bill_putinbindingheader
                                        SET QUESTFLAG=0,LOCATIONCODE='',STATE=0
                                        WHERE CONTAINER=@CONTAINER AND (STATE<2 OR STATE=3) LIMIT 1";
                            dbcontext.Db.Ado.ExecuteCommand(updateHeader2, new List<SugarParameter>()
                            {
                                new SugarParameter("@CONTAINER", barcode)
                            });
                            string deleteTaskSql = @"
                                        DELETE FROM wms_stocker_task
                                        WHERE CONTAINER=@CONTAINER AND TYPE=1 AND (STATE<2 OR STATE=3)";
                            dbcontext.Db.Ado.ExecuteCommand(deleteTaskSql, new List<SugarParameter>()
                            {
                                new SugarParameter("@CONTAINER", barcode)
                            });
                        }
                        else
                        {
                            Dictionary<string, string> data1 = new Dictionary<string, string>();
                            data1.Add("taskId", checkdata.Rows[0]["ID"].ToString());
                            data1.Add("taskType", "1");
                            data1.Add("conTainer", barcode);
                            data1.Add("startTaxi", start);
                            data1.Add("endTaxi", wms_Base_Location.LOCATIONCODE);

                            reslutJson.success = true;
                            int state = int.Parse(checkdata.Rows[0]["STATE"].ToString());
                            if (state < 2)
                            {
                                reslutJson.msg = "重复请求";
                            }
                            else
                            {
                                reslutJson.msg = "WCS请求成功!";
                            }
                            reslutJson.data = data1;
                            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg + data1.ToString(), "入库请求库位");
                            CloseTransaction(false);
                            return reslutJson;
                        }
                    }

                    // 5. 查询入库单（不变）
                    string bindSql = @"
SELECT a.ID, a.CONTAINER, b.ITEMCODE
FROM wms_bill_putinbindingheader a
INNER JOIN wms_bill_putinbindingbody b ON a.ID=b.HEADERID
WHERE a.CONTAINER=@CONTAINER AND a.QUESTFLAG=0 AND a.STATE=0 LIMIT 1";

                    DataTable glist = dbcontext.Db.Ado.GetDataTable(bindSql, new List<SugarParameter>()
                    {
                        new SugarParameter("@CONTAINER", barcode)
                    });

                    if (glist == null || glist.Rows.Count <= 0)
                    {
                        CloseTransaction(false);
                        string locSql2 = @"SELECT LOCATIONCODE FROM wms_base_location WHERE CONTAINER=@CONTAINER";
                        DataTable loc2 = dbcontext.Db.Ado.GetDataTable(locSql2, new List<SugarParameter>()
                        {
                            new SugarParameter("@CONTAINER", barcode)
                        });
                        string locationcode = loc2.Rows.Count > 0 ? loc2.Rows[0]["LOCATIONCODE"].ToString() : "";
                        if (!string.IsNullOrEmpty(locationcode))
                        {
                            reslutJson.success = false;
                            reslutJson.msg = "缓存箱：" + barcode + "已入库至" + locationcode + "，请确认！";
                        }
                        else
                        {
                            reslutJson.success = false;
                            reslutJson.msg = "未查询到入库清单,且未查询到库位";
                        }
                        return reslutJson;
                    }

                    string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
                    string ID = glist.Rows[0]["ID"].ToString();

                    // 1. 先查询并锁定一条可用库位（FOR UPDATE 行锁，高并发绝对不重复） 新增条件：Region区域
                    /* string selectLockSql = @"
     SELECT LOCATIONCODE
     FROM wms_base_location
     WHERE TYPE='ST'
       AND ENABLED=1
       AND TUNNELNUM=@TUNNELNUM
       AND ISAVIALABLE=1
       AND REMARK=''
       AND SOURCEORDER=''
         AND REGION=@REGION
     ORDER BY ROWNUM ASC, LAYERNUM ASC, COLUMNNUM ASC
     LIMIT 1
     FOR UPDATE;  -- 关键：行锁，其他事务必须等待
 ";*/
                    string selectLockSql = @"SELECT LOCATIONCODE
    FROM wms_base_location
    WHERE TYPE='ST'
      AND ENABLED=1
      AND TUNNELNUM=@TUNNELNUM
      AND ISAVIALABLE=1
      AND REMARK=''
      AND SOURCEORDER=''
    ORDER BY ABS(LIMITHEIGHT - @LIMITHEIGHT) ASC,LAYERNUM,COLUMNNUM, ROWNUM 
    LIMIT 1
    FOR UPDATE;"; // 备用查询：先按区域差值升序，再按库位顺序降序，增加命中率

                    // 2. 更新锁定的这条库位
                    string updateSql = @"
    UPDATE wms_base_location
    SET ISAVIALABLE = 0,
        REMARK = @REMARK,
        SOURCEORDER = @SOURCEORDER
    WHERE LOCATIONCODE = @LOCATIONCODE;
";


                    // ====================== 第一步：查询并锁定库位 ======================
                    var locationCode = dbcontext.Db.Ado.SqlQuery<string>(selectLockSql, new
                    {
                        TUNNELNUM = tune,
                        LIMITHEIGHT = start //区域
                    }).FirstOrDefault();

                    // 没查到可用库位
                    if (string.IsNullOrEmpty(locationCode))
                    {
                        CloseTransaction(false);
                        reslutJson.success = false;
                        reslutJson.msg = "库位分配失败(无可用库位)";
                        return reslutJson;
                    }

                    // ====================== 第二步：更新这条锁定的库位 ======================
                    int updateRows = dbcontext.Db.Ado.ExecuteCommand(updateSql, new
                    {
                        REMARK = barcode,
                        SOURCEORDER = ITEMCODE,
                        LOCATIONCODE = locationCode
                    });

                    if (updateRows <= 0)
                    {
                        CloseTransaction(false);
                        reslutJson.success = false;
                        reslutJson.msg = "库位分配失败(更新失败)";
                        return reslutJson;
                    }

                    string LOCATIONCODE = locationCode;

                    // 更新入库单（不变）
                    string updateHeader = @"
UPDATE wms_bill_putinbindingheader
SET QUESTFLAG=1,QUESTTIME=NOW(),LOCATIONCODE=@LOCATIONCODE,STATE=1
WHERE ID=@ID";

                    dbcontext.Db.Ado.ExecuteCommand(updateHeader, new List<SugarParameter>()
            {
                new SugarParameter("@LOCATIONCODE", LOCATIONCODE),
                new SugarParameter("@ID", ID)
            });

                    // 生成任务（不变）
                    string taskId = Guid.NewGuid().ToString("D");
                    string insertSql = @"
INSERT INTO wms_stocker_task
(ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
VALUES
(@ID,@CODE,@ENDTAXI,@CONTAINER,@STARTTAXI,1,NOW(),@ZONECODE,@SOURCEORDER,0,1,NOW(),'WMS','WMS',NOW())";

                    dbcontext.Db.Ado.ExecuteCommand(insertSql, new List<SugarParameter>()
            {
                new SugarParameter("@ID", taskId),
                new SugarParameter("@CODE", taskId),
                new SugarParameter("@ENDTAXI", LOCATIONCODE),
                new SugarParameter("@CONTAINER", barcode),
                new SugarParameter("@STARTTAXI", start),
                new SugarParameter("@ZONECODE", "101"),
                new SugarParameter("@SOURCEORDER", ITEMCODE)
            });

                    // 提交事务
                    CloseTransaction(true);

                    // 返回结果
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("taskId", taskId);
                    data.Add("taskType", "1");
                    data.Add("conTainer", barcode);
                    data.Add("startTaxi", start);
                    data.Add("endTaxi", LOCATIONCODE);

                    reslutJson.success = true;
                    reslutJson.msg = "WCS请求成功!";
                    reslutJson.data = data;
                    LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "入库请求库位");
                    return reslutJson;
                }
                catch (Exception e)
                {
                    // ✅ 修改6：捕获死锁异常，自动重试
                    if (e.Message.Contains("Deadlock") || e.Message.Contains("死锁"))
                    {
                        retryCount++;
                        CloseTransaction(false);
                        System.Threading.Thread.Sleep(50 * retryCount); // 重试等待
                        continue;
                    }

                    // 其他异常直接返回
                    CloseTransaction(false);
                    reslutJson.success = false;
                    reslutJson.msg = e.Message;
                    LOGService.WriteOperateLogF("原因：" + e.Message, "托盘立库请求库位", ip);
                    LOGService.WriteWcsRequestLog(context, json, "WCS请求失败，IP：" + ip + "错误信息：" + e.Message, "入库请求库位");
                    return reslutJson;
                }
            }

            // 重试次数用完
            reslutJson.success = false;
            reslutJson.msg = "库位分配失败，并发死锁多次重试无效";
            LOGService.WriteOperateLogF("死锁重试耗尽", "托盘立库请求库位", ip);
            LOGService.WriteWcsRequestLog(context, json, reslutJson.msg, "入库请求库位");
            return reslutJson;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson lygrnLocationTransporold(JObject json, string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            // 1. 参数校验
            if (json == null)
            {
                //reslutJson = new ReslutJson();
                reslutJson.success = false;
                reslutJson.msg = "参数不能为空";
                //LOGService.WriteOperateLogF("参数json为null", "托盘立库请求库位", ip);
                return reslutJson;
            }

            string barcode = string.Empty;
            string start = string.Empty;

            try
            {
                if (json["barcode"] != null)
                    barcode = json["barcode"].ToString().Trim();

                if (json["start"] != null)
                    start = json["start"].ToString().Trim();
            }
            catch
            {
                //reslutJson = new ReslutJson();
                reslutJson.success = false;
                reslutJson.msg = "参数格式错误";
                //LOGService.WriteOperateLogF("barcode/start参数解析失败", "托盘立库请求库位", ip);
                return reslutJson;
            }

            if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(start))
            {
                //reslutJson = new ReslutJson();
                reslutJson.success = false;
                reslutJson.msg = "条码/入库口不能为空";
                return reslutJson;
            }

            // 2. 巷道判断
            int tune = 3;
            if (start == "6100" || start == "6120")
            {
                tune = 1;
            }
            else if (start == "6140" || start == "6160")
            {
                tune = 2;
            }
            else
            {
                tune = 3;
            }

            try
            {

                // 3. 事务只开启一次
                dbcontext.Db.Ado.BeginTran();
                // 4. 防重复请求
                string checkSql = @"
SELECT 1 FROM wms_stocker_task WHERE CONTAINER=@CONTAINER AND STATE<2 LIMIT 1";

                DataTable checkdata = dbcontext.Db.Ado.GetDataTable(checkSql, new List<SugarParameter>()
        {
            new SugarParameter("@CONTAINER", barcode)
        });

                if (checkdata != null && checkdata.Rows.Count > 0)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    // reslutJson = new ReslutJson();
                    reslutJson.success = false;
                    reslutJson.msg = "重复请求";

                    return reslutJson;
                }

                // 5. 查询入库单
                string bindSql = @"
SELECT a.ID, a.CONTAINER, b.ITEMCODE
FROM wms_bill_putinbindingheader a
INNER JOIN wms_bill_putinbindingbody b ON a.ID=b.HEADERID
WHERE a.CONTAINER=@CONTAINER AND a.QUESTFLAG=0 AND a.STATE=0 LIMIT 1";

                DataTable glist = dbcontext.Db.Ado.GetDataTable(bindSql, new List<SugarParameter>()
        {
            new SugarParameter("@CONTAINER", barcode)
        });

                if (glist == null || glist.Rows.Count <= 0)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    //reslutJson = new ReslutJson();
                    reslutJson.success = false;
                    reslutJson.msg = "未查询到入库清单";
                    // dbcontext.Db.Ado.RollbackTran();
                    //LOGService.WriteOperateLogF("未查询到入库清单", "托盘立库请求库位", ip);
                    return reslutJson;
                }

                string ITEMCODE = glist.Rows[0]["ITEMCODE"].ToString();
                string ID = glist.Rows[0]["ID"].ToString();

                // ====================== 核心：安全分配库位（支持你的SQL） ======================
                string updateSql = @"
UPDATE wms_base_location
SET ISAVIALABLE=0,
    REMARK=@REMARK,
    SOURCEORDER=@SOURCEORDER
WHERE TYPE='ST'
  AND ENABLED=1
  AND TUNNELNUM=@TUNNELNUM
  AND ISAVIALABLE=1
  AND REMARK=''
  AND SOURCEORDER=''
ORDER BY ROWNUM ASC, LAYERNUM ASC, COLUMNNUM ASC
LIMIT 1";


                int updateRows = dbcontext.Db.Ado.ExecuteCommand(updateSql, new List<SugarParameter>()
        {
            new SugarParameter("@REMARK", barcode),
            new SugarParameter("@TUNNELNUM", tune),
            new SugarParameter("@SOURCEORDER", ITEMCODE)
        });

                // 必须判断：并发情况下没更新到就是没抢到库位
                if (updateRows <= 0)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    //reslutJson = new ReslutJson();
                    reslutJson.success = false;
                    reslutJson.msg = "库位分配失败(无可用库位/并发冲突)";
                    //dbcontext.Db.Ado.RollbackTran();
                    //LOGService.WriteOperateLogF("未查询到可用库位或并发冲突", "托盘立库请求库位", ip);
                    return reslutJson;
                }

                // 查询已占用的库位
                string locSql = @"
SELECT LOCATIONCODE FROM wms_base_location
WHERE  
  ISAVIALABLE=0 
  AND REMARK=@REMARK
  AND SOURCEORDER=@SOURCEORDER
  AND TUNNELNUM=@TUNNELNUM   LIMIT 1";

                DataTable inlocation = dbcontext.Db.Ado.GetDataTable(locSql, new List<SugarParameter>()
        {
            new SugarParameter("@REMARK", barcode),
            new SugarParameter("@TUNNELNUM", tune),
            new SugarParameter("@SOURCEORDER", ITEMCODE)
        });

                if (inlocation == null || inlocation.Rows.Count <= 0)
                {
                    dbcontext.Db.Ado.RollbackTran();
                    // reslutJson = new ReslutJson();
                    reslutJson.success = false;
                    reslutJson.msg = "未查询到库位";
                    //dbcontext.Db.Ado.RollbackTran();
                    //LOGService.WriteOperateLogF("未查询到库位", "托盘立库请求库位", ip);
                    return reslutJson;
                }

                string LOCATIONCODE = inlocation.Rows[0]["LOCATIONCODE"].ToString();

                // 更新入库单
                string updateHeader = @"
UPDATE wms_bill_putinbindingheader
SET QUESTFLAG=1,QUESTTIME=NOW(),LOCATIONCODE=@LOCATIONCODE,STATE=1
WHERE ID=@ID";

                dbcontext.Db.Ado.ExecuteCommand(updateHeader, new List<SugarParameter>()
        {
            new SugarParameter("@LOCATIONCODE", LOCATIONCODE),
            new SugarParameter("@ID", ID)
        });

                // 生成任务
                string taskId = Guid.NewGuid().ToString("D");
                string insertSql = @"
INSERT INTO wms_stocker_task
(ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
VALUES
(@ID,@CODE,@ENDTAXI,@CONTAINER,@STARTTAXI,1,NOW(),@ZONECODE,@SOURCEORDER,0,1,NOW(),'WMS','WMS',NOW())";

                dbcontext.Db.Ado.ExecuteCommand(insertSql, new List<SugarParameter>()
        {
            new SugarParameter("@ID", taskId),
            new SugarParameter("@CODE", taskId),
            new SugarParameter("@ENDTAXI", LOCATIONCODE),
            new SugarParameter("@CONTAINER", barcode),
            new SugarParameter("@STARTTAXI", start),
            new SugarParameter("@ZONECODE", "101"),
            new SugarParameter("@SOURCEORDER", ITEMCODE)
        });

                // 提交事务
                dbcontext.Db.Ado.CommitTran();

                // 返回结果
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("taskId", taskId);
                data.Add("taskType", "1");
                data.Add("conTainer", barcode);
                data.Add("startTaxi", start);
                data.Add("endTaxi", LOCATIONCODE);

                reslutJson = new ReslutJson();
                reslutJson.success = true;
                reslutJson.msg = "WCS请求成功!";
                reslutJson.data = data;

                // LOGService.WriteOperateLogT("托盘条码:" + barcode + ",库位:" + LOCATIONCODE, "立库请求库位", ip);
            }
            catch (Exception e)
            {

                dbcontext.Db.Ado.RollbackTran();


                reslutJson = new ReslutJson();
                reslutJson.success = false;
                reslutJson.msg = e.Message;
                LOGService.WriteOperateLogF("原因：" + e.Message, "托盘立库请求库位", ip);
            }

            return reslutJson;
        }
        #endregion

        #region    三方交互


        /// <summary>
        /// 请求看板信息
        /// </summary>	
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson getkanbaninfo(string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            string sql = "";
            int a = 0, b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, h = 0, i = 0, j = 0, k = 0, l = 0, m = 0, n = 0, o = 0, p = 0;
            try
            {
                //空托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NULL   AND  a.TYPE='ST'
				";
                a = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","101-1")
                                }).Rows.Count;
                //有货托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NOT NULL  AND  b.LOT != ''  AND  a.TYPE='ST'

				";
                b = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","101-1")
                                }).Rows.Count;
                //空库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER = '' and  a.ENABLED=1   AND  a.TYPE='ST'

				";
                c = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","101-1")
                                }).Rows.Count;
                //冻结库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE='101-1'  AND   a.CONTAINER = '' and  a.ENABLED=0 and a.ISAVIALABLE=0 AND  a.LOCATIONCODE 
 not  in('024101','024201','010702','010602','010601','010501','010502','010402','010401','010302','010301')   AND  a.TYPE='ST'

				";
                d = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","101-1")
                                }).Rows.Count;


                //pp片空托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NULL   AND  a.TYPE='ST'  AND a.CONTAINERTYPE='1'
				";
                e = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;
                //pp片有货托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NOT NULL  AND  b.LOT != ''  AND  a.TYPE='ST'   AND a.CONTAINERTYPE='1'

				";
                f = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;

                //pp卷空托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NULL   AND  a.TYPE='ST'  AND a.CONTAINERTYPE='2'
				";
                g = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;
                //pp卷有货托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NOT NULL  AND  b.LOT != ''  AND  a.TYPE='ST'   AND a.CONTAINERTYPE='2'

				";
                h = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;

                //铜箔空托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NULL   AND  a.TYPE='ST'  AND a.CONTAINERTYPE='0'
				";
                i = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;
                //铜箔有货托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NOT NULL  AND  b.LOT != ''  AND  a.TYPE='ST'   AND a.CONTAINERTYPE='0'

				";
                j = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;

                //半成品空库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER = '' and  a.ENABLED=1   AND  a.TYPE='ST'

				";
                k = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;
                //半成品冻结库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE='101-1'  AND   a.CONTAINER = '' and  a.ENABLED=0 and a.ISAVIALABLE=0 AND  a.LOCATIONCODE 
 not  in('012801','022501','022506','022507','021907','021906','012001','011901','021401','021306','021307','020707','020706','010201','010101')   AND  a.TYPE='ST'

				";
                l = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","201-1")
                                }).Rows.Count;

                //成品空托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NULL   AND  a.TYPE='ST'
				";
                m = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","301-1")
                                }).Rows.Count;
                //成品有货托盘
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER != '' and b.LOT IS NOT NULL  AND  b.LOT != ''  AND  a.TYPE='ST'

				";
                n = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","301-1")
                                }).Rows.Count;
                //成品空库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE=@ZONECODE  AND  a.CONTAINER = '' and  a.ENABLED=1   AND  a.TYPE='ST'

				";
                o = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","301-1")
                                }).Rows.Count;
                //成品冻结库位
                sql = @"
				SELECT DISTINCT a.ZONECODE, a.LOCATIONCODE, a.CONTAINERTYPE    from wms_base_location a  LEFT JOIN   wms_bill_inventorybook  b  on  a.CONTAINER = b.BARCODE 
 and  a.SOURCEORDER = b.SOURCEORDER  WHERE  a.ZONECODE='101-1'  AND   a.CONTAINER = '' and  a.ENABLED=0 and a.ISAVIALABLE=0 AND  a.LOCATIONCODE 
 not  in('010101','010301','010705','010706','010906','010905','013501')   AND  a.TYPE='ST'

				";
                p = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
                                    new SugarParameter("@ZONECODE","301-1")
                                }).Rows.Count;

                reslutJson = new ReslutJson()
                {
                    success = true,
                    msg = "看板请求成功!",
                    data = new Dictionary<string, int>()
                                {
                                                    {"a", a },
                                                    {"b", b },
                                                    {"c", c },
                                                    { "d", d },
                                                    { "e", e },
                                                    { "f", f },
                                                    { "g", g },
                                                    { "h", h },
                                                    { "i", i },
                                                    { "j", j },
                                                    { "k", k },
                                                    { "l", l },
                                                    { "m", m },
                                                    { "n", n },
                                                    { "o", o },
                                                    { "p", p }
                                }
                };
            }

            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
            }

            return reslutJson;
        }
        /// <summary>
        /// 请求待执行入库信息
        /// </summary>	
        /// <param name="ip"></param>
        /// <returns></returns>
        public ReslutJson getunputininfo(string ip)
        {
            ReslutJson reslutJson = new ReslutJson();

            string sql = "";

            try
            {

                //待执行入库信息
                sql = @"
			
SELECT  SOURCEORDER,CONTAINER,STATE,TARGET as ZONECODE,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME  FROM  wms_bill_putinbindingheader   WHERE  STATE=0  and  ENABLED=1
				";
                var dt = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
                {

                });
                List<UNPutIn> rows = new List<UNPutIn>();
                foreach (DataRow dr in dt.Rows)
                {
                    UNPutIn model = new UNPutIn();
                    model.SOURCEORDER = dr["SOURCEORDER"].ToString();
                    model.CONTAINER = dr["CONTAINER"].ToString();
                    model.STATE = int.Parse(dr["STATE"].ToString());
                    model.ZONECODE = dr["ZONECODE"].ToString();
                    model.LOCATIONCODE = dr["LOCATIONCODE"].ToString();
                    model.ENABLED = int.Parse(dr["ENABLED"].ToString());
                    model.REMARK = dr["REMARK"].ToString();
                    model.CREATOR = dr["CREATOR"].ToString();
                    model.CREATETIME = dr["CREATETIME"].ToString();
                    rows.Add(model);
                }

                reslutJson = new ReslutJson()
                {
                    success = true,
                    msg = "请求成功!",
                    data = rows
                };
            }

            catch (Exception ex)
            {
                reslutJson = new ReslutJson()
                {
                    success = false,
                    msg = ex.Message,
                };
                LOGService.WriteExceptionLog(ex);
            }

            return reslutJson;
        }

        #endregion

        public string GetLoclation(string zc)
        {
            string resut = "";
            switch (zc)
            {
                case "2011":
                    resut = "010901";
                    break;
                case "2014":
                    resut = "011001";
                    break;
                case "2015":
                    resut = "012601";
                    break;
                case "2018":
                    resut = "012701";
                    break;
                case "2019":
                    resut = "014701";
                    break;
                case "2022":
                    resut = "014801";
                    break;
                case "2002":
                    resut = "021001";
                    break;
                case "2024":
                    resut = "021206";
                    break;
                case "2026":
                    resut = "022006";
                    break;
                case "2004":
                    resut = "022301";
                    break;
                case "2028":
                    resut = "022806";
                    break;
                case "2030":
                    resut = "023406";
                    break;
                case "2006":
                    resut = "023801";
                    break;
                case "2008":
                    resut = "024401";
                    break;
                case "2032":
                    resut = "024806";
                    break;
                case "2034":
                    resut = "025406";
                    break;
                //原材料
                case "1015":
                    resut = "010201";
                    break;

                case "1001":
                    resut = "020101";
                    break;
                case "1002":
                    resut = "020201";
                    break;
                case "1018":
                    resut = "020601";
                    break;
                case "1021":
                    resut = "020701";
                    break;
                case "1025":
                    resut = "024801";
                    break;
                case "1027":
                    resut = "024901";
                    break;
                //成品库
                case "3006":
                    resut = "010205";
                    break;
                case "3008":
                    resut = "012405";
                    break;
                case "3002":
                    resut = "014401";
                    break;
                case "3004":
                    resut = "014801";
                    break;
                case "3007":
                    resut = "013501";
                    break;
                default:
                    resut = "0";
                    break;


            }
            return resut;
        }
    }
}