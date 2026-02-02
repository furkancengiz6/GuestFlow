@echo off
echo ====================================
echo GuestFlow Demo Launcher
echo ====================================
echo.
echo Starting demo environment...
echo.

REM Set environment variables
set ASPNETCORE_ENVIRONMENT=Development
set SeedDemoData=true

echo [1/3] Starting Backend with Demo Data...
echo.
echo IMPORTANT: Save the demo user credentials from the console output!
echo.

start "GuestFlow API" cmd /k "cd /d %~dp0GuestFlow.Api && dotnet run"

timeout /t 10 /nobreak

echo [2/3] Waiting for database seeding to complete...
timeout /t 15 /nobreak

echo [3/3] Starting Frontend...
start "GuestFlow Frontend" cmd /k "cd /d %~dp0GuestFlow.Frontend && npm run dev"

echo.
echo ====================================
echo Demo environment is launching!
echo ====================================
echo.
echo Please wait for services to start:
echo - Backend API: http://localhost:5146
echo - Swagger UI: http://localhost:5146/swagger
echo - Frontend: http://localhost:5173
echo.
echo IMPORTANT: Check the Backend terminal for demo user credentials!
echo.
pause
