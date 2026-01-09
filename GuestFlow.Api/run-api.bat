@echo off
REM Wrapper to start GuestFlow.Api with required environment variables for local dev
SET ASPNETCORE_ENVIRONMENT=Development
SET JWT__SecretKey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
SET JWT__MinimumKeyLength=64
REM Use environment-provided connection string if available, otherwise fall back to a local trusted connection for dev.
IF DEFINED ConnectionStrings__DefaultConnection (
  ECHO Using ConnectionStrings__DefaultConnection from environment
) ELSE (
  ECHO No ConnectionStrings__DefaultConnection provided — falling back to localhost Trusted Connection for dev.
  SET ConnectionStrings__DefaultConnection=Server=localhost;Database=GuestFlowDb;Trusted_Connection=True;MultipleActiveResultSets=True;
)
REM Enable demo data seeding for local e2e runs
SET SeedDemoData=true
cd /d %~dp0
REM Redirect stdout/stderr to log file for inspection
if not exist C:\temp\guestflow-logs mkdir C:\temp\guestflow-logs
dotnet run --no-launch-profile --project . --urls http://localhost:5146 > C:\temp\guestflow-logs\api_run.log 2>&1

