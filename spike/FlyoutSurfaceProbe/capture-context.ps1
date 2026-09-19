<#
.SYNOPSIS
  Hold the probe's context menu open, PrintWindow the host, and leave the frame on disk for a corner reading.

.DESCRIPTION
  The shared xunit host cannot realize a grafted popup with a size (measured: the surface Border reads 0x0
  there while the same call gives 74.27x70 in a window the probe shows itself), so the arc of the menu surface is
  read from a real frame instead. Run this, then point spike/VisualQA/scan-line.ps1 at the rows near the card's
  top-left corner.
#>
[CmdletBinding()]
param(
    [string] $Mode = 'context',
    [int] $Settle = 14,
    [string] $Out = ''
)

$ErrorActionPreference = 'Stop'
if (-not $Out) { $Out = Join-Path $PSScriptRoot 'out' }
$exe = Join-Path $PSScriptRoot 'bin\Debug\net10.0-windows\FlyoutSurfaceProbe.exe'
$grab = Join-Path (Split-Path $PSScriptRoot) 'VisualQA\capture-pid-windows.ps1'
if (-not (Test-Path $exe)) { throw "build the probe first: $exe" }
New-Item -ItemType Directory -Force -Path $Out | Out-Null

$process = Start-Process -FilePath $exe -ArgumentList $Mode -PassThru
try {
    Start-Sleep -Seconds $Settle
    # -Out is a filename PREFIX in capture-pid-windows.ps1, so the directory has to be inside it.
    & $grab -Name 'FlyoutSurfaceProbe' -Out (Join-Path $Out $Mode)
}
finally {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
}

Get-ChildItem $Out -Filter '*.png' | Sort-Object LastWriteTime -Descending | Select-Object -First 4 |
    ForEach-Object { "  $($_.Name)" }
