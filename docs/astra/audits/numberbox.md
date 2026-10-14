# NumberBox 审计（阶段 3 第五段）

参考：`../microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，只读。

| 上游文件 | blob(8) | 用途 |
|---|---|---|
| `controls/dev/NumberBox/NumberBox_themeresources.xaml` | `3b532d4b` | 13 个 NumberBox* 键（Light/Default/HighContrast 各 6 + 作用域外 7） |
| `controls/dev/NumberBox/NumberBox.xaml` | `d70025ee` | 控件模板、`NumberBoxSpinButtonStyle`、`NumberBoxPopupSpinButtonStyle`、`NumberBoxTextBoxStyle`、模板内 RepeatButton* 别名块 |

方法参考：`../ModernWpf` @`23555a6c`（同样把 spin 按钮的别名放在模板局部作用域里）。

---

## 0. 先量后写：这一批量到的运行时事实

**目标把 NumberBox 标成"自有类型"，实测不成立。** Jalium.UI 26.10.9 有原生
`Jalium.UI.Controls.NumberBox : TextBoxBase`，声明属性 16 个：`AcceptsExpression`、`CaretIndex`、
`DecimalPlaces`、`Header`、`IsImeAllowed`、`IsImeComposing`、`IsWrapEnabled`、`LargeChange`、`Maximum`、
`Minimum`、`NumberFormatter`、`PlaceholderText`、`SmallChange`、`SpinButtonPlacementMode`、`Text`、`Value`，
外加 `StepUp()` / `StepDown()` / `OnApplyTemplate()`。默认值：`SpinButtonPlacementMode=Inline`、
`DecimalPlaces=-1`、`SmallChange=1`、`Minimum/Maximum=±double.MaxValue`。
`NumberBoxSpinButtonPlacementMode = {Hidden, Inline, Compact}`，与上游同名同序。
按 AGENTS.md"原生控件优先 .jalxaml 重模板，只有证明的行为缺口才自有类型"，这里走重模板。

下面每条都有断言或探针支撑，探针文件不入库（结论进本文与 `adaptation/00`）。

1. **它自带代码构建的默认模板，且没有任何样式。** 全新 `NumberBox` 挂上屏后 `Template` 非 null、树里有
   `OuterBorder`/`PART_LayoutRoot`/`PART_ContentHost`/`PART_UpSpinButton`/`PART_DownSpinButton`，
   而 `ApplyTemplate()` 在挂载前返回 `False`、`Style` 为 null、`Template` 无本地值。
   这**更正了普查的两处说法**：`adaptation/01` 的"163 个控件 0 个有框架默认样式"仍然对（确实没有*样式*），
   但"没有样式"不等于"没有外观"；`adaptation/05` 把 NumberBox 记成"自绘，模板只能改外围"是错的——
   它有可替换的 `ControlTemplate`，部件是真 `RepeatButton`。
2. **我们的 Style 能换掉它。** 带 `Template` setter 的样式装上后 `ReferenceEquals(box.Template, ours) == true`，
   哨兵色进像素。
3. **文本宿主必须是一个面板。** 框架把 `TextBoxContentHost` 嫁接进名为 `PART_ContentHost` 的 **Grid**；
   同名元素换成 `ContentPresenter` 时不嫁接，整块捕获只剩 2 个颜色（文字面失效）。
4. **框架在两个 spin 部件上写本地值。** 挂载后 `PART_UpSpinButton` 本地 `BorderThickness=1,0,0,0`、
   `CornerRadius=0,4,0,0`，`PART_DownSpinButton` 是 `0,0,4,0`。本地值压过样式 setter 与模板格子：
   我们在模板标记里写的上游 `NumberBoxSpinButtonBorderThickness`（`0,1,1,1`）**输了**，读回是 `1,0,0,0`。
   圆角那半是好消息：`4` 来自控件自己的 `ControlCornerRadius`，即我们的令牌在驱动它
   （框架默认模板下读到的是它自己的 10）。
5. **换模板之后 `SpinButtonPlacementMode` 完全没人管。** 三种取值来回切，框架不碰我们的任何部件
   （可见性、弹层全不动）。所以上游 `SpinButtonStates` 的三个状态只能由格子实现，
   `Compact` 的弹层只能靠 `MultiTrigger(Compact + IsKeyboardFocusWithin) → UpDownPopup.IsOpen`。
6. **生成的文字元素带框架本地前景。** `HeaderContentPresenter` 里生成的 `TextBlock` 有本地
   `Foreground=#FF1D1D1F`，既不是我们的 `TextFillColorPrimaryBrush` 也不是 `Secondary`，
   压过一切样式与继承 → 标题颜色到不了像素（断言写成 `Assert.NotSame`）。
