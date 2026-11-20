-- 入库/出库清单查询性能优化索引
-- 背景：两张单头表各约 11~12 万行，此前无 CREATETIME 索引，
--       列表默认按 CREATETIME 倒序分页，导致全表扫描 + filesort。
-- 说明：MySQL 的 ALTER TABLE ... ADD INDEX 不支持 IF NOT EXISTS，
--       重复执行会报 1061 Duplicate key name，可忽略或先查 SHOW INDEX 确认。

-- 出库单头：创建时间倒序分页
ALTER TABLE wms_bill_putoutheader ADD INDEX idx_putout_ctime (CREATETIME);

-- 出库单头：按库区 + 创建时间（按用户名限定库区的用户走这条）
ALTER TABLE wms_bill_putoutheader ADD INDEX idx_putout_zone_ctime (ZONECODE, CREATETIME);

-- 入库单头：创建时间倒序分页
ALTER TABLE wms_bill_putinbindingheader ADD INDEX idx_putin_ctime (CREATETIME);

-- 明细表：EXISTS 子查询按 HEADERID + 物料/批次定位
ALTER TABLE wms_bill_putoutbody ADD INDEX idx_putoutbody_header_item (HEADERID, ITEMCODE);
ALTER TABLE wms_bill_putoutbody ADD INDEX idx_putoutbody_header_lot (HEADERID, LOT);
ALTER TABLE wms_bill_putinbindingbody ADD INDEX idx_putinbody_header_item (HEADERID, ITEMCODE);
ALTER TABLE wms_bill_putinbindingbody ADD INDEX idx_putinbody_header_lot (HEADERID, LOT);

-- ===== 全站列表：按 CREATETIME 倒序分页的大表 =====

-- 堆垛机任务（约 24 万行，页面按 CREATETIME 倒序分页）
ALTER TABLE wms_stocker_task ADD INDEX idx_task_ctime (CREATETIME);
-- 小钻工单接口固定 TYPE=2、ENDTAXI=1002，并按时间窗读取任务
ALTER TABLE wms_stocker_task ADD INDEX idx_task_type_endtaxi_time (TYPE, ENDTAXI, CREATETIME);

-- 小钻工单出库单头固定 TARGET=1002，并按时间窗读取
ALTER TABLE wms_bill_putoutheader ADD INDEX idx_putout_target_ctime (TARGET, CREATETIME);

-- 单针库存（约 71 万行，页面固定 ENABLED=1 且按 CREATETIME / LASTEDITTIME 倒序）
ALTER TABLE wms_needle_inventory ADD INDEX idx_needle_enabled_ctime (ENABLED, CREATETIME);
ALTER TABLE wms_needle_inventory ADD INDEX idx_needle_enabled_lastedit (ENABLED, LASTEDITTIME);

-- 系统日志（按时间倒序）
ALTER TABLE sys_log ADD INDEX idx_syslog_createtime (CreateTime);

-- 验证执行计划（应看到 key=idx_putout_ctime，且没有 Using filesort 之外的全表扫描）
-- EXPLAIN SELECT h.* FROM wms_bill_putoutheader h
--  WHERE h.CREATETIME >= DATE_SUB(NOW(), INTERVAL 30 DAY)
--    AND h.CREATETIME <  DATE_ADD(NOW(), INTERVAL 1 DAY)
--  ORDER BY h.CREATETIME DESC LIMIT 0, 30;
