# SplitButton / DropDownButton 审计（阶段 2 收尾批）

上游依据：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`

- `controls/dev/SplitButton/SplitButton_themeresources.xaml` blob `e9601d1f29cd59aa5fe7b7f0ebb49ef9c836ac19`
  （Light/Default 分支 **5–35 行**：29 条别名 + `SplitButtonBorderThemeThickness`；分支外还有
  两条 `x:Double` 尺寸、一条 `SplitButtonPadding`，以及整份 `SplitButtonCommandBarStyle`）
- `controls/dev/SplitButton/SplitButton.xaml` blob `0be293a87cd5d944267906e159fbcc4d1d85911b`
  （`SplitButtonStyle` 在 **3–244 行**：`CommonStates` 一个组 **15 个状态**、
  `SecondaryButtonPlacementStates` 一个组 2 个状态，三列网格在 219–223，两个半区在 227–237，
  两条半区描边在 238–239）
- `controls/dev/DropDownButton/DropDownButton_themeresources.xaml` blob `c1e68023c2f729b36e83b7fc6b91ed7e0d2e97fe`
  （Light 分支只有 **3 行**，全部是箭头前景）
- `controls/dev/DropDownButton/DropDownButton.xaml` blob `aa07b40c6f40b28c347ab25fd73a8ba36524505f`
  （`DefaultDropDownButtonStyle` 3–103 行，`CommonStates` **4 个状态**：Normal、PointerOver、Pressed、Disabled）

运行时：NuGet Jalium.UI **26.10.9**。`Jalium.UI.Controls.SplitButton` 存在，
`DropDownButton` 与 `ToggleSplitButton` **不存在**——`spike/SplitButtonProbe` 扫过 3129 个导出类型名，
这两个名字一个都没有。

一句话结论：**这一批把"框架到底靠什么把点击接到弹层上"从猜测变成了断言**，
顺带纠正两处此前会想当然的地方——命令会被执行两次，以及隐式样式其实从不写进 `Style` 属性。

## 0. 先量后写：这一批量到的运行时事实

**S1 · `SplitButton` 的公开面只有三个属性，且没有任何可读的"打开"状态。**
声明的依赖属性是 `Command`、`CommandParameter`、`Flyout`；事件只有 `Click`（`SplitButtonClickEventHandler`）；
方法只有 `OnApplyTemplate`（探针 B2/B3/B5）。**没有 `IsDropDownOpen`**（探针 F 明确报出这一条），
`CreateAutomationPeer` 返回空（探针 I），所以控件层面既没有可绑定的打开状态，也没有可驱动的自动化模式。

**S2 · `FlyoutBase.IsOpen` 是只读 CLR 属性，不是依赖属性。**
`ShowAt(FrameworkElement)` / `ShowAt(FrameworkElement, FlyoutShowOptions)` / `Hide()` 是方法，
`Placement` 是依赖属性，`Opened`/`Opening`/`Closed`/`Closing` 是普通 `EventHandler` 事件（探针 J）。
`DependencyProperty.FromName(FlyoutBase, "IsOpen")` 返回 null——**这就是"打开态画不出来"的根因**：
本运行时的格子条件只能读被模板控件自身的属性，一条既非依赖属性、又不在控件上的布尔值，
绑定和格子都够不着（详见 `adaptation/00` S0-k 与本文 §5）。

**S3 · 半区的名字是契约，不是排版习惯。**
我们自己写的模板里，两个半区**必须**叫 `PrimaryButton` 和 `SecondaryButton`：
叫对了，Invoke 次半区会把 `Flyout.IsOpen` 变成 true，Invoke 主半区会让控件 raise 一次 `Click`；
改名成 `PrimaryButtonZ`/`SecondaryButtonZ`，同样建树成功、同样画得出东西，
但 Invoke 之后 `IsOpen=false`、`Click` 一次都没发生（探针 I，两个方向都量了）。
`AstraSplitButtonTests.The_named_halves_drive_the_flyout_and_the_click_and_the_renamed_ones_do_not`
把这条钉成断言，包含反例——因为"模板能画对但点了没反应"正是最便宜也最难发现的破坏。

**S4 · 半区是真 `Button`，会吃到我们的隐式按钮样式。**
探针里未命名的次半区一度带着 `OURS:ControlFillColorDefaultBrush` 和我们的 `Surface`/`FocusOutline` 结构，
这就是上游要在模板的 `Grid.Resources` 里塞一个 `<Style TargetType="Button">` 覆盖的原因
（`SplitButton.xaml` 24–66 行）。我们改成给两个半区各挂一个**显式 keyed 样式**
（`SplitButtonPrimaryButtonStyle` / `SplitButtonSecondaryButtonStyle`）。
注意边界：上游那种"模板内 `Grid.Resources` 隐式样式"在本运行时能不能被模板内部解析，
**我们没有测过**，所以没有依赖它；显式 keyed 样式是量过的路径。

**S5 · 半区必须自己拉伸，否则控件右侧一整块不画。**
共用的 `ButtonLayoutStyle` 把按钮对齐设成 `Left` / `Center`（对应上游按钮的默认值），
而星形列会尊重这个对齐：在 220×36 的控件上主半区实测只有 **53.09×34.78**，
剩下 131px 宽（4832 像素）在捕获里是空的。修正是在两个半区样式上写 `Stretch`/`Stretch`
（上游 227 行同样是 `HorizontalAlignment="Stretch" VerticalAlignment="Stretch"`）。
这条由 `Both_halves_stretch_into_their_columns` 把关，休息底色像素断言的阈值
（>5000）也依赖它——否则它只会测到"一个窄条被染色"。

**S6 · 没有 `Flyout` 时，框架自己禁用次半区。**
一个未设 `Flyout` 的挂载实例，次半区的 `Background` 读回来是
`OURS:ControlFillColorDisabledBrush`、前景是框架自己的 `#FF636366`（探针 C），
即"禁用格子"命中了。这与 WinUI 的语义一致（没有可展开的东西，箭头半区不可用），
但要点在于：**我们的禁用格子会被框架的启用状态驱动，而不是被 `IsEnabled=False` 驱动**。

