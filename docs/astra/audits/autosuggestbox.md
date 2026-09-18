# AutoSuggestBox 审计（阶段 3 第六段）

参考：`../microsoft-ui-xaml` @`19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，只读。

| 上游文件 | blob(8) | 用途 |
|---|---|---|
| `controls/dev/AutoSuggestBox/AutoSuggestBox_themeresources.xaml` | `7cd762eb` | 10 个 AutoSuggestBox* 行 + `DefaultAutoSuggestBoxStyle` + `AutoSuggestBoxTextBoxStyle`（状态写成 Storyboard 的那一份） |
| `controls/dev/AutoSuggestBox/AutoSuggestBox_themeresources_perf2026.xaml` | `39e8d87f` | 同一表面，状态改写成 `VisualState.Setters`；**逐行的键名与值与上一份完全相同** |
| `dxaml/xcp/dxaml/themes/generic.xaml` | `def26a61` | `AutoSuggestList*` 六行、`HelperButtonThemePadding`、`AutoSuggestBoxLeftHeader*` 两行、`AutoSuggestBackgroundThemeBrush` |

两份并行字典要选一份并说明选了哪份：这里从 `7cd762eb` 抄行，状态形式记录在读它的样式里。

方法参考：`../ModernWpf` @`23555a6c`（同样把 AutoSuggestBox 落到"另一个控件 + 一层弹层"的宿主替换路线上）。

---

## 0. 先量后写：这一批量到的运行时事实

**目标把 AutoSuggestBox 标成"自有类型"，实测只有一半成立。** 本运行时无 `AutoSuggestBox` 类型；
原生 `Jalium.UI.Controls.AutoCompleteBox : TextBoxBase` 在，声明 11 个依赖属性
（`FilterMode`、`IsDropDownOpen`、`IsTextCompletionEnabled`、`ItemsSource`、`MaxDropDownHeight`、
`MinimumPopulateDelay`、`MinimumPrefixLength`、`PlaceholderText`、`SelectedItem`、`TextMemberPath`、`Text`）、
5 个事件（`DropDownOpened`、`DropDownClosed`、`Populating`、`SelectionChanged`、`TextChanged`）、
2 个 CLR 只读/委托成员（`FilteredItems`、`ItemFilter`）。
默认值实测：`FilterMode=StartsWith`、`MinimumPrefixLength=1`、`MaxDropDownHeight=200`、
`IsTextCompletionEnabled=false`、`MinimumPopulateDelay=0`。
**没有** `Items` 集合（只有 `ItemsSource`），**没有** `QueryIcon`、`Header`、`Description`、`TextReplacementFilter`。
所以：外观走重模板（AGENTS.md 的"原生优先"），行为缺口留给一层薄自有类型，单独取证，不混进本批。

探针（`spike/AutoCompleteProbe`，不入库）逐条量到：

1. **无样式、无模板——直到挂载。** 未挂载时 `Style == null` 且 `Template == null`；挂上屏后
   `Template` 非 null、`Style` 仍为 null、`Template` 无本地值。框架在 `OnApplyTemplate` 时给它自己造一份模板：
   `OuterBorder`(Border) → `PART_ContentHost`(Grid) → `TextBoxContentHost`，加一个 `PART_Popup`
   （`Placement=Bottom`、child 是 Grid）。
   框架模板的一个颜色都不是我们的调色板实例（`#FF2C2C2E` / `#FF48484A` / `#FFF5F5F7`）。
   这条把 `adaptation/01` 的"判断能不能重模板要测 `Template != null`"再确认一遍：AutoCompleteBox 是唯一
   **挂载前连 `Template` 都是 null** 的样本，所以"能不能重模板"对它只能测挂载之后。
2. **我们的 Style 换得掉它。** 带 `Template` setter 的隐式样式装上后
   `ReferenceEquals(box.Template, ours) == true`，控件与 `OuterBorder` 的三面都读回我们的实例。
3. **弹层部件名就是契约，四个名字一个都不能少。** 框架把自己的弹层嫁接到
   `OverlayLayer → PopupRoot`，并在其中按名字找：`PART_Popup` → `PART_DropDownBorder`(Grid) →
   `PART_DropDownScrollViewer` → `PART_DropDownItemsHost`。实测三种失败：
   `PART_Popup` **没有 Child** → 弹层开成一个空的 20 DIP PopupRoot；
   `PART_DropDownItemsHost` 是 **ItemsControl** → 框架永远不填它（容器数为 0）；
   改成 **Panel**（StackPanel）→ 容器照常生成，类型是 `ComboBoxItem`。
   我们的模板保留这四个名字，并且容器 Border 用上游的 `SuggestionsContainer` 名字（上游同名部件）。
