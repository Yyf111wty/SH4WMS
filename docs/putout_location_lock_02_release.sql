-- =============================================================================
-- 出库单未完结/已删除但库位仍被占用 — 释放脚本（会修改数据，执行前请先跑 01_preview）
-- 建议：先在测试库验证；生产环境务必先备份相关表或整库
-- =============================================================================

SET @TIME_START := '2026-08-01 00:00:00';
SET @TIME_END   := '2026-08-25 23:59:59';
SET @ZONE_CODE  := NULL;   -- 指定库区如 '101'；NULL 表示不限
SET @OPERATOR   := 'SQL_FIX';

-- =============================================================================
-- A. 释放预览：将要被改为 OUTENABLED=1 的库位（与下方 UPDATE 条件一致）
-- =============================================================================
SELECT loc.ZONECODE,
       loc.LOCATIONCODE,
       loc.CONTAINER,
       loc.SOURCEORDER,
       loc.OUTENABLED      AS outenabled_before,
       loc.LASTEDITTIME    AS loc_last_edit_before,
       poh.ID              AS putout_header_id,
       poh.STATE           AS putout_state,
       poh.CREATETIME      AS putout_create_time,
       poh.REMARK          AS putout_remark
FROM wms_base_location loc
INNER JOIN wms_bill_putoutheader poh
        ON poh.ZONECODE = loc.ZONECODE
       AND poh.LOCATIONCODE = loc.LOCATIONCODE
       AND (IFNULL(poh.CONTAINER, '') = '' OR poh.CONTAINER = loc.CONTAINER)
WHERE loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR loc.ZONECODE = @ZONE_CODE)
ORDER BY loc.LOCATIONCODE;

-- =============================================================================
-- B. 释放：时间窗内「未完成出库单 + 仍在库内且占用」的库位 → OUTENABLED=1
--    场景：出库单还在，但业务上确认不再执行，仅需解除库位锁定
-- =============================================================================
/*
UPDATE wms_base_location loc
INNER JOIN wms_bill_putoutheader poh
        ON poh.ZONECODE = loc.ZONECODE
       AND poh.LOCATIONCODE = loc.LOCATIONCODE
       AND (IFNULL(poh.CONTAINER, '') = '' OR poh.CONTAINER = loc.CONTAINER)
SET loc.OUTENABLED   = 1,
    loc.LASTEDITOR   = @OPERATOR,
    loc.LASTEDITTIME = NOW()
WHERE loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR loc.ZONECODE = @ZONE_CODE);
*/

-- =============================================================================
-- C. 释放预览：孤儿占用（无有效未完成出库单，但 OUTENABLED=0）
--    场景：出库单已在页面删除，库位未释放
-- =============================================================================
SELECT loc.ZONECODE,
       loc.LOCATIONCODE,
       loc.CONTAINER,
       loc.SOURCEORDER,
       loc.OUTENABLED,
       loc.LASTEDITTIME
FROM wms_base_location loc
WHERE loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND (@ZONE_CODE IS NULL OR loc.ZONECODE = @ZONE_CODE)
  AND NOT EXISTS (
        SELECT 1
        FROM wms_bill_putoutheader poh
        WHERE poh.ENABLED = 1
          AND poh.STATE NOT IN (2, 3)
          AND poh.ZONECODE = loc.ZONECODE
          AND poh.LOCATIONCODE = loc.LOCATIONCODE
          AND (IFNULL(poh.CONTAINER, '') = '' OR poh.CONTAINER = loc.CONTAINER)
      )
ORDER BY loc.LOCATIONCODE;

-- =============================================================================
-- D. 释放：孤儿占用库位 → OUTENABLED=1
-- =============================================================================
/*
UPDATE wms_base_location loc
SET loc.OUTENABLED   = 1,
    loc.LASTEDITOR   = @OPERATOR,
    loc.LASTEDITTIME = NOW()
WHERE loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND (@ZONE_CODE IS NULL OR loc.ZONECODE = @ZONE_CODE)
  AND NOT EXISTS (
        SELECT 1
        FROM wms_bill_putoutheader poh
        WHERE poh.ENABLED = 1
          AND poh.STATE NOT IN (2, 3)
          AND poh.ZONECODE = loc.ZONECODE
          AND poh.LOCATIONCODE = loc.LOCATIONCODE
          AND (IFNULL(poh.CONTAINER, '') = '' OR poh.CONTAINER = loc.CONTAINER)
      );
*/

-- =============================================================================
-- E. 释放后验证（执行 UPDATE 后运行）
-- =============================================================================
SELECT '时间窗内仍在库内且被占用的未完成出库单数(应为0或明显减少)' AS metric,
       COUNT(*) AS cnt
FROM wms_bill_putoutheader poh
INNER JOIN wms_base_location loc
        ON loc.ZONECODE = poh.ZONECODE
       AND loc.LOCATIONCODE = poh.LOCATIONCODE
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE)
  AND loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND (IFNULL(poh.CONTAINER, '') = '' OR loc.CONTAINER = poh.CONTAINER);

SELECT '孤儿占用库位数(应为0或明显减少)' AS metric,
       COUNT(*) AS cnt
FROM wms_base_location loc
WHERE loc.ENABLED = 1
  AND loc.ISAVIALABLE = 0
  AND IFNULL(loc.CONTAINER, '') <> ''
  AND loc.OUTENABLED = 0
  AND (@ZONE_CODE IS NULL OR loc.ZONECODE = @ZONE_CODE)
  AND NOT EXISTS (
        SELECT 1
        FROM wms_bill_putoutheader poh
        WHERE poh.ENABLED = 1
          AND poh.STATE NOT IN (2, 3)
          AND poh.ZONECODE = loc.ZONECODE
          AND poh.LOCATIONCODE = loc.LOCATIONCODE
          AND (IFNULL(poh.CONTAINER, '') = '' OR poh.CONTAINER = loc.CONTAINER)
      );
