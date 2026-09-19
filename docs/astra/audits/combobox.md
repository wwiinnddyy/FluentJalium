# ComboBox 审计：上游 103 行怎么砍到 59 行，以及框架替控件自己画的那四样

上游出处：`microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
`controls/dev/ComboBox/ComboBox_themeresources.xaml`，blob `4578f4c9b2e32fc701ca803aba51c032f9c23ead`，
全文 814 行。`ThemeDictionaries` 三段：Default 4-108、High Contrast 109-213、Light 214-318，
每段 103 行；字典外的 29 行度量在 320-348；`DefaultComboBoxStyle` 359-600，
`DefaultComboBoxItemStyle` 602-769，`ComboBoxTextBoxStyle` 770-813。

和 Slider 那次不一样的一点，是先数出来再写代码数出来的：**这个文件的三段作用域行名完全相同**
（Default∆HighContrast = ∅、Default∆Light = ∅），而且 Light 的 61+7 行当前代内容与 Default
逐字节相同——两段只差那 35 行 Win8.1 时代遗留的 `*ThemeBrush` 十六进制。所以一个作用域无关的别名层
就能同时覆盖 Light 与 Dark，高对比靠调色板层的 `SystemColor*` 重映射近似，不需要为 ComboBox 单开一段。

方法照旧（`../ModernWpf` @`23555a6`）：先数上游有多少行，再逐行问"这个运行时有没有能读它的属性"。

## 0. 先把运行时量出来

`ComboBox` 在 26.10.9 上声明 11 个依赖属性（反射实测）：
`IsDropDownOpen, IsEditable, IsReadOnly, MaxDropDownHeight, PlaceholderText, SelectionBoxItem,
SelectionBoxItemTemplate, SelectionBoxItemStringFormat, ShouldPreserveUserEnteredPrefix,
StaysOpenOnEdit, Text`，外加 `Selector`（`SelectedIndex/SelectedItem/IsSelectionActive`…）、
`ItemsControl`、`Control`（`Background/BorderBrush/BorderThickness/CornerRadius/Padding`）、
`UIElement`（`IsMouseOver/IsMouseCaptureWithin/IsKeyboardFocused/AreAnyTouchesCaptured`）几层继承。
`ComboBoxItem` 只加一个 `IsHighlighted`（setter 不公开），`IsSelected` 从 `ListBoxItem` 继承，
**没有 `IsPressed`**。没有 `Header`（和 Slider 同一件事），没有 `BorderVisual`。

这一批量到的框架行为，比颜色更需要写下来——它们决定模板能碰什么：

1. **五个 `PART_` 名是框架契约。** `OnApplyTemplate` 按名字取
   `PART_ToggleButton / PART_Popup / PART_EditableTextBox / PART_SelectionPresenter / PART_DropDownArea`，
   取不到就静默失去对应能力（`GetTemplateChild(...) as T`）。上一版模板已经在用这套名字，保留。
2. **框架替控件画四样东西。** 实测：`IsEditable` 改变 `PART_EditableTextBox` 与
   `PART_SelectionPresenter` 的 `Visibility`（模板不能自己加这条触发）；`Text` 与可编辑框双向同步
   （往部件里打字 `combo.Text` 跟着变，反向同理）；弹层打开时框架把 `PART_Popup.Width` 设成控件宽
   （220→220）；**箭头是框架改 `Path.Data` 实现翻转的**，静止 `M 0,1.5 L 5,6.5 L 10,1.5`，
   打开后换成翻转的那串，且不动 `RenderTransform`——模板自己再加一条旋转就会翻两次。
3. **占位符是框架塞进 `SelectionBoxItem` 的。** `SelectedIndex=-1` 时 `Text` 是 `""` 而
   `SelectionBoxItem` 等于占位字符串（实测读回 `"Pick one"`），所以"没有选中项"这件事**不能**用
   `SelectionBoxItem={x:Null}` 判据表达，只能用 `SelectedIndex`。
4. **空字符串条件在本运行时永不匹配。** 先按 `Trigger Property="Text" Value=""` 写了六格，
   结构测试全过（属性解析成功、键名正确），但读回的选择框前景一直是控件自己的
   `#E4000000`；同一格换成 `SelectedIndex=-1` 后立刻读到 `ComboBoxPlaceHolderForeground` 实例，
   且 `presenter.ReadLocalValue(...) == UnsetValue` 证明这不是谁写了本地值压住它。
   这是一条新的基座契约：`Value=""` 在 JALXAML 里不等于"匹配空字符串"。
5. **禁用时框架在控件自己身上写前景。** `IsEnabled=false` 之后挂上去的 `ComboBox.Foreground`
   读回 `#FFAEAEB2`（本地值），压过样式 setter 与禁用格——`TextBox.Foreground`、
   `PasswordBox.Background` 之后这张账单的第三次。后果：`ComboBoxForegroundDisabled` 只到得了
   表面与箭头，到不了文字；`ComboBoxPlaceHolderForegroundDisabled` 无处可用，所以没抄。
6. **合上的 ComboBox 没有弹层子树。** 关闭态在控件树里找不到 `PART_PopupBorder`，打开后整块
   `PopupRoot` 出现在窗口 `OverlayLayer` 里。关于弹层表面的断言必须从宿主窗口取，不能从控件取。

## 1. 68 行当前代，抄了 59 行

| 上游段 | 行数 | 本仓 | 说明 |
|---|---|---|---|
| Item 别名（5-31） | 27 | 24 | 三条 `*SelectedUnfocused` 未抄：见下 |
| 控件别名（32-65） | 34 | 28 | 2 行 header、1 行 light-dismiss、2 行 `*Unfocused` 未抄 |
| legacy `*ThemeBrush`（66-100） | 35 | 0 | 当前模板不读，与 Slider 同一处理 |
| 尾部别名（101-106） | 6 | 6 | 箭头小片 + 选中 pill |
| `ComboBoxDropdownBorderPadding`（107） | 1 | 1 | Thickness，可声明 |
| **合计** | **103** | **59** | 9 行按实测原因不抄，35 行遗留不抄 |

字典外 29 行度量：抄 11 行（Thickness/CornerRadius 能被本 reader 解析），
11 行是 `x:Double`/`x:Int32`/`x:String`（解析不了，按 Slider 批的结论留在模板里当字面量：
`ComboBoxArrowThemeFontSize 21`、`ComboBoxThemeMinWidth 64`、`ComboBoxMinHeight 32`、
`ComboBoxItemPill{Height 16,Width 3,MinScale 0.625}`、`ComboBoxPopupTheme{MinWidth 80,TouchMinWidth 240}`、
`ComboBoxPopupMaxNumberOfItems 15`、`…OnOneSide 7`、`ComboBoxItemScaleAnimationDuration 00:00:00.167`），
7 行是这套运行时没有的功能位（3 行 header、2 行 InputMode 内边距、`ComboBoxPlaceholderTextThemeFontWeight`、
遗留的 `ComboBoxPopupBorderThemeThickness`）。

不抄的 9 行别名，每一行都指着一条实测：

| 行 | 原因 |
|---|---|
| `ComboBoxHeaderForeground` / `…Disabled` | ComboBox 没有 `Header` 属性（反射实测） |
| `ComboBoxLightDismissOverlayBackground` | 运行时 `Popup` 有 `IsLightDismissEnabled`，没有遮罩刷子槽位 |
| `ComboBoxBackgroundBorderBrushUnfocused` | 上游自己也不读：全文件 3 次出现=3 次声明，0 次引用（实测计数） |
| `ComboBoxBackgroundUnfocused` | 只被高对比段的 `ComboBoxBackgroundFocused` 别名指向，模板不读，两行三段同值 |
| `ComboBoxPlaceHolderForegroundDisabled` | 见 §0.5：框架在控件上写了本地前景，模板格压不过 |
| `ComboBoxItem{Background,BorderBrush,Foreground}SelectedUnfocused` | 上游给"选中但下拉失焦"用；本运行时失焦即关闭弹层，且 `IsSelectionActive` 在 ComboBox 上而非条目上，条目模板无从判据 |

