Add-Type -AssemblyName System.Windows.Forms
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class Scr {
  [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr h);
  [DllImport("gdi32.dll")] public static extern int GetDeviceCaps(IntPtr dc, int index);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
}
'@
[void][Scr]::SetProcessDPIAware()
$scr = [System.Windows.Forms.Screen]::PrimaryScreen
Write-Host "bounds=$($scr.Bounds) work=$($scr.WorkingArea) device=$($scr.DeviceName)"
$dc = [Scr]::GetDC([IntPtr]::Zero)
Write-Host "horzdpi=$([Scr]::GetDeviceCaps($dc,88)) logpixels=$([Scr]::GetDeviceCaps($dc,8)) physw=$([Scr]::GetDeviceCaps($dc,110)) physh=$([Scr]::GetDeviceCaps($dc,111))"
