using System;
using System.Collections.Generic;
using System.Linq;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Helper
{
	public static class BdjInventoryMenuRegistrar
	{
		private const string MenuLinkUrl = "/WMS/BdjInventory/Index";
		private const string MenuName = "\u5907\u5200\u673a\u5e93\u5b58\u7ba1\u7406";

		public static string Register()
		{
			try
			{
				var permService = new sys_permissionsService();
				var rolePermService = new sys_rolepermissionService();

				if (permService.Query(x => x.LinkUrl == MenuLinkUrl).Any())
				{
					FixMenuLink(permService);
					return "\u83dc\u5355\u5df2\u5b58\u5728\uff0c\u8df3\u8fc7\u6ce8\u518c";
				}

				var inventoryPage = permService.Query(x =>
						x.LinkUrl != null &&
						(x.LinkUrl.Contains("Inventory/Index") || x.NodeName == "\u5e93\u5b58\u62a5\u8868"))
					.OrderBy(x => x.SortNo)
					.FirstOrDefault();

				sys_permissions parentModule = null;
				if (inventoryPage != null)
				{
					parentModule = permService.Query(x => x.NodeId == inventoryPage.ParentId).FirstOrDefault();
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

				var nodeType = inventoryPage?.NodeType ?? parentModule.NodeType ?? "WMS";
				var parentNodeId = parentModule.NodeId;
				var sortNo = (inventoryPage?.SortNo ?? parentModule.SortNo) + 1;
				var operatorName = "system";
				var allPerms = permService.Query(x => true);
				var nextNodeId = allPerms.Any() ? allPerms.Max(x => x.NodeId) + 1 : 1;

				var entity = new sys_permissions
				{
					NodeId = nextNodeId,
					NodeName = MenuName,
					IsEnabled = 1,
					Type = "\u9875\u9762",
					Icon = "icon-log",
					ImageUrl = "/images/log.png",
					LinkUrl = MenuLinkUrl,
					ControllerName = "BdjInventory",
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

				var referenceNodeId = inventoryPage?.NodeId ?? parentNodeId;
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

				return "\u5df2\u6210\u529f\u6ce8\u518c\u5907\u5200\u673a\u5e93\u5b58\u7ba1\u7406\u83dc\u5355";
			}
			catch (Exception ex)
			{
				return "\u83dc\u5355\u6ce8\u518c\u5931\u8d25\uff1a" + ex.Message;
			}
		}

		private static void FixMenuLink(sys_permissionsService permService)
		{
			var perm = permService.Query(x =>
					x.ControllerName == "BdjInventory"
					|| x.LinkUrl != null && x.LinkUrl.IndexOf("BdjInventory", StringComparison.OrdinalIgnoreCase) >= 0)
				.FirstOrDefault();
			if (perm == null)
			{
				return;
			}

			var changed = false;
			if (!string.Equals(perm.LinkUrl, MenuLinkUrl, StringComparison.OrdinalIgnoreCase))
			{
				perm.LinkUrl = MenuLinkUrl;
				changed = true;
			}
			if (!string.Equals(perm.ControllerName, "BdjInventory", StringComparison.OrdinalIgnoreCase))
			{
				perm.ControllerName = "BdjInventory";
				changed = true;
			}
			if (!string.Equals(perm.ActionName, "Index", StringComparison.OrdinalIgnoreCase))
			{
				perm.ActionName = "Index";
				changed = true;
			}
			if (!string.Equals(perm.NodeName, MenuName, StringComparison.Ordinal))
			{
				perm.NodeName = MenuName;
				changed = true;
			}
			if (changed)
			{
				permService.Update(perm);
			}
		}
	}
}