4. **`PART_ContentHost` 必须是面板。** NumberBox 批量到的这条在 AutoCompleteBox 上重演：
   同名元素是 Grid 时框架把 `TextBoxContentHost` 挂进去（`GetChild(host,0).GetType().Name`），
   换成 `ContentPresenter` 时不挂。
5. **框架在弹层上写本地 `Width`。** 打开后 `PART_Popup` 有本地 `Width`，值等于控件宽度（320）——
   样式里没有任何东西能设它，唯一诚实的断言是读回。和 ComboBox 批同一契约。
6. **条目容器一半是我们的、一半是框架的。** 生成的 `ComboBoxItem`：`Foreground` 实例等于我们的
   `TextFillColorPrimaryBrush`（经由隐式 ComboBoxItem 样式），但 `Background` 是框架本地值
   （一个 `LinearGradientBrush`）、还有本地 `MinHeight=28`。本地值压过样式 setter，
   所以**建议项的底色到不了**，只有文字颜色到得了。
7. **禁用态的文字颜色仍是框架的。** `IsEnabled=false` 后控件 `Foreground` 读到框架自己的 `#FF636366`，
   不是我们的 `TextControlForegroundDisabled` 实例；重新启用后回到我们的实例。断言写成 `Assert.NotSame`，
   把损失钉住而不是不提。
8. **换掉模板之后，"选中即回填文本"这条行为就没了。** 框架模板下 `SelectedItem = filtered[0]`（聚焦、
   `IsTextCompletionEnabled=true`）会把补全文本写进 `Text`；我们的模板下同一操作 `Text` 保持原样。
   上游叫 `UpdateTextOnSelect`，没有资源行能把它找回来——要代码。本批把它写成一条断言，防止日后当巧合。
9. **`Trigger Value=""` 在 String 属性上会触发——这条把 `adaptation/00` S0-g 从"语法级禁令"降级。**
   ComboBox 批记的是"空串判据永不匹配"（那里换成别的判据就命中）。本批在 AutoCompleteBox 上以生产形状
   （字典 → 样式 → setter → 内联模板）实测：静息 `Text == ""`，以 `Value=""` 为条件的格子把占位符从
   `Collapsed` 翻成 `Visible`，输入一个字符后又翻回去
   （断言：`An_empty_string_condition_matches_a_string_property_that_is_actually_empty`）。
   两次实测的差别在哪（属性静息值？控件类型？）本批没有取证，能说的只有：空串条件不是永远失效。
   本批**没有**据此上占位符：那要新增 4 行 `TextControlPlaceholderForeground*`，而文本引擎自己是否已经画
   占位串没有取证（§5.6）。
10. **框架自己算弹层高度。** 0/1/2 行条目对应 PopupRoot 高度 20 / 39.78 / 63.56 DIP（每行 ≈23.6），
    所以我们容器上的 `MaxHeight=374` 是天花板而不是尺寸；上游那行 `AutoSuggestListMaxHeight` 是
    x:Double，本 reader 解析不了，只能以字面量进模板（§1）。

## 1. 资源行：上游 19 行，抄 5 行

`ThemeResources/AutoSuggestBox.jalxaml`（新增，进 Manifest，进消费点闸口）：

| 键 | 类型 | 上游目标 / 值 | 消费点 |
|---|---|---|---|
| AutoSuggestBoxSuggestionsListBackground | 别名 | **FlyoutPresenterBackground**（值替换，见 §2） | `SuggestionsContainer.Background` |
| AutoSuggestBoxSuggestionsListBorderBrush | 别名 | SurfaceStrokeColorFlyoutBrush | `SuggestionsContainer.BorderBrush` |
| AutoSuggestListBorderThemeThickness | 度量 | 1 | `SuggestionsContainer.BorderThickness` |
| AutoSuggestListMargin | 度量 | 0,2,0,2 | `SuggestionsContainer.Padding` |
| AutoSuggestListPadding | 度量 | -1,0,-1,0 | `PART_DropDownScrollViewer.Margin` |

