# Slider 审计：上游行照抄、状态落位，以及两条被它掀翻的基座假设

上游出处：`microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
`controls/dev/CommonStyles/Slider_themeresources.xaml`，blob `3804392c5c54081dcc64a1c533812f9b07a4155d`，
全文 441 行。`ThemeDictionaries` 三段：Default 4-55、High Contrast 56-107、Light 108-159；
字典外的 14 行度量在 161-174；`DefaultSliderStyle` 从 175 起。
方法照抄 ModernWpf（`../ModernWpf` @`23555a6`）：先数上游有多少行，再逐行问"这个运行时有没有能读它的属性"。

## 0. 先把运行时量出来，再决定抄多少

`Slider` 在本机 26.10.9 上声明 18 个属性（反射实测，不是文档）：
`AutoToolTipPlacement, AutoToolTipPrecision, Delay, Interval, IsDirectionReversed, IsMoveToPointEnabled,
IsSelectionRangeEnabled, SelectionStart, SelectionEnd, TickPlacement, Ticks, Orientation, TickFrequency,
IsSnapToTickEnabled, TrackMode, SegmentGap, TrackBrush, ThumbBrush`，
外加继承来的 `Background / Foreground / BorderBrush / BorderThickness / CornerRadius / Padding / IsEnabled`。
这份清单直接决定了三件事：有 `TickPlacement`+`TickFrequency` 所以刻度条行是可用的；
没有 `Header` 所以两行 header 令牌无处可写；两个枚举里都没有 `Inline` 所以内联刻度条无处显示。

**这一批同时推翻了本仓两条既有假设**，两条都是用控件自己量出来的：

1. **字典里的 `ControlTemplate` 资源带不动状态格。** 把模板作为资源存放、样式用
   `Value="{StaticResource …}"` 引它，运行时读回来的每个 `Trigger.Property` 都是 null——格子在、setter 在、
   键名也对，但条件永远匹配不上，控件被钉在静止态。内联写进 `<Setter Property="Template">` 的模板没有这个
   问题（同一次读取里 TextBox/RadioButton 的格子全部解析成功）。改完前后各测一次：
   禁用态滑块拇指 `#FF0078D4`（静止的 AccentFillColorDefault）→ 改完读到 `SliderThumbBackgroundDisabled` 实例，
   `TickPlacement=Both` 的刻度条 `Collapsed`/`0x0` → `Visible`/`204x4`。
   闸口：`AstraGateTests.State_cells_are_not_written_into_a_keyed_template_resource`（扫 markup，带 `x:Key` 的
   ControlTemplate 里有 Trigger/MultiTrigger 即失败）。同一个缺陷在 `ButtonSurfaceTemplate` 上也存在，
   一并改成内联——它带走的是"按钮获得键盘焦点时描边出现"这条从未生效的声明。
2. **像素基座的推帧看门狗释放的不是正在等的那个调度器。** `PixelHarness.Pump` 的
   `System.Threading.Timer` 回调里写 `Dispatcher.CurrentDispatcher`，在线程池线程上求值拿到的是一个没人跑的
   调度器，`frame.Continue = false` 入队后再也不会执行。于是"不带动画的状态变化"必挂 60 秒——
   聚焦一个 Slider 正好是这种（静止态与焦点态只差一个 `Opacity`）。选择批里两次"未归因"的超时是同一件事。
   修法是在推帧前抓住调用线程的调度器实例。判据：同一次运行里 Button 聚焦后推帧 8 ms 返回、
   框架自带模板的 Slider 也返回、我们的 Slider 在修复前挂死、修复后读到 `Opacity=1`。

## 1. 抄了多少行，为什么

