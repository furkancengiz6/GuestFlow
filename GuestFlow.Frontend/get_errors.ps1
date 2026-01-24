$json = Get-Content lint_errors_v3.json | ConvertFrom-Json
foreach ($file in $json) {
    $filePath = $file.filePath
    foreach ($msg in $file.messages) {
        if ($msg.severity -eq 2) {
            Write-Host "FILE: $filePath"
            Write-Host "LINE: $($msg.line)"
            Write-Host "RULE: $($msg.ruleId)"
            Write-Host "MSG : $($msg.message)"
            Write-Host "-------------------"
        }
    }
}
