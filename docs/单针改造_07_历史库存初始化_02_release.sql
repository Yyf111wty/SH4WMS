-- =============================================================================
-- 单针改造 07 · 历史库存初始化 02_release
-- 前置：01_preview 已跑且结论过目；两张针表已存在；建议业务低峰执行
-- 幂等：候选圈定排除"已有针数据/已有 T-HIST- 前缀"的盒，可安全重跑
-- 标记：CREATOR='HIST_INIT'、REMARK='历史库存初始化_20261105'（回滚按此圈定）
--
-- 可选善后（默认不执行，人工核对 preview 第 6 条后再处理）：
--   若某些候选盒存在进行中的出库任务，初始化后它们会被算成"可用"。两种处理：
--   a) 少量盒：直接等出库流程走完——completeOutKu 的 MarkOutboundComplete 会把
--      AVAILABLE 针一并翻转 OUTBOUND（第七批已做兼容），履历自动补齐，无需手工；
--   b) 需立即体现预占：仿模板执行——
--     -- SET @out_order := '出库单号/任务SOURCEORDER';
--     -- UPDATE wms_needle_inventory SET STATUS='RESERVED', RESERVEORDER=@out_order,
--     --        LASTEDITOR='HIST_INIT', LASTEDITTIME=sysdate()
--     --  WHERE CONTAINER IN ('WH000001','WH000002') AND STATUS='AVAILABLE';
--     -- INSERT INTO wms_needle_transaction (ID,NEEDLEID,NEEDLENO,BIZTYPE,SOURCEORDER,FROMSTATUS,TOSTATUS,
--     --        TOITEMCODE,TOCONTAINER,TOPOSITION,INBOUNDSOURCE,REMARK,CREATOR,CREATETIME)
--     -- SELECT UUID(),i.ID,i.NEEDLENO,'RESERVE',@out_order,'AVAILABLE','RESERVED',
--     --        i.ITEMCODE,i.CONTAINER,i.POSITIONCODE,i.INBOUNDSOURCE,'历史初始化善后-预占','HIST_INIT',sysdate()
--     --   FROM wms_needle_inventory i WHERE i.CONTAINER IN ('WH000001','WH000002') AND i.STATUS='RESERVED';
-- =============================================================================

SET @cre := 'HIST_INIT';
SET @rk  := '历史库存初始化_20261105';

-- 0) 超长盒号将被排除（NEEDLENO= 'T-HIST-'+盒号+'-槽位' 须 ≤100），先看清楚再决定是否放宽
SELECT DISTINCT BARCODE AS too_long_excluded
FROM wms_bill_inventorybook
WHERE CHAR_LENGTH(BARCODE) > 85
  AND BARCODE IS NOT NULL AND BARCODE <> ''
LIMIT 50;

-- 1) 候选盒快照（临时表，圈定逻辑与 preview 第 1 条一致 + 空规格排除）
--    规格/数量/入库时间取"该盒号最新 CREATETIME 的台账行"：
--    同盒号多行=盒周转时旧行未随 SAP 旁路出库删除（实测 14455 盒仅 7 例），最新行才是当前实装规格
DROP TEMPORARY TABLE IF EXISTS tmp_hist_box;
CREATE TEMPORARY TABLE tmp_hist_box (
    CONTAINER   varchar(100) PRIMARY KEY,
    ITEMCODE    varchar(255),
    INBOUNDSOURCE varchar(50),
    POSCNT      int,
    CT          datetime
);

INSERT INTO tmp_hist_box (CONTAINER, ITEMCODE, INBOUNDSOURCE, POSCNT, CT)
SELECT ib.BARCODE,
       MAX(ib.ITEMCODE),
       MAX(NULLIF(ib.SUPPLIERID, '')),
       IF(MAX(ib.QTY) BETWEEN 1 AND 200, MAX(ib.QTY), 50),
       IF(MAX(ib.CREATETIME) > '1990-01-01', MAX(ib.CREATETIME), NOW())
FROM wms_bill_inventorybook ib
JOIN (SELECT BARCODE, MAX(CREATETIME) AS LAST_CT
        FROM wms_bill_inventorybook
       WHERE BARCODE IS NOT NULL AND BARCODE <> ''
       GROUP BY BARCODE) L ON L.BARCODE = ib.BARCODE AND L.LAST_CT = ib.CREATETIME
