# 右侧空隙与导航换行审计（2026-09-19）

上游基线：`microsoft-ui-xaml` `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`；方法基线：`ModernWpf`
`23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。引用仓库只读，本批未改动它们。

## 1 · 用户读数与把它变成数

> 你这个下拉框或者说一些控件，它的右边是空了很大一块的，明显这个空隙不存在。你对照一下 ModernWPF 和
> Microsoft ui xaml 项目，因为你这个右边就是多了一块长度，它长度和左边的空隙是不对齐的。然后，第二个就是
> 你需要记住，导航栏的文本是不需要自动换行的。

两句都可判定。第一句先排除掉一个可能：控件**内部**的左右内缩没有偏差——`ComboBoxPadding` `12,5,0,7`、
`TextControlThemePadding` `10,5,6,6`、`ExpanderChevronMargin` `20,0,8,0`、`ComboBoxEditableTextPadding`
`11,5,38,6` 与上游逐字相同（`ComboBox_themeresources.xaml:341-342`、`Common_themeresources.xaml:12/26/40`、
`Expander_themeresources.xaml:81`），探针的树读回也对得上（260 宽的 ComboBox：文本左缩 12、箭头右缩 14、
箭头列 38）。所以"右边多出来的一块"不在控件盒子里。

它在**弹层**里。`spike/RightGapProbe` 把嫌疑控件按同一 260 DIP 宽度摆在已上屏窗口上，进程外用
`spike/VisualQA/capture-pid-windows.ps1` 抓该进程的每个顶层窗口，再按 DIP 逐行扫非背景像素：

| 对象 | 弹窗顶层窗口 | 画出来的表面 | 右侧空白 | 左侧空白 |
| --- | --- | --- | --- | --- |
| `AutoCompleteBox` 建议列表 | 260.0x80.0 DIP | 0.6..147.4 DIP | **112.6 DIP，纯 `#000000`** | 0.6 |

左 0.6、右 112.6——"长度和左边的空隙不对齐"就是这个。

## 2 · 根因

框架给 `Popup` 元素本身写了本地 `Width`（读回 `width=260 localWidth=260`，`min=260 localMin=260`），
也就是弹窗的顶层窗口按控件宽度开；但它用**无穷大**量 `Popup` 的子节点，于是我们模板里那张表面只拿到内容的
 desired 宽度。`HorizontalAlignment=Stretch` 在无穷大量度下不起作用（加上之后帧逐像素不变），够得着盒子的
只有 `MinWidth`。

把 `MinWidth` 接回控件宽度时撞到第二层：弹层子节点被搬走之后，`{TemplateBinding}` 与
`{RelativeSource AncestorType=…}` 在 `SuggestionsContainer` 上都读回 **0**，而字面量 `MinWidth="260"`
能读回 260 —— 属性本身没被解析器丢掉，丢的是绑定。`{Binding ActualWidth, ElementName=OuterBorder}` 读回
260。对照：ComboBox 的 `PART_PopupBorder` 用 `{TemplateBinding ActualWidth}` 就能读到 260。
差别是弹层被接到哪一棵树（ComboBox 的下拉进宿主窗口的 overlay 层，建议列表有自己的顶层窗口）。
底座事实记在 `adaptation/00` 的 S0-t。

## 3 · 改动

- `src/FluentJalium/Styles/Selection.jalxaml` — `PART_PopupBorder` 加 `MinWidth="{TemplateBinding ActualWidth}"`。
- `src/FluentJalium/Styles/AutoSuggestBox.jalxaml` — `SuggestionsContainer` 加 `HorizontalAlignment="Stretch"`
  与 `MinWidth="{Binding ActualWidth, ElementName=OuterBorder}"`。
- `src/FluentJalium/Styles/Navigation.jalxaml` — `PART_Label` 里放隐式 `TextBlock` 样式，
  `TextWrapping=NoWrap`（属性直接写在 presenter 上是死格子）。

## 4 · 四类证据

- **构建**：`tools/Test-AstraGates.ps1`（restore → build → test → 调色板漂移）。Debug **623/623、0 skip**。
  Release 通道没能走完整脚本：闸口第 1 步拒绝启动，因为机器上有一个**别的项目**的 `testhost.exe`
  （`C:\git\agent\TinadecOffice\...`，PID 18564）在跑，闸口按进程名找占用者，这是它该有的行为，不去杀它。
  于是 Release 手工跑同三步：`dotnet build -c Release` 0 错误 → `dotnet test -c Release --no-build`
  第一遍 **1 失败 / 622 通过**，第二遍 **623/623**，`Sync-AstraPalette.ps1 -Check` 三主题 `checked=True`。
  **那条失败没有名字**：重跑即绿，抓不到用例名，所以这是一条未命名的 flake（约 1/623），本批不声称已定位。
  上一批记下的四条 Debug-only 失败（`AstraSliderTests` x2、
  `AstraNumberBoxTests.A_focused_compact_numberbox_opens_its_spinner_popup`、
  `AstraAutoSuggestBoxTests.A_resting_box_paints_its_rows_and_nothing_invented`）在本批 Debug 闸口里全部消失，
  而闸口开头 `taskkill` 掉了两个上一遍遗留的 `testhost.exe`（PID 36352 / 21320）——它们共用同一个宿主窗口，
  所以那四条是采集器串台的假阳性，不是产品缺陷。这条更正同时说明上一批 ROADMAP 里"两通道各一遍"当时
  只有 Release 成立。
