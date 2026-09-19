# 菜单族审计（阶段 4 第二段）

上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- `controls/dev/CommonStyles/MenuFlyout_themeresources.xaml`，blob `6f9f3fd322583d2d537dc30c439e62accf1efbd4`（1050 行：Dark 分支 4-86、High Contrast 87-169、Light 170-252、分支外 254-264、样式 265 起）
- `controls/dev/MenuBar/MenuBar_themeresources.xaml`，blob `96cf1b1e788a327c5c00e07af0e3fc3cbf23e40b`（Light/Default 分支 18-28、分支外 45-46）
- `controls/dev/MenuBar/MenuBar.xaml` + `MenuBarItem.xaml`（模板，含 `ContentRoot` / `Background` / `ContentButton` 三个名字）
- `controls/dev/CommonStyles/RadioMenuFlyoutItem_themeresources.xaml`，blob `078fadf4058d7c6b269b335350a075d6c079ab03`（只有样式，无主题分支）
- `_perf2026` 变体已核对：把 Storyboard 机械改写成 Setter，键集合逐字节相同，因此不作为第二份对照。

运行时：NuGet Jalium.UI **26.10.9**。测量：`spike/MenuProbe`（pass 1-6，原始日志 `menu-probe1..6.txt` 全部留在原处）。

## 0. 先量后写：pass 1-6 量到的事实

1. **整族都是原生类型**：`Menu : MenuBase : ItemsControl`、`MenuItem : HeaderedItemsControl`、`MenuBar`、`MenuBarItem`（自有 DP 只有 `Title`，`Items` 是普通 CLR 列表）、`ContextMenu : MenuBase`（9 个自有 DP，含可写的 `IsOpen`/`Placement`/`StaysOpen`）、`MenuFlyout : FlyoutBase`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`（+`IsChecked`）、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`Separator`、`CommandBar` 家族。
   **缺**：`RadioMenuFlyoutItem`、`SplitMenuFlyoutItem`、`MenuFlyoutPresenter`、`MenuScroller`、`MenuScrollViewer`、`Flyout`。
2. **能否重模板是分类型的**（pass 4 把样式装在建窗之前重装一遍才敢下结论）：`Menu`、`ContextMenu`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`MenuBarItem` 都会实例化装好的模板；**`MenuItem` 存下 `Template` 但从不实例化**——它靠 `OnRender` + `ResolveBackgroundBrush` / `ResolveMenuBrush` / `DrawCheckMark` / `DrawSubmenuArrow` 自绘。
3. **状态杠杆**：`MenuItem` 的 `IsHighlighted` / `IsPressed` / `IsSelected` / `IsSubmenuOpen` 是只读 DP（外部 `SetValue` 抛 “read-only … DependencyPropertyKey”），但进程内合成的 MouseDown 能驱动它们；`MenuFlyoutItem` / `MenuFlyoutSubItem` 只暴露 CLR getter（`IsHighlighted` / `IsSubMenuOpen`），**没有 DP**，所以按下与子菜单展开两格无处可挂。
4. **菜单外观不读任何上游资源名**：以 `MenuBackground`、`MenuFlyoutPresenterBackground` 等 33 个应用级哨兵键装入后，框架自带的菜单/弹层像素纹丝不动。因此颜色一致性只能靠我们自己的消费点，和其余批次一样。
5. **弹层外壳是框架的**：`MenuFlyout` 打开后内容落在 `OverlayLayer > PopupRoot`，其下是框架的 `MenuPopupScrollHost`（两个 `RepeatButton` 箭头 + `ScrollViewer`），那层 Border（`#FF2C2C2E` / `#FF48484A`）我们碰不到；`ContextMenu` 打开后同样被包进这层宿主。
6. **`MenuFlyoutItem` 自量自绘**：它的 `MeasureOverride` 用文本宽度算宽、用常量封顶高（26.10.9 实测 34），并且 `OnRender` 仍会自绘背景。所以：
   - 上游 `11,8,11,9` 内边距 + 4,2,4,2 外边距 + 17 高的文字（合计 38）在 34 的盒子里被压成 13，文字被挤；样式里那条 **38 字面量**是为了让上游行装得下，属于宿主替换而非上游数值。
   - 真指针悬停时框架自绘的那层填充可能盖在我们格子上——无指针，未测。
