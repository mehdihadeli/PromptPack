# Repo-local development launcher for the PromptPack CLI.
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $PromptPackArgs
)

$project = Join-Path $PSScriptRoot "src/PromptPack.Cli/PromptPack.Cli.csproj"
dotnet run --project $project --no-restore -v:q -- @PromptPackArgs
exit $LASTEXITCODE
