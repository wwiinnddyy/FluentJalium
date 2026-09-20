# 菜单族审计（阶段 4 第二段）

上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- `controls/dev/CommonStyles/MenuFlyout_themeresources.xaml`，blob `6f9f3fd322583d2d537dc30c439e62accf1efbd4`（1050 行：Dark 分支 4-86、High Contrast 87-169、Light 170-252、分支外 254-264、样式 265 起）
- `controls/dev/MenuBar/MenuBar_themeresources.xaml`，blob `96cf1b1e788a327c5c00e07af0e3fc3cbf23e40b`（Light/Default 分支 18-28、分支外 45-46）
- `controls/dev/MenuBar/MenuBar.xaml` + `MenuBarItem.xaml`（模板，含 `ContentRoot` / `Background` / `ContentButton` 三个名字）
- `controls/dev/CommonStyles/RadioMenuFlyoutItem_themeresources.xaml`，blob `078fadf4058d7c6b269b335350a075d6c079ab03`（只有样式，无主题分支）
- `_perf2026` 变体已核对：把 Storyboard 机械改写成 Setter，键集合逐字节相同，因此不作为第二份对照。

运行时：NuGet Jalium.UI **26.10.9**。测量：`spike/MenuProbe`（pass 1-6，原始日志 `menu-probe1..6.txt` 全部留在原处）、`spike/FlyoutSurfaceProbe`（表面身份与半径，2026-09-20，原始输出 `adaptation/s1a-menu-surface-raw.txt`）。

## 0. 先量后写：pass 1-6 量到的事实

