# ScrollBar 审计（阶段 1 底座批）

- 上游：`controls/dev/CommonStyles/ScrollBar_themeresources.xaml`，commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
  blob `6aaa54e2b229039c3021a0a0b2f4d7a3deaec517`，720 行（别名键、度量与 `DefaultScrollBarStyle`
  模板在同一文件；`x:Key="DefaultScrollBarStyle"` 在 L200）。核对过：钉住版本的字节数与工作树一致。
- 运行时：NuGet Jalium.UI 26.10.9。
- 一手读数：`scrollbar-raw.txt`（同一探针 `spike/PixelAttribution` 的 `scrollkeys` 与 `parts` 两次运行，
  每个候选只单独装一次，所以谁动了像素就归谁）。

## 1. 上游面（76 个 `ScrollBar*` 键，逐字分类）

| 类别 | 个数 | 内容 |
|---|---|---|
| 刷别名（三分支各一份） | 33 | `ScrollBarBackground{,PointerOver,Disabled}`、`ScrollBarForeground`、`ScrollBarBorderBrush{,…Over,…Disabled}`、`ScrollBarButtonBackground{,…Over,…Pressed,…Disabled}`、`ScrollBarButtonBorderBrush{,…Over,…Pressed,…Disabled}`、`ScrollBarButtonArrowForeground{,…Over,…Pressed,…Disabled}`、`ScrollBarThumbFill{,…Over,…Pressed,…Disabled}`、`ScrollBarThumbBorderBrush`、`ScrollBarThumbBackground`、`ScrollBarTrackFill{,…Over,…Disabled}`、`ScrollBarTrackStroke{,…Over,…Disabled}`、`ScrollBarPanningThumbBackground{,Disabled}` |
| 色别名 | 2 | `ScrollBarThumbBackgroundColor`、`ScrollBarPanningThumbBackgroundColor` |
| 旧式 `*ThemeBrush` | 17 | `ScrollBarButtonForegroundThemeBrush` 一族；高对比分支里它们直接写 `{ThemeResource SystemColor*}` |
| 边框粗细 | 2 | `ScrollBarTrackBorderThemeThickness`（L/D=0，HC=1）、`ScrollBarPanningBorderThemeThickness`=1 |
| 度量/时长/边距 | 22 | `ScrollBarSize`=12、`ScrollBarCornerRadius`=3、`ScrollBarVerticalThumbMinWidth`=8 / `…MinHeight`=30、`ScrollBarHorizontalThumbMinHeight`=8 / `…MinWidth`=30、`ScrollBarThumbOffset`=2、`ScrollBarThumbStrokeThickness`=6、`ScrollBarButtonArrowIconFontSize`=8、`ScrollBarButtonArrowScalePressed`=0.875、四个 `*Margin`(4,0,0,0 族)、`ScrollBarExpandDuration`/`ContractDuration`=0.167、`ColorChangeDuration`/`OpacityChangeDuration`=0.083、`ContractDelay`=2s、`ContractFinalKeyframe`=2.1s、`ExpandBeginTime`=0.40s、`ContractBeginTime`=0.50s |

模板部件（`DefaultScrollBarStyle` 内）：`VerticalRoot`、`VerticalTrackRect`、
`VerticalSmallDecrease`/`VerticalSmallIncrease`（箭头 RepeatButton，**静止 `Opacity="0"`**，L703/L711）、
`VerticalLargeDecrease`/`VerticalLargeIncrease`（页面按钮）、`VerticalThumb`、`VerticalThumbTransform`、
`VerticalPanningRoot`/`VerticalPanningThumb`。状态组：`ScrollingIndicatorStates`（L446：`NoIndicator`、
`MouseIndicator`、`TouchIndicator` …）、`ConsciousStates`（L528：`Collapsed{,WithoutAnimation}`、
`Expanded{,WithoutAnimation}` …），箭头按钮另带自己的 `CommonStates`（`Normal`/`PointerOver`/`Pressed`/`Disabled`）。

## 2. 26.10.9 实际可达的面（实测，逐条）

