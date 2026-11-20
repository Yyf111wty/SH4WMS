-- =============================================================================
-- 4巷道入库错位 — 预览分析（只读，不修改数据）
-- 时间窗：2026-08-12 12:22:32 ~ 2026-08-13 19:00:00
-- 出库口：1002
-- =============================================================================

SET @TUNNEL_NUM     := 4;
SET @ZONE_CODE      := '101';
SET @OUT_TARGET     := '1002';
SET @TIME_START     := '2026-08-12 12:22:32';
SET @TIME_END       := '2026-08-13 19:00:00';

-- -----------------------------------------------------------------------------
-- 1. 基础统计
-- -----------------------------------------------------------------------------
SELECT '4巷道当前有货库位总数' AS metric,
       COUNT(*) AS cnt
FROM wms_base_location
WHERE TUNNELNUM = @TUNNEL_NUM
  AND ENABLED = 1
  AND ISAVIALABLE = 0
  AND CONTAINER <> '';

SELECT '时间窗内入库完成(putinbinding STATE=2)条数' AS metric,
       COUNT(*) AS cnt
FROM wms_bill_putinbindingheader pin
INNER JOIN wms_base_location loc
        ON loc.LOCATIONCODE = pin.LOCATIONCODE
       AND loc.CONTAINER = pin.CONTAINER
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND pin.STATE = 2
  AND pin.QUESTFLAG = 1
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END;

-- -----------------------------------------------------------------------------
-- 2. 待生成出库任务的候选清单（与 02_generate 筛选条件一致）
-- -----------------------------------------------------------------------------
SELECT loc.TUNNELNUM,
       loc.ROWNUM,
       loc.LOCATIONCODE,
       loc.CONTAINER,
       loc.SOURCEORDER,
       loc.OUTENABLED,
       loc.LASTEDITTIME AS loc_last_edit,
       pin.FINISHTIME   AS putin_finish_time,
       pin.ID           AS putin_header_id
FROM wms_base_location loc
INNER JOIN wms_bill_putinbindingheader pin
        ON pin.LOCATIONCODE = loc.LOCATIONCODE
       AND pin.CONTAINER = loc.CONTAINER
       AND pin.STATE = 2
       AND pin.QUESTFLAG = 1
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ZONECODE = @ZONE_CODE
  AND loc.ENABLED = 1
  AND loc.TYPE = 'ST'
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> ''
  AND loc.OUTENABLED = 1
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END
  AND NOT EXISTS (
        SELECT 1
        FROM wms_bill_putoutheader poh
        WHERE poh.LOCATIONCODE = loc.LOCATIONCODE
          AND poh.ENABLED = 1
          AND poh.STATE IN (0, 1)
      )
  AND NOT EXISTS (
        SELECT 1
        FROM wms_stocker_task st
        WHERE st.STARTTAXI = loc.LOCATIONCODE
          AND st.TYPE = 2
          AND st.STATE IN (0, 1)
      )
ORDER BY pin.FINISHTIME, loc.LOCATIONCODE;

-- -----------------------------------------------------------------------------
-- 3. 候选数量汇总
-- -----------------------------------------------------------------------------
SELECT '可生成出库任务数' AS metric,
       COUNT(*) AS cnt
FROM wms_base_location loc
INNER JOIN wms_bill_putinbindingheader pin
        ON pin.LOCATIONCODE = loc.LOCATIONCODE
       AND pin.CONTAINER = loc.CONTAINER
       AND pin.STATE = 2
       AND pin.QUESTFLAG = 1
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ZONECODE = @ZONE_CODE
  AND loc.ENABLED = 1
  AND loc.TYPE = 'ST'
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> ''
  AND loc.OUTENABLED = 1
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END
  AND NOT EXISTS (
        SELECT 1
        FROM wms_bill_putoutheader poh
        WHERE poh.LOCATIONCODE = loc.LOCATIONCODE
          AND poh.ENABLED = 1
          AND poh.STATE IN (0, 1)
      )
  AND NOT EXISTS (
        SELECT 1
        FROM wms_stocker_task st
        WHERE st.STARTTAXI = loc.LOCATIONCODE
          AND st.TYPE = 2
          AND st.STATE IN (0, 1)
      );

-- -----------------------------------------------------------------------------
-- 4. 冲突/跳过项（需人工处理后再生成）
-- -----------------------------------------------------------------------------
SELECT 'OUTENABLED=0 已有待出库锁定' AS skip_reason,
       COUNT(*) AS cnt
FROM wms_base_location loc
INNER JOIN wms_bill_putinbindingheader pin
        ON pin.LOCATIONCODE = loc.LOCATIONCODE
       AND pin.CONTAINER = loc.CONTAINER
       AND pin.STATE = 2
       AND pin.QUESTFLAG = 1
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> ''
  AND loc.OUTENABLED = 0
  AND pin.FINISHTIME >= @TIME_START
  AND pin.FINISHTIME <= @TIME_END;

