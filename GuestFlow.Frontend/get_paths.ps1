$json = Get-Content lint_errors_v2.json | ConvertFrom-Json
$errorFiles = @()
foreach ($file in $json) {
    foreach ($msg in $file.messages) {
        if ($msg.severity -eq 2) {
            $errorFiles += $file.filePath
            break
        }
    }
}
$errorFiles | Select-Object -Unique
