<#
.SYNOPSIS
  Pins the newest published Jalium release on a minor line, including previews.
.DESCRIPTION
  Requires a common version for every Jalium package referenced through
  JaliumPackageVersion. Checks restore and a serial Release build before changing
  the pin. Historical spike projects with explicit versions keep their baselines.
#>
[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+$')] [string] $ReleaseLine = '26.11',
    [switch] $Check
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    $propsPath = Join-Path $root 'Directory.Build.props'
    $propsText = [System.IO.File]::ReadAllText($propsPath)
    $pinPattern = '(<JaliumPackageVersion\b[^>]*>)([^<]+)(</JaliumPackageVersion>)'
    $pin = [regex]::Match($propsText, $pinPattern)
    if (-not $pin.Success) { throw 'Directory.Build.props has no JaliumPackageVersion pin.' }
    $current = $pin.Groups[2].Value

    # Use NuGet's comparer: preview.10 must sort after preview.9, and a stable
    # release must sort after previews of that same version.
    $dotnetRoot = Split-Path -Parent (Get-Command dotnet -ErrorAction Stop).Source
    $sdkVersion = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve the repository .NET SDK.' }
    $versioningPath = Join-Path $dotnetRoot "sdk/$sdkVersion/Sdks/Microsoft.NET.Sdk/tools/net472/NuGet.Versioning.dll"
    Add-Type -Path $versioningPath

    $packages = @(foreach ($folder in @('src', 'samples', 'tools')) {
        foreach ($project in Get-ChildItem (Join-Path $root $folder) -Filter '*.csproj' -Recurse) {
            [xml] $xml = [System.IO.File]::ReadAllText($project.FullName)
            foreach ($reference in $xml.SelectNodes('//PackageReference')) {
                if ($reference.Include -like 'Jalium.UI*' -and $reference.Version -eq '$(JaliumPackageVersion)') {
                    $reference.Include
                }
            }
        }
    }) | Sort-Object -Unique
    if ($packages.Count -eq 0) { throw 'No Jalium package references use the shared pin.' }

    $index = Invoke-RestMethod 'https://api.nuget.org/v3/index.json'
    $baseAddress = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'
    if (-not $baseAddress) { throw 'NuGet has no package content endpoint.' }
    $candidates = $null
    foreach ($package in $packages) {
        $uri = $baseAddress.TrimEnd('/') + '/' + $package.ToLowerInvariant() + '/index.json'
        $versions = @((Invoke-RestMethod $uri).versions | Where-Object {
            $_ -match ('^' + [regex]::Escape($ReleaseLine) + '\.\d+(?:\.\d+)?(?:-|$)')
        })
        if ($null -eq $candidates) { $candidates = $versions }
        else { $candidates = @($candidates | Where-Object { $_ -in $versions }) }
    }

    $latest = $null
    foreach ($candidate in $candidates) {
        $parsed = [NuGet.Versioning.NuGetVersion]::Parse($candidate)
        if ($null -eq $latest -or $parsed.CompareTo($latest) -gt 0) { $latest = $parsed }
    }
    if ($null -eq $latest) {
        $message = "No common published $ReleaseLine.x version (including previews) for $($packages -join ', '). Pin stays $current."
        if (-not $Check) { throw $message }
        Write-Warning $message
        return
    }
    $next = $latest.ToNormalizedString()
    Write-Host "Jalium: $current -> $next (including previews; $($packages -join ', '))"
    if ($Check -or $latest.CompareTo([NuGet.Versioning.NuGetVersion]::Parse($current)) -le 0) { return }

    # The override validates the candidate while the committed pin stays intact.
    try {
        & dotnet restore FluentJalium.slnx --disable-parallel "-p:JaliumPackageVersion=$next"
        if ($LASTEXITCODE -ne 0) { throw "Restore failed for Jalium $next." }
        foreach ($folder in @('src', 'samples')) {
            foreach ($project in Get-ChildItem (Join-Path $root $folder) -Filter '*.csproj' -Recurse) {
                $assetsPath = Join-Path $project.DirectoryName 'obj/project.assets.json'
                if (-not (Test-Path -LiteralPath $assetsPath)) { continue }
                $assets = [System.IO.File]::ReadAllText($assetsPath) | ConvertFrom-Json
                foreach ($library in $assets.libraries.PSObject.Properties) {
                    if ($library.Name -like 'Jalium.UI*/*' -and ($library.Name -split '/')[-1] -ne $next) {
                        throw "Mixed Jalium versions in $assetsPath : $($library.Name), expected $next."
                    }
                }
            }
        }
        & dotnet build FluentJalium.slnx -c Release --no-restore --maxcpucount:1 "-p:JaliumPackageVersion=$next"
        if ($LASTEXITCODE -ne 0) { throw "Release build failed for Jalium $next." }
    }
    catch {
        # Restore the assets matching the unchanged pin after a failed attempt.
        & dotnet restore FluentJalium.slnx --disable-parallel
        if ($LASTEXITCODE -ne 0) { Write-Warning "Restoring the original $current assets also failed." }
        throw
    }

    $encoding = [System.Text.UTF8Encoding]::new($false)
    $updated = $propsText.Substring(0, $pin.Groups[2].Index) + $next + $propsText.Substring($pin.Groups[2].Index + $pin.Groups[2].Length)
    [System.IO.File]::WriteAllText($propsPath, $updated, $encoding)
    foreach ($name in @('AGENTS.md', 'README.md')) {
        $path = Join-Path $root $name
        $text = [System.IO.File]::ReadAllText($path)
        [System.IO.File]::WriteAllText($path, $text.Replace("**$current**", "**$next**"), $encoding)
    }
    Write-Host "Pinned Jalium $next. Run the Astra behavior and pixel gates before claiming runtime parity."
}
finally {
    Pop-Location
}