**顺带纠正一处我自己写错的结论**：`audits/textbox-passwordbox.md` 记着 `ComboBoxPadding` 与
`RadioButtonContentMargin` "不是上游键名"。前者不成立——上游第 341 行就是
`<Thickness x:Key="ComboBoxPadding">12,5,0,7</Thickness>`；真正的错是**值**：那行从 `Metrics.jalxaml`
搬过来时抄成了 Button 的 `11,5,11,6`，右边差了 0/6。`RadioButtonContentMargin` 那条不变——它确实不是上游键
（上游把这段间距写成样式 setter `Padding=8,6,0,0`，我们的样式也已经是这个值），AutoSuggestBox 批把那行
从 `Metrics.jalxaml` 删掉了，理由记在该文件头注里。

## 2. 值替换（写代码前就知道要偏离的地方）

1. `ComboBoxBorderBrush` / `…PointerOver` 上游指 `ControlElevationBorderBrush`。渐变带不动
   就地重染那套模型（`{ThemeResource}` 写进的停靠点会冻），照 Button/Slider 的先例取实心那档
   `ControlStrokeColorDefaultBrush`——上游自己的高对比段也是实心的。
2. `ComboBoxDropDownGlyphForegroundFocusedPressed` 上游指遗留的
   `SystemControlHighlightAltBaseMediumHighBrush`，不在我们调色板里；取聚焦箭头同一个
   `TextFillColorSecondaryBrush`。
3. 焦点视觉换成上游的 `HighlightBackground` 光晕（`Margin=-4`、`ComboBoxBackgroundFocused` 填充、
   `ComboBoxBackgroundBorderBrushFocused` 描边、2 DIP、圆角 7），删掉上一版自造的
   `ComboFocus` 描边环——WinUI 的 ComboBox 焦点态本来就不是那圈 2px 环。
   顺带：`ComboBoxBackgroundFocused` 与 `ComboBoxBackgroundUnfocused` 三段同值，高对比段那条自指
   因此不需要用第二个键表达。
4. `ComboBoxDropDownBackground` 上游是 `AcrylicInAppFillColorDefaultBrush`（真材质）。本仓别名层
   直接转发该调色板键，Light/Dark 拿到的是该键当前的近似值，材质本体仍归阶段 3 材质摸底（任务 #8）。
5. 条目根 `Border` 的九条 `ComboBoxItemBorderBrush*` 照上游写进格子，但条目样式不给
   `BorderThickness`（上游也不给），所以这些格子在哪个主题下都不画东西——抄它是为了让
   "按键名覆盖"这句话在状态表上成立，不是因为看得见。
6. `ComboBoxDropDownGlyphStyle` 是本批新增的**内部辅助键**，不是上游键：箭头在
   `PART_ToggleButton` 的嵌套模板里，ComboBox 层的格子够不到 `Path.Stroke`，于是让箭头的
   `Stroke` 走 `{TemplateBinding Foreground}`，静止值由这条样式 setter 提供（样式优先级低于模板格，
   格子才压得过）。上游同一位置用的是 `ComboBoxTextBoxStyle` 这种"部件样式"做法。

## 3. WinUI 视觉状态 → 本运行时触发格

