#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$src = 'c:\Users\40115\Desktop\旺宏.docx'
$backup = 'c:\Users\40115\Desktop\旺宏_备份.docx'
Copy-Item $src $backup -Force

$wdStyleHeading1 = -2
$wdStyleHeading2 = -3
$wdStyleHeading3 = -4
$wdStyleHeading4 = -5
$wdStyleNormal = -1
$wdStyleListBullet = 2
$wdStyleListNumber = 3

function Clean-MarkdownText([string]$t) {
    if ([string]::IsNullOrWhiteSpace($t)) { return $t }
    $t = $t.Trim()
    $t = $t -replace '^#+\s*', ''
    $t = $t -replace '^>\s*', ''
    $t = $t -replace '^\*\*(.+?)\*\*$', '$1'
    $t = $t -replace '\*\*', ''
    $t = $t -replace '`', ''
    $t = $t -replace '^---+$', ''
    $t = $t -replace '^```+.*$', ''
    $t = $t -replace '^/\s*$', ''
    $t = $t -replace '^-\s*', ''
    return $t.Trim()
}

function Set-ParaStyle($para, $styleId, [string]$text) {
    $para.Range.Text = $text
    $para.Range.set_Style($styleId)
    $para.Range.Font.Name = '微软雅黑'
    $para.Range.Font.NameFarEast = '微软雅黑'
    $para.Range.ParagraphFormat.SpaceAfter = 6
    $para.Range.ParagraphFormat.SpaceBefore = 0
    $para.Range.ParagraphFormat.LineSpacingRule = 0
}

function Set-ParaNormal($para, [string]$text) {
    Set-ParaStyle $para $wdStyleNormal $text
    $para.Range.Font.Size = 10.5
}

