$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$cscPath = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
# The tests use C# 6 initializers; prefer the repository's Roslyn compiler.
$roslynPath = Join-Path $repoRoot 'packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.0\tools\RoslynLatest\csc.exe'
if (Test-Path -LiteralPath $roslynPath) { $cscPath = $roslynPath }
$jsonPath = Join-Path $repoRoot 'packages\Newtonsoft.Json.11.0.1\lib\net45\Newtonsoft.Json.dll'
$testOutput = Join-Path ([System.IO.Path]::GetTempPath()) ('NeedleServiceBatchTests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testOutput | Out-Null
$executable = Join-Path $testOutput 'NeedleServiceBatchTests.exe'
try {
    Copy-Item -LiteralPath $jsonPath -Destination $testOutput
    & $cscPath /nologo /target:exe /r:System.Data.dll /r:System.Core.dll "/r:$jsonPath" "/out:$executable" `
        (Join-Path $repoRoot 'TRWMS_BillApi\BILL\WCS\NeedleService.cs') (Join-Path $PSScriptRoot 'NeedleServiceBatchTests.cs')
    if ($LASTEXITCODE -ne 0) { throw 'NeedleService batch test compilation failed.' }
    & $executable
    if ($LASTEXITCODE -ne 0) { throw 'NeedleService batch contract tests failed.' }
}
finally {
    # Remove only the two files created by this run, then the empty directory.
    if (Test-Path -LiteralPath $executable) { Remove-Item -LiteralPath $executable }
    $copiedJson = Join-Path $testOutput 'Newtonsoft.Json.dll'
    if (Test-Path -LiteralPath $copiedJson) { Remove-Item -LiteralPath $copiedJson }
    Remove-Item -LiteralPath $testOutput
}