后三行照抄上游自己那个**名字与落点相反**的用法：名为 `Margin` 的行是容器 Padding，名为 `Padding` 的行是
列表 Margin。按名字写会把 2 DIP 的间隙从边框内挪到边框外。

文本半（面、框、字、光标、选中色、圆角、内边距）不重复定义：上游的 `AutoSuggestBoxTextBoxStyle`
本来就是 `DefaultTextBoxStyle` 的一份手抄分叉，所以这里直接读 `ThemeResources/TextBox.jalxaml` 的
`TextControl*` 行——与上游同一个来源。

不抄的 14 行（`ThemeResources/AutoSuggestBox.jalxaml` 注释里逐行给理由）：

| 键 | 上游值 | 类别 |
|---|---|---|
| AutoSuggestListMaxHeight | x:Double 374 | reader 解析不了；值以字面量进容器（落点差 6 DIP，§5.7） |
| AutoSuggestBoxIconFontSize | x:Double 12 | 同上；且本宿主无查询按钮字形可 sizing |
| AutoSuggestBoxLeftButtonMargin | x:Double 3 | 同上；**上游亦无消费点** |
| AutoSuggestBoxRightButtonMargin | x:Double 4 | 同上 |
| AutoSuggestListBorderOpacity | x:Double 0/1 | 同上；**上游亦无消费点** |
| AutoSuggestBoxLeftHeaderMaxWidth | x:Double 296 | 同上；**上游亦无消费点** |
| AutoSuggestBoxLightDismissOverlayBackground | 别名 | 宿主无 light-dismiss 覆盖层元素 |
| AutoSuggestBoxTopHeaderMargin | Thickness 0,0,0,8 | 宿主无 Header 属性 |
| AutoSuggestBoxLeftHeaderMargin | Thickness 0,5,32,0 | 同上；**上游亦无消费点** |
| AutoSuggestBoxInnerButtonMargin | Thickness 1,3 | 宿主无删除/内联按钮 |
| AutoSuggestBoxDeleteButtonMargin | Thickness 0,4 | 同上 |
| AutoSuggestBoxQueryButtonPadding | Thickness 3,2 | 同上；**上游亦无消费点** |
| HelperButtonThemePadding | Thickness 0,0,-2,0 | 同上（这行归通用按钮块，AutoSuggestBox 只是引用者） |
| AutoSuggestListViewItemMargin | Thickness 12,11,0,13 | 条目容器由框架生成，**上游亦无消费点** |

19 行里 6 行是 x:Double（本 reader 一律解析不了），6 行在上游就没有消费者。剩下的是宿主真的没有那块面。
`AutoSuggestBackgroundThemeBrush`（generic.xaml 前 Fluent 时代的实底刷）不属于本库抄的别名层，也不算行。

## 2. 值替换（就地标注，共 3 处）

1. `AutoSuggestBoxSuggestionsListBackground`：上游是 `AcrylicBackgroundFillColorDefaultBrush`（`AcrylicBrush`，
   Tint `#2C2C2C`/`#FCFCFC`、Fallback `#2C2C2C`/`#F9F9F9`）。本运行时无 `AcrylicBrush` 类型，
   走 FlyoutPresenter / ComboBox 下拉 / NumberBox 弹层同一处理：实底 flyout 令牌
   `FlyoutPresenterBackground`——它就是上游自己的 FallbackColor 值。
2. 文本框底边的强度线：上游把 `SurfaceStrokeColorStrongBrush` 与 `ControlStrokeColorDefaultBrush`
   混进一个 `LinearGradientBrush`。别名层带不动渐变（主题翻转只就地重染 `SolidColorBrush` 实例），
   所以 `BottomEdge` 用这两个令牌之一，与 TextBox 批同一个替换。
3. `AutoSuggestBoxTextBoxStyle`：上游把它作为一个可覆盖的 TextBox 样式键发布；本宿主的文本半是
   `AutoCompleteBox` 自己的属性与模板格子，不是嵌套 TextBox，所以这个键不转发，
   等价内容由 `DefaultAutoCompleteBoxStyle` 承担。

## 3. WinUI 视觉状态 → 本模板格子