7. **`Text` 与 `Value` 的同步是单向的。** 赋 `Text="12.5"` 不重解析（`Value` 仍是旧值）；
   赋 `Value=99` 会把 `Text` 重排成 `"99"`。`StepUp/StepDown` 穿过我们的模板照常工作。
8. **静息的 NumberBox 没有框架本地值**（`Background`/`Foreground`/`BorderBrush`/`Padding` 都读回
   `UnsetValue`）——和挂载的 TextBox/PasswordBox/ComboBox 不同，那三个各有本地值账单。
9. **探针方法本身有一条要记：** 把裸 `ControlTemplate` 交给 `XamlReader.Parse` 时，每个
   `Trigger.Property` 读回 null、连 `IsEnabled=False` 都不触发；同一标记放进
   `ResourceDictionary → Style → Setter → 内联 ControlTemplate` 的生产形状就全部水合。
   第一轮据此得出的"枚举条件不匹配"结论是探针伪影，已作废；生产形状下枚举条件实测会触发。

## 1. 资源行：13 个上游 NumberBox* 键，抄 8 个

`ThemeResources/NumberBox.jalxaml`（新增，进 Manifest，进消费点闸口）：

| 键 | 类型 | 上游目标 / 值 | 消费点 |
|---|---|---|---|
| NumberBoxPopupIndicatorForeground | 别名 | TextFillColorSecondaryBrush | `PopupIndicator` 描边 |
| NumberBoxPopupBackground | 别名 | **FlyoutPresenterBackground**（值替换，见 §2） | `PopupContentRoot` |
| NumberBoxPopupBorderBrush | 别名 | SurfaceStrokeColorFlyoutBrush | `PopupContentRoot` |
| NumberBoxPopupSpinButtonBackground | 别名 | SubtleFillColorTransparentBrush | `NumberBoxPopupSpinButtonStyle` |
| NumberBoxSpinButtonBorderThickness | 度量 | 0,1,1,1 | spin 样式 + 模板标记（读回输给框架本地值，§0.4） |
| NumberBoxIconMargin | 度量 | 10,0,0,0 | `PopupIndicator.Margin` |
| NumberBoxPopupBorderThickness | 度量 | 1 | `PopupContentRoot` |
| NumberBoxPopupSpinButtonBorderThickness | 度量 | 0 | 弹层 spin 样式 |

不抄的 5 个，每个都有理由：

| 键 | 上游值 | 为什么没有行 |
|---|---|---|
| NumberBoxPopupHorizonalOffset | x:Double -21 | 本 reader 解析不了 x:Double（`adaptation/00`），值以字面量进模板 |
| NumberBoxPopupVerticalOffset | x:Double -27 | 同上 |
| NumberBoxPopupShadowDepth | x:Double 16 | 同上；且本运行时 Popup 没有 ThemeShadow 面 |
| NumberBoxMinWidth | x:Double 120 | 同上；`MinWidth=120` 以字面量进样式，并有断言钉住 |
| NumberBoxPopupIndicatorMargin | Thickness 0,0,8,0 | 上游有两处指示器消费点，我们只有一处，第二个边距没有消费点会被闸口判死 |

