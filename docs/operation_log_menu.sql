-- 操作审计日志菜单与按钮（手动执行）
-- 页面挂到与「操作日志」相同的 WMS 模块下，并复制操作日志的角色权限

SET @parentNodeId := (
    SELECT p.ParentId
    FROM sys_Permissions p
    WHERE p.LinkUrl LIKE '%Log/Index%' OR p.NodeName IN ('操作日志', '操作审计日志')
    ORDER BY p.SortNo
    LIMIT 1
);

SET @nodeType := (
    SELECT IFNULL(p.NodeType, 'WMS')
    FROM sys_Permissions p
    WHERE p.LinkUrl LIKE '%Log/Index%' OR p.NodeName IN ('操作日志', '操作审计日志')
    ORDER BY p.SortNo
    LIMIT 1
);

SET @baseSort := IFNULL((
    SELECT MAX(p.SortNo)
    FROM sys_Permissions p
    WHERE p.ParentId = @parentNodeId
), 100);

SET @nextNodeId := IFNULL((SELECT MAX(NodeId) FROM sys_Permissions), 0);

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '操作审计日志', 1, '页面', 'icon-log', '/images/log.png', '/WMS/OperationLog/Index', 'OperationLog', 'Index', '', @parentNodeId, @baseSort + 1, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/OperationLog/Index');

SET @pageNodeId := (
    SELECT NodeId FROM sys_Permissions WHERE LinkUrl = '/WMS/OperationLog/Index' LIMIT 1
);

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '查询', 1, '按钮', '', '', '', '', '', 'search', @pageNodeId, 1, NOW(), 'system', @nodeType
WHERE @pageNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE ParentId = @pageNodeId AND KeyName = 'search');

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '重置', 1, '按钮', '', '', '', '', '', 'reset', @pageNodeId, 2, NOW(), 'system', @nodeType
WHERE @pageNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE ParentId = @pageNodeId AND KeyName = 'reset');

INSERT INTO sys_RolePermission (RoleId, PermissionId, CreateTime, CreateOperator)
SELECT DISTINCT rp.RoleId, p.NodeId, NOW(), 'system'
FROM sys_RolePermission rp
JOIN sys_Permissions logPage ON logPage.NodeId = rp.PermissionId AND (logPage.LinkUrl LIKE '%Log/Index%' OR logPage.NodeName = '操作日志')
JOIN sys_Permissions p ON p.LinkUrl = '/WMS/OperationLog/Index' OR (p.ParentId = @pageNodeId AND p.KeyName IN ('search', 'reset'))
WHERE @pageNodeId IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM sys_RolePermission x WHERE x.RoleId = rp.RoleId AND x.PermissionId = p.NodeId
);
