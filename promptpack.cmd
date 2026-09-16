@rem Repo-local development launcher for the PromptPack CLI.
@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PROJECT=%SCRIPT_DIR%src\PromptPack.Cli\PromptPack.Cli.csproj"

dotnet run --project "%PROJECT%" --no-restore -v:q -- %*
exit /b %ERRORLEVEL%