function Is-ImagePara($para) {
    return $para.Range.InlineShapes.Count -gt 0
}

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
    $doc = $word.Documents.Open($src)
    $doc.Content.Font.Name = '微软雅黑'
    $doc.Content.Font.NameFarEast = '微软雅黑'
    $doc.Content.Font.Size = 10.5

    # 收集需删除的段落索引（markdown 表格分隔行等）
    $toDelete = New-Object System.Collections.Generic.List[int]

    for ($i = 1; $i -le $doc.Paragraphs.Count; $i++) {
        $p = $doc.Paragraphs.Item($i)
        if (Is-ImagePara $p) { continue }

        $raw = ($p.Range.Text -replace '[\r\n]+$','').Trim()
        if (-not $raw) { continue }

        # 删除 markdown 表格分隔行、空分隔符
        if ($raw -match '^\|[\s\-:|]+\|$' -or $raw -eq '---' -or $raw -eq '```' -or $raw -eq '/') {
            $toDelete.Add($i) | Out-Null
            continue
        }

        $text = Clean-MarkdownText $raw

        # ===== 一级标题 =====
        if ($raw -match '^##\s*1\.') { Set-ParaStyle $p $wdStyleHeading1 '1  系统简介'; continue }
        if ($raw -match '^##\s*2\.') { Set-ParaStyle $p $wdStyleHeading1 '2  快速登录与界面说明'; continue }
        if ($raw -match '^##\s*4\.\s*核心') { Set-ParaStyle $p $wdStyleHeading1 '3  核心业务操作'; continue }
        if ($raw -match '^##\s*5\.\s*基础') { Set-ParaStyle $p $wdStyleHeading1 '4  基础数据维护'; continue }
        if ($raw -match '^##\s*6\.\s*系统') { Set-ParaStyle $p $wdStyleHeading1 '5  系统管理'; continue }
        if ($raw -match '^##\s*8\.\s*典型') { Set-ParaStyle $p $wdStyleHeading1 '6  典型业务场景速查'; continue }
        if ($raw -match '^##\s*9\.\s*状态') { Set-ParaStyle $p $wdStyleHeading1 '7  状态码对照表'; continue }
        if ($raw -match '^##\s*10\.\s*常见') { Set-ParaStyle $p $wdStyleHeading1 '8  常见问题（FAQ）'; continue }

        # ===== 二级标题 =====
        if ($raw -match '^###\s*2\.1') { Set-ParaStyle $p $wdStyleHeading2 '2.1  登录'; continue }
        if ($raw -match '^###\s*4\.1') { Set-ParaStyle $p $wdStyleHeading2 '3.1  入库清单'; continue }
        if ($raw -match '^###\s*4\.2') { Set-ParaStyle $p $wdStyleHeading2 '3.2  出库清单'; continue }
        if ($raw -match '^###\s*4\.3') { Set-ParaStyle $p $wdStyleHeading2 '3.3  库存报表'; continue }
        if ($raw -match '^###\s*4\.4') { Set-ParaStyle $p $wdStyleHeading2 '3.4  堆垛机任务'; continue }
        if ($raw -match '^###\s*4\.9') { Set-ParaStyle $p $wdStyleHeading2 '3.5  巷道开关管理'; continue }
        if ($raw -match '^###\s*5\.3') { Set-ParaStyle $p $wdStyleHeading2 '4.1  库位管理'; continue }
        if ($raw -match '^###\s*6\.1') { Set-ParaStyle $p $wdStyleHeading2 '5.1  用户管理'; continue }
        if ($raw -match '^###\s*6\.3') { Set-ParaStyle $p $wdStyleHeading2 '5.2  角色管理'; continue }
        if ($raw -match '^###\s*6\.4') { Set-ParaStyle $p $wdStyleHeading2 '5.3  角色权限'; continue }
        if ($raw -match '^###\s*场景\s*A') { Set-ParaStyle $p $wdStyleHeading2 '6.1  物料入库'; continue }
        if ($raw -match '^###\s*场景\s*B') { Set-ParaStyle $p $wdStyleHeading2 '6.2  物料出库'; continue }
        if ($raw -match '^###\s*场景\s*C') { Set-ParaStyle $p $wdStyleHeading2 '6.3  任务卡住排查'; continue }
        if ($raw -match '^###\s*场景\s*D') { Set-ParaStyle $p $wdStyleHeading2 '6.4  新用户上岗'; continue }
        if ($raw -match '^###\s*入库/出库') { Set-ParaStyle $p $wdStyleHeading2 '7.1  入库/出库单据状态'; continue }
        if ($raw -match '^###\s*堆垛机任务类型') { Set-ParaStyle $p $wdStyleHeading2 '7.2  堆垛机任务类型'; continue }
        if ($raw -match '^###\s*有效标识') { Set-ParaStyle $p $wdStyleHeading2 '7.3  有效标识'; continue }

        # ===== 三级标题 =====
        if ($raw -match '^####\s*功能说明') { Set-ParaStyle $p $wdStyleHeading3 '功能说明'; continue }
        if ($raw -match '^####\s*查询条件') { Set-ParaStyle $p $wdStyleHeading3 '查询条件'; continue }
        if ($raw -match '^####\s*状态说明') { Set-ParaStyle $p $wdStyleHeading3 '状态说明'; continue }
        if ($raw -match '^####\s*操作流程') { Set-ParaStyle $p $wdStyleHeading3 '操作流程'; continue }
        if ($raw -match '^####\s*库位状态') { Set-ParaStyle $p $wdStyleHeading3 '库位状态'; continue }
        if ($raw -match '^####\s*使用建议') { Set-ParaStyle $p $wdStyleHeading3 '使用建议'; continue }
        if ($raw -match '^####\s*操作建议') { Set-ParaStyle $p $wdStyleHeading3 '操作建议'; continue }
        if ($raw -match '^####\s*操作步骤') { Set-ParaStyle $p $wdStyleHeading3 '操作步骤'; continue }
        if ($raw -match '^####\s*主要字段') { Set-ParaStyle $p $wdStyleHeading3 '主要字段'; continue }
        if ($raw -match '^####\s*常见操作') { Set-ParaStyle $p $wdStyleHeading3 '常见操作'; continue }

        # ===== 文档标题与元信息 =====
        if ($raw -match '^#\s*旺宏') {
            $p.Range.Text = '旺宏·综合物流管理系统（WMS）操作手册'
            $p.Range.set_Style($wdStyleHeading1)
            $p.Range.Font.Size = 18
            $p.Range.Font.Bold = $true
            $p.Range.ParagraphFormat.Alignment = 1
            continue
        }
        if ($raw -match '^>\s*版本') { Set-ParaNormal $p '版本：PM1018　　适用对象：仓库操作员、班组长、系统管理员'; continue }
        if ($raw -match '^>\s*适用对象') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^>\s*文档说明') { Set-ParaNormal $p '说明：按钮以角色权限配置为准，不同账号可见按钮可能不同。'; continue }

        # ===== 合并 markdown 表格行为简洁查询条件 =====
        if ($raw -match '^\|\s*字段\s*\|') {
            Set-ParaNormal $p '库区、单据编号、载具编号、分配库位、物料编号/批次、请求入库标识、状态、有效标识、时间范围'
            continue
        }
        if ($raw -match '^\|\s*库区\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*单据编号\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*载具编号\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*分配库位\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*物料编号') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*请求入库') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*状态\s*\|' -and $raw -notmatch '状态值') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*有效标识\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*时间范围\s*\|') { $toDelete.Add($i) | Out-Null; continue }

        # 状态码表格 → 单行
        if ($raw -match '^\|\s*状态值\s*\|') {
            Set-ParaNormal $p '0 待执行　|　1 正在执行　|　2 已完成　|　3 已装盘　|　5 异常结束'
            continue
        }
        if ($raw -match '^\|\s*0\s*\|\s*待执行\s*\|' -and $raw -notmatch '失效') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*1\s*\|\s*正在执行') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*2\s*\|\s*已完成') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*3\s*\|\s*已装盘') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*5\s*\|\s*异常') { $toDelete.Add($i) | Out-Null; continue }

        if ($raw -match '^\|\s*状态\s*\|\s*含义\s*\|' -and $raw -notmatch '状态值') {
            Set-ParaNormal $p '空库位（无货）　|　空托盘（有空载具）　|　有库存　|　冻结站台（不可用）'
            continue
        }
        if ($raw -match '^\|\s*空库位\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*空托盘\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*有库存\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*冻结站台\s*\|') { $toDelete.Add($i) | Out-Null; continue }

        if ($raw -match '^\|\s*值\s*\|\s*含义\s*\|') {
            # 由后续行判断，先标记删除表头
            $toDelete.Add($i) | Out-Null
            continue
        }
        if ($raw -match '^\|\s*1\s*\|\s*入库\s*\|') {
            Set-ParaNormal $p '1 入库　|　2 出库　|　3 移库'
            continue
        }
        if ($raw -match '^\|\s*2\s*\|\s*出库\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*3\s*\|\s*移库\s*\|') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^\|\s*0\s*\|\s*失效\s*\|') {
            Set-ParaNormal $p '0 失效　|　1 有效'
            continue
        }
        if ($raw -match '^\|\s*1\s*\|\s*有效\s*\|') { $toDelete.Add($i) | Out-Null; continue }

        # ===== 简化查询条件列表 =====
        if ($raw -match '^-\s*库位编号、库位状态') {
            Set-ParaNormal $p '库位编号、库位状态、载具编号、单据编号、排/列/层/巷道、时间范围、预警天数（默认7天）'
            continue
        }
        if ($raw -match '^-\s*排 / 列 / 层 / 巷道' -or $raw -match '^-\s*时间范围' -or $raw -match '^\*\*预警天数') { $toDelete.Add($i) | Out-Null; continue }
        if ($raw -match '^-\s*单据编号、载具编号$') {
            Set-ParaNormal $p '单据编号、载具编号、库区、任务起点/终点、任务类型（入库/出库/移库）、任务状态'
            continue
        }
        if ($raw -match '^-\s*库区（101-1' -or $raw -match '^-\s*任务起点' -or $raw -match '^\*\*任务类型' -or $raw -match '^\*\*任务状态') { $toDelete.Add($i) | Out-Null; continue }

        # ===== 登录账号格式化 =====
        if ($raw -match '管理员：admin' -or ($raw -match 'admin123' -and $raw -notmatch '用户')) {
            Set-ParaNormal $p '测试账号：管理员 admin / admin123　　普通用户 user / 123456'
            continue
        }
        if ($raw -match '用户：user' -or $raw -match '123456') { $toDelete.Add($i) | Out-Null; continue }

        # ===== 常用操作 bullets =====
        if ($raw -match '打开功能') { Set-ParaNormal $p '· 打开功能：点击左侧菜单，在 Tab 中打开页面'; continue }
        if ($raw -match '关闭 Tab') { Set-ParaNormal $p '· 关闭 Tab：点击 Tab 上的 ×'; continue }
        if ($raw -match '修改密码') { Set-ParaNormal $p '· 修改密码：右上角用户菜单 → 修改密码'; continue }
        if ($raw -match '退出') { Set-ParaNormal $p '· 退出：右上角 → 退出'; continue }

        # ===== 按钮功能列表格式化 =====
        if ($text -eq '按钮功能：') { Set-ParaStyle $p $wdStyleHeading3 '按钮功能'; continue }
        if ($text -match '清空库位') { Set-ParaNormal $p '· 清空库位：清空库位数据并还原为空闲状态。须在未入库完成或已执行「清空托盘物料」后操作，否则会产生多余库存。'; continue }
        if ($text -match '清空托盘物料') { Set-ParaNormal $p '· 清空托盘物料：删除该托盘在系统中的库存记录。'; continue }
        if ($text -match '冻结库位') { Set-ParaNormal $p '· 冻结库位：冻结后无法入库；库位有物料时不支持此操作，可通过激活状态搜索冻结数据。'; continue }
        if ($text -match '解封库位') { Set-ParaNormal $p '· 解封库位：解除冻结，恢复正常入库、出库。'; continue }
        if ($text -match '复位库位') { Set-ParaNormal $p '· 复位库位：需使用 SAP 账号操作，将异常数据恢复至初始状态（恢复前确认堆垛机任务已处理完成）。'; continue }

        # ===== FAQ 格式化 =====
        if ($raw -match '^\*\*Q1') { Set-ParaNormal $p 'Q1：登录后左侧菜单为空？'; $p.Range.Font.Bold = $true; continue }
        if ($raw -match '^A：当前用户未分配') { Set-ParaNormal $p 'A：请联系管理员在「角色权限」中配置页面权限。'; continue }
        if ($raw -match '^\*\*Q2') { Set-ParaNormal $p 'Q2：看不到「新增」「下发」等按钮？'; $p.Range.Font.Bold = $true; continue }
        if ($raw -match '^A：按钮级权限') { Set-ParaNormal $p 'A：需在「权限维护」中配置按钮，并在「角色权限」中勾选。'; continue }
        if ($raw -match '^\*\*Q3') { Set-ParaNormal $p 'Q3：入库单一直「待执行」？'; $p.Range.Font.Bold = $true; continue }
        if ($raw -match '^A：检查是否已') { Set-ParaNormal $p 'A：检查是否已请求入库、WCS 是否在线、巷道是否关闭、库位是否可用。'; continue }
        if ($raw -match '^\*\*Q4') { Set-ParaNormal $p 'Q4：库存与实物不符？'; $p.Range.Font.Bold = $true; continue }
        if ($raw -match '^A：查报工信息') { Set-ParaNormal $p 'A：查报工信息及堆垛机任务是否异常；必要时由管理员在库位管理中处理。'; continue }
        if ($raw -match '^\*\*Q5') { Set-ParaNormal $p 'Q5：如何限定用户只看某个库区？'; $p.Range.Font.Bold = $true; continue }
        if ($raw -match '^A：使用专用账号') { Set-ParaNormal $p 'A：使用专用账号（ycl/bcp/cp/lck），系统自动过滤对应库区。'; continue }

        # ===== 其余正文清理 markdown =====
        if ($text -and $text -ne $raw) {
            Set-ParaNormal $p $text
        } elseif ($text) {
            $p.Range.Font.Name = '微软雅黑'
            $p.Range.Font.Size = 10.5
            $p.Range.ParagraphFormat.SpaceAfter = 6
        }
    }

    # 倒序删除冗余段落
    [array]::Reverse($toDelete)
    foreach ($idx in $toDelete) {
        if ($idx -le $doc.Paragraphs.Count) {
            $doc.Paragraphs.Item($idx).Range.Delete() | Out-Null
        }
    }

    # 页面边距
    $doc.PageSetup.TopMargin = $word.CentimetersToPoints(2.54)
    $doc.PageSetup.BottomMargin = $word.CentimetersToPoints(2.54)
    $doc.PageSetup.LeftMargin = $word.CentimetersToPoints(2.8)
    $doc.PageSetup.RightMargin = $word.CentimetersToPoints(2.8)

    $doc.Save()
    $doc.Close()
    Write-Output 'OK: optimized document saved'
}
finally {
    $word.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
