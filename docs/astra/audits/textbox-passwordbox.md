# TextBox / PasswordBox 审计（选择批第二段）

- 上游：`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/TextBox_themeresources.xaml`，blob `6934b646465eb602218f2ebf950268ac689ea33c`
  - `controls/dev/CommonStyles/PasswordBox_themeresources.xaml`，blob `ec52d3f52cdda9002a858000b359ac5752c8eb22`
  - 度量行：`controls/dev/CommonStyles/Common_themeresources.xaml`（`TextControlBorderThemeThickness` 等）
- 方法参照：`../ModernWpf` @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`，`ModernWpf/Styles/TextBox.xaml`、
  `ModernWpf/ThemeResources/Dark.xaml:1926-1939`
- 我们的落地：`ThemeResources/TextBox.jalxaml`（别名与度量）、`Styles/TextInput.jalxaml`（两份样式）

## 0. 为什么这一段先量运行时，而不是先抄表

抄上游别名表的前提是"这个键在这里有东西能读"。上游一张表里读得通的东西，本机不一定存在对应属性；
属性不存在时 Jalium **静默丢弃** setter（见 `adaptation/00`、`memory: jalium-xbind-silent-drop`），
构建和渲染都照常通过。所以本段第一步是反射出 26.10.9 里两个文本控件的真实属性面：

| 读取项 | `TextBox` | `PasswordBox` | `ComboBox` |
|---|---|---|---|
| `PlaceholderText` | **无** | **无** | 有 |
| `SelectionBrush` | 有 | 有 | — |
| `CaretBrush` | 有 | 有 | — |
| `Header` / `Description` | 无 | 无 | 无（有 `PlaceholderText`） |
| 声明属性（DeclaredOnly，14/10 项） | `CaretIndex, CharacterCasing, LineCount, MaxLength, MaxLines, MinLines, SelectedText, SelectionLength, SelectionStart, Text, TextAlignment, TextDecorations, TextWrapping, Typography` | `CaretBrush, IsInactiveSelectionHighlightEnabled, IsSelectionActive, MaxLength, Password, PasswordChar, SecurePassword, SelectionBrush, SelectionOpacity, SelectionTextBrush` | `IsDropDownOpen, IsEditable, IsReadOnly, IsSelectionBoxHighlighted, MaxDropDownHeight, PlaceholderText, SelectionBoxItem, SelectionBoxItemStringFormat, SelectionBoxItemTemplate, ShouldPreserveUserEnteredPrefix, StaysOpenOnEdit, Text` |

这张表推翻了我自己上一轮的survey两处：
- 我以为 `TextControlSelectionHighlightColor` 没有消费点（把 `SelectionBrush` 记成不存在）；实测 `TextBox` 和
  `PasswordBox` 都有 `SelectionBrush`，这一行能落地。
- 我在动手前的计划里写"顺手加 `SelectionBrush` = 上游键"，方向对，但理由当时是猜的。
读取方式：临时反射探针跑在测试工程里（同 NuGet 26.10.9，同 `net10.0-windows`），一次性，读完即删；
把"哪个属性存在"升级为一条长期闸口 `The_rows_left_out_are_left_out_because_no_property_can_read_them`，
将来框架加上 `PlaceholderText` 时该测试会红，逼着重做决定，而不是让字典悄悄漂。

## 1. 转录规模

| 上游 Light/Default 分支 | 行数 | 我们的处置 |
|---|---|---|
| `TextControlBackground*` | 4 | 逐字 |
| `TextControlBorderBrush*` | 4 | 3 行替换（见 §2） |
| `TextControlForeground*` | 4 | 1 行替换 |
| `TextControlSelectionHighlightColor` | 1 | 逐字 |
| `TextControlPlaceholderForeground*` | 4 | **不声明**：无 `PlaceholderText` 属性 |
| `TextControlButton*` | 8 | **不声明**：模板里没有删除按钮部件 |
| 合计 | 25 | 声明 13，10 行逐字，3 行替换，12 行给出实测理由 |

PasswordBox 的 `PasswordBox_themeresources.xaml` 在 Light 分支里**没有别名行**：上游它的样式直接消费
`TextControl*` 这一族（`grep` 其样式得到的 40 个 `ThemeResource` 名字里只有
`PasswordBoxTopHeaderMargin`、`PasswordBoxIconFontSize`、`TextBoxInnerButtonMargin` 是自家度量，其余全是
`TextControl*`）。所以我们一份 `TextBox.jalxaml` 同时喂两个控件，与上游一致。
两个 `PasswordBox*` 度量行未声明：前者服务没有的 Header，后者是 `x:Double`（本机 reader 解析不了）。

度量行取自 `Common_themeresources.xaml`：

| 键 | 上游值 | 我们的处置 |
|---|---|---|
| `TextControlBorderThemeThickness` | `1` | 逐字（Thickness） |
| `TextControlBorderThemeThicknessFocused` | `1,1,1,2` | 逐字（Thickness） |
| `TextControlThemePadding` | `10,5,6,6` | 逐字；**同时删掉我们自造的 `TextControlPadding` = `11,5,11,6`** |
| `TextControlThemeMinHeight` / `MinWidth` | `32` / `64` | 值是 `x:Double`，键名无法转录，数值以字面量进样式 |
| `TextBoxTopHeaderMargin` `0,0,0,8`、`TextBoxInnerButtonMargin` `0,4,4,4` | — | 无部件可读，不声明 |

`TextControlPadding` 这条是自造的，而且值抄的是按钮的 padding：`11,5,11,6` 对上游 `10,5,6,6`。
两边不一致会让"改上游键名"这个承诺落空，所以名字和值一起改成上游的。
顺带记下同类问题（本段不动，等 ComboBox 里程碑一起处理）：`Metrics.jalxaml` 里
`ComboBoxPadding`、`RadioButtonContentMargin` 也不是上游键名。

## 2. 三处替换，以及为什么只能替换

别名行的机制事实（`Themes/FluentThemeManager.cs` `RefreshPalette`）：主题切换时只把
`SolidColorBrush` **原地改色**保持实例不变，其他类型的值是整个替换。
而 `<StaticResource x:Key="A" ResourceKey="B"/>` 在加载时就把 B 的**实例**绑给了 A。
两条合起来 ⇒ 别名层只能转发 SolidColorBrush；转发的目标若是 Color 或渐变，切换主题时会冻在 Light 上。
上游恰好有三行是这种情况：

| 行 | 上游目标 | 我们给的 | 差别 |
|---|---|---|---|
| `TextControlBorderBrush` | `TextControlElevationBorderBrush`（LinearGradientBrush：上边 `ControlStrongStrokeColorDefault`，下边 `ControlStrokeColorDefault`） | `ControlStrokeColorDefaultBrush` | 1px 边框取渐变的下边色；上边强色由模板的底边元素画同一个 token |
| `TextControlBorderBrushPointerOver` | 同上 | 同上 | 同上 |
| `TextControlBorderBrushFocused` | `TextControlElevationBorderFocusedBrush`（两个 stop 都在 offset 1.0：`SystemAccentColorLight2` 压 `ControlStrokeColorDefault`） | `AccentFillColorDefaultBrush` | 上游配 `1,1,1,2` 的厚度实际画出来就是"下边 2px 强调色"；我们把强调色放在底边元素上，且跟随应用强调色而非 `Light2` 一档 |
| `TextControlForegroundDisabled` | `TemporaryTextFillColorDisabled`（Color：Light `#5DFEFEFE`，Dark `#5C010101`，上游自己叫 temporary） | `TextFillColorDisabledBrush`（`#5C000000`） | 上游禁用文字是"36% 近白"，我们换成"36% 近黑"。ModernWpf 用 `<SolidColorBrush Color="{StaticResource TemporaryTextFillColorDisabled}"/>` 保住上游值；我们若这么做会把 Light 的值冻进共享字典，Dark 就再也翻不过来 |

结论：这是**值**偏差，不是键名偏差；覆盖 `TextControlBorderBrush` 这一族的上游名字仍然有效。
若将来 `Application.ThemeMode` 之外找到能让非画刷值随主题重解析的路子，这四格应当退役成逐字转录。

## 3. VisualState → Trigger 映射

上游 `DefaultTextBoxStyle` 两个状态组：`CommonStates`（Normal / Disabled / PointerOver / Focused）与
`ButtonStates`（ButtonVisible / ButtonCollapsed）；`DeleteButtonStyle` 里另有一个四态 `CommonStates`。

| 上游状态 | 上游动效目标 | 我们的触发条件 | 我们写的格 |
|---|---|---|---|
| Normal | — | 静息格（样式 setter + 模板 setter） | `OuterBorder.Background/BorderBrush/BorderThickness`、`BottomEdge.Background`、`Foreground`、`CaretBrush`、`SelectionBrush` |
| PointerOver | `BorderElement` Background+BorderBrush、`ContentElement` Foreground（+ Placeholder） | `IsMouseOver=True` | 3 条 setter（Placeholder 无消费点） |
| Focused | `BorderElement` Background+BorderBrush+BorderThickness、`ContentElement` Foreground（+ Placeholder） | `IsKeyboardFocusWithin=True` | 4 条：`TextControlBackgroundFocused`、`TextControlBorderThemeThicknessFocused`、底边 = `TextControlBorderBrushFocused`、`TextControlForegroundFocused` |
| Disabled | `HeaderContentPresenter`、`BorderElement` Background+BorderBrush、`ContentElement` Foreground、`PlaceholderTextContentPresenter` | `IsEnabled=False`（放在最后，覆盖 hover/focus） | 5 条：两_surface_ + 底边 + `Foreground` + `CaretBrush` |
| ButtonVisible / ButtonCollapsed | `DeleteButton.Visibility` | 无 | 没有删除按钮部件 |
| 删除按钮自己的 PointerOver / Pressed / Disabled | `ButtonLayoutGrid`、`GlyphElement` | 无 | 同上；这也是 8 行 `TextControlButton*` 不声明的原因 |

