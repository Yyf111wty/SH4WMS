# 部署后 / 应用池回收后执行：把冷启动成本在无人使用时先付掉
# 用法：
#   powershell -ExecutionPolicy Bypass -File Scripts\部署后预热.ps1
#   powershell -ExecutionPolicy Bypass -File Scripts\部署后预热.ps1 -BaseUrl http://10.100.3.236/

param(
    [string]$BaseUrl = "http://localhost/"
)

$ErrorActionPreference = "Continue"
$BaseUrl = $BaseUrl.TrimEnd('/') + '/'

"目标站点: $BaseUrl"
""

# 预检：确认地址可达，避免因为端口或路径写错导致全部 404 却看不出原因
try {
    $probe = Invoke-WebRequest -Uri $BaseUrl -UseBasicParsing -TimeoutSec 60 -MaximumRedirection 3
    "根路径预检: HTTP {0}, {1} 字节" -f $probe.StatusCode, $probe.RawContentLength
}
catch {
    "根路径预检失败: {0}" -f $_.Exception.Message
    "请确认 -BaseUrl 是否包含正确端口/应用程序名，例如："
    "  -BaseUrl http://localhost:8084/"
    "  -BaseUrl http://localhost:8084/TRXWMS_Website/"
    "查看 IIS 实际绑定："
    "  %windir%\system32\inetsrv\appcmd.exe list site"
    "  %windir%\system32\inetsrv\appcmd.exe list vdir"
}
""

$pages = @(
    "/",
    "/Home/Index",
    "/WMS/PutOut/Index",
    "/WMS/PutIn/Index",
    "/WMS/StockerTask/Index",
    "/WMS/Needle/Index",
    "/WMS/Inventory/Index",
    "/WMS/OperationLog/Index",
    "/WMS/Location/Index",
    "/WMS/AgvTask/Index",
    "/WMS/ConveryTask/Index",
    "/WMS/ReportWork/Index",
    "/WMS/Weigh/Index",
    "/WMS/Master/Index",
    "/WMS/Warehouse/Index",
    "/WMS/Zone/Index",
    "/Users/Index",
    "/Roles/Index"
)

$total = 0
$failed = 0

foreach ($page in $pages) {
    $url = ($BaseUrl.TrimEnd('/')) + $page
    $watch = [Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 180 -MaximumRedirection 3
        $watch.Stop()
        $total += $watch.ElapsedMilliseconds
        "{0,-42} {1} {2,7} ms" -f $page, $response.StatusCode, $watch.ElapsedMilliseconds
    }
    catch {
        $watch.Stop()
        $failed++
        "{0,-42} ERROR {1,7} ms  {2}" -f $page, $watch.ElapsedMilliseconds, $_.Exception.Message
    }
}

""
"总计 {0} 个页面，失败 {1} 个，累计 {2} ms" -f $pages.Count, $failed, $total
if ($failed -gt 0) {
    ""
    "提示：失败大多是地址没带端口或站点不在根路径。正确示例："
    "  powershell -ExecutionPolicy Bypass -File .\Scripts\部署后预热.ps1 -BaseUrl http://localhost:8084/"
    "  powershell -ExecutionPolicy Bypass -File .\Scripts\部署后预热.ps1 -BaseUrl http://localhost:8084/TRXWMS_Website/"
}
"说明：第一次执行会较慢（正在编译视图/JIT），之后再访问应稳定在几十毫秒级。"
