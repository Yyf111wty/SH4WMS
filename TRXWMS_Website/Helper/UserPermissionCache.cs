using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRWMS_PrApi.Models;
using TRWMS_Website.ViewModels;

namespace TRXWMS_Website.Helper
{
	public static class UserPermissionCache
	{
		private const string PermissionsSessionKey = "__UserPermissions";
		private const string PermissionUserSessionKey = "__UserPermissionsUser";

		public static List<Rows> LoadPermissions(HttpSessionStateBase session, string userName)
		{
			if (session == null || string.IsNullOrWhiteSpace(userName))
			{
				return new List<Rows>();
			}

			var cachedUser = session[PermissionUserSessionKey] as string;
			var cached = session[PermissionsSessionKey] as List<Rows>;
			if (cached != null &&
				cachedUser != null &&
				string.Equals(cachedUser, userName, StringComparison.OrdinalIgnoreCase))
			{
				return cached;
			}

			var permissions = TRWMS_PrApi.Controllers.LoginController.TRWCS_GetUserPermissionDataApi(userName, string.Empty);
			var rows = MapToRows(permissions);
			session[PermissionUserSessionKey] = userName;
			session[PermissionsSessionKey] = rows;
			return rows;
		}

		public static List<Rows> GetPermissions(HttpSessionStateBase session, string userName)
		{
			if (session == null || string.IsNullOrWhiteSpace(userName))
			{
				return new List<Rows>();
			}

			var cachedUser = session[PermissionUserSessionKey] as string;
			var cached = session[PermissionsSessionKey] as List<Rows>;
			if (cached != null &&
				cachedUser != null &&
				string.Equals(cachedUser, userName, StringComparison.OrdinalIgnoreCase))
			{
				return cached;
			}

			return LoadPermissions(session, userName);
		}

		public static void Clear(HttpSessionStateBase session)
		{
			if (session == null)
			{
				return;
			}

			session.Remove(PermissionsSessionKey);
			session.Remove(PermissionUserSessionKey);
		}

		private static List<Rows> MapToRows(List<Permission> permissions)
		{
			if (permissions == null || permissions.Count == 0)
			{
				return new List<Rows>();
			}

			return permissions.Select(p => new Rows
			{
				Id = p.Id,
				RoleId = p.RoleId,
				NodeId = p.NodeId,
				NodeName = p.NodeName,
				KeyName = p.KeyName,
				IsEnabled = p.IsEnabled,
				Type = p.Type,
				Linkurl = p.LinkUrl,
				ControllerName = p.ControllerName,
				ActionName = p.ActionName,
				SortNo = p.SortNo,
				icon = p.Icon,
				Imageurl = p.ImageUrl,
				_parentId = p._parentId,
				ParentName = p.ParentName,
				state = p.State,
				NodeType = p.NodeType
			}).ToList();
		}
	}
}