探针把候选一样一样装进 `Application.Resources`，每次都重建一个 `ScrollViewer`（内容 800 高、视口 44 高），
推 16 帧后裁 ScrollBar/Thumb/RepeatButton 各自的 bounds。`scrollkeys` 段的读数是整窗裁剪，
`parts` 段是部件裁剪（`#00…` 前缀是内层直采不写 alpha 字节，见 `06`）。

| 候选（一次只装一个） | 动像素？ | 读数 |
|---|---|---|
| `ScrollBarStyle` 的 `Background` setter | **是** | 青 `x144` + 被拇指压住的 `#008D8D x64`（parts t2） |
| `ScrollBarStyle` 的 `Foreground` / `BorderBrush` setter | 否 | 与 t0 基线逐桶相同 |
| `ScrollBarStyle` 的 `ThumbStyle` setter | 否 | 同上 |
| `ScrollBarStyle` 的 `Template` setter | **否** | parts t1 与 t0 完全一致 |
| 命名刷 `ScrollBarTrack` | **是** | 黄 `#FFFF00 x144`（h5）；在最终配置里是 parts t6 的青柠 `x144` |
| 命名刷 `ScrollBarThumb` | **是** | 拇指部件裁剪 `x92`（h6 橙、t6 品红） |
| 命名刷 `ScrollBarArrow` | 否 | h7 与基线一致 |
| 隐式 `typeof(ScrollBar)` + Template | 否 | h8/t3 |
| 隐式 `typeof(ScrollBar)` + Background | 否 | h9 |
| 隐式 `typeof(RepeatButton)` / `typeof(Thumb)` | 否 | t4/t5 |
| 隐式 `typeof(ScrollViewer)` + Template | 否 | t7 |

结论（也是对本目录 `07` 早前判断的更正）：**ScrollBar 不是"自绘一个矩形"，它在代码里建了真的
`RepeatButton`/`Track`/`Thumb`/`Border`/`Path` 部件树**——静止读数 `ScrollBar 12x44`、
`Thumb 8x12`、两个箭头 `RepeatButton 12x12`（各含 `Path 8x8`）、页面按钮 `12x0` 与 `12x8`。
但**没有任何一条改结构的路**：`ControlTemplate`、隐式样式（对 ScrollBar、它的部件、或宿主 ScrollViewer）
全部测不出来。能改的只有拇指颜色、轨道颜色，和一个 `ScrollBarStyle.Background`。
按 ROADMAP D 的分级，这是 `frozen-brush`，不是 `template-only`。

> **2026-09-18 更正与加强（`audits/scrollviewer.md`，同一套部件级判据）**
> 1. 措辞不准的地方：**宿主 `ScrollViewer` 的隐式样式是生效的**（`IsTabStop`/`Padding`/`Background`
>    都能从建出来的宿主读回）——只是它对滚动条的像素没有影响，当时用像素判据就把两件事混成了一句。
>    反过来，**ScrollBar 自己的隐式样式连值都落不上**：setter 写 `Padding=7`，建出来的 bar 读回
>    仍是代码设的 `2`。所以"改结构无路"这条对 ScrollBar 更严重，对宿主不成立。
> 2. 为什么只有两个刷名管用，现在有了机制层面的读数：`ScrollBar.ThumbStyle` 是框架自己设的
>    （`TargetType==Thumb`），那份样式的 `Background` 就是 `ScrollBarThumb`；应用级
>    `Resources[typeof(Thumb)]` 也顶不掉它。
> 3. 箭头颜色**确实无路**，且这次是逐个名字量过的：dll 元数据里的
>    `ScrollBarArrowBrushKey`/`ScrollBarArrowHoverBrushKey`/`ScrollBarArrowPressedBrushKey`/
>    `ScrollBarBrushKey`/`ScrollBarColorKey` 所指向的 10 个公开名，一个一个塞进
>    `Application.Resources` 后静止灰色不变（10 次 `sentinel=0`）。内部键名不在公开面上。
> 4. 新增一条候选（未采用）：`ScrollViewer.IsOverlayScrollBarEnabled=true` 会让静止灰色归零，
>    也就是唯一能做出上游"平时看不见"外观的开关；缺悬停证据，见 `scrollviewer.md`。

