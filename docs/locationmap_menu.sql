-- 库位状态展示菜单（手动执行备用脚本）
-- 将三个页面挂到与「库位管理」相同的 WMS 模块下，并复制库位管理的角色权限

SET @parentNodeId := (
    SELECT p.ParentId
    FROM sys_Permissions p
    WHERE p.LinkUrl LIKE '%Location/Index%' OR p.NodeName = '库位管理'
    ORDER BY p.SortNo
    LIMIT 1
);

SET @nodeType := (
    SELECT IFNULL(p.NodeType, 'WMS')
    FROM sys_Permissions p
    WHERE p.LinkUrl LIKE '%Location/Index%' OR p.NodeName = '库位管理'
    ORDER BY p.SortNo
    LIMIT 1
);

SET @baseSort := IFNULL((
    SELECT p.SortNo
    FROM sys_Permissions p
    WHERE p.LinkUrl LIKE '%Location/Index%' OR p.NodeName = '库位管理'
    ORDER BY p.SortNo
    LIMIT 1
), 100);

SET @nextNodeId := IFNULL((SELECT MAX(NodeId) FROM sys_Permissions), 0);

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '库位状态展示', 1, '页面', 'icon-log', '/images/log.png', '/WMS/LocationMap/Index', 'LocationMap', 'Index', '', @parentNodeId, @baseSort + 1, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/LocationMap/Index');

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '库位矩阵Canvas', 1, '页面', 'icon-log', '/images/log.png', '/WMS/LocationMap/Canvas', 'LocationMap', 'Canvas', '', @parentNodeId, @baseSort + 2, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/LocationMap/Canvas');

INSERT INTO sys_Permissions
    (NodeId, NodeName, IsEnabled, Type, Icon, ImageUrl, LinkUrl, ControllerName, ActionName, KeyName, ParentId, SortNo, CreateTime, CreateOperator, NodeType)
SELECT @nextNodeId := @nextNodeId + 1, '库位状态Layui', 1, '页面', 'icon-log', '/images/log.png', '/WMS/LocationMap/RowTable', 'LocationMap', 'RowTable', '', @parentNodeId, @baseSort + 3, NOW(), 'system', @nodeType
WHERE @parentNodeId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys_Permissions WHERE LinkUrl = '/WMS/LocationMap/RowTable');

INSERT INTO sys_RolePermission (RoleId, PermissionId, CreateTime, CreateOperator)
SELECT DISTINCT rp.RoleId, p.NodeId, NOW(), 'system'
FROM sys_RolePermission rp
JOIN sys_Permissions loc ON loc.NodeId = rp.PermissionId AND (loc.LinkUrl LIKE '%Location/Index%' OR loc.NodeName = '库位管理')
JOIN sys_Permissions p ON p.LinkUrl IN ('/WMS/LocationMap/Index', '/WMS/LocationMap/Canvas', '/WMS/LocationMap/RowTable')
WHERE NOT EXISTS (
    SELECT 1 FROM sys_RolePermission x WHERE x.RoleId = rp.RoleId AND x.PermissionId = p.NodeId
);
