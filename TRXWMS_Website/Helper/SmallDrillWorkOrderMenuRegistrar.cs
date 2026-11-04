using System;
using System.Linq;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Helper
{
    public static class SmallDrillWorkOrderMenuRegistrar
    {
        private const string MenuLinkUrl = "/WMS/SmallDrillWorkOrder/Index";
        private const string MenuName = "小钻工单调度";

        public static string Register()
        {
            try
            {
                var permService = new sys_permissionsService();
                var rolePermService = new sys_rolepermissionService();

                if (permService.Query(x => x.LinkUrl == MenuLinkUrl).Any())
                {
                    return "菜单已存在，跳过注册";
                }

                var putOutPage = permService.Query(x =>
                        x.LinkUrl != null &&
                        (x.LinkUrl.Contains("PutOut/Index") || x.NodeName == "出库清单"))
                    .OrderBy(x => x.SortNo)
                    .FirstOrDefault();

                sys_permissions parentModule = null;
                if (putOutPage != null)
                {
                    parentModule = permService.Query(x => x.NodeId == putOutPage.ParentId).FirstOrDefault();
                }

                if (parentModule == null)
                {
                    parentModule = permService.Query(x => x.Type == "模块" && x.NodeType == "WMS")
                        .OrderBy(x => x.SortNo)
                        .FirstOrDefault();
                }

                if (parentModule == null)
                {
                    return "未找到 WMS 模块，请手动在权限管理中添加菜单";
                }

                var nodeType = putOutPage?.NodeType ?? parentModule.NodeType ?? "WMS";
                var parentNodeId = parentModule.NodeId;
                var sortNo = (putOutPage?.SortNo ?? parentModule.SortNo) + 1;
                var operatorName = "system";
                var allPerms = permService.Query(x => true);
                var nextNodeId = allPerms.Any() ? allPerms.Max(x => x.NodeId) + 1 : 1;

                var entity = new sys_permissions
                {
                    NodeId = nextNodeId,
                    NodeName = MenuName,
                    IsEnabled = 1,
                    Type = "页面",
                    Icon = "icon-log",
                    ImageUrl = "/images/log.png",
                    LinkUrl = MenuLinkUrl,
                    ControllerName = "SmallDrillWorkOrder",
                    ActionName = "Index",
                    KeyName = string.Empty,
                    ParentId = parentNodeId,
                    SortNo = sortNo,
                    CreateTime = DateTime.Now,
                    CreateOperator = operatorName,
                    UpdateTime = DateTime.Now,
                    UpdateOperator = operatorName,
                    State = string.Empty,
                    NodeType = nodeType
                };

                permService.InsertReturnIdentity(entity);

                var referenceNodeId = putOutPage?.NodeId ?? parentNodeId;
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
                    if (rolePermService.Query(x => x.RoleId == roleId && x.PermissionId == entity.NodeId).Any())
                    {
                        continue;
                    }

                    rolePermService.InsertReturnIdentity(new sys_rolepermission
                    {
                        RoleId = roleId,
                        PermissionId = entity.NodeId,
                        CreateTime = DateTime.Now,
                        CreateOperator = operatorName
                    });
                }

                return "已成功注册小钻工单调度菜单";
            }
            catch (Exception ex)
            {
                return "菜单注册失败：" + ex.Message;
            }
        }
    }
}
