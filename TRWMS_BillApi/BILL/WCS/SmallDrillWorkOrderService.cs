using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using SqlSugar;
using TR.ORM.Model;
using TR.Service;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.DAL;
using TRXWMS.BILL.LOG;
using TRXWMS.BILL.PrepareMachine;

namespace TRXWMS.BILL.WCS
{
    /// <summary>
    /// 小钻出库工单调度：单盒异常、换料补配、开启下一规格/工单。
    /// </summary>
    public class SmallDrillWorkOrderService : ISmallDrillWorkOrderService
    {
        private const string SmallDrillRemark = "组盘出库";
        private const string SmallDrillTarget = "1002";
        private const string ReplacementTag = "换料补配";
        private const string AbnormalTag = "异常";
        private const string ReplacedContainerTag = "原盒:";
        private const string RootContainerTag = "根盒:";

        private readonly DbContext _db = new DbContext();
        private readonly wms_bill_putoutheaderService _putoutHeaderService = new wms_bill_putoutheaderService();
        private readonly wms_bill_putoutbodyService _putoutBodyService = new wms_bill_putoutbodyService();
        private readonly wms_stocker_taskService _taskService = new wms_stocker_taskService();
        private readonly wms_base_locationService _locationService = new wms_base_locationService();

        public SmallDrillWorkOrderListResult GetWorkOrders(int historyDays)
        {
            if (historyDays <= 0)
            {
                historyDays = 7;
            }

            DateTime historyFrom = DateTime.Now.AddDays(-historyDays);
            var headers = QuerySmallDrillHeaders(h => h.CREATETIME >= historyFrom);
            var tasks = QuerySmallDrillTasks();

            var orderGroups = headers
                .GroupBy(h => h.ORDERID.Trim())
                .Select(g => BuildWorkOrderNode(g.Key, g.ToList(), tasks))
                .OrderBy(n => n.CreateTime)
                .ToList();

            string currentOrderId = ResolveCurrentOrderId(orderGroups, tasks);
            int currentSkuSeq = ResolveCurrentSkuSeq(currentOrderId, headers, tasks);
            SmallDrillWorkOrderTreeNode currentNode = null;

            foreach (var node in orderGroups)
            {
                bool isCurrentOrder = string.Equals(node.OrderId, currentOrderId, StringComparison.OrdinalIgnoreCase);
                NormalizeQueueStatusForManualAdvance(node, isCurrentOrder, orderGroups);

                bool readOnly = node.QueueStatus == "completed" || node.QueueStatus == "abnormal" || node.QueueStatus == "waiting";
                int executingSkuSeq = ResolveCurrentSkuSeq(node.OrderId, headers, tasks);
                int advanceFromSkuSeq = ResolveAdvanceFromSkuSeq(node.OrderId, headers, tasks);

                node.ReadOnly = readOnly;
                bool canActivateNextOrder = isCurrentOrder && ResolveCanActivateNextOrder(node.OrderId, orderGroups);
                node.CanActivateNextOrder = canActivateNextOrder;
                node.CanCancelEntireOrder = (node.QueueStatus == "active" || node.QueueStatus == "waiting")
                    && node.PendingTrays > 0;
                var orderHeadersForNode = headers
                    .Where(h => OrderIdEquals(h.ORDERID, node.OrderId))
                    .ToList();
                node.CanForceCloseWorkOrder = node.QueueStatus == "active"
                    && node.PendingTrays == 0
                    && HasAbnormalAwaitingSwap(orderHeadersForNode);

                if (node.Children != null)
                {
                    foreach (var spec in node.Children)
                    {
                        spec.ReadOnly = readOnly;
                        spec.IsCurrentExecutingSpec = node.QueueStatus == "active"
                            && executingSkuSeq > 0
                            && spec.SkuSeq == executingSkuSeq;
                        spec.CanActivateNextSpec = node.QueueStatus == "active"
                            && spec.SkuSeq == advanceFromSkuSeq
                            && !HasBlockingWork(node.OrderId, spec.SkuSeq)
                            && HasActivatableNextSpec(node.OrderId, spec.SkuSeq);
                        spec.CanCancelCurrentSpec = node.QueueStatus == "active"
                            && spec.IsCurrentExecutingSpec
                            && spec.PendingTrays > 0;
                        // 当前执行规格行也显示「下一工单」（父行折叠时用户常只看规格行）
                        spec.CanActivateNextOrder = canActivateNextOrder && spec.IsCurrentExecutingSpec;
                    }
                }
                if (isCurrentOrder)
                {
                    currentNode = node;
                }
            }

            var activeList = new List<SmallDrillWorkOrderTreeNode>();
            if (currentNode != null && currentNode.QueueStatus == "active")
            {
                activeList.Add(currentNode);
            }

            bool listCanActivateNextOrder = ResolveCanActivateNextOrder(currentOrderId, orderGroups);
            return new SmallDrillWorkOrderListResult
            {
                CurrentOrderId = currentOrderId,
                CurrentSkuSeq = currentSkuSeq,
                CanActivateNextOrder = listCanActivateNextOrder,
                ActivateNextOrderBlockReason = listCanActivateNextOrder
                    ? string.Empty
                    : ExplainCanActivateNextOrderBlock(currentOrderId, orderGroups),
                Active = activeList,
                Waiting = orderGroups.Where(n => n.QueueStatus == "waiting").OrderBy(n => n.CreateTime).ToList(),
                History = orderGroups
                    .Where(n => n.QueueStatus == "completed" || n.QueueStatus == "abnormal")
                    .OrderByDescending(n => n.CreateTime)
                    .Take(50)
                    .ToList()
            };
        }

        public SmallDrillSpecTrayListResult GetSpecTrays(string orderId, int skuSeq)
        {
            orderId = (orderId ?? string.Empty).Trim();
            var headers = _putoutHeaderService
                .Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget)
                .OrderBy(h => h.CREATETIME)
                .ToList();

