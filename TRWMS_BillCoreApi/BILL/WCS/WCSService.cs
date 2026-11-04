using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TRXWMS.BILL.DAL;
using TRXWMS.BILL.DAL.WMS;
using TRXWMS.BILL.EXTENSIONS;
using TRXWMS.BILL.LOG;
using TRXWMS.Models.Response;

using TRXWMS.BILL.MES;
using TRWMS_BillApi.Models.Response;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Xml.Linq;
using System.Xml;
using TRWMS_BillApi.Models.Request;
using System.Data;
using System.Diagnostics;
using SqlSugar.Extensions;
using TR.ORM.Model;

namespace TRXWMS.BILL.WCS
{
	public class WCSService
	{
		
		
		//public static DbContext context = new DbContext();

		public static DbContext dbcontext = new DbContext();
		public static MESService mesService = new MESService();

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
			string startcode = json["startcode"].ToString();
			string container = json["container"].ToString();
			string zonecode = json["zonecode"].ToString();
			string realweight = json["realweight"].ToString();
			if (container.Length > 0)
			{
				//查询入库绑定记录
				string sql = @"
SELECT DISTINCT A.ID AS ID,A.SOURCEORDER AS SOURCEORDER,A.CONTAINER AS CONTAINER,A.REMARK AS REMARK 
FROM wms_bill_putinbindingheader A left JOIN wms_bill_putinbindingbody B ON A.ID=B.HEADERID   
WHERE A.CONTAINER=@CONTAINER AND A.ENABLED=1 AND A.QUESTFLAG=0  AND  A.STATE=0 ORDER BY A.CREATETIME DESC ";
				var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

				  new SugarParameter("@CONTAINER",container)
				});
				if (bingID != null && bingID.Rows.Count > 0)
				{
					string ID = bingID.Rows[0]["ID"].ToString();
					//int TUNNELNUM = bingID.Rows[0]["TUNNELNUM"].ObjToInt();
					//string ITMECODE = bingID.Rows[0]["ITMECODE"].ToString();
					string REMARK = bingID.Rows[0]["REMARK"].ToString();
					string SOURCEORDER = bingID.Rows[0]["SOURCEORDER"].ToString();
					string CONTAINER = bingID.Rows[0]["CONTAINER"].ToString();

					//获取仓库配置

					sql = @"SELECT  A.ENTPRINCIPLE,A.OUTPRINCIPLE,B.ROWNUM,B.COLUMNNUM,B.SIGINNUM ,B.LAYERNUM   FROM wms_base_warehouse A   JOIN wms_base_zone B  ON  A.WAREHOUSECODE=B.WAREHOUSECODE WHERE A.ENABLED = 1 AND B.ENABLED = 1  AND B.ZONECODE = @ZONECODE  LIMIT 1";
					var warehouse = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
					{
						  new SugarParameter("@ZONECODE",zonecode)
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
								string code = "";
								if (zonecode == "201-1")
								{
									#region 库位分配  半成品库

									//按排列层托盘类型分配
									//托盘类型和入库口判断是否
									//1～27  1号  其他2号    托盘类型切入具体范围     先判断未放满时执行一套，放满时执行周转库位流程

									if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
										 startcode == "2024" || startcode == "2026") && (container.Substring(0, 2) == "21" || container.Substring(0, 2) == "22"))//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});

												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{

												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

														dbcontext.Db.Ado.CommitTran();

														reslutJson = new ReslutJson()
														{
															success = true,
															msg = "WCS请求成功!",
															data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

										}

									}

									else if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
										 startcode == "2024" || startcode == "2026") && (container.Substring(0, 2) == "25"))//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=7
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完需要周转时，申请前查看是否有未完成的周转任务
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{
												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=7
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//													//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

														dbcontext.Db.Ado.CommitTran();

														reslutJson = new ReslutJson()
														{
															success = true,
															msg = "WCS请求成功!",
															data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

										}

									}
									else if ((startcode == "2011" || startcode == "2014" || startcode == "2015" || startcode == "2018" || startcode == "2002" || startcode == "2004" ||
									   startcode == "2024" || startcode == "2026") && container.Substring(0, 2) != "25" && container.Substring(0, 2) != "21" && container.Substring(0, 2) != "22")//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM!=7 and LAYERNUM!=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{
												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM!=7 and LAYERNUM!=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

														dbcontext.Db.Ado.CommitTran();

														reslutJson = new ReslutJson()
														{
															success = true,
															msg = "WCS请求成功!",
															data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

										}

									}

									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
									   startcode == "2032" || startcode == "2034") && (container.Substring(0, 2) == "21" || container.Substring(0, 2) == "22"))//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{
												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														//													string sid = Guid.NewGuid().ToString("D");
														//													string sinsertsql = @"
														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
														//                       VALUES
														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
														//";
														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
														//					{

														//						new SugarParameter("@ID",sid),
														//						new SugarParameter("@CODE",sid),
														//						new SugarParameter("@ENDTAXI",code),
														//						new SugarParameter("@CONTAINER",CONTAINER),
														//						new SugarParameter("@STARTTAXI","022704"),
														//						new SugarParameter("@ZONECODE",zonecode),
														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

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
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

										}

									}
									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
									   startcode == "2032" || startcode == "2034") && (container.Substring(0, 2) == "25"))//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM=7
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{
												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM=7
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														//													string sid = Guid.NewGuid().ToString("D");
														//													string sinsertsql = @"
														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
														//                       VALUES
														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
														//";
														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
														//					{

														//						new SugarParameter("@ID",sid),
														//						new SugarParameter("@CODE",sid),
														//						new SugarParameter("@ENDTAXI",code),
														//						new SugarParameter("@CONTAINER",CONTAINER),
														//						new SugarParameter("@STARTTAXI","022704"),
														//						new SugarParameter("@ZONECODE",zonecode),
														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

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
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

										}

									}
									else if ((startcode == "2019" || startcode == "2022" || startcode == "2006" || startcode == "2008" || startcode == "2028" || startcode == "2030" ||
									   startcode == "2032" || startcode == "2034") && container.Substring(0, 2) != "25" && container.Substring(0, 2) != "21" && container.Substring(0, 2) != "22")//1号堆垛机范围
									{

										sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM>27  and LAYERNUM!=7 and LAYERNUM!=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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
											//1库位用完申请前查看是否有未完成的周转任务
											sql = @"SELECT  ISAVIALABLE   FROM wms_base_location  WHERE ZONECODE = @ZONECODE  and LOCATIONCODE='022704'";
											var scount = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											  new SugarParameter("@ZONECODE",zonecode)
										});
											if (scount.Rows[0]["ISAVIALABLE"].ObjToString() == "0")
											{
												reslutJson = new ReslutJson()
												{
													success = false,
													msg = "立库周转库位正在执行任务，请稍候尝试！!",
												};
												LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
											}
											else
											{
												sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  and COLUMNNUM<=27  and LAYERNUM!=7 and LAYERNUM!=6
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";

												var flocation = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
										{
											 new SugarParameter("@ZONECODE",zonecode)
										});

												if (flocation != null && flocation.Rows.Count > 0)
												{
													code = flocation.Rows[0]["LOCATIONCODE"].ObjToString();
													//需要周转时
													try
													{
														//更新库位信息   入库信息   生成堆垛机任务  
														dbcontext.Db.Ado.BeginTran();
														sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
														sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
					;
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@ENDTAXI","022704"),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});
														//更新周转库位为使用状态
														string upsql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
														dbcontext.Db.Ado.ExecuteCommand(upsql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",zonecode)

									});
														//更新入库单为正在处理		
														sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
														dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
														//													string sid = Guid.NewGuid().ToString("D");
														//													string sinsertsql = @"
														//INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
														//                       VALUES
														//                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
														//";
														//													dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
														//					{

														//						new SugarParameter("@ID",sid),
														//						new SugarParameter("@CODE",sid),
														//						new SugarParameter("@ENDTAXI",code),
														//						new SugarParameter("@CONTAINER",CONTAINER),
														//						new SugarParameter("@STARTTAXI","022704"),
														//						new SugarParameter("@ZONECODE",zonecode),
														//						new SugarParameter("@SOURCEORDER",SOURCEORDER)

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
													{ "conTainer", CONTAINER },
													{ "startTaxi", startcode },
													{ "endTaxi", "022704" }
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
														msg = "立库无可用库位！!",
													};
													LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
												}
											}

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


									#endregion
								}
								else if (zonecode == "301-1")
								{
									#region 库位分配  成品库时

									//按排列层托盘类型分配

									if (startcode != "")//1号堆垛机范围
									{
										//大托盘入库
										if (container.Substring(0, 2) == "32")
										{
											sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND (LAYERNUM<=3 OR (LAYERNUM=4 AND 
COLUMNNUM<=40) OR (LAYERNUM>=4 AND 
COLUMNNUM=53))  
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";
										}
										else
										{
											sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND ((LAYERNUM>3  AND COLUMNNUM!=54) OR (LAYERNUM=4 AND 
COLUMNNUM>40) )
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";
										}


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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});

												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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


									#endregion
								}
								else
								{
									#region 库位分配  原材料时

									//按排列层托盘类型分配

									if (startcode != "")//1号堆垛机范围
									{
										if (REMARK == "空托盘入库")
										{
											sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  AND (COLUMNNUM<62 AND COLUMNNUM>1)  
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";
										}
										else
										{
											sql = @"
SELECT  LOCATIONCODE,ROWNUM,COLUMNNUM,LAYERNUM FROM wms_base_location
WHERE ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1  AND ENABLED=1  
ORDER BY  ROWNUM ASC , LAYERNUM ASC,COLUMNNUM ASC  LIMIT 1
";
										}



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
												//更新库位信息   入库信息   生成堆垛机任务  
												dbcontext.Db.Ado.BeginTran();
												sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

											new SugarParameter("@REMARK",CONTAINER),
											new SugarParameter("@ZONECODE",zonecode),
											new SugarParameter("@LOCATIONCODE",code)
										});
												sql = @"
UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
WHERE   ID=@ID"
			;
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
												   new SugarParameter("@LOCATIONCODE",code),
													new SugarParameter("@ID",ID)

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
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",startcode),
						new SugarParameter("@ZONECODE",zonecode),
						new SugarParameter("@SOURCEORDER",SOURCEORDER)

					});

												//更新入库单为正在处理		
												sql = @"UPDATE wms_stocker_task  SET  STATE=1 WHERE   ID=@ID";
												dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

													new SugarParameter("@ID",id)

												});
												LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

												dbcontext.Db.Ado.CommitTran();

												reslutJson = new ReslutJson()
												{
													success = true,
													msg = "WCS请求成功!",
													data = new Dictionary<string, string>() {
													{"taskId", id },
													{"taskType", "1" },
													{ "conTainer", CONTAINER },
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


									#endregion
								}


								#region   作废
								//								if (code == "")
								//								{
								//									reslutJson = new ReslutJson()
								//									{
								//										success = false,
								//										msg = "立库无可用库位!",
								//									};
								//									LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "立库请求库位", ip);
								//								}
								//								else
								//								{
								//									try
								//									{
								//										//更新库位信息   入库信息   生成堆垛机任务  
								//										dbcontext.Db.Ado.BeginTran();
								//										sql = @"
								//UPDATE wms_base_location  SET ISAVIALABLE=0,REMARK=@REMARK
								//WHERE  ZONECODE=@ZONECODE AND TYPE='ST'  AND ISAVIALABLE=1   AND LOCATIONCODE=@LOCATIONCODE
								//";
								//										int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								//											new SugarParameter("@REMARK",CONTAINER),
								//											new SugarParameter("@ZONECODE",zonecode),
								//											new SugarParameter("@LOCATIONCODE",code)
								//										});
								//										sql = @"
								//UPDATE wms_bill_putinbindingheader  SET QUESTFLAG=1,QUESTTIME=sysdate(),LOCATIONCODE=@LOCATIONCODE,STATE=1 
								//WHERE   ID=@ID"
								//	;
								//										int b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
								//												   new SugarParameter("@LOCATIONCODE",code),
								//													new SugarParameter("@ID",ID)

								//												});
								//										//                                        sql = @"
								//										//UPDATE WMS_BILL_PUTINBINDINGBODY SET ENABLED=1 ,LASTEDITTIME=GETDATE() WHERE HEADERID=@ID AND BARCODE IN(" + sb.ToString().Substring(0, sb.Length - 1) + ")";
								//										//                                        int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
								//										//                                                    new SugarParameter("@ID",ID),
								//										//                                                });
								//										//                                        sql = @"
								//										//DELETE WMS_BILL_PUTINBINDINGBODY WHERE HEADERID=@ID AND BARCODE NOT IN(" + sb.ToString().Substring(0, sb.Length - 1) + ")";
								//										//                                        int d = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
								//										//                                                    new SugarParameter("@ID",ID),
								//										//                                                });
								//										dbcontext.Db.Ado.CommitTran();
								//										if (a > 0 && b > 0)
								//										{
								//											string id = Guid.NewGuid().ToString("D");
								//											wms_stocker_task wmsStocker = new wms_stocker_task();
								//											wmsStocker.ID = id;
								//											wmsStocker.CODE = id;
								//											wmsStocker.ENDTAXI = code;
								//											wmsStocker.CONTAINER = CONTAINER;
								//											wmsStocker.STARTTAXI = startcode;//起始点

								//											wmsStocker.TYPE = 1;
								//											wmsStocker.STARTTIME = DateTime.Now;
								//											wmsStocker.ZONECODE = zonecode;
								//											wmsStocker.SOURCEORDER = SOURCEORDER;
								//											wmsStocker.PRIORITY = 0;
								//											wmsStocker.STATE = 0;
								//											WmsStockerTaskDao.insert(wmsStocker);
								//											LOGService.WriteOperateLogT(string.Format("托盘条码:{0},库位:{1}", container, zonecode + "|" + code), "立库请求库位", ip);

								//											reslutJson = new ReslutJson()
								//											{
								//												success = true,
								//												msg = "WCS请求成功!",
								//												data = new Dictionary<string, string>() {
								//													{"taskId", id },
								//													{"taskType", "1" },
								//													{ "conTainer", CONTAINER },
								//													{ "startTaxi", startcode },
								//													{ "endTaxi", code }
								//												}
								//											};
								//										}
								//										else
								//										{
								//											dbcontext.Db.Ado.RollbackTran();
								//											reslutJson = new ReslutJson()
								//											{
								//												success = false,
								//												msg = "WMS内部错误",
								//											};
								//										}
								//									}

								//									catch (Exception ex)
								//									{
								//										dbcontext.Db.Ado.RollbackTran();
								//										reslutJson = new ReslutJson()
								//										{
								//											success = false,
								//											msg = ex.Message,
								//										};
								//										LOGService.WriteOperateLogF(string.Format("原因{0}", ex.Message), "托盘立库请求库位", ip);
								//									}
								//								}
								#endregion
							}
						}
						else
						{
							reslutJson = new ReslutJson()
							{
								success = false,
								msg = "立库配置单双伸类型不对!",
							};
							LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "托盘立库请求库位", ip);
						}

					}
					else
					{
						reslutJson = new ReslutJson()
						{
							success = false,
							msg = "请查看立库仓库配置!",
						};
						LOGService.WriteOperateLogF(string.Format("托盘条码:{0},原因:{1}", container, zonecode + reslutJson.msg), "托盘立库请求库位", ip);
					}
				}

				else
				{
					reslutJson = new ReslutJson()
					{
						success = false,
						msg = "错误:无入库单或入库单非新建状态!"
					};
				}
			}
			else
			{
				reslutJson = new ReslutJson()
				{
					success = false,
					msg = "WCS上传的条码为空!"
				};
			}

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
					if (reslutJson.success == true)
					{
						if (type != 2)
						{

							if (warehouseCode == "201-1" & endTaxi != "022701")
							{
								string url = "http://192.168.80.83:18080/esb/returnTaskResult/zhuhai/api";
								var content = new
								{
									TASK_NO = sourceOrder,
									TASK_TYPE = "IN",
									LENUM = container,
									BOUND_SUCCESS = 1,
									MESSAGE = "入库成功"
								};
								HttpRequestPost.HttpPostMethod(url, content);
								//交互成功执行的操作
							}
							else
							{
								string url = "http://192.168.80.83:18080/esb/returnTaskResult/zhuhai/api";
								var content = new
								{
									TASK_NO = sourceOrder,
									TASK_TYPE = "IN",
									LENUM = container,
									BOUND_SUCCESS = 1,
									MESSAGE = "入库成功"
								};
								HttpRequestPost.HttpPostMethod(url, content);
								//交互成功执行的操作
							}
							//else
							//{
							//	if (endTaxi.Length == 6)
							//	{
							//		string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
							//		var content = new
							//		{
							//			TASK_NO = sourceOrder,
							//			TASK_TYPE = "IN",
							//			LENUM = container,
							//			BOUND_SUCCESS = 1,
							//			MESSAGE = "入库成功"
							//		};
							//		HttpRequestPost.HttpPostMethod(url, content);
							//	}


							//}
						}
						else
						{
							if (warehouseCode == "201-1" & endTaxi != "022701")
							{
								string url = "http://192.168.80.83:18080/esb/returnTaskResult/zhuhai/api";
								var content = new
								{
									TASK_NO = sourceOrder,
									TASK_TYPE = "OUT",
									LENUM = container,
									BOUND_SUCCESS = 1,
									MESSAGE = "出库成功"
								};

								HttpRequestPost.HttpPostMethod(url, content);

							}
							else
							{
								string url = "http://192.168.80.83:18080/esb/returnTaskResult/zhuhai/api";
								var content = new
								{
									TASK_NO = sourceOrder,
									TASK_TYPE = "OUT",
									LENUM = container,
									BOUND_SUCCESS = 1,
									MESSAGE = "出库成功"
								};

								HttpRequestPost.HttpPostMethod(url, content);
							}



						}
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
						else
						{
							//入到冷藏库时
							if (endTaxi.Length == 6)
							{
								reslutJson = completeEnterKu(sourceOrder, warehouseCode, container, endTaxi, id, ip);
							}
							else//直接出到出库口时
							{
								#region 生成空托盘出库任务


								//查询出托盘类型
								string CONTAINERTYPE = "0";

								//获取空托盘信息
								string gsql = @"
SELECT a.SOURCEORDER,a.CONTAINER,a.ZONECODE,a.LOCATIONCODE  FROM wms_base_location  a    WHERE a.ZONECODE=@ZONECODE   AND  ''!=a.CONTAINER  AND a.CONTAINERTYPE=@CONTAINERTYPE 
AND  CONTAINER  NOT IN(SELECT  CONTAINER  from wms_stocker_task  where ZONECODE=@ZONECODE and  TYPE=2 and  STATE<=1)
AND  (SELECT  COUNT(*)  from  wms_bill_inventorybook  b  where b.SOURCEORDER=a.SOURCEORDER  and  b.BARCODE=a.CONTAINER)=0 ORDER BY  a.ROWNUM ASC LIMIT 1 
";
								var glist = dbcontext.Db.Ado.GetDataTable(gsql, new List<SugarParameter>()
					{

						new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
						new SugarParameter("@ZONECODE","401-1")

					});
								//有空托盘信息时
								if (glist != null && glist.Rows.Count > 0)
								{   //获取出库起点
									string ZONECODE = glist.Rows[0]["ZONECODE"].ToString();
									string LOCATIONCODE = glist.Rows[0]["LOCATIONCODE"].ToString();
									string SOURCEORDER = glist.Rows[0]["SOURCEORDER"].ToString();
									string CONTAINER = glist.Rows[0]["CONTAINER"].ToString();
									sql = @"
select * from wms_stocker_task  WHERE ID=@ID
";
									var aj = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
					new SugarParameter("@ID",id),

				});
									string TARGET = aj.Rows[0]["STARTTAXI"].ToString();

									string exid = Guid.NewGuid().ToString("D");
									string insertsql = @"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER ,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@ZONECODE,@SOURCEORDER,@CONTAINER,@CONTAINERTYPE,0,0,@TARGET,@LOCATIONCODE,1,'空托盘出库','SAP',sysdate()) 
";
									int ak = dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
					{

						new SugarParameter("@ID",exid),
						new SugarParameter("@ZONECODE","401-1"),
						new SugarParameter("@SOURCEORDER",exid),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@CONTAINERTYPE",CONTAINERTYPE),
						new SugarParameter("@TARGET",TARGET),
						new SugarParameter("@LOCATIONCODE",LOCATIONCODE)

					});

									//生成堆垛机任务								
									string taskid = Guid.NewGuid().ToString("D");
									sql = @"
INSERT INTO wms_stocker_task (ID,CODE,SOURCEORDER ,ZONECODE,CONTAINER,TYPE,PRIORITY,STARTTAXI,ENDTAXI,STATE,STARTTIME,CREATOR,
CREATETIME,LASTEDITOR,LASTEDITTIME
)
                       VALUES
                       (@ID,@CODE,@SOURCEORDER,@ZONECODE,@CONTAINER,2,0,@STARTTAXI,@ENDTAXI,0,sysdate(),'WMS',sysdate(),'WMS',sysdate()) 
";
									dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>()
					{

						new SugarParameter("@ID",taskid),
						new SugarParameter("@CODE",taskid),
						new SugarParameter("@SOURCEORDER",exid),
						new SugarParameter("@ZONECODE",ZONECODE),
						new SugarParameter("@CONTAINER",CONTAINER),
						new SugarParameter("@STARTTAXI",LOCATIONCODE),
						new SugarParameter("@ENDTAXI",TARGET)

					});

								}
								else
								{
									mymes = "立库无可用空托盘，待有空托盘后请手动出空托盘！";

								}


								#endregion
								reslutJson = new ReslutJson()
								{
									success = true,
									msg = "已确认出库已成功!" + mymes
								};
							}
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
					if (reslutJson.success == true)
					{
						if (type != 2)
						{
							if (warehouseCode != "401-1")
							{
								string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
								//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
								var content = new
								{
									TASK_NO = sourceOrder,
									TASK_TYPE = "IN",
									LENUM = container,
									BOUND_SUCCESS = 1,
									MESSAGE = "入库成功" + mymes
								};
								HttpRequestPost.HttpPostMethod(url, content);
								//交互成功执行的操作
							}
							else
							{
								if (endTaxi.Length == 6)
								{
									string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
									//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
									var content = new
									{
										TASK_NO = sourceOrder,
										TASK_TYPE = "IN",
										LENUM = container,
										BOUND_SUCCESS = 1,
										MESSAGE = "入库成功" + mymes
									};
									HttpRequestPost.HttpPostMethod(url, content);
								}


							}


						}
						else
						{
							string url = "http://192.168.80.83:18080/esb/returnTaskResult/api";
							//string url = "http://192.168.80.57:18080/esb/returnTaskResult/api";
							var content = new
							{
								TASK_NO = sourceOrder,
								TASK_TYPE = "OUT",
								LENUM = container,
								BOUND_SUCCESS = 1,
								MESSAGE = "出库成功" + mymes
							};

							HttpRequestPost.HttpPostMethod(url, content);


						}
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


					//更新库区绑定关系
					sql = @"
SELECT CONTAINERTYPE 
FROM wms_bill_putinbindingheader 
WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
					var ss = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)
				});

					var containertype = ss.Rows[0][0].ToString();
					//半成品库时
					if (warehouseCode == "201-1")
					{
						if (endTaxi != "022704")//非周转任务
						{
							sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=0,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
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
							//更新入库单状态
							sql = @"
UPDATE wms_bill_putinbindingheader SET STATE =2,FINISHTIME=sysdate() WHERE SOURCEORDER=@SOURCEORDER AND CONTAINER=@CONTAINER AND LOCATIONCODE=@LOCATIONCODE  AND  QUESTFLAG=1 AND STATE=1
";
							int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container),
								new SugarParameter("@LOCATIONCODE",endTaxi),
				});

							//更新库存状态   空托盘入库不需更新库存 
							sql = @"
SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE 
FROM wms_bill_putinbindingheader 
WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
							var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)
				});
							var headid = bingID.Rows[0][0].ToString();

							sql = @"
SELECT *
FROM wms_bill_putinbindingbody 
WHERE HEADERID=@HEADERID   ";
							var bodylist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

								 new SugarParameter("@HEADERID",headid)
				});
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
									string insertsql = @"
INSERT INTO wms_bill_inventorybook (ID,SOURCEORDER,BARCODE,ITEMNAME,ITEMCODE,LOT,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @BARCODE,@ITEMNAME,@ITEMCODE,@LOT,'','SAP',sysdate()) 
";
									dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
					{

						new SugarParameter("@ID",myid),
						new SugarParameter("@SOURCEORDER",sourceOrder),
						new SugarParameter("@BARCODE",container),
						new SugarParameter("@ITEMNAME",TASK_NO_SN),
						new SugarParameter("@ITEMCODE",ITEMCODE),
						new SugarParameter("@LOT",LOT)

					});
								}

							}
						}

						else
						{
							//						sql = @"
							//UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=0,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
							//";
							//						b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
							//										new SugarParameter("@SOURCEORDER",sourceOrder),
							//										new SugarParameter("@CONTAINERTYPE",containertype),
							//										new SugarParameter("@CONTAINER",container),
							//										new SugarParameter("@LOCATIONCODE",endTaxi),
							//										new SugarParameter("@ZONECODE",warehouseCode)

							//									});

							//更新堆垛机状态
							sql = @"
UPDATE wms_stocker_task SET STATE=2 ,FINISHTIME=sysdate(),LASTEDITOR='WCS' WHERE ID=@ID
";
							int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					new SugarParameter("@ID",id),

				});
							//查询入库信息  生成堆垛机周转任务
							sql = @"
SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE ,LOCATIONCODE 
FROM wms_bill_putinbindingheader 
WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
							var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)
				});
							var endcode = bingID.Rows[0][4].ToString();

							string sid = Guid.NewGuid().ToString("D");
							string sinsertsql = @"
						INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
						                       VALUES
						                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,1,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
						";
							dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
											{

												new SugarParameter("@ID",sid),
												new SugarParameter("@CODE",sid),
												new SugarParameter("@ENDTAXI",endcode),
												new SugarParameter("@CONTAINER",container),
												new SugarParameter("@STARTTAXI","022704"),
												new SugarParameter("@ZONECODE",warehouseCode),
												new SugarParameter("@SOURCEORDER",sourceOrder)

											});
							//更新周转库位为使用状态
							sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
							b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",warehouseCode)

									});

						}
					}
					//其他库
					else
					{

						sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=0,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
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
						//更新入库单状态
						sql = @"
UPDATE wms_bill_putinbindingheader SET STATE =2,FINISHTIME=sysdate() WHERE SOURCEORDER=@SOURCEORDER AND CONTAINER=@CONTAINER AND LOCATIONCODE=@LOCATIONCODE  AND  QUESTFLAG=1 AND STATE=1
";
						int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container),
								new SugarParameter("@LOCATIONCODE",endTaxi),
				});

						//更新库存状态   空托盘入库不需更新库存 
						sql = @"
SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE 
FROM wms_bill_putinbindingheader 
WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
						var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)
				});
						var headid = bingID.Rows[0][0].ToString();

						sql = @"
SELECT *
FROM wms_bill_putinbindingbody 
WHERE HEADERID=@HEADERID   ";
						var bodylist = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

								 new SugarParameter("@HEADERID",headid)
				});
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
								string insertsql = @"
INSERT INTO wms_bill_inventorybook (ID,SOURCEORDER,BARCODE,ITEMNAME,ITEMCODE,LOT,REMARK,CREATOR,CREATETIME)
                       VALUES
                       (@ID,@SOURCEORDER, @BARCODE,@ITEMNAME,@ITEMCODE,@LOT,'','SAP',sysdate()) 
";
								dbcontext.Db.Ado.ExecuteCommand(insertsql, new List<SugarParameter>()
					{

						new SugarParameter("@ID",myid),
						new SugarParameter("@SOURCEORDER",sourceOrder),
						new SugarParameter("@BARCODE",container),
						new SugarParameter("@ITEMNAME",TASK_NO_SN),
						new SugarParameter("@ITEMCODE",ITEMCODE),
						new SugarParameter("@LOT",LOT)

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
					LOGService.WriteOperateLogT(string.Format("任务号:{0},库位:{1}", sourceOrder, warehouseCode + "|" + endTaxi), "入库完成", ip);


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
					//半成品库报完成
					if (warehouseCode == "201-1")
					{
						if (endTaxi != "022704" & startTaxi != "022704")
						{
							//更新库区绑定关系
							sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=1,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
";
							b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
									 new SugarParameter("@SOURCEORDER",""),
										new SugarParameter("@CONTAINER",""),
										new SugarParameter("@CONTAINERTYPE",""),
										new SugarParameter("@LOCATIONCODE",startTaxi),
										new SugarParameter("@ZONECODE",warehouseCode)

									});

							//更新堆垛机状态
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
DELETE FROM wms_bill_inventorybook  WHERE SOURCEORDER=@SOURCEORDER AND BARCODE=@BARCODE  
";
							dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@BARCODE",container)

				});

						}
						else if (startTaxi == "022704")
						{
							sql = @"
select LOCATIONCODE from wms_bill_putoutheader  WHERE SOURCEORDER=@SOURCEORDER AND CONTAINER=@CONTAINER 
";
							var ss = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)

				});

							var location = ss.Rows[0][0].ToString();
							//更新库区绑定关系
							sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=1,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