## 3. 落地

- `ThemeResources/Light.jalxaml` / `Dark.jalxaml` 各加两条由生成器产出的刷：
  `ScrollBarThumb` = `#72000000` / `#8BFFFFFF`，`ScrollBarTrack` = `#00FFFFFF`（两分支同值）。
  值不是手抄的字面量：`tools/Sync-AstraPalette.ps1` 里 `$hooks` 把它们指到
  `ControlStrongFillColorDefaultBrush`（上游 `ScrollBarThumbFill` L26）与
  `ControlFillColorTransparentBrush`（轨道的替换值，见第 5 节），调色板变了它们跟着变。
- `ThemeResources/HighContrast.map` 加两行，用同一上游文件高对比分支自带的旧式行做证据：
  `ScrollBarThumb=SystemColorButtonTextColor`（`ScrollBarThumbBackgroundThemeBrush`）、
  `ScrollBarTrack=SystemColorButtonFaceColor`（`ScrollBarTrackBackgroundThemeBrush`）。
- 放在调色板而不是样式表，是因为只有调色板会被内核**原地改色**（`RefreshPalette` 保住刷实例）；
  样式表里写 `<SolidColorBrush Color="{ThemeResource X}">` 在解析时就定色，翻主题不动。
- **不发 `ScrollBarStyle`**。试过并撤回：它的 `Background` 一旦有值（哪怕是透明），
  同一块轨道像素就不再由 `ScrollBarTrack` 决定（t6 里青柠缺席，t2 里青占 144）。
  上游 `ScrollBarBackground` 本来就透明，框架默认也什么都不画（t0 基线轨道区 0 像素），
  所以这个 setter 只会挡路。样式表因此不建文件，`Themes/Manifest.txt` 也没有新条目。
- 闸口随之收紧：`FluentThemeManager.PaletteBrushKeys` 原来按 `*Brush` 后缀筛刷键，
  而钩子名字是框架定死的、不带后缀；改成按值类型筛，`High_contrast_mapping_covers_every_palette_brush`
  才真的覆盖这两个键（否则它们从两边同时消失，断言会空过）。

## 4. 状态映射表（WinUI VisualState → 这里能做到什么）

| 上游状态 | 上游做的事 | Jalium 26.10.9 | 出口 |
|---|---|---|---|
| `ConsciousStates.Expanded` / `Collapsed` | 拇指宽 8↔12、`VerticalThumbTransform.Y`=2、0.167s 关键帧 | 部件宽度和动画都拿不到 | Known Gap |
| `ConsciousStates.*WithoutAnimation` | 同上但无动画（减动效路径） | 同上 | Known Gap |
| `ScrollingIndicatorStates.Mouse/Touch/PenIndicator` | 按输入来源显隐 | 无入口，且框架没有指示来源信号 | Known Gap |
| `CommonStates.PointerOver`（箭头按钮） | `ScrollBarButtonBackgroundPointerOver`=`SubtleFillColorSecondary` | `ScrollBarArrow` 与隐式 RepeatButton 都不读 | Known Gap |
| `CommonStates.Pressed` | 箭头按下去换成 `ControlStrongFillColorDefault`、拇指 `ScrollBarThumbFillPressed`（与静止同色） | 同上；拇指 pressed 色本来等于静止色，代价为零 | Known Gap（仅箭头） |
| `CommonStates.Disabled` | `ScrollBarThumbFillDisabled`=`ControlStrongFillColorDisabled`、轨道透明 | 无 `IsEnabled` 通路可挂钩子 | Known Gap |
| `VerticalPanningThumb` | 触摸拖拽时换成 `ScrollBarPanningThumbBackground` | 拿不到 panning 部件 | Known Gap |

`ScrollBarThumbFill{,PointerOver,Pressed}` 三个别名都指 `ControlStrongFillColorDefaultBrush`，
所以"拇指三态不同色"这条本来就没有视觉差；差异全在尺寸与淡入淡出。

## 5. Host substitution（两条，都要写明代价）