SELECT '已有未完成出库单' AS skip_reason,
       COUNT(DISTINCT loc.LOCATIONCODE) AS cnt
FROM wms_base_location loc
INNER JOIN wms_bill_putoutheader poh
        ON poh.LOCATIONCODE = loc.LOCATIONCODE
       AND poh.ENABLED = 1
       AND poh.STATE IN (0, 1)
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> '';

SELECT '已有未完成堆垛机出库任务(TYPE=2)' AS skip_reason,
       COUNT(DISTINCT loc.LOCATIONCODE) AS cnt
FROM wms_base_location loc
INNER JOIN wms_stocker_task st
        ON st.STARTTAXI = loc.LOCATIONCODE
       AND st.TYPE = 2
       AND st.STATE IN (0, 1)
WHERE loc.TUNNELNUM = @TUNNEL_NUM
  AND loc.ISAVIALABLE = 0
  AND loc.CONTAINER <> '';

-- -----------------------------------------------------------------------------
-- 5. 4巷道在途堆垛机任务（生成前建议清零或等待完成）
-- -----------------------------------------------------------------------------
SELECT st.ID,
       st.TYPE,
       st.STATE,
       st.CONTAINER,
       st.STARTTAXI,
       st.ENDTAXI,
       st.STARTTIME
FROM wms_stocker_task st
WHERE st.STATE IN (0, 1)
  AND (
        st.STARTTAXI IN (SELECT LOCATIONCODE FROM wms_base_location WHERE TUNNELNUM = @TUNNEL_NUM)
     OR st.ENDTAXI   IN (SELECT LOCATIONCODE FROM wms_base_location WHERE TUNNELNUM = @TUNNEL_NUM)
      )
ORDER BY st.STARTTIME;

-- -----------------------------------------------------------------------------
-- 6. 可选：入库完成时序偏移检测（物理为「下一盒」时，DB 盒号 ≈ 上一条入库盒号）
-- -----------------------------------------------------------------------------
SELECT cur.LOCATIONCODE,
       cur.CONTAINER          AS db_container,
       cur.SOURCEORDER        AS db_sourceorder,
       cur.FINISHTIME,
       prev.CONTAINER         AS prev_putin_container,
       nxt.CONTAINER          AS next_putin_container,
       CASE
         WHEN prev.CONTAINER IS NOT NULL AND cur.CONTAINER = prev.CONTAINER THEN 'SAME_AS_PREV'
         WHEN nxt.CONTAINER  IS NOT NULL AND cur.CONTAINER = nxt.CONTAINER  THEN 'SAME_AS_NEXT'
         ELSE 'NO_SIMPLE_SHIFT'
       END AS shift_hint
FROM (
    SELECT pin.LOCATIONCODE,
           pin.CONTAINER,
           pin.SOURCEORDER,
           pin.FINISHTIME,
           ROW_NUMBER() OVER (ORDER BY pin.FINISHTIME, pin.LOCATIONCODE) AS rn
    FROM wms_bill_putinbindingheader pin
    INNER JOIN wms_base_location loc
            ON loc.LOCATIONCODE = pin.LOCATIONCODE
           AND loc.CONTAINER = pin.CONTAINER
    WHERE loc.TUNNELNUM = @TUNNEL_NUM
      AND pin.STATE = 2
      AND pin.QUESTFLAG = 1
      AND pin.FINISHTIME >= @TIME_START
      AND pin.FINISHTIME <= @TIME_END
) cur
LEFT JOIN (
    SELECT pin.CONTAINER,
           ROW_NUMBER() OVER (ORDER BY pin.FINISHTIME, pin.LOCATIONCODE) AS rn
    FROM wms_bill_putinbindingheader pin
    INNER JOIN wms_base_location loc
            ON loc.LOCATIONCODE = pin.LOCATIONCODE
           AND loc.CONTAINER = pin.CONTAINER
    WHERE loc.TUNNELNUM = @TUNNEL_NUM
      AND pin.STATE = 2
      AND pin.QUESTFLAG = 1
      AND pin.FINISHTIME >= @TIME_START
      AND pin.FINISHTIME <= @TIME_END
) prev ON prev.rn = cur.rn - 1
LEFT JOIN (
    SELECT pin.CONTAINER,
           ROW_NUMBER() OVER (ORDER BY pin.FINISHTIME, pin.LOCATIONCODE) AS rn
    FROM wms_bill_putinbindingheader pin
    INNER JOIN wms_base_location loc
            ON loc.LOCATIONCODE = pin.LOCATIONCODE
           AND loc.CONTAINER = pin.CONTAINER
    WHERE loc.TUNNELNUM = @TUNNEL_NUM
      AND pin.STATE = 2
      AND pin.QUESTFLAG = 1
      AND pin.FINISHTIME >= @TIME_START
      AND pin.FINISHTIME <= @TIME_END
) nxt ON nxt.rn = cur.rn + 1
ORDER BY cur.FINISHTIME, cur.LOCATIONCODE
LIMIT 200;
