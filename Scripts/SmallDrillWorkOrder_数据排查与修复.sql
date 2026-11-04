/*
  小钻工单调度 - 数据排查 / 脏数据修复（与 SmallDrillWorkOrderService 逻辑对齐）
  范围：组盘出库 REMARK 含「组盘出库」、TARGET/ENDTAXI = 1002、任务 TYPE=2

  使用前：
  1. 在测试库或先只跑 SELECT 段确认结果
  2. 修改下方 @CurrentOrderId / @WaitingOrderId
  3. 修复段务必 BEGIN TRAN … ROLLBACK 试跑，确认无误再 COMMIT
  4. 数据库为 MySQL 请用同目录：SmallDrillWorkOrder_数据排查与修复_MySQL.sql
     （勿在本文件使用 DECLARE @变量 / GETDATE / N' 前缀）
*/

SET NOCOUNT ON;

DECLARE @CurrentOrderId  VARCHAR(50) = '20261027092518973';
DECLARE @WaitingOrderId  VARCHAR(50) = '20261028125120341';
DECLARE @SmallDrillTarget VARCHAR(10) = '1002';

/* ORDERINFO 首段 = 工单号（与 ExtractOrderIdFromOrderInfo 一致） */
-- SQL Server 表达式（子查询/CTE 中复用）:
-- OrderIdFromInfo = LEFT(t.ORDERINFO, NULLIF(CHARINDEX('-', t.ORDERINFO), 0) - 1)  -- CHARINDEX=0 时需单独处理

/*======================================================================
  一、当前工单 / 等待工单 总览（单头 STATE 分布）
  STATE: 0待出库 1堆垛机执行 2到站台待装盘 3已完成 5异常
======================================================================*/
SELECT
    h.ORDERID,
    h.SKUSEQ,
    h.STATE,
    COUNT(*) AS Cnt,
    SUM(CASE WHEN h.STATE BETWEEN 0 AND 2 THEN 1 ELSE 0 END) AS PendingLike,  /* 界面「待处理」同源 */
    SUM(CASE WHEN h.STATE = 3 THEN 1 ELSE 0 END) AS DoneCnt,
    SUM(CASE WHEN h.STATE = 5 THEN 1 ELSE 0 END) AS AbnormalCnt
FROM wms_bill_putoutheader h
WHERE h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.ORDERID IN (@CurrentOrderId, @WaitingOrderId)
GROUP BY h.ORDERID, h.SKUSEQ, h.STATE
ORDER BY h.ORDERID, h.SKUSEQ, h.STATE;

/* 是否挡住「开启下一工单」：当前工单仍有单头 STATE 0~2（HasBlockingWork） */
SELECT
    h.ORDERID,
    COUNT(*) AS BlockingHeaderCnt,
    MIN(h.STATE) AS MinState,
    MAX(h.STATE) AS MaxState
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.STATE BETWEEN 0 AND 2
GROUP BY h.ORDERID;

/* 挡住「开启下一工单」的明细（当前工单未结案单头） */
SELECT
    h.ID, h.ORDERID, h.SKUSEQ, h.STATE, h.CONTAINER, h.LOCATIONCODE,
    h.SOURCEORDER, h.REMARK, h.CREATETIME, h.LASTEDITTIME
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.STATE BETWEEN 0 AND 2
ORDER BY h.SKUSEQ, h.STATE, h.CONTAINER;

/*======================================================================
  二、任务与单头不一致（常见脏数据）
======================================================================*/

/* 2.1 单头已结案(3/5)，任务仍为 0~2（PRE_FETCH/NORMAL 残留 → 易误判其它工单在执行） */
SELECT
    h.ORDERID,
    h.SKUSEQ,
    h.CONTAINER,
    h.STATE AS HeaderState,
    h.REMARK AS HeaderRemark,
    t.ID AS TaskId,
    t.STATE AS TaskState,
    t.TASKMODE,
    t.ORDERINFO,
    t.LASTEDITTIME AS TaskLastEdit
FROM wms_bill_putoutheader h
INNER JOIN wms_stocker_task t
    ON t.TYPE = 2
   AND t.ENDTAXI = @SmallDrillTarget
   AND t.CONTAINER = h.CONTAINER
   AND t.SKUSEQ = h.SKUSEQ
   AND t.ORDERINFO IS NOT NULL
   AND t.ORDERINFO LIKE h.ORDERID + N'-%'
