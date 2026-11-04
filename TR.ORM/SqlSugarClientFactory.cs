using SqlSugar;
using System;
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
        private static SqlSugarClient _client;

        /// <summary>
        /// 可选：宿主程序启动前手动指定连接串。
        /// </summary>
        public static string DB_ConnectionString { get; set; }

        public static SqlSugarClient GetInstance()
        {
            if (_client != null)
            {
                return _client;
            }

            lock (SyncRoot)
            {
                if (_client == null)
                {
                    _client = CreateClient(ResolveConnectionString());
                }
            }

            return _client;
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
            lock (SyncRoot)
            {
                if (_client == null)
                {
                    return;
                }

                try
                {
                    _client.Dispose();
                }
                catch
                {
                    // ignore dispose errors during recovery
                }

                _client = null;
            }
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
            return new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }
    }
}
