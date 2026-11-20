using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

namespace TRXWMS_Website.Helper
{
    internal static class SlowRequestLogger
    {
        private static readonly object SyncRoot = new object();

        public static long GetThresholdMilliseconds()
        {
            int seconds;
            if (int.TryParse(ConfigurationManager.AppSettings["SlowRequestLogThresholdSeconds"], out seconds)
                && seconds > 0)
            {
                return seconds * 1000L;
            }

            return 10000L;
        }

        public static void Write(HttpContext context, long elapsedMilliseconds, Exception exception)
        {
            if (context == null || elapsedMilliseconds < GetThresholdMilliseconds())
            {
                return;
            }

            try
            {
                string root = context.Server.MapPath("~/App_Data/SlowRequests");
                Directory.CreateDirectory(root);

                HttpRequest request = context.Request;
                HttpResponse response = context.Response;
                string filePath = Path.Combine(root, "slow-requests-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                string line = string.Join(" | ", new[]
                {
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    "elapsedMs=" + elapsedMilliseconds,
                    "status=" + response.StatusCode,
                    "method=" + (request.HttpMethod ?? string.Empty),
                    "url=" + (request.Url == null ? string.Empty : request.Url.PathAndQuery),
                    "client=" + (request.UserHostAddress ?? string.Empty),
                    "user=" + (context.User == null || context.User.Identity == null ? string.Empty : context.User.Identity.Name),
                    "thread=" + System.Threading.Thread.CurrentThread.ManagedThreadId,
                    "worker=" + Environment.WorkingSet,
                    "exception=" + (exception == null ? string.Empty : exception.GetType().FullName + ": " + exception.Message)
                }) + Environment.NewLine;

                lock (SyncRoot)
                {
                    File.AppendAllText(filePath, line, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}
