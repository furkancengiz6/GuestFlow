@echo off
echo ========================================
echo GuestFlow Testing Suite Starting...
echo ========================================

echo Running PowerShell test runner...
powershell -ExecutionPolicy Bypass -File "%~dp0test-all.ps1"
exit /b %ERRORLEVEL%