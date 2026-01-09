@echo off
REM Wrapper to start GuestFlow.Api with required environment variables for local dev
SET ASPNETCORE_ENVIRONMENT=Development
SET JWT__SecretKey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
SET JWT__MinimumKeyLength=64
SET ConnectionStrings__DefaultConnection=Server=HPLAPTOP\SQLEXPRESS;Database=GuestFlowDb;User Id=sa;Password=StrongPass123!;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=True;
REM Enable demo data seeding for local e2e runs
SET SeedDemoData=true
cd /d %~dp0
REM Redirect stdout/stderr to log file for inspection
if not exist C:\temp\guestflow-logs mkdir C:\temp\guestflow-logs
dotnet run --no-launch-profile --project . --urls http://localhost:5146 > C:\temp\guestflow-logs\api_run.log 2>&1