| 上游 | 本仓 | 说明 |
|---|---|---|
| Default 段 23 条别名行（**只有 22 个不同名字**：12 与 13 两行都叫 `SliderContainerBackgroundDisabled`，后者指向 `ControlFillColorTransparent` 这个 Color） | `ThemeResources/Slider.jalxaml` 20 条别名行 | 重复行不复制；Default 段根本没有 `SliderContainerBackground` |
| Light 段 23 条别名行（含 `SliderContainerBackground`） | 同上，共用一套 | 别名层与主题无关，见 §2 第一条偏差 |
| High Contrast 段 22 条别名行 + 1 条实底 | 未抄 | 见 §5 最后一条 |
| 每段 24 条 `*ThemeBrush` 遗留行 | 未抄 | Win8.1 时代的名字，现行模板没有读者 |
| `SliderBorderThemeThickness` / `SliderTrackCornerRadius` / `SliderThumbCornerRadius` | 抄 | 三条都有消费者（样式 1 条、模板 2 条） |
| 8 条 `x:Double` 度量 + `SliderHeaderThemeMargin` / `FontWeight` / `SliderTopHeaderMargin` | 不抄 | 前者这个读器解析不了 `x:Double` 资源，落到模板字面量；后者没有 header 可摆 |

三行**故意**没抄，每行都配了一条反射断言（`The_rows_left_out_are_left_out_because_no_state_can_reach_them`），
将来运行时补了属性，断言会失败逼我们重新决定：`SliderHeaderForeground`、`SliderHeaderForegroundDisabled`
（`Slider` 无 `Header`）、`SliderInlineTickBarFill`（`TickBarPlacement` 是 `{Left,Top,Right,Bottom}`，
`Slider.TickPlacement` 是 `{None,TopLeft,BottomRight,Both}`，两边都没有 Inline）。

样式行抄了 3 条：`Background=SliderTrackFill`、`BorderThickness=SliderBorderThemeThickness`、
`Foreground=SliderTrackValueFill`。没抄 6 条并说明原因：`BorderBrush=SliderThumbBorderBrush` 与
`CornerRadius=SliderTrackCornerRadius` 在本仓由拇指圆环和轨道 Border 直接读键，控件自身没有边框消费者；
`FontFamily` / `FontSize` / `ManipulationMode` / `UseSystemFocusVisuals` / `FocusVisualMargin` /
`IsFocusEngagementEnabled` 在这个运行时没有对应属性（18 条声明属性里没有后四条）。

## 2. 三处替换与它们的机制原因

- **`SliderThumbBorderBrush`：值替换。** 上游指 `ControlElevationBorderBrush`，是渐变；渐变刷进不了
  "就地改色"的调色板模型（`FluentThemeManager.RefreshPalette` 只改 `SolidColorBrush` 实例），所以取同族实底
  `ControlStrokeColorDefaultBrush`——与按钮边框第 4 号偏差同一处理（`audits/button.md`）。
  这一行同时**回收了一个自造键**：`SliderThumbStrokeBrush` 是我们自己的调色板生成器为这个位置造的名字，
  上游从未有过；它连同 `Light/Dark` 两行和 `HighContrast.map` 一行一起删掉了（现在 101 刷/101 键）。
- **`SliderContainerBackground`：作用域替换。** 上游只在 Light 与 High Contrast 段声明它，Dark 段没有静止
  容器行；本仓别名层与主题无关，四行容器令牌都指向同一个透明刷，所以"补上第四行"不改变任何一个像素。
- **`x:Double` 度量落成字面量。** 轨道 4、刻度条 4、内芯 12、拇指 20/18、`MinHeight`/`MinWidth` 32。
  理由写进 `ThemeResources/Slider.jalxaml` 的头注释，不是随手写的数。

## 3. WinUI 视觉状态 → 本仓触发器

