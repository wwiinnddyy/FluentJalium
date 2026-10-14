<#
.SYNOPSIS
  Capture every Gallery page through the blank-frame gate, one process per page.

.DESCRIPTION
  capture-pages.ps1 walks the pages too, but it gates on WaitForInputIdle and skips a page that does not
  answer it, which loses pages silently. This sweep calls grab-page.ps1 once per page id from Catalog.json,
  so every page either produces a file or reports the attempts it spent looking for a painted frame. The
  pictures are a starting point for measurement, not a conclusion - pair them with count-colors.ps1 or a
  crop before claiming anything about a spacing value.
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [string] $Only = '',
    [int] $SettleMilliseconds = 9000
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '../..')
$gallery = Join-Path $root "samples/FluentJalium.Gallery/bin/$Configuration/net10.0-windows/FluentJalium.Gallery.exe"
if (-not (Test-Path $gallery)) { throw "no gallery exe at $gallery" }
$out = Join-Path $PSScriptRoot 'out/pages'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$catalog = Get-Content -Raw (Join-Path $root 'samples/FluentJalium.Gallery/Catalog.json') | ConvertFrom-Json
$ids = @($catalog.pages | ForEach-Object { $_.id })
if ($Only) { $ids = @($Only -split ',') }
foreach ($id in $ids) {
    $target = Join-Path $out "page-$id.png"
    Write-Host "== $id"
    & (Join-Path $PSScriptRoot 'grab-page.ps1') -Exe $gallery -Arguments "--page $id" `
        -Out $target -SettleMilliseconds $SettleMilliseconds -Attempts 10
}
Write-Host "captured $($ids.Count) pages -> $out"
