# RadioButtons 审计：一个"选项列表"在两套运行时里各自是什么

阶段 5 收尾批第一段。上游基线 `microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
运行时权威 NuGet Jalium.UI **26.10.9**（AGENTS.md 钉住），方法对照 ModernWpf @ `23555a6c0`。

## 0 · 依据

| 上游文件 | blob | 用在哪里 |
| --- | --- | --- |
| `controls/dev/RadioButtons/RadioButtons_themeresources.xaml` | `efbc00b43e516636dcbe421bd5f5640a64c9260a` | `ThemeResources/RadioButtons.jalxaml` |
| `controls/dev/RadioButtons/RadioButtons.xaml` | `40a2827458d18bd57fd164c3f00fc88b19501fb8` | `Styles/RadioButtons.jalxaml` |
| `controls/dev/RadioButtons/RadioButtons.idl` | 公开面 13-37 | `Controls/Input/FluentRadioButtons.cs` 的属性/事件面 |
| `controls/dev/RadioButtons/RadioButtonsPrimitives.idl` | `ColumnMajorUniformToLargestGridLayout` 7-21 | `Controls/Layout/FluentRadioButtonsPanel.cs` |
| `controls/dev/CommonStyles/RadioButton_themeresources.xaml` | `223a28385186f36a4629d354928e5fb705ab10b3` | 条目本身沿用阶段 3 的 `ThemeResources/RadioButton.jalxaml`，本批不重复 |

注意目录名：这份检出里控件源码在 `controls/dev/<Area>/`，不是 `dev/<Area>/`。
上游另有 `RadioButtons_themeresources_perf2026.xaml`，**0 个** ThemeDictionaries 段，
是同一批键的换值版本，不是新键面——本批的键工作对它同样成立。

## 1 · 键清单（全部 5 条，逐字）

ThemeDictionaries 三段各 2 行（Light `:4-7`、Default `:8-11`、HighContrast `:12-15`）。
`x:Key="Dark"` 在这个文件里 **0 命中**——上游的暗色段叫 `Default`。
Light 与 Default 按 (名 → 目标) 排序逐行比对：完全一致（diff 为空）。

| 上游行 | 键 | 类型 | Light/Default 目标 | HighContrast 目标 |
| --- | --- | --- | --- | --- |
| :5/:9 | `RadioButtonsHeaderForeground` | StaticResource | `TextFillColorPrimaryBrush` | `SystemControlForegroundBaseHighBrush` (:13) |
| :6/:10 | `RadioButtonsHeaderForegroundDisabled` | StaticResource | `TextFillColorDisabledBrush` | `SystemControlDisabledBaseMediumLowBrush` (:14) |

ThemeDictionaries 之外 3 行：

| 上游行 | 键 | 类型 | 值 | 本层 |
| --- | --- | --- | --- | --- |
| :17 | `RadioButtonsColumnSpacing` | x:Double | 7 | **不发布**，改成面板上的同名属性默认值 |
| :18 | `RadioButtonsRowSpacing` | x:Double | 8 | 同上 |
| :19 | `RadioButtonsTopHeaderMargin` | Thickness | 0,0,0,8 | 发布，模板直接读 |

模板消费的键（`RadioButtons.xaml`，5 个不同名）：
`RadioButtonsHeaderForegroundDisabled`(:16)、`RadioButtonsHeaderForeground`(:21)、
`RadioButtonsTopHeaderMargin`(:21)、`RadioButtonsColumnSpacing`(:24)、`RadioButtonsRowSpacing`(:24)。
后两条只喂布局类，正是本层唯一没法照抄的一对。

## 2 · 运行时面与差分

普查（`spike/ItemHostProbe` mode `census`，转录在 `adaptation/s1l-itemhost-raw.txt`）：
`RadioButtons` 在 26.10.9 **0 命中**（`BreadcrumbBar`/`PipsPager`/`ItemsRepeater`/`CardAction` 同样 0）。
上游侧的机制是：`RadioButtons : Control`（`RadioButtons.idl:9`）**不是** ItemsControl，
条目由 `RadioButtonsElementFactory::GetElementCore` 造——已是 `RadioButton` 就原样交给它（:49-52），
否则新建一个 `RadioButton` 包装（:56-87，实例化在 :70）、`Content=args.Data()`（:75）、
把用户的 `ItemTemplate` 推进去（:77-84）。互斥靠挂 `Checked/Unchecked` 撤销器（`ChildHandlers`，`RadioButtons.h:18-23`）。
`GetContainerForItemOverride`/`IsItemItsOwnContainerOverride` 在上游这个控件里 **0 命中**。

本层的等价通路是运行时的 ItemsControl 管线：那五个覆写在 26.10.9 里是
**protected/virtual**（签名与 WPF 一致，`s1l` 段 [B]），`IsItemItsOwnContainerOverride` 收 `RadioButton`
即"原样交给它"，`GetContainerForItemOverride` 返回 `new RadioButton()` 即"包一个"。
`FluentRadioButtons : ItemsControl` 因此是同一个机制的另一种宿主，而不是"退回 ItemsControl"。

模板面差分（上游 `RadioButtons.xaml:9-28`）：

| 上游 | 本层 | 原因 |
| --- | --- | --- |
| `StackPanel` 根 | 同 | — |
| `ContentPresenter HeaderContentPresenter` (:21)，带 `IsHidden` 绑定 | `ContentPresenter HeaderContentPresenter`，可见性由控件写 `Visibility` | 本运行时的 presenter 内容为 null 时仍保留 margin；没有 `IsHidden` 这个属性 |
| `ItemsRepeater InnerRepeater` (:22) + `ColumnMajorUniformToLargestGridLayout` (:24) | `ItemsPresenter` + `FluentRadioButtonsPanel` | 运行时无 repeater；`ItemsPanelTemplate` 只携带 `PanelType`，`FrameworkElementFactory` 那个构造器**只抄 `root.Type`、丢掉所有 SetValue**——所以列数必须由面板自己找宿主读（`Parent` 链），不能由外部写进去 |
| `MaxColumns` 绑进布局（:24） | 面板 `MeasureOverride` 里向 `Parent` 链要 `MaxColumns` | 同上。这条不是推出来的：只走 `TemplatedParent` 时条目实测全挤在一列（X 恒等于 100），断言 `MaxColumns_reaches_the_layout_and_fills_the_first_column_before_the_second` 就是抓这个的 |
| 头部前景写在 presenter 上 | 写在控件上，靠继承链到文字 | `ContentPresenter` 在本运行时不声明 `Foreground`，写上去是哑格（`adaptation/00` S1-f） |

## 3 · 状态映射

上游只有一个组：`CommonStates`（:11-20），`Normal`(:13) 空，`Disabled`(:14-18) 只写一件事——
`HeaderContentPresenter.Foreground = {ThemeResource RadioButtonsHeaderForegroundDisabled}`(:16)。
映射到本层就是模板里唯一那条 `Trigger Property="IsEnabled" Value="False"`。
条目自己的四态沿用阶段 3 已审完的 `RadioButton` 格子（`audits/checkbox-radiobutton.md`），本批没有第二套。

`IsSelected`/`Checked` 不是视觉态：勾选的视觉全部来自条目样式，控件只负责把互斥变成一次
`SelectionChanged`（`AddedItems`/`RemovedItems`，用运行时自带的 `SelectionChangedEventArgs`）。
第一条选择里 `RemovedItems` 必须是空的——把"之前没得选"写成"选掉了一个 null"是接口在说谎，
这条断言写在 `Taking_a_choice_clears_the_others_and_reports_it_once`。

## 4 · 四类证据

* **构建**：串行闸口 `tools/Test-AstraGates.ps1` exit 0，Debug 真重编 **0 警告 / 0 错误**，
  整套 **1137/1137 通过、0 跳过**（6 m 47 s），调色板漂移三行 `checked=True`，`All Astra gates passed.`。
* **行为**：`AstraRadioButtonsTests` 16 条——容器是真 `RadioButton` 且带我们的模板部件；
  列数经 `TranslatePoint` 读回的位置证实"先填满第一列"；单列时全部同 X 逐行下移；
  选择互斥 + 事件序列 `added=[choice 2] removed=[]` → `added=[choice 3] removed=[choice 2]`；
  代码侧写 `SelectedItem` 会回过去勾选容器，`ContainerFromIndex` 读回的就是树上那一个；
  头部的有/无切换可见性；禁用换到 disabled 前景行。
* **视觉**：`A_choice_marks_pixels_the_moment_it_is_taken` 用单一颜色键的增量断言
  （勾选环 `RadioButtonOuterEllipseCheckedFill` 的像素数在 Dark 下 +≥40），
  并断言框架品牌绿 `#207245` 一个像素都不出现。**不用圆点自身的颜色做判据**：
  Dark 下圆点就是标签那支白，勾选时它的键反而少了 165 个像素——先按点子的颜色断言会得出反的结论。
