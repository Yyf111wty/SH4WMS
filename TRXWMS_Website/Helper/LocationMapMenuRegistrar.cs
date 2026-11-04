using System;
using System.Collections.Generic;
using System.Linq;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Helper
{
    public static class LocationMapMenuRegistrar
    {
        private class MenuDef
        {
            public string NodeName { get; set; }
            public string LinkUrl { get; set; }
            public string ControllerName { get; set; }
            public string ActionName { get; set; }
        }

        private static readonly string[] ObsoleteLinkUrls =
        {
            "/WMS/LocationMap/Index",
            "/WMS/LocationMap/RowTable"
        };

        private static readonly MenuDef[] MenuItems =
        {
            new MenuDef { NodeName = "\u5e93\u4f4d\u77e9\u9635Canvas", LinkUrl = "/WMS/LocationMap/Canvas", ControllerName = "LocationMap", ActionName = "Canvas" }
        };

        public static string Register()
        {
            try
            {
                var permService = new sys_permissionsService();
                var rolePermService = new sys_rolepermissionService();

                RemoveObsoleteMenus(permService, rolePermService);
                FixMenuNames(permService);

                var locationPage = permService.Query(x =>
                        x.LinkUrl != null &&
                        (x.LinkUrl.Contains("Location/Index") || x.NodeName == "\u5e93\u4f4d\u7ba1\u7406"))
                    .OrderBy(x => x.SortNo)
                    .FirstOrDefault();

                sys_permissions parentModule = null;
                if (locationPage != null)
                {
                    parentModule = permService.Query(x => x.NodeId == locationPage.ParentId).FirstOrDefault();
                }

                if (parentModule == null)
                {
                    parentModule = permService.Query(x => x.Type == "\u6a21\u5757" && x.NodeType == "WMS")
                        .OrderBy(x => x.SortNo)
                        .FirstOrDefault();
                }

                if (parentModule == null)
                {
                    return "\u672a\u627e\u5230 WMS \u6a21\u5757\uff0c\u8bf7\u624b\u52a8\u5728\u6743\u9650\u7ba1\u7406\u4e2d\u6dfb\u52a0\u83dc\u5355";
                }

                var nodeType = locationPage?.NodeType ?? parentModule.NodeType ?? "WMS";
                var parentNodeId = parentModule.NodeId;
                var baseSort = locationPage?.SortNo ?? parentModule.SortNo;
                var operatorName = "system";

                var allPerms = permService.Query(x => true);
                var nextNodeId = allPerms.Any() ? allPerms.Max(x => x.NodeId) + 1 : 1;
                var addedNodeIds = new List<int>();
                var sortNo = baseSort + 1;

                foreach (var item in MenuItems)
                {
                    if (permService.Query(x => x.LinkUrl == item.LinkUrl).Any())
                    {
                        continue;
                    }

                    var entity = new sys_permissions
                    {
                        NodeId = nextNodeId++,
                        NodeName = item.NodeName,
                        IsEnabled = 1,
                        Type = "\u9875\u9762",
                        Icon = "icon-log",
                        ImageUrl = "/images/log.png",
                        LinkUrl = item.LinkUrl,
                        ControllerName = item.ControllerName,
                        ActionName = item.ActionName,
                        KeyName = string.Empty,
                        ParentId = parentNodeId,
                        SortNo = sortNo++,
                        CreateTime = DateTime.Now,
                        CreateOperator = operatorName,
                        UpdateTime = DateTime.Now,
                        UpdateOperator = operatorName,
                        State = string.Empty,
                        NodeType = nodeType
                    };

                    permService.InsertReturnIdentity(entity);
                    addedNodeIds.Add(entity.NodeId);
                }

                if (addedNodeIds.Count == 0)
                {
                    return "\u83dc\u5355\u5df2\u5b58\u5728\uff0c\u8df3\u8fc7\u6ce8\u518c";
                }

                var referenceNodeId = locationPage?.NodeId ?? parentNodeId;
                var roleIds = rolePermService.Query(x => x.PermissionId == referenceNodeId)
                    .Select(x => x.RoleId)
                    .Distinct()
                    .ToList();

                if (roleIds.Count == 0)
                {
                    roleIds = rolePermService.Query(x => true)
                        .Select(x => x.RoleId)
                        .Distinct()
                        .ToList();
                }

                foreach (var roleId in roleIds)
                {
                    foreach (var nodeId in addedNodeIds)
                    {
                        if (rolePermService.Query(x => x.RoleId == roleId && x.PermissionId == nodeId).Any())
                        {
                            continue;
                        }

                        rolePermService.InsertReturnIdentity(new sys_rolepermission
                        {
                            RoleId = roleId,
                            PermissionId = nodeId,
                            CreateTime = DateTime.Now,
                            CreateOperator = operatorName
                        });
                    }
                }

                return "\u5df2\u6210\u529f\u6ce8\u518c " + addedNodeIds.Count + " \u4e2a\u5e93\u4f4d\u72b6\u6001\u83dc\u5355\u9879";
            }
            catch (Exception ex)
            {
                return "\u83dc\u5355\u6ce8\u518c\u5931\u8d25\uff1a" + ex.Message;
            }
        }

        private static void RemoveObsoleteMenus(sys_permissionsService permService, sys_rolepermissionService rolePermService)
        {
            foreach (var linkUrl in ObsoleteLinkUrls)
            {
                var obsolete = permService.Query(x => x.LinkUrl == linkUrl).ToList();
                foreach (var perm in obsolete)
                {
                    var rolePerms = rolePermService.Query(x => x.PermissionId == perm.NodeId).ToList();
                    foreach (var rp in rolePerms)
                    {
                        rolePermService.Delete(rp);
                    }
                    permService.Delete(perm);
                }
            }
        }

        private static void FixMenuNames(sys_permissionsService permService)
        {
            foreach (var item in MenuItems)
            {
                var perm = permService.Query(x => x.LinkUrl == item.LinkUrl).FirstOrDefault();
                if (perm == null || perm.NodeName == item.NodeName)
                {
                    continue;
                }

                perm.NodeName = item.NodeName;
                permService.Update(perm);
            }
        }
    }
}
