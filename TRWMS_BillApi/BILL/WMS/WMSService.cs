using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using TRXWMS.BILL.DAL;
using TRXWMS.BILL.DAL.WMS;
using TRXWMS.BILL.LOG;
using TRXWMS.BILL.WCS;
using TRXWMS.Models.Response;

namespace TRXWMS.BILL.WMS
{
    public class WMSService
    {
        public static DbContext dbContext = new DbContext();
        //public static SqlSugarClient db = SugarContext.GetInstance();

        public static WCSService wCSService = new WCSService();

//        public static ReslutJson taskStartRequest(string sourceOrder,string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                //查询出库单
//                string sql = @"
//SELECT TOP 1 A.SOURCEORDER,B.ITEMCODE,B.ITEMNAME,B.OUTLINE,B.LOT,A.SOURCEORDERTYPE,B.CTNQTY,B.SEQUENCE,A.REMARK
//FROM WMS_BILL_PUTOUTHEADER A JOIN WMS_BILL_PUTOUTBODY B ON A.ID=B.HEADERID
//WHERE A.APPOVEFLAG=0 AND A.SOURCEORDER=@SOURCEORDER
//ORDER BY A.CREATETIME ASC
//";
//                var PUTOUTHEADER = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                    new SugarParameter("@SOURCEORDER",sourceOrder)
//                });
//                if (PUTOUTHEADER != null && PUTOUTHEADER.Rows.Count > 0)
//                {
//                    string SourceOrder = PUTOUTHEADER.Rows[0]["SOURCEORDER"].ObjToString();
//                    string ItemCode = PUTOUTHEADER.Rows[0]["ITEMCODE"].ObjToString();
//                    string ItemName = PUTOUTHEADER.Rows[0]["ITEMNAME"].ObjToString();
//                    string OutLine = PUTOUTHEADER.Rows[0]["OUTLINE"].ObjToString();
//                    string Lot = PUTOUTHEADER.Rows[0]["LOT"].ObjToString();
//                    string SourceorderType = PUTOUTHEADER.Rows[0]["SOURCEORDERTYPE"].ObjToString();
//                    string Sequence = PUTOUTHEADER.Rows[0]["SEQUENCE"].ObjToString();
//                    int CtnQty = PUTOUTHEADER.Rows[0]["CTNQTY"].ObjToInt();
//                    string remark= PUTOUTHEADER.Rows[0]["REMARK"].ObjToString();
//                    if (SourceorderType.Equals("1"))
//                    {

//                        reslutJson = BillAjustType(ItemCode, Lot, sourceOrder, OutLine, ItemName, ip);
//                    }
//                    else {
//                        reslutJson = BillAjustType(ItemCode, Lot, sourceOrder, OutLine, ItemName, ip, Sequence,CtnQty, remark);
//                    }
                   
//                }
//                else {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "未收到出库信息",
//                    };
//                }
//            }
//            catch (Exception ex )
//            {
//                reslutJson = new ReslutJson() {
//                    success=false,
//                    msg=ex.Message,
//                };
//                LOGService.WriteExceptionLog(ex);               
//            }
//            return reslutJson;
//        }

//        public static ReslutJson BillAjustType(string ItemCode, string Lot,string sourceOrder,string OutLine,string ItemName,string ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                //查询物料属性
//                string sql = @"
//SELECT A.OUTERMODE,B.SIGINNUM,A.PRIORITYLEVEL FROM WMS_BASE_MASTER A JOIN WMS_BASE_WAREHOUSE B ON A.OUTERMODE =B.CODE 
//WHERE A.ITEMCODE = @ITEMCODE AND A.IFUSE=1
//";
//                var MASTET = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                    new SugarParameter("@ITEMCODE",ItemCode),
//                });
//                if (MASTET != null && MASTET.Rows.Count > 0)
//                {
//                    string ENTRANCE = MASTET.Rows[0]["OUTERMODE"].ObjToString();
//                    int SIGINNUM = MASTET.Rows[0]["SIGINNUM"].ObjToInt();
//                    int PRIORITYLEVEL = MASTET.Rows[0]["PRIORITYLEVEL"].ObjToInt();
//                    if (!string.IsNullOrEmpty(ENTRANCE) && SIGINNUM == 0)
//                    {
//                        if (string.IsNullOrEmpty(Lot))
//                        {
//                            //查询库位
//                            sql = @"
//SELECT TOP 1 A.WAREHOUSECODE,A.ZONECODE,B.CODE,A.SYSBOLCODE AS SYSBOLCODE  FROM WMS_BILL_INVENTORYBOOK A JOIN WMS_BASE_LOCATION B 
//ON A.WAREHOUSECODE=B.WAREHOUSECODE AND A.ZONECODE=B.ZONECODE AND A.LOCATIONCODE=B.CODE
//WHERE B.QTY=1 AND B.ISAVIALABLE=1 AND B.ENABLED=1 AND A.STOCKFLAG=1 AND A.FEEDBACKFLAG=1 AND
//A.FREEZEFLAG=1 AND A.REMARK='正常' AND A.ITEMCODE=@ITEMCODE
//ORDER BY CONVERT(VARCHAR(10),A.STOCKTIME,120) ASC, LAYERNUM ASC,COLUMNNUM DESC,PICKORDER DESC
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    //new SugarParameter("@LOT",Lot),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                string sysbolCode = LOCATION.Rows[0]["SYSBOLCODE"].ObjToString();
//                                reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                            }
//                            else
//                            {
//                                LOGService.WriteOperateLogF(string.Format("单据:{0},原因:{1}", sourceOrder, "WMS无可用库存"), "WMS下发任务", ip);
//                            }
//                        }
//                        else
//                        {
//                            sql = @"
//SELECT TOP 1 A.WAREHOUSECODE,A.ZONECODE,B.CODE,A.SYSBOLCODE AS SYSBOLCODE FROM WMS_BILL_INVENTORYBOOK A JOIN WMS_BASE_LOCATION B 
//ON A.WAREHOUSECODE=B.WAREHOUSECODE AND A.ZONECODE=B.ZONECODE AND A.LOCATIONCODE=B.CODE
//WHERE B.QTY=1 AND B.ISAVIALABLE=1 AND B.ENABLED=1 AND A.STOCKFLAG=1 AND A.FEEDBACKFLAG=1 AND
//A.FREEZEFLAG=1 AND A.REMARK='正常' AND A.LOT=@LOT AND A.ITEMCODE=@ITEMCODE
//ORDER BY CONVERT(VARCHAR(10),A.STOCKTIME,120) ASC, LAYERNUM ASC,COLUMNNUM DESC,PICKORDER DESC
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    new SugarParameter("@LOT",Lot),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                string sysbolCode = LOCATION.Rows[0]["SYSBOLCODE"].ObjToString();
//                                reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                            }
//                        }

//                    }
//                    else if (!string.IsNullOrEmpty(ENTRANCE) && SIGINNUM == 1)
//                    {
//                        if (!string.IsNullOrEmpty(Lot))
//                        {
//                            sql = @"
//SELECT TOP 1 TEMP2.WAREHOUSECODE,TEMP2.ZONECODE,TEMP2.CODE,TEMP2.ROWNUM,TEMP2.COLUMNNUM,TEMP2.LAYERNUM,TEMP2.SYMBOL
//FROM
//	(
//	SELECT DISTINCT A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM,MIN(B.LOT) AS LOT,MIN(CONVERT(VARCHAR(10),B.STOCKTIME,120)) AS STOCKTIME
//	FROM WMS_BASE_LOCATION A LEFT JOIN WMS_BILL_INVENTORYBOOK B
//	ON A.WAREHOUSECODE = B.WAREHOUSECODE AND A.CODE = B.LOCATIONCODE AND A.REMARK = B.ITEMCODE+B.LOT 
//	WHERE A.WAREHOUSECODE ='1103C' AND A.TYPE = 'ST' AND A.QTY  = 1 AND A.ISAVIALABLE = 1 AND A.ENABLED = 1 AND A.REMARK = @ITEMCODE+B.LOT
//	AND B.STOCKFLAG = 1 AND B.FREEZEFLAG = 1 AND B.LOT=@LOT AND B.FEEDBACKFLAG=1 
//	GROUP BY A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM
//	) AS TEMP1
//JOIN WMS_BASE_LOCATION TEMP2
//ON TEMP1.WAREHOUSECODE = TEMP2.WAREHOUSECODE AND TEMP1.GROUPCODE = TEMP2.GROUPCODE 
//AND TEMP1.LAYERNUM = TEMP2.LAYERNUM AND TEMP1.COLUMNNUM = TEMP2.COLUMNNUM
//WHERE TEMP2.WAREHOUSECODE ='1103C' AND TEMP2.TYPE = 'ST' AND TEMP2.QTY  = 1 AND TEMP2.REMARK = @ITEMCODE + TEMP1.LOT
//AND TEMP2.ISAVIALABLE = 1 AND TEMP2.ENABLED = 1
//ORDER BY TEMP1.LOT,TEMP1.STOCKTIME,TEMP2.WAREHOUSECODE,TEMP2.LAYERNUM,TEMP2.GROUPCODE,TEMP2.COLUMNNUM,TEMP2.PICKORDER DESC;
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    new SugarParameter("@LOT",Lot),
//                                    new SugarParameter("@WAREHOUSECODE",ENTRANCE),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                string sysbolCode = LOCATION.Rows[0]["SYSBOLCODE"].ObjToString();
//                                reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                            }
//                            else
//                            {
//                                sql = @"
//SELECT TOP 1 TEMP2.WAREHOUSECODE,TEMP2.ZONECODE,TEMP2.CODE,TEMP2.ROWNUM,TEMP2.COLUMNNUM,TEMP2.LAYERNUM,TEMP2.SYMBOL
//FROM
//	(
//	SELECT DISTINCT A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM,MIN(B.LOT) AS LOT,MIN(CONVERT(VARCHAR(10),B.STOCKTIME,120)) AS STOCKTIME
//	FROM WMS_BASE_LOCATION A LEFT JOIN WMS_BILL_INVENTORYBOOK B
//	ON A.WAREHOUSECODE = B.WAREHOUSECODE AND A.CODE = B.LOCATIONCODE AND A.REMARK = B.ITEMCODE+B.LOT
//	WHERE A.WAREHOUSECODE ='1103C' AND A.TYPE = 'ST' AND A.QTY  = 1 AND A.ISAVIALABLE = 1 AND A.ENABLED = 1 AND A.REMARK = @ITEMCODE+B.LOT
//	AND B.STOCKFLAG = 1 AND B.FREEZEFLAG = 1 AND B.FEEDBACKFLAG=1
//	GROUP BY A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM
//	) AS TEMP1
//JOIN WMS_BASE_LOCATION TEMP2
//ON TEMP1.WAREHOUSECODE = TEMP2.WAREHOUSECODE AND TEMP1.GROUPCODE = TEMP2.GROUPCODE 
//AND TEMP1.LAYERNUM = TEMP2.LAYERNUM AND TEMP1.COLUMNNUM = TEMP2.COLUMNNUM
//WHERE TEMP2.WAREHOUSECODE ='1103C' AND TEMP2.TYPE = 'ST' AND TEMP2.QTY  = 1 AND TEMP2.REMARK = @ITEMCODE + TEMP1.LOT
//AND TEMP2.ISAVIALABLE = 1 AND TEMP2.ENABLED = 1  
//ORDER BY TEMP1.LOT,TEMP1.STOCKTIME,TEMP2.WAREHOUSECODE,TEMP2.LAYERNUM,TEMP2.GROUPCODE,TEMP2.COLUMNNUM,TEMP2.PICKORDER DESC;
//";
//                                LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    //new SugarParameter("@LOT",Lot),
//                                     new SugarParameter("@WAREHOUSECODE",ENTRANCE),
//                                });
//                                if (LOCATION != null && LOCATION.Rows.Count > 0)
//                                {
//                                    string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                    string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                    string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                    string sysbolCode = LOCATION.Rows[0]["SYMBOL"].ObjToString();
//                                    reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            sql = @"
//SELECT TOP 1 TEMP2.WAREHOUSECODE,TEMP2.ZONECODE,TEMP2.CODE,TEMP2.ROWNUM,TEMP2.COLUMNNUM,TEMP2.LAYERNUM,TEMP2.SYMBOL
//FROM
//	(
//	SELECT DISTINCT A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM,MIN(B.LOT) AS LOT,MIN(CONVERT(VARCHAR(10),B.STOCKTIME,120)) AS STOCKTIME
//	FROM WMS_BASE_LOCATION A LEFT JOIN WMS_BILL_INVENTORYBOOK B
//	ON A.WAREHOUSECODE = B.WAREHOUSECODE AND A.CODE = B.LOCATIONCODE AND A.REMARK = B.ITEMCODE+B.LOT
//	WHERE A.WAREHOUSECODE ='1103C' AND A.TYPE = 'ST' AND A.QTY  = 1 AND A.ISAVIALABLE = 1 AND A.ENABLED = 1 AND A.REMARK = @ITEMCODE+B.LOT
//	AND B.STOCKFLAG = 1 AND B.FREEZEFLAG = 1 AND B.FEEDBACKFLAG=1 
//	GROUP BY A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM
//	) AS TEMP1
//JOIN WMS_BASE_LOCATION TEMP2
//ON TEMP1.WAREHOUSECODE = TEMP2.WAREHOUSECODE AND TEMP1.GROUPCODE = TEMP2.GROUPCODE 
//AND TEMP1.LAYERNUM = TEMP2.LAYERNUM AND TEMP1.COLUMNNUM = TEMP2.COLUMNNUM
//WHERE TEMP2.WAREHOUSECODE ='1103C' AND TEMP2.TYPE = 'ST' AND TEMP2.QTY  = 1 AND TEMP2.REMARK = @ITEMCODE + TEMP1.LOT
//AND TEMP2.ISAVIALABLE = 1 AND TEMP2.ENABLED = 1 
//ORDER BY TEMP1.LOT,TEMP1.STOCKTIME,TEMP2.WAREHOUSECODE,TEMP2.LAYERNUM,TEMP2.GROUPCODE,TEMP2.COLUMNNUM,TEMP2.PICKORDER DESC;
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    //new SugarParameter("@LOT",Lot),
//                                     new SugarParameter("@WAREHOUSECODE",ENTRANCE),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                string sysbolCode = LOCATION.Rows[0]["SYMBOL"].ObjToString();
//                                reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                            }

//                        }
//                    }
//                }
//                else
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "未配置主表信息",
//                    };
//                }
//            }
//            catch (Exception)
//            {

//                throw;
//            }
//            return reslutJson;

//        }

//        public static ReslutJson BillAjustType(string ItemCode, string Lot, string sourceOrder, string OutLine, string ItemName, string ip,string Sequence,int CtnQty,string remark)
//        {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                //查询物料属性
//                string sql = @"
//SELECT A.OUTERMODE,B.SIGINNUM,A.PRIORITYLEVEL FROM WMS_BASE_MASTER A JOIN WMS_BASE_WAREHOUSE B ON A.OUTERMODE =B.CODE 
//WHERE A.ITEMCODE = @ITEMCODE AND A.IFUSE=1
//";
//                var MASTET = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                    new SugarParameter("@ITEMCODE",ItemCode),
//                });
//                if (MASTET != null && MASTET.Rows.Count > 0)
//                {
//                    string ENTRANCE = MASTET.Rows[0]["OUTERMODE"].ObjToString();
//                    int SIGINNUM = MASTET.Rows[0]["SIGINNUM"].ObjToInt();
//                    int PRIORITYLEVEL = MASTET.Rows[0]["PRIORITYLEVEL"].ObjToInt();
//                    if (ENTRANCE.Equals("1103B")) {
//                        int four = Convert.ToInt32(Sequence.Substring(0,4));
//                        int three = Convert.ToInt32(Sequence.Substring(4, 4));
//                        int two = Convert.ToInt32(Sequence.Substring(8, 4));
//                        int one = Convert.ToInt32(Sequence.Substring(12, 4));
//                        if (four > 0) {
//                            reslutJson=OutTask(ItemCode, remark,Lot, four, 4, sourceOrder, OutLine, ItemName, PRIORITYLEVEL,ip);
//                        }
//                        if (three > 0) {
//                            reslutJson=OutTask(ItemCode, remark, Lot, three, 3, sourceOrder, OutLine, ItemName, PRIORITYLEVEL, ip);
//                        }
//                        if (two > 0) {
//                            reslutJson=OutTask(ItemCode, remark, Lot, two, 2, sourceOrder, OutLine, ItemName, PRIORITYLEVEL, ip);

//                        }
//                        if (one > 0) {
//                            reslutJson=OutTask(ItemCode, remark, Lot, one, 1, sourceOrder, OutLine, ItemName, PRIORITYLEVEL, ip);
//                        }
                        
//                    } else if (ENTRANCE.Equals("1103C")) {
//                        int total = CtnQty;
//                        while (total>0) {
//                            sql = @"
//SELECT TOP 1 TEMP2.WAREHOUSECODE,TEMP2.ZONECODE,TEMP2.CODE,TEMP2.ROWNUM,TEMP2.COLUMNNUM,TEMP2.LAYERNUM,TEMP2.SYMBOL
//FROM
//	(
//	SELECT DISTINCT A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM,MIN(B.LOT) AS LOT,MIN(CONVERT(VARCHAR(10),B.STOCKTIME,120)) AS STOCKTIME
//	FROM WMS_BASE_LOCATION A LEFT JOIN WMS_BILL_INVENTORYBOOK B
//	ON A.WAREHOUSECODE = B.WAREHOUSECODE AND A.CODE = B.LOCATIONCODE AND A.REMARK = B.ITEMCODE+B.LOT
//	WHERE A.WAREHOUSECODE ='1103C' AND A.TYPE = 'ST' AND A.QTY  = 1 AND A.ISAVIALABLE = 1 AND A.ENABLED = 1 AND A.REMARK = @ITEMCODE+B.LOT
//	AND B.STOCKFLAG = 1 AND B.FREEZEFLAG = 1 AND B.LOT=@LOT AND B.FEEDBACKFLAG=1
//	GROUP BY A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM
//	) AS TEMP1
//JOIN WMS_BASE_LOCATION TEMP2
//ON TEMP1.WAREHOUSECODE = TEMP2.WAREHOUSECODE AND TEMP1.GROUPCODE = TEMP2.GROUPCODE 
//AND TEMP1.LAYERNUM = TEMP2.LAYERNUM AND TEMP1.COLUMNNUM = TEMP2.COLUMNNUM
//WHERE TEMP2.WAREHOUSECODE ='1103C' AND TEMP2.TYPE = 'ST' AND TEMP2.QTY  = 1 AND TEMP2.REMARK = @ITEMCODE + TEMP1.LOT
//AND TEMP2.ISAVIALABLE = 1 AND TEMP2.ENABLED = 1
//ORDER BY TEMP1.LOT,TEMP1.STOCKTIME,TEMP2.WAREHOUSECODE,TEMP2.LAYERNUM,TEMP2.GROUPCODE,TEMP2.COLUMNNUM,TEMP2.PICKORDER DESC;
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                    new SugarParameter("@LOT",Lot),
//                                    new SugarParameter("@WAREHOUSECODE",ENTRANCE),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                string warehouseCode = LOCATION.Rows[0]["WAREHOUSECODE"].ObjToString();
//                                string zoneCode = LOCATION.Rows[0]["ZONECODE"].ObjToString();
//                                string code = LOCATION.Rows[0]["CODE"].ObjToString();
//                                string sysbolCode = LOCATION.Rows[0]["SYSBOLCODE"].ObjToString();
//                                sql = @"
//SELECT COUNT(ID) FROM WMS_BILL_INVENTORYBOOK WHERE LOCATIONCODE=@LOCATIONCODE AND WAREHOUSECODE=@WAREHOUSECODE
//";
//                                int a = dbContext.Db.Ado.ExecuteCommand(sql,new List<SugarParameter>() { });
//                                total = total - a;
//                                reslutJson = taskStartWarehouse(warehouseCode, Lot, sourceOrder, ItemCode, OutLine, ItemName, code, PRIORITYLEVEL, sysbolCode, zoneCode, ip);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success=false,
//                    msg=ex.Message
//                };
//            }
//            return reslutJson;
//        }

//        public static ReslutJson OutTask(string ItemCode, string Remark,string Lot,int Four,int Type,string SourceOrder,string OutLine,string ItemName,int ProrityLevel,string Ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                string sql = @"
//SELECT TOP (@QTY) TEMP.LOCATIONCODE,C.WAREHOUSECODE,C.ZONECODE,C.SYMBOL FROM 
//(
//SELECT A.LOT, A.LOCATIONCODE FROM 
//WMS_BILL_INVENTORYBOOK A JOIN WMS_BASE_LOCATION B ON A.ZONECODE=B.ZONECODE AND A.WAREHOUSECODE=B.WAREHOUSECODE
//AND A.LOCATIONCODE=B.CODE
//WHERE A.ITEMCODE =@ITEMCODE AND A.LOT=@LOT AND A.STOCKFLAG=1 AND A.FREEZEFLAG=1 AND B.QTY=1 AND B.ENABLED=1 AND B.ISAVIALABLE=1 
//AND A.REMARK=@REMARK AND A.FEEDBACKFLAG=1
//GROUP BY A.LOT,A.LOCATIONCODE
//HAVING COUNT(B.ID)=@TYPE
//) AS TEMP JOIN WMS_BASE_LOCATION AS C ON TEMP.LOCATIONCODE=C.CODE AND C.QTY=1 AND C.ISAVIALABLE=1 AND C.ENABLED=1
//ORDER BY TEMP.LOT ASC,LAYERNUM ASC,COLUMNNUM ASC,PICKORDER DESC
//";
//                var fourTask = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                            new SugarParameter("@ITEMCODE",ItemCode),
//                            new SugarParameter("@REMARK",Remark),
//                            new SugarParameter("@LOT",Lot),
//                            new SugarParameter("@QTY",Four),
//                            new SugarParameter("@TYPE",Type),
//                        });
//                if (fourTask != null && fourTask.Rows.Count > 0)
//                {
//                    for (int i = 0; i < fourTask.Rows.Count; i++)
//                    {
//                        string locationCode = fourTask.Rows[i]["LOCATIONCODE"].ObjToString();
//                        string warehouseCode = fourTask.Rows[i]["WAREHOUSECODE"].ObjToString();
//                        string zoneCode = fourTask.Rows[i]["ZONECODE"].ObjToString();
//                        string sysbolCode = fourTask.Rows[i]["SYMBOL"].ObjToString();
//                        reslutJson = taskStartWarehouse(warehouseCode, Lot, SourceOrder, ItemCode, OutLine, ItemName, locationCode, ProrityLevel, sysbolCode, zoneCode, Ip);
//                    }
//                    reslutJson = new ReslutJson()
//                    {
//                        success = true,
//                        msg = "WMS开始任务"
//                    };
//                }

//            }
//            catch (Exception EX)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success=false,
//                    msg=EX.Message
//                };
//            }
//            return reslutJson;
//        }

//        public static ReslutJson taskStartWarehouse(string WarehouseCode, string Lot, string SourceOrder,string ItemCode, string OutLine, 
//            string ItemName, string Code, int ProrityLevel, string SysbolCode, string ZoneCode ,string Ip) {
//            ReslutJson reslutJson = new ReslutJson();
//            dbContext.Db.Ado.BeginTran();
//            try
//            {
//                int a = WmsStockerTaskDao.insert(new WMS_STOCKER_TASK()
//                {
//                    ID = Guid.NewGuid().ToString("D"),
//                    SOURCEORDER = SourceOrder,
//                    WAREHOUSECODE = WarehouseCode,
//                    ITEMCODE = ItemCode,
//                    ITEMNAME = ItemName,
//                    STARTTAXI = Code,
//                    ENDTAXI = OutLine,
//                    TYPE = 2,
//                    PRIORITY = ProrityLevel,
//                    STATE = -1,
//                    CONTAINER = SysbolCode,
//                    CODE = DateTime.Now.ToString("yyyyMMddhh")

//                });
//                int b = WmsBillPutoutDetailDao.insert(new WMS_BILL_PUTOUTDETAIL()
//                {
//                    ID = Guid.NewGuid().ToString("D"),
//                    SOURCEORDER = SourceOrder,
//                    ITEMCODE = ItemCode,
//                    ITEMNAME = ItemName,
//                    LOT = Lot,
//                    CTNQTY = 1,
//                    PRIORITY = ProrityLevel,
//                    WAREHOUSECODE = WarehouseCode,
//                    ZONECODE = ZoneCode,
//                    LOCATIONCODE = Code,
//                    OUTLINE = OutLine,
//                    PLANT = OutLine,
//                    AGVLINE = OutLine,
//                    WORKCELL = OutLine,
//                    STATE = 0
//                });
//                string sql = @"
//UPDATE WMS_BILL_INVENTORYBOOK SET FREEZEFLAG=0,FREEZEREASON='待出库' WHERE WAREHOUSECODE=@WAREHOUSCODE AND ZONECODE=@ZONECODE AND LOCATIONCODE = @CODE
//";
//                int c = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                new SugarParameter("@WAREHOUSCODE",WarehouseCode),
//                new SugarParameter("@ZONECODE",ZoneCode),
//                new SugarParameter("@CODE",Code),
//            });
//                sql = @"
//UPDATE WMS_BILL_PUTOUTHEADER SET APPOVEFLAG=1,APPOVETIME=GETDATE(),APPOVEEXECUTOR='WMS' WHERE SOURCEORDER=@SOURCEORDER
//";
//                int d = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                new SugarParameter("@SOURCEORDER",SourceOrder),
//            });

//                dbContext.Db.Ado.CommitTran();
//                if (a > 0 && b > 0 && c > 0 && d > 0)
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = true,
//                        msg = "任务生成成功"
//                    };
//                    wCSService.A01_NewTask(WarehouseCode, Ip);
//                    LOGService.WriteOperateLogT(string.Format("单据:{0}", SourceOrder), "WMS生成堆垛机任务", Ip);
//                }
//                else
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "WMS内部错误"
//                    };
//                    dbContext.Db.Ado.RollbackTran();
//                }
//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg =ex.Message
//                };
//                LOGService.WriteExceptionLog(ex);
//                dbContext.Db.Ado.RollbackTran();
//            }
//            return reslutJson;
//        }

//        public void DeleteLog() {
//            string sql=@"DELETE FROM SYS_LOG WHERE CREATETIME < DATEADD(DAY,-2,GETDATE())";
//            dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                });
//        }

//        public static ReslutJson GetCountDetail(string sourceOrder,string ip)
//        {
//            ReslutJson reslutJson = new ReslutJson();
//            try
//            {
//                //查询盘点单
//                string sql = @"
//SELECT TOP 1 A.SOURCEORDER,B.ITEMCODE,B.ITEMNAME
//FROM WMS_BILL_COUNTHEADER A JOIN WMS_BILL_COUNTBODY B ON A.ID=B.HEADERID
//WHERE A.APPROVEFLAG =0 AND A.SOURCEORDER=@SOURCEORDER
//ORDER BY A.CREATETIME ASC
//";
//                var PUTOUTHEADER = dbContext.Db.Ado.GetDataTable(sql, new List<SugarParameter>() {
//                    new SugarParameter("@SOURCEORDER",sourceOrder)
//                });
//                if (PUTOUTHEADER != null && PUTOUTHEADER.Rows.Count > 0)
//                {
//                    string SourceOrder = PUTOUTHEADER.Rows[0]["SOURCEORDER"].ObjToString();
//                    string ItemCode = PUTOUTHEADER.Rows[0]["ITEMCODE"].ObjToString();
//                    string ItemName = PUTOUTHEADER.Rows[0]["ITEMNAME"].ObjToString();
//                    //查询物料属性
//                    string sql2 = @"
//SELECT A.OUTERMODE,B.SIGINNUM,A.PRIORITYLEVEL FROM WMS_BASE_MASTER A JOIN WMS_BASE_WAREHOUSE B ON A.OUTERMODE =B.CODE 
//WHERE A.ITEMCODE = @ITEMCODE AND A.IFUSE=1
//";
//                    var MASTET = dbContext.Db.Ado.GetDataTable(sql2, new List<SugarParameter>() {
//                        new SugarParameter("@ITEMCODE",ItemCode)                   
//                    });
//                    if(MASTET != null && MASTET.Rows.Count > 0)
//                    {
//                        string ENTRANCE = MASTET.Rows[0]["OUTERMODE"].ObjToString();
//                        int SIGINNUM = MASTET.Rows[0]["SIGINNUM"].ObjToInt();
//                        int PRIORITYLEVEL = MASTET.Rows[0]["PRIORITYLEVEL"].ObjToInt();
//                        if (!string.IsNullOrEmpty(ENTRANCE) && SIGINNUM == 0)
//                        {
//                            //查询所有的库位
//                           string sql3 = @"
//SELECT A.WAREHOUSECODE,A.ZONECODE,B.CODE,A.LOT, A.PACKQTY
//FROM (SELECT A.WAREHOUSECODE,A.ZONECODE,A.LOCATIONCODE,CONVERT(VARCHAR(10),A.STOCKTIME,120) AS STOCKTIME, A.LOT,count(ID) as PACKQTY
//FROM WMS_BILL_INVENTORYBOOK A 
//WHERE A.STOCKFLAG=1 AND A.FEEDBACKFLAG=1 AND
//A.FREEZEFLAG=1 AND A.REMARK='正常' AND A.ITEMCODE=@ITEMCODE
//GROUP BY CONVERT(VARCHAR(10),A.STOCKTIME,120),A.WAREHOUSECODE,A.ZONECODE,A.LOCATIONCODE,A.LOT) A JOIN WMS_BASE_LOCATION B 
//ON A.WAREHOUSECODE=B.WAREHOUSECODE AND A.ZONECODE=B.ZONECODE AND A.LOCATIONCODE=B.CODE
//WHERE B.QTY=1 AND B.ISAVIALABLE=1 AND B.ENABLED=1
//ORDER BY A.STOCKTIME ASC, LAYERNUM ASC,COLUMNNUM DESC,PICKORDER DESC
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql3, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                dbContext.Db.Ado.BeginTran();
//                                int b = 0;
//                                int c = 0;
//                                string sysbolcode = "";
//                                string warehouseCode = "";
//                                string zoneCode = "";
//                                string code = "";
//                                int packqty = 0;
//                                for (int i=0; i< LOCATION.Rows.Count; i++)
//                                {
//                                    warehouseCode = LOCATION.Rows[i]["WAREHOUSECODE"].ObjToString();
//                                    zoneCode = LOCATION.Rows[i]["ZONECODE"].ObjToString();
//                                    code = LOCATION.Rows[i]["CODE"].ObjToString();
//                                    string Lot = LOCATION.Rows[i]["LOT"].ObjToString();
//                                    packqty = Convert.ToInt32(LOCATION.Rows[i]["PACKQTY"].ObjToString());
//                                    b = WmsBillCountDetailDao.insert(new WMS_BILL_COUNTDETAIL()
//                                    {
//                                        ID = Guid.NewGuid().ToString("D"),
//                                        SOURCEORDER = SourceOrder,
//                                        WAREHOUSECODE = warehouseCode,
//                                        ZONECODE = zoneCode,
//                                        LOCATIONCODE = code,
//                                        BARCODE = sysbolcode,
//                                        ITEMCODE = ItemCode,
//                                        ITEMNAME = ItemName,
//                                        LOT = Lot,
//                                        LOCALQTY = packqty,
//                                        CHECKQTY = 0,
//                                        DIFFQTY = 0,
//                                        CHECKFLAG = 0
//                                    });
//                                    string sql5 = @"
//UPDATE WMS_BILL_INVENTORYBOOK SET FREEZEFLAG=0,FREEZEREASON='盘点' WHERE WAREHOUSECODE=@WAREHOUSCODE AND ZONECODE=@ZONECODE AND LOCATIONCODE = @CODE
//";
//                                     c = dbContext.Db.Ado.ExecuteCommand(sql5, new List<SugarParameter>() {
//                                        new SugarParameter("@WAREHOUSCODE",warehouseCode),
//                                        new SugarParameter("@ZONECODE",zoneCode),
//                                        new SugarParameter("@CODE",code),
//                                    });
//                                    dbContext.Db.Ado.CommitTran();
//                                }
//                                if(b > 0 && c > 0)
//                                {
//                                    string sql6 = @"UPDATE WMS_BILL_COUNTHEADER SET APPROVEFLAG = 1, APPROVETIME = GETDATE() WHERE SOURCEORDER = @SOURCEORDER";
//                                    dbContext.Db.Ado.ExecuteCommand(sql6, new List<SugarParameter>() {
//                                        new SugarParameter("@SOURCEORDER",SourceOrder),
//                                    });
//                                    reslutJson = new ReslutJson()
//                                    {
//                                        success = true,
//                                        msg = "任务生成成功"
//                                    };
//                                }
//                                else
//                                {
//                                    reslutJson = new ReslutJson()
//                                    {
//                                        success = false,
//                                        msg = "WMS内部错误"
//                                    };
//                                    dbContext.Db.Ado.RollbackTran();
//                                }
                

//                            }
//                            else
//                            {
//                                LOGService.WriteOperateLogF(string.Format("单据:{0},原因:{1}", sourceOrder, "WMS无可用库存"), "WMS下发任务", ip);
//                            }
//                        }
//                        else if (!string.IsNullOrEmpty(ENTRANCE) && SIGINNUM == 1)
//                        {
//                            //查询所有的库位
//                            string sql3 = @"
//SELECT TEMP2.WAREHOUSECODE,TEMP2.ZONECODE,TEMP2.CODE,TEMP2.SYMBOL,TEMP1.LOT, TEMP1.PAKCQTY
//FROM
//	(
//	SELECT DISTINCT A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM,MIN(B.LOT) AS LOT,MIN(CONVERT(VARCHAR(10),B.STOCKTIME,120)) AS STOCKTIME, Count(b.ID) AS PAKCQTY
//	FROM WMS_BASE_LOCATION A LEFT JOIN WMS_BILL_INVENTORYBOOK B
//	ON A.WAREHOUSECODE = B.WAREHOUSECODE AND A.CODE = B.LOCATIONCODE AND A.REMARK = B.ITEMCODE+B.LOT
//	WHERE A.WAREHOUSECODE ='1103C' AND A.TYPE = 'ST' AND A.QTY  = 1 AND A.ISAVIALABLE = 1 AND A.ENABLED = 1 AND A.REMARK = @ITEMCODE+B.LOT
//	AND B.STOCKFLAG = 1 AND B.FREEZEFLAG = 1 AND B.FEEDBACKFLAG=1 
//	GROUP BY A.WAREHOUSECODE,A.GROUPCODE,A.LAYERNUM,A.COLUMNNUM
//	) AS TEMP1
//JOIN WMS_BASE_LOCATION TEMP2
//ON TEMP1.WAREHOUSECODE = TEMP2.WAREHOUSECODE AND TEMP1.GROUPCODE = TEMP2.GROUPCODE 
//AND TEMP1.LAYERNUM = TEMP2.LAYERNUM AND TEMP1.COLUMNNUM = TEMP2.COLUMNNUM
//WHERE TEMP2.WAREHOUSECODE ='1103C' AND TEMP2.TYPE = 'ST' AND TEMP2.QTY  = 1 AND TEMP2.REMARK = @ITEMCODE + TEMP1.LOT
//AND TEMP2.ISAVIALABLE = 1 AND TEMP2.ENABLED = 1 
//ORDER BY TEMP1.LOT,TEMP1.STOCKTIME,TEMP2.WAREHOUSECODE,TEMP2.LAYERNUM,TEMP2.GROUPCODE,TEMP2.COLUMNNUM,TEMP2.PICKORDER DESC;
//";
//                            var LOCATION = dbContext.Db.Ado.GetDataTable(sql3, new List<SugarParameter>() {
//                                    new SugarParameter("@ITEMCODE",ItemCode),
//                                });
//                            if (LOCATION != null && LOCATION.Rows.Count > 0)
//                            {
//                                dbContext.Db.Ado.BeginTran();
//                                int b = 0;
//                                int c = 0;
//                                string sysbolcode = "";
//                                string warehouseCode = "";
//                                string zoneCode = "";
//                                string code = "";
//                                int packqty = 0;
//                                for (int i = 0; i < LOCATION.Rows.Count; i++)
//                                {
//                                    warehouseCode = LOCATION.Rows[i]["WAREHOUSECODE"].ObjToString();
//                                    zoneCode = LOCATION.Rows[i]["ZONECODE"].ObjToString();
//                                    code = LOCATION.Rows[i]["CODE"].ObjToString();
//                                    string Lot = LOCATION.Rows[i]["LOT"].ObjToString();
//                                    packqty = Convert.ToInt32(LOCATION.Rows[i]["PACKQTY"].ObjToString());
//                                    b =  WmsBillCountDetailDao.insert(new WMS_BILL_COUNTDETAIL()
//                                    {
//                                        ID = Guid.NewGuid().ToString("D"),
//                                        SOURCEORDER = SourceOrder,
//                                        WAREHOUSECODE = warehouseCode,
//                                        ZONECODE = zoneCode,
//                                        LOCATIONCODE = code,
//                                        BARCODE = sysbolcode,
//                                        ITEMCODE = ItemCode,
//                                        ITEMNAME = ItemName,
//                                        LOT = Lot,
//                                        LOCALQTY = packqty,
//                                        CHECKQTY = 0,
//                                        DIFFQTY = 0,
//                                        CHECKFLAG = 0
//                                    });
//                                    string sql5 = @"
//UPDATE WMS_BILL_INVENTORYBOOK SET FREEZEFLAG=0,FREEZEREASON='盘点' WHERE WAREHOUSECODE=@WAREHOUSCODE AND ZONECODE=@ZONECODE AND LOCATIONCODE = @CODE
//";
//                                    c = dbContext.Db.Ado.ExecuteCommand(sql5, new List<SugarParameter>() {
//                                        new SugarParameter("@WAREHOUSCODE",warehouseCode),
//                                        new SugarParameter("@ZONECODE",zoneCode),
//                                        new SugarParameter("@CODE",code),
//                                    });
//                                    dbContext.Db.Ado.CommitTran();
//                                }
//                                if (b > 0 && c >0)
//                                {
//                                    string sql6 = @"UPDATE WMS_BILL_COUNTHEADER SET APPROVEFLAG = 1, APPROVETIME = GETDATE() WHERE SOURCEORDER = @SOURCEORDER";
//                                    dbContext.Db.Ado.ExecuteCommand(sql6, new List<SugarParameter>() {
//                                        new SugarParameter("@SOURCEORDER",SourceOrder),
//                                    });
//                                    reslutJson = new ReslutJson()
//                                    {
//                                        success = true,
//                                        msg = "任务生成成功"
//                                    };
//                                }
//                                else
//                                {
//                                    reslutJson = new ReslutJson()
//                                    {
//                                        success = false,
//                                        msg = "WMS内部错误"
//                                    };
//                                    dbContext.Db.Ado.RollbackTran();
//                                }

//                            }
//                            else
//                            {
//                                LOGService.WriteOperateLogF(string.Format("单据:{0},原因:{1}", sourceOrder, "WMS无可用库存"), "WMS下发任务", ip);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        reslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "未配置主表信息",
//                        };
//                    }
//                }
//                else
//                {
//                    reslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "盘点信息不全",
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                reslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message,
//                };
//                LOGService.WriteExceptionLog(ex);
//            }
//            return reslutJson;
//        }
//        public static ReslutJson CountTask(string id,string Sourceorder, string Location, string ItemCode, string Lot)
//        {
//            ReslutJson ReslutJson = new ReslutJson();
//            dbContext.Db.Ado.BeginTran();
//            try
//            {
//                //查询盘点单
//                StringBuilder sql = new StringBuilder();
//                sql.Append(@"SELECT ID,SOURCEORDER,WAREHOUSECODE,ITEMCODE,ITEMNAME,LOCATIONCODE,BARCODE FROM WMS_BILL_COUNTDETAIL WHERE CHECKFLAG = 0");
//                var PUTOUTHEADER = new System.Data.DataTable();
//                if (id != null && id != "")
//                {
//                    sql.Append(@"AND ID IN (@ID)");
//                    PUTOUTHEADER = dbContext.Db.Ado.GetDataTable(sql.ToString(), new List<SugarParameter>() {
//                    new SugarParameter("@ID",id)
//                    });
//                }
//                else
//                {
//                    if (Sourceorder != null && Sourceorder != "")
//                    {
//                        sql.Append(@"AND SOURCEORDER = @SOURCEORDER");
//                    }
//                    if (Location != null && Location != "")
//                    {
//                        sql.Append(@"AND LOCATIONCODE = @LOCATIONCODE");                    
//                    }
//                    if(ItemCode != null && ItemCode != "")
//                    {
//                        sql.Append(@"AND ITEMCODE = @ITEMCODE");
//                    }
//                    if(Lot != null && Lot != "")
//                    {
//                        sql.Append(@"AND LOT = @LOT");
//                    }
//                    PUTOUTHEADER = dbContext.Db.Ado.GetDataTable(sql.ToString(), new List<SugarParameter>() {
//                        new SugarParameter("@SOURCEORDER",Sourceorder),
//                        new SugarParameter("@LOCATIONCODE",Location),
//                        new SugarParameter("@ITEMCODE",ItemCode),
//                        new SugarParameter("@LOT",Lot)
//                    });
//                }
//                if (PUTOUTHEADER != null && PUTOUTHEADER.Rows.Count > 0)
//                {
//                    int a = 0;
//                    int b = 0;
//                    dbContext.Db.Ado.BeginTran();
//                    for (int i = 0; i < PUTOUTHEADER.Rows.Count; i++)
//                    {
//                        string SOURCEORDER = PUTOUTHEADER.Rows[i]["SOURCEORDER"].ObjToString();
//                        string WAREHOUSECODE = PUTOUTHEADER.Rows[i]["WAREHOUSECODE"].ObjToString();
//                        string ITEMCODE = PUTOUTHEADER.Rows[i]["ITEMCODE"].ObjToString();
//                        string ITEMNAME = PUTOUTHEADER.Rows[i]["ITEMNAME"].ObjToString();
//                        string LOCATIONCODE = PUTOUTHEADER.Rows[i]["LOCATIONCODE"].ObjToString();
//                        string BARCODE = PUTOUTHEADER.Rows[i]["BARCODE"].ObjToString();
//                        string ID = PUTOUTHEADER.Rows[i]["ID"].ObjToString();
//                        a = a + WmsStockerTaskDao.insert(new WMS_STOCKER_TASK()
//                        {
//                            ID = Guid.NewGuid().ToString("D"),
//                            SOURCEORDER = SOURCEORDER,
//                            WAREHOUSECODE = WAREHOUSECODE,
//                            ITEMCODE = ITEMCODE,
//                            ITEMNAME = ITEMNAME,
//                            STARTTAXI = LOCATIONCODE,
//                            ENDTAXI = "OutLine",
//                            TYPE = 2,
//                            PRIORITY = 1,
//                            STATE = -1,
//                            CONTAINER = BARCODE,
//                            CODE = DateTime.Now.ToString("yyyyMMddhh")
//                        });
//                        string sql5 = @"
//UPDATE WMS_BILL_COUNTDETAIL SET CHECKFLAG = 1 WHERE ID = @ID
//";
//                        b = b + dbContext.Db.Ado.ExecuteCommand(sql5, new List<SugarParameter>() {
//                            new SugarParameter("@ID",ID),
//                        });
//                        dbContext.Db.Ado.CommitTran();
//                    } 
//                    if(a == PUTOUTHEADER.Rows.Count && b == PUTOUTHEADER.Rows.Count)
//                    {
//                        ReslutJson = new ReslutJson()
//                        {
//                            success = true,
//                            msg = "盘点任务生成成功"
//                        };
//                    }
//                    else
//                    {
//                        ReslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "盘点信息下发出错",
//                        };
//                        dbContext.Db.Ado.RollbackTran();
//                    }

//                }
//                else
//                {
//                    ReslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "没有查到盘点信息",
//                    };
//                }                 
//            }
//            catch (Exception ex)
//            {
//                ReslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };
//                LOGService.WriteExceptionLog(ex);
//                dbContext.Db.Ado.RollbackTran();
//            }
//            return ReslutJson;
//        }
//        public static ReslutJson CountPutOut(string id)
//        {
//            ReslutJson ReslutJson = new ReslutJson();
//            dbContext.Db.Ado.BeginTran();
//            try
//            {
//                //查询盘点单
//                String sql = @"SELECT C.QTY, C.SOURCEORDER,C.LOT, C.BARCODE, B.TUNNELNUM, A.WAREHOUSECODE,A.ITEMCODE,A.ITEMNAME,A.LOCATIONCODE FROM WMS_BILL_COUNTDETAIL A JOIN WMS_BASE_LOCATION B ON A.LOCATIONCODE = B.CODE AND A.WAREHOUSECODE = B.WAREHOUSECODE
//JOIN WMS_BILL_INVENTORYBOOK C ON A.LOCATIONCODE = C.LOCATIONCODE AND A.WAREHOUSECODE = C.WAREHOUSECODE WHERE A.ID = @ID";
//                var PUTOUTHEADER = dbContext.Db.Ado.GetDataTable(sql.ToString(), new List<SugarParameter>() {
//                    new SugarParameter("@ID",id)
//                });
//                if (PUTOUTHEADER != null && PUTOUTHEADER.Rows.Count > 0)
//                {
//                    int a = 0;
//                    int b = 0;
//                    int c = 0;
//                    int d = 0;
//                    for (int i = 0; i < PUTOUTHEADER.Rows.Count; i++)
//                    {
//                        string SOURCEORDER = PUTOUTHEADER.Rows[i]["SOURCEORDER"].ObjToString();
//                        string LOT = PUTOUTHEADER.Rows[i]["LOT"].ObjToString();
//                        string QTY = PUTOUTHEADER.Rows[i]["QTY"].ObjToString();
//                        string BARCODE = PUTOUTHEADER.Rows[i]["BARCODE"].ObjToString();
//                        string TUNNELNUM = PUTOUTHEADER.Rows[i]["TUNNELNUM"].ObjToString();
//                        string WAREHOUSECODE = PUTOUTHEADER.Rows[i]["WAREHOUSECODE"].ObjToString();
//                        string ITEMCODE = PUTOUTHEADER.Rows[i]["ITEMCODE"].ObjToString();
//                        string ITEMNAME = PUTOUTHEADER.Rows[i]["ITEMNAME"].ObjToString();
//                        string LOCATIONCODE = PUTOUTHEADER.Rows[i]["LOCATIONCODE"].ObjToString();
//                        if(i == 0)
//                        {
//                            a = a + WmsStockerTaskDao.insert(new WMS_STOCKER_TASK()
//                            {
//                                ID = Guid.NewGuid().ToString("D"),
//                                SOURCEORDER = SOURCEORDER,
//                                WAREHOUSECODE = WAREHOUSECODE,
//                                ITEMCODE = ITEMCODE,
//                                ITEMNAME = ITEMNAME,
//                                STARTTAXI = LOCATIONCODE,
//                                ENDTAXI = "OutLine",
//                                TYPE = 2,
//                                PRIORITY = 1,
//                                STATE = 0,
//                                CONTAINER = BARCODE,
//                                CODE = DateTime.Now.ToString("yyyyMMddhh")
//                            });

//                            sql = @"
//UPDATE WMS_BASE_LOCATION SET ISAVIALABLE=0 WHERE CODE=@CODE AND WAREHOUSECODE = @WAREHOUSECODE AND ISAVIALABLE=1
//";
//                            b = dbContext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>() {
//                                new SugarParameter("@WAREHOUSECODE",WAREHOUSECODE),
//                                new SugarParameter("@CODE",LOCATIONCODE)
//                            });

//                            c = WmsBillPutinBindingHeaderDao.insert(new WMS_BILL_PUTINBINDINGHEADER()
//                            {
//                                ID = id,
//                                SOURCEORDER = SOURCEORDER,
//                                WAREHOUSECODE = WAREHOUSECODE,
//                                CONTAINER = BARCODE,
//                                ITEMCODE = ITEMCODE,
//                                ITEMNAME = ITEMNAME,
//                                QUESTFLAG = 0,
//                                STOCKFLAG = 0,
//                                REMARK = "",
//                                TUNNELNUM = TUNNELNUM,
//                                ENABLED = 1
//                            });
//                        }
//                        d = d + WmsBillPutinBindingBodyDao.insert(new WMS_BILL_PUTINBINDINGBODY()
//                        {
//                            ID = Guid.NewGuid().ToString("D"),
//                            HEADERID = id,
//                            BARCODE = BARCODE,
//                            LOT = LOT,
//                            Qty = Convert.ToInt32(QTY),
//                            ENABLED = 0,
//                            CONTAINER = BARCODE
//                        });

//                    }                    
//                    dbContext.Db.Ado.CommitTran();
//                    if (a > 0  && b > 0 && c > 0 && d > 0)
//                    {
//                        ReslutJson = new ReslutJson()
//                        {
//                            success = true,
//                            msg = "盘点任务生成成功"
//                        };
//                    }
//                    else
//                    {
//                        ReslutJson = new ReslutJson()
//                        {
//                            success = false,
//                            msg = "盘点信息下发出错",
//                        };
//                        dbContext.Db.Ado.RollbackTran();
//                    }

//                }
//                else
//                {
//                    ReslutJson = new ReslutJson()
//                    {
//                        success = false,
//                        msg = "没有查到盘点信息",
//                    };
//                }

//            }
//            catch (Exception ex)
//            {
//                ReslutJson = new ReslutJson()
//                {
//                    success = false,
//                    msg = ex.Message
//                };
//                LOGService.WriteExceptionLog(ex);
//                dbContext.Db.Ado.RollbackTran();
//            }
//            return ReslutJson;
//        }
    }
}