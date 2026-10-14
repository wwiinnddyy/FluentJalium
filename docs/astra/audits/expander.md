# Expander 审计（阶段 4 第一段）

上游依据：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`

- `controls/dev/Expander/Expander_themeresources.xaml` blob `270447141f6f1a39ad7c3e570f0ac8ed3ccb0a8e`（501 行）
  - Light 分支 **5–26 行**：20 条别名 + `ExpanderHeaderBorderThickness`(14) + `ExpanderChevronBorderThickness`(24)
  - Dark 分支 29–50 与 Light 逐行同值；High Contrast 分支 53–74 把 20 条别名改指
    `SystemColorButtonFaceColor`/`ButtonTextColor`/`HighlightColor`/`WindowColor`/`WindowTextColor`、
    `SystemControlTransparentBrush` 与两条 `TextFillColorDisabled`，并把 `ExpanderHeaderBorderThickness` 抬到 2
  - 分支外 77–88：`ExpanderMinHeight`(x:Double)、两条对齐枚举、`ExpanderHeaderPadding`、
    `ExpanderChevronMargin`、两条 `x:String` 箭头字形、`ExpanderChevronButtonSize`/`ExpanderChevronGlyphSize`(x:Double)、
    `ExpanderContentPadding`、`ExpanderContentDownBorderThickness`、`ExpanderContentUpBorderThickness`
  - 89–294 `ExpanderHeaderDownStyle`、295–500 `ExpanderHeaderUpStyle`：整份头 ToggleButton 模板 + 12 个 `CommonStates`
- `controls/dev/Expander/Expander.xaml` blob `42067bde3bf95d33ca5f1718798b6ef81a9be420`（125 行）
  - `DefaultExpanderStyle` 4–125；模板 29–124；`ExpandStates` 32–91（4 个状态带 Storyboard）、
    `ExpandDirectionStates` 92–105（Down/Up）；头 ToggleButton 111、`ExpanderContentClip` 113、`ExpanderContent` 114

运行时：NuGet Jalium.UI **26.10.9**，`Jalium.UI.Controls.Expander`（`HeaderedContentControl` 之下）**原生存在**，
自带一份代码构造的模板，部件名是 `RootBorder` / `PART_HeaderBorder` / `PART_Chevron` / `PART_HeaderContent` /
`PART_ContentBorder`。声明面只有 `ExpandDirection`、`HeaderBackground`、`IsExpanded` 三个 DP、`Expanded`/`Collapsed`
两个事件与 `OnApplyTemplate`（`spike/SurfaceProbe` B2/B3/B4）。

一句话结论：**Expander 的模板不是外观契约而是功能契约**——三个 `PART_*` 名字由控件自己读写，
改名后模板照样构建、照样布局、照样上色，只是永远不再展开。这一点被正反两侧断言钉住，而不是靠读源码信任。

## 0. 先量后写：`spike/ExpanderInfoBarProbe` 三遍量到的事实

**S1 · 内容可见性由 `PART_ContentBorder` 这个名字驱动。**
自带名字的模板：`IsExpanded=true` → 该部件 `Visibility` 从 `Collapsed` 翻到 `Visible`（`0x0` → `320x40`）。
全部改名（后缀 Z）的模板：同一次赋值之后仍是 `Collapsed`，且 `PART_ContentBorder` 根本找不到（pass 1 F/H、pass 3 N）。

**S2 · 箭头旋转由 `PART_Chevron` 驱动，且它必须是 `Shapes.Path`。**
展开后 `RenderTransform` 变成 `RotateTransform`，静置时为 `null`（pass 1 F、pass 3 N）。
框架读到的是 `FrameworkElement`/`Path` 具体类型，所以用 `TextBlock` 字形占这个位置会静默失效。
旋转角固定 **+90 度**、绕 `(0.5,0.5)`——不随 `ExpandDirection` 变，也不像上游那样在 up/down 之间转 180 度。

**S3 · 点击展开是控件在 `PART_HeaderBorder` 上挂的 `MouseDown` 处理器。**
公开面没有任何按钮部件，原生头是 `Grid`，因此点击路径只能由控件自己安装。
用公开 `UIElement.RaiseEvent` 合成一个左键 `MouseDown`：原生控件与我们的重模板控件都恰好展开一次
（`Expanded` 事件计数 =1，内容部件转为可见），再点一次收起（pass 3 R/R2）。
这条同时给出键盘路径的形状：控件自身 `Focusable=true`，所以焦点环与 Tab 停靠点归控件，而不是像上游那样归头按钮。

**S4 · 头部件不能是 `ToggleButton`。**
既然 S3 的处理器挂在按名字找到的部件上，同一个部件再挂一份自己的点击语义就会一次点击展开两次
（与阶段 2 的 `SplitButton.Command` 双执行同一形状）。因此头保持 `Border`，测试
`The_header_is_a_border_because_the_control_itself_handles_the_click` 把这条钉住。

**S5 · 控件没有 `IsPressed`。**
`AstraGateTests.Style_setters_name_properties_the_controls_actually_have` 逐条解析样式里点名的属性，
它直接拒绝了 `Expander.IsPressed`——这条不是风格意见，是"那个格子永远不会触发"。
上游 5 条 `*Pressed*` 行里，4 条与静置行别名同一实例，唯一有像素差的是 `ExpanderChevronPressedBackground`
（`SubtleFillColorTertiary` 对上透明），所以本次放弃的是"按下时箭头底盘变深"这一处视觉，其余四处本来无差别。

**S6 · `HeaderBackground` 不会自己落到像素。**
重模板之后给 `HeaderBackground` 赋一个显色 brushes，`PART_HeaderBorder.Background` 纹丝不动（pass 3 尾段）。
上游把这个 DP 绑在头按钮的 `Background` 上，所以我们的头 `Border` 也必须显式 `TemplateBinding HeaderBackground`，
其余状态格改写 `HeaderBackground` 本身（样式格写 DP，模板读 DP）。

## 1. 行去向表（上游 20 别名 + 2 条分支内 Thickness + 12 条分支外行）

| 上游行 | 处置 | 落点 |
| --- | --- | --- |
| 20 条别名中的 15 条 | 逐字转录 | `ThemeResources/Expander.jalxaml`，模板/样式以 `{ThemeResource}` 消费 |
| `ExpanderHeaderForegroundPressed`、`ExpanderHeaderBorderPressedBrush`、`ExpanderChevronPressedForeground`、`ExpanderChevronPressedBackground`、`ExpanderChevronBorderPressedBrush` | **不发布**（S5：无 `IsPressed` 可监听） | 由 `A_row_with_no_consumer_is_not_published` 反向钉住 |
| `ExpanderHeaderBorderThickness`、`ExpanderChevronBorderThickness` | 转录 | 头/箭头底盘 |
| `ExpanderHeaderPadding`、`ExpanderChevronMargin`、`ExpanderContentPadding`、`ExpanderContentDownBorderThickness` | 转录（`Thickness` 可解析） | 头内边距、箭头底盘外边距、内容内边距与描边 |
| `ExpanderContentUpBorderThickness` | **不发布**（S2：`ExpandDirection` 无人实现，内容永远在下） | 同上钉住 |
| `ExpanderMinHeight`=48、`ExpanderChevronButtonSize`=32、`ExpanderChevronGlyphSize`=12 | 字面量（`x:Double` 不可解析） | `Styles/Surfaces.jalxaml` |
| `ExpanderChevronDownGlyph`/`UpGlyph`（`x:String`）、两条对齐枚举 | **不发布**：箭头是 `Path`，对齐写死 | 同上 |

## 2. 宿主替换清单

| WinUI 侧 | 本运行时 | 依据 |
| --- | --- | --- |
| 头是 `ToggleButton`（`ExpanderHeaderDownStyle` 整份模板） | 头是 `Border`，点击/键盘由控件自己处理 | S3/S4 |
| `AnimatedIcon` + `AnimatedChevronUpDownSmallVisualSource`，回退 `FontIcon` 字形 | `Path` 折角，控件原生旋转 +90 | S2 |
| `ExpandStates` 四个状态的 `Storyboard`（TranslateY + 0.333s 样条） | 控件自带的展开动画（`ExpandCollapseAnimator`，实测 6 帧内可见、收起需更多帧稳定） | pass 1 E/F |
| `TopCornerRadiusFilterConverter`/`BottomCornerRadiusFilterConverter` 磨平接缝 | `IsExpanded=True` 格把头圆角改 `4,4,0,0`，内容侧固定 `0,0,4,4` | 模板内注释 |
| `BackgroundSizing=InnerBorderEdge` | 无该属性，描边一律外缘 | 与其余家族同一偏差 |
| `Expander.TemplateSettings.ContentHeight` | 无 `TemplateSettings` | 探针 B3 |

## 3. 状态映射（WinUI VisualState → 本实现）

| 上游 | 本实现 |
| --- | --- |
| `ExpandStates/ExpandDown`、`CollapseDown`、`ExpandUp`、`CollapseUp` | 控件自身驱动（S1/S2），我们只提供被驱动的三个部件 |
| `ExpandDirectionStates/Down`、`Up` | **无**（S2：方向被框架忽略，`ExpanderContentUpBorderThickness` 因此不发布） |
| 头 `CommonStates/Normal` | 样式 setter：`HeaderBackground`、`Foreground`、头 `BorderBrush`、箭头 `Stroke`/底盘 |
| `PointerOver` | 模板格 `IsMouseOver=True SourceName=PART_HeaderBorder`：5 个值（前景、头描边、箭头描边、箭头底盘前景与描边） |
| `Pressed` | **无**（S5） |
| `Disabled` | 模板格 `IsEnabled=False`：4 个值（前景、头描边、箭头描边、箭头底盘描边） |
| `Checked*` 四态 | 与对应非 Checked 态别名同一实例；展开由 `IsExpanded` 格只改写头圆角 |
| `Indeterminate*` | 不适用（头不是 ToggleButton） |
| 焦点 | 控件级 `FocusVisualStyle={ThemeResource FocusVisualRingStyle}`，门是框架的 `ShowFocusCues`（上游靠头按钮自身）。2026-09-22 改，原来是控件级 `IsKeyboardFocused` 驱动 `FocusOutline`，鼠标点同样抬（`audits/focus-visual.md`） |

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行通过（restore → build → test → 调色板漂移）。
- **行为**：`AstraExpanderInfoBarTests` —— 三个名字仍在（`The_expander_keeps_the_three_names_it_reads`）、
  头不是按钮（S4）、左键 `MouseDown` 恰好展开一次再收起一次（S3）、禁用时不响应、
  `IsExpanded` 驱动内容可见 + 箭头 `RotateTransform.Angle≈90`、
  **负控制** `A_renamed_header_does_not_expand`（改名后既不展开、内容部件仍 `Collapsed`、箭头无变换）。
- **视觉**：同一文件里 `The_header_and_the_content_paint_their_own_card_tokens`
  覆盖 `CardBackgroundFillColorDefaultBrush`/`CardBackgroundFillColorSecondaryBrush` 为哨兵色并数像素
  （>3000 / >1000），`The_expander_takes_our_style_rather_than_the_framework_chrome` 拒绝 `#2C2C2E`，
  `The_expander_follows_the_theme` 断言 Light↔Dark 顶部色不同。
