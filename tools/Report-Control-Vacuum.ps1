<#
.SYNOPSIS
  Reports which controls still have no Fluent appearance ("the vacuum"), scoped by ModernWpf.
.DESCRIPTION
  Three read-only sources:
    1. spike/ControlCensus output (docs/astra/adaptation/01-census-raw-output.txt) - the native
       Jalium public concrete Control types, with the census' own OnRender/OnPaint classification;
    2. the implicit styles we declare (any <Style> without x:Key, under src/**/*.jalxaml);
    3. the scope authority: ../ModernWpf's two layers - ModernWpf/Styles/*.xaml (Fluent style for a
       control the host already has) and ModernWpf.Controls/<Name>/ (a WinUI-derived type the host
       lacks). That is the same split this repository is being restructured to.
  Writes docs/astra/adaptation/05-native-control-vacuum.md. Re-run after adding any style.
#>
[CmdletBinding()]
param(
    [string]$ModernWpfRoot,
    [string]$Census,
    [string]$Out
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Xml.Linq

# $PSScriptRoot is still empty while a param default expression runs, so the defaults resolve here.
if (-not $ModernWpfRoot) { $ModernWpfRoot = Join-Path $PSScriptRoot '../../ModernWpf' }
if (-not $Census) { $Census = Join-Path $PSScriptRoot '../docs/astra/adaptation/01-census-raw-output.txt' }
if (-not $Out) { $Out = Join-Path $PSScriptRoot '../docs/astra/adaptation/05-native-control-vacuum.md' }
if (-not [System.IO.File]::Exists($Census)) { throw "Missing census output: $Census (build and run spike/ControlCensus first)" }
if (-not [System.IO.Directory]::Exists($ModernWpfRoot)) { throw "Missing ModernWpf reference checkout: $ModernWpfRoot" }

# 1. native types + how they draw.
$types = @{}
foreach ($line in [System.IO.File]::ReadAllLines($Census)) {
    $m = [regex]::Match($line, '^\[TYPE\] (?<name>\S+)\s+style=\S+ render=(?<render>\S+) paint=(?<paint>\S+)')
    if ($m.Success) { $types[$m.Groups['name'].Value] = (@($m.Groups['render'].Value, $m.Groups['paint'].Value) -contains 'True') }
}

# 2. our implicit styles: a <Style> with no x:Key is implicit for its TargetType.
$ns = [System.Xml.Linq.XNamespace]'http://schemas.jalium.ui/2024'
$xamlNs = [System.Xml.Linq.XNamespace]'http://schemas.microsoft.com/winfx/2006/xaml'
$implicit = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($file in Get-ChildItem -Path (Split-Path -Parent $PSScriptRoot) -Filter *.jalxaml -Recurse) {
    foreach ($style in [System.Xml.Linq.XDocument]::Load($file.FullName).Root.Descendants($ns + 'Style')) {
        if ($style.Attribute($xamlNs + 'Key')) { continue }
        $target = $style.Attribute('TargetType').Value
        if (-not $target) { continue }
        $short = ([regex]::Replace($target, '^\{x:Type\s+(?<t>[A-Za-z0-9_.]+)\}$', '${t}')) -split '\.' | Select-Object -Last 1
        [void]$implicit.Add(($short -split ':')[-1])
    }
}

# 3. scope: what ModernWpf committed to covering, in the two layers it used.
$nonControl = @('Common', 'Properties', 'Themes', 'CornerRadius', 'TextStyles', 'Resources', 'Strings', 'Primitives')
$styled = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($file in Get-ChildItem -Path (Join-Path $ModernWpfRoot 'ModernWpf/Styles') -Filter *.xaml) {
    if ($nonControl -notcontains $file.BaseName) { [void]$styled.Add($file.BaseName) }
}
$own = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($dir in Get-ChildItem -Path (Join-Path $ModernWpfRoot 'ModernWpf.Controls') -Directory) {
    if ($nonControl -notcontains $dir.Name) { [void]$own.Add($dir.Name) }
}
$scope = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($name in @($styled) + @($own)) { [void]$scope.Add($name) }

$native = @($scope | Where-Object { $types.ContainsKey($_) })
$gaps = @($native | Where-Object { -not $implicit.Contains($_) })
$done = @($native | Where-Object { $implicit.Contains($_) })
$needsType = @($scope | Where-Object { -not $types.ContainsKey($_) })
$extra = @($types.Keys | Where-Object { -not $scope.Contains($_) -and -not $implicit.Contains($_) })
$styledOffScope = @($implicit | Where-Object { $types.ContainsKey($_) -and -not $scope.Contains($_) })
# A gap is only real if nothing of ours stands in for it. Our own types are named Fluent<Control>,
# so a suffixed name means the native control was replaced rather than styled.
function Test-Substitute([string]$name) { @($implicit | Where-Object { $_ -ne $name -and $_ -like "*$name" }).Count -gt 0 }
$hardGaps = @($gaps | Where-Object { -not (Test-Substitute $_) })
$substituted = @($gaps | Where-Object { Test-Substitute $_ })

function Quote([string[]]$items) { ($items | ForEach-Object { "'" + $_ + "'" }) -join ' ' }

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('# 原生控件真空清单（Fluent 外观覆盖到哪一步）')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('由 `tools/Report-Control-Vacuum.ps1` 生成，**勿手改**。范围权威是 ModernWpf 的两层：')
[void]$sb.AppendLine('``ModernWpf/Styles/*.xaml``（宿主已有控件的 Fluent 样式）与 ``ModernWpf.Controls/<Name>/``（宿主没有的 WinUI 派生类型）。')
[void]$sb.AppendLine('原生类型与绘制分级来自 ``01-census-raw-output.txt``。')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('| 量 | 数 |')
[void]$sb.AppendLine('|---|---|')
[void]$sb.AppendLine("| Jalium public 具体 ``Control`` 类型 | $($types.Count) |")
[void]$sb.AppendLine("| ModernWpf 承诺覆盖的控件名（并集） | $($scope.Count) |")
[void]$sb.AppendLine("| 其中 Jalium 有同名原生类型（可走样式层） | $($native.Count) |")
[void]$sb.AppendLine("| 其中我们已声明隐式样式 | $($done.Count) |")
[void]$sb.AppendLine("| **其中真空，且无自有类型顶替** | **$($hardGaps.Count)** |")
[void]$sb.AppendLine("| 其中真空但已有自有类型顶替 | $($substituted.Count) |")
[void]$sb.AppendLine("| ModernWpf 得自己写类型、Jalium 也无同名原生类型 | $($needsType.Count) |")
[void]$sb.AppendLine("| Jalium 独有、不在这份范围内（只标注，不还原） | $($extra.Count) |")
[void]$sb.AppendLine("| 我们已样式、但 ModernWpf 无同名文件（按别名/内联实现） | $($styledOffScope.Count) |")
[void]$sb.AppendLine('')
[void]$sb.AppendLine("## A. 真空：范围内、Jalium 有原生类型、我们还没有样式（$($hardGaps.Count)）")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('绘制方式列只说明**能不能**靠模板解决；排批顺序仍以"底座先于叶子"为准（见 ROADMAP D 节）。')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('| 控件 | 绘制方式 | ModernWpf 放在哪一层 |')
[void]$sb.AppendLine('|---|---|---|')
foreach ($name in $hardGaps) {
    $drawn = if ($types[$name]) { '自绘，模板只能改外围' } else { '纯模板，可完全覆盖' }
    $layer = @()
    if ($styled.Contains($name)) { $layer += 'Styles' }
    if ($own.Contains($name)) { $layer += 'Controls' }
    [void]$sb.AppendLine("| $name | $drawn | $($layer -join ' + ') |")
}
[void]$sb.AppendLine('')
[void]$sb.AppendLine("## B. 已上隐式样式（$($done.Count)）")
[void]$sb.AppendLine('')
[void]$sb.AppendLine((Quote $done))
if ($substituted.Count) {
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine("## C. 真空但已有自有类型顶替（$($substituted.Count)）")
    [void]$sb.AppendLine('')
    foreach ($name in $substituted) {
        [void]$sb.AppendLine("- $name  ←  " + (Quote @($implicit | Where-Object { $_ -ne $name -and $_ -like "*$name" })))
    }
}
[void]$sb.AppendLine('')
[void]$sb.AppendLine("## D. Jalium 无同名原生类型（$($needsType.Count)）")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('补样式解决不了：要么自有类型，要么明确放弃。名单里含 ModernWpf 的工程目录名（非控件），保留不筛。')
[void]$sb.AppendLine('')
[void]$sb.AppendLine((Quote $needsType))
[void]$sb.AppendLine('')
[void]$sb.AppendLine("## E. Jalium 独有、这份范围不覆盖（$($extra.Count)）")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('不做 Fluent 还原，但**它们会露出框架素外观**：Gallery 里出现任何一个都必须显式标"未样式化"，')
[void]$sb.AppendLine('这也是把 Gallery 当成唯一回归面的原因。')
[void]$sb.AppendLine('')
[void]$sb.AppendLine((Quote $extra))
[void]$sb.AppendLine('')
[void]$sb.AppendLine('## 这份清单证明不了什么')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('- 只统计**声明**。声明的隐式样式是否真的落到原生控件像素上仍未证：')
[void]$sb.AppendLine('  ``00-pixel-harness-raw-output.txt`` 的归因实验里，覆盖我们的令牌后原生 Button 的像素不动。')
[void]$sb.AppendLine('- 绘制方式列来自 census 的 ``OnRender``/``OnPaint`` 判定，不区分读 DP 还是读死的 ``ThemeColors``；')
[void]$sb.AppendLine('  后者见 ``02-render-ceiling.md``。')
[void]$sb.AppendLine('- 范围以 ModernWpf 为**文件/目录名**为锚，所以按别名或内联在父控件文件里的条目样式会漏计')
[void]$sb.AppendLine('  （``TabItem``/``ComboBoxItem``/``MenuFlyoutItem`` 这类条目容器就是假阴性来源）。下面这份')
[void]$sb.AppendLine('  "已样式但名单没有"的差集是人工核对入口，不要当成"范围外"：')
[void]$sb.AppendLine('')
[void]$sb.AppendLine((Quote $styledOffScope))

[System.IO.File]::WriteAllText([System.IO.Path]::GetFullPath($Out), $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Output "native=$($types.Count) scope=$($scope.Count) canStyle=$($native.Count) done=$($done.Count) HARDGAPS=$($hardGaps.Count) substituted=$($substituted.Count) needsOwnType=$($needsType.Count) outOfScope=$($extra.Count)"