            var tasks = QuerySmallDrillTasks()
                .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal) && t.SKUSEQ == skuSeq)
                .ToList();

            string workname = tasks.Select(t => t.WORKNAME).FirstOrDefault(w => !string.IsNullOrWhiteSpace(w)) ?? orderId;
            string spec = headers.Select(h => h.SOURCEORDER).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;
            int demandQty = GetDemandQty(headers);
            int activeQty = GetActiveQty(headers);

            var allHeaders = QuerySmallDrillHeaders();
            var allTasks = QuerySmallDrillTasks();
            var orderHeaders = allHeaders
                .Where(h => string.Equals((h.ORDERID ?? string.Empty).Trim(), orderId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var orderTasks = allTasks
                .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal))
                .ToList();

            var woNode = BuildWorkOrderNode(orderId, orderHeaders, orderTasks);
            int executingSkuSeq = ResolveCurrentSkuSeq(orderId, allHeaders, allTasks);
            bool specAwaitingSwap = headers.Any(IsAbnormalAwaitingSwapHeader);
            bool isCurrentSpec = woNode.QueueStatus == "active"
                && skuSeq > 0
                && ((executingSkuSeq > 0 && skuSeq == executingSkuSeq) || specAwaitingSwap);
            bool readOnly = woNode.QueueStatus != "active" || !isCurrentSpec;

            var swappedContainers = new HashSet<string>(
                headers
                    .Where(h => IsReplacementRemark(h.REMARK))
                    .Select(h => ParseReplacedContainer(h.REMARK))
                    .Where(c => !string.IsNullOrWhiteSpace(c)),
                StringComparer.OrdinalIgnoreCase);

            var trayItems = headers.Select(h =>
            {
                var trayType = IsReplacementRemark(h.REMARK) ? "换料补配" : "正常";
                bool alreadySwappedOut = swappedContainers.Contains(h.CONTAINER ?? string.Empty);
                return new SmallDrillSpecTrayItem
                {
                    HeaderId = h.ID,
                    Container = h.CONTAINER,
                    Spec = h.SOURCEORDER,
                    State = h.STATE,
                    StateText = MapStateText(h.STATE),
                    TrayType = trayType,
                    ReplacedContainer = ParseReplacedContainer(h.REMARK),
                    RootContainer = ParseRootContainer(h.REMARK, h.CONTAINER),
                    LocationCode = h.LOCATIONCODE,
                    Remark = h.REMARK,
                    CreateTime = h.CREATETIME,
                    ReadOnly = readOnly,
                    CanMarkAbnormal = !readOnly && (h.STATE == 0 || h.STATE == 2),
                    AlreadySwappedOut = alreadySwappedOut,
                    CanSwap = !readOnly && h.STATE == 5 && !alreadySwappedOut
                };
            }).ToList();

            return new SmallDrillSpecTrayListResult
            {
                OrderId = orderId,
                Workname = workname,
                SkuSeq = skuSeq,
                Spec = spec,
                DemandQty = demandQty,
                ActiveQty = activeQty,
                IsCurrentExecutingSpec = isCurrentSpec,
                ReadOnly = readOnly,
                Trays = trayItems
            };
        }

        public SmallDrillWorkOrderActionResult MarkTrayAbnormal(string orderId, int skuSeq, string container, string reason, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            container = (container ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(container))
            {
                result.Message = "orderId与container不能为空";
                return result;
            }

            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? "现场无法继续装盘" : reason.Trim();

            try
            {
                if (!EnsureCurrentExecutingSpec(orderId, skuSeq, out string specError))
                {
                    result.Message = specError;
                    return result;
                }

                var header = _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.CONTAINER == container
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget).FirstOrDefault();

                if (header == null)
                {
                    result.Message = "未找到对应托盘出库单";
                    return result;
                }

                if (header.STATE == 3)
                {
                    result.Message = "已装盘的托盘不能标记异常";
                    return result;
                }

                if (header.STATE == 5)
                {
                    result.Message = "该托盘已是异常状态";
                    return result;
                }

                if (header.STATE == 1)
                {
                    result.Message = "堆垛机执行中，请稍后再标记异常";
                    return result;
                }

                if (header.STATE != 0 && header.STATE != 2)
                {
                    result.Message = "当前状态不允许标记异常";
                    return result;
                }

                int released = 0;
                if (header.STATE == 0)
                {
                    released = ReleaseLocationForHeader(header, editor) ? 1 : 0;
                }

                header.STATE = 5;
                header.REMARK = AppendRemarkTag(header.REMARK, AbnormalTag + ":" + reason);
                header.LASTEDITOR = editor;
                header.LASTEDITTIME = DateTime.Now;
                _putoutHeaderService.Update(header);

                int taskAffected = MarkTasksAbnormalForTray(orderId, skuSeq, container, editor);
                result.AffectedCount = 1 + taskAffected;
                result.Success = true;
                result.ReleasedLocations = released;
                if (taskAffected <= 0)
                {
                    result.Message = string.Format(
                        "托盘 {0} 出库单已标记异常，但未更新到堆垛机任务（CONTAINER={0}, SKUSEQ={1}）。",
                        container,
                        skuSeq);
                }
                else
                {
                    result.Message = string.Format("托盘 {0} 已标记异常（出库单 1 条，任务 {1} 条）。", container, taskAffected);
                }
                LOGService.WriteOperateLogT(result.Message, "小钻工单-单盒异常", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "标记异常失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-单盒异常", ip ?? string.Empty);
            }

            return result;
        }

        private int MarkTasksAbnormalForTray(string orderId, int skuSeq, string container, string editor)
        {
            orderId = (orderId ?? string.Empty).Trim();
            container = (container ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(container))
            {
                return 0;
            }

            string orderPrefix = orderId + "-";
            var parameters = new List<SugarParameter>
            {
                new SugarParameter("@EDITOR", editor),
                new SugarParameter("@ENDTAXI", SmallDrillTarget),
                new SugarParameter("@CONTAINER", container),
                new SugarParameter("@SKUSEQ", skuSeq),
                new SugarParameter("@ORDERPREFIX", orderPrefix + "%")
            };

            string updateSql = @"
UPDATE wms_stocker_task
SET STATE = 5,
    LASTEDITOR = @EDITOR,
    LASTEDITTIME = SYSDATE(),
    TASKMODE = CASE WHEN UPPER(IFNULL(TASKMODE, '')) = 'NORMAL' THEN 'PRE_FETCH' ELSE TASKMODE END
WHERE TYPE = 2
  AND ENDTAXI = @ENDTAXI
  AND CONTAINER = @CONTAINER
  AND SKUSEQ = @SKUSEQ
  AND STATE >= 0
  AND STATE <= 2
  AND ORDERINFO IS NOT NULL
  AND ORDERINFO LIKE @ORDERPREFIX";

            int affected = _db.Db.Ado.ExecuteCommand(updateSql, parameters);
            if (affected > 0)
            {
                return affected;
            }

            // 兜底：部分历史数据 ORDERINFO 格式不一致，按容器+规格序号匹配小钻出库任务
            return _db.Db.Ado.ExecuteCommand(@"
UPDATE wms_stocker_task
SET STATE = 5,
    LASTEDITOR = @EDITOR,
    LASTEDITTIME = SYSDATE(),
    TASKMODE = CASE WHEN UPPER(IFNULL(TASKMODE, '')) = 'NORMAL' THEN 'PRE_FETCH' ELSE TASKMODE END
WHERE TYPE = 2
  AND ENDTAXI = @ENDTAXI
  AND CONTAINER = @CONTAINER
  AND SKUSEQ = @SKUSEQ
  AND STATE >= 0
  AND STATE <= 2",
                parameters);
        }

        public SmallDrillWorkOrderActionResult SwapTray(string orderId, int skuSeq, string abnormalContainer, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            abnormalContainer = (abnormalContainer ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(abnormalContainer))
            {
                result.Message = "orderId与abnormalContainer不能为空";
                return result;
            }

            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();

            try
            {
                if (!EnsureCurrentExecutingSpec(orderId, skuSeq, out string specError))
                {
                    result.Message = specError;
                    return result;
                }

                var abnormalHeader = _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.CONTAINER == abnormalContainer
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget).FirstOrDefault();

                if (abnormalHeader == null)
                {
                    result.Message = "未找到对应托盘";
                    return result;
                }

                if (abnormalHeader.STATE != 5)
                {
                    result.Message = "仅已标记异常的托盘可换料；STATE=2 请先标记异常";
                    return result;
                }

                if (HasReplacementForContainer(orderId, skuSeq, abnormalContainer))
                {
                    result.Message = string.Format("盒 {0} 已换过料，同一盒仅允许换料一次", abnormalContainer);
                    return result;
                }

                var specHeaders = _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget).ToList();

                int demandQty = GetDemandQty(specHeaders);
                if (!CanSwapWithinDemand(specHeaders, demandQty))
                {
                    result.Message = string.Format(
                        "当前规格已完成/在途数已达需求上限（需求 {0}），请先处理其它在途托盘",
                        demandQty);
                    return result;
                }

                string requestedSku = abnormalHeader.SOURCEORDER;
                DataTable pick = SelectAvailableOutboundByGrindFallback(requestedSku, 1);
                if (pick == null || pick.Rows.Count == 0)
                {
                    result.Message = "库存不足，无法补配新托盘";
                    return result;
                }

                string location = pick.Rows[0]["LOCATIONCODE"].ToString();
                string newContainer = pick.Rows[0]["CONTAINER"].ToString();
                string actualSourceOrder = pick.Rows[0]["SOURCEORDER"]?.ToString()?.Trim() ?? requestedSku;

                if (specHeaders.Any(h => h.CONTAINER == newContainer && h.STATE != 5))
                {
                    result.Message = "补配托盘已在当前规格任务中，请稍后重试";
                    return result;
                }

                var refTask = _taskService.Query(t =>
                    t.TYPE == 2
                    && t.ENDTAXI == SmallDrillTarget
                    && t.CONTAINER == abnormalContainer
                    && t.ORDERINFO != null
                    && t.ORDERINFO.StartsWith(orderId + "-")
                    && t.SKUSEQ == skuSeq).FirstOrDefault();

                string workname = refTask?.WORKNAME ?? orderId;
                string orderInfo = refTask?.ORDERINFO;
                if (string.IsNullOrWhiteSpace(orderInfo))
                {
                    var sibling = _taskService.Query(t =>
                        t.TYPE == 2
                        && t.ENDTAXI == SmallDrillTarget
                        && t.ORDERINFO != null
                        && t.ORDERINFO.StartsWith(orderId + "-")
                        && t.SKUSEQ == skuSeq).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.ORDERINFO));
                    orderInfo = sibling?.ORDERINFO;
                }
                if (string.IsNullOrWhiteSpace(orderInfo))
                {
                    orderInfo = orderId + "-" + actualSourceOrder + "-1-" + skuSeq;
                }

                _db.Db.Ado.ExecuteCommand(
                    @"UPDATE wms_base_location SET OUTENABLED=0 WHERE LOCATIONCODE=@LOCATION",
                    new SugarParameter("@LOCATION", location));

                string headerId = Guid.NewGuid().ToString("D");
                string headerRemark = BuildReplacementRemark(abnormalHeader, abnormalContainer);

                _db.Db.Ado.ExecuteCommand(@"
INSERT INTO wms_bill_putoutheader (ID,ZONECODE,SOURCEORDER,CONTAINER,CONTAINERTYPE,QUESTFLAG,STATE,TARGET,LOCATIONCODE,ENABLED,REMARK,CREATOR,CREATETIME,SKUSEQ,ORDERID)
VALUES(@ID,'101',@SOURCEORDER,@CONTAINER,'0',0,0,'1002',@LOCATION,1,@REMARK,@CREATOR,SYSDATE(),@SKUSEQ,@ORDERID)",
                    new List<SugarParameter>
                    {
                        new SugarParameter("@ID", headerId),
                        new SugarParameter("@SOURCEORDER", actualSourceOrder),
                        new SugarParameter("@CONTAINER", newContainer),
                        new SugarParameter("@LOCATION", location),
                        new SugarParameter("@REMARK", headerRemark),
                        new SugarParameter("@CREATOR", editor),
                        new SugarParameter("@SKUSEQ", skuSeq),
                        new SugarParameter("@ORDERID", orderId)
                    });

                _db.Db.Ado.ExecuteCommand(@"
INSERT INTO wms_bill_putoutbody (ID,HEADERID,ITEMCODE,LOT,TASK_NO_SN,CREATOR,CREATETIME)
VALUES(@ID,@HEADERID,@ITEMCODE,'','',@CREATOR,SYSDATE())",
                    new List<SugarParameter>
                    {
                        new SugarParameter("@ID", Guid.NewGuid().ToString("D")),
                        new SugarParameter("@HEADERID", headerId),
                        new SugarParameter("@ITEMCODE", actualSourceOrder),
                        new SugarParameter("@CREATOR", editor)
                    });

                string taskId = Guid.NewGuid().ToString("D");
                _db.Db.Ado.ExecuteCommand(@"
INSERT INTO wms_stocker_task (ID,CODE,ENDTAXI,CONTAINER,STARTTAXI,TYPE,STARTTIME,ZONECODE,SOURCEORDER,PRIORITY,STATE,CREATETIME,CREATOR,LASTEDITOR,LASTEDITTIME,SKUSEQ,TASKMODE,ORDERINFO,WORKNAME)
VALUES(@ID,@CODE,'1002',@CONTAINER,@START,2,SYSDATE(),'101',@SOURCEORDER,2,0,SYSDATE(),@CREATOR,@CREATOR,SYSDATE(),@SKUSEQ,'NORMAL',@ORDERINFO,@WORKNAME)",
                    new List<SugarParameter>
                    {
                        new SugarParameter("@ID", taskId),
                        new SugarParameter("@CODE", taskId),
                        new SugarParameter("@CONTAINER", newContainer),
                        new SugarParameter("@START", location),
                        new SugarParameter("@SOURCEORDER", actualSourceOrder),
                        new SugarParameter("@CREATOR", editor),
                        new SugarParameter("@SKUSEQ", skuSeq),
                        new SugarParameter("@ORDERINFO", orderInfo),
                        new SugarParameter("@WORKNAME", workname)
                    });

                abnormalHeader.REMARK = AppendRemarkTag(abnormalHeader.REMARK, "已换料");
                abnormalHeader.LASTEDITOR = editor;
                abnormalHeader.LASTEDITTIME = DateTime.Now;
                _putoutHeaderService.Update(abnormalHeader);

                result.Success = true;
                result.NewContainer = newContainer;
                result.AffectedCount = 1;
                result.Message = string.Format("换料成功：原盒 {0} → 新盒 {1}（{2}）", abnormalContainer, newContainer, actualSourceOrder);

                NotifyPrepareMachineQrCodeUpdate(
                    abnormalContainer,
                    newContainer,
                    actualSourceOrder,
                    editor,
                    result);

                LOGService.WriteOperateLogT(result.Message, "小钻工单-换料补配", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "换料失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-换料补配", ip ?? string.Empty);
            }

            return result;
        }

        public SmallDrillWorkOrderActionResult ActivateNextSpec(string orderId, int currentSkuSeq, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();

            try
            {
                if (HasBlockingWork(orderId, currentSkuSeq))
                {
                    result.Message = "当前规格仍有未完成且未标记异常的托盘，请先处理";
                    return result;
                }

                int nextSkuSeq = currentSkuSeq + 1;
                if (!HasSpec(orderId, nextSkuSeq))
                {
                    result.Message = "当前工单没有下一规格";
                    return result;
                }

                if (!HasActivatableNextSpec(orderId, currentSkuSeq))
                {
                    result.Message = string.Format(
                        "工单 {0} 的 SKUSEQ={1} 已全部取消或异常，无法开启下一规格；请使用顶部「开启下一工单」处理排队工单",
                        orderId,
                        nextSkuSeq);
                    return result;
                }

                int activated = ActivatePendingTasksToNormal(orderId, nextSkuSeq, editor);
                if (activated == 0)
                {
                    result.Message = string.Format(
                        "工单 {0} 存在 SKUSEQ={1}，但未找到可激活的 PRE_FETCH 任务，请检查 ORDERINFO 与任务状态",
                        orderId,
                        nextSkuSeq);
                    return result;
                }

                result.Success = true;
                result.NextSkuSeq = nextSkuSeq;
                result.AffectedCount = activated;
                result.Message = string.Format("已开启工单 {0} 的下一规格 SKUSEQ={1}（激活 {2} 条任务）", orderId, nextSkuSeq, activated);
                LOGService.WriteOperateLogT(result.Message, "小钻工单-开启下一规格", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "开启下一规格失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-开启下一规格", ip ?? string.Empty);
            }

            return result;
        }

        public SmallDrillWorkOrderActionResult ActivateNextWorkOrder(string currentOrderId, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();

            try
            {
                var orderGroups = BuildWorkOrderGroupList(7);
                if (!ResolveCanActivateNextOrder(currentOrderId, orderGroups))
                {
                    if (!string.IsNullOrEmpty(currentOrderId) && HasBlockingWork(currentOrderId, null))
                    {
                        result.Message = "当前工单仍有未完成且未标记异常的托盘，请先处理";
                    }
                    else if (HasOtherExecutingWorkOrder(currentOrderId, orderGroups))
                    {
                        result.Message = "已有其他工单处于执行中，无法重复开启";
                    }
                    else
                    {
                        result.Message = "等待队列中没有可开启的下一工单";
                    }

                    return result;
                }

                string nextOrderId = FindNextPendingOrderId(currentOrderId);
                if (string.IsNullOrWhiteSpace(nextOrderId))
                {
                    nextOrderId = FindNextWaitingOrderIdFromQueue(currentOrderId, orderGroups)?.OrderId;
                }

                if (string.IsNullOrWhiteSpace(nextOrderId))
                {
                    result.Message = "等待队列中没有可开启的下一工单";
                    return result;
                }

                int activated = ActivatePendingTasksToNormal(nextOrderId, 1, editor);
                if (activated == 0)
                {
                    result.Message = string.Format(
                        "已定位下一工单 {0}，但未找到可激活的 PRE_FETCH 任务，请检查 ORDERINFO 与任务状态",
                        nextOrderId);
                    return result;
                }

                result.Success = true;
                result.NextOrderId = nextOrderId;
                result.AffectedCount = activated;
                result.Message = string.Format("已开启下一工单：{0}（激活 {1} 条任务）", nextOrderId, activated);
                LOGService.WriteOperateLogT(result.Message, "小钻工单-开启下一工单", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "开启下一工单失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-开启下一工单", ip ?? string.Empty);
            }

            return result;
        }

        public SmallDrillWorkOrderActionResult ForceCloseWorkOrder(string orderId, string reason, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? "强制结案(无库存无法换料)" : reason.Trim();

            try
            {
                var allHeaders = QuerySmallDrillHeaders();
                var allTasks = QuerySmallDrillTasks();
                var orderHeaders = allHeaders
                    .Where(h => OrderIdEquals(h.ORDERID, orderId))
                    .ToList();
                if (orderHeaders.Count == 0)
                {
                    result.Message = "工单不存在";
                    return result;
                }

                var orderTasks = allTasks
                    .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal))
                    .ToList();
                var woNode = BuildWorkOrderNode(orderId, orderHeaders, orderTasks);
                if (woNode.QueueStatus != "active")
                {
                    result.Message = "仅进行中的工单可强制结案";
                    return result;
                }

                if (woNode.PendingTrays > 0)
                {
                    result.Message = "仍有待处理托盘，请先完成出库或整单取消";
                    return result;
                }

                if (!HasAbnormalAwaitingSwap(orderHeaders))
                {
                    result.Message = "当前工单没有待换料的异常托盘，无需强制结案";
                    return result;
                }

                if (orderHeaders.Any(h => h.STATE == 1))
                {
                    result.Message = "存在堆垛机执行中的托盘，请稍后再操作";
                    return result;
                }

                var awaitingSwapHeaders = orderHeaders.Where(IsAbnormalAwaitingSwapHeader).ToList();
                int headerAffected = 0;
                int taskAffected = 0;
                foreach (var header in awaitingSwapHeaders)
                {
                    header.REMARK = AppendRemarkTag(header.REMARK, AbnormalTag + ":" + reason);
                    header.LASTEDITOR = editor;
                    header.LASTEDITTIME = DateTime.Now;
                    _putoutHeaderService.Update(header);
                    headerAffected++;
                    taskAffected += MarkTasksAbnormalForTray(orderId, header.SKUSEQ, header.CONTAINER, editor);
                }

                result.Success = true;
                result.AffectedCount = headerAffected;
                result.Message = string.Format(
                    "工单 {0} 已强制结案：处理待换料异常 {1} 个托盘，更新任务 {2} 条",
                    orderId,
                    headerAffected,
                    taskAffected);
                LOGService.WriteOperateLogT(result.Message, "小钻工单-强制结案", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "强制结案失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-强制结案", ip ?? string.Empty);
            }

            return result;
        }

        public SmallDrillWorkOrderActionResult CancelCurrentSpec(string orderId, int skuSeq, string reason, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? "取消当前规格" : reason.Trim();

            try
            {
                if (!EnsureCurrentExecutingSpec(orderId, skuSeq, out string specError))
                {
                    result.Message = specError;
                    return result;
                }

                result = MarkPendingTraysAbnormal(orderId, skuSeq, reason, editor, ip, "小钻工单-取消当前规格");
            }
            catch (Exception ex)
            {
                result.Message = "取消当前规格失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-取消当前规格", ip ?? string.Empty);
            }

            return result;
        }

        public SmallDrillWorkOrderActionResult CancelEntireOrder(string orderId, string reason, string editor, string ip)
        {
            var result = new SmallDrillWorkOrderActionResult();
            orderId = (orderId ?? string.Empty).Trim();
            editor = string.IsNullOrWhiteSpace(editor) ? "工单调度" : editor.Trim();
            reason = string.IsNullOrWhiteSpace(reason) ? "整单取消" : reason.Trim();

            try
            {
                var allHeaders = QuerySmallDrillHeaders();
                var allTasks = QuerySmallDrillTasks();
                var orderHeaders = allHeaders
                    .Where(h => OrderIdEquals(h.ORDERID, orderId))
                    .ToList();
                if (orderHeaders.Count == 0)
                {
                    result.Message = "工单不存在";
                    return result;
                }

                var orderTasks = allTasks
                    .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal))
                    .ToList();
                var woNode = BuildWorkOrderNode(orderId, orderHeaders, orderTasks);
                if (woNode.QueueStatus != "active" && woNode.QueueStatus != "waiting")
                {
                    result.Message = "仅进行中或等待中的工单可整单取消";
                    return result;
                }

                if (!orderHeaders.Any(h => h.STATE >= 0 && h.STATE <= 2))
                {
                    result.Message = "当前工单没有可取消的未完成托盘";
                    return result;
                }

                if (orderHeaders.Any(h => h.STATE == 1))
                {
                    result.Message = "存在堆垛机执行中的托盘，请稍后再取消";
                    return result;
                }

                int headerAffected = 0;
                int taskAffected = 0;
                int released = 0;
                foreach (var header in orderHeaders.Where(h => h.STATE == 0 || h.STATE == 2).ToList())
                {
                    if (header.STATE == 0)
                    {
                        if (ReleaseLocationForHeader(header, editor))
                        {
                            released++;
                        }
                    }

                    header.STATE = 5;
                    header.REMARK = AppendRemarkTag(header.REMARK, AbnormalTag + ":" + reason);
                    header.LASTEDITOR = editor;
                    header.LASTEDITTIME = DateTime.Now;
                    _putoutHeaderService.Update(header);
                    headerAffected++;
                    taskAffected += MarkTasksAbnormalForTray(orderId, header.SKUSEQ, header.CONTAINER, editor);
                }

                result.Success = true;
                result.AffectedCount = headerAffected;
                result.ReleasedLocations = released;
                result.Message = string.Format(
                    "工单 {0} 已整单取消：标记异常 {1} 个托盘，更新任务 {2} 条，释放库位 {3} 个",
                    orderId,
                    headerAffected,
                    taskAffected,
                    released);
                LOGService.WriteOperateLogT(result.Message, "小钻工单-整单取消", ip ?? string.Empty);
            }
            catch (Exception ex)
            {
                result.Message = "整单取消失败：" + ex.Message;
                LOGService.WriteOperateLogF(ex.ToString(), "小钻工单-整单取消", ip ?? string.Empty);
            }

            return result;
        }

        private SmallDrillWorkOrderActionResult MarkPendingTraysAbnormal(string orderId, int skuSeq, string reason, string editor, string ip, string logTitle)
        {
            var result = new SmallDrillWorkOrderActionResult();
            var headers = _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget
                    && (h.STATE == 0 || h.STATE == 2))
                .ToList();

            if (headers.Count == 0)
            {
                result.Message = "当前规格没有可取消的未完成托盘";
                return result;
            }

            if (_putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget
                    && h.STATE == 1).Any())
            {
                result.Message = "存在堆垛机执行中的托盘，请稍后再取消";
                return result;
            }

            int headerAffected = 0;
            int taskAffected = 0;
            int released = 0;
            foreach (var header in headers)
            {
                if (header.STATE == 0)
                {
                    if (ReleaseLocationForHeader(header, editor))
                    {
                        released++;
                    }
                }

                header.STATE = 5;
                header.REMARK = AppendRemarkTag(header.REMARK, AbnormalTag + ":" + reason);
                header.LASTEDITOR = editor;
                header.LASTEDITTIME = DateTime.Now;
                _putoutHeaderService.Update(header);
                headerAffected++;
                taskAffected += MarkTasksAbnormalForTray(orderId, skuSeq, header.CONTAINER, editor);
            }

            result.Success = true;
            result.AffectedCount = headerAffected;
            result.ReleasedLocations = released;
            result.Message = string.Format(
                "规格 SKUSEQ={0} 已取消：标记异常 {1} 个托盘，更新任务 {2} 条，释放库位 {3} 个",
                skuSeq,
                headerAffected,
                taskAffected,
                released);
            LOGService.WriteOperateLogT(result.Message, logTitle, ip ?? string.Empty);
            return result;
        }

        #region 树形与规格

        private SmallDrillWorkOrderTreeNode BuildWorkOrderNode(string orderId, List<wms_bill_putoutheader> headers, List<wms_stocker_task> allTasks)
        {
            var orderTasks = allTasks
                .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal))
                .ToList();

            string workname = orderTasks.Select(t => t.WORKNAME).FirstOrDefault(w => !string.IsNullOrWhiteSpace(w)) ?? orderId;

            var specNodes = headers
                .GroupBy(h => h.SKUSEQ)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var list = g.ToList();
                    return new SmallDrillWorkOrderTreeNode
                    {
                        NodeId = orderId + "-S" + g.Key,
                        IsSpec = true,
                        OrderId = orderId,
                        Workname = workname,
                        SkuSeq = g.Key,
                        Spec = list.Select(x => x.SOURCEORDER).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty,
                        DemandQty = GetDemandQty(list),
                        ActiveQty = GetActiveQty(list),
                        DoneTrays = list.Count(x => x.STATE == 3),
                        AbnormalTrays = list.Count(x => x.STATE == 5),
                        PendingTrays = list.Count(x => x.STATE >= 0 && x.STATE <= 2),
                        CreateTime = orderTasks.Where(t => t.SKUSEQ == g.Key).Any()
                            ? orderTasks.Where(t => t.SKUSEQ == g.Key).Min(t => t.CREATETIME)
                            : list.Min(x => x.CREATETIME)
                    };
                })
                .ToList();

            int pendingTrays = headers.Count(h => h.STATE >= 0 && h.STATE <= 2);
            int doneTrays = headers.Count(h => h.STATE == 3);
            int abnormalTrays = headers.Count(h => h.STATE == 5);
            string queueStatus = ResolveQueueStatus(headers, orderTasks, pendingTrays, doneTrays, abnormalTrays);

            return new SmallDrillWorkOrderTreeNode
            {
                NodeId = orderId,
                IsSpec = false,
                OrderId = orderId,
                Workname = workname,
                QueueStatus = queueStatus,
                QueueStatusText = MapQueueStatusText(queueStatus),
                CreateTime = orderTasks.Any()
                    ? orderTasks.Min(t => t.CREATETIME)
                    : headers.Min(h => h.CREATETIME),
                DemandQty = specNodes.Sum(s => s.DemandQty),
                ActiveQty = specNodes.Sum(s => s.ActiveQty),
                DoneTrays = doneTrays,
                AbnormalTrays = abnormalTrays,
                PendingTrays = pendingTrays,
                SkuCount = specNodes.Count,
                Children = specNodes
            };
        }

        private bool EnsureCurrentExecutingSpec(string orderId, int skuSeq, out string error)
        {
            error = string.Empty;
            orderId = (orderId ?? string.Empty).Trim();

            var allHeaders = QuerySmallDrillHeaders();
            var allTasks = QuerySmallDrillTasks();
            var orderHeaders = allHeaders
                .Where(h => string.Equals((h.ORDERID ?? string.Empty).Trim(), orderId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (orderHeaders.Count == 0)
            {
                error = "工单不存在";
                return false;
            }

            var orderTasks = allTasks
                .Where(t => t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal))
                .ToList();
            var woNode = BuildWorkOrderNode(orderId, orderHeaders, orderTasks);

            if (woNode.QueueStatus != "active")
            {
                error = "仅进行中的工单可操作";
                return false;
            }

            int executingSkuSeq = ResolveCurrentSkuSeq(orderId, allHeaders, allTasks);
            if (executingSkuSeq <= 0 || skuSeq != executingSkuSeq)
            {
                error = "仅当前正在执行的规格可换料或标记异常";
                return false;
            }

            return true;
        }

        private static int GetDemandQty(List<wms_bill_putoutheader> specHeaders)
        {
            return specHeaders.Count(h => !IsReplacementRemark(h.REMARK));
        }

        private static int GetActiveQty(List<wms_bill_putoutheader> specHeaders)
        {
            return specHeaders.Count(h => h.STATE >= 0 && h.STATE <= 3);
        }

        private static bool CanSwapWithinDemand(List<wms_bill_putoutheader> specHeaders, int demandQty)
        {
            int doneQty = specHeaders.Count(h => h.STATE == 3);
            int inProgressQty = specHeaders.Count(h => h.STATE >= 0 && h.STATE <= 2);
            return doneQty + inProgressQty < demandQty;
        }

        private static bool IsReplacementRemark(string remark)
        {
            return !string.IsNullOrEmpty(remark) && remark.Contains(ReplacementTag);
        }

        private static string ParseReplacedContainer(string remark)
        {
            if (string.IsNullOrEmpty(remark))
            {
                return string.Empty;
            }

            int idx = remark.IndexOf(ReplacedContainerTag, StringComparison.Ordinal);
            if (idx < 0)
            {
                return string.Empty;
            }

            return remark.Substring(idx + ReplacedContainerTag.Length).Trim().Split('|')[0].Trim();
        }

        private static string ParseRootContainer(string remark, string container)
        {
            if (!string.IsNullOrEmpty(remark))
            {
                int rootIdx = remark.IndexOf(RootContainerTag, StringComparison.Ordinal);
                if (rootIdx >= 0)
                {
                    string root = remark.Substring(rootIdx + RootContainerTag.Length).Trim().Split('|')[0].Trim();
                    if (!string.IsNullOrWhiteSpace(root))
                    {
                        return root;
                    }
                }
            }

            if (IsReplacementRemark(remark))
            {
                string replaced = ParseReplacedContainer(remark);
                if (!string.IsNullOrWhiteSpace(replaced))
                {
                    return replaced;
                }
            }

            return (container ?? string.Empty).Trim();
        }

        private static string BuildReplacementRemark(wms_bill_putoutheader abnormalHeader, string abnormalContainer)
        {
            abnormalContainer = (abnormalContainer ?? string.Empty).Trim();
            string rootContainer = ParseRootContainer(abnormalHeader?.REMARK, abnormalContainer);
            string remark = SmallDrillRemark + "|" + ReplacementTag + "|" + ReplacedContainerTag + abnormalContainer;
            if (!string.IsNullOrWhiteSpace(rootContainer)
                && !string.Equals(rootContainer, abnormalContainer, StringComparison.OrdinalIgnoreCase))
            {
                remark += "|" + RootContainerTag + rootContainer;
            }

            return remark;
        }

        private bool HasReplacementForContainer(string orderId, int skuSeq, string container)
        {
            if (string.IsNullOrWhiteSpace(container))
            {
                return false;
            }

            string replacedTag = ReplacedContainerTag + container.Trim();
            return _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.SKUSEQ == skuSeq
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.REMARK.Contains(ReplacementTag)
                    && h.REMARK.Contains(replacedTag)
                    && h.TARGET == SmallDrillTarget)
                .Any();
        }

        private static string AppendRemarkTag(string remark, string tag)
        {
            if (string.IsNullOrWhiteSpace(remark))
            {
                return SmallDrillRemark + "|" + tag;
            }

            if (remark.Contains(tag))
            {
                return remark;
            }

            return remark + "|" + tag;
        }

        private bool HasSpec(string orderId, int skuSeq)
        {
            return _putoutHeaderService.Query(h =>
                h.ORDERID == orderId
                && h.SKUSEQ == skuSeq
                && h.REMARK != null
                && h.REMARK.Contains(SmallDrillRemark)
                && h.TARGET == SmallDrillTarget).Any();
        }

        private bool HasNextSpec(string orderId, int currentSkuSeq, List<wms_bill_putoutheader> allHeaders)
        {
            return allHeaders.Any(h =>
                OrderIdEquals(h.ORDERID, orderId)
                && h.SKUSEQ == currentSkuSeq + 1
                && h.REMARK != null
                && h.REMARK.Contains(SmallDrillRemark)
                && h.TARGET == SmallDrillTarget);
        }

        /// <summary>
        /// 下一规格是否存在可激活的 PRE_FETCH 任务（已全部取消/异常的规格不算）。
        /// </summary>
        private bool HasActivatableNextSpec(string orderId, int currentSkuSeq)
        {
            int nextSkuSeq = currentSkuSeq + 1;
            if (!HasSpec(orderId, nextSkuSeq))
            {
                return false;
            }

            return QueryPendingPrefetchTasksForOrder(orderId, nextSkuSeq).Count > 0;
        }

        #endregion

        #region 状态与队列

        private List<wms_bill_putoutheader> QuerySmallDrillHeaders(Func<wms_bill_putoutheader, bool> extraFilter = null)
        {
            var list = _putoutHeaderService
                .Query(h =>
                    h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget
                    && h.ORDERID != null
                    && h.ORDERID != "")
                .ToList();

            if (extraFilter != null)
            {
                list = list.Where(extraFilter).ToList();
            }

            return list;
        }

        private List<wms_stocker_task> QuerySmallDrillTasks()
        {
            return _taskService
                .Query(t =>
                    t.TYPE == 2
                    && t.ENDTAXI == SmallDrillTarget
                    && t.ORDERINFO != null
                    && t.ORDERINFO != "")
                .ToList();
        }

        private static string ResolveQueueStatus(
            List<wms_bill_putoutheader> headers,
            List<wms_stocker_task> orderTasks,
            int pendingTrays,
            int doneTrays,
            int abnormalTrays)
        {
            // 仍有待换料的异常托盘（非取消结案、未已换料）时，工单应保持在「进行中」
            if (pendingTrays == 0 && abnormalTrays > 0 && HasAbnormalAwaitingSwap(headers))
            {
                return "active";
            }

            if (pendingTrays == 0 && doneTrays > 0 && abnormalTrays == 0)
            {
                return "completed";
            }

            if (pendingTrays == 0 && abnormalTrays > 0 && doneTrays == 0)
            {
                return "abnormal";
            }

            if (pendingTrays == 0 && doneTrays > 0)
            {
                return abnormalTrays > 0 ? "abnormal" : "completed";
            }

            bool hasNormalActive = orderTasks.Any(t =>
                string.Equals(t.TASKMODE, "NORMAL", StringComparison.OrdinalIgnoreCase)
                && t.STATE >= 0
                && t.STATE <= 2);

            bool hasInProgressHeader = headers.Any(h => h.STATE >= 1 && h.STATE <= 2);
            if (hasNormalActive || hasInProgressHeader)
            {
                return "active";
            }

            bool hasProgress = doneTrays > 0 || abnormalTrays > 0;
            if (pendingTrays > 0 && !hasProgress)
            {
                return "waiting";
            }

            if (pendingTrays > 0)
            {
                return "active";
            }

            return abnormalTrays > 0 ? "abnormal" : "completed";
        }

        /// <summary>
        /// 异常托盘中是否仍有待换料（已标记异常、未换料完成、非取消结案）。
        /// </summary>
        private static bool HasAbnormalAwaitingSwap(List<wms_bill_putoutheader> headers)
        {
            return headers != null && headers.Any(IsAbnormalAwaitingSwapHeader);
        }

        private static bool IsAbnormalAwaitingSwapHeader(wms_bill_putoutheader header)
        {
            return header != null
                && header.STATE == 5
                && !IsReplacementRemark(header.REMARK)
                && !HasSwappedOutRemark(header.REMARK)
                && !IsClosureAbnormalRemark(header.REMARK);
        }

        private static bool HasSwappedOutRemark(string remark)
        {
            return !string.IsNullOrEmpty(remark) && remark.Contains("已换料");
        }

        /// <summary>
        /// 取消规格/整单标记的异常视为结案，不再保留在进行中。
        /// </summary>
        private static bool IsClosureAbnormalRemark(string remark)
        {
            if (string.IsNullOrEmpty(remark))
            {
                return false;
            }

            return remark.Contains("取消") || remark.Contains("强制结案");
        }

        private void NormalizeQueueStatusForManualAdvance(
            SmallDrillWorkOrderTreeNode node,
            bool isCurrentOrder,
            List<SmallDrillWorkOrderTreeNode> orderGroups)
        {
            if (!isCurrentOrder)
            {
                return;
            }

            if (node.QueueStatus == "completed" && HasWaitingWorkOrders(node.OrderId, orderGroups))
            {
                node.QueueStatus = "active";
                node.QueueStatusText = "待开启下一工单";
                return;
            }

            if (node.QueueStatus == "waiting" && (node.DoneTrays > 0 || node.AbnormalTrays > 0))
            {
                node.QueueStatus = "active";
                node.QueueStatusText = MapQueueStatusText("active");
            }
        }

        private string ResolveCurrentOrderId(List<SmallDrillWorkOrderTreeNode> orders, List<wms_stocker_task> tasks)
        {
            var active = orders
                .Where(s => s.QueueStatus == "active")
                .OrderByDescending(n => n.CreateTime)
                .FirstOrDefault();
            if (active != null)
            {
                return active.OrderId;
            }

            foreach (var node in orders.OrderBy(n => n.CreateTime))
            {
                if (node.QueueStatus == "completed"
                    && node.DoneTrays > 0
                    && !HasBlockingWork(node.OrderId, null)
                    && HasWaitingWorkOrders(node.OrderId))
                {
                    return node.OrderId;
                }
            }

            foreach (var node in orders.OrderBy(n => n.CreateTime))
            {
                if (node.QueueStatus == "waiting" && (node.DoneTrays > 0 || node.AbnormalTrays > 0))
                {
                    return node.OrderId;
                }
            }

            return tasks
                .Where(t =>
                    string.Equals(t.TASKMODE, "NORMAL", StringComparison.OrdinalIgnoreCase)
                    && t.STATE >= 0
                    && t.STATE <= 2
                    && !string.IsNullOrWhiteSpace(t.ORDERINFO))
                .OrderBy(t => t.CREATETIME)
                .Select(t => ExtractOrderIdFromOrderInfo(t.ORDERINFO))
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        }

        private static int ResolveCurrentSkuSeq(string orderId, List<wms_bill_putoutheader> headers, List<wms_stocker_task> tasks)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return 0;
            }

            var orderHeaders = headers
                .Where(h => string.Equals((h.ORDERID ?? string.Empty).Trim(), orderId.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            var orderTasks = tasks.Where(t =>
                t.ORDERINFO != null && t.ORDERINFO.StartsWith(orderId + "-", StringComparison.Ordinal)).ToList();

            int? fromTask = orderTasks
                .Where(t => string.Equals(t.TASKMODE, "NORMAL", StringComparison.OrdinalIgnoreCase) && t.STATE >= 0 && t.STATE <= 2)
                .OrderBy(t => t.SKUSEQ)
                .Select(t => (int?)t.SKUSEQ)
                .FirstOrDefault();

            if (fromTask.HasValue && fromTask.Value > 0)
            {
                return fromTask.Value;
            }

            int? fromHeader = orderHeaders
                .Where(h => h.STATE >= 1 && h.STATE <= 2)
                .OrderBy(h => h.SKUSEQ)
                .Select(h => (int?)h.SKUSEQ)
                .FirstOrDefault();

            if (fromHeader.HasValue && fromHeader.Value > 0)
            {
                return fromHeader.Value;
            }

            // 全部标记异常待换料时，无 NORMAL 任务/在途单头，仍视为当前执行规格
            int? fromAwaitingSwap = orderHeaders
                .GroupBy(h => h.SKUSEQ)
                .Where(g => g.Any(IsAbnormalAwaitingSwapHeader))
                .OrderBy(g => g.Key)
                .Select(g => (int?)g.Key)
                .FirstOrDefault();

            return fromAwaitingSwap ?? 0;
        }

        private int ResolveAdvanceFromSkuSeq(string orderId, List<wms_bill_putoutheader> headers, List<wms_stocker_task> tasks)
        {
            orderId = (orderId ?? string.Empty).Trim();
            int executing = ResolveCurrentSkuSeq(orderId, headers, tasks);
            if (executing > 0)
            {
                if (!HasBlockingWork(orderId, executing) && HasActivatableNextSpec(orderId, executing))
                {
                    return executing;
                }

                return 0;
            }

            var orderHeaders = headers
                .Where(h => OrderIdEquals(h.ORDERID, orderId))
                .ToList();
            foreach (int sku in orderHeaders.Select(h => h.SKUSEQ).Distinct().OrderByDescending(s => s))
            {
                if (HasBlockingWork(orderId, sku) || !HasNextSpec(orderId, sku, headers))
                {
                    continue;
                }

                bool hasProgress = orderHeaders.Any(h => h.SKUSEQ == sku && (h.STATE == 3 || h.STATE == 5));
                if (hasProgress)
                {
                    return sku;
                }
            }

            return 0;
        }

        private static bool OrderIdEquals(string left, string right)
        {
            return string.Equals((left ?? string.Empty).Trim(), (right ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool HasBlockingWork(string orderId, int? skuSeq)
        {
            return _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && (skuSeq == null || h.SKUSEQ == skuSeq.Value)
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget
                    && h.STATE >= 0
                    && h.STATE <= 2)
                .Any();
        }

        private bool HasWaitingWorkOrders(string currentOrderId, List<SmallDrillWorkOrderTreeNode> orderGroups = null)
        {
            if (!string.IsNullOrWhiteSpace(FindNextPendingOrderId(currentOrderId)))
            {
                return true;
            }

            return FindNextWaitingOrderIdFromQueue(currentOrderId, orderGroups) != null;
        }

        private bool ResolveCanActivateNextOrder(string currentOrderId, List<SmallDrillWorkOrderTreeNode> orderGroups = null)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(currentOrderId) && HasBlockingWork(currentOrderId, null))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(FindNextPendingOrderId(currentOrderId))
                && FindNextWaitingOrderIdFromQueue(currentOrderId, orderGroups) == null)
            {
                return false;
            }

            if (HasOtherExecutingWorkOrder(currentOrderId, orderGroups))
            {
                return false;
            }

            return true;
        }

        private string ExplainCanActivateNextOrderBlock(string currentOrderId, List<SmallDrillWorkOrderTreeNode> orderGroups)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(currentOrderId) && HasBlockingWork(currentOrderId, null))
            {
                var currentNode = orderGroups?.FirstOrDefault(n => OrderIdEquals(n.OrderId, currentOrderId));
                int pending = currentNode?.PendingTrays ?? -1;
                return string.Format("当前工单仍有未结案单头(STATE 0~2)，待处理={0}", pending);
            }

            if (string.IsNullOrWhiteSpace(FindNextPendingOrderId(currentOrderId))
                && FindNextWaitingOrderIdFromQueue(currentOrderId, orderGroups) == null)
            {
                return "等待队列中没有可开启的下一工单";
            }

            string blockingOrderId = FindOtherExecutingWorkOrderId(currentOrderId, orderGroups);
            if (!string.IsNullOrWhiteSpace(blockingOrderId))
            {
                return string.Format("系统认为工单 {0} 仍在执行（在途单头或堆垛机任务）", blockingOrderId);
            }

            return "未知原因";
        }

        private static SmallDrillWorkOrderTreeNode FindNextWaitingOrderIdFromQueue(
            string currentOrderId,
            List<SmallDrillWorkOrderTreeNode> orderGroups)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            if (orderGroups == null || orderGroups.Count == 0)
            {
                return null;
            }

            return orderGroups
                .Where(n => string.Equals(n.QueueStatus, "waiting", StringComparison.OrdinalIgnoreCase))
                .Where(n => !OrderIdEquals(n.OrderId, currentOrderId))
                .OrderBy(n => n.CreateTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// 是否存在其它工单正在执行（单头在途/待装盘，或 NORMAL 任务 STATE≥1）。
        /// 等待队列（仅 PRE_FETCH、单头 STATE=0）不算执行中。
        /// </summary>
        private bool HasOtherExecutingWorkOrder(string currentOrderId, List<SmallDrillWorkOrderTreeNode> orderGroups = null)
        {
            return !string.IsNullOrWhiteSpace(FindOtherExecutingWorkOrderId(currentOrderId, orderGroups));
        }

        /// <summary>
        /// 返回正在阻塞「开启下一工单」的其它工单号；无则 null。
        /// 已完成/异常队列中的残留 NORMAL 任务不再视为执行中（除非单头仍在 STATE 1~2）。
        /// </summary>
        private string FindOtherExecutingWorkOrderId(string currentOrderId, List<SmallDrillWorkOrderTreeNode> orderGroups = null)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            var normalTasks = _taskService
                .Query(t =>
                    t.TYPE == 2
                    && t.ENDTAXI == SmallDrillTarget
                    && t.STATE >= 0
                    && t.STATE <= 2
                    && t.ORDERINFO != null
                    && t.ORDERINFO != "")
                .Where(t => IsNormalTaskMode(t))
                .ToList();

            if (orderGroups != null && orderGroups.Count > 0)
            {
                foreach (var node in orderGroups)
                {
                    if (OrderIdEquals(node.OrderId, currentOrderId))
                    {
                        continue;
                    }

                    if (string.Equals(node.QueueStatus, "waiting", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (IsTerminalQueueStatus(node.QueueStatus))
                    {
                        if (HasPutoutHeaderInProgress(node.OrderId))
                        {
                            return node.OrderId;
                        }

                        continue;
                    }

                    if (string.Equals(node.QueueStatus, "active", StringComparison.OrdinalIgnoreCase))
                    {
                        if (HasPutoutHeaderInProgress(node.OrderId))
                        {
                            return node.OrderId;
                        }

                        if (HasNormalTaskInProgressForOrder(node.OrderId, normalTasks))
                        {
                            return node.OrderId;
                        }
                    }
                }

                foreach (string orderId in normalTasks
                    .Where(t => t.STATE >= 1)
                    .Select(t => ExtractOrderIdFromOrderInfo(t.ORDERINFO))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (OrderIdEquals(orderId, currentOrderId))
                    {
                        continue;
                    }

                    if (orderGroups.Any(n => OrderIdEquals(n.OrderId, orderId)))
                    {
                        continue;
                    }

                    if (HasPutoutHeaderInProgress(orderId))
                    {
                        return orderId;
                    }
                }

                return null;
            }

            foreach (string orderId in normalTasks
                .Where(t => t.STATE >= 1)
                .Select(t => ExtractOrderIdFromOrderInfo(t.ORDERINFO))
                .Where(id => !string.IsNullOrWhiteSpace(id) && !OrderIdEquals(id, currentOrderId))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (HasPutoutHeaderInProgress(orderId))
                {
                    return orderId;
                }

                if (HasNormalTaskInProgressForOrder(orderId, normalTasks))
                {
                    return orderId;
                }
            }

            return null;
        }

        private static bool IsTerminalQueueStatus(string queueStatus)
        {
            return string.Equals(queueStatus, "completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(queueStatus, "abnormal", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasNormalTaskInProgressForOrder(string orderId, List<wms_stocker_task> normalTasks)
        {
            return normalTasks.Any(t =>
                t.STATE >= 1
                && OrderIdEquals(ExtractOrderIdFromOrderInfo(t.ORDERINFO), orderId));
        }

        private static bool IsWaitingQueueOrder(string orderId, List<SmallDrillWorkOrderTreeNode> orderGroups)
        {
            if (orderGroups == null || orderGroups.Count == 0)
            {
                return false;
            }

            return orderGroups.Any(n =>
                OrderIdEquals(n.OrderId, orderId)
                && string.Equals(n.QueueStatus, "waiting", StringComparison.OrdinalIgnoreCase));
        }

        private bool HasPutoutHeaderInProgress(string orderId)
        {
            return _putoutHeaderService.Query(h =>
                    h.ORDERID == orderId
                    && h.REMARK != null
                    && h.REMARK.Contains(SmallDrillRemark)
                    && h.TARGET == SmallDrillTarget
                    && (h.STATE == 1 || h.STATE == 2))
                .Any();
        }

        private bool HasOtherActiveWorkOrder(string currentOrderId)
        {
            return HasOtherExecutingWorkOrder(currentOrderId);
        }

        private List<SmallDrillWorkOrderTreeNode> BuildWorkOrderGroupList(int historyDays)
        {
            if (historyDays <= 0)
            {
                historyDays = 7;
            }

            DateTime historyFrom = DateTime.Now.AddDays(-historyDays);
            var headers = QuerySmallDrillHeaders(h => h.CREATETIME >= historyFrom);
            var tasks = QuerySmallDrillTasks();
            return headers
                .GroupBy(h => h.ORDERID.Trim())
                .Select(g => BuildWorkOrderNode(g.Key, g.ToList(), tasks))
                .OrderBy(n => n.CreateTime)
                .ToList();
        }

        private string ResolveNextWaitingOrderIdFallback(string currentOrderId)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            var orderGroups = BuildWorkOrderGroupList(7);
            return FindNextWaitingOrderIdFromQueue(currentOrderId, orderGroups)?.OrderId;
        }

        private string FindNextPendingOrderId(string currentOrderId)
        {
            currentOrderId = (currentOrderId ?? string.Empty).Trim();
            var pendingTasks = _taskService
                .Query(t =>
                    t.TYPE == 2
                    && t.ENDTAXI == SmallDrillTarget
                    && t.STATE == 0
                    && t.ORDERINFO != null
                    && t.ORDERINFO != "")
                .Where(t => IsPreFetchTaskMode(t) && t.SKUSEQ == 1)
                .ToList();

            return pendingTasks
                .Select(t => new { Task = t, OrderId = ExtractOrderIdFromOrderInfo(t.ORDERINFO) })
                .Where(x => !string.IsNullOrWhiteSpace(x.OrderId)
                    && !string.Equals(x.OrderId, currentOrderId, StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.OrderId)
                .OrderBy(g => g.Min(x => x.Task.CREATETIME))
                .Select(g => g.Key)
                .FirstOrDefault();
        }

        private int ActivatePendingTasksToNormal(string orderId, int skuSeq, string editor)
        {
            orderId = (orderId ?? string.Empty).Trim();
            var tasks = QueryPendingPrefetchTasksForOrder(orderId, skuSeq);

            foreach (var task in tasks)
            {
                task.TASKMODE = "NORMAL";
                task.LASTEDITOR = editor;
                task.LASTEDITTIME = DateTime.Now;
                _taskService.Update(task);
            }

            return tasks.Count;
        }

        private List<wms_stocker_task> QueryPendingPrefetchTasksForOrder(string orderId, int skuSeq)
        {
            orderId = (orderId ?? string.Empty).Trim();
            return _taskService
                .Query(t =>
                    t.TYPE == 2
                    && t.ENDTAXI == SmallDrillTarget
                    && t.SKUSEQ == skuSeq
                    && t.STATE == 0
                    && t.ORDERINFO != null
                    && t.ORDERINFO != "")
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

        private static bool IsNormalTaskMode(wms_stocker_task task)
        {
            return string.Equals((task.TASKMODE ?? string.Empty).Trim(), "NORMAL", StringComparison.OrdinalIgnoreCase);
        }

        private bool ReleaseLocationForHeader(wms_bill_putoutheader header, string editor)
        {
            if (header == null || string.IsNullOrWhiteSpace(header.LOCATIONCODE))
            {
                return false;
            }

            string zoneCode = string.IsNullOrWhiteSpace(header.ZONECODE) ? "101" : header.ZONECODE.Trim();
            string locationCode = header.LOCATIONCODE.Trim();
            string container = (header.CONTAINER ?? string.Empty).Trim();

            var location = _locationService
                .Query(t =>
                    t.ZONECODE == zoneCode
                    && t.LOCATIONCODE == locationCode
                    && (string.IsNullOrEmpty(container) || t.CONTAINER == container))
                .FirstOrDefault();

            if (location == null || location.OUTENABLED != 0)
            {
                return false;
            }

            location.OUTENABLED = 1;
            location.LASTEDITOR = editor;
            location.LASTEDITTIME = DateTime.Now;
            _locationService.Update(location);
            return true;
        }

        #endregion

        #region 磨次选库（与 putoutbound 一致）

        private DataTable SelectAvailableOutboundByGrindFallback(string requestedSourceOrder, int quantity)
        {
            requestedSourceOrder = requestedSourceOrder?.Trim() ?? string.Empty;
            if (quantity <= 0)
            {
                return EmptyLocationTable();
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
                return _db.Db.Ado.GetDataTable(sql, new List<SugarParameter>
                {
                    new SugarParameter("@req", requestedSourceOrder),
                    new SugarParameter("@targetMc", targetMc)
                });
            }

            string exactSql = $@"
SELECT LOCATIONCODE, CONTAINER, SOURCEORDER, TUNNELNUM
FROM wms_base_location
WHERE SOURCEORDER = @SOURCEORDER
  AND ENABLED = 1
  AND OUTENABLED = 1
  AND CONTAINER != ''
  AND ISAVIALABLE = 0
ORDER BY LASTEDITTIME ASC, LOCATIONCODE ASC
LIMIT {quantity}";
            return _db.Db.Ado.GetDataTable(exactSql, new SugarParameter("@SOURCEORDER", requestedSourceOrder));
        }

        private static DataTable EmptyLocationTable()
        {
            return new DataTable();
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

        #endregion

        private void NotifyPrepareMachineQrCodeUpdate(
            string abnormalContainer,
            string newContainer,
            string actualSourceOrder,
            string editor,
            SmallDrillWorkOrderActionResult result)
        {
            string qrContent = QrCodeUpdateService.ResolveSwapContent(
                abnormalContainer,
                actualSourceOrder,
                LookupPutinBindingContent);

            var qrUpdate = QrCodeUpdateService.UpdateQRCodeList(
                new[] { newContainer },
                new[] { qrContent });

            OperationLogWriter.WriteQrCodeUpdate(
                qrUpdate.Success ? "备刀机二维码更新-成功" : "备刀机二维码更新-失败",
                qrUpdate.Success,
                qrUpdate.RequestJson,
                qrUpdate.ResponseJson,
                qrUpdate.ErrorMessage,
                qrUpdate.DurationMs,
                editor);

            if (qrUpdate.Success)
            {
                result.Message += string.Format("；已同步备刀机二维码绑定（Content={0}）", qrContent);
                return;
            }

            result.Message += "；备刀机二维码更新失败：" + (qrUpdate.ErrorMessage ?? "未知错误");
        }

        private string LookupPutinBindingContent(string container)
        {
            container = (container ?? string.Empty).Trim();
            if (container.Length == 0)
            {
                return string.Empty;
            }

            var table = _db.Db.Ado.GetDataTable(@"
SELECT b.ITEMCODE
FROM wms_bill_putinbindingheader a
INNER JOIN wms_bill_putinbindingbody b ON a.ID = b.HEADERID
WHERE a.CONTAINER = @CONTAINER
  AND a.STATE = 2
ORDER BY a.CREATETIME DESC
LIMIT 1",
                new SugarParameter("@CONTAINER", container));

            if (table == null || table.Rows.Count == 0)
            {
                return string.Empty;
            }

            return table.Rows[0][0]?.ToString()?.Trim() ?? string.Empty;
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

        private static string MapQueueStatusText(string queueStatus)
        {
            switch (queueStatus)
            {
                case "active": return "进行中";
                case "waiting": return "等待中";
                case "completed": return "已完成";
                case "abnormal": return "异常结案";
                default: return queueStatus ?? string.Empty;
            }
        }

        private static string MapStateText(int state)
        {
            switch (state)
            {
                case 0: return "待出库";
                case 1: return "在途";
                case 2: return "待装盘";
                case 3: return "已装盘";
                case 5: return "异常";
                default: return "未知(" + state + ")";
            }
        }
    }
}