- **硬件输入**：**未做**。`RaiseEvent` 合成的是路由事件，不是指针设备；hover/press 像素与真指针/触摸仍在
  欠账清单（任务 #13）。

## 5. Known Gaps

1. 按下态没有驱动（S5），5 条 `*Pressed*` 行不发布。
2. `ExpandDirection` 不被框架实现：内容永远在下，箭头永远 +90；`ExpanderContentUpBorderThickness` 不发布。
3. 箭头静置朝右（朝下=展开），与 WinUI 的 up/down 180° 语义不同。
4. 头不是 `ToggleButton`，因此没有"头按钮可聚焦"这一层；焦点、Tab 与键盘语义落在 `Expander` 自身。
5. `IsMouseOver`/`IsPressed` 格用 `SourceName` 指向头部件；本运行时是否真的按名字取条件**未经像素验证**
   （需要真指针，任务 #13）。若被忽略，副作用是"鼠标停在正文上时头部也高亮"。
6. 接缝圆角用字面量 `4,4,0,0`，因为 `ControlCornerRadius` 是整体资源、上游的两个转换器不存在。
7. 触摸/笔路径、混合 DPI、高对比逐键断言未做；`ExpanderHeaderBorderThickness` 在 HC 下上游是 2，此处发布 1。
8. `RootBorder`/`PART_HeaderContent` 两个名字保留但**未证明**有行为（只证了 S1/S2/S3 那三个）。

## 更正（属性死写批 2026-09-20，`adaptation/00` S1-g）

§2 部件表里正文 presenter 带的那两条 `HorizontalContentAlignment` / `VerticalContentAlignment`，在
`ContentPresenter` 身上**没有对应成员**（该类型有 `HorizontalAlignment` / `VerticalAlignment`）：属性被读进去、
没人取，"正文对齐跟随控件"这句话从来没落地。同一段另外两条 `Foreground="{TemplateBinding Foreground}"`
（`Styles/Surfaces.jalxaml:52` 头部、`:83` 正文）是 S1-f 已经量到的空转——标签颜色走控件级 `Foreground`
加生成文字的继承。修法：两条对齐改绑 presenter 自身的 `HorizontalAlignment` / `VerticalAlignment`
（复选、单选两个标签模板本来就是这个形状），两条 `Foreground` 删除。
读回见 `AstraSurfaceGeometryTests.The_expander_hands_its_content_alignment_to_the_presenter`
（先 `Right` 后 `Center`，读回随赋值走）。
不声称：对齐与颜色都只到属性读回，**没有像素捕获**；正文 presenter 是按 `PART_ContentBorder` 往下
`Descendant<ContentPresenter>` 取到的，它自己没有名字。