1. **整族都是原生类型**：`Menu : MenuBase : ItemsControl`、`MenuItem : HeaderedItemsControl`、`MenuBar`、`MenuBarItem`（自有 DP 只有 `Title`，`Items` 是普通 CLR 列表）、`ContextMenu : MenuBase`（9 个自有 DP，含可写的 `IsOpen`/`Placement`/`StaysOpen`）、`MenuFlyout : FlyoutBase`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`（+`IsChecked`）、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`Separator`、`CommandBar` 家族。
   **缺**：`RadioMenuFlyoutItem`、`SplitMenuFlyoutItem`、`MenuScroller`、`MenuScrollViewer`、`Flyout`、`FlyoutPresenter`、`CardElement`。
   **更正（26.10.9 复测，2026-09-19，日志 `adaptation/s0y-outstanding-names.txt`）**：这一行原本还把 `MenuFlyoutPresenter` 算成缺失，**那是错的**——`Jalium.UI.Controls.MenuFlyoutPresenter : Control` 确实在，只是它唯一的公开构造器是 `MenuFlyoutPresenter(MenuFlyout)`，没有无参构造器，所以 `Activator.CreateInstance` 抛 `MissingMethodException`，当时按"造不出来 = 没有类型"记了。它是不是框架给 `MenuFlyout` 用的那层表面（第 3 节把它记成"框架 `MenuPopupScrollHost`，不可样式"）因此**重新变成未结问题**，见 §5 第 9 条。
2. **能否重模板是分类型的**（pass 4 把样式装在建窗之前重装一遍才敢下结论）：`Menu`、`ContextMenu`、`MenuFlyoutItem`、`ToggleMenuFlyoutItem`、`MenuFlyoutSubItem`、`MenuFlyoutSeparator`、`MenuBarItem` 都会实例化装好的模板；**`MenuItem` 存下 `Template` 但从不实例化**——它靠 `OnRender` + `ResolveBackgroundBrush` / `ResolveMenuBrush` / `DrawCheckMark` / `DrawSubmenuArrow` 自绘。
3. **状态杠杆**：`MenuItem` 的 `IsHighlighted` / `IsPressed` / `IsSelected` / `IsSubmenuOpen` 是只读 DP（外部 `SetValue` 抛 “read-only … DependencyPropertyKey”），但进程内合成的 MouseDown 能驱动它们；`MenuFlyoutItem` / `MenuFlyoutSubItem` 只暴露 CLR getter（`IsHighlighted` / `IsSubMenuOpen`），**没有 DP**，所以按下与子菜单展开两格无处可挂。
4. **菜单外观不读任何上游资源名**：以 `MenuBackground`、`MenuFlyoutPresenterBackground` 等 33 个应用级哨兵键装入后，框架自带的菜单/弹层像素纹丝不动。因此颜色一致性只能靠我们自己的消费点，和其余批次一样。
5. **弹层外壳是框架的**：`MenuFlyout` 打开后内容落在 `OverlayLayer > PopupRoot`，其下是框架的 `MenuPopupScrollHost`（两个 `RepeatButton` 箭头 + `ScrollViewer`），那层 Border（`#FF2C2C2E` / `#FF48484A`）我们碰不到；`ContextMenu` 打开后同样被包进这层宿主。
   **更正（2026-09-20 复测，`spike/FlyoutSurfaceProbe`）**：这条把两件事混成一件了。`(a)` `ContextMenu` 的那层外壳**不是** `MenuPopupScrollHost`，而是框架在我们模板之外新建的一个 `Border`，且它**吃得到我们的行**——`Background`/`BorderBrush`/`BorderThickness`/`CornerRadius` 四项由控件抄成它的本地值，`MenuFlyoutPresenter*` 那几格正是经这一次复制才落到像素（详见 0.12）。`(b)` `MenuFlyout` 打开后根本不在宿主的 `OverlayLayer` 里，它在自己顶层 `PopupWindow` 里，表面是 `MenuFlyoutPresenter`（0.13）。"碰不到"只剩 `(b)` 那一半。
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
12. **`ContextMenu` 的表面是框架抄出来的一层 `Border`，抄的是"控件被告知的值"**：`PopupRoot > Border > MenuPopupScrollHost > ScrollViewer > 条目`，那层 `Border` 的 `Background`/`BorderBrush`/`BorderThickness`/`CornerRadius` 都是本地值，由控件在打开时抄过去，而且**抄是活的**（开起来后把控件半径改成 3，表面立刻读 3；清掉本地值又回到样式给的 8）。抄的条件量清了：控件显式写 0 → 表面 0（所以 0 是真实请求，不是"未设置"哨兵）；控件从未被告知任何值 → 表面留在框架自带的 **14**；往那层 `Border` 自己身上写不是出路（清掉之后掉到 0，它背后没有样式）。`MenuFlyout`/`FlyoutBase`/`Popup`/`PopupRoot` 整条链上都没有半径属性，只有 `ContextMenu` 有 → **样式 setter 是这张表面唯一能听我们的入口**。
13. **`MenuFlyoutPresenter` 是 `internal sealed : Control`，自带 8 但不画卡片**：公开构造器唯一签名 `(MenuFlyout)`、自己不声明任何依赖属性、按类型键与按名字键查隐式样式都是 `null` → 样式改不动它，C# 里点名直接 CS0122。它打开时读回 `radius=8,8,8,8 LOCAL`、`bt=1,1,1,1 LOCAL`、`Background`/`BorderBrush` **为 null**——半径已经和上游一致（不需要我们），卡片色则完全归框架。反射把它挂进我们的树里能长出完整的内部（`MenuPopupScrollHost`、条目 r=4、分隔线 margin `-4,1,-4,1`），证明那些本地值是类型自己写的、不是样式查找的结果；产品代码不做反射，这条只用于量边界。

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
| `MenuFlyoutPresenter` | `ContextMenu`：框架抄出来的那层 `Border`（吃我们的行，0.12）；`MenuFlyout`：真正的 `MenuFlyoutPresenter`，`internal sealed`、无隐式样式、`Background` 为 null | 2026-09-20 复测：原记"框架 `MenuPopupScrollHost`（不可样式）"把两种表面混成一种，`MenuPopupScrollHost` 是抄出来的 `Border` 的孩子 |
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
- **行为/资源**：`tests/FluentJalium.Tests/AstraMenuTests.cs`（菜单族 **110** 条断言全过）—— 别名同一性、Thickness 数值、未发布行（含撤回的 11 条）、可重模板类型、`MenuItem` 存而不建、**单画者契约**（`TextBlock`/`KeyboardAcceleratorTextBlock`/`CheckGlyph`/`CheckPlaceholder`/`SubItemChevron` 五个名字必须缺席）、格子表与逐格行名、标签态在样式触发器上、`ShowAt/Hide`、`ContextMenu.Open`、peer Invoke、Toggle 暴露 Invoke 而非 Toggle、勾选让图标收起、`MenuBarItem` 的 `ContentButton.Content` 为空且树里没有承载 Title 的 TextBlock。间距复核批加一条 `The_flyout_rows_measure_upstreams_heights`：条目 38、`LayoutRoot` 34、分隔线 3、线盒 1 且 margin `-4,1,-4,1`。
  - 计数更正：上一版这里写的是"90→126 条"。按 `dotnet test --filter FullyQualifiedName~AstraMenuTests` 实测，本批之前该类是 103 条（`[Fact]` 18 + `[InlineData]` 85），加这条是 104。126 是当时数错了，不是有 23 条被删。
  - **表面身份复测批（2026-09-20）+6 条 → 110**：`The_context_menu_style_writes_upstreams_radius_on_the_row_the_framework_copies`（样式那格读回 `OverlayCornerRadius`）、`A_context_menu_surface_is_the_frameworks_border_wearing_our_rows_and_upstreams_radius`（爬到那层 `Border`：行名 + 半径 + `bt=1` 全在）、`A_submenu_surface_wears_upstreams_radius_from_the_framework_itself`（`Menu` 子菜单同一条通路）、`The_flyout_presenter_type_is_real_internal_and_unstyleable_from_our_markup`（类型在、`IsPublic=False`、`sealed`、无隐式样式）、`A_flyouts_presenter_is_in_the_tree_but_paints_nothing_of_ours`（它在树里而 `Background` 为 null）、`The_copied_surface_takes_the_radius_and_the_edge_it_is_told_to_take`（本地写 3 就走 3、`BorderThickness` 同理）。
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