上游两份字典的状态集合相同（`Normal`/`PointerOver`/`Focused`/`Disabled` × 控件与内层文本框，
`ButtonVisible`/`ButtonCollapsed`，`Pressed`，`Landscape`/`Portrait`），
28 个 `DiscreteObjectKeyFrame` 全是 `KeyTime="0"`，文件里 `TransitionProperty` 出现 0 次——
上游本来就是离散状态。本库因此也只写离散格子（插值会造出不属于任何调色板的刷实例）。

| 上游 | 本运行时 | 备注 |
|---|---|---|
| `CommonStates/Normal` | 静息 setter | 7 行 + 2 个字面量 |
| `CommonStates/PointerOver` | `Trigger IsMouseOver=True`（面、框、文字三行） | 逐行按名读回 |
| `CommonStates/Focused` | `Trigger IsKeyboardFocusWithin=True`（面、`1,1,1,2` 框、底边强调、文字） | 上游另有 `ContentElement.RequestedTheme=Light`，本运行时无对应 |
| `CommonStates/Disabled` | `Trigger IsEnabled=False`（5 行，含光标） | 文字行会被框架本地值压过（§0.7） |
| 内层 `AutoSuggestBoxTextBoxStyle` 的同名四态 | 与上面同一批格子 | 上游是"外层控件 + 内层 TextBox"两套，本宿主只有一套属性面 |
| `ButtonStates/ButtonVisible,ButtonCollapsed` | 不映射 | 宿主无删除按钮部件 |
| QueryButton `Pressed` | 不映射 | 宿主无 `QueryIcon` |
| `PopupStates/Landscape,Portrait` | 不映射 | 上游靠代码在两者间切弹层方向，本宿主 `Placement=Bottom` 固定 |
| 建议项（上游 `SuggestionsList` 的 `ListViewItem` 状态） | 不映射 | 容器由框架生成并写本地底色（§0.6）；本批只能靠既有隐式 ComboBoxItem 样式的文字行 |
| 弹层开合动画 | 不映射 | 上游这两个文件没有带时长的 Storyboard；本宿主由代码开合 `PART_Popup` |

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，Debug **0 警告 0 错误**，
  全套 **268/268 通过、0 skip（19 秒）**，其中本批新类 30 项，
  调色板三档 `checked=True`（Light/Dark 83 个源色、101 个解析刷；HighContrast 101 个映射键）。
  `tools/Report-Control-Vacuum.ps1` 重跑：`AutoCompleteBox` 从"E. 范围内没样式"移到
  "已样式但 ModernWpf 名单没有（按别名/内联实现）"，`outOfScope 106→105`；
  `done=13 / HARDGAPS=41` 不变，因为 `AutoSuggestBox` 仍在 D 节"无同名原生类型"里——
  行为那一层还欠着自有类型，这张表把它记对了。
- **行为**：`AstraAutoSuggestBoxTests` 30 项 —— 2 条别名实例身份 + 3 条度量值（含上游名字互换）+
  3 个 x:Double 键"确实没有行"且 374 字面量在位 + 13 个键按宿主理由保持未声明 +
  空串判据那一条（S0-g 的反例）+ 8 条样式 setter 逐行按键名 + 3 个格子逐 setter 按键名 +
  格子集合恰好等于上游那三个 +
  框架占有五条（换模板成功、四个部件名在位、内容宿主是面板、文本引擎嫁接进来、弹层本地 `Width`）+
  条目容器两条（文字是我们的、底色是框架的）+ 禁用前景是框架的 + 输入即过滤进我们的 items host +
  选中不回填文本 + 全格子不得写调色板字面刷。
- **视觉**：静息离屏捕获 `ControlFillColorDefaultBrush` 哨兵 >3000 px、品牌绿 `#207245` 为 0、
  accent 哨兵不进静息态；同一场景 Light 与 Dark 主色分布不同。
  **弹层也进了像素**：给弹层链末端那把调色板刷盖哨兵后，`Chrome(SuggestionsContainer)` 里
  哨兵是该裁剪的第一大色（1092 px / 260 DIP 宽）——`AutoSuggestBoxSuggestionsListBackground`
  不是只挂在树上。同时量到：这个裁剪包含条目容器，而容器有自己的框架本地渐变，
  于是该裁剪**永远不逐位稳定**，`Stable` 与"品牌绿不出现"在这里都不可断言，
  所以那条用例只断哨兵的存在与规模（已写进用例注释，规则进 `adaptation/00` S0-j）。