WHERE h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.ORDERID IN (@CurrentOrderId, @WaitingOrderId)
  AND h.STATE IN (3, 5)
  AND t.STATE BETWEEN 0 AND 2
ORDER BY h.ORDERID, h.SKUSEQ, h.CONTAINER;

/* 2.2 全库：历史工单 NORMAL 任务 STATE>=1，但同单号已无单头 STATE 1~2（旧逻辑会挡「下一工单」） */
;WITH H AS (
    SELECT
        h.ORDERID,
        SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) AS HeaderInProgress,
        SUM(CASE WHEN h.STATE BETWEEN 0 AND 2 THEN 1 ELSE 0 END) AS HeaderPendingOrProgress,
        SUM(CASE WHEN h.STATE = 3 THEN 1 ELSE 0 END) AS DoneCnt,
        SUM(CASE WHEN h.STATE = 5 THEN 1 ELSE 0 END) AS AbnormalCnt,
        MAX(h.CREATETIME) AS LastCreate
    FROM wms_bill_putoutheader h
    WHERE h.TARGET = @SmallDrillTarget
      AND h.REMARK LIKE N'%组盘出库%'
    GROUP BY h.ORDERID
),
T AS (
    SELECT
        CASE
            WHEN CHARINDEX('-', t.ORDERINFO) > 0
            THEN LEFT(t.ORDERINFO, CHARINDEX('-', t.ORDERINFO) - 1)
            ELSE t.ORDERINFO
        END AS ORDERID,
        COUNT(*) AS StaleNormalTaskCnt
    FROM wms_stocker_task t
    WHERE t.TYPE = 2
      AND t.ENDTAXI = @SmallDrillTarget
      AND UPPER(ISNULL(t.TASKMODE, '')) = 'NORMAL'
      AND t.STATE BETWEEN 1 AND 2
      AND t.ORDERINFO IS NOT NULL
      AND t.ORDERINFO <> ''
    GROUP BY
        CASE
            WHEN CHARINDEX('-', t.ORDERINFO) > 0
            THEN LEFT(t.ORDERINFO, CHARINDEX('-', t.ORDERINFO) - 1)
            ELSE t.ORDERINFO
        END
)
SELECT
    H.ORDERID,
    H.HeaderInProgress,
    H.HeaderPendingOrProgress,
    H.DoneCnt,
    H.AbnormalCnt,
    T.StaleNormalTaskCnt,
    H.LastCreate
FROM H
INNER JOIN T ON T.ORDERID = H.ORDERID
WHERE H.ORDERID <> @CurrentOrderId
  AND H.HeaderInProgress = 0          /* 无在途/待装盘单头 */
  AND T.StaleNormalTaskCnt > 0
ORDER BY H.LastCreate DESC;

/* 2.3 当前工单：PRE_FETCH 且 STATE=1~2，但单头已不是 0~2 */
SELECT
    t.ID, t.CONTAINER, t.SKUSEQ, t.STATE, t.TASKMODE, t.ORDERINFO,
    h.STATE AS HeaderState, h.REMARK AS HeaderRemark
FROM wms_stocker_task t
LEFT JOIN wms_bill_putoutheader h
    ON h.ORDERID = @CurrentOrderId
   AND h.TARGET = @SmallDrillTarget
   AND h.REMARK LIKE N'%组盘出库%'
   AND h.CONTAINER = t.CONTAINER
   AND h.SKUSEQ = t.SKUSEQ
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE @CurrentOrderId + N'-%'
  AND UPPER(ISNULL(t.TASKMODE, '')) = 'PRE_FETCH'
  AND t.STATE BETWEEN 1 AND 2
  AND (h.ID IS NULL OR h.STATE NOT BETWEEN 0 AND 2)
ORDER BY t.SKUSEQ, t.CONTAINER;

/*======================================================================
  三、等待队列能否被「开启下一工单」激活
  条件：SKUSEQ=1、STATE=0、TASKMODE=PRE_FETCH
======================================================================*/
SELECT
    t.ID, t.CONTAINER, t.SKUSEQ, t.STATE, t.TASKMODE, t.ORDERINFO, t.CREATETIME
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE @WaitingOrderId + N'-%'
  AND t.SKUSEQ = 1
ORDER BY t.STATE, t.CREATETIME;

