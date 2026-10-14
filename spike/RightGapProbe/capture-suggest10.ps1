<#
.SYNOPSIS
  Start the RightGapProbe suggestion-list mode and PrintWindow it twice: once with the suggestion list open,
  once with the combo box dropped in the same card.

.DESCRIPTION
  The layout readings say the suggestion surface is 374 DIP tall; only a frame can say whether that is what
  paints. The probe holds each state for twenty-five seconds and prints a one-word state marker to stdout, so
  the two grabs below are timed against a known schedule rather than against a guess.
#>
[CmdletBinding()]
param(
    [string] $Text = 'a',
    [string] $OutDir = ''
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrEmpty($OutDir)) { $OutDir = Join-Path $PSScriptRoot 'out' }
$grab = Join-Path (Split-Path $PSScriptRoot) 'VisualQA\capture-pid-windows.ps1'
$exe = Join-Path $PSScriptRoot 'bin\Debug\net10.0-windows\RightGapProbe.exe'
if (-not (Test-Path $exe)) { throw "build the probe first: $exe" }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$process = Start-Process -FilePath $exe -ArgumentList @('--open', 'suggest10', "text=$Text") -PassThru
try {
    # 20 pumps to lay the window out, 40 more to let the drop-down realize its rows, then a 25 s hold.
    Start-Sleep -Seconds 14
    & $grab -Name RightGapProbe -Out (Join-Path $OutDir "suggest10-$Text-surface")
    Start-Sleep -Seconds 22
    & $grab -Name RightGapProbe -Out (Join-Path $OutDir 'suggest10-combo-surface')
    Wait-Process -Id $process.Id -Timeout 90
}
finally {
    if (-not $process.HasExited) { $process.Kill() }
}
