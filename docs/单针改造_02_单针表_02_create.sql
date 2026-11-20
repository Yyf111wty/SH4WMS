-- =============================================================================
-- 单针改造 02 · 02_create（开发库执行；执行前先跑 01_preview）
-- 与来源系统提供的 DDL 相比仅做一处调整：表级字符集由 utf8mb4 改为 utf8 /
-- utf8_general_ci，对齐目标库既有 wms_* 表，避免与 wms_base_location 等
-- JOIN 时出现 Illegal mix of collations。列定义、索引完全保持来源 DDL 原样。
-- 如坚持 utf8mb4，请将两表的 CHARACTER SET/COLLATE 改回，并接受查询需加 COLLATE。
-- =============================================================================

CREATE TABLE IF NOT EXISTS `wms_needle_inventory` (
  `ID` varchar(36) NOT NULL,
  `NEEDLENO` varchar(100) NOT NULL,
  `ITEMCODE` varchar(255) NOT NULL,
  `CONTAINER` varchar(100) NOT NULL,
  `POSITIONCODE` int NOT NULL,
  `STATUS` varchar(32) NOT NULL,
  `INBOUNDSOURCE` varchar(50) NULL,
  `INBOUNDORDER` varchar(100) NULL,
  `RESERVEORDER` varchar(100) NULL,
  `OUTBOUNDORDER` varchar(100) NULL,
  `OUTMACHINENO` int NULL,
  `OUTTARGET` varchar(50) NULL,
  `REMARK` varchar(500) NULL,
  `ENABLED` int NOT NULL DEFAULT 1,
  `CREATOR` varchar(100) NULL,
  `CREATETIME` datetime NOT NULL,
  `LASTEDITOR` varchar(100) NULL,
  `LASTEDITTIME` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `uk_needle_no` (`NEEDLENO`),
  KEY `idx_needle_container_status` (`CONTAINER`,`STATUS`,`POSITIONCODE`),
  KEY `idx_needle_item_status` (`ITEMCODE`,`STATUS`,`CONTAINER`),
  KEY `idx_needle_reserve_order` (`RESERVEORDER`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;

CREATE TABLE IF NOT EXISTS `wms_needle_transaction` (
  `ID` varchar(36) NOT NULL,
  `NEEDLEID` varchar(36) NOT NULL,
  `NEEDLENO` varchar(100) NOT NULL,
  `BIZTYPE` varchar(50) NOT NULL,
  `SOURCEORDER` varchar(100) NULL,
  `FROMSTATUS` varchar(32) NULL,
  `TOSTATUS` varchar(32) NULL,
  `FROMITEMCODE` varchar(255) NULL,
  `TOITEMCODE` varchar(255) NULL,
  `FROMCONTAINER` varchar(100) NULL,
  `FROMPOSITION` int NULL,
  `TOCONTAINER` varchar(100) NULL,
  `TOPOSITION` int NULL,
  `INBOUNDSOURCE` varchar(50) NULL,
  `OUTMACHINENO` int NULL,
  `OUTTARGET` varchar(50) NULL,
  `REMARK` varchar(500) NULL,
  `CREATOR` varchar(100) NULL,
  `CREATETIME` datetime NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `idx_needle_txn_no_time` (`NEEDLENO`,`CREATETIME`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;

-- 建后校验（期望两张表存在、uk_needle_no 唯一键生效）
SELECT TABLE_NAME, TABLE_COLLATION FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN ('wms_needle_inventory','wms_needle_transaction');
SHOW INDEX FROM `wms_needle_inventory`;
SHOW INDEX FROM `wms_needle_transaction`;
