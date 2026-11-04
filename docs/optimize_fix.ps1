$ErrorActionPreference = 'Stop'
$src = 'c:\Users\40115\Desktop\旺宏.docx'
function ApplyStyle($para,[int]$styleId,[string]$txt) {
    $para.Range.Text = $txt
    $para.Range.Style = $styleId
    $para.Range.Font.Name = 'Microsoft YaHei'
    $para.Range.ParagraphFormat.SpaceAfter = 6
}
function ApplyNormal($para,[string]$txt) {
    ApplyStyle $para (-1) $txt
    $para.Range.Font.Size = 10.5
}
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
    $doc = $word.Documents.Open($src)
    $del = New-Object System.Collections.Generic.List[int]
    for ($i = 1; $i -le $doc.Paragraphs.Count; $i++) {
        $p = $doc.Paragraphs.Item($i)
        if ($p.Range.InlineShapes.Count -gt 0) { continue }
        $raw = ($p.Range.Text -replace '[\r\n]+$','').Trim()
        if (-not $raw) { continue }
        if ($raw.StartsWith('|')) { $del.Add($i) | Out-Null; continue }
        if ($raw -match '^3\.1') { ApplyStyle $p (-3) '3.1  入库清单'; continue }
        if ($raw -match '^3\.2') { ApplyStyle $p (-3) '3.2  出库清单'; continue }
        if ($raw -match '^3\.3') { ApplyStyle $p (-3) '3.3  库存报表'; continue }
        if ($raw -match '^3\.4') { ApplyStyle $p (-3) '3.4  堆垛机任务'; continue }
        if ($raw -match '^3\.5') { ApplyStyle $p (-3) '3.5  巷道开关管理'; continue }
        if ($raw -match '^5\.1') { ApplyStyle $p (-3) '5.1  用户管理'; continue }
        if ($raw -match '^5\.2') { ApplyStyle $p (-3) '5.2  角色管理'; continue }
        if ($raw -match '^5\.3') { ApplyStyle $p (-3) '5.3  角色权限'; continue }
        if ($raw -match 'WCS' -and $raw.Length -gt 60) { ApplyNormal $p $raw; continue }
        try {
            if ($p.Range.Style.NameLocal -match '标题 3' -and $raw.Length -gt 50) {
                ApplyNormal $p $raw
            }
        } catch {}
    }
    [array]::Reverse($del)
    foreach ($idx in $del) {
        if ($idx -le $doc.Paragraphs.Count) {
            $doc.Paragraphs.Item($idx).Range.Delete() | Out-Null
        }
    }
    $doc.Save()
    $doc.Close()
    Write-Output 'FIX OK'
} finally {
    $word.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
}