| 上游组/状态 | 本仓格 | 判据 |
|---|---|---|
| CommonStates/Normal | 样式 setter（`ComboBoxBackground/BorderBrush/Foreground`） | — |
| CommonStates/PointerOver | `Trigger IsMouseOver=True` | 与上游同 |
| CommonStates/Pressed | `Trigger IsMouseCaptureWithin=True` | ComboBox 不是 ButtonBase，没有 `IsPressed`；按下由内部 toggle 捕获，故取"捕获在自己子树内" |
| CommonStates/Disabled | `Trigger IsEnabled=False`（倒数第二格） | 表面/边框/箭头/光晕生效；文字受 §0.5 限制 |
| FocusStates/Focused | `Trigger IsKeyboardFocused=True` → 光晕 `Opacity=1` | 上游同一格 |
| FocusStates/FocusedPressed | `MultiTrigger(IsKeyboardFocused, IsMouseCaptureWithin)` | 上游同 |
| FocusStates/Unfocused, PointerFocused | 不映射 | 上游这两格本身是空的 |
| FocusStates/FocusedDropDown | 不映射 | 上游只把 `PopupBorder` 设成可见，本运行时弹层可见性由框架管 |
| DropDownStates/Opened, Closed | 不映射 | 上游是 `SplitOpen/CloseThemeAnimation` + `TemplateSettings`，属动效批（B5） |
| EditableModeStates/TextBoxFocused | `Trigger IsEditable=True`（箭头色 + 小片可见） | 上游用 `x:Load`，本 reader 没有，改判 `Visibility` |
| EditableModeStates/TextBoxOverlay{PointerOver,Pressed} | `MultiTrigger(IsEditable, IsMouseOver / IsMouseCaptureWithin)` | 上游同 |
| EditableModeStates/TextBoxFocusedOverlay{PointerOver,Pressed} | `MultiTrigger(IsKeyboardFocused, IsMouseOver / IsMouseCaptureWithin)` | 上游同（不加 IsEditable：小片在静止态已 `Collapsed`，非编辑态改它的颜色看不见） |
| 占位符六个前景 | `SelectedIndex=-1` 打头，配 `IsMouseOver/IsMouseCaptureWithin/IsKeyboardFocused` 与三条件格 | 上游靠 `PlaceholderForeground` 绑定回落；本运行时没有该属性，改由格直接写选择框前景 |
| Item CommonStates/{Normal,PointerOver,Pressed,Selected,SelectedPointerOver,SelectedPressed,Disabled,SelectedDisabled} | 9 格（`IsMouseOver`、`IsHighlighted`、`IsMouseCaptureWithin`、`IsSelected` + 三条组合 + 两条禁用） | `IsHighlighted` 是框架给"指针或键盘高亮那一条"的状态，与上游 PointerOver 同源 |
| Item SelectedUnfocused | 不映射 | 见 §1 |
| Item InputModeStates/{Touch,GameController} | 不映射 | 本运行时无输入模式状态组，对应两行内边距未抄 |

三条件 `MultiTrigger` 是这批第一次用到的形状，所以按条件逐个读回属性名，而不是假设它成立
（`Both_combo_templates_carry_one_cell_per_upstream_state` 末尾那条）。

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移；
  Debug 构建 0 警告 0 错误；闸口 209 项全过（`AstraComboBoxTests` 自己 71 项），跳过 0。
- **行为**：`AstraComboBoxTests` —— 框架保留的四样（部件可见性、双向文本同步、弹层宽度、箭头几何）；
  占位符随选中态得失（`SelectedIndex` 三次来回）；捕获/焦点驱动的 Pressed 与 Focused 格；
  可编辑小片的装载与禁用覆盖；条目 pill 随 `IsSelected` 起落，禁用选中对仍赢。
- **视觉**：静止态离屏捕获里 `ControlFillColorDefaultBrush` 哨兵 >3000 px、
  品牌绿 `#207245` 恒为 0、染上的 accent 行不进闭合态；同一场景 Light 与 Dark 主色分布不同。
- **硬件输入**：**没有真指针/真触摸证据**——Pressed 族是拿 `CaptureMouse()` 在 toggle 上驱动
  `IsMouseCaptureWithin` 得到的，键盘焦点是 `Focus()` 直接调用得到的；
  任务 #13（真指针像素通路）仍未结。
- **Gallery 冒烟**（第 7 步的运行时侧）：`FluentJalium.Gallery.exe` 上屏（dpi=168，1925×1435）、
  `CloseMainWindow` 优雅退出 code=0、无残留进程。可见的 Overview 页本来就挂着一个 `ComboBox`
  （`MainWindow.jalxaml:167` 的 `ProfileComboBox`），它这次是**带着新模板上屏并被测量过**的：
  启动不炸，说明隐式样式命中、内联模板能实例化、模板里的 `{ThemeResource}` 别名行在活窗口里解析得到。
  Selection 页新加的四个 ComboBox 在 `InitializeComponent()` 期就被构造（六页共用一份标记），
  但页面切换走 `ContentHost.Children`，所以它们**没进可视树、模板未应用**。
  另外这次截图被一个外部"Windows 安全中心 / server.exe"防火墙弹窗盖住（不是我们的窗口，未点它），
  图片没有留档，也没有作为任何视觉结论。

## 5. 不声称清单（Known Gaps）

1. 不声称鼠标悬停/按下/触摸的**像素**：见上，`IsMouseCaptureWithin` 是可达判据的替代，
   真实指针路径未测。
2. 不声称弹层动效：`SplitOpen/CloseThemeAnimation`、`OverlayOpening/OverlayClosingAnimation`、
   pill 的 `ScaleY 0.625` 弹入都没落（动效批）。
3. 不声称 Header/Description：运行时没有这两个属性，相关 5 行未抄。
4. 不声称高对比逐键一致：本文件的别名层走的是调色板级近似，上游那 103 行高对比目标
   （全 `SystemControl*`）没有逐键断言。
5. 不声称禁用态文字色是上游那档：框架在控件上写 `#FFAEAEB2` 本地值，我们的
   `ComboBoxForegroundDisabled` 到不了文字（§0.5）。
6. 不声称 `MaxDropDownHeight=504` 与上游弹层约束等价：上游还有 `ComboBoxPopupMaxNumberOfItems`
   一类的 C++ 侧计算，本运行时只是把值绑到了 `ScrollViewer.MaxHeight`。
7. 不声称条目 9 条边框行画得出东西：条目不给 `BorderThickness`，与上游同样画不出（§2.5）。
8. 不声称箭头几何与上游一致：上游是 `AnimatedChevronDownSmallVisualSource`（回落 `FontIcon &#xE70D;`），
   本运行时由框架写死一串 `Path.Data`，我们只提供颜色与 `StrokeThickness=1.25`。
9. 不声称开合状态机没有回归：`IsDropDownOpen` 与 toggle 的 `IsChecked` 同步、
   弹层关闭回调把 `IsDropDownOpen` 写回 false 都是读回来的，没有真点击验证。
10. 不声称 Gallery Selection 页的 ComboBox 已被像素证明：那四个新卡片只是被构造，没进可视树，
    模板未应用；冒烟能给的只有 Overview 那一个既存 ComboBox 的"带新模板上屏不炸"。逐页渲染到像素
    仍是 E4（`adaptation/10`）。


## 7. 间距批（2026-09-19）：条目行内缩 5,2,5,2

上游 `ComboBox_themeresources.xaml:614` 把行内缩写在**模板根** `LayoutRoot` 的 `Margin="5,2,5,2"` 上，
不在条目的 `Padding` 里——`ComboBoxItemThemePadding`（`11,5,11,7`，:335/:606）管的是文字到行的距离，
那 5 管的是行到下拉边框的距离。本实现原来只有后者，所以悬停/选中的高亮一直铺到弹窗描边。
现在补上同一条字面量（上游它就是字面量，不是行，因此不新发键），药丸随根一起内缩，与上游一致。

判据不是"标记里有这条属性"，而是排布尺寸：`AstraComboBoxTests` 在打开的下拉里读回
高亮 `ActualWidth = 条目 ActualWidth − 10`、`ActualHeight = 条目 ActualHeight − 4`。
另记一笔未治：条目样式上的 `MinHeight=32` 与 `FontSize=14` 是自加的（上游 `DefaultComboBoxItemStyle`
:602–610 两条都没有，32 是它的 padding 与字号自然量出来的高度），留着是因为去掉后本运行时的行高要另量。
