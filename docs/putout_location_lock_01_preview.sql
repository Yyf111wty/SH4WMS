-- =============================================================================
-- 出库单未完结但库位仍被占用 — 诊断脚本（只读，默认不修改数据）
-- 用途：排查「出库单未执行/未完成，但 wms_base_location.OUTENABLED=0 仍锁定」的脏数据
--
-- 出库单 STATE 说明（wms_bill_putoutheader）：
--   0 = 待执行    1 = 正在执行    2 = 已完成    3 = 已装盘    5 = 异常结束
-- 库位 OUTENABLED 说明（wms_base_location）：
--   0 = 出库占用（不可用）    1 = 可用
-- 「仍在库内」判定：ENABLED=1 AND ISAVIALABLE=0 AND CONTAINER<>''
-- =============================================================================

-- 按需修改时间窗、库区
SET @TIME_START := '2026-08-01 00:00:00';
SET @TIME_END   := '2026-08-25 23:59:59';
SET @ZONE_CODE  := NULL;   -- 指定库区如 '101'；NULL 表示不限库区

-- =============================================================================
-- 1. 时间范围内、状态未完成（非 2/3）的出库单 — 汇总
-- =============================================================================
SELECT '时间窗内未完成出库单总数(STATE NOT IN 2,3)' AS metric,
       COUNT(*) AS cnt
FROM wms_bill_putoutheader poh
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE);

SELECT poh.STATE,
       CASE poh.STATE
           WHEN 0 THEN '待执行'
           WHEN 1 THEN '正在执行'
           WHEN 5 THEN '异常结束'
           ELSE CONCAT('其他(', poh.STATE, ')')
       END AS state_desc,
       COUNT(*) AS cnt
FROM wms_bill_putoutheader poh
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE)
GROUP BY poh.STATE
ORDER BY poh.STATE;

-- =============================================================================
-- 2. 时间范围内、状态未完成（非 2/3）的出库单 — 明细清单
-- =============================================================================
SELECT poh.ID              AS putout_header_id,
       poh.ZONECODE,
       poh.LOCATIONCODE,
       poh.CONTAINER,
       poh.SOURCEORDER,
       poh.TARGET,
       poh.STATE,
       CASE poh.STATE
           WHEN 0 THEN '待执行'
           WHEN 1 THEN '正在执行'
           WHEN 5 THEN '异常结束'
           ELSE CONCAT('其他(', poh.STATE, ')')
       END                 AS state_desc,
       poh.REMARK,
       poh.ORDERID,
       poh.CREATETIME,
       poh.FINISHTIME,
       poh.LASTEDITTIME
FROM wms_bill_putoutheader poh
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE)
ORDER BY poh.CREATETIME DESC, poh.LOCATIONCODE;

-- =============================================================================
-- 3. 上述出库单对应库位：仍在库内且 OUTENABLED=0（异常占用清单）★核心
-- =============================================================================
SELECT poh.ID              AS putout_header_id,
       poh.STATE           AS putout_state,
       CASE poh.STATE
           WHEN 0 THEN '待执行'
           WHEN 1 THEN '正在执行'
           WHEN 5 THEN '异常结束'
           ELSE CONCAT('其他(', poh.STATE, ')')
       END                 AS putout_state_desc,
       poh.CREATETIME      AS putout_create_time,
       poh.REMARK          AS putout_remark,
       poh.ZONECODE,
       poh.LOCATIONCODE,
       poh.CONTAINER       AS putout_container,
       poh.SOURCEORDER     AS putout_sourceorder,
       poh.TARGET,
       loc.CONTAINER       AS loc_container,
       loc.SOURCEORDER     AS loc_sourceorder,
       loc.OUTENABLED,
       CASE loc.OUTENABLED
           WHEN 0 THEN '占用'
           WHEN 1 THEN '可用'
           ELSE CONCAT('其他(', loc.OUTENABLED, ')')
       END                 AS outenabled_desc,
       loc.ISAVIALABLE,
       loc.ENABLED         AS loc_enabled,
       loc.TUNNELNUM,
       loc.LASTEDITTIME    AS loc_last_edit_time,
       CASE
           WHEN loc.LOCATIONCODE IS NULL THEN '库位不存在'
           WHEN loc.ENABLED <> 1 THEN '库位未激活'
           WHEN IFNULL(loc.CONTAINER, '') = '' THEN '库位已空'
           WHEN loc.ISAVIALABLE <> 0 THEN '库位非有货状态'
           WHEN loc.OUTENABLED <> 0 THEN '库位未占用'
           WHEN IFNULL(poh.CONTAINER, '') <> '' AND loc.CONTAINER <> poh.CONTAINER THEN '载具不一致'
           ELSE '仍在库内且被占用'
       END                 AS diagnose_result,
       CASE
           WHEN st.ID IS NOT NULL THEN CONCAT('有堆垛机任务 STATE=', st.STATE)
           ELSE '无未完成堆垛机任务'
       END                 AS stocker_task_hint
