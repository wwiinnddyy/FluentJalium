# ProgressBar 审计（阶段 6 第一段）

运行时权威：NuGet Jalium.UI 26.10.9。WinUI 参考树：`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。
方法参考：`../ModernWpf` @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。两份参考树只读，blob 一律用
`git hash-object` 现算。原始读数：`docs/astra/adaptation/s1o-progress-raw.txt`（`spike/ProgressProbe` 七个模式）。
跨控件结论：`adaptation/00` §S1-o。

## 1. 源文件与 blob

| 文件 | blob |
| --- | --- |
| `controls/dev/ProgressBar/ProgressBar.xaml`（隐式样式 + 模板 + 11 个 VisualState） | `5f188ab8b3d45632d74a90b742299e0412e45feb` |
| `controls/dev/ProgressBar/ProgressBar_themeresources.xaml`（10 行） | `f2cf8b8edbc50ab21d0f2d5f460f30b92d662a36` |
| `controls/dev/ProgressBar/ProgressBar.idl` | `ffa5531f73b8add16756e254b6a76cc630a6d270` |
| `controls/dev/ProgressBar/ProgressBar.cpp` | `7ab65894f91ca59504729240d044cdf468d266cc` |
| `ModernWpf/ProgressBar/ProgressBar.xaml`（方法参考） | `56e7260446a98fdca940957a9ab299cbcca5ef40` |

另有一批**废弃**同名行在 `controls/dev/CommonStyles/Deprecated_themeresources.xaml`
（`ProgressBarThemeMinHeight`、`ProgressBarIndicatorPauseOpacity`、四条 `*ThemeBrush`）：ModernWpf 仍保留它们，
本层一律不发（见 §3 的反向闸口），理由与 AutoSuggestBox 那批"上游自己已废弃/无人读"的处置一致。

## 2. 基型选择：原生重模板，不起自有类型

目标里阶段 6 写的是"ProgressBar"（无"自有"标注）。实测决定它确实不必自有：

- `ProgressBar : RangeBase : Control` 在（mode `census`），属性面 `Minimum`/`Maximum`/`Value`/`IsIndeterminate`/
  `Orientation` 齐，出厂就带 `Template`，部件名 `PART_Track` / `PART_Indicator`。
- 换上我们的样式与模板后，**宿主仍按 Value 比例改写 `PART_Indicator` 宽度**：mode `mount` 与 `retemplate` 同一组
  读数（300 宽、25/50/75/100 → 75/150/225/300），三条声明式替代路线都不等价（`{TemplateBinding Value}` 给的是
  DIP 值而非比例，固定宽度不动，`PART_GlowRect`/`PART_Decorator` 宿主不查）。
- 因此 AGENTS.md"只有证明的行为缺口才自有类型"这一条不成立 → 本层走原生重模板，自有类型不在本段开工。

代价与账单：宿主在部件上写本地值（`Height=NaN` + `VerticalAlignment=Stretch`），所以带形必须由**包裹层**承担
（S1-o 2）。这是本段唯一的结构适应。

## 3. 资源键清单（上游 10 行 → 本层 6 行）

发出去的 6 行，逐字照抄上游键名，消费点全在 `Styles/ProgressBar.jalxaml`：

| 键 | 类型 | 上游目标 | 本层目标 | 差异 |
| --- | --- | --- | --- | --- |
| `ProgressBarForeground` | StaticResource | `AccentFillColorDefaultBrush` | 同 | 无 |
| `ProgressBarBackground` | StaticResource | `ControlStrongStrokeColorDefault` | `ControlStrongStrokeColorDefaultBrush` | **值替换**：上游指 Color 行，Color 落到 Brush 属性时整条写入消失（mode `alias` 读回 `Background=null`），故保留键名改吃孪生刷子 |
| `ProgressBarBorderBrush` | StaticResource | `ControlStrokeColorDefaultBrush` | 同 | 无 |
| `ProgressBarBorderThemeThickness` | Thickness | `0`（HC `1`） | `0` | HighContrast 那一档不在此文件重指向（本层 HC 故事在 `ThemeResources/HighContrast.map` 的调色板层） |
| `ProgressBarCornerRadius` | CornerRadius | `1.5` | 同 | 无 |
| `ProgressBarTrackCornerRadius` | CornerRadius | `0.5` | 同 | 无 |

扣住的 4 行，每条在 `AstraProgressBarTests` 里有逐名反向断言，且理由不同：

1. `ProgressBarMinHeight` = 3、`ProgressBarTrackHeight` = 1：`x:Double` 行。这个 reader 不是"读成 null"而是
   **解析失败**（mode `later`：`Cannot resolve type 'Double' ...`），所以两个度量变成模板字面量（带 3、轨 1），
   与 `MinHeight` 那条 setter 一致。
2. `ProgressBarErrorForegroundColor`、`ProgressBarPausedForegroundColor`：上游由 `ShowError`/`ShowPaused` 两个
   属性驱动状态；本运行时的 `ProgressBar` **没有这两个属性**（mode `later` 两条 ABSENT），因此没有任何消费者可
   写——写进公开集合就等于承诺"覆盖它能改像素"而承诺不成立（消费点闸口也会当场判红）。

另外 `ProgressRingStrokeThickness`（`x:Double` 4，参考树里零消费者）与 `ProgressBar*ThemeBrush` 四条废弃名同样不发。
`keys.md` 里本层公开集因此多 6 行。

## 4. 状态映射（上游 11 个 VisualState → 本层 1 个 Trigger）

| 上游状态 | 驱动 | 本层 | 依据 |
| --- | --- | --- | --- |
| `Normal` / `Determinate` / `Updating` | 内部 | 不写（空故事板） | 上游本身也是空 |
| `Error` / `Paused` / `UpdatingError` / `IndeterminateError` / `IndeterminatePaused` | `ShowError` / `ShowPaused` | **无对应** | 宿主类型没有这两个属性 |
| `Indeterminate` | `IsIndeterminate` | 不写格子：宿主自己把 `PART_Indicator` 收成静态的 1/3 段 | mode `mount`：2 帧与 40 帧后 bbox 都是 90x40 |
| 各 transition（`RepositionThemeAnimation`、`FadeInThemeAnimation`、`ColorAnimation`） | — | **无对应** | 本运行时无这两种主题动画；且 markup `Storyboard` 水合为 0 子元素、变换 DP 上的动画永不振（S1-o 4） |
| —（上游无朝向） | `Orientation`（宿主自有） | 1 格：`Trigger Property="Orientation" Value="Vertical"` 把带子换轴 | 本层自加，断言按几何读回，不写成 parity |

## 5. 模板与几何对照

| 上游（ProgressBar.xaml:156-176） | 本层 | 理由 |
| --- | --- | --- |
| `Border ProgressBarRoot`（TemplateBinding BorderBrush/BorderThickness/Padding/CornerRadius） | 同名同绑 | 无差异 |
| 内层 `Border Clip={Binding TemplateSettings.ClipRect}` | **删** | 本运行时无 `ProgressBarTemplateSettings`，也没有 `TemplateSettings` 属性（mode `later`） |
| `Grid Height={TemplateBinding MinHeight}` | `Grid Name="Band" Height="{TemplateBinding MinHeight}"`，并在朝向格里换轴 | 带子必须有名字才能被 Trigger 换轴；这是宿主覆盖部件尺寸后的唯一声明式出路 |
| `Rectangle ProgressBarTrack` `Height={ThemeResource ProgressBarTrackHeight}` + `RadiusX/Y` 走 CornerRadiusFilter | `Border PART_Track Height="1"` + `CornerRadius={ThemeResource ProgressBarTrackCornerRadius}` | 度量行读不了；圆角不需要拆成 RadiusX/RadiusY（Border 直接吃 CornerRadius） |
| `Rectangle DeterminateProgressBarIndicator`，`Fill={TemplateBinding Foreground}`，宽/位移由代码写 | `Border PART_Indicator Background={TemplateBinding Foreground}`，宽由宿主写 | 名字沿用宿主契约，行为因此保留 |
| `IndeterminateProgressBarIndicator` / `...2` + `CompositeTransform` 滑移 | **不建** | 无动画通路（S1-o 4），且宿主只查一个指示器 |

## 6. Known Gaps（不声称清单）

1. **不定进度是静图**：本段只交付"宿主给的静态 1/3 段"。上游两段滑移要代码动画器（可走 `Canvas.Left`/`Width`
   这条已量通的路，形如 `Motion/NavigationIndicatorAnimator`），本段没有开工，也没有把它记成"已实现"。
2. `Error`/`Paused`/`Updating` 三格无属性可驱动，因此**不存在**于本层，不是"未测"。
3. 无 `TemplateSettings`：`ClipRect` 裁剪、`IndicatorLengthDelta` 回弹、`Container*` 动画停靠点全部无法转录。
4. 竖直朝向是本层随宿主能力加的，**不声称**与 WinUI 对齐（上游根本没有竖直进度条）。
5. 零硬件输入证据：进度条不接受输入，本段没有交互通路可测；全局真指针通路仍欠（Task #13）。
6. 颜色身份只到"刷子实例等于某行"这一层：`Foreground`/`Background`/`BorderBrush` 三条断言读的是实例，
   像素断言只覆盖指示器色（不透明）与 Light↔Dark 的整图差异；轨道在 Light 是黑色 44% 透明，逐键数色会被背衬吃掉。
7. HighContrast 的上游重指向（`SystemColorHighlightColor` 等 5 条）本层不做控件级复写，沿用调色板级映射。
8. 字体/文本：上游进度条无文本（审计确认模板里没有 TextBlock），"百分比文字/省略号"这类要求不存在，本层也不加。

## 7. 四类证据

**构建**（`tools/Test-AstraGates.ps1` 串行，一趟通过）：restore 全部最新；Debug **0 警告 / 0 错误**（真重编，
Manifest 56 → 57… 实为 57 份字典：新增两份，`ThemeResources/ProgressBar.jalxaml` + `Styles/ProgressBar.jalxaml`）；
整套 **1244/1244 通过、0 失败、0 跳过**（5 m 16 s，比第十段 1208 多 36 条 = 新控件 35 条 + 消费点闸口那份新字典 +1）；
调色板漂移 Light 83 源色 / 101 刷、Dark 同、HighContrast 101 映射 + 3 条上游键因调色板无对应而按住，三行
`checked=True`（本批零改动）；`keys.md is current: 1252 canonical lines.`；`All Astra gates passed.`；`GATE_EXIT=0`。
闸口只跑了一趟就绿——与第十段的两趟不同，那趟的差集是目录与死键，这两处本批一次写对，没有用"再跑一遍看看"代替判断。

**行为**（`AstraProgressBarTests` 35 条）：逐值比例（0/25/50/75/100 → 0/75/150/225/300，`Assert.Equal(…, 1)`）；
上游九条 setter 的实例身份与字面值；五个部件名（含 `Band`/`LayoutRoot`/`ProgressBarRoot`）；两轴几何（竖直带 3×40、
轨 1 宽、指示器 20 高）；`MinHeight=7` 在两个朝向下都跟着变（同时证明 Trigger setter 里的 `{TemplateBinding}` 会解析）；
不定态"两帧与四十帧后同宽"的静图断言；6 行发布的正向 theory + 10 行扣住的反向 theory（含 4 条废弃名）；
`ProgressBarBackground` 与 `ControlStrongStrokeColorDefaultBrush` 同一实例、且不是 `Color`。

**A/B 有牙**：把模板里的 `Name="PART_Indicator"` 改成 `Name="Indicator"`（改动落地前先 grep 确认只有一处命中，
改后 `grep -c` 读回 1）→ **11 红 / 24 绿**，红的正是五条比例 theory + 带形/像素/静图/宿主所有权四条；还原后
`Name="PART_Indicator"` 计数回到 1。所以"比例来自部件名契约"不是注释里的说法，是能被打断的断言。

**视觉**：三条像素断言全部只读实际测得的量——带形用"50% 处强调色像素数落在 200~1500 之间"框住
（300×3≈450 是理论值，宿主自己的模板在同一被摄体上量到 5704，差一个数量级）；品牌绿 `#207245` 计数 0；
Light↔Dark 两档各数到自己那档的强调色、且整图直方图 top4 不同。轨道色在 Light 是黑色 44% 透明，逐键数色会被
背衬吃掉，因此本批对轨道只主张实例身份不主张像素（见 §6 第 6 条）。
Gallery 侧：新 **Status 页**（第 10 页）冒烟 10.5 秒上屏、优雅退出、无残留进程
（`tools/Test-AstraGallerySmoke.ps1 -Page status`）；该脚本按 `adaptation/06` 的结论**故意不截屏**，
所以新卡片本身没有目视帧——像素主张只在上面那三条断言里。

**硬件输入**：**仍为零**，且本批不假装有。进度条不接受指针/键盘输入，因此没有"退化"可测；真正欠的是全局真指针
通路（Task #13），阶段 2~5 的 hover/press 触发器至今没有一条硬件证据。