**S7 · 命令会被执行两次，如果照抄上游的绑定。**
上游把 `Command="{TemplateBinding Command}"` 绑在主半区上（`SplitButton.xaml` 227 行），
这在 WinUI 是对的，因为 WinUI 的 `SplitButton` 自己不去执行命令。本运行时会：
一次 Invoke 主半区，实测命令 `Execute` 被调用 **2** 次。因此模板里**不**把 `Command`/`CommandParameter`
传给半区，改由控件自己执行；`The_primary_half_carries_the_content_and_the_command`
断言"恰好一次"并且 `primary.Command is null`。内容和前景仍然走 `TemplateBinding`（同一条用例断言）。

**S8 · 隐式样式从不写进 `FrameworkElement.Style`。**
挂上我们的主题之后，`Button`、`ToggleButton`、`HyperlinkButton`、`SplitButton` 的
`Style` **全部是 null**，而 `TryFindResource(该类型)` 能取回样式对象，像素也确实是我们令牌的颜色（探针 K）。
后果很具体：任何"`control.Style` 非空"的断言都是错的判据，本批用**模板对象身份**
（`Assert.Same(Template(ours), split.Template)`）作为"我们的样式生效了"的读法。

**S9 · 我们接手之前它画的是框架外观，还漏品牌绿。**
`SplitButtonStyle` 落地之前，同一个控件的背景是 `#FF2C2C2E`、描边 `#FF48484A`、前景 `#FFF5F5F7`，
拿到焦点时两条半区描边变成 `#FF1E793F` 与 `#FF267440`（探针 C/G，框架自带的强调绿）。
现在 `The_split_button_takes_our_style_rather_than_the_framework_chrome` 与
`The_split_button_follows_the_theme` 在捕获里同时拒绝 `#2C2C2E` 和 `#207245`。

## 1. 逐行处置：上游 30 条主题行去哪了

| 处置 | 条数 | 说明 |
| --- | --- | --- |
| 原样转录并被读取 | 15 条别名 + `SplitButtonBorderThemeThickness` + `SplitButtonPadding` | 见 `ThemeResources/SplitButton.jalxaml` |
| 转录但替换取值 | 2 条 | `SplitButtonBorderBrush`、`…BorderBrushPointerOver` 上游指向 `ControlElevationBorderBrush`（渐变）；本调色板的渐变**不能**随原地重染色一起走（`ComboBox.jalxaml` 已记录），于是落在按钮家族同一个 `ControlStrokeColorDefaultBrush` 上。后果：休息/悬停/按下三条描边是同一个实例，**没有可区分像素**，与 S2/ToggleButton 混合态同类 |
| 暂不转录 | 14 条 | 13 条 `*Checked*`（4 底 + 4 前 + 4 描边 + `…CheckedDivider`）属于 `ToggleSplitButton`，本运行时无此类型；`SplitButtonInAppBarUnfocusedPointerOver` 只被 `SplitButtonCommandBarStyle` 读取，属于阶段 4 的 CommandBar 批 |
| 以字面量随行 | 2 条 | `SplitButtonPrimaryButtonSize`、`SplitButtonSecondaryButtonSize`（`x:Double`，本读取器不解析），在模板列上写死 35 |
| 整份推迟 | 1 份样式 | `SplitButtonCommandBarStyle`（含它自己的内层按钮样式与 16 个状态），随 CommandBar 批 |

