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
   **缺**：`RadioMenuFlyoutItem`、`SplitMenuFlyoutItem`、`MenuScroller`、`MenuScrollViewer`、`Flyout`、`FlyoutPresenter`、`CardElement`。
   **更正（26.10.9 复测，2026-09-19，日志 `adaptation/s0y-outstanding-names.txt`）**：这一行原本还把 `MenuFlyoutPresenter` 算成缺失，**那是错的**——`Jalium.UI.Controls.MenuFlyoutPresenter : Control` 确实在，只是它唯一的公开构造器是 `MenuFlyoutPresenter(MenuFlyout)`，没有无参构造器，所以 `Activator.CreateInstance` 抛 `MissingMethodException`，当时按"造不出来 = 没有类型"记了。它是不是框架给 `MenuFlyout` 用的那层表面（第 3 节把它记成"框架 `MenuPopupScrollHost`，不可样式"）因此**重新变成未结问题**，见 §5 第 9 条。
2. **能否重模板是分类型的**（pass 4 把样式装在建窗之前重装一遍才敢下结论）：`Menu`、`ContextMenu`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`MenuBarItem` 都会实例化装好的模板；**`MenuItem` 存下 `Template` 但从不实例化**——它靠 `OnRender` + `ResolveBackgroundBrush` / `ResolveMenuBrush` / `DrawCheckMark` / `DrawSubmenuArrow` 自绘。
3. **状态杠杆**：`MenuItem` 的 `IsHighlighted` / `IsPressed` / `IsSelected` / `IsSubmenuOpen` 是只读 DP（外部 `SetValue` 抛 “read-only … DependencyPropertyKey”），但进程内合成的 MouseDown 能驱动它们；`MenuFlyoutItem` / `MenuFlyoutSubItem` 只暴露 CLR getter（`IsHighlighted` / `IsSubMenuOpen`），**没有 DP**，所以按下与子菜单展开两格无处可挂。
4. **菜单外观不读任何上游资源名**：以 `MenuBackground`、`MenuFlyoutPresenterBackground` 等 33 个应用级哨兵键装入后，框架自带的菜单/弹层像素纹丝不动。因此颜色一致性只能靠我们自己的消费点，和其余批次一样。
5. **弹层外壳是框架的**：`MenuFlyout` 打开后内容落在 `OverlayLayer > PopupRoot`，其下是框架的 `MenuPopupScrollHost`（两个 `RepeatButton` 箭头 + `ScrollViewer`），那层 Border（`#FF2C2C2E` / `#FF48484A`）我们碰不到；`ContextMenu` 打开后同样被包进这层宿主。
6. **`MenuFlyoutItem` 自量自绘**：它的 `MeasureOverride` 用文本宽度算宽、用常量封顶高（26.10.9 实测 34），并且 `OnRender` 仍会自绘背景。所以：
   - 上游 `11,8,11,9` 内边距 + 4,2,4,2 外边距 + 17 高的文字（合计 38）在 34 的盒子里被压成 13，文字被挤；样式里那条 **38 字面量**是为了让上游行装得下，属于宿主替换而非上游数值。
   - 真指针悬停时框架自绘的那层填充可能盖在我们格子上——无指针，未测。
7. **可达驱动**：`ContextMenu.Open(Point)`、`MenuFlyout.ShowAt/Hide`、`RaiseEvent(MouseDown…)`、自动化 peer（`MenuItemAutomationPeer`→ExpandCollapse；`MenuFlyoutItem`/子项/切换项都是 Invoke）。子菜单是 enter 驱动（`OnSubItemMouseEnter`），`OpenSubMenuAndFocusFirstItem` / `EnsureSubPopup` / `FocusFirstSubMenuItem` 可调用但都不展开，`MenuBarItem.OpenFromKeyboard` 等三个同理——无指针即无结论。
8. **菜单族不读模板部件名**：`GetTemplateChild` / `PART_` 在整个菜单源码里零命中，子菜单弹窗由控件在代码里 `new Popup` 自建。因此本批的 `LayoutRoot` / `IconContent` / `SubItemChevron` 等名字是**上游对齐**而非功能契约（对比 Expander 的三个名字）。
9. **（重影批更正）第 8 条只说对了一半**：不读名字不等于不画。`spike/FlyoutGhostProbe` 直接解 IL（`GetMethodBody().GetILAsByteArray()` 扫 `0x72` ldstr，`Module.ResolveString`）量到四个 flyout 类型各自 `OnRender` 里写死的字面量——
   - `MenuFlyoutItem.OnRender`：`OneSurfaceHover, MenuFlyoutItemBackgroundHover, OneTextDisabled, TextDisabled, Segoe MDL2 Assets, OneTextSecondary, TextSecondary`；
   - `ToggleMenuFlyoutItem.OnRender`：`✓`；`MenuFlyoutSubItem.OnRender`：`OneTextSecondary, TextSecondary`（且该类型 `sealed`）；`MenuFlyoutSeparator.OnRender`：`MenuFlyoutPresenterBorderBrush`。
   也就是说标签、加速键文案、勾选标记、子项箭头、分隔线**全是控件自绘**，我们的模板再摆一份就是第二位画者——这正是用户报的 flyout 重影。`MenuBarItem` 同族同病（它自绘 `Title`，`spike/MenuBarGhostProbe` 量到 1431 亮像素对 603）。