7. **可达驱动**：`ContextMenu.Open(Point)`、`MenuFlyout.ShowAt/Hide`、`RaiseEvent(MouseDown…)`、自动化 peer（`MenuItemAutomationPeer`→ExpandCollapse；`MenuFlyoutItem`/子项/切换项都是 Invoke）。子菜单是 enter 驱动（`OnSubItemMouseEnter`），`OpenSubMenuAndFocusFirstItem` / `EnsureSubPopup` / `FocusFirstSubMenuItem` 可调用但都不展开，`MenuBarItem.OpenFromKeyboard` 等三个同理——无指针即无结论。
8. **菜单族不读模板部件名**：`GetTemplateChild` / `PART_` 在整个菜单源码里零命中，子菜单弹窗由控件在代码里 `new Popup` 自建。因此本批的 `LayoutRoot` / `IconContent` / `SubItemChevron` 等名字是**上游对齐**而非功能契约（对比 Expander 的三个名字）。

## 1. 行去向表（上游 81 条主题行 + 分支外行 → 本实现 26 别名 + 8 Thickness）

| 上游组 | 上游条数 | 落到 | 未落地的原因 |
| --- | --- | --- | --- |
| Item / SubItem / Toggle 的 rest·hover·disabled | 26 | `ThemeResources/MenuFlyout.jalxaml` 26 条别名 | — |
| `*Pressed*` | 11 | 不发布 | flyout item 无按下 DP，无格可挂 |
| `*SubMenuOpened*` | 2 | 不发布 | `IsSubMenuOpen` 是只读 CLR getter |
| `*Reveal*` | 24 | 不发布 | 本运行时无 reveal 材质 |
| `MenuFlyoutItemPlaceholderThemeThickness` / `…Double…` / `…PaddingNarrow` | 3 | 不发布 | 属 `CheckPlaceholderStates` / `PaddingSizeStates` 组，运行时无对应状态；图标槽改用 `Icon={x:Null}` 格收起 |
| `MenuFlyoutItemTextTrimming` | 1 | 不发布 | `TextTrimming` 资源本 reader 从未解析成功 |
| `MenuFlyoutSystemBackdrop` | 1 | 不发布 | 背衬对象，材质批欠 |
| `MenuFlyoutLightDismissOverlayBackground` | 1 | 不发布 | 上游由原生代码按名读取，发布即承诺我们证不了的像素 |
| `MenuFlyoutSeparatorHeight`=1 / `MenuFlyoutThemeMinHeight`=32 / 两条 Split x:Double | 4 | 字面量 | `x:Double` 不解析；32 只给 `ContextMenu` 表面，不给条目 |
| MenuBar 别名（含 hover）| 6 | `ThemeResources/MenuBar.jalxaml` | — |
| MenuBar pressed/selected 四条 | 4 | 不发布 | `MenuBarItem` 只有 `Title`，第四状态上游就叫 `Selected` 而无对应属性 |
| `MenuBarHeight`=40 | 1 | 字面量 | `x:Double` |
| 三条 `Thickness`（presenter/item/subitem/separator 等） | 8 | 两本字典共 11 条 Thickness | — |

值替换一处：`MenuFlyoutPresenterBackground` 别到 `AcrylicInAppFillColorDefaultBrush`（本调色板无 `DesktopAcrylicTransparentBrush`），与 `ThemeResources/FlyoutPresenter.jalxaml` 同一先例。

名字替换两处，都写进注释与 Catalog：`Menu` / `MenuBar` 读 `MenuBar*` 行（上游根本没有 `Menu_themeresources.xaml`，ModernWpf 也是这么做的）；`ContextMenu` 读 `MenuFlyoutPresenter*` 行（WinUI 的右键菜单表面就是 flyout presenter，ModernWpf 的 `ContextMenu*` 三条指向同一对颜色）。

## 2. 宿主替换清单

| WinUI | 本实现 | 原因 |
| --- | --- | --- |
| `MenuFlyoutPresenter` | 框架 `MenuPopupScrollHost`（不可样式） | 弹层宿主由框架构造 |
| `RadioMenuFlyoutItem` | `ToggleMenuFlyoutItem` 代替 | 类型缺失 |
| `SplitMenuFlyoutItem` | 无 | 类型缺失 |
| `FontIcon`（`CheckGlyph` E73E、chevron E974） | `Path` | 本运行时图标字体 glyph 不可靠（记忆：icon font defects） |
| `Viewbox` 图标槽 | `Border` + `Icon={x:Null}` 格 | 无 `CheckPlaceholderStates` 可挂 |
| `KeyboardAcceleratorTextVisibility` 状态组 | 文案格（空/null → Collapsed） | 状态由代码写，改由值驱动 |
| `MenuItem` 可重模板 | 只上色 | 见 0.2 |
| WinUI `MenuFlyoutItem` 行高（≈40） | 38 字面量 | 见 0.6 |