**上游模板内的 `RepeatButton*` 别名块（10 行）没有搬到应用级。** 第一次尝试搬了，结果
`ThemeResources/RepeatButton.jalxaml` 自己的同名行被压掉（它在 Manifest 里更早加载），
`AstraButtonTests` 两条像素断言当场红。上游把它放在模板局部 `Grid.Resources` 正是为了这个名字归属。
现在的做法：spin 格子直接读别名块转发的目标（`TextControlButton*`），静息面用
`ControlFillColorTransparentBrush`。代价写进 §5。

**随本批补进 `ThemeResources/TextBox.jalxaml` 的 11 行**（键名逐字照抄上游，之前"没有消费点"因此没抄）：
`TextBoxTopHeaderMargin`、`TextControlHeaderForeground`、`TextControlHeaderForegroundDisabled`、
`TextControlButtonForeground{,PointerOver,Pressed}`、`TextControlButtonBackground{PointerOver,Pressed}`、
`TextControlButtonBorderBrush{,PointerOver,Pressed}`。文本框那两条 gap 文案同步更正
（12 行无消费点 → 4 行占位符；8 行删除按钮现在有消费点了，但消费它的是 NumberBox 的 spin 按钮）。

## 2. 值替换（就地标注，共 4 处）

1. `NumberBoxPopupBackground`：上游是 `AcrylicBackgroundFillColorDefaultBrush`（`AcrylicBrush`）。
   本运行时无 `AcrylicBrush` 类型，走 FlyoutPresenter 批同一处理：实底弹层令牌。
2. `TextControlButtonBorderBrush{,PointerOver,Pressed}`：上游指向 `ControlFillColorTransparent`
   （无 `Brush` 后缀，本仓库也没有该键的定义处），我们指自己调色板的 `ControlFillColorTransparentBrush`。
3. `TextControlHeaderForeground{,Disabled}`：上游在模板里消费这两个名字，但**本仓库没有它们的定义行**
   （`grep x:Key="TextControlHeaderForeground"` 命中 0，属 UWP generic.xaml 遗留），
   值按 WinUI 语义取 `TextFillColorPrimaryBrush` / `TextFillColorDisabledBrush`。
4. `RepeatButtonBackground`：上游转发 `TextControlButtonBackground`，该键在本仓库同样无定义
   （命中 0），因此上游静息态本来就没画东西；我们直接用透明刷。

## 3. WinUI 视觉状态 → 本模板格子

| 上游 | 本运行时 | 备注 |
|---|---|---|
| NumberBox `CommonStates/Normal` | 静息 setter | |
| NumberBox `CommonStates/Disabled` | `Trigger IsEnabled=False`（6 个 setter，含标题行） | 标题只到得出生成文字的本地值之前，见 §0.6 |
| InputBox `CommonStates/PointerOver` | `Trigger IsMouseOver=True`（面、框、文字三行） | |
| InputBox `CommonStates/Focused` | `Trigger IsKeyboardFocusWithin=True`（面、`1,1,1,2` 框、底边强调、文字） | 上游还有 `ContentElement.RequestedTheme=Light`，本运行时无对应 |
| InputBox `CommonStates/Disabled` | 同上 `IsEnabled=False` 格 | |
| InputBox `ButtonStates/ButtonVisible,ButtonCollapsed` | 不映射 | 本运行时没有删除按钮部件（上游 `DeleteButton`） |
| NumberBox `SpinButtonStates/SpinButtonsCollapsed` | `Trigger SpinButtonPlacementMode=Hidden` | 框架不管替换后的模板，格子是唯一机制 |
| NumberBox `SpinButtonStates/SpinButtonsVisible` | `Trigger SpinButtonPlacementMode=Inline` | |
| NumberBox `SpinButtonStates/SpinButtonsPopup` | `Trigger =Compact` + `MultiTrigger(Compact, IsKeyboardFocusWithin) → UpDownPopup.IsOpen` | 上游另有 `InputBox.Style` 切换与代码开弹层，我们只到"聚焦即开" |
| NumberBox `UpSpinButtonEnabledStates/*` | 不映射 | 上游靠代码在 `Value` 触到 `Minimum/Maximum` 时禁用 spin 按钮；换模板后框架不再管我们的部件，标记里也表达不了 |
| NumberBox `DownSpinButtonEnabledStates/*` | 不映射 | 同上 |
| Spin 按钮 `CommonStates/{Normal,PointerOver,Pressed,Disabled}` | 内联 spin 模板 3 格 | 上游 Disabled 只转基础边框行，照抄 |
| `NumberBoxTextBoxStyle` 的 `PopupIndicator` | `Trigger =Compact` 让 `PopupIndicator` 可见 | 上游用 `x:Load`，本 reader 没有，改判 `Visibility` |
| 弹层开合 `ThemeAnimation` | 不映射 | 上游这两个文件里没有任何带时长的 Storyboard，弹层也没有淡入（`NumberBoxPopupShadowDepth` 走代码 ThemeShadow） |

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，Debug **0 警告 0 错误**，
  全套 **237/237 通过、0 skip（17 秒）**，调色板三档 `checked=True`
  （Light/Dark 83 个源色、101 个解析刷；HighContrast 101 个映射键）。