10. **但控件的画者读控件属性**：同一探针的 tint pass 给第 1 行与切换行本地写 `Foreground=#FF00FF`、`FontSize=20`，PrintWindow 抓回的弹窗里那两行标签就是品红、就是 20 号——所以 `Foreground`/`FontSize` 是活的，标签态可以留在样式触发器上。反之覆盖 `TextFillColorSecondaryBrush` 动不了加速键文案、勾选、箭头与分隔线：那几处用的是运行时自己调色板里的 `TextSecondary`/`TextDisabled`/`MenuFlyoutPresenterBorderBrush`，与 pass 4 的 33 个哨兵纹丝不动同构。
11. **`MenuFlyoutItem.Icon` 是 `System.Object`**：控件不画图标（`bare` pass 里图标区是空的），槽位必须留在我们的模板里；`ContentPresenter` 正是承载任意内容对象的正确部件。

## 1. 行去向表（上游 81 条主题行 + 分支外行 → 本实现 16 别名 + 7 Thickness）

| 上游组 | 上游条数 | 落到 | 未落地的原因 |
| --- | --- | --- | --- |
| Item / SubItem / Toggle 的 rest·hover·disabled | 26 | `ThemeResources/MenuFlyout.jalxaml` 16 条别名 | 另 11 条见下一行 |
| 加速键文案 3×2 + chevron 3 + `MenuFlyoutItemChevronMargin` + `MenuFlyoutSeparatorBackground` | 11 | 重影批撤回 | 那几处像素是控件自绘（0.9），发布即承诺我们证不到的消费点 |
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
| `FontIcon`（`CheckGlyph` E73E、chevron E974） | 不画：控件 `OnRender` 自绘 | 重影批：摆 `Path` 就是第二位画者（0.9） |
| `Viewbox` 图标槽 | `Border` + `Icon={x:Null}` 格 | 无 `CheckPlaceholderStates` 可挂；控件不画 `Icon`（0.11） |
| `KeyboardAcceleratorTextVisibility` 状态组 | 不画：控件 `OnRender` 自绘 | 同上 |
| `MenuItem` 可重模板 | 只上色 | 见 0.2 |
| WinUI `MenuFlyoutItem` 行高（≈40） | 38 字面量 | 见 0.6 |

## 3. 状态映射（WinUI VisualState → 本实现的格子）

| 上游组/状态 | 本实现格子 | 写入的行 |
| --- | --- | --- |
| CommonStates / PointerOver | 模板 `IsMouseOver=True` + 样式 `IsMouseOver=True` | `*BackgroundPointerOver`（表面）与 `*ForegroundPointerOver`（标签，走属性：0.10） |
| CommonStates / Pressed | 无 | —（不发布） |
| CommonStates / Disabled | 模板 `IsEnabled=False` + 样式 `IsEnabled=False` | `*BackgroundDisabled` 与 `*ForegroundDisabled` |
| CheckStates / Checked（Toggle） | `IsChecked=True` | 收起 `IconRoot`（上游 CheckedWithIcon 的"图标让位给标记"），标记本身由控件画 |
| CheckPlaceholderStates / IconPlaceholder | `Icon=null`（反向） | 收起 `IconRoot` |
| KeyboardAcceleratorTextVisibility / Visible | 无 | —（文案由控件自绘，收起与否也是它的事） |
| PaddingSizeStates / NarrowPadding | 无 | —（无状态） |
| MenuBarItem CommonStates / PointerOver | `IsMouseOver=True` | `MenuBarItemBackgroundPointerOver` + `…BorderBrushPointerOver` |
| MenuBarItem CommonStates / Pressed、Selected | 无 | —（无属性） |