- **行为**：
  - `AstraComboBoxTests.An_open_combo_grafts_its_dropdown_into_the_overlay_and_wears_its_rows` 新增两条：
    `popupBorder.MinWidth == combo.ActualWidth`、`popupBorder.ActualWidth == combo.ActualWidth`。
  - `AstraAutoSuggestBoxTests.The_suggestions_surface_spans_the_box_instead_of_its_longest_row`（新）：
    `container.MinWidth == box.ActualWidth`、`container.ActualWidth == box.ActualWidth`。
  - `AstraNavigationTests.A_long_pane_label_stays_on_one_line`（新）：长标签下
    `TextBlock.TextWrapping == NoWrap`、条目 `ActualHeight == 36`、文本盒 < 30。
    这条在改之前是**红的**，报 `Expected NoWrap, Actual Wrap` 与 `Expected 36, Actual 55.34`——
    它先证明缺陷为真，再证明修复为真。
- **视觉**：两条通路分开。
  - ComboBox 下拉：真上屏窗口里逐行扫非背景像素，下拉表面 **242.9..502.3 DIP = 259.4**，与它挂着的
    ComboBox（243..504）同宽；原图 `spike/VisualQA/out/rightgap-combo.png-1330x1575-168.png`。
    属性侧同一版本从宿主往下走读到 `PART_PopupBorder 260x117.3 min=260 localMin=UnsetValue`。
  - ~~修前的对照：`rightgap-open.png-455x140-168.png`（建议列表 148/260）~~ —— **这条读数作废，见 §7。**
- **硬件输入**：**本批为零**。全部驱动仍在进程内（`IsDropDownOpen`、`Text` setter、`Focus()`），
  没有一条真指针/触摸路径被证明。

## 5 · Known Gaps

1. **`AutoCompleteBox` 建议列表的宽度只在 harness 一侧有证据。** `ElementName` 绑定在 harness（弹层被 graft
   进宿主 overlay）里读到 260；真上屏那一侧**没有证据**——本批原本用来反证的 148/260 后来作废（§7·1）。
   探针里 `IsDropDownOpen=true` / `Text` setter 都不能把 `ComboBox`、`AutoCompleteBox` 的弹层送上屏
   （整屏截图里两个控件都是闭合的，见 §7·3），所以样式侧够不够得着仍未判定。
   `{TemplateBinding}` 与两种 `RelativeSource` 在 graft 之后实测 0，字面量会冻住一个用户可拖宽的盒子。
2. ~~**`NumberBox` 无 Header 时高 39 而不是 32。**~~ **已结（2026-09-20 追批）。**
   `HeaderContentPresenter` 的 `0,0,0,8` 是本地 margin，Content 为空也照扣（读回：presenter `0x0 margin=0,0,0,8`，
   `OuterBorder` 31，控件 39）。上游那个 presenter 默认 `Collapsed`、由代码在 Header 存在时才打开。
   追批验证了样式侧的等价写法：`<Trigger Property="Header" Value="{x:Null}">` 在本运行时**会触发**，
   把 presenter 收成 `Collapsed` 就连 margin 一起请出布局。两通道各读一遍：闸口里
   `A_headerless_numberbox_does_not_pay_the_headers_gap` 断到 32，另一个进程的树读回也从 39 变 32；
   反向也钉住——`Header` 一给，presenter 回 `Visible`，控件按"文本行高 + 8"长回去。
   这条 cell 不在上游的 cell 清单里，所以 `The_numberbox_template_carries_one_cell_per_upstream_state`
   的期望表里把它单列一行并注明是替代，不是转录。
3. 建议列表条目的高亮是框架本地值，实测为紫色 `#6A0881` 一族，不是任何 Fluent token；
   `An_item_takes_our_text_row_but_keeps_the_frameworks_own_fill` 已经把这条损失钉住，本批不重复声称能改。
4. ComboBox 下拉的 259.4 DIP 是"与宿主同宽"，不是上游的机制：上游由 `ComboBox` 的代码给 presenter
   设宽，这里由样式给表面设下限。长条目仍会把列表撑宽（`MinWidth` 是地板），这一点与上游一致但没有单独用例。
5. 本批只量了 260 DIP 这一个宽度。同一控件在 160/440/拉伸下的左右关系未测；`MaxWidth=440` 的 Gallery
   字段与卡片右边界之间那段空白是页布局，不是控件度量，本批没动。
6. 减动效、触摸/笔、混合 DPI 下的弹层宽度未测。

## 6 · 不声称