- **行为**：`AstraNumberBoxTests` 27 项 —— 8 条别名逐行身份 + 8 条 `TextControlButton*` 身份 + 5 条度量值 +
  5 个键"确实没有行"且 `MinWidth=120` 字面量在位 + `RepeatButton*` 归属未被抢走 + 两份模板逐格按键名断言 +
  样式 setter 逐行（刷按实例、度量按值）+ 框架占有四条（内容宿主、两个本地值、静息无本地值）+
  三态可见性来回 + 聚焦开弹层与回 Inline 即关 + StepUp/StepDown/Value→Text + 标题文本与禁用行。
- **视觉**：静息离屏捕获 `ControlFillColorDefaultBrush` 哨兵 >3000 px、品牌绿 `#207245` 恒为 0、
  染上的 accent 行不进静息态；同一场景 Light 与 Dark 主色分布不同。
- **硬件输入**：**无**。悬停/按下是 `IsMouseOver`/`IsPressed` 的格子命名与读回，没有真指针；
  任务 #13 仍未结。
- **Gallery 冒烟**：本批给 Gallery 加了 `--page <id>` 启动参数（`Program.cs` + `MainWindow.SetStartPage` /
  `NavigateToPage`，导航在 `Loaded` 里做，所以 `FluentThemeManager.Enter(page)` 照常广播）。
  同机连跑两次并对比内容区采样：`--page overview` 与 `--page inputs` 在
  `x∈[320,1240] y∈[120,820]` 的 161 000 个采样点里有 **39% 不同**，两页各 548 / 540 个不同色——
  "只有 Overview 会上屏"这条老限制到此为止，两个进程都 `exit=0`、无残留。
  目视 `inputs` 全幅截图：Inputs 页标题、"Text and passwords" 卡与四字段在位，
  parity 条读出 `Inputs · FluentToggleSwitch own-type, NumberBox audited, PasswordBox audited, Slider audited,
  TextBox audited · 5 of 18 restyled types`，其下的 not-claimed 段落里能看到本批写进
  `Catalog.json` 的 NumberBox 三条 gap——**目录文案确实走到了屏幕上**。
  截图里 **NumberBox 卡本身没有入镜**：它在 Inputs 页第一节之下，窗口高度不够，
  且一个第三方"Windows 安全中心 / server.exe"防火墙授权框压在页面中部（不是本仓库的进程，未点击、未处理）。

## 5. 不声称清单（Known Gaps）

1. 不声称 spin 按钮的边框是上游那档：框架在部件上写本地 `BorderThickness=1,0,0,0`，
   我们的 `NumberBoxSpinButtonBorderThickness`（`0,1,1,1`）到不了（§0.4）。
2. 不声称应用覆盖 `RepeatButtonBackground` 会改到 NumberBox 的 spin 按钮：上游的模板局部别名块
   在本运行时搬不到应用级（会抢走 RepeatButton 控件自己的行），所以这些名字归 RepeatButton，
   spin 按钮读的是它们的目标令牌。