";
							b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
									 new SugarParameter("@SOURCEORDER",""),
										new SugarParameter("@CONTAINER",""),
										new SugarParameter("@CONTAINERTYPE",""),
										new SugarParameter("@LOCATIONCODE",location),
										new SugarParameter("@ZONECODE",warehouseCode)

									});
							//更新周转库位状态
							sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=1  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
							b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",warehouseCode)

									});

							//更新堆垛机状态
							sql = @"
UPDATE wms_stocker_task SET STATE=2 ,FINISHTIME=sysdate(),LASTEDITOR='WCS' WHERE ID=@ID
";
							int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					new SugarParameter("@ID",id),

				});
							//更新出库单状态
							sql = @"
UPDATE wms_bill_putoutheader SET STATE =2,FINISHTIME=sysdate() WHERE SOURCEORDER=@SOURCEORDER AND CONTAINER=@CONTAINER 
";
							int c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)

				});
							//删除库存
							sql = @"
DELETE FROM wms_bill_inventorybook  WHERE SOURCEORDER=@SOURCEORDER AND BARCODE=@BARCODE  
";
							dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@BARCODE",container)

				});

						}
						else
						{
							//						//更新库区绑定关系
							//						sql = @"
							//UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=1,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
							//";
							//						b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
							//									 new SugarParameter("@SOURCEORDER",""),
							//										new SugarParameter("@CONTAINER",""),
							//										new SugarParameter("@CONTAINERTYPE",""),
							//										new SugarParameter("@LOCATIONCODE",startTaxi),
							//										new SugarParameter("@ZONECODE",warehouseCode)

							//									});

							//更新堆垛机状态
							sql = @"
UPDATE wms_stocker_task SET STATE=2 ,FINISHTIME=sysdate(),LASTEDITOR='WCS' WHERE ID=@ID
";
							int a = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					new SugarParameter("@ID",id),

				});
							//查询出库信息  生成堆垛机周转任务
							sql = @"
