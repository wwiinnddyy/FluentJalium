<#
.SYNOPSIS
  Runs the Astra verification sequence serially, in the order that fails fastest.
.DESCRIPTION
  Orchestration only. The assertions themselves live in tests/FluentJalium.Tests, so a
  developer who skips this script still gets the same gates from `dotnet test`.

  Builds must not run in parallel: concurrent project builds lock shared Jalium obj files.
  Stale testhost and Gallery processes hold FluentJalium*.dll in the output folders, which
  makes the next build fail with MSB3021/MSB3027 and read like a compile error.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')] [string] $Configuration = 'Debug',
    [switch] $SkipPalette
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

foreach ($name in 'testhost', 'FluentJalium.Gallery', 'FluentJalium.Tests') {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force
    if (Get-Process -Name $name -ErrorAction SilentlyContinue) {
        Write-Error "Process '$name' is still holding output files; close it manually."
    }
}

function Invoke-Step([string] $title, [scriptblock] $body) {
    Write-Host "==> $title"
    & $body
    if ($LASTEXITCODE -ne 0) { Write-Error "$title failed with exit code $LASTEXITCODE" }
}

Push-Location $root
try {
    Invoke-Step 'restore' { dotnet restore FluentJalium.slnx }
    Invoke-Step "build ($Configuration)" { dotnet build FluentJalium.slnx -c $Configuration --no-restore }
    Invoke-Step 'test suite (structure, resource keys, theme runtime)' {
        dotnet test tests/FluentJalium.Tests -c $Configuration --no-build --no-restore
    }
    if (-not $SkipPalette) {
        Write-Host '==> palette drift'
        & (Join-Path $PSScriptRoot 'Sync-AstraPalette.ps1') -Check
        if ($LASTEXITCODE -ne 0) { Write-Error 'Palette drift gate failed.' }
    }
    Write-Host 'All Astra gates passed.'
}
finally {
    Pop-Location
}