## 4. 四类证据

- **构建**：`dotnet build FluentJalium.slnx -c Release`（闸口脚本第一步），两本字典 + `Styles/Menus.jalxaml` 已登记进 `Themes/Manifest.txt`（30 本）。
- **行为/资源**：`tests/FluentJalium.Tests/AstraMenuTests.cs`（菜单族 **104** 条断言全过）—— 别名同一性、Thickness 数值、未发布行（含撤回的 11 条）、可重模板类型、`MenuItem` 存而不建、**单画者契约**（`TextBlock`/`KeyboardAcceleratorTextBlock`/`CheckGlyph`/`CheckPlaceholder`/`SubItemChevron` 五个名字必须缺席）、格子表与逐格行名、标签态在样式触发器上、`ShowAt/Hide`、`ContextMenu.Open`、peer Invoke、Toggle 暴露 Invoke 而非 Toggle、勾选让图标收起、`MenuBarItem` 的 `ContentButton.Content` 为空且树里没有承载 Title 的 TextBlock。间距复核批加一条 `The_flyout_rows_measure_upstreams_heights`：条目 38、`LayoutRoot` 34、分隔线 3、线盒 1 且 margin `-4,1,-4,1`。
  - 计数更正：上一版这里写的是"90→126 条"。按 `dotnet test --filter FullyQualifiedName~AstraMenuTests` 实测，本批之前该类是 103 条（`[Fact]` 18 + `[InlineData]` 85），加这条是 104。126 是当时数错了，不是有 23 条被删。
- **视觉**：两条通路分开记。
  - 进程内 `PixelHarness`：`Menu` 的 `Background` 行、flyout 条目的静置填充行覆盖成哨兵后各自进像素（并断言框架灰 `#2C2C2E` 与品牌绿为 0），子项行有 Light↔Dark 差；新增一条**反向**断言——覆盖 `DividerStrokeColorDefaultBrush` 与 `TextFillColorSecondaryBrush` 之后，分隔线区与子项行的哨兵色都是 0，而控件自绘的灰字 `#6E6E73` 仍在，即"画者是它，色不是我们的"。
  - 真上屏合成捕获：`spike/FlyoutGhostProbe` + `spike/VisualQA/capture-window.ps1`（PrintWindow + `PW_RENDERFULLCONTENT`，dpi=168）抓 `PopupWindow` 自己的顶层窗口。修前 `#FFFFFF` 标签层 1194 px 叠在框架的 `#F5F5F7` 1670 px 上；修后 shipped / stripped / bare 三个 pass 的 `#FFFFFF` 都是 **0**，框架层逐像素计数一致（`#F5F5F7x1659 #D6D6D7x556`），且同一 pass 连抓两次差异 0 px——排除交换链伪影。原图留在 `spike/VisualQA/out/probe-{flyout,stripped-a,fixed-a,bare-a}*.png`。
- **硬件输入**：**本批为零**。全部驱动都在进程内（`ShowAt`、`Open(Point)`、`RaiseEvent`、peer），没有一条真指针/触摸/键盘路径被证明，`IsMouseOver` 也无法从外部写入。

### 4b. 间距复核批加的那张弹层捕获（2026-09-19）

`spike/VisualQA/capture-pid-windows.ps1`（新）把探针进程的**每个**顶层窗口分别 PrintWindow 落盘，于是弹窗与
宿主各一张：`spike/VisualQA/out/flyout-400x356-168.png` 是弹窗（228.6x203.4 DIP），`flyout-1330x1085-168.png`
是宿主。逐行扫弹窗：静置表面 `#2C2C2C`，在 **y≈122.3..124.6 DIP** 有一条横穿整行的深色带（`#2B/#26/#25/#2A`，
每行 54 个非底色像素）——那就是框架画的规则，行高从 9 压到 3 之后它仍然完整。弹窗总高 203.4 DIP 与
"5 行 x38 + 分隔线 3 + 上下各 5 的宿主留白"逐数吻合。