SELECT ID,SOURCEORDER,CONTAINER ,CONTAINERTYPE ,TARGET 
FROM wms_bill_putoutheader 
WHERE CONTAINER=@CONTAINER  AND SOURCEORDER=@SOURCEORDER ";
							var bingID = dbcontext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@CONTAINER",container)
				});
							var endcode = bingID.Rows[0][4].ToString();

							string sid = Guid.NewGuid().ToString("D");
							string sinsertsql = @"
						INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME)
						                       VALUES
						                       (@ID,@CODE, @ENDTAXI,@CONTAINER,@STARTTAXI,2,sysdate(),@ZONECODE,@SOURCEORDER,0,0,sysdate(),'WMS','WMS',sysdate()) 
						";
							dbcontext.Db.Ado.ExecuteCommand(sinsertsql, new List<SugarParameter>()
											{

												new SugarParameter("@ID",sid),
												new SugarParameter("@CODE",sid),
												new SugarParameter("@ENDTAXI",endcode),
												new SugarParameter("@CONTAINER",container),
												new SugarParameter("@STARTTAXI","022704"),
												new SugarParameter("@ZONECODE",warehouseCode),
												new SugarParameter("@SOURCEORDER",sourceOrder)

											});
							//更新周转库位为使用状态
							sql = @"
