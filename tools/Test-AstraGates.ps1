<#
.SYNOPSIS
  Runs the Astra verification sequence serially, in the order that fails fastest.
.DESCRIPTION
  Orchestration only. The assertions themselves live in tests/FluentJalium.Tests, so a
  developer who skips this script still gets the same gates from `dotnet test`. The one
  exception is the gallery page pixel gate, which is a process of its own (tools/AstraPagePixels)
  because it cannot run inside the shared test host - see the step below for why.

  Builds must not run in parallel: concurrent project builds lock shared Jalium obj files.
  Stale testhost and Gallery processes hold FluentJalium*.dll in the output folders, which
  makes the next build fail with MSB3021/MSB3027 and read like a compile error.

  Only processes whose executable lives under this repository are stopped, and that is deliberate:
  `testhost.exe` is the name every .NET test run on this machine uses, so stopping one by name can
  kill a build belonging to a project this script has no business touching.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')] [string] $Configuration = 'Debug',
    [switch] $SkipPalette
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

foreach ($name in 'testhost', 'FluentJalium.Gallery', 'FluentJalium.Tests', 'AstraPagePixels') {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -and $_.Path.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
    } | Stop-Process -Force
    $held = Get-Process -Name $name -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -and $_.Path.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
    }
    if ($held) {
        Write-Error "Process '$name' (pid $($held.Id -join ', ')) is still holding output files; close it manually."
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

    # The page-level pixel gate runs as its own process on purpose: rendering the Gallery inside the shared
    # test host leaves each page's dropdown subtree grafted in that host's overlay layer, and the suite
    # resolves popup parts by name from the same window (docs/astra/ROADMAP.md, "#10 缺口②").
    # Restored on its own because the probe project is deliberately outside FluentJalium.slnx.
    Invoke-Step "build page pixel gate ($Configuration)" {
        dotnet build tools/AstraPagePixels -c $Configuration
    }
    Invoke-Step 'gallery page pixels (13 pages x light, dark, high contrast)' {
        & (Join-Path $root "tools\AstraPagePixels\bin\$Configuration\net10.0-windows\AstraPagePixels.exe")
    }
    if (-not $SkipPalette) {
        Write-Host '==> palette drift'
        & (Join-Path $PSScriptRoot 'Sync-AstraPalette.ps1') -Check
        if ($LASTEXITCODE -ne 0) { Write-Error 'Palette drift gate failed.' }

        # The inventory document is generated, so staleness is the failure mode and -Check is the assertion.
        # The suite cross-reads the same files with its own parser; this step is what proves the document
        # itself was regenerated, which a test in the suite cannot see.
        Write-Host '==> public resource key inventory'
        & (Join-Path $PSScriptRoot 'Report-AstraResourceKeys.ps1') -Check
        if ($LASTEXITCODE -ne 0) { Write-Error 'Public resource key inventory is stale.' }
    }

    # Same shape as the key inventory: the document is generated from prose, so its staleness is the failure mode.
    # Independent of -SkipPalette because it reads docs, not the palette.
    Write-Host '==> known gap inventory'
    & (Join-Path $PSScriptRoot 'Report-AstraKnownGaps.ps1') -Check
    if ($LASTEXITCODE -ne 0) { Write-Error 'Known gap inventory is stale.' }
    Write-Host 'All Astra gates passed.'
}
finally {
    Pop-Location
}