**没有 Pressed 格**：上游文本框的 `CommonStates` 里就没有 Pressed，只有删除按钮有。这条与 Button 族不同，
写下来是为了防止下一段照抄按钮的格子表。

部件对应：上游 `BorderElement` ↔ `OuterBorder`；上游 `ContentElement` 是个 `ScrollViewer`，
本机 `TextBox` 要的是 `PART_ContentHost`（框架往里塞输入宿主），类型换成了 `Grid`；
上游 `HeaderContentPresenter`、`PlaceholderTextContentPresenter`、`DeleteButton`、`DescriptionPresenter`
四个部件在本机没有可读属性，整个不建。`BottomEdge` 是我们自加的元素，用来承接上游渐变做的那条底边。

PasswordBox 没有可用模板（框架自绘），所以三格写在 `Style.Triggers` 上，属性落在控件自身；
差别记在表里：没有底边元素，因此 `TextControlBorderThemeThicknessFocused` 与
`TextControlBorderBrushFocused` 在它身上合成"整圈强调色边框"。

## 3.5 一处新测到的架构代价：原生默认会压过我们的 setter

阶段 1 决定把原生控件默认交回 Jalium（`ApplyNativeThemeMode` 设 `Application.ThemeMode`），
好处是不重模板也能跟着明暗走。这两段第一次测出它的账单——**框架在某些状态上给控件设的是本地值，
而本地值优先级高于样式 setter 和模板触发器 setter**：

| 读数（26.10.9，Light，实测） | 我们写的 | 挂到树上后读回 |
|---|---|---|
| `TextBox.Foreground`（`IsEnabled=false` 挂载） | `TextControlForegroundDisabled` = `#5C000000` | `#FFAEAEB2`，框架自己的禁用灰 |
| `PasswordBox.Background`（静息） | `TextControlBackground` = `#B3FFFFFF` | `#D9FFFFFF`（scRGB 0.8526,1,1,1） |
| `TextBox.Background` / `Foreground`（静息） | 同上两族 | **就是我们的实例**（`Assert.Same` 通过） |
| `TextBox` 模板件 `OuterBorder.Background/BorderBrush`、`BottomEdge.Background`（禁用格） | 三个 disabled 键 | 全部命中实例 |

也就是说：文本框的**面**（我们模板里的 Border）拿得到，**控件自身**在框架插手的那个状态上拿不到。
两条 `Assert.NotSame` 就钉在测试里，防止下一次有人把"setter 上有键"当成"像素上是这个颜色"。
可选的解法只有两种：把 PasswordBox 也重模板（要先证明框架不会同样设本地值），或者放弃
`Application.ThemeMode` 的原生默认（阶段 1 的实测说这样会丢明暗跟随）。本段都不做，先记账。

## 4. 四类证据（分开记）

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，
  实测 **104/104 通过、0 警告、0 跳过**，调色板 Light/Dark 各 83 色→102 刷 `checked=True`，
  高对比 102 键 `checked=True`（本段没有新增调色板行）。