### 4c. 半径复测批：一张卡片的两个角（2026-09-20）

用户那句"有的控件圆角好像都不对"这次变成了数。全族打开后的表面半径（`spike/FlyoutSurfaceProbe -Mode sweep`，
上游 = 飞出表面 `OverlayCornerRadius` 8、条目 `ControlCornerRadius` 4）：

| 表面 | 修前 | 修后 | 谁写的 |
| --- | --- | --- | --- |
| `ContextMenu` 卡片 | **14** | **8** | 框架从我们样式的 setter 抄过去（0.12） |
| `Menu` 子菜单卡片 | 8 | 8 | 同一条复制通路 |
| `MenuFlyout` 卡片 | 8 | 8 | 框架自己写的，我们改不动也不需要改（0.13） |
| `ComboBox` 下拉卡 | 8 | 8 | 我们的模板 `PART_PopupBorder` |
| `AutoCompleteBox` 建议卡 | 8 | 8 | 我们的模板 `SuggestionsContainer` |
| 菜单条目 | 4 | 4 | 我们的模板（与上游同值） |

角本身用真上屏帧对照（`capture-context.ps1 -Mode context` / `-Mode context14`，读数由新工具
`spike/VisualQA/corner-edge.ps1` 按"第一个非底色列"取外沿，底色 `#1C1C1E`）。三帧同一张卡片、同一条左边
x=350..479、宽 130 px = 74.27 DIP×1.75，只差半径：**弧深**在 8 的那一档两次重采都是 9~10 行，14 那一档是
17 行；控件显式写 0 时弧消失。**顶行咬入不作论据**：同一个 `context` 模式重采一次就从 12 px 变到 9 px
（弹窗落点两帧差 0.67 DIP，顶行第一个非底色列正好被这件事挪动），写出来是因为量过，不是因为可信。
弧的绝对尺寸也不等于 半径×1.75（8 DIP 应 14 px 弧、量到 9~10；14 DIP 应 24.5、量到 17），两档都短同一个系数，
所以这两帧是"变化证明"、不是尺子；半径数值以读回为准。是渲染器把弧画短、还是这条测法吃掉几个像素，需要一次
跨 DPI 或跨已知半径的对照才能分开，本机一台 175% 显示器给不了——记成上游开放问题（见 5.12），它会挪动
全项目每一个圆角。

进程内那条"角上第一个填充列"的像素断言这次**没留下**：同一个 `Open()` 在探针自己 show 的窗口里给 74.27x70 的
表面，在 xunit 共享宿主里给 0×0，`PixelHarness.PixelAt` 直接抛"has no layout to sample"。改成钉复制契约（宿主读
得到那一层），弧的证据写在帧里——测不到的东西不伪装成测到。

## 5. Known Gaps

