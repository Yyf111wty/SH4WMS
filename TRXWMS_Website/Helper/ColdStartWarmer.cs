using System;
using System.IO;
using System.Web.Compilation;
using System.Web.Hosting;
using TR.ORM;
using TR.ORM.Model;

namespace TRXWMS_Website.Helper
{
    /// <summary>
    /// 冷启动预热：应用池回收/重启后，用户第一次访问需要承担
    /// 视图编译、MVC 管道 JIT、实体映射和首次数据库连接的开销（实测 8~23 秒）。
    /// 这里在后台线程提前把这些成本做掉，用户首击直接命中已预热的应用。
    /// 全部操作都吞掉异常，任何失败都不影响站点正常启动。
    /// </summary>
    public static class ColdStartWarmer
    {
        private static int _started;

        public static void WarmUpInBackground()
        {
            if (System.Threading.Interlocked.Exchange(ref _started, 1) == 1)
            {
                return;
            }

            System.Threading.ThreadPool.QueueUserWorkItem(_ => Run());
        }

        private static void Run()
        {
            try
            {
                WarmUpDatabase();
            }
            catch
            {
            }

            PrecompileViews();

            try
            {
                SqlSugarClientFactory.DisposeCurrent();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 预热 SqlSugar 与连接池，避免第一个用户请求承担建连和实体映射成本。
        /// </summary>
        public static void WarmUpDatabase()
        {
            var db = SugarContext.GetInstance();
            db.Ado.GetInt("SELECT 1");

            // 覆盖首页、清单页、任务页高频访问的实体，让 ORM 提前完成映射
            db.Queryable<wms_bill_putoutheader>().Where(x => x.ID == string.Empty).Take(1).ToList();
            db.Queryable<wms_bill_putinbindingheader>().Where(x => x.ID == string.Empty).Take(1).ToList();
            db.Queryable<wms_stocker_task>().Where(x => x.ID == string.Empty).Take(1).ToList();
            db.Queryable<wms_base_location>().Where(x => x.ID == string.Empty).Take(1).ToList();
            db.Queryable<wms_needle_inventory>().Where(x => x.ID == string.Empty).Take(1).ToList();
            db.Queryable<sys_permissions>().Where(x => x.NodeId == -1).Take(1).ToList();
            db.Queryable<sys_rolepermission>().Where(x => x.RoleId == -1).Take(1).ToList();
            db.Queryable<sys_users>().Where(x => x.Id == -1).Take(1).ToList();

            // 预热列表页真实使用的 SQL 形态，让 MySQL 提前完成解析与执行计划
            db.Ado.SqlQuery<wms_bill_putoutheader>(
                "SELECT h.* FROM wms_bill_putoutheader h WHERE h.CREATETIME >= DATE_SUB(NOW(), INTERVAL 30 DAY) ORDER BY h.CREATETIME DESC LIMIT 0, 30");
            db.Ado.GetInt("SELECT COUNT(1) FROM wms_bill_putoutheader h WHERE h.CREATETIME >= DATE_SUB(NOW(), INTERVAL 30 DAY)");
            db.Ado.SqlQuery<wms_bill_putinbindingheader>(
                "SELECT h.* FROM wms_bill_putinbindingheader h WHERE h.CREATETIME >= DATE_SUB(NOW(), INTERVAL 30 DAY) ORDER BY h.CREATETIME DESC LIMIT 0, 30");
            db.Ado.GetInt("SELECT COUNT(1) FROM wms_bill_putinbindingheader h WHERE h.CREATETIME >= DATE_SUB(NOW(), INTERVAL 30 DAY)");
            db.Ado.SqlQuery<wms_stocker_task>(
                "SELECT * FROM wms_stocker_task ORDER BY CREATETIME DESC LIMIT 0, 30");
            db.Ado.GetInt("SELECT COUNT(1) FROM wms_stocker_task");
            db.Ado.SqlQuery<wms_needle_inventory>(
                "SELECT * FROM wms_needle_inventory WHERE ENABLED = 1 ORDER BY CREATETIME DESC LIMIT 0, 30");
            db.Ado.GetInt("SELECT COUNT(1) FROM wms_needle_inventory WHERE ENABLED = 1");
        }

        /// <summary>
        /// 预编译所有 Razor 视图。首次访问页面时的编译耗时（每个页面 0.2~1 秒）
        /// 会由这里的后台任务承担，用户不再等待。
        /// 高频页面优先编译，保证用户最常打开的页面最先就绪。
        /// </summary>
        public static void PrecompileViews()
        {
            string[] priorityViews =
            {
                "~/Views/Login/Index.cshtml",
                "~/Views/Home/Index.cshtml",
                "~/Areas/WMS/Views/PutOut/Index.cshtml",
                "~/Areas/WMS/Views/PutIn/Index.cshtml",
                "~/Areas/WMS/Views/StockerTask/Index.cshtml",
                "~/Areas/WMS/Views/Needle/Index.cshtml",
                "~/Areas/WMS/Views/Inventory/Index.cshtml",
                "~/Areas/WMS/Views/OperationLog/Index.cshtml",
                "~/Areas/WMS/Views/Location/Index.cshtml",
                "~/Areas/WMS/Views/AgvTask/Index.cshtml",
                "~/Areas/WMS/Views/ConveryTask/Index.cshtml",
                "~/Areas/WMS/Views/ReportWork/Index.cshtml",
                "~/Areas/WMS/Views/Weigh/Index.cshtml",
                "~/Areas/WMS/Views/Master/Index.cshtml",
                "~/Areas/WMS/Views/Warehouse/Index.cshtml",
                "~/Areas/WMS/Views/Zone/Index.cshtml"
            };

            for (int i = 0; i < priorityViews.Length; i++)
            {
                CompileView(priorityViews[i]);
            }

            string root;
            try
            {
                root = HostingEnvironment.MapPath("~/");
            }
            catch
            {
                return;
            }

            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(root, "*.cshtml", SearchOption.AllDirectories);
            }
            catch
            {
                return;
            }

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (IsSkippedPath(file))
                {
                    continue;
                }

                string virtualPath;
                try
                {
                    virtualPath = "~/" + file.Substring(root.Length).Replace('\\', '/');
                }
                catch
                {
                    continue;
                }

                CompileView(virtualPath);
            }
        }

        private static void CompileView(string virtualPath)
        {
            try
            {
                BuildManager.GetCompiledType(virtualPath);
            }
            catch
            {
                // 单个视图编译失败不影响其他视图和站点运行
            }
        }

        private static bool IsSkippedPath(string file)
        {
            return file.IndexOf(@"\bin\", StringComparison.OrdinalIgnoreCase) >= 0
                || file.IndexOf(@"\obj\", StringComparison.OrdinalIgnoreCase) >= 0
                || file.IndexOf(@"\App_Data\", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