FROM wms_bill_putoutheader poh
LEFT JOIN wms_base_location loc
       ON loc.ZONECODE = poh.ZONECODE
      AND loc.LOCATIONCODE = poh.LOCATIONCODE
LEFT JOIN wms_stocker_task st
       ON st.TYPE = 2
      AND st.STATE IN (0, 1)
      AND st.STARTTAXI = poh.LOCATIONCODE
      AND (IFNULL(poh.CONTAINER, '') = '' OR st.CONTAINER = poh.CONTAINER)
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE)
  AND IFNULL(poh.LOCATIONCODE, '') <> ''
ORDER BY
    CASE
        WHEN loc.OUTENABLED = 0
         AND loc.ENABLED = 1
         AND loc.ISAVIALABLE = 0
         AND IFNULL(loc.CONTAINER, '') <> '' THEN 0
        ELSE 1
    END,
    poh.CREATETIME DESC,
    poh.LOCATIONCODE;

-- =============================================================================
-- 4. 仅「仍在库内且被占用」— 精简列表（建议优先处理）
-- =============================================================================
SELECT poh.ID              AS putout_header_id,
       poh.STATE,
       poh.CREATETIME,
       poh.REMARK,
       poh.ZONECODE,
       poh.LOCATIONCODE,
       poh.CONTAINER,
       poh.SOURCEORDER,
       loc.OUTENABLED,
       loc.TUNNELNUM,
       loc.LASTEDITTIME    AS loc_last_edit_time
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
  AND (IFNULL(poh.CONTAINER, '') = '' OR loc.CONTAINER = poh.CONTAINER)
ORDER BY poh.CREATETIME DESC, poh.LOCATIONCODE;

-- =============================================================================
-- 5. 汇总：异常占用条数
-- =============================================================================
SELECT '仍在库内且被占用的未完成出库单数' AS metric,
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

-- =============================================================================
-- 6. 补充：出库单已删/无效，但库位仍 OUTENABLED=0（孤儿占用，不限时间窗）
--    适用于「页面删了出库单，库位没释放」的历史脏数据
-- =============================================================================
SELECT loc.ZONECODE,
       loc.LOCATIONCODE,
       loc.CONTAINER,
       loc.SOURCEORDER,
       loc.OUTENABLED,
       loc.TUNNELNUM,
       loc.LASTEDITTIME,
       '无有效未完成出库单但仍占用' AS diagnose_result
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
ORDER BY loc.LASTEDITTIME DESC, loc.LOCATIONCODE;

-- =============================================================================
-- 7. 补充：未完成出库单关联的未完成堆垛机出库任务
-- =============================================================================
SELECT poh.ID              AS putout_header_id,
       poh.LOCATIONCODE,
       poh.CONTAINER,
       poh.STATE           AS putout_state,
       st.ID               AS task_id,
       st.STATE            AS task_state,
       st.ENDTAXI,
       st.TASKMODE,
       st.CREATETIME       AS task_create_time,
       st.ORDERINFO
FROM wms_bill_putoutheader poh
INNER JOIN wms_stocker_task st
        ON st.TYPE = 2
       AND st.STATE IN (0, 1)
       AND st.STARTTAXI = poh.LOCATIONCODE
       AND (IFNULL(poh.CONTAINER, '') = '' OR st.CONTAINER = poh.CONTAINER)
WHERE poh.ENABLED = 1
  AND poh.STATE NOT IN (2, 3)
  AND poh.CREATETIME >= @TIME_START
  AND poh.CREATETIME <= @TIME_END
  AND (@ZONE_CODE IS NULL OR poh.ZONECODE = @ZONE_CODE)
ORDER BY st.CREATETIME DESC;