1. `MenuItem` 不可重模板：高亮、勾选标记、子菜单箭头三处是框架自绘；本批只拿到 rest/disabled 两个颜色。
2. 悬停与按下无像素证据：`IsMouseOver` 不可外部写，`IsPressed` 在 flyout item 上不存在；且 `MenuFlyoutItem.OnRender` 会自绘 hover 填充，真指针下它是否盖在我们的格子上、两层 hover 会不会叠深，未知（任务 13）。
3. 子菜单/菜单栏展开未证：`OnSubItemMouseEnter` 与 `MenuBarItem.OpenFromKeyboard` 一类路径可调用但无可观察的展开状态。**2026-09-20 复测仍然成立**：`IsSubmenuOpen` 不是 `MenuBarItem` 的属性（读回 `null`），进程内合成 MouseDown 之后条目仍挂在 `Window` 根下（父链 `StackPanel < MenuBar < StackPanel < Window`），弹窗没有实例化。`Menu` 的**子菜单**倒是能开（`MenuItem` 上合成 MouseDown → `submenu open=True`，其表面见 4c），所以缺的只有菜单栏那一条。
4. 弹层外壳归框架：**`ContextMenu` 那一半已经收回来**（2026-09-20 复测）。它的卡片是框架抄出来的 `Border`，`MenuFlyoutPresenterBackground` / `…BorderBrush` / `…BorderThemeThickness` / 半径四格都是从控件抄过去的本地值（0.12），所以那三行对 `ContextMenu` 是**逐像素兑现**的，不再是"只对自绘表面兑现"。**剩下一半仍归框架**：`MenuFlyout` 的表面是 `MenuFlyoutPresenter`，`Background`/`BorderBrush` 为 null、卡片色由它自己的 `PopupWindow` 提供（`#FF2C2C2E` / `#FF48484A`），我们没有任何通路；`ContextMenu.MinWidth=140` 也不被那层宿主尊重（表面实测 74.27 宽装两条短标签）。
5. `RadioMenuFlyoutItem`、`SplitMenuFlyoutItem` 无原生类型，其行不发布；`MenuFlyoutItemReveal*` 等 reveal 行等材质批。**更正（2026-09-19 复测）**：这一条原先还把 `MenuFlyoutPresenter` 算进"无原生类型"，那条不成立（见 §0.1 的更正），所以"`MenuFlyoutPresenter*` 行无处可挂"这个理由要换——行不发布现在的真实理由是：没有任何已证的消费者能拿到那个类型（它只有 `MenuFlyoutPresenter(MenuFlyout)` 一个公开构造器），而不是"类型不存在"。**2026-09-20 再更正**：这三行其实有已证的消费者——`ContextMenu` 的那层复制（0.12）。所以"无处可挂"也不成立了，真实的理由是：它是**运行时自己那层表面的属性**，而我们的 `MenuFlyout` 路径拿不到它（0.13）。
6. 38 高度字面量是对控件自量的补偿，非上游数值；上游 `MenuFlyoutThemeMinHeight`=32 未发布。
7. 触摸/笔与混合 DPI 下的菜单未测；减动效对菜单过渡（0.083s）未资源键化。
8. **控件自绘的那几处文字色拿不到**：加速键文案、勾选标记、子项箭头、分隔线走运行时调色板的 `TextSecondary`/`TextDisabled`/`MenuFlyoutPresenterBorderBrush`，覆盖同名上游行（0.10）与 pass 4 的 33 个哨兵一样不动像素。因此这 11 条上游行撤回而非发布，WinUI 的 `TextFillColorSecondary` 加速键色只能算"运行时自己也是这个灰"的巧合，不声称逐 token 一致。
9. 标签的 hover/disabled 变色由样式触发器写 `Foreground` 达成，但**没有真指针证据**：证到的是"画者读 Foreground"（tint pass 的品红标签），不是"悬停时这一格会被写"。
10. 分隔线那 3 DIP 是**压出来的**，不是上游那条自然度量：上游是 1 高的 `Rectangle` 加它自己 1,1 的 padding；本运行时的 `MenuFlyoutSeparator` 把自己量到 9 高，样式里 `Margin=0` 改得动属性（读回 `0,0,0,0`）却改不动盒子，只有 `Height=3` 够得着（`adaptation/00` S0-s 1）。代价是这条 `Height` 会盖住框架将来的度量变化，所以行高进了断言而不是注释。同一类型也没有 `Background` 行（规则色归框架），因此这一处只有几何是我们说了算。
11. **已结（2026-09-20，`spike/FlyoutSurfaceProbe -Mode tree/sweep`）**：`MenuPopupScrollHost` 与 `MenuFlyoutPresenter` 的关系结清了——**两条都是真的，且分属两种表面**。`MenuFlyout` 的是 `PopupWindow > PopupRoot > MenuFlyoutPresenter > MenuPopupScrollHost`（presenter 在上面，是宿主 `Control`）；`ContextMenu` 的是 `PopupRoot > Border > MenuPopupScrollHost > ScrollViewer`，那里**根本没有 presenter**，外壳是一个普通 `Border`，四格由控件抄过去（0.12）。`MenuBar` 的下拉半径欠账同时收掉：`Menu` 的子菜单走 `ContextMenu` 那条复制通路，读回 `radius=8,8,8,8 LOCAL`（0.13 与 4c）。
12. **弧的量比它自己的名义半径短，且顶行那一格不稳**（4c 的三帧）：`CornerRadius=8` 在 dpi 168 下应给 14 px 的弧，弧深度量到 9~10；`=14` 应给 24.5，量到 17。同一张卡片的宽度方向逐数吻合 1.75 倍，所以不是采集器缩放错了。另外**顶行咬入在同一个模式重采之间就有 3 px 抖动**（12 → 9，弹窗落点差 0.67 DIP），因此本批只用"弧深"这一档稳定的量做对比。两档半径短同一个系数 ⇒ 比值可信、绝对值不可信，帧因此只证"变了"。**未解释，且不只影响菜单**：如果这个系数成立，全项目每一个圆角都比名义值小，会影响与 WinUI 的 1:1 判定；要定性需要一次跨 DPI 或跨已知半径的对照，本机给不了，所以不在这里下结论。
13. **`ContextMenu` 的卡片没有我们那份亚克力**：那层复制 `Border` 的 `Background` 抄到的是 `{ThemeResource MenuFlyoutPresenterBackground}`，帧上读出 `#FF2C2C2C`——与 4b 里 `MenuFlyout` 弹窗的框架灰同一支。也就是说 `ContextMenu` 现在**画的是卡片色，但不是 acrylic**（背衬在本运行时未接）。与 5.4 的"卡片色归框架"是同一件事的两面：一个拿不到我们的行，一个拿到了却没有材质。材质批（任务 8）欠。

