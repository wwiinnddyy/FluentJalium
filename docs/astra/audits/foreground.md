# 前景继承审计（全库文本部件）

日期：2026-09-21。运行时：NuGet Jalium.UI 26.10.9（权威版本）。探针：`spike/ForegroundProbe`（原始读数
`adaptation/s6-foreground-probe-raw.txt`）。闸口：`tests/FluentJalium.Tests/AstraForegroundAuditTests.cs`（14 条）。
上游参照：microsoft-ui-xaml `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。

## 0 起因

阶段 6 量到一处用户可见哑色：`FluentInfoBadge` 的数值标签带着框架默认深墨，而它所在控件的前景是白
（`adaptation/00` 第 4 条），同一条里留了一句"未审的相邻面"——其余自有样式里靠继承拿前景的文本部件可能同样哑。
本段把那句话结清：不再逐控件猜，而是把全库的前景声明列成清单，逐条问"它到不到"。

## 1 这一层写前景的三种形状（数量差别很大，之前只数过第一种）

| 形状 | 行数 | 说明 |
| --- | --- | --- |
| 要素属性 `Foreground="{ThemeResource/StaticResource K}"` | 9 个键 / 15 行中的 9 行 | 模板部件自己点名一个令牌 |
| 要素属性 `Foreground="{TemplateBinding Foreground}"` | 6 行 | 把控件自身的前景带进模板（InfoBadge 那一次修法） |
| `Setter Property="Foreground"` | 228 行 | 样式格子与模板 trigger 里的状态色，**占绝大多数** |
| `DataTemplate` 内的无名行 | 1 行 | 评级的未选星（`ThemeResources/RatingControl.jalxaml:44`），前两类的扫描漏掉它 |

`{ThemeResource}` 写错键名不报错：markup 装载放行、属性拿到继承墨、构建全绿。所以第 3 类那 228 行此前没有任何一道
闸在问"键名不存在会怎样"。

## 2 结论（都是树上读数，理由见第 4 节）

1. **0 条死键**：Light、Dark 两档各把 243 条声明点名的每个键解析一遍，全部是 `SolidColorBrush`。
2. **0 条声明不到达**：可达的具名声明逐条比对"部件自身前景 == 该键解析出的色"，两档各 3 条（InfoBar 的
   `Title`/`Message`/`IconGlyph`）全对；`{TemplateBinding}` 那 3 条用"把控件前景改成一个调色板产不出的色"证明跟随
   （`ForegroundProbe` v3 读数：`FluentInfoBadge.ValueTextBlock`、`FluentRatingControl.Caption`、
   `SplitButton.PrimaryButton` 全部跟随）。
3. **42 个可读文本节点全部随主题翻转**，没有一个停在框架默认墨上——这是"未审的相邻面"的直接答案：InfoBadge 那一类
   在本层是孤例，且已经修掉。
4. **低对比只剩 5 条，且不是本层的偏差**：评级满星用强调色（`#0078D4` 叠页面 `#F3F3F3` = 4.08；Dark 侧
   `#60CDFF` 叠 `#202020` = 9.06），这是任何强调色文本在 Fluent 浅档页面上的固有数值，不是哑色。
5. **上游 InfoBar 的 `ForegroundSet` 状态对本层不可表达**：上游只在 `ForegroundSet` 视觉状态里让标题跟随控件前景
   （`InfoBar.xaml:76-80,113-114`），本运行时没有状态管理器，所以"给 InfoBar 设一个本地前景色"不会染到标题——
   这与上游一致，不是缺陷；记为已知偏差（第 5 节）。

## 3 本层与上游一致、但值得钉住的两对色

* InfoBar 严重度标记：`#FFFFFF` 画在**兄弟要素** `Ellipse.IconBackground` 的 `#0078D4` 圆盘上（Light 4.53，
  Dark 侧 `#000000` 叠 `#60CDFF` = 11.67）。圆盘是兄弟不是祖先，所以断言只能按部件名配对，已在
  `A_severity_mark_lands_on_the_disc_that_paints_behind_it` 钉住。
* 评级未选星：Light `#9E000000`、Dark `#C5FFFFFF`，即上游 `RatingUnreadStarColor` 的 61% 黑 / 77% 白。
  由 `The_unread_rating_star_keeps_its_alpha_not_just_its_hue` 钉住 alpha，而不只是色相。

## 4 仪器自身错了三次（这部分比结论更有复用价值）