同一张图的反面证据：把宿主窗口改成 240x140 之后，弹窗自己只剩 400x219 px（125 DIP、三行）——**弹层窗口被宿主
的剩余空间裁掉**。这条写进 Known Gaps 之外也写进 `adaptation/00` S0-s，因为它会让下一个人把"条目不见了"读成
模板缺陷。

## 5. Known Gaps

1. `MenuItem` 不可重模板：高亮、勾选标记、子菜单箭头三处是框架自绘；本批只拿到 rest/disabled 两个颜色。
2. 悬停与按下无像素证据：`IsMouseOver` 不可外部写，`IsPressed` 在 flyout item 上不存在；且 `MenuFlyoutItem.OnRender` 会自绘 hover 填充，真指针下它是否盖在我们的格子上、两层 hover 会不会叠深，未知（任务 13）。
3. 子菜单/菜单栏展开未证：`OnSubItemMouseEnter` 与 `MenuBarItem.OpenFromKeyboard` 一类路径可调用但无可观察的展开状态。
4. 弹层外壳归框架：`MenuFlyoutPresenter*` 两行只对 `ContextMenu` 的自绘表面兑现承诺，`MenuFlyout` 与 `ContextMenu` 外面那层宿主 Border 仍是框架色；`ContextMenu.MinWidth=140` 也不被宿主尊重。
5. `RadioMenuFlyoutItem`、`SplitMenuFlyoutItem` 无原生类型，其行不发布；`MenuFlyoutItemReveal*` 等 reveal 行等材质批。**更正（2026-09-19 复测）**：这一条原先还把 `MenuFlyoutPresenter` 算进"无原生类型"，那条不成立（见 §0.1 的更正），所以"`MenuFlyoutPresenter*` 行无处可挂"这个理由要换——行不发布现在的真实理由是：没有任何已证的消费者能拿到那个类型（它只有 `MenuFlyoutPresenter(MenuFlyout)` 一个公开构造器），而不是"类型不存在"。
6. 38 高度字面量是对控件自量的补偿，非上游数值；上游 `MenuFlyoutThemeMinHeight`=32 未发布。
7. 触摸/笔与混合 DPI 下的菜单未测；减动效对菜单过渡（0.083s）未资源键化。
8. **控件自绘的那几处文字色拿不到**：加速键文案、勾选标记、子项箭头、分隔线走运行时调色板的 `TextSecondary`/`TextDisabled`/`MenuFlyoutPresenterBorderBrush`，覆盖同名上游行（0.10）与 pass 4 的 33 个哨兵一样不动像素。因此这 11 条上游行撤回而非发布，WinUI 的 `TextFillColorSecondary` 加速键色只能算"运行时自己也是这个灰"的巧合，不声称逐 token 一致。
9. 标签的 hover/disabled 变色由样式触发器写 `Foreground` 达成，但**没有真指针证据**：证到的是"画者读 Foreground"（tint pass 的品红标签），不是"悬停时这一格会被写"。
10. 分隔线那 3 DIP 是**压出来的**，不是上游那条自然度量：上游是 1 高的 `Rectangle` 加它自己 1,1 的 padding；本运行时的 `MenuFlyoutSeparator` 把自己量到 9 高，样式里 `Margin=0` 改得动属性（读回 `0,0,0,0`）却改不动盒子，只有 `Height=3` 够得着（`adaptation/00` S0-s 1）。代价是这条 `Height` 会盖住框架将来的度量变化，所以行高进了断言而不是注释。同一类型也没有 `Background` 行（规则色归框架），因此这一处只有几何是我们说了算。
11. **`MenuPopupScrollHost` 与原生 `MenuFlyoutPresenter` 的关系重新变成未结问题**（2026-09-19 复测带出来的）：§0.5 说弹层外面那层宿主是框架的 `MenuPopupScrollHost`，而 `MenuFlyoutPresenter` 是一个真实存在的 `Control`。两条都可能是真的（宿主包着 presenter），也可能 `MenuPopupScrollHost` 只是那层 Border 的名字而 presenter 才是表面本身。要分开只需要一次：`MenuFlyout.ShowAt` 之后从上屏的 `OverlayLayer > PopupRoot` 往下打整棵视觉树，看 `MenuFlyoutPresenter` 有没有实例出现、它的 `Background`/`CornerRadius` 是谁写的。**这一条也直接决定 `MenuBar` 下拉的半径欠账**（`MenuBarItem` 自己 `new MenuFlyout()`，见 `adaptation/00` 下一节的记录），所以它排在下一批。