1. **轨道材质**：上游 `ScrollBarTrackFill` = `AcrylicInAppFillColorDefaultBrush`（L31）是背衬材质，
   框架在自己的轨道绘制里画不出折射/模糊，而把它当资源值写进调色板会被"主题令牌不得冻死"的闸口拦下。
   落地取 `ControlFillColorTransparent`，也就是 WinUI 静止（未悬停、未展开）时用户看到的样子。
   代价：悬停/展开时的亚克力轨道没有，且本来也拿不到那个状态。高对比不损失——上游 HC 分支
   的 `ScrollBarTrackFill` 是 `SystemControlPageBackgroundChromeLowBrush`，映射后就是
   `SystemColorButtonFaceColor`。
2. **不可重模板**：AGENTS 要求"原生控件优先 `.jalxaml` 重模板"。这条对 ScrollBar 不成立，
   第 2 节是证据；因此本控件的颜色层用命名刷钩子实现，而不是模板。这条同时是**上游 33 个别名键**
   进不了我们键面的原因：没有消费点，写了就是静默无效键，会被"每个被引用的键都有声明"的反向检查
   （键清单，任务 #11）挑出来。别名层的转录仍按 A2 单独推进，但那批键的归属要按"无运行时消费点"标注。

## 6. 证据（像素，`AstraPixelTests`，26.10.9 实测通过）

- `The_scroll_bar_matches_the_upstream_size_and_thumb_width`：`RenderPart<ScrollBar>` 宽 12
  （= 上游 `ScrollBarSize`），`RenderPart<Thumb>` 宽 8（= `ScrollBarVerticalThumbMinWidth`）。
- `The_scrollbar_thumb_hook_paints_the_thumb`：覆盖 `ScrollBarThumb` 为哨兵后，拇指部件裁剪里哨兵 > 48 px
  （读数 92/96）。
- `The_scrollbar_track_hook_paints_the_track`：覆盖 `ScrollBarTrack`，整条滚动栏里哨兵 > 48 px（读数 144/528）。
- `The_scroll_bar_thumb_follows_the_theme_and_shows_no_brand_emerald`：不动覆盖时，Dark 的栏里有
  `#FFFFFF` > 48 px（`#8BFFFFFF` 那支），Light 里没有，且两支都没有品牌绿。
- 闸口全套：`tools/Test-AstraGates.ps1` → 24/24 通过、0 skip、调色板无漂移（生成器改动重跑 `-Check` 一致）。

四类证据的分配：**视觉**=上面四条；**行为**=滚动条的交互（滚轮/拖拇指/点页面区）不是本批改的，
框架自建部件仍走框架逻辑，未新增行为，故无行为断言可记；**硬件输入**=触摸 panning 部件拿不到（第 4 节），
未验证；**构建**=闸口脚本串行跑过。

## 7. Known Gaps（不许用相邻证据替代）

1. 箭头 glyph 静止就画：t0 基线里两个箭头各 40 px `#D2D2D2`，Light/Dark 同色。上游的箭头按钮静止是
   `Opacity="0"`，只在展开态淡入。既改不了透明度（无模板/样式通路），也改不了颜色（`ScrollBarArrow` 不读）。
   这是当前 ScrollBar 与 WinUI 3 唯一看得见的结构差。
2. 悬停/按下/展开/收缩、触摸 panning、禁用态全部不可达（第 4 节）。
3. `ScrollBarCornerRadius`=3 无法施加（拇指是框架自己的 Border）。
4. 33 个上游公开别名键没有消费点，本批不声明（第 5 节第 2 条）。
5. 高对比只有逐键映射的断言（`High_contrast_mapping_covers_every_palette_brush`），
   没有上屏像素证据；轨道在 HC 会变实色这一点未经像素验证。
6. 裸 `ScrollBar`（不在 ScrollViewer 里）会把推帧循环挂住（`07` 记过），所以它的样式通路是**没测**而不是"测了不通"；
   库使用者把 ScrollBar 单独摆进界面时能不能吃到这两个钩子，未证。
7. 混合 DPI、真实触摸/笔未验证。