1. **`ReadLocalValue` 看不见 markup 写**。第一版判据是"部件没有本地前景 setter ⇒ 模板没写前景"，于是把 InfoBar
   那三处令牌前景判成缺陷。`{ThemeResource}` 不是本地值。改法：声明清单从 markup 读，证明靠"解析值比对"或"改控件前景看跟不跟"。
2. **沿祖先链找背景，看不见兄弟图元**；而且把所有被测控件塞在同一个宿主 `StackPanel` 里时，别的被测控件会被当成
   "前面的兄弟"。于是 InfoBar 标题一度被算成"画在另一个 `FluentInfoBadge` 的蓝底上"。改法：背景回退只在
   subject 内部走，并且只有 `Grid`/`Canvas` 这类重叠布局的兄弟才算底。
3. **打印色不带 alpha**，于是上游 61% 黑的未选星被读成"纯黑文本"，看起来像一处新缺陷。同时"全库只有 16 处写前景"
   这句差点进文档——那只是 attribute 形状，`Setter` 形状有 228 行。**均匀读数先怀疑仪器**这条老账再一次应验。
4. 顺带量到的框架事实（省掉后面每一族一次试验）：`Control.ForegroundProperty`、`TextBlock.ForegroundProperty`、
   `TextElement.ForegroundProperty` 是**同一个对象**（`DependencyProperty.AddOwner` 直接 `return this`，
   `Jalium.UI.Core/DependencyProperty.cs:397-416`），两行都注册 `inherits: true`
   （`Control.cs:180-182`、`TextBlock.cs:76-78`）。所以一次翻主题能同时判全库，"读部件自己的属性"也不必在两个 DP 间挑。

## 5 牙的验证（A/B）

把 `Styles/Surfaces.jalxaml:170` 的 `InfoBarTitleForeground` 改成一个不存在的键，重建后跑这 14 条：
**3 红**（解析闸的 Light/Dark 两档 + 清单闸），其余 11 绿；改回后 **14/14 绿**，`git diff` 对该文件为空。
这次 A/B 同时量出闸口的灵敏边界：**"读部件自身前景"那条保持绿**，因为 InfoBar 标题的令牌色与继承墨本来就同色
（都是 `TextFillColorPrimary`）——只有令牌与继承色不同的配对（`IconGlyph` 白对继承黑、InfoBadge 值对强调底）
才能由读数发现死键。所以灵敏的那道是"键必须解析成刷"，读数断言是第二道。

## 6 四类证据

* **构建**：`dotnet build tests/...` `0 个警告 / 0 个错误`；串行闸口读数见 ROADMAP 本段。
* **行为**：新增 14 条（两档键解析、attribute 键清单 + 6 条 TemplateBinding 计数、InfoBar 三部件两档读数、
  6 个族文本节点随主题翻转、未选星 alpha、严重度标记对圆盘对比）。
* **视觉**：本段只有树上读数，没有像素断言——理由是老账 #50（文本字形在任何捕获通路都拿不到墨），
  断言色身份靠树上读数与合成对比计算。
* **硬件输入**：本段不动输入路径。

## 7 Known Gaps（不声称清单）

1. 探针没有 subject 行、因此**未测**的 7 个模板 owner：`AppBarToggleButton`、`DataGridCell`、`DataGridColumnHeader`、
   `MenuBarItem`、`MenuFlyoutItem`、`MenuFlyoutSubItem`、`ToggleMenuFlyoutItem`。菜单项与表格单元是文本最密集的
   两处，它们的前景只由第 5 节的"键必解析"闸覆盖，没有"到达部件"的读数。
2. 5 处具名声明在实测树上取不到：`FluentTeachingTip.TitleTextBlock`/`SubtitleTextBlock`（提示未真正弹出）、
   `FluentTabViewItem.IconHost`、`FluentBreadcrumbBarItem.PART_ChevronTextBlock`、`ComboBox.PART_ScrollViewer`。
3. 兄弟图元判底用的是"重叠布局面板 + 尺寸同阶"的启发式，不是几何命中（本运行时没有可用的跨要素变换读数），
   所以它可能漏判异形叠色。
4. 文本是否**真的印出墨**仍不在断言里（#50）；#46 的五处 1 DIP `Rectangle` 无墨仍未结。
5. 高对比档下这些前景是否走 `HighContrast` 映射，本段没测——那是逐键映射闸（已存在）之外的一层。
6. 状态格子（hover/pressed/disabled）的 228 条 Setter 只测了"键能解析"，没测"该状态下这一格确实被画上"。
