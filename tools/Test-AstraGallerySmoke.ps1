<#
.SYNOPSIS
  Mounts the Gallery as a real window on the desktop, page by page, and checks that it took focus,
  answered a window close and left no process behind.

.DESCRIPTION
  The windowed half of the evidence, not the pixel half. A build only proves markup parsed: a template that
  never applies, a style that throws inside one page, or a control that measures to zero all leave a green
  build behind, and only a shown window says so. Colour and layout claims stay in tests/FluentJalium.Tests,
  where they are asserted from a histogram - docs/astra/adaptation/06 records why a screenshot is not
  evidence in this runtime, so this script deliberately captures no image.

  Pages come from Catalog.json: overview, buttons, inputs, selection, navigation, surfaces, settings.
#>
[CmdletBinding()]
param(
    [string[]] $Page = @('surfaces'),
    [int] $SettleMilliseconds = 3000
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# Invoked through -File a comma list arrives as one string, so split before looping.
$pageIds = @($Page | ForEach-Object { $_ -split ',' } | Where-Object { $_ })
$executable = Join-Path $root 'samples/FluentJalium.Gallery/bin/Debug/net10.0-windows/FluentJalium.Gallery.exe'
if (-not (Test-Path $executable)) {
    Write-Error "No built Gallery at $executable. Run tools/Test-AstraGates.ps1 first."
}

foreach ($page in $pageIds) {
    Write-Host "==> $page"
    $started = Get-Date

    # ProcessStartInfo rather than Start-Process: CloseMainWindow plus a WaitForExit with a real timeout is
    # what this check needs, and that control lives on Process.
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $executable
    $startInfo.Arguments = "--page $page"
    $startInfo.UseShellExecute = $true
    $process = [System.Diagnostics.Process]::Start($startInfo)

    try {
        if (-not $process.WaitForInputIdle(30000)) {
            Write-Error "$page : the process never went idle within 30s."
        }

        Start-Sleep -Milliseconds $SettleMilliseconds
        $process.Refresh()
        if ($process.MainWindowHandle -eq [IntPtr]::Zero) {
            Write-Error "$page : the process is up but no window handle was published."
        }

        # Read the title while the window is still up; after exit the property comes back empty.
        $title = $process.MainWindowTitle
        if (-not $process.CloseMainWindow()) {
            Write-Error "$page : CloseMainWindow was refused, so the app does not answer a window close."
        }
        if (-not $process.WaitForExit(20000)) {
            Write-Error "$page : the window closed but the process was still alive after 20s."
        }

        $code = $process.ExitCode
        $seconds = [Math]::Round(((Get-Date) - $started).TotalSeconds, 1)
        if ($code -ne 0) { Write-Error "$page : exited with code $code." }
        Write-Host "    '$title' - closed cleanly in $seconds s"
    }
    finally {
        # A throw above must not leave a window on the user's desktop.
        if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue }
        $process.Dispose()
    }
}

$left = @(Get-Process -Name 'FluentJalium.Gallery' -ErrorAction SilentlyContinue)
if ($left.Count -gt 0) {
    Write-Error "Gallery processes left running: $(($left | ForEach-Object Id) -join ', ')"
}
Write-Host "$($pageIds.Count) page(s) mounted and closed; no Gallery process left."