WHERE ib.BARCODE IS NOT NULL AND ib.BARCODE <> ''
  AND ib.ITEMCODE IS NOT NULL AND ib.ITEMCODE <> ''
  AND CHAR_LENGTH(ib.BARCODE) <= 85
  AND NOT EXISTS (SELECT 1 FROM wms_needle_inventory ni WHERE ni.CONTAINER = ib.BARCODE)
  AND NOT EXISTS (SELECT 1 FROM wms_needle_inventory ni2 WHERE ni2.NEEDLENO LIKE CONCAT('T-HIST-', ib.BARCODE, '-%'))
GROUP BY ib.BARCODE;

SELECT COUNT(*) AS boxes_to_init, IFNULL(SUM(POSCNT),0) AS needles_to_create FROM tmp_hist_box;

-- 2) 生成当前表针行（槽位 1..POSCNT；状态 AVAILABLE）
START TRANSACTION;

INSERT INTO wms_needle_inventory
    (ID, NEEDLENO, ITEMCODE, CONTAINER, POSITIONCODE, STATUS,
     INBOUNDSOURCE, INBOUNDORDER, RESERVEORDER, OUTBOUNDORDER, OUTMACHINENO, OUTTARGET,
     REMARK, ENABLED, CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME)
SELECT UUID(), CONCAT('T-HIST-', b.CONTAINER, '-', n.n), b.ITEMCODE, b.CONTAINER, n.n, 'AVAILABLE',
       b.INBOUNDSOURCE, b.CONTAINER, NULL, NULL, NULL, NULL,
       @rk, 1, @cre, b.CT, @cre, b.CT
FROM tmp_hist_box b
JOIN (
    SELECT (ones.d + tens.d * 10 + 1) AS n
    FROM (SELECT 0 d UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
          UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) ones
    CROSS JOIN (SELECT 0 d UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
          UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9
          UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14
          UNION ALL SELECT 15 UNION ALL SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19) tens
) n ON n.n <= b.POSCNT;

-- 3) 生成入库履历（每针一条 INBOUND 事件；FROMSTATUS 留空=未经过待入库态）
INSERT INTO wms_needle_transaction
    (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
     FROMITEMCODE, TOITEMCODE, FROMCONTAINER, FROMPOSITION, TOCONTAINER, TOPOSITION,
     INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT i.ID, i.ID, i.NEEDLENO, 'INBOUND', i.CONTAINER, NULL, 'AVAILABLE',
       NULL, i.ITEMCODE, NULL, NULL, i.CONTAINER, i.POSITIONCODE,
       i.INBOUNDSOURCE, NULL, NULL, @rk, @cre, i.CREATETIME
FROM wms_needle_inventory i
WHERE i.CREATOR = @cre AND i.REMARK = @rk
  AND NOT EXISTS (SELECT 1 FROM wms_needle_transaction t WHERE t.NEEDLEID = i.ID);

COMMIT;

-- 4) 执行后校验
SELECT COUNT(*) AS inv_created, COUNT(DISTINCT CONTAINER) AS boxes_covered
FROM wms_needle_inventory WHERE CREATOR = @cre AND REMARK = @rk;

SELECT COUNT(*) AS txn_created
FROM wms_needle_transaction WHERE CREATOR = @cre AND REMARK = @rk;

-- 应恒等于：inv_created = txn_created；boxes_covered = boxes_to_init
SELECT (SELECT COUNT(*) FROM wms_needle_inventory WHERE CREATOR=@cre AND REMARK=@rk) AS inv_cnt,
       (SELECT COUNT(*) FROM wms_needle_transaction WHERE CREATOR=@cre AND REMARK=@rk) AS txn_cnt;

-- 5) 抽样看一个盒（把 WH000001 换成任意候选盒号）
-- SELECT NEEDLENO, ITEMCODE, POSITIONCODE, STATUS, INBOUNDSOURCE, CREATETIME
--   FROM wms_needle_inventory WHERE CONTAINER = 'WH000001' ORDER BY POSITIONCODE;
