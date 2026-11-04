/*
  小钻工单调度 - MySQL 版（Navicat / DBeaver 等请「运行当前语句」或允许多语句）
  组盘出库：REMARK LIKE '%组盘出库%'，TARGET/ENDTAXI = '1002'，任务 TYPE=2

  工单号请改下面三行 SET（不要用 SQL Server 的 DECLARE @xxx）
*/

SET @CurrentOrderId   = '20261101031107530';
SET @WaitingOrderId   = '20261101073746688';
SET @BlockingOrderId  = '20261030174015941';   /* 页面提示「仍在执行」的工单，按需改 */
SET @SmallDrillTarget = '1002';

/* ORDERINFO 解析工单号（与程序 ExtractOrderIdFromOrderInfo 一致：第一个 '-' 前） */
/* SUBSTRING_INDEX(t.ORDERINFO, '-', 1) */

-- =============================================================================
-- 一、单头 STATE 分布（0~2=待处理，3=已装，5=异常）
-- =============================================================================
SELECT
    h.ORDERID,
    h.SKUSEQ,
    h.STATE,
    COUNT(*) AS Cnt,
    SUM(CASE WHEN h.STATE BETWEEN 0 AND 2 THEN 1 ELSE 0 END) AS PendingLike,
    SUM(CASE WHEN h.STATE = 3 THEN 1 ELSE 0 END) AS DoneCnt,
    SUM(CASE WHEN h.STATE = 5 THEN 1 ELSE 0 END) AS AbnormalCnt
FROM wms_bill_putoutheader h
WHERE h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
  AND h.ORDERID IN (@CurrentOrderId, @WaitingOrderId)
GROUP BY h.ORDERID, h.SKUSEQ, h.STATE
ORDER BY h.ORDERID, h.SKUSEQ, h.STATE;

-- 当前工单是否还有 STATE 0~2（挡住整单取消 / 部分逻辑）
SELECT h.ID, h.SKUSEQ, h.STATE, h.CONTAINER, h.REMARK
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
  AND h.STATE BETWEEN 0 AND 2;

-- =============================================================================
-- 二、待换料异常盘（不含量「取消」「已换料」「换料补配」→ 工单会卡在进行中）
-- =============================================================================
SELECT h.ID, h.SKUSEQ, h.CONTAINER, h.STATE, h.REMARK
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
  AND h.STATE = 5
  AND h.REMARK NOT LIKE '%取消%'
  AND h.REMARK NOT LIKE '%强制结案%'
  AND h.REMARK NOT LIKE '%已换料%'
  AND h.REMARK NOT LIKE '%换料补配%';

-- =============================================================================
-- 三、【人工强制结案】无库存无法换料时（与页面「强制结案」等效，不改代码时用）
--     先跑上面 SELECT，确认行数后再 START TRANSACTION
-- =============================================================================
/*
START TRANSACTION;

UPDATE wms_bill_putoutheader h
SET h.REMARK = CONCAT(IFNULL(h.REMARK, ''), '|异常:强制结案(无库存无法换料)'),
    h.LASTEDITOR = 'MANUAL_CLOSE',
    h.LASTEDITTIME = NOW()
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
  AND h.STATE = 5
  AND h.REMARK NOT LIKE '%取消%'
  AND h.REMARK NOT LIKE '%强制结案%'
  AND h.REMARK NOT LIKE '%已换料%'
  AND h.REMARK NOT LIKE '%换料补配%';

-- SELECT ROW_COUNT() AS UpdatedHeaders;

UPDATE wms_stocker_task t
SET t.STATE = 5,
    t.LASTEDITOR = 'MANUAL_CLOSE',
    t.LASTEDITTIME = NOW(),
    t.TASKMODE = CASE WHEN UPPER(IFNULL(t.TASKMODE, '')) = 'NORMAL' THEN 'PRE_FETCH' ELSE t.TASKMODE END
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE CONCAT(@CurrentOrderId, '-%')
  AND t.STATE BETWEEN 0 AND 2;

-- SELECT ROW_COUNT() AS UpdatedTasks;
-- ROLLBACK;
COMMIT;
*/

-- =============================================================================
-- 四、单头已 3/5，任务仍 0~2（头任务不一致）
-- =============================================================================
SELECT
    h.ORDERID, h.CONTAINER, h.STATE AS HeaderState,
    t.ID AS TaskId, t.STATE AS TaskState, t.TASKMODE, t.ORDERINFO
FROM wms_bill_putoutheader h
INNER JOIN wms_stocker_task t
    ON t.TYPE = 2
   AND t.ENDTAXI = @SmallDrillTarget
   AND t.CONTAINER = h.CONTAINER
   AND t.SKUSEQ = h.SKUSEQ
   AND t.ORDERINFO LIKE CONCAT(h.ORDERID, '-%')
WHERE h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
  AND h.ORDERID = @CurrentOrderId
  AND h.STATE IN (3, 5)
  AND t.STATE BETWEEN 0 AND 2;

-- =============================================================================
-- 五、挡「开启下一工单」：其它工单 NORMAL 任务 1~2，但无单头 1~2
-- =============================================================================
SELECT
    h.ORDERID,
    SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) AS HeaderInProgress,
    t.StaleNormalTaskCnt
FROM wms_bill_putoutheader h
INNER JOIN (
    SELECT
        SUBSTRING_INDEX(t.ORDERINFO, '-', 1) AS ORDERID,
        COUNT(*) AS StaleNormalTaskCnt
    FROM wms_stocker_task t
    WHERE t.TYPE = 2
      AND t.ENDTAXI = @SmallDrillTarget
      AND UPPER(IFNULL(t.TASKMODE, '')) = 'NORMAL'
      AND t.STATE BETWEEN 1 AND 2
      AND t.ORDERINFO IS NOT NULL
      AND t.ORDERINFO <> ''
    GROUP BY SUBSTRING_INDEX(t.ORDERINFO, '-', 1)
) t ON t.ORDERID = h.ORDERID
WHERE h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE '%组盘出库%'
GROUP BY h.ORDERID, t.StaleNormalTaskCnt
HAVING SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) = 0
   AND t.StaleNormalTaskCnt > 0
ORDER BY h.ORDERID DESC;

-- 清理指定「幽灵工单」残留 NORMAL 任务（先 SELECT 确认 ORDERINFO 归属）
/*
START TRANSACTION;

UPDATE wms_stocker_task t
SET t.STATE = 3,
    t.LASTEDITOR = 'MANUAL_CLOSE',
    t.LASTEDITTIME = NOW()
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND UPPER(IFNULL(t.TASKMODE, '')) = 'NORMAL'
  AND t.STATE BETWEEN 1 AND 2
  AND t.ORDERINFO LIKE CONCAT(@BlockingOrderId, '-%');

-- ROLLBACK;
COMMIT;
*/

-- =============================================================================
-- 六、等待队列能否激活（SKUSEQ=1, STATE=0, PRE_FETCH）
-- =============================================================================
SELECT t.ID, t.STATE, t.TASKMODE, t.ORDERINFO, t.CREATETIME
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE CONCAT(@WaitingOrderId, '-%')
  AND t.SKUSEQ = 1
ORDER BY t.STATE, t.CREATETIME;