SELECT
    SUM(CASE WHEN t.STATE = 0 AND UPPER(ISNULL(t.TASKMODE, '')) = 'PRE_FETCH' THEN 1 ELSE 0 END) AS ActivatablePrefetchCnt,
    COUNT(*) AS TotalTaskCnt
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE @WaitingOrderId + N'-%'
  AND t.SKUSEQ = 1;

/* 等待工单单头是否仍全是「排队」态（STATE=0，无 1~2） */
SELECT
    h.ORDERID,
    SUM(CASE WHEN h.STATE = 0 THEN 1 ELSE 0 END) AS State0,
    SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) AS State12,
    SUM(CASE WHEN h.STATE = 3 THEN 1 ELSE 0 END) AS State3,
    SUM(CASE WHEN h.STATE = 5 THEN 1 ELSE 0 END) AS State5
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @WaitingOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
GROUP BY h.ORDERID;

/*======================================================================
  四、规格2 已整单取消但任务未收尾（你现场 M1 典型问题）
======================================================================*/
SELECT
    h.ID, h.CONTAINER, h.SKUSEQ, h.STATE, h.REMARK,
    t.ID AS TaskId, t.STATE AS TaskState, t.TASKMODE
FROM wms_bill_putoutheader h
LEFT JOIN wms_stocker_task t
    ON t.TYPE = 2 AND t.ENDTAXI = @SmallDrillTarget
   AND t.CONTAINER = h.CONTAINER AND t.SKUSEQ = h.SKUSEQ
   AND t.ORDERINFO LIKE h.ORDERID + N'-%'
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.SKUSEQ = 2
  AND (
        (h.STATE = 5 AND h.REMARK LIKE N'%取消%')
        OR (t.ID IS NOT NULL AND t.STATE BETWEEN 0 AND 2)
      )
ORDER BY h.CONTAINER;

/*======================================================================
  五、修复模板（先 SELECT 对应段，再开事务执行）
  建议 LASTEDITOR = 'DATA_FIX' 便于审计
======================================================================*/

/*
-- ---------- 5.1 单头已 3/5，任务仍 0~2：任务改为与单头一致 ----------
BEGIN TRAN;

UPDATE t
SET t.STATE = h.STATE,
    t.LASTEDITOR = N'DATA_FIX',
    t.LASTEDITTIME = GETDATE(),
    t.TASKMODE = CASE
        WHEN h.STATE = 5 AND UPPER(ISNULL(t.TASKMODE, '')) = 'NORMAL' THEN N'PRE_FETCH'
        ELSE t.TASKMODE
    END
FROM wms_stocker_task t
INNER JOIN wms_bill_putoutheader h
    ON h.TARGET = @SmallDrillTarget
   AND h.REMARK LIKE N'%组盘出库%'
   AND h.CONTAINER = t.CONTAINER
   AND h.SKUSEQ = t.SKUSEQ
   AND t.ORDERINFO LIKE h.ORDERID + N'-%'
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND h.STATE IN (3, 5)
  AND t.STATE BETWEEN 0 AND 2
  AND h.ORDERID IN (@CurrentOrderId, @WaitingOrderId);
-- SELECT @@ROWCOUNT AS FixedTaskRows;
-- ROLLBACK TRAN;
-- COMMIT TRAN;
*/

/*
-- ---------- 5.2 历史工单：无单头 1~2，NORMAL 任务仍 1~2 → 置完成 STATE=3 ----------
BEGIN TRAN;

UPDATE t
SET t.STATE = 3,
    t.LASTEDITOR = N'DATA_FIX',
    t.LASTEDITTIME = GETDATE()
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND UPPER(ISNULL(t.TASKMODE, '')) = 'NORMAL'
  AND t.STATE BETWEEN 1 AND 2
  AND t.ORDERINFO IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM wms_bill_putoutheader h
      WHERE h.TARGET = @SmallDrillTarget
        AND h.REMARK LIKE N'%组盘出库%'
        AND h.ORDERID = CASE
              WHEN CHARINDEX('-', t.ORDERINFO) > 0
              THEN LEFT(t.ORDERINFO, CHARINDEX('-', t.ORDERINFO) - 1)
              ELSE t.ORDERINFO
            END
      GROUP BY h.ORDERID
      HAVING SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) = 0
         AND SUM(CASE WHEN h.STATE = 3 THEN 1 ELSE 0 END) > 0
  )
  AND (
      CASE
          WHEN CHARINDEX('-', t.ORDERINFO) > 0
          THEN LEFT(t.ORDERINFO, CHARINDEX('-', t.ORDERINFO) - 1)
          ELSE t.ORDERINFO
      END
  ) <> @CurrentOrderId;

-- SELECT @@ROWCOUNT AS ClosedStaleNormalTasks;
-- ROLLBACK TRAN;
-- COMMIT TRAN;
*/

