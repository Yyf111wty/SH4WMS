using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TR.ORM;

namespace TRXWMS
{
    /// <summary>
    /// 对所有同步 Web API Action 统一压制 ASP.NET SynchronizationContext，
    /// 防止 MySql.Data 8.0.33 在打开/释放连接时等待请求上下文导致死锁。
    /// 异步 Action（返回 Task / Task&lt;T&gt;）跳过整体压制，避免 await 恢复后丢失 HttpContext.Current。
    /// </summary>
    public sealed class SuppressSyncContextWebApiFilter : IActionFilter
    {
        public bool AllowMultiple => false;

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(
            HttpActionContext actionContext,
            CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            if (IsAsyncAction(actionContext))
            {
                return continuation();
            }

            using (SqlSugarClientFactory.SuppressSynchronizationContext())
            {
                return continuation();
            }
        }

        private static bool IsAsyncAction(HttpActionContext actionContext)
        {
            HttpActionDescriptor descriptor = actionContext?.ActionDescriptor;
            if (descriptor == null)
            {
                return false;
            }

            Type returnType = descriptor.ReturnType;
            if (returnType != null && typeof(Task).IsAssignableFrom(returnType))
            {
                return true;
            }

            var reflected = descriptor as ReflectedHttpActionDescriptor;
            if (reflected?.MethodInfo != null && typeof(Task).IsAssignableFrom(reflected.MethodInfo.ReturnType))
            {
                return true;
            }

            return false;
        }
    }
}
