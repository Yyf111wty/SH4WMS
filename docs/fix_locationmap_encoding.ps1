$utf8Bom = New-Object System.Text.UTF8Encoding $true
$files = @(
    "TRXWMS_Website\Areas\WMS\Views\LocationMap\Index.cshtml",
    "TRXWMS_Website\Areas\WMS\Views\LocationMap\Canvas.cshtml",
    "TRXWMS_Website\Areas\WMS\Views\LocationMap\RowTable.cshtml"
)
$root = Join-Path $PSScriptRoot ".."
foreach ($rel in $files) {
    $path = Join-Path $root $rel
    if (-not (Test-Path $path)) { continue }
    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        Write-Output "Already UTF8 BOM: $rel"
        continue
    }
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    [System.IO.File]::WriteAllText($path, $text, $utf8Bom)
    Write-Output "UTF8 BOM: $rel"
}
