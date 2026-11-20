using SqlSugar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;

namespace TR.ORM
{
    public static class SqlSugarClientFactory
    {
        private static readonly object SyncRoot = new object();
        private static readonly AsyncLocal<RequestScope> AmbientScope = new AsyncLocal<RequestScope>();
        private const string ScopeKey = "__TR_SqlSugar_Scope__";

        private static long _clientsCreated;
        private static long _clientsDisposed;
        private static long _tranAutoRollback;

        /// <summary>
        /// 宿主注入请求级存储（网站传入 HttpContext.Items）。
        /// 返回 null 时回退 AsyncLocal，适用于服务/控制台宿主。
        /// </summary>
        public static Func<IDictionary> ScopeStoreResolver { get; set; }

        /// <summary>单条命令超时（秒），避免慢 SQL 长期占用工作线程。</summary>
        public static int CommandTimeoutSeconds { get; set; } = 30;

        internal sealed class RequestScope
        {
            public SqlSugarClient Client;
            public int TranDepth;
            public DateTime CreatedAt;
        }

        /// <summary>
        /// 可选：宿主程序启动前手动指定连接串。
        /// </summary>
        public static string DB_ConnectionString { get; set; }

        private static IDictionary GetScopeStore()
        {
            if (ScopeStoreResolver == null)
            {
                return null;
            }

            try
            {
                return ScopeStoreResolver();
            }
            catch
            {
                return null;
            }
        }

        private static RequestScope GetCurrentScope()
        {
            IDictionary store = GetScopeStore();
            if (store != null)
            {
                RequestScope scope = store[ScopeKey] as RequestScope;
                if (scope == null)
                {
                    scope = new RequestScope();
                    store[ScopeKey] = scope;
                }

                return scope;
            }

            RequestScope ambient = AmbientScope.Value;
            if (ambient == null)
            {
                ambient = new RequestScope();
                AmbientScope.Value = ambient;
            }

            return ambient;
        }

        public static SqlSugarClient GetInstance()
        {
            RequestScope scope = GetCurrentScope();
            if (scope.Client != null)
            {
                return scope.Client;
            }

            lock (SyncRoot)
            {
                if (scope.Client == null)
                {
                    scope.Client = CreateClient(ResolveConnectionString());
                    scope.CreatedAt = DateTime.Now;
                    Interlocked.Increment(ref _clientsCreated);
                }
            }

            return scope.Client;
        }

        public static void BeginTran()
        {
            RequestScope scope = GetCurrentScope();
            if (scope.Client == null)
            {
                GetInstance();
            }

            if (scope.TranDepth == 0)
            {
                scope.Client.Ado.BeginTran();
            }

            scope.TranDepth++;
        }

        public static void CommitTran()
        {
            RequestScope scope = GetCurrentScope();
            if (scope.TranDepth <= 0)
            {
                throw new InvalidOperationException("当前没有已开启的事务，不能提交。");
            }

            scope.TranDepth--;
            if (scope.TranDepth == 0)
            {
                scope.Client.Ado.CommitTran();
            }
        }

        public static void RollbackTran()
        {
            RequestScope scope = GetCurrentScope();
            if (scope.TranDepth <= 0)
            {
                return;
            }

            scope.TranDepth = 0;
            try
            {
                scope.Client.Ado.RollbackTran();
            }
            catch
            {
            }
        }

        public static void DisposeCurrent()
        {
            RequestScope scope = null;
            IDictionary store = GetScopeStore();
            if (store != null)
            {
                scope = store[ScopeKey] as RequestScope;
                store.Remove(ScopeKey);
            }
            else
            {
                scope = AmbientScope.Value;
                AmbientScope.Value = null;
            }

            if (scope == null || scope.Client == null)
            {
                return;
            }

            if (scope.TranDepth > 0)
            {
                Interlocked.Increment(ref _tranAutoRollback);
                try
                {
                    scope.Client.Ado.RollbackTran();
                }
                catch
                {
                }
                scope.TranDepth = 0;
            }

            try
            {
                scope.Client.Dispose();
            }
            catch
            {
            }
            finally
            {
                Interlocked.Increment(ref _clientsDisposed);
            }
        }

        /// <summary>
        /// 诊断用：created 与 disposed 长期应接近，tranAutoRollback 正常应为 0。
        /// </summary>
        public static string GetDiagnostics()
        {
            long created = Interlocked.Read(ref _clientsCreated);
            long disposed = Interlocked.Read(ref _clientsDisposed);
            long autoRollback = Interlocked.Read(ref _tranAutoRollback);

            int depth = 0;
            try
            {
                depth = GetCurrentScope().TranDepth;
            }
            catch
            {
            }

            return string.Format(
                "created={0}, disposed={1}, alive={2}, tranAutoRollback={3}, currentTranDepth={4}",
                created, disposed, created - disposed, autoRollback, depth);
        }

        public static T ExecuteWithRetry<T>(Func<T> action, int maxAttempts = 2)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (maxAttempts < 1)
            {
                maxAttempts = 1;
            }