## 更正（哑格批 2026-09-20，`adaptation/00` S1-f）

全仓哑格普查在本族命中 **6** 条：`MenuFlyoutItem` / `ToggleMenuFlyoutItem` / `MenuFlyoutSubItem` 三条样式的模板里
各有两条 `Setter TargetName="IconContent" Property="Foreground"`，而 `IconContent` 是 `ContentPresenter`——
本运行时该类型没有 `Foreground` 成员，那 6 格永远无事。本族的标签行**没有**中招：它们一直作为
`Style.Triggers` 的活格写在控件上（§3 与 S0-q 的那条"标签读 `Foreground`"），所以菜单的外观不因这批变化。
处理是**删除**这 6 条重复副本而不是重定向：同一行的活格已经在写控件，`IconContent` 自己又带
`Foreground="{TemplateBinding Foreground}"`，留着只是埋一个日后与活格抢优先级的死格。
随动：`ToggleMenuFlyoutItem` 模板里那条 `IsEnabled=False` 触发器删掉最后一格后变空，一并撤掉（该态的底色本族从不写，
见 §3）。`AstraMenuTests` 两处同调：`Each_item_style_carries_one_cell_per_reachable_state` 里该样式的状态集合去掉
`IsEnabled=False`（模板现在确实没有那一格），另一条逐格断言理论里把该格当契约的 `InlineData` 一行删除（−1 条事实）。
守卫：`AstraForegroundRoutingTests.A_disabled_menu_item_carries_its_row_into_the_icon_as_well_as_the_text`
钉图标与标签同时取到行——这条是删格子之后唯一的证据通路，仍然只是属性读回，不是像素。

## 更正（属性死写批 2026-09-20，`adaptation/00` S1-g）

同三处 `IconContent` 上还各挂着一条 `Foreground="{TemplateBinding Foreground}"` 属性（`Styles/Menus.jalxaml:66 / :117 / :160`），
`ContentPresenter` 没有这个成员，上一条更正删掉格子之后它就是纯空转，一并删除。
`MenuBarItem` 那段还有一条真死掉的：根 `ContentRoot` 是 `Grid`，却带着 `CornerRadius="{TemplateBinding CornerRadius}"`（`:300`）。
另外那层 Grid 与内层圆角 Border **绑同一支笔刷**，本批把它也删了（只让 Border 画）——但要说清楚：
`MenuBarItemBackground` 别名 `SubtleFillColorTransparentBrush`，底层方角画的是 `#00FFFFFF`，
**静置与悬停两态都看不见**这条溢出；会看见的条件是应用自己给条目设 `Background`（本地值经 `TemplateBinding` 喂给两层）。
读回见 `AstraSurfaceGeometryTests.A_menu_bar_item_paints_its_fill_once_so_the_corners_stay_round`
（静置 + 本地填色两半：Grid 不声明 `Background`，Border 取到那一支）。
这条更正还要记一次自打：该事实**第一版**读的是 `root.GetValue(Control.BackgroundProperty)`，而 `Grid` 不是 `Control`，
那格依赖属性与它自己声明的 `Background` 不是同一个对象——元素带着 `#00FFFFFF` 时读数仍是 `null`，
是一条永远为真的断言（两行并排放一次就分出来，见 `spike/AttributeSweep/menuitem-fill-first.txt`）。
不声称：四角**没有像素捕获**（`IsMouseOver` 从外部写不进去，S0-m），只到属性读回 + 形状推理。