不声称"右侧空隙已全量消除"——ComboBox 有属性 + 合成帧两证，建议列表只有 harness 一证且真窗口反证。
不声称导航换行修好了所有文本：只证了 `PART_Label` 这一处，`Selection.jalxaml:27/130` 与
`Inputs.jalxaml:368` 上那几个 `TextWrapping="Wrap"` 同样是死格子，本批没碰。
不声称任何 hover/press/触摸状态：本批硬件输入为零。

## 7 · 追批更正（2026-09-20，"左右边距不一样长"再查）

用户第二次报同一件事，并把范围说成"下拉框 / flyout 这一类型"。这一批的结论是：**上一批找错了对象。**

**1 · "148/260" 那张图不是弹层。** `rightgap-*.png-455x140-168.png`（260x80 DIP）在 `closed`、`combo`、
`suggest2`、`suggest3`、`suggest4` 五种模式下的 `painted` 计数与非黑扫描范围**逐位相同**
（10864 / 19082 / 2041278；0..148 DIP）。一个闭合的探针里不该有任何弹层——所以那个 260x80 的可见顶层
根本不是建议列表，"表面只画了 148/260"从头到尾是一条读错窗口的证据。§4 的该行与 Known Gap 1 的
原表述已按此作废。`AutoCompleteBox` 的 `MinWidth` 绑定**上屏与否未测**，不是"上屏无效"。

**2 · 真正的左右不等长是滚动条槽。** 整屏采集（`spike/VisualQA/grab-screen.ps1`，新）配合 Gallery 页
逐像素扫描，量到 `page-selection.png` 里样卡离内容区**左 25.1 DIP、右 36.6 DIP**，声明值是
`PageHost Margin="24,8,24,24"`。差的 11.5 DIP 与 `AstraNavigationTests` 早就钉住的那条同量：
**26.10.9 的 `ScrollViewer` 在 `Auto` 下只在右侧扣走 12 DIP 布局宽度**，而 WinUI 的滚动条是 overlay、
不占布局。所以这不是某个控件的度量写错，是**每一个会溢出的表面都右边窄一条**——页面、ComboBox 下拉、
建议列表全中。同一页里样卡右边缘 1820、Live output 右边缘 1841，也是这 12 DIP 造成的两条线。
改法沿用仓内既有决定：`Auto` → `Hidden`（`A_hidden_bar_still_lets_the_pane_scroll` 已证明滚轮与键盘
照常）。落点：`Selection.jalxaml` 的 `PART_ScrollViewer`、`AutoSuggestBox.jalxaml` 的
`PART_DropDownScrollViewer`、`MainWindow.jalxaml` 的 `PageScrollViewer` 与 Live output 宿主。
复测：左 25.1 / 右 24.6，skew 从 11.5 收到 −0.6 DIP，两条右边缘并成一条（1841）。
代价说清楚：**可见滚动条没了**，上游溢出时是显示条的；这是这条基座的替代，不是等价。

**3 · 探针开不了这两个弹层。** `combo.Focus(); combo.IsDropDownOpen = true;` 读回 `True`、
`AutoCompleteBox` 的 `Text` setter 也一样，但整屏截图里两个控件都是闭合的，`PART_Popup` 只有
`260x32`（控件自身的高度，不是下拉高度）。所以 §1 那条"上屏未测"没法用现有探针关掉，需要一次真指针
或框架侧的开口。`MenuFlyout` 是唯一能这样开出来的弹层。

**4 · flyout 一族的左右是对称的，圆角也没有量出错。** 用 `MenuFlyout` 上屏那张逐行扫：条目文本左
16.0 DIP = 上游 `1(border) + 0(presenter padding) + 4(MenuFlyoutItemMargin) + 11(MenuFlyoutItemThemePadding)`；
带勾选列的行 44.6 DIP = 再加 `12+16` 的 CheckGlyph，与上游 `MenuFlyout_themeresources.xaml:483-487`
的 `Auto/*/Auto` 三列一致。样式侧读回 `LayoutRoot 210.5x34 margin=4,2,4,2 padding=11,8,11,9 radius=4,4,4,4`，
`OverlayCornerRadius=8`、`ControlCornerRadius=4` 逐字对上 `ComboBox.jalxaml` / `Metrics.jalxaml`。
`Expander` 的头部圆角在 `IsExpanded=True` 时由 `Surfaces.jalxaml:95` 改成 `4,4,0,0`，与上游
`Expander.xaml:35/64` 的 Top/Bottom filter 同形。**本批没有在这些值上找到缺陷**，用户看到的
圆角异常更可能是 §2 那条窄条造成的视觉偏移，或归 §5·3 那条框架本地高亮。

**5 · MenuBar 的下拉不归样式管。** `crop-flyout.png` 里"New tab / Close window"文本缩进约 43 DIP，
比 `MenuFlyout` 的 16 DIP 宽出一整列。`Menus.jalxaml:316` 已经写明原因：26.10.9 的 `MenuItem`
存了 Template 却从不构建，只能给颜色，行盒与缩进由框架自己画。这条属于 `audits/menu-flyout.md`，
不在本批的可改范围内。