- **硬件输入**：**无**。悬停/聚焦是格子命名与读回，弹层由赋值 `Text` 打开，没有真指针或触摸。
- **Gallery 冒烟**：`FluentJalium.Gallery.exe --page inputs` 与 `--page overview` 各跑一次，
  两页都出窗口（dpi=168、1280x860）、`exit=0`、无残留进程。目视 `inputs` 截图（同批产物）：
  Live output 卡里读到 **"Navigated to Inputs."**，其下 parity 条完整可读——
  **"Inputs · AutoCompleteBox audited, FluentToggleSwitch own-type, NumberBox audited, PasswordBox audited,
  Slider audited, TextBox audited · 6 of 19 restyled types"**：本批新增的目录条目走到了屏幕上，
  restyled 计数从 5 变 6。再下面那段 "Not claimed" 逐条是本批写进 `Catalog.json` 的六条 gap
  （"upstream's AutoSuggestBox has no type here…"、"14 of the 19 rows…"、"suggestion no longer writes
  its text into the box…"、"the list surface is a solid flyout substitution…"、"no real pointer or touch
  opens or closes the popup"）。parity 条在页首，所以这次是**入镜的**；"Suggested text" 卡本身仍在折叠线以下，
  没入镜（E4 逐卡裁剪仍欠）。截图右侧三分之一被一个第三方"Windows 安全中心 / server.exe"防火墙授权框盖住
  （不是本仓库的进程，未点击、未处理），它遮掉了这段文字的行尾。

## 5. 不声称清单（Known Gaps）

1. 不声称"这是一个 AutoSuggestBox"。它是 AutoCompleteBox 穿了 AutoSuggestBox 的行。
   `QueryIcon`、`Header`、`Description`、`TextReplacementFilter`、`SuggestionKind` 在本宿主没有属性面，
   上游模板里靠它们定位的 6 行因此没有行（§1）。
2. 不声称选中建议会回填文本：换掉框架模板之后 `UpdateTextOnSelect` 就没了（§0.8，已写成断言）。
   要它得加自有类型，那一层单独取证。
3. 不声称建议项的底色可主题化：容器的 `Background` 是框架本地值，压过一切样式与格子（§0.6）。
   我们能到的只有它的文字颜色。
4. 不声称禁用态文字颜色对：同一类框架本地值（`#FF636366`）在控件层拦住了
   `TextControlForegroundDisabled`（§0.7）。
5. 不声称弹层表面是亚克力：§2.1 的实底替换。也不声称边缘内角方形：上游
   `KeepInteriorCornersSquare` 只在代码里生效，本运行时无对应面。
6. 不声称占位符有颜色行：`PlaceholderText` 属性在、文本由框架的 `TextBoxContentHost` 画，
   没有可指向的元素，所以 `TextControlPlaceholderForeground*` 仍不抄（与 TextBox/NumberBox 批一致）。
7. 不声称建议列表的高度上限与上游逐位一致：`AutoSuggestListMaxHeight` 以字面量落在容器上而不是
   列表上，因此我们的列表比上游少 6 DIP（4 padding + 2 border，§1）。
8. 不声称上游的两份字典合并了：本批从 `7cd762eb` 抄行；`39e8d87f` 的状态重写形式没有搬，
   因为本运行时的标记层没有 `VisualState.Setters`（`adaptation/00`）。
9. 不声称高对比逐键一致：AutoSuggestBox 没有独立 HC 分支（它继承文本框与 flyout 的），
   我们只有调色板级重映射。
10. 不声称悬停/聚焦/打开弹层有真指针证据：格子是命名与读回，没有点击；任务 #13 仍未结。
11. 不声称 Gallery 里这块"画对了"：入镜并被读到的是**页首的 parity 条与本批的 gap 文案**（§4 冒烟），
    "Suggested text" 卡本身在折叠线以下、没有入镜，也没有逐卡裁剪断言（E4 仍欠）。
12. 不声称弹层裁剪能过"品牌绿不出现"闸口：那一层包含条目容器，容器底色是框架自己的渐变（§0.6），
    所以该裁剪既不逐位稳定也不干净（§4 视觉）。哨兵用例只断"我们的面到了像素"。
13. 不声称 `ItemsSource` 换成对象集合时的显示：`TextMemberPath` 属性在，但条目由框架的
    `ComboBoxItem` 承载，本批只用字符串集合取证。
