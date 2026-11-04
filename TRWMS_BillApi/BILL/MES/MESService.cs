using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TR.ORM.Model;
using TRXWMS.BILL.DAL;
using TRXWMS.BILL.DAL.WMS;
using TRXWMS.BILL.EXTENSIONS;
using TRXWMS.BILL.LOG;
using TRXWMS.BILL.WMS;
using TRXWMS.Models.Request;
using TRXWMS.Models.Response;

namespace TRXWMS.BILL.MES
{
    public class MESService
    {
        //public static DbContext dbContext = new DbContext();

        public static WMSService wMSService = new WMSService();

//        public ReslutJson InRequest(JObject json, string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                InRequest inReuqst = JsonConvert.DeserializeObject<InRequest>(JsonConvert.SerializeObject(json));
//                string sql = @"
//  SELECT COUNT(ID) AS QTY FROM WMS_BILL_INVENTORYBOOK WHERE BARCODE=@BARCODE
//";
//                var QTY = dbContext.Db.Ado.GetInt(sql, new List<SugarParameter>()
//            {
//                new SugarParameter("@BARCODE",inReuqst.Barcode)
//            });
//                if (QTY > 0)
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "WMS库存存在条码信息"
//                    };
//                }
//                else
//                {
//                    sql = @"
//SELECT ENTRANCE FROM WMS_BASE_MASTER WHERE ITEMCODE = @ITEMCODE AND IFUSE=1
//";
//                    var ENTRANCE = dbContext.Db.Ado.GetString(sql, new List<SugarParameter>() {
//                    new SugarParameter("@ITEMCODE",inReuqst.ItemCode),
//                });
//                    if (!string.IsNullOrEmpty(ENTRANCE))
//                    {
//                        WMS_BILL_PUTIN wmsBillPutIn = new WMS_BILL_PUTIN()
//                        {
//                            ID = Guid.NewGuid().ToString("D"),
//                            SOURCEORDER = inReuqst.SourceOrder,
//                            BARCODE = inReuqst.Barcode,
//                            CONTAINER = inReuqst.Barcode,
//                            ITEMCODE = inReuqst.ItemCode,
//                            ITEMNAME = inReuqst.ItemName,
//                            LOT = inReuqst.Lot,
//                            ENABLED = true,
//                            WAREHOUSECODE = ENTRANCE,
//                            QTY = inReuqst.Qty,
//                            REMARK = "正常",
//                            CREATETIME = DateTime.Now,
//                            CREATOR = "MES",
//                            LASTEDITOR = "MES",
//                            LASTEDITTIME = DateTime.Now

//                        };
//                        WMS_BILL_INVENTORYBOOK wMS_BILL_INVENTORYBOOK = new WMS_BILL_INVENTORYBOOK()
//                        {
//                            ID = Guid.NewGuid().ToString("D"),
//                            SOURCEORDER = inReuqst.SourceOrder,
//                            SOURCEORDERTYPE="1",
//                            SYSBOLCODE = inReuqst.SourceOrder + inReuqst.Barcode,
//                            BARCODE = inReuqst.Barcode,
//                            ITEMCODE = inReuqst.ItemCode,
//                            ITEMNAME = inReuqst.ItemName,
//                            LOT = inReuqst.Lot,
//                            WAREHOUSECODE = ENTRANCE,
//                            QTY = inReuqst.Qty,
//                            FREEZEFLAG = 0,
//                            REMARK = "正常",
//                            FREEZETIME = DateTime.Now,
//                            FREEZEREASON = "待入库",
//                            CREATETIME = DateTime.Now,
//                            CREATOR = "MES",
//                            LASTEDITOR = "MES",
//                            LASTEDITTIME = DateTime.Now,
//                            FEEDBACKFLAG=1,
//                            FEEDBACKTIME = DateTime.Now,

//                        };

//                        int a = WmsBillPutInDao.insert(wmsBillPutIn);
//                        int b = WmsBillInventoryBookDao.insert(wMS_BILL_INVENTORYBOOK);
//                        if (a > 0 && b > 0)
//                        {
//                            reslutJson = new ReslutJson()
//                            {
//                                success = true,
//                                msg = "接收入库单"
//                            };
//                            LOGService.WriteOperateLogT(string.Format("条码:{0},单据:{1}", inReuqst.Barcode, inReuqst.SourceOrder), "MES下发入库单", ip);
//                        }
//                    }
//                    else
//                    {
//                        reslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "未配置物料主表"
//                        };
//                        LOGService.WriteOperateLogF(string.Format("条码:{0},单据:{1},原因:{2}", inReuqst.Barcode, inReuqst.SourceOrder, reslutJson.msg), "MES下发入库单", ip);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };

//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }

//        public ReslutJson OutRequest(JObject json, string ip) {

//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                OutRequest outReuqst = JsonConvert.DeserializeObject<OutRequest>(JsonConvert.SerializeObject(json));
//                //判断是否库存
//                string sql = @"
//SELECT B.ID AS ID, A.QTY AS QTY FROM 
//WMS_BILL_INVENTORYBOOK A  RIGHT JOIN WMS_BASE_MASTER B 
//ON A.ITEMCODE=B.ITEMCODE AND A.STOCKFLAG=1 AND A.REMARK=@Sign AND B.IFUSE=1  
//WHERE B.ITEMCODE=@ITMECODE
//";
//                var INVENTORYBOOK = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>()
//                {
//                    new SugarParameter("@ITMECODE",outReuqst.ItemCode),
//                    new SugarParameter("@Sign",outReuqst.Sign)
//                });
//                if (INVENTORYBOOK !=null && INVENTORYBOOK.Rows.Count>0)
//                {
//                    if (string.IsNullOrEmpty(INVENTORYBOOK.Rows[0]["QTY"].ObjToString()))
//                    {

//                        reslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "WMS物料库存不足"
//                        };
//                        LOGService.WriteOperateLogF(string.Format("物料:{0},单据:{1},原因:{2}", outReuqst.ItemCode, outReuqst.SourceOrder, reslutJson.msg), "MES下发出库单", ip);
//                    }
//                    else {
//                        int a = 0, b = 0,c=0;
//                        if (outReuqst.CtnQty ==0)
//                        {
//                            string guid = Guid.NewGuid().ToString("D").Replace("-", "");
//                            a = WmsBillPutoutHeaderDao.insert(new WMS_BILL_PUTOUTHEADER()
//                            {
//                                SOURCEORDER = outReuqst.SourceOrder,
//                                ID = guid,
//                                SUGGESTTIME = DateTime.Now.AddMinutes(30),
//                                SOURCEORDERTYPE = "1",
//                                APPOVEFLAG = -1,
//                                CREATETIME = DateTime.Now,
//                                CREATOR = "MES",
//                                LASTEDITOR = "MES",
//                                LASTEDITTIME = DateTime.Now,
//                                REMARK=outReuqst.Sign

//                            });
//                            b = WmsBillPutoutBodyDao.insert(new WMS_BILL_PUTOUTBODY()
//                            {
//                                ID = Guid.NewGuid().ToString("D"),
//                                HEADERID = guid,
//                                ITEMCODE = outReuqst.ItemCode,
//                                ITEMNAME = outReuqst.ItemName,
//                                LOT = outReuqst.Lot,
//                                PLANT = outReuqst.OutLine,
//                                OUTLINE = outReuqst.OutLine,
//                                AGVLINE = outReuqst.OutLine,
//                                REMARK = outReuqst.Sign

//                            });
//                            c = 1;
//                        }
//                        else {
//                            string guid = Guid.NewGuid().ToString("D").Replace("-", "");
//                            a = WmsBillPutoutHeaderDao.insert(new WMS_BILL_PUTOUTHEADER()
//                            {
//                                SOURCEORDER = outReuqst.SourceOrder,
//                                ID = guid,
//                                SUGGESTTIME = DateTime.Now.AddMinutes(30),
//                                SOURCEORDERTYPE = "2",
//                                APPOVEFLAG = -1,
//                                CREATETIME = DateTime.Now,
//                                CREATOR = "MES",
//                                LASTEDITOR = "MES",
//                                LASTEDITTIME = DateTime.Now,
//                                REMARK = outReuqst.Sign

//                            });
//                            b = WmsBillPutoutBodyDao.insert(new WMS_BILL_PUTOUTBODY()
//                            {
//                                ID = Guid.NewGuid().ToString("D"),
//                                HEADERID = guid,
//                                ITEMCODE = outReuqst.ItemCode,
//                                ITEMNAME = outReuqst.ItemName,
//                                LOT = outReuqst.Lot,
//                                PLANT = outReuqst.OutLine,
//                                OUTLINE = outReuqst.OutLine,
//                                AGVLINE = outReuqst.OutLine,
//                                CTNQTY = outReuqst.CtnQty,
//                                SEQUENCE=outReuqst.Sequence,
//                                REMARK = outReuqst.Sign
//                            });
//                            sql = @"
//UPDATE WMS_BILL_PUTOUTHEADER SET APPOVEFLAG=0 WHERE SOURCEORDER=@SOURCEORDER
//";
//                            c = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                                new SugarParameter("@SOURCEORDER",outReuqst.SourceOrder),
//                            });
//                        }
                        
//                        if (a > 0 && b > 0 && c>0) {

//                            reslutJson = new ReslutJson()
//                            {
//                                success = true,
//                                msg = "接收成功"
//                            };
//                            LOGService.WriteOperateLogT(string.Format("物料:{0},单据:{1},原因:{2}", outReuqst.ItemCode, outReuqst.SourceOrder, reslutJson.msg), "MES下发出库单", ip);
//                        }
//                    }                  
//                }
//                else {

//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "物料主表属性配置错误"
//                    };
//                    LOGService.WriteOperateLogF(string.Format("物料:{0},单据:{1},原因:{2}", outReuqst.ItemCode, outReuqst.SourceOrder, reslutJson.msg), "MES下发出库单", ip);
//                }
//            }
//            catch (Exception ex )
//            {

//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };

//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }

//        public ReslutJson AgvRequest(JObject json, string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                AgvTaskRequest agvReuqstList = JsonConvert.DeserializeObject<AgvTaskRequest>(JsonConvert.SerializeObject(json));
//                WMS_AGV_TASK wMS_AGV_TASK = new WMS_AGV_TASK();
//                    string sql = @"
//SELECT ID FROM WMS_AGV_TASK WHERE SOURCEORDER=@SOURCEORDER
//";
//                    var ID = dbContext.Db.Ado.GetString(sql, new List<SugarParameter>()
//                {
//                    new SugarParameter("@SOURCEORDER",agvReuqstList.SourceOrder)
//                });
//                if (string.IsNullOrEmpty(ID))
//                {
//                    int a = 0;
//                    wMS_AGV_TASK = new WMS_AGV_TASK();
//                    wMS_AGV_TASK.SOURCEORDER = agvReuqstList.SourceOrder;
//                    wMS_AGV_TASK.STARTTAXI = agvReuqstList.StartTaxi;
//                    wMS_AGV_TASK.STATE = agvReuqstList.State;
//                    wMS_AGV_TASK.BARCODE = agvReuqstList.Barcode;
//                    wMS_AGV_TASK.CONTAINER = agvReuqstList.Barcode;
//                    wMS_AGV_TASK.GROUPCODE = agvReuqstList.SourceOrder;
//                    wMS_AGV_TASK.ENDTAXI = agvReuqstList.EndTaxi;
//                    wMS_AGV_TASK.PRIORITY = agvReuqstList.Priority;
//                    a = WmsAgvTaskDao.insert(wMS_AGV_TASK);
//                    if (a > 0)
//                    {
//                        reslutJson = new ReslutJson()
//                        {
//                            success = true,
//                            msg = "接收成功"
//                        };
//                        LOGService.WriteOperateLogT(string.Format("单据:{0},原因:{1}", agvReuqstList.SourceOrder, reslutJson.msg), "MES下发AGV任务", ip);
//                    }
//                }
//                else
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "MES重复传单据"
//                    };
//                    LOGService.WriteOperateLogF(string.Format("单据:{0},原因:{1}", agvReuqstList.SourceOrder, reslutJson.msg), "MES下发AGV任务", ip);
//                }
//            }
//            catch (Exception ex)
//            {


//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };

//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }

//        public ReslutJson taskStartRequest(JObject json, string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                string SOURCEORDR = json["SourceOrder"].ObjToString();
                
//                try
//                {
//                    dbContext.Db.Ado.BeginTran();
//                    string sql = @"
//UPDATE WMS_BILL_PUTOUTHEADER SET APPOVEFLAG=0 WHERE SOURCEORDER=@SOURCEORDER AND APPOVEFLAG=-1
//";
//                    int a = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                        new SugarParameter("@SOURCEORDER",SOURCEORDR)
//                    });
//                    sql = @"
//  UPDATE WMS_AGV_TASK SET STATE=0 WHERE SOURCEORDER=@SOURCEORDER
//";
//                    int b = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                        new SugarParameter("@SOURCEORDER",SOURCEORDR)
//                    });
//                    dbContext.Db.Ado.CommitTran();
//                    if (a > 0 && b >=0)
//                    {
                        
//                        reslutJson=WMSService.taskStartRequest(SOURCEORDR, ip);
//                        LOGService.WriteOperateLogT(string.Format("单据:{0},原因:{1}", SOURCEORDR, reslutJson.msg), "MES下发任务执行", ip);
//                    }
//                    else {
//                        dbContext.Db.Ado.RollbackTran();
//                        reslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "WMS业务处理失败,或单据状态不对"
//                        };                     
//                        LOGService.WriteOperateLogF(string.Format("单据:{0},原因:{1}", SOURCEORDR, reslutJson.msg), "MES下发任务执行", ip);
//                    }
                  
//                }
//                catch (Exception)
//                {

//                    dbContext.Db.Ado.RollbackTran();
//                }

//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };
//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }

//        public ReslutJson getInventory(JObject json, string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                string ItemCode = json["ItemCode"].ObjToString();
//                string lot= json["Lot"].ObjToString();
//                string Sign = json["Sign"].ObjToString();
//                string sql = @"
//SELECT SUM(C.ONE) AS ONE,SUM(C.TWO) AS TWO,SUM(C.THREE) AS THREE ,SUM(C.FOUR) AS FOUR
//FROM (
//SELECT CASE WHEN COUNT(B.ID) = 1 THEN 1 ELSE 0 END AS ONE ,
//	   CASE WHEN COUNT(B.ID) = 2 THEN 1 ELSE 0 END AS TWO,
//	   CASE WHEN COUNT(B.ID) = 3 THEN 1 ELSE 0 END AS THREE,
//	   CASE WHEN COUNT(B.ID)=4 THEN 1 ELSE 0 END AS FOUR
//FROM WMS_BASE_LOCATION A
//JOIN WMS_BILL_INVENTORYBOOK B 
//ON A.ZONECODE=B.ZONECODE AND A.WAREHOUSECODE=B.WAREHOUSECODE AND A.CODE=B.LOCATIONCODE
//JOIN WMS_BASE_MASTER C ON B.ITEMCODE = C.ITEMCODE AND B.WAREHOUSECODE=C.OUTERMODE
//WHERE B.FREEZEFLAG=1 AND B.STOCKFLAG=1 AND A.QTY=1 AND B.REMARK=@Sign
//AND B.LOT=@LOT AND B.ITEMCODE=@ITEMCODE
//GROUP BY B.LOCATIONCODE
//) C
//";
//                var inventory = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                        new SugarParameter("@ITEMCODE",ItemCode),
//                        new SugarParameter("@LOT",lot),
//                         new SugarParameter("@Sign",Sign)
//                    });
//                if (inventory != null && inventory.Rows.Count > 0)
//                {
//                    int one = inventory.Rows[0]["ONE"].ObjToInt();
//                    int two = inventory.Rows[0]["TWO"].ObjToInt();
//                    int three = inventory.Rows[0]["THREE"].ObjToInt();
//                    int four = inventory.Rows[0]["FOUR"].ObjToInt();
//                    reslutJson = new ReslutJson()
//                    {
//                        success = true,
//                        data = new Dictionary<string, object>(){
//                            { "one", one },
//                            { "two", two },
//                            { "three", three },
//                            { "four", four }
//                        }
//                    };
//                }
//                else {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "WMS内部错误"
//                    };

//                }

//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson() {
//                    success=false,
//                    msg=ex.Message
//                };
//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }

//        public string Method07(string BarCode,string ip) {


//            try
//            {
//                string sql = @"
//SELECT TOP 1 ID,ITEMCODE,BARCODE,ITEMNAME FROM WMS_BILL_PUTIN WHERE BARCODE = @BARCODE AND QUESTFLAG=0
//ORDER BY CREATETIME DESC 
//";
//                var PutIn = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                new SugarParameter("@BARCODE",BarCode),
//            });
//                if (PutIn != null && PutIn.Rows.Count > 0)
//                {
//                    string ItemCode = PutIn.Rows[0]["ITEMCODE"].ObjToString();
//                    BarCode = PutIn.Rows[0]["BARCODE"].ObjToString();
//                    string ItemName = PutIn.Rows[0]["ITEMNAME"].ObjToString();
//                    string Id = PutIn.Rows[0]["ID"].ObjToString();
//                    JObject jObject = new JObject() {
//                            {"barcode",BarCode},
//                            {"mcode",ItemCode},
//                            {"mdesc",ItemName},
//                        };
//                    var reslut = HttpRequestHelper.HttpPostMethod(ApiConfig.method07, jObject);
//                    ReslutJson reslutJson = JsonConvert.DeserializeObject<ReslutJson>(JsonConvert.SerializeObject(reslut));
//                    LOGService.WriteOperateLogT(string.Format("返回信息:{0}", reslutJson.msg), "MES返回物料信息", ip);
//                    if (reslutJson.success == false)
//                    {
//                        var jarry = JArray.FromObject(reslutJson.data)[0];
//                        ItemCode = jarry["ItemCode"].ToString();
//                        ItemName = jarry["ItemName"].ToString();
//                        string state = jarry["State"].ToString();
//                        int Qty = jarry["Qty"].ObjToInt();
//                        sql = @"
//UPDATE WMS_BILL_PUTIN SET QTY=@QTY,ITEMCODE=@ITEMCODE,ITEMNAME=@ITEMNAME,REMARK=@REMATK WHERE ID=@ID AND QUESTFLAG=0
//";
//                        int a = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                            new SugarParameter("@BARCODE",BarCode),
//                            new SugarParameter("@QTY",Qty),
//                            new SugarParameter("@ITEMCODE",ItemCode),
//                            new SugarParameter("@ITEMNAME",ItemName),
//                            new SugarParameter("@REMATK",state),
//                            new SugarParameter("@ID",Id),
//                    });
//                        sql = @"
//UPDATE WMS_BILL_INVENTORYBOOK SET QTY=@QTY,ITEMCODE=@ITEMCODE,ITEMNAME=@ITEMNAME,REMARK=@REMATK WHERE BARCODE=@BARCODE 
//";
//                        int b = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                             new SugarParameter("@BARCODE",BarCode),
//                            new SugarParameter("@QTY",Qty),
//                            new SugarParameter("@ITEMCODE",ItemCode),
//                            new SugarParameter("@ITEMNAME",ItemName),
//                            new SugarParameter("@REMATK",state),
//                    });
//                        if (a > 0 && b > 0)
//                        {
//                            jObject = new JObject() {
//                            {"barcode",BarCode},
//                        };
//                            reslut = HttpRequestHelper.HttpPostMethod(ApiConfig.method08, jObject);
//                            LOGService.WriteOperateLogT(string.Format("物料信息:{0}", reslut.msg), "MES返回物料信息确认信息", ip);
//                        }
//                        else
//                        {
//                        }
//                    }                    
//                }
//            }

//            catch (Exception EX)
//            {
//                throw;
//            }
//            return BarCode;
//        }

//        public ReslutJson ChangeCase(JObject json, string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            string[] Iqty = json["Iqty"].ObjToString().Split(',');
//            string[] Oqty = json["Oqty"].ObjToString().Split(',');
//            try
//            {
//                string sql = "";
//                bool flag = true;
//                bool flag2 = true;
//                dbContext.Db.Ado.BeginTran();
//                foreach (var barcode in Iqty) {
//                    sql = @"
//SELECT COUNT(ID) FROM WMS_BILL_PUTIN WHERE BARCODE = @BARCODE AND QUESTFLAG = 0";
//                    int a = dbContext.Db.Ado.GetInt(sql, new List<SugarParameter>() {
//                        new SugarParameter("@BARCODE",barcode),
//                    });
//                    if (a==0) {
//                        flag = false;
//                    }
//                }
//                foreach (var barcode in Oqty)
//                {
//                    sql = @"
//SELECT COUNT(ID) FROM WMS_BILL_PUTIN WHERE BARCODE = @BARCODE AND QUESTFLAG = 0";
//                    int b = dbContext.Db.Ado.GetInt(sql, new List<SugarParameter>() {
//                        new SugarParameter("@BARCODE",barcode),
//                    });
//                    if (b > 0)
//                    {
//                        flag2 = false;
//                    }
//                }
//                if (flag && flag2)
//                {
//                    foreach (var barcode in Oqty)
//                    {
//                        sql = @"
//INSERT INTO WMS_BILL_PUTIN(ID,SOURCEORDER,WAREHOUSECODE,CONTAINER,ITEMCODE,ITEMNAME,BARCODE,QUESTFLAG,ENABLED,REMARK,CREATOR,CREATETIME,LASTEDITOR,LASTEDITTIME,QTY,LOT)
//SELECT TOP 1 NEWID(),SOURCEORDER,WAREHOUSECODE,@BARCODE,ITEMCODE,ITEMNAME,@BARCODE,0,1,REMARK,CREATOR,GETDATE(),LASTEDITOR,GETDATE(),QTY,LOT FROM WMS_BILL_PUTIN 
//WHERE BARCODE=@IQTY AND QUESTFLAG=0
//";
//                        var PutIn = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                        new SugarParameter("@BARCODE",barcode),
//                        new SugarParameter("@IQTY",Iqty[0]),
//                     });
//                        sql = @"
//INSERT INTO WMS_BILL_INVENTORYBOOK(ID,SOURCEORDER,SOURCEORDERTYPE,SYSBOLCODE,BARCODE,ITEMCODE,ITEMNAME,LOT,QTY,WAREHOUSECODE,ZONECODE,LOCATIONCODE,SUPPLIERID,SUPPLIERNAME
//,STOCKFLAG,FREEZEFLAG,FREEZETIME,FREEZEREASON,REMARK,CREATOR,CREATETIME,LASTEDITOR,LASTEDITTIME)
//SELECT TOP 1 NEWID(),SOURCEORDER,SOURCEORDERTYPE,@BARCODE,@BARCODE,ITEMCODE,ITEMNAME,LOT,QTY,WAREHOUSECODE,ZONECODE,LOCATIONCODE,SUPPLIERID,SUPPLIERNAME,
// STOCKFLAG,FREEZEFLAG,GETDATE(),FREEZEREASON,REMARK,CREATOR,GETDATE(),LASTEDITOR,GETDATE() FROM WMS_BILL_INVENTORYBOOK 
//WHERE BARCODE=@IQTY
//";
//                        var invertory = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                            new SugarParameter("@BARCODE",barcode),
//                            new SugarParameter("@IQTY",Iqty[0]),
//                     });
//                    }
//                    foreach (var barcode in Iqty)
//                    {
//                        sql = @"
//DELETE FROM WMS_BILL_PUTIN WHERE BARCODE=@BARCODE AND QUESTFLAG=0
//";
//                        dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                        new SugarParameter("@BARCODE",barcode),
//                    });
//                        sql = @"
//DELETE FROM WMS_BILL_INVENTORYBOOK WHERE BARCODE=@BARCODE
//";
//                        dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                        new SugarParameter("@BARCODE",barcode),
//                    });
//                    }
//                    dbContext.Db.Ado.CommitTran();
//                    reslutJson = new ReslutJson()
//                    {
//                        success = true,
//                        msg = "WMS换箱成功",
//                    };
//                }
//                else {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "换箱失败",
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                dbContext.Db.Ado.RollbackTran();
//                reslutJson = new ReslutJson() {
//                    success=false,
//                    msg=ex.Message
//                };
//            }
                  
//            return reslutJson;
//        }
    }
}