#!/usr/bin/env pwsh
# Harness feedback loop: build gates, (optionally) boot the API, run gates.
# Exit code 0 = all gates green. Non-zero = at least one gate red.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$gatesSln = Join-Path $root "gates/Gates.sln"
$apiProject = Join-Path $root "src/Api/Api.csproj"

Write-Host "== Gate: dotnet build (gates) ==" -ForegroundColor Cyan
dotnet build $gatesSln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Gate build failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

$apiProcess = $null
if (Test-Path $apiProject) {
    Write-Host "== Booting API for functional gates ==" -ForegroundColor Cyan
    $apiProcess = Start-Process dotnet `
        -ArgumentList "run", "--project", $apiProject, "--urls", "http://localhost:5087" `
        -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 5
} else {
    Write-Host "src/Api not found - functional gates (AC-xx) will fail as expected (nothing implemented yet)." -ForegroundColor Yellow
}

Write-Host "== Gate: dotnet test (gates) ==" -ForegroundColor Cyan
$env:GATE_API_BASE_URL = "http://localhost:5087"
dotnet test $gatesSln --logger "console;verbosity=normal"
$testExit = $LASTEXITCODE

if ($apiProcess) {
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
}

if ($testExit -eq 0) {
    Write-Host "All gates green." -ForegroundColor Green
} else {
    Write-Host "At least one gate is red." -ForegroundColor Red
}

exit $testExit