- **结构 / 键**：`An_upstream_text_control_row_hands_back_the_palette_instance_it_aliases` 13 条
  `[Theory]`，逐行断言上游名解析到的就是调色板那个实例（`Assert.Same`，不是同色）；
  `Transcribed_control_rows_are_read_by_a_template` 把 `ThemeResources/TextBox.jalxaml` 纳入"声明的 16 行
  必须全部被模板读到"的闸口；新增 `Style_setters_name_properties_the_controls_actually_have`，把
  `Styles/*.jalxaml` 里每条不带 `TargetName` 的 setter/condition 属性名拿去反射比对 DependencyObject 类型
  （本仓全部样式，闸口自身带 `>100` 条的下限断言防止空跑），静默丢弃这一族缺陷第一次有了通用闸口。
- **行为 / 读回**：`The_text_box_template_carries_one_cell_per_upstream_state`（三格逐条键名，
  含 `IsEnabled` 的 `Value=False` 实测形状）、
  `The_password_box_style_reads_the_same_family_it_shares_with_the_text_box`、
  `Both_text_styles_size_themselves_from_upstreams_metric_rows`（键名 + 挂树后的有效 `Padding`/厚度/32/64/
  `SelectionBrush` 实例）、`A_disabled_text_box_drops_its_surfaces_to_the_disabled_rows`、
  `A_focused_text_box_widens_only_the_bottom_edge`：在宿主窗口里真的调用 `Focus()` 并断言它返回 true，
  再读回 `1,1,1,2` 与底边实例——这是本仓第一条**键盘焦点态**的读回证据（Button 批的 Pressed 仍无）。
- **像素**：`A_resting_text_box_paints_its_surface_token_and_nothing_accented`——一次捕获、两个哨兵色：
  `ControlFillColorDefaultBrush` 覆盖后必须在 120×32 的框里出现 >1500 px，同时强调色哨兵必须为 0 px。
  本段没有第二格像素断言，原因见 §5。

## 5. Known Gaps（不许用相邻证据替代）

- 不声称聚焦态有像素证据。选择批里"一次 fixture 调用做两次捕获"两次挂满 60 秒看门狗且未归因，
  本段每格只留一次捕获；聚焦格的证据止于真实 `Focus()` + 读回。
- 不声称 Placeholder：4 行不声明是属性缺失的实测结论，不是偷懒；但这意味着上游"空文本时显示占位文案"
  这个可见行为我们根本没有，Gallery 里也没有可对比的卡片。
- 不声称删除按钮：8 行 `TextControlButton*` 无消费点；上游"输入后悬停出现清除键"的行为缺失。
- **不声称禁用文字色是我们的**：上游那格本身是 `#5DFEFEFE`（Light）/`#5C010101`（Dark）的 Color，我们换成
  `#5C000000` 的 `TextFillColorDisabledBrush`；更要紧的是挂树读回发现框架在禁用态给 `TextBox.Foreground`
  设了本地值 `#FFAEAEB2`，所以我们连"换成我们自己那支"都没做到（§3.5）。同理不声称 `CaretBrush` 覆盖得掉。
- **不声称 PasswordBox 的悬停/聚焦面画对了**：它静息 `Background` 读回就是框架的 `#D9FFFFFF`，
  三格写的是 Style trigger，对本地值谁赢没有测；本段没给它任何像素断言。
- 不声称底边线与上游逐位一致：上游是 2px 绝对坐标渐变，我们是 1px 元素 + 聚焦时加厚边框，
  抗锯齿与圆角裁剪处不会相同。
- 不声称打字、选区、IME、密码显隐任何路径：`SelectionBrush` 只证明接上了 token 实例，选中像素、
  拖动选区、中文 IME 组合、`PasswordChar` 圆点全部未测；PasswordBox 根本没有显隐按钮。
- 不声称触摸：本段没有任何触摸输入证据，键盘也只有 `Focus()` 一步（Tab 进入、Ctrl+A、退格未测）。
- 不声称 Gallery 的 Inputs 页画对了：本段没有跑页面渲染，页面上的 TextBox/PasswordBox 卡片可见结果未目视。
- 未处理：`Metrics.jalxaml` 里 `ComboBoxPadding`、`RadioButtonContentMargin` 同样是自造键名，
  留给 ComboBox / 选择批收尾。
- 未归因：带文本的 TextBox 在 `Build` + 实时翻转 `IsEnabled` + `Settle` 的组合下挂满 60 秒看门狗；
  去掉文本与实时翻转后同一组断言 568 ms 通过。文本捕获贵是 `adaptation/06` 量过的，这一条没有单独定位。