| 上游组/状态 | 本仓 | 证据 |
|---|---|---|
| `CommonStates/Normal` | 元素静止值（`SliderContainer`、`PART_Track`、`PART_SelectionRange`、`SliderInnerThumb`、刻度条 `Fill`） | 键名 + 挂树实例 |
| `CommonStates/PointerOver` | `Trigger IsMouseOver=True` | 格内 6 条 setter 的键名逐条断言 |
| `CommonStates/Pressed` | `Trigger IsMouseCaptured=True` **+** `AreAnyTouchesCaptured=True`（两格同值，指针与触摸各自成立） | 键名；**无真机输入证据**，见 §5 |
| `CommonStates/Disabled` | `Trigger IsEnabled=False`（列在最后一格，压过悬停） | 挂树读回 6 个实例 + 12 DIP 内芯 |
| `TickStates`（`TickBar` 显隐，上游用 `ObjectAnimationUsingKeyFrames` 写 `Visibility`） | 三格 `Trigger TickPlacement=TopLeft/BottomRight/Both` | `None/TopLeft/Both` 与竖排 `BottomRight` 的挂树可见性 + 尺寸 |
| `FocusEngagementStates` | 无 | 本运行时无 `IsFocusEngagementEnabled`，见 §5 |
| 拇指挤压（上游动画 `ScaleX/ScaleY` 0.86/1.167） | `SliderInnerThumb` 的 `Width/Height` 12→14→10→12 + `TransitionProperty` | 静止/禁用格都读到 12；**没有动画过程证据** |
| 描边刷切换（上游 `ObjectAnimationUsingKeyFrames` KeyTime 0） | 直接赋值，且刻意**不**放进 `TransitionProperty` | 过渡中途的刷是内插实例，不属于任何调色板（`adaptation/12`） |

部件名两处让给框架契约（实测）：`PART_SelectionRange` 被框架按值改宽（204 DIP 轨道上 0/51/102/153/204），
`PART_Thumb` 被框架写 `Margin`（同一组数），所以上游的 `HorizontalDecreaseRect` / `HorizontalThumb`
在本仓必须是这两个名字；其余部件名照抄上游（`SliderContainer`、`TopTickBar`、`BottomTickBar`、
`SliderInnerThumb`）。上游 `SliderContainer` 是 Grid，本仓用 Border 才能吃 `Background` 令牌——元素类型替换。

## 4. 四类证据（分开记）

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，
  **136/136 通过、0 警告、0 跳过**；调色板 Light/Dark 各 83 色→101 刷 `checked=True`，高对比 101 键
  `checked=True`（本批删掉了自造的 `SliderThumbStrokeBrush`，两个作用域各 -1）。
- **结构 / 键**：20 条 `Assert.Same` 逐行证明别名解析到调色板那个实例；
  `Transcribed_control_rows_are_read_by_a_template` 把 `ThemeResources/Slider.jalxaml` 纳入"声明的行必须全被读到"
  （现在 4 个文件）；`Both_slider_templates_carry_one_cell_per_upstream_state` 除逐格键名外还断言
  每个 `Trigger.Property` 与每个 `Setter.Property` 都非空——这条正是 keyed 模板缺陷的反向闸口；
  新闸口 `State_cells_are_not_written_into_a_keyed_template_resource` 在 markup 层挡住形状。
- **行为 / 读回**：`TickPlacement` 四值 × 横竖两向的刻度条可见性与尺寸；值跟随
  （`PART_SelectionRange.ActualWidth` 与 `PART_Thumb.Margin.Left` 逐值相等）；禁用格 6 个实例 + 内芯 12；
  真实 `Focus()` 返回 true 后读到焦点环 `Opacity=1`（本仓第二条键盘焦点读回，也是基座修复的验收）。
  顺带补上 Button 的 `A_focused_button_raises_its_focus_ring`。
- **像素**：`A_resting_slider_paints_its_rows_and_nothing_invented` 一次捕获、三个判定：
  轨道令牌哨兵 >300 px、强调色族哨兵 >300 px、品牌绿 `#207245` = 0 px。

## 5. Known Gaps（不许用相邻证据替代）

- 不声称悬停、按下、拖动有任何**真机输入**证据。Pressed 是同一值的两格（`IsMouseCaptured` /
  `AreAnyTouchesCaptured`），键名与实例都读过，但没有任何一次指针或触摸按下驱动它；触摸路径按
  "不得退化成鼠标"的要求单列，仍然欠着。
- 不声称拇指挤压动画被看过：只有静止与禁用格的 12 DIP 宽度，没有过程帧。
- 不声称焦点环的像素：读到 `Opacity=1`，没截过图。
- 不声称 `FocusEngagementStates`：这个运行时无 `IsFocusEngagementEnabled`，上游"键盘需要先按 Space 才接管滑块"
  的交互在本仓根本不存在，不是没做而是无处做。