* **硬件输入**：无。见 §5 第 1 条。

## 5 · Known Gaps

1. **没有真指针/键盘/触摸进过这些行**。测试写 `IsChecked = true`，那是点击留下的结果状态，不是点击本身。
   本仓库至今没有真指针的像素通路（任务 #13），这条欠账对每个列表控件都一样。
2. **方向键跨行导航未量**。运行时 `RadioButton` 有按父级归组的默认组名（`GetDefaultGroupName()`），
   互斥实测有效；键盘箭头是否在同一组里移动焦点，本批一次都没测。
3. **面板比上游粗**：上游按列取最大（列内 largest）且可虚拟化；本层所有格子都取全局最大，且每条都实例化。
   条目很多时的开销差异没有测过。
4. **两条间距不是主题行**：`RadioButtonsColumnSpacing`/`RadioButtonsRowSpacing` 是 x:Double，
   这个 reader 解析不了（会连整本字典一起废掉），所以落在面板属性默认值上。应用侧能改属性，改不了资源键。
5. **上游有、本层没有的公开面**：`HeaderTemplateSelector`、`ItemContainerStyleSelector` 一类选择器，
   以及 `ContainerContentChanging` 事件。`ContainerFromIndex(int)` 有，但只在容器已生成时给得出。
6. **高对比未测**：两条前景行别名到调色板实例，`HighContrast.map` 会重指目标；
   这个控件在 HC 下的读数一次都没量过。
7. **头部只有 `Header`/`HeaderTemplate`**：上游同名字段还带 `IsHidden` 联动与 `ContentTransitions`，本层没有对应机制。

## 9 · 为什么仍是自有类型（带读数的判断）

运行时既没有 `RadioButtons` 这个名字（0 命中），也没有能承接它的宿主：上游是
`Control` + repeater + 自定义布局，本层最接近的是 `ListBox`/`Selector`——而 `Selector` 的选中是
"选中一条"，不是"这组里哪一个勾着"，用 `ListBox` 会把勾选视觉变成选中视觉，和上游差一层。
`ItemsControl` 派生 + 我们自己造 `RadioButton` 容器，恰好落在上游那条线上：条目是货真价实的
`RadioButton`（继承阶段 3 的全部状态格子），互斥交给框架的组行为，控件只持有一个"选了哪个"的读数。
所以本批的交付是 `FluentRadioButtons : ItemsControl`，且它是这个库里**第一个**继承 ItemsControl 的类型——
依据是 `s1l` 段的三处读数，而不是旧文档里那条已被撤回的纪律（`adaptation/09`）。
