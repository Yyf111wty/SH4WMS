using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS_Website.Helper
{

    public static class HttpContextHelper
    {
        /// <summary>
        /// 【全项目通用】从 Token 获取当前登录用户名
        /// 任何控制器、任何地方、包括 Areas 都能用
        /// </summary>
        public static string GetCurrentUserName()
        {
            try
            {
                // 从请求头拿 Token
                string token = HttpContext.Current.Request.Headers["Authorization"]?.Replace("Bearer ", "")?.Trim();

                if (string.IsNullOrEmpty(token))
                    return null;

                // 调用你的 Token 解析
                return TokenHelper.GetUserName(token);
            }
            catch
            {
                return null;
            }
        }
    }
}