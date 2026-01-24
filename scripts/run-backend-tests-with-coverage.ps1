# GuestFlow Backend Test Coverage Script
# Runs backend tests with code coverage reporting

param(
    [string]$OutputFormat = "opencover,cobertura,json",
    [string]$OutputPath = "./coverage/backend-coverage",
    [int]$Threshold = 0
)

Write-Host "🧪 Running backend tests with code coverage..." -ForegroundColor Cyan

# Create coverage directory if it doesn't exist
$coverageDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
}

# Run tests with coverage
$testResult = dotnet test `
    GuestFlow.Application.Tests/GuestFlow.Application.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --settings:GuestFlow.Application.Tests/coverlet.runsettings `
    --results-directory:$coverageDir `
    --logger:"trx;LogFileName=test-results.trx" `
    --logger:"console;verbosity=normal"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Tests passed!" -ForegroundColor Green
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path $coverageDir -Recurse -Filter "coverage.*" | Where-Object { $_.Extension -in @('.xml', '.json') }
    
    if ($coverageFiles.Count -gt 0) {
        Write-Host "📊 Coverage reports generated:" -ForegroundColor Cyan
        foreach ($file in $coverageFiles) {
            Write-Host "   - $($file.FullName)" -ForegroundColor Gray
        }
        
        # Try to parse and display coverage summary (if cobertura format exists)
        $coberturaFile = $coverageFiles | Where-Object { $_.Name -like "*cobertura*" } | Select-Object -First 1
        if ($coberturaFile) {
            Write-Host "`n📈 Coverage Summary:" -ForegroundColor Cyan
            # Note: Detailed parsing would require XML parsing, but basic info is shown
            Write-Host "   Coverage report: $($coberturaFile.FullName)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "❌ Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`n💡 Tip: Open coverage/index.html in a browser to view detailed coverage report" -ForegroundColor Yellow