## 3. 状态映射（WinUI VisualState → ControlTemplate.Triggers）

| 上游组/状态 | 本实现格子 | 写入的行 |
| --- | --- | --- |
| CommonStates / PointerOver | `IsMouseOver=True` | Background/Foreground/Chevron/Accelerator 的 `*PointerOver` |
| CommonStates / Pressed | 无 | —（不发布） |
| CommonStates / Disabled | `IsEnabled=False` | `*Disabled` 全套 |
| CheckStates / Checked（Toggle） | `IsChecked=True` | `CheckGlyph.Opacity=1` |
| CheckPlaceholderStates / IconPlaceholder | `Icon=null`（反向） | 收起 `IconRoot` |
| KeyboardAcceleratorTextVisibility / Visible | `KeyboardAcceleratorTextOverride=""` / `=null`（反向） | 收起文案块 |
| PaddingSizeStates / NarrowPadding | 无 | —（无状态） |
| MenuBarItem CommonStates / PointerOver | `IsMouseOver=True` | `MenuBarItemBackgroundPointerOver` + `…BorderBrushPointerOver` |
| MenuBarItem CommonStates / Pressed、Selected | 无 | —（无属性） |

## 4. 四类证据

- **构建**：`dotnet build FluentJalium.slnx -c Release`（闸口脚本第一步），两本字典 + `Styles/Menus.jalxaml` 已登记进 `Themes/Manifest.txt`（30 本）。
- **行为/资源**：`tests/FluentJalium.Tests/AstraMenuTests.cs` —— 别名同一性、Thickness 数值、未发布行、可重模板类型、`MenuItem` 存而不建、部件名、格子表与逐格行名、`ShowAt/Hide`、`ContextMenu.Open`、peer Invoke、Toggle 暴露 Invoke 而非 Toggle、勾选圈不透明度、禁用色、悬停行名。
- **视觉**：同文件的像素用例改走**表面与描边**——`Menu` 的 `Background` 行、flyout 条目的静置填充行覆盖成哨兵后各自进像素（并断言框架灰 `#2C2C2E` 与品牌绿为 0），子项 chevron 有 Light↔Dark 差，分隔线与 chevron 覆盖后可见。**标签文字不进像素断言**：纯文字主体在这条离屏通路上写 0 像素（`PixelHarness.Build` 的既有注记），文字色只证到实例身份。悬停一格因此只断"行名 + 与静置行不是同一实例"，像素半边明说欠着。
- **硬件输入**：**本批为零**。全部驱动都在进程内（`ShowAt`、`Open(Point)`、`RaiseEvent`、peer），没有一条真指针/触摸/键盘路径被证明，`IsMouseOver` 也无法从外部写入。

## 5. Known Gaps

1. `MenuItem` 不可重模板：高亮、勾选标记、子菜单箭头三处是框架自绘；本批只拿到 rest/disabled 两个颜色。
2. 悬停与按下无像素证据：`IsMouseOver` 不可外部写，`IsPressed` 在 flyout item 上不存在；且 `MenuFlyoutItem.OnRender` 会自绘背景，真指针下我们的格子是否被盖住未知（任务 13）。
3. 子菜单/菜单栏展开未证：`OnSubItemMouseEnter` 与 `MenuBarItem.OpenFromKeyboard` 一类路径可调用但无可观察的展开状态。
4. 弹层外壳归框架：`MenuFlyoutPresenter*` 两行只对 `ContextMenu` 的自绘表面兑现承诺，`MenuFlyout` 与 `ContextMenu` 外面那层宿主 Border 仍是框架色；`ContextMenu.MinWidth=140` 也不被宿主尊重。
5. `RadioMenuFlyoutItem`、`SplitMenuFlyoutItem`、`MenuFlyoutPresenter` 无原生类型，其行不发布；`MenuFlyoutItemReveal*` 等 reveal 行等材质批。
6. 38 高度字面量是对控件自量的补偿，非上游数值；上游 `MenuFlyoutThemeMinHeight`=32 未发布。
7. 触摸/笔与混合 DPI 下的菜单未测；减动效对菜单过渡（0.083s）未资源键化。