            Exception last = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt >= maxAttempts || !IsTransientConnectionError(ex))
                    {
                        throw;
                    }

                    ResetClient();
                    Thread.Sleep(150 * attempt);
                }
            }

            throw last ?? new Exception("数据库操作失败");
        }

        public static void ExecuteWithRetry(Action action, int maxAttempts = 2)
        {
            ExecuteWithRetry(() =>
            {
                action();
                return true;
            }, maxAttempts);
        }

        internal static void ResetClient()
        {
            RequestScope scope = GetCurrentScope();
            if (scope.TranDepth > 0)
            {
                return;
            }

            DisposeCurrent();
        }

        internal static bool IsTransientConnectionError(Exception ex)
        {
            while (ex != null)
            {
                string msg = ex.Message ?? string.Empty;
                if (msg.IndexOf("Connection open error", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("Reading from the stream has failed", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("Unable to connect", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("Connection must be valid", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("Fatal error encountered", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("has gone away", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("broken pipe", StringComparison.OrdinalIgnoreCase) >= 0
                    || msg.IndexOf("Lost connection", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                ex = ex.InnerException;
            }

            return false;
        }

        private static string ResolveConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(DB_ConnectionString))
            {
                return EnhanceConnectionString(DB_ConnectionString.Trim());
            }

            string fromWebsiteConfig = TryGetConnectionStringFromWebsiteWebConfig();
            if (!string.IsNullOrWhiteSpace(fromWebsiteConfig))
            {
                return EnhanceConnectionString(fromWebsiteConfig);
            }

            ConnectionStringSettings local = ConfigurationManager.ConnectionStrings["TRWCSConnection"];
            if (local != null && !string.IsNullOrWhiteSpace(local.ConnectionString))
            {
                return EnhanceConnectionString(local.ConnectionString.Trim());
            }

            throw new InvalidOperationException(
                "未找到数据库连接串。请在 TRXWMS_Website/Web.config 中配置 TRWCSConnection，" +
                "或在当前程序 AppSettings 中设置 WebsiteWebConfigPath 指向该 Web.config 的完整路径。");
        }

        private static string TryGetConnectionStringFromWebsiteWebConfig()
        {
            foreach (string path in GetWebsiteWebConfigCandidates())
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    var map = new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = path
                    };
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                    ConnectionStringSettings cs = config.ConnectionStrings?.ConnectionStrings["TRWCSConnection"];
                    if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
                    {
                        return cs.ConnectionString.Trim();
                    }
                }
                catch
                {
                    // 尝试下一个候选路径
                }
            }

            return null;
        }

        private static IEnumerable<string> GetWebsiteWebConfigCandidates()
        {
            var candidates = new List<string>();

            string configuredPath = ConfigurationManager.AppSettings["WebsiteWebConfigPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath.Trim());
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

            // Website 站点自身（bin 上一级）
            candidates.Add(Path.Combine(baseDir, "..", "Web.config"));

            // 开发环境：BillApi / ServiceAuto 等同仓相对路径
            candidates.Add(Path.Combine(baseDir, "..", "TRXWMS_Website", "Web.config"));
            candidates.Add(Path.Combine(baseDir, "..", "..", "TRXWMS_Website", "Web.config"));
            candidates.Add(Path.Combine(baseDir, "..", "..", "..", "TRXWMS_Website", "Web.config"));

            // 常见部署：BillApi 与 Website 为 IIS 同级站点
            candidates.Add(Path.Combine(baseDir, "..", "..", "TRXWMS_Website", "Web.config"));
            candidates.Add(Path.Combine(baseDir, "..", "Website", "Web.config"));

            return candidates
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string EnhanceConnectionString(string connectionString)
        {
            Dictionary<string, string> dict = ParseConnectionString(connectionString);
            Ensure(dict, "charset", "utf8");
            Ensure(dict, "pooling", "true");
            Ensure(dict, "Minimum Pool Size", "0");
            Ensure(dict, "Maximum Pool Size", "100");
            Ensure(dict, "Connection Lifetime", "300");
            Ensure(dict, "Connection Timeout", "30");
            Ensure(dict, "SslMode", "none");
            Ensure(dict, "AllowPublicKeyRetrieval", "True");
            Ensure(dict, "Convert Zero Datetime", "True");
            Ensure(dict, "Allow Zero Datetime", "True");
            return string.Join(";", dict.Select(kv => kv.Key + "=" + kv.Value));
        }

        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int idx = part.IndexOf('=');
                if (idx > 0)
                {
                    dict[part.Substring(0, idx).Trim()] = part.Substring(idx + 1).Trim();
                }
            }

            return dict;
        }

        private static void Ensure(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
            {
                dict[key] = value;
            }
        }

        private static SqlSugarClient CreateClient(string connectionString)
        {
            var client = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            client.Ado.CommandTimeOut = CommandTimeoutSeconds;
            return client;
        }
    }
}
