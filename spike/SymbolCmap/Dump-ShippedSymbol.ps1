<#
.SYNOPSIS
  Dumps the Symbol enum members and codepoints from the assembly the tests actually run against.
.DESCRIPTION
  The roadmap's cmap entry was read out of the sibling source tree, which may be newer than the pinned
  26.10.9 runtime. The shipped package's XML documentation is a 143-byte stub, so the only authority for
  "what is in the enum we ship" is the assembly itself. The DLL is copied out of the test output folder
  first: a test run holds it open, and a read that fails mid-suite reads like a missing type.
#>
param(
    [string] $Dll = 'tests/FluentJalium.Tests/bin/Debug/net10.0-windows/Jalium.UI.Controls.dll',
    [string] $Out = 'spike/SymbolCmap/out/symbol-shipped.tsv'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$copy = Join-Path $PSScriptRoot 'out\Jalium.UI.Controls.copy.dll'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $copy) | Out-Null
Copy-Item (Join-Path $root $Dll) $copy -Force
$assembly = [Reflection.Assembly]::LoadFrom($copy)
$type = $assembly.GetType('Jalium.UI.Controls.Symbol')
if (-not $type) { Write-Output 'TYPE MISSING'; exit 1 }

$values = [Enum]::GetValues($type)
$tab = [string][char]9
$rows = foreach ($value in $values) {
    [string]$value + $tab + [string][int]$value
}
$target = Join-Path $root $Out
Set-Content -Path $target -Value $rows -Encoding ascii
Write-Output ("members=" + $values.Count)
Write-Output ("wrote " + $target)
