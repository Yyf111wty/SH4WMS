-- =============================================================================
-- 4巷道入库错位 — 批量生成出库任务（会修改数据，执行前请先跑 01_preview）
-- 时间窗：2026-08-12 12:22:32 ~ 2026-08-13 19:00:00
-- 出库口：1002（空闲口，人工取走重入库）
-- =============================================================================
-- 执行说明：
--   1. 先备份相关表或在测试库验证
--   2. 确认 01_preview 中「可生成出库任务数」与预期一致
--   3. 确认 4 巷道无在途堆垛机任务后再执行本脚本
--   4. 本脚本使用事务；若 INSERT 行数不符预期，请 ROLLBACK
-- =============================================================================

SET @TUNNEL_NUM     := 4;
SET @ZONE_CODE      := '101';
SET @OUT_TARGET     := '1001';
SET @TIME_START     := '2026-08-12 12:22:32';
SET @TIME_END       := '2026-08-13 19:00:00';
SET @REMARK         := '4巷道错位修复出库';
SET @CREATOR        := 'WMS_FIX';

DROP TEMPORARY TABLE IF EXISTS tmp_tunnel4_fix_out;
CREATE TEMPORARY TABLE tmp_tunnel4_fix_out (
    header_id       VARCHAR(255) NOT NULL,
    body_id         VARCHAR(255) NOT NULL,
    task_id         VARCHAR(255) NOT NULL,
    location_code   VARCHAR(255) NOT NULL,
    container       VARCHAR(255) NOT NULL,
    source_order    VARCHAR(255) NOT NULL,
    zone_code       VARCHAR(255) NOT NULL,
    container_type  VARCHAR(255) NOT NULL,
    PRIMARY KEY (location_code)
);

INSERT INTO tmp_tunnel4_fix_out (
    header_id, body_id, task_id,
    location_code, container, source_order, zone_code, container_type
)
SELECT UUID() AS header_id,
       UUID() AS body_id,
       UUID() AS task_id,
       loc.LOCATIONCODE,
       loc.CONTAINER,
       loc.SOURCEORDER,
       loc.ZONECODE,
       IFNULL(NULLIF(loc.CONTAINERTYPE, ''), '0') AS container_type
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

SELECT CONCAT('staging 待生成条数 = ', COUNT(*)) AS staging_check
FROM tmp_tunnel4_fix_out;

START TRANSACTION;

-- 1) 锁定库位待出库
UPDATE wms_base_location loc
INNER JOIN tmp_tunnel4_fix_out t ON t.location_code = loc.LOCATIONCODE
SET loc.OUTENABLED = 0,
    loc.LASTEDITOR = @CREATOR,
    loc.LASTEDITTIME = NOW()
WHERE loc.OUTENABLED = 1;

-- 2) 出库单头
INSERT INTO wms_bill_putoutheader (
    ID, ZONECODE, SOURCEORDER, TASKTYPE, CONTAINER, CONTAINERTYPE,
    QUESTFLAG, QUESTTIME, FINISHTIME, STATE, TARGET, LOCATIONCODE,
    ENABLED, REMARK, CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME,
    SKUSEQ, ORDERID
)
SELECT
    t.header_id,
    t.zone_code,
    t.source_order,
    '',
    t.container,
    t.container_type,
    0,
    '1900-01-01 00:00:00',
    '1900-01-01 00:00:00',
    0,
    @OUT_TARGET,
    t.location_code,
    1,
    @REMARK,
    @CREATOR,
    NOW(),
    '',
    '1900-01-01 00:00:00',
    0,
    NULL
FROM tmp_tunnel4_fix_out t;

-- 3) 出库单明细
INSERT INTO wms_bill_putoutbody (
    ID, SOURCEORDER, CONTAINER, ITEMCODE, LOT, QTY,
    CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME,
    HEADERID, WEIGHT, TASK_NO_SN
)
SELECT
    t.body_id,
    t.source_order,
    t.container,
    t.source_order,
    '',
    0,
    @CREATOR,
    NOW(),
    '',
    '1900-01-01 00:00:00',
    t.header_id,
    '',
    ''
FROM tmp_tunnel4_fix_out t;

-- 4) 堆垛机出库任务
INSERT INTO wms_stocker_task (
    ID, CODE, SOURCEORDER, ZONECODE, CONTAINER, TYPE, PRIORITY,
    STARTTAXI, ENDTAXI, STATE, STARTTIME, IMPLEMENTTIME, FINISHTIME,
    CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME,
    SKUSEQ, TASKMODE, PALLET, ORDERINFO
)
SELECT
    t.task_id,
    t.task_id,
    t.source_order,
    t.zone_code,
    t.container,
    2,
    1,
    t.location_code,
    @OUT_TARGET,
    0,
    NOW(),
    '1900-01-01 00:00:00',
    '1900-01-01 00:00:00',
    @CREATOR,
    NOW(),
    @REMARK,
    NOW(),
    0,
    'NORMAL',
    '',
    CONCAT('T4_FIX|', t.location_code, '|', t.container)
FROM tmp_tunnel4_fix_out t;

SELECT CONCAT('putoutheader 新增 = ', COUNT(*)) AS insert_check
FROM wms_bill_putoutheader poh
INNER JOIN tmp_tunnel4_fix_out t ON t.header_id = poh.ID;

SELECT CONCAT('stocker_task 新增 = ', COUNT(*)) AS insert_check
FROM wms_stocker_task st
INNER JOIN tmp_tunnel4_fix_out t ON t.task_id = st.ID;

-- 核对 staging 与 insert 数量一致后再 COMMIT
-- COMMIT;
-- 若数量不对：ROLLBACK;

-- =============================================================================
-- 手动确认后，取消下面一行注释并执行：
-- COMMIT;
-- =============================================================================
