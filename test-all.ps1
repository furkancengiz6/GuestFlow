# GuestFlow Comprehensive Testing Script (PowerShell-safe ASCII version)
# NOTE: This file intentionally avoids emojis to prevent Windows PowerShell encoding/parser issues.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Invoke-Step {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][scriptblock]$Action
    )

    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan

    & $Action

    if ($LASTEXITCODE -ne $null -and $LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE"
    }
}

Write-Host "GuestFlow Testing Suite Starting..." -ForegroundColor Green

try {
    Set-Location $root

    Invoke-Step "BACKEND BUILD (Release)" {
        dotnet build .\GuestFlow.Api --configuration Release --verbosity minimal
    }

    Invoke-Step "BACKEND TESTS (dotnet test)" {
        # Run the full solution tests so integration tests (WebApplicationFactory) are included.
        dotnet test .\GuestFlow.sln --configuration Release --verbosity minimal
    }

    Invoke-Step "FRONTEND BUILD" {
        Push-Location .\GuestFlow.Frontend
        try {
            npm run build
        } finally {
            Pop-Location
        }
    }

    Invoke-Step "FRONTEND UNIT TESTS (Jest)" {
        Push-Location .\GuestFlow.Frontend
        try {
            npm test -- --watchAll=false --passWithNoTests
        } finally {
            Pop-Location
        }
    }

    # E2E can be slow/flaky depending on environment (browsers, running backend, etc.).
    # Opt-in via RUN_E2E=true (or 1/yes) to keep the default "test all" flow reliable for dev/CI.
    $runE2e = ($env:RUN_E2E -as [string])
    $runE2e = if ($runE2e) { $runE2e.Trim().ToLower() } else { "" }

    if ($runE2e -in @("1","true","yes","y")) {
        Invoke-Step "E2E TESTS (Playwright)" {
            Push-Location .\GuestFlow.Frontend
            try {
                npm run test:e2e
            } finally {
                Pop-Location
            }
        }
    } else {
        Write-Host ""
        Write-Host "=== E2E TESTS (Playwright) ===" -ForegroundColor Cyan
        Write-Host "Skipped. Set RUN_E2E=true to enable." -ForegroundColor DarkGray
    }

    # Optional performance tests (only if k6 exists)
    Write-Host ""
    Write-Host "=== PERFORMANCE TESTS (optional) ===" -ForegroundColor Cyan
    if (Get-Command k6 -ErrorAction SilentlyContinue) {
        $env:BASE_URL = "http://localhost:5146"
        k6 run ..\tests\performance\load-test.js --vus 5 --duration 15s
        if ($LASTEXITCODE -ne 0) {
            throw "Performance tests failed with exit code $LASTEXITCODE"
        }
    } else {
        Write-Host "k6 not installed, skipping performance tests." -ForegroundColor DarkGray
    }

    Write-Host ""
    Write-Host "Testing completed successfully." -ForegroundColor Green
    exit 0
}
catch {
    Write-Host ""
    Write-Host "Testing failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}