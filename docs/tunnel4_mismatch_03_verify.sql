-- =============================================================================
-- 4巷道入库错位 — 生成后验证 / 回滚辅助
-- =============================================================================

SET @TUNNEL_NUM     := 4;
SET @OUT_TARGET     := '1002';
SET @TIME_START     := '2026-08-12 12:22:32';
SET @TIME_END       := '2026-08-13 19:00:00';
SET @REMARK         := '4巷道错位修复出库';
SET @CREATOR        := 'WMS_FIX';

-- -----------------------------------------------------------------------------
-- 1. 本次修复出库单统计
-- -----------------------------------------------------------------------------
SELECT poh.STATE,
       COUNT(*) AS cnt
FROM wms_bill_putoutheader poh
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = poh.LOCATIONCODE
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND poh.TARGET = @OUT_TARGET
  AND poh.REMARK = @REMARK
  AND poh.CREATOR = @CREATOR
GROUP BY poh.STATE
ORDER BY poh.STATE;

-- -----------------------------------------------------------------------------
-- 2. 堆垛机任务统计
-- -----------------------------------------------------------------------------
SELECT st.STATE,
       COUNT(*) AS cnt
FROM wms_stocker_task st
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = st.STARTTAXI
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND st.TYPE = 2
  AND st.ENDTAXI = @OUT_TARGET
  AND st.CREATOR = @CREATOR
  AND st.ORDERINFO LIKE 'T4_FIX|%'
GROUP BY st.STATE
ORDER BY st.STATE;

-- -----------------------------------------------------------------------------
-- 3. 4巷道仍待出库(OUTENABLED=0)库位
-- -----------------------------------------------------------------------------
SELECT COUNT(*) AS locked_location_cnt
FROM wms_base_location
WHERE TUNNELNUM = @TUNNEL_NUM
  AND ISAVIALABLE = 0
  AND CONTAINER <> ''
  AND OUTENABLED = 0;

-- -----------------------------------------------------------------------------
-- 4. 明细清单（便于 WCS/现场核对）
-- -----------------------------------------------------------------------------
SELECT st.ID              AS task_id,
       st.STATE           AS task_state,
       st.STARTTAXI       AS location_code,
       st.CONTAINER,
       st.SOURCEORDER,
       st.ENDTAXI         AS out_port,
       st.CREATETIME,
       poh.ID             AS putout_header_id,
       poh.STATE          AS putout_state
FROM wms_stocker_task st
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = st.STARTTAXI
LEFT JOIN wms_bill_putoutheader poh
       ON poh.LOCATIONCODE = st.STARTTAXI
      AND poh.CONTAINER = st.CONTAINER
      AND poh.TARGET = st.ENDTAXI
      AND poh.REMARK = @REMARK
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND st.TYPE = 2
  AND st.ENDTAXI = @OUT_TARGET
  AND st.ORDERINFO LIKE 'T4_FIX|%'
ORDER BY st.CREATETIME, st.STARTTAXI;

-- -----------------------------------------------------------------------------
-- 5. 出库完成后检查（WCS 回传 completeOutKu 后执行）
--    期望：4巷道时间窗内库位已清空，出库单 STATE=2，inventorybook 已删
-- -----------------------------------------------------------------------------
SELECT '时间窗内库位仍占用' AS check_item,
       COUNT(*) AS cnt
FROM wms_base_location loc
INNER JOIN wms_bill_putinbindingheader pin
        ON pin.LOCATIONCODE = loc.LOCATIONCODE
       AND pin.CONTAINER = loc.CONTAINER
       AND pin.STATE = 2
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> ''
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END;

SELECT '未完成修复出库单' AS check_item,
       COUNT(*) AS cnt
FROM wms_bill_putoutheader poh
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = poh.LOCATIONCODE
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND poh.REMARK = @REMARK
  AND poh.CREATOR = @CREATOR
  AND poh.STATE IN (0, 1);

-- -----------------------------------------------------------------------------
-- 6. 紧急回滚（仅适用于尚未 COMMIT 的 02 脚本同事务内，或手动删除误生成数据）
--    注意：若堆垛机已开始执行，请勿回滚，应走正常出库完成流程
-- -----------------------------------------------------------------------------
/*
START TRANSACTION;

DELETE st
FROM wms_stocker_task st
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = st.STARTTAXI
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND st.TYPE = 2
  AND st.ENDTAXI = @OUT_TARGET
  AND st.ORDERINFO LIKE 'T4_FIX|%'
  AND st.STATE IN (0, 1);

DELETE pob
FROM wms_bill_putoutbody pob
INNER JOIN wms_bill_putoutheader poh ON poh.ID = pob.HEADERID
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = poh.LOCATIONCODE
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND poh.REMARK = @REMARK
  AND poh.CREATOR = @CREATOR
  AND poh.STATE IN (0, 1);

DELETE poh
FROM wms_bill_putoutheader poh
INNER JOIN wms_base_location loc ON loc.LOCATIONCODE = poh.LOCATIONCODE
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND poh.REMARK = @REMARK
  AND poh.CREATOR = @CREATOR
  AND poh.STATE IN (0, 1);

UPDATE wms_base_location loc
INNER JOIN wms_bill_putinbindingheader pin
        ON pin.LOCATIONCODE = loc.LOCATIONCODE
       AND pin.CONTAINER = loc.CONTAINER
       AND pin.STATE = 2
SET loc.OUTENABLED = 1,
    loc.LASTEDITOR = @CREATOR,
    loc.LASTEDITTIME = NOW()
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> ''
  AND loc.OUTENABLED = 0
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END;

COMMIT;
*/