3. 不声称标题颜色对：生成的文字元素带框架本地 `#FF1D1D1F`，标题两行只写到 presenter 上（§0.6）。
4. 不声称 `Compact` 与上游等价：上游是"点指示器开弹层 + 代码定位 + ThemeShadow"，
   我们是"聚焦即开 + Placement=Bottom"，且偏移量与阴影根本没有面（§2、§3）。
5. 不声称触到 `Minimum/Maximum` 时 spin 按钮会禁用：上游那两对状态是代码驱动的，换模板后无人实现。
6. 不声称删除按钮、`Description`、`HeaderTemplate`、`TextAlignment`、`InputScope` 传递：
   本运行时 NumberBox 没有 `Description`/`HeaderTemplate`，上游 `InputEater` 部件也没有对应物。
7. 不声称占位符有颜色行：`PlaceholderText` 属性在，但文字由框架的 `TextBoxContentHost` 画，
   没有可指向的元素，所以 `TextControlPlaceholderForeground*` 四行仍不抄。
8. 不声称高对比逐键一致：上游 NumberBox 的 HC 分支与 Light 同值（弹层三项除外），
   我们只有调色板级重映射。
9. 不声称悬停/按下有像素证据：见 §4 硬件输入。
10. 不声称 Gallery 里 NumberBox **画对了**：`--page inputs` 已能证明 Inputs 页真的上屏、
    parity 与 gap 文案走到了屏幕（§4），但 NumberBox 卡本身没入镜，也没有逐控件的裁剪断言——
    这属于 E4（每页/每卡渲染到像素），仍欠。

## 更正（哑格批 2026-09-20，`adaptation/00` S1-f）

§0.6 那条读数的**机制**当时读错了半层：写向 `HeaderContentPresenter.Foreground` 的那一格根本不曾到达任何元素，
因为本运行时的 `ContentPresenter` 没有 `Foreground` 成员（哑格，S1-e 第 7 条）；标题之所以看起来是我们的颜色，
是**继承**来的，不是格子写出来的。修法是把承载元素换成 `ContentControl`（该类型有 `Foreground`，其余属性一字未动），
于是 disabled 格现在确实落在头部承载元素上。两处随动：
**(a)** 承载元素上**故意不再写** `Foreground=` 属性——本地值压过一切格子，写上就是把 disabled 格重新锁死；
静止色由盒子继承（`TextControlHeaderForeground` 与控件的 `Foreground` 都别名 `TextFillColorPrimaryBrush`，同一支笔刷）。
**(b)** `ThemeResources/TextBox.jalxaml` 因此**收回** `TextControlHeaderForeground` 不发布，
只留 `TextControlHeaderForegroundDisabled` 与 `TextBoxTopHeaderMargin`——没有读者的行不发布。
§0.6 的结论在效果上仍然成立：**字形那一枚**读回的还是它构建时继承到的颜色，disabled 头部离像素差一跳，
`AstraForegroundRoutingTests.A_disabled_number_box_sends_the_disabled_row_into_its_header` 把"格子到承载元素"和
"字形不动"两半同时钉住，不做相邻替代。

## 更正（属性死写批 2026-09-20，`adaptation/00` S1-g）

§3 与部件表里那层 Spinner 弹层的表面（`PopupContentRoot`）**原本是 Grid**，而 `OverlayCornerRadius`、
`NumberBoxPopupBorderBrush`、`NumberBoxPopupBorderThickness` 三条挂在它身上——Grid 没有这三个成员，
所以本审计里"弹层有 8 DIP 圆角和 1 DIP 描边"这句话在运行时**从来不成立**：弹层是直角、无边框。
修法是把画者换成 Border（名字留在画者上，两行布局挪进内层 Grid），读回见
`AstraSurfaceGeometryTests.The_spinner_popup_surfaces_on_a_border_that_can_hold_its_radius`
（半径 / 边框厚度 / 底色 / 描边四项，弹层用 `IsOpen=true` 打开后从 `Popup.Child` 本身读——按名字查找只往下走）。
不声称：弹层的圆角与描边**没有像素捕获**，这条是属性读回；打开通路仍只到"焦点格写 `IsOpen`"，没有真指针。