/*
-- ---------- 5.3 规格2 已异常取消：任务统一 STATE=5（与 MarkTasksAbnormalForTray 一致） ----------
BEGIN TRAN;

UPDATE t
SET t.STATE = 5,
    t.LASTEDITOR = N'DATA_FIX',
    t.LASTEDITTIME = GETDATE(),
    t.TASKMODE = CASE WHEN UPPER(ISNULL(t.TASKMODE, '')) = 'NORMAL' THEN N'PRE_FETCH' ELSE t.TASKMODE END
FROM wms_stocker_task t
INNER JOIN wms_bill_putoutheader h
    ON h.ORDERID = @CurrentOrderId
   AND h.TARGET = @SmallDrillTarget
   AND h.REMARK LIKE N'%组盘出库%'
   AND h.SKUSEQ = 2
   AND h.STATE = 5
   AND h.REMARK LIKE N'%取消%'
   AND h.CONTAINER = t.CONTAINER
   AND t.SKUSEQ = h.SKUSEQ
   AND t.ORDERINFO LIKE @CurrentOrderId + N'-%'
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.STATE BETWEEN 0 AND 2;

-- ROLLBACK TRAN;
-- COMMIT TRAN;
*/

/*
-- ---------- 5.4 【慎用】当前工单仍有个别单头卡在 1~2，但现场确认已无效 → 改为异常 5 ----------
-- 必须按 一、明细 的 ID 逐条确认，不要整单盲改
BEGIN TRAN;

UPDATE wms_bill_putoutheader
SET STATE = 5,
    REMARK = REMARK + N'|异常:数据修复',
    LASTEDITOR = N'DATA_FIX',
    LASTEDITTIME = GETDATE()
WHERE ID IN (
    -- 粘贴要修复的单头 ID
    N'00000000-0000-0000-0000-000000000000'
);

-- 同步任务（同容器+SKUSEQ）
-- 参考 5.1 / 5.3

-- ROLLBACK TRAN;
-- COMMIT TRAN;
*/

/*======================================================================
  六、修复后快速验收（与界面逻辑对照）
======================================================================*/
SELECT N'Current Blocking Headers (should be 0)' AS CheckItem,
       COUNT(*) AS Cnt
FROM wms_bill_putoutheader h
WHERE h.ORDERID = @CurrentOrderId
  AND h.TARGET = @SmallDrillTarget
  AND h.REMARK LIKE N'%组盘出库%'
  AND h.STATE BETWEEN 0 AND 2

UNION ALL

SELECT N'Waiting Activatable PRE_FETCH SKUSEQ=1 STATE=0',
       COUNT(*)
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND t.ORDERINFO LIKE @WaitingOrderId + N'-%'
  AND t.SKUSEQ = 1
  AND t.STATE = 0
  AND UPPER(ISNULL(t.TASKMODE, '')) = 'PRE_FETCH'

UNION ALL

SELECT N'Other orders: stale NORMAL 1~2 without header 1~2',
       COUNT(*)
FROM wms_stocker_task t
WHERE t.TYPE = 2
  AND t.ENDTAXI = @SmallDrillTarget
  AND UPPER(ISNULL(t.TASKMODE, '')) = 'NORMAL'
  AND t.STATE BETWEEN 1 AND 2
  AND EXISTS (
      SELECT 1 FROM wms_bill_putoutheader h
      WHERE h.TARGET = @SmallDrillTarget
        AND h.REMARK LIKE N'%组盘出库%'
        AND h.ORDERID = CASE
              WHEN CHARINDEX('-', t.ORDERINFO) > 0
              THEN LEFT(t.ORDERINFO, CHARINDEX('-', t.ORDERINFO) - 1)
              ELSE t.ORDERINFO END
        AND h.ORDERID <> @CurrentOrderId
      GROUP BY h.ORDERID
      HAVING SUM(CASE WHEN h.STATE BETWEEN 1 AND 2 THEN 1 ELSE 0 END) = 0
  );