`DropDownButton.jalxaml` 只有 3 条，全部被 `DefaultDropDownButtonStyle` 的箭头格子读取；
上游那 3 条之外的表面颜色用的是 `Button*` 行，因此我们不另立名字。

闸口侧同步扩了两行 `InlineData`（`AstraResourceKeyTests`，现在共 14 份字典受"每行必须被读"约束），
并且 `A_row_with_no_consumer_is_not_published` 反向钉住"没有消费者的键不发布"这条推迟
（13 条里点名 8 条 + CommandBar 那条）。

## 2. 结构替换（host substitution）

| 上游 | 这里 | 为什么 |
| --- | --- | --- |
| `PrimaryBackgroundGrid` / `SecondaryBackgroundGrid` 两个兄弟节点承载底色 | 每个半区自己的 `Surface` | 兄弟节点要按"哪个半区被悬停"换色，而本运行时读不到那个内部标志（S1/S2）；把填充交给半区自身，就能用原生 `Button` 的 `IsMouseOver`/`IsPressed` 得到**互相独立**的两半 |
| `PrimaryButtonBorder` / `SecondaryButtonBorder` 两条覆盖网格 | 半区自己的 `BorderThickness`（`1,1,0,1` 与 `0,1,1,1`）+ 圆角 `4,0,0,4` / `0,4,4,0` | 同一批像素，少两个节点；两侧字面量与上游模板里的字面量一致 |
| `AnimatedIcon` + `FontIconSource` 回退（`E96E`，8px） | `Path` 折线 `M 0,1.5 L 5,6.5 L 10,1.5`，12×12，`StrokeThickness=1.25` | 本运行时无 `AnimatedIcon`；沿用 ComboBox 的箭头画法（`audits/button.md` 缺口 4） |
| `UseSystemFocusVisuals=True` + `FocusVisualMargin=-1` | 根上的自绘 `FocusOutline`（`Grid.ColumnSpan=3`） | 本运行时无系统焦点框。焦点落在控件上而非半区（两个半区 `IsTabStop=False`），所以环属于根；判据只能取根的那一个，因为按名字找到的第一个 `FocusOutline` 属于主半区（测试里用 `RootRing`） |
| `Grid.Resources` 内隐式 `Button` 样式 | 两个显式 keyed 样式 | S4 |
| `SplitButtonPadding` 经控件 → 主半区 | 同一个行同时写在控件与主半区样式上 | 上游把它绑给主半区；这里控件上也留一份，读回的 `Padding` 才是应用者覆盖的那条 |

## 3. 状态映射表（上游 15 + 2 → 这里 8 格）

| 上游 `CommonStates` | 这里的驱动 | 落的属性 | 状态 |
| --- | --- | --- | --- |
| `Normal` | 两个半区样式的休息 setter | Background / Foreground / BorderBrush + 箭头 Stroke | ✅ |
| `PrimaryPointerOver` | 主半区 `IsMouseOver=True` | 同上三行 | ✅ |
| `PrimaryPressed` | 主半区 `IsPressed=True` | 同上三行 | ✅ |
| `SecondaryPointerOver` | 次半区 `IsMouseOver=True` | 底色、描边、箭头（写的是 `SplitButtonForegroundPointerOver`，与上游同一行） | ✅ |
| `SecondaryPressed` | 次半区 `IsPressed=True` | 底色、描边、箭头 `…ForegroundSecondaryPressed` | ✅ |
| `Disabled` | 两个半区各自 `IsEnabled=False` | 底色、前景、描边、箭头、焦点环 | ✅ |
| `FlyoutOpen` | — | 需要"打开"可读（S2） | ❌ 无驱动 |
| `TouchPressed` | — | 需要触摸按下标志 | ❌ 无驱动 |
| `Checked` / `CheckedFlyoutOpen` / `CheckedTouchPressed` / `CheckedPrimaryPointerOver` / `CheckedPrimaryPressed` / `CheckedSecondaryPointerOver` / `CheckedSecondaryPressed` / `CheckedDisabled` … | — | 属于 `ToggleSplitButton`，本运行时无此类型 | ❌ 13 条行随之推迟 |
| `SecondaryButtonRight` | 模板列定义即此布局 | — | ✅（唯一实现的排布） |
| `SecondaryButtonSpan` | — | 次半区跨列，`FluentDropDownButton` 走的是另一条路（见下） | ❌ |