- 不声称 `Ticks`、`AutoToolTipPlacement`、`AutoToolTipPrecision`、`TrackMode`、`SegmentGap` 任何一条：
  它们是本运行时独有的 WPF 风味属性，上游没有对应概念，本批既没接也没测。
- 不声称高对比 parity：High Contrast 段那 22 条别名行（把圆环指向强调色、轨道指向
  `SystemControlForegroundBaseMediumLowBrush`）没有逐控件移植，只有调色板层的整体重映射近似。
- 不声称竖排滑块的拖动与刻度条像素：竖排只测到 `RightTickBar` 可见、`PART_SelectionRange` 4×51。
- 不声称 Gallery 的 Inputs 页画对了：页面上新增了三行滑块（隐式样式、带刻度、禁用），
  本批没有做页面级进程内渲染测量。


## 8. 间距批（2026-09-19）：拇指、刻度与三段带

上游 `Slider_themeresources.xaml`（`controls/dev/CommonStyles/`，@19e3bdc3c）第 166–173 行是这批的判据：
`SliderHorizontalThumbWidth/Height` 与 `SliderVerticalThumbWidth/Height` 都是 **18**，
`SliderInnerThumbWidth/Height` 是 **12**，拇指模板的环是 `Border Margin="-2"`（:198，即环 = 拇指 +4），
刻度条与轨道之间是 **4**（`TopTickBar` `VerticalAlignment=Bottom Margin="0,0,0,4"` :413、
`BottomTickBar` `VerticalAlignment=Top Margin="0,4,0,0"` :415、竖 :431/:433），
而轨道所在的行是 `SliderPreContentMargin` 14 / Auto / `SliderPostContentMargin` 14（:164–165、:406–408）。

本批前的实现三处都不对：拇指 16×20（竖 20×16）、环 20×20 只横向 -2、刻度间隙 2，并且轨道与刻度条都自加了
**8 DIP 横向内缩**——上游没有这个内缩。现在横竖两份模板都改成 14/Auto/Auto-4-14 的三段带、拇指 18×18、
环 `Margin="-2"`（无显式尺寸，随父格 +4）、内芯 12、刻度间隙 4。

**框架侧的一条实测契约**：`PART_Thumb.Margin.Left` 与 `PART_SelectionRange.ActualWidth` 逐值相等，
220 宽的控件上行程是 **0/51/102/153/204**——把拇指从 16 换成 18、把轨道内缩从 8 换成 9，这两个数**不变**。
也就是说框架给拇指留的是字面量 16，不看我们给的轨道宽度。因此内缩取"半个拇指"（9）时拇指**中心**恰好跟着
填充末端走，而 18 的盒在最大值处右边缘到 222、环到 224，即**超出控件 2 DIP**；上游是靠 14 的前后段吸收这个
悬出，本运行时的行程公式不吃段宽。没有把拇指缩回 16 去掩盖它——那会让拇指与上游差 2 DIP 而换来一个数字好看。
断言：`The_value_fill_and_the_thumb_follow_the_value`（行程表 + 18×18 + 悬出量 `InRange(0, 2.5)`）、
`A_tick_placement_shows_only_the_bars_the_orientation_calls_for`（刻度条宽 202、`Margin=9,0,9,4`）。

仍不声称：拇指悬出 2 DIP 在真窗口里是否被相邻控件裁掉——Inputs 页这次捕获成功（`spike/VisualQA/out/inputs.png`，
PrintWindow，dpi=168），但三条 Slider 都在折叠线以下：整页扫 880 列宽找不到一段 ≥120 px 的强调色轨道，
所以滑块的视觉半边本批**没有**像素证据，只有上面的排布读数；留给任务 #13 的滚动/直挂通路。`SliderFocus` 部件已经不在了（2026-09-22 起环挂在 `FocusVisualStyle` 上，竖排由 `Orientation` 那一格换成 `FocusVisualSliderVerticalStyle`，见 `audits/focus-visual.md`），它的落点仍与上游的
`FocusVisualMargin="-14,-6,-14,-6"` 不同形（我们是环自己模板里的一圈描边）。