UPDATE wms_base_location  SET ISAVIALABLE=0  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE  
";
							b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

										new SugarParameter("@LOCATIONCODE","022704"),
										new SugarParameter("@ZONECODE",warehouseCode)

									});
							//删除库存
							//						sql = @"
							//DELETE FROM wms_bill_inventorybook  WHERE SOURCEORDER=@SOURCEORDER AND BARCODE=@BARCODE  
							//";
							//						dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

							//								new SugarParameter("@SOURCEORDER",sourceOrder),
							//								 new SugarParameter("@BARCODE",container)

							//				});

						}
					}
					//其他库位报完成时
					else
					{

						//更新库区绑定关系
						sql = @"
UPDATE wms_base_location  SET SOURCEORDER=@SOURCEORDER,CONTAINER=@CONTAINER,ISAVIALABLE=1,CONTAINERTYPE=@CONTAINERTYPE  WHERE ZONECODE=@ZONECODE  AND LOCATIONCODE=@LOCATIONCODE 
";
						b = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
									 new SugarParameter("@SOURCEORDER",""),
										new SugarParameter("@CONTAINER",""),
										new SugarParameter("@CONTAINERTYPE",""),
										new SugarParameter("@LOCATIONCODE",startTaxi),
										new SugarParameter("@ZONECODE",warehouseCode)

									});

						//更新堆垛机状态
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
DELETE FROM wms_bill_inventorybook  WHERE SOURCEORDER=@SOURCEORDER AND BARCODE=@BARCODE  
";
						dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {

								new SugarParameter("@SOURCEORDER",sourceOrder),
								 new SugarParameter("@BARCODE",container)

				});


					}

					dbcontext.Db.Ado.CommitTran();
					#region   其他业务
					//                if(IFCOUNT == "盘点")
					//                {
					//                    sql = @"
					//UPDATE WMS_BILL_COUNTDETAIL SET CHECKFLAG=2 WHERE SOURCEORDER = @SOURCEORDER AND WAREHOUSECODE=@WAREHOUSECODE AND LOCATIONCODE=@CODE
					//";
					//                    c = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					//                    new SugarParameter("@CODE",startTaxi),
					//                    new SugarParameter("@SOURCEORDER",sourceOrder),
					//                    new SugarParameter("@WAREHOUSECODE",warehouseCode),
					//                    });

					//                    sql = @"
					//INSERT INTO WMS_REPORT_WORK(ID,SOURCEORDER,BARCODE,ITEMCODE,ITEMNAME,TYPE,QTY,STATE,REMARK,
					//WAREHOUSECODE,CREATOR,CREATETIME,LASTEDITOR,LASTEDITTIME)
					//SELECT NEWID(),SOURCEORDER,BARCODE,ITEMCODE,ITEMNAME,2,QTY,0,REMARK,WAREHOUSECODE,'WMS',GETDATE(),'WMS',GETDATE()
					//FROM WMS_BILL_INVENTORYBOOK WHERE WAREHOUSECODE=@WAREHOUSECODE AND LOCATIONCODE=@LOCATIONCODE
					//";
					//                    e = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					//                    new SugarParameter("@LOCATIONCODE",startTaxi),
					//                    //new SugarParameter("@SOURCEORDER",sourceOrder),
					//                    new SugarParameter("@WAREHOUSECODE",warehouseCode),

					//                    });

					//                    sql = @"
					//UPDATE WMS_BILL_INVENTORYBOOK SET ZONECODE = NULL, LOCATIONCODE = NULL, STOCKFLAG = 0, STOCKTIME = NULL, FREEZEFLAG = 0, FREEZEREASON = '待入库' 
					//WHERE WAREHOUSECODE=@WAREHOUSECODE AND LOCATIONCODE=@LOCATIONCODE
					//";
					//                    d = dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
					//                    new SugarParameter("@LOCATIONCODE",startTaxi),
					//                    //new SugarParameter("@SOURCEORDER",sourceOrder),
					//                    new SugarParameter("@WAREHOUSECODE",warehouseCode),

					//                    });

					//                }
					//else
					//{
					#endregion

					//}


					//Stopwatch sp = new Stopwatch();
					//sp.Start();


					//sp.Stop();
					//TimeSpan ts = sp.Elapsed;
					//LOGService.WriteOperateLogF(string.Format("任务号:{0},库位:{1},执行时间{2}", sourceOrder, startTaxi, ts.TotalSeconds.ToString()), "出库完成", ip);
					reslutJson = new ReslutJson()
					{
						success = true,
						msg = "已确认出库已成功!",
					};
					//下发输送线任务
					// B01_NewTransportTask(sourceOrder,ip);
					LOGService.WriteOperateLogT(string.Format("任务号:{0},库位:{1}", sourceOrder, warehouseCode + "|" + startTaxi), "出库完成", ip);
					#region  zhushi
					//if (a > 0 && b > 0 && c > 0)
					//{
					//	//上报sap
					//	//		string Url = System.Configuration.ConfigurationManager.AppSettings["sapUrl"].ToString();
					//	//		string GetClasssApi_url = Url + "/sap/UPDATE_TASK_BOUND";
					//	//		string guid = Guid.NewGuid().ToString();
					//	//		string xmlstr = @"<ROOT><GUID> </GUID >   < TYPE > UPDATE_TASK_BOUND </ TYPE >
					//	//< DATA >
					//	//< TASK_NO > 1231213 </ TASK_NO >
					//	//< LENUM > 201231231234 </ LENUM >
					//	//< BOUND_SUCCESS > 0 </ BOUND_SUCCESS >
					//	//< MESSAGE > 因“”原因未出入库 </ MESSAGE >
					//	//   </ DATA >
					//	//   </ ROOT > ";
					//	//		var reslut = HttpPostMethod(GetClasssApi_url, xmlstr);

					//}
					//else
					//{

					//	var content = new
					//	{
					//		TASK_NO = sourceOrder,
					//		TASK_TYPE = "OUT",
					//		LENUM = container,
					//		BOUND_SUCCESS = 1,
					//		MESSAGE = "出库成功"
					//	};

					//	HttpRequestPost.HttpPostMethod(url, content);
					//	dbcontext.Db.Ado.RollbackTran();
					//	reslutJson = new ReslutJson()
					//	{
					//		success = false,
					//		msg = "WMS内部错误!",
					//	};
					//	LOGService.WriteOperateLogF(string.Format("任务号:{0},库位:{1}", sourceOrder, startTaxi), "出库完成", ip);
					//}
					#endregion
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
				string warehtunnelNum = json["tunnelNum"].ToString();
				reslutJson = A01_NewTask(warehouse, ip, warehtunnelNum);

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
				//半成品库
				if (warehouse == "201-1")
				{

					//1号堆垛机出库任务和周转任务
					if (tunnelNum == "1")
					{
						//获取最早出入库任务，优先级，创建时间
						//					string sql = @"
						//			SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
						//			FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 AND  ((TYPE=2  AND Convert(SUBSTRING(STARTTAXI,3,2),UNSIGNED)<=27
						//AND ENDTAXI  IN('2011','2014','2015','2018','2002','2004','2024','2026','022704') )
						//	OR (TYPE=1 AND STARTTAXI='022704' AND Convert(SUBSTRING(ENDTAXI,3,2),UNSIGNED)<=27  ))
						//			ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
						//			";
						string sql = @"
				SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
				FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 AND  ((TYPE=2  AND Convert(SUBSTRING(STARTTAXI,3,2),UNSIGNED)<=27
 AND ENDTAXI  IN('2011','2014','2015','2018','2002','2004','2024','2026','022704') )
		OR (TYPE=1 AND STARTTAXI  IN('2011','2014','2015','2018','2002','2004','2024','2026') AND Convert(SUBSTRING(ENDTAXI,3,2),UNSIGNED)<=27  ))
				ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
						//
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
													{ "endTaxi", endTaxi }


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
					else
					{
						//获取最早出库任务，优先级，创建时间
						string sql = @"
				SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
				FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 AND  ((TYPE=2  AND Convert(SUBSTRING(STARTTAXI,3,2),UNSIGNED)>=27
 AND ENDTAXI  IN('2019','2022','2015','2006','2008','2028','2030','2032','2034','022704'))
	OR (TYPE=1 AND STARTTAXI  IN('2019','2022','2015','2006','2008','2028','2030','2032') AND (Convert(SUBSTRING(ENDTAXI,3,2),UNSIGNED)>=28 OR ENDTAXI='022704') ))
				ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
						//	
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
													{ "endTaxi", endTaxi }


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
				//其他库时
				else
				{

					//获取最早出库任务，优先级，创建时间
					string sql = @"
				SELECT  ID,CONTAINER,PRIORITY,STARTTAXI,ENDTAXI,TYPE
				FROM wms_stocker_task WHERE ZONECODE=@ZONECODE  AND STATE=0 
				ORDER BY PRIORITY DESC,CREATETIME ASC  LIMIT 1
				";
					//
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
													{ "endTaxi", endTaxi }


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


		#region    看板交互


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