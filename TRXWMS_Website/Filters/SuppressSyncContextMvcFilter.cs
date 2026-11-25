using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using TR.ORM;

namespace TRWMS_Website.Filters
{
    /// <summary>
    /// 对所有同步 MVC Action 统一压制 ASP.NET SynchronizationContext，
    /// 防止 MySql.Data 8.0.33 在打开/释放连接时等待请求上下文导致死锁。
    /// 异步 Action 跳过整体压制，避免 await 恢复后丢失 HttpContext.Current。
    /// </summary>
    public sealed class SuppressSyncContextMvcFilter : ActionFilterAttribute
    {
        private const string ScopeStackKey = "__TR_SuppressSyncContext_Stack__";

        public SuppressSyncContextMvcFilter()
        {
            // MVC 5 要求 FilterAttribute.Order >= -1；使用默认 Order = -1 并在 FilterConfig 中首位注册，
            // 可确保在同级全局 Filter（如操作日志快照查询）与 Action 本体之前进入、之后恢复。
            Order = -1;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext == null || IsAsyncAction(filterContext.ActionDescriptor))
            {
                return;
            }

            var items = filterContext.HttpContext.Items;
            var stack = items[ScopeStackKey] as Stack<IDisposable>;
            if (stack == null)
            {
                stack = new Stack<IDisposable>(2);
                items[ScopeStackKey] = stack;
            }

            // 即使嵌套（例如子 Action）返回 null 也压栈，保证与 OnActionExecuted 严格配对。
            stack.Push(SqlSugarClientFactory.SuppressSynchronizationContext());
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext?.HttpContext == null || IsAsyncAction(filterContext.ActionDescriptor))
            {
                return;
            }

            var items = filterContext.HttpContext.Items;
            var stack = items[ScopeStackKey] as Stack<IDisposable>;
            if (stack == null || stack.Count == 0)
            {
                return;
            }

            IDisposable scope = stack.Pop();
            if (stack.Count == 0)
            {
                items.Remove(ScopeStackKey);
            }

            scope?.Dispose();
        }

        private static bool IsAsyncAction(ActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return false;
            }

            if (descriptor is AsyncActionDescriptor)
            {
                return true;
            }

            var reflected = descriptor as ReflectedActionDescriptor;
            if (reflected?.MethodInfo != null && typeof(Task).IsAssignableFrom(reflected.MethodInfo.ReturnType))
            {
                return true;
            }

            return false;
        }
    }
}
