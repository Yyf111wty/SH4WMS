using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using TRWMS_PrApi.Models;
using TRWMS_Website.ViewModels;

namespace TRXWMS_Website.Helper
{
    public static class UserPermissionCache
    {
        private const string PermissionsSessionKey = "__UserPermissions";
        private const string PermissionUserSessionKey = "__UserPermissionsUser";
        private const string CacheKeyPrefix = "__UserPermissions_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 强制重新加载权限（登录时调用）：结果写入进程内缓存，并尽力镜像到 Session。
        /// </summary>
        public static List<Rows> LoadPermissions(HttpSessionStateBase session, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new List<Rows>();
            }

            var permissions = TRWMS_PrApi.Controllers.LoginController.TRWCS_GetUserPermissionDataApi(userName, string.Empty);
            var rows = MapToRows(permissions);

            HttpRuntime.Cache.Insert(
                BuildCacheKey(userName),
                rows,
                null,
                DateTime.Now.Add(CacheDuration),
                Cache.NoSlidingExpiration);

            TryStoreSession(session, userName, rows);
            return rows;
        }

        /// <summary>
        /// 读取权限：优先进程内缓存，其次 Session，最后回源。
        /// 进程内缓存不受 Session 锁影响，多个页面并行请求不会互相等待。
        /// </summary>
        public static List<Rows> GetPermissions(HttpSessionStateBase session, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new List<Rows>();
            }

            var cached = HttpRuntime.Cache.Get(BuildCacheKey(userName)) as List<Rows>;
            if (cached != null)
            {
                return cached;
            }

            var fromSession = TryReadSession(session, userName);
            if (fromSession != null)
            {
                HttpRuntime.Cache.Insert(
                    BuildCacheKey(userName),
                    fromSession,
                    null,
                    DateTime.Now.Add(CacheDuration),
                    Cache.NoSlidingExpiration);
                return fromSession;
            }

            return LoadPermissions(session, userName);
        }

        public static void Clear(HttpSessionStateBase session, string userName = null)
        {
            string targetUser = userName;

            if (string.IsNullOrWhiteSpace(targetUser) && session != null)
            {
                try
                {
                    targetUser = session[PermissionUserSessionKey] as string;
                }
                catch
                {
                }
            }

            if (!string.IsNullOrWhiteSpace(targetUser))
            {
                HttpRuntime.Cache.Remove(BuildCacheKey(targetUser));
            }

            if (session == null)
            {
                return;
            }

            try
            {
                session.Remove(PermissionsSessionKey);
                session.Remove(PermissionUserSessionKey);
            }
            catch
            {
                // 只读 Session 模式下清理失败不影响登录态
            }
        }

        private static string BuildCacheKey(string userName)
        {
            return CacheKeyPrefix + (userName ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static void TryStoreSession(HttpSessionStateBase session, string userName, List<Rows> rows)
        {
            if (session == null)
            {
                return;
            }

            try
            {
                session[PermissionUserSessionKey] = userName;
                session[PermissionsSessionKey] = rows;
            }
            catch
            {
                // Session 被标记为只读时忽略，权限仍可从进程内缓存获取
            }
        }

        private static List<Rows> TryReadSession(HttpSessionStateBase session, string userName)
        {
            if (session == null)
            {
                return null;
            }

            try
            {
                var cachedUser = session[PermissionUserSessionKey] as string;
                var cached = session[PermissionsSessionKey] as List<Rows>;
                if (cached != null
                    && cachedUser != null
                    && string.Equals(cachedUser, userName, StringComparison.OrdinalIgnoreCase))
                {
                    return cached;
                }
            }
            catch
            {
            }

            return null;
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