箭头是部件，样式格子够不着，所以它的三行颜色写在模板自己的格子里；
`The_secondary_chevron_has_a_cell_for_every_state_that_colors_it` 允许同一条件出现两格
（禁用条件同时被焦点环和箭头使用）。

## 4. 四类证据

- **构建**：`dotnet build src/FluentJalium` 0 warning 0 error；`tools/Test-AstraGates.ps1` 全绿（见提交说明的计数）。
- **行为**：`tests/FluentJalium.Tests/AstraSplitButtonTests.cs` 52 例——半区命名契约（正反两向）、
  主半区 `Click` 与命令恰好一次、内容与前景流到半区、分隔线读回自己的行、
  焦点环（根的 `RootRing`）、`FluentDropDownButton` 展开/折叠/双向同步/无 `Flyout` 时拒绝展开/
  禁用时 `Invoke` 抛 `InvalidOperationException`。全部通过框架自身的 `ButtonAutomationPeer` +
  `IInvokeProvider`，**不需要指针，也不会碰到用户桌面上的任何东西**。
- **视觉**：休息底色覆盖 `ControlFillColorDefaultBrush` 后在主半区与次半区同时出现（>5000 px）、
  明暗两主题顶部颜色不同、`#2C2C2E` 与品牌绿 `#207245` 在两个控件的捕获里为 0。
- **硬件输入**：只有 S3 那条是"框架内部接线"级别的真输入证据（自动化模式入口），
  **没有**真指针、真触摸、真键盘按下；悬停/按下的像素仍归未列闸口的 `adaptation/10` 通路。

## 5. Known Gaps（不许用相邻证据替代）

1. **打开态画不出来**：`FlyoutOpen`、`TouchPressed` 无驱动（S1、S2）。这是本控件最显眼的状态之一
   ——菜单打开时上游会整体压暗。我们**没有**伪造驱动（既没有 `IsFlyoutOpen` 的自有类型，也没有反射读私有字段）。
2. **`Checked` 一族 13 条行未转录**，因为 `ToggleSplitButton` 不存在；随之而来的 8 个状态全部未实现。
3. **`SecondaryButtonSpan` 排布未实现**；`FluentDropDownButton` 是另一个类型（`Button` + `Flyout` + `IsExpanded`），
   不是上游那个用 `Button*` 行画、带 `IsExpanded` 的 `DropDownButton : ButtonBase` 的等价物：
   基类是宿主的，API 名字是上游的，`Command`/`Click` 走 `Button` 的语义。
4. **箭头不进像素**：字形与细线笔画不在直方图证据链里（`audits/button.md` 缺口 9），
   箭头的四条行颜色只有"建树 + 读回部件"级别的结构证据。
5. **三条描边行没有可区分像素**（§1 表格第 2 行）：休息、悬停、按下都指向同一实例，
   覆盖 `SplitButtonBorderBrushPointerOver` 不会让画面变化，这不是 bug 而是替换的代价，
   但它确实意味着"这条别名可覆盖"这句话在描边上是空话。
6. **无 `AnimatedIcon` 换帧**，箭头不会跟随状态做位移动画。
7. **`UseSystemFocusVisuals`、`FocusVisualMargin=-1` 无对应属性**，自绘环的位置与上游系统框不逐位一致。
8. **明暗两主题的差异只在休息态验证过**；半区悬停/按下的像素差异未进闸口（依赖真指针通路）。
9. **无 `Expanded`/`Collapsed` 路由事件**（上游 DropDownButton 有），`IsExpanded` 只有属性与自动化读法。
10. **`FluentDropDownButton` 的 `IsExpanded` 不驱动任何视觉**——这一点与上游一致（上游只有 4 个状态），
    所以不能拿"展开时看起来不同"作为证据；它存在的理由是：`FlyoutBase.IsOpen` 不可绑定、不可作格子条件（S2），
    测试与应用需要一个可读回的状态位。
11. **高对比**：本批沿用调色板实例的逐键映射（`HighContrast.map`），
    上游 SplitButton 的 HC 分支里 `…BackgroundPointerOver`→`SystemColorHighlightText` 这类**逐键**选择
    尚未逐条断言（并行任务"高对比逐键断言"）。
