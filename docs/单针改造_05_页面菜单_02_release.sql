-- =============================================================================
-- 单针改造 05 · 页面菜单 02_release（确认 01_preview 无冲突后手动执行）
-- 挂到「库存查询」同父节点：单针追溯 + 盒内针明细，并复制库存查询的角色权限
-- 风格沿用 docs/operation_log_menu.sql
-- =============================================================================

SET @parentNodeId := (
    SELECT ParentId
    FROM sys_Permissions
    WHERE LinkUrl LIKE '%Inventory/Index%'
    ORDER BY SortNo
    LIMIT 1
);

SET @nodeType := (
    SELECT IFNULL(p.NodeType, 'WMS')
    FROM sys_Permissions p
    WHERE LinkUrl LIKE '%Inventory/Index%'
    ORDER BY p.SortNo
    LIMIT 1
);

SET @baseSort := IFNULL((
    SELECT MAX(p.SortNo)
    FROM sys_Permissions p
    WHERE p.ParentId = @parentNodeId
), 100);

SET @nextNodeId := IFNULL((SELECT MAX(NodeId) FROM sys_Permissions), 0);

-- 页面1：单针追溯
INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '单针追溯', 1, '页面', 'icon-log', '/images/log.png', '/WMS/Needle/Index', 'Needle', 'Index', '', @parentNodeId, @baseSort + 1, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/Needle/Index');

SET @needlePageNodeId := (
    SELECT NodeId FROM sys_Permissions WHERE LinkUrl = '/WMS/Needle/Index' LIMIT 1
);

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '查询', 1, '按钮', '', '', '', '', '', 'search', @needlePageNodeId, 1, NOW(), 'system', @nodeType
WHERE @needlePageNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE ParentId = @needlePageNodeId AND KeyName = 'search');

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '重置', 1, '按钮', '', '', '', '', '', 'reset', @needlePageNodeId, 2, NOW(), 'system', @nodeType
WHERE @needlePageNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE ParentId = @needlePageNodeId AND KeyName = 'reset');

-- 页面2：盒内针明细
INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '盒内针明细', 1, '页面', 'icon-log', '/images/log.png', '/WMS/Needle/Container', 'Needle', 'Container', '', @parentNodeId, @baseSort + 2, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/Needle/Container');

SET @boxPageNodeId := (
    SELECT NodeId FROM sys_Permissions WHERE LinkUrl = '/WMS/Needle/Container' LIMIT 1
);

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '查询', 1, '按钮', '', '', '', '', '', 'search', @boxPageNodeId, 1, NOW(), 'system', @nodeType
WHERE @boxPageNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE ParentId = @boxPageNodeId AND KeyName = 'search');

-- 角色权限：凡有「库存查询」页权限的角色，自动获得两个新页面及其按钮
INSERT INTO sys_RolePermission (RoleId, PermissionId, CreateTime, CreateOperator)
SELECT DISTINCT rp.RoleId, p.NodeId, NOW(), 'system'
FROM sys_RolePermission rp
JOIN sys_Permissions inv ON inv.NodeId = rp.PermissionId AND inv.LinkUrl LIKE '%Inventory/Index%'
JOIN sys_Permissions p ON p.NodeId IN (@needlePageNodeId, @boxPageNodeId)
                      OR (p.ParentId = @needlePageNodeId AND p.KeyName IN ('search', 'reset'))
                      OR (p.ParentId = @boxPageNodeId AND p.KeyName = 'search')
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM sys_RolePermission x WHERE x.RoleId = rp.RoleId AND x.PermissionId = p.NodeId
);

-- 执行后校验（期望：2 页面 + 3 按钮；角色授权数 ≥ 页面数）
SELECT NodeId, NodeName, Type, LinkUrl, ParentId, KeyName
FROM sys_Permissions
WHERE LinkUrl IN ('/WMS/Needle/Index', '/WMS/Needle/Container')
   OR ParentId IN (SELECT NodeId FROM sys_Permissions WHERE LinkUrl IN ('/WMS/Needle/Index', '/WMS/Needle/Container'));
