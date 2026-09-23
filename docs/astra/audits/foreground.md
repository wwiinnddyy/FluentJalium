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

1. ~~探针没有 subject 行、因此**未测**的 7 个模板 owner~~ —— **已结 6 个**（2026-09-21 余账批，ROADMAP"前景审计余账批"）：
   `AppBarToggleButton`（carrier 是具名部件 `LabelText`）、`DataGridCell`、`DataGridColumnHeader`、`MenuBarItem`、
   `MenuFlyoutSubItem`、`ToggleMenuFlyoutItem` 现在各有两档"到达 carrier"的读数，`MenuFlyoutItem` 早在 #55 就有。
   留下的部分是：这六条只到 **carrier**，carrier 若是控件自身（菜单项、表格单元）则"标签真以该色画出"仍不在断言里
   （#50），且 6 个 owner 只有 2 个状态格被量（勾选、列头禁用），hover/pressed 行照旧归真指针通路（#13）。
1b. sentinel 通路在**别名单元**上不可用（本批量出）：`FluentThemeManager.OverrideBrush` 经 `GetBrush` 只认生成调色板，
   对 `DataGridRowForeground` 这类别名键直接 `KeyNotFoundException`；改打它别名指向的调色板刷，或 mount 之后翻档，
   carrier 一律不动（`#E4000000` 原地不动，6/6）。可判定的写法只剩"在该档下 mount、两档各一条"，
   而它的牙由 A/B 证明：抽掉 `Styles/Menus.jalxaml:288` 只有 `menu-bar-item` 两档红，回落值是继承墨
   （`#FF1D1D1F` / `#FFF5F5F7`）不是默认黑——所以"等于令牌"才是干活的那条。
2. ~~5 处具名声明在实测树上取不到：`FluentTeachingTip.TitleTextBlock`/`SubtitleTextBlock`（提示未真正弹出）、
   `FluentTabViewItem.IconHost`、`FluentBreadcrumbBarItem.PART_ChevronTextBlock`、`ComboBox.PART_ScrollViewer`~~
   —— **这一条整条作废**（2026-09-23 #87 逐处复测）：五处全都有读者，两处早就有（`AstraBreadcrumbBarTests` 的
   `PART_ChevronTextBlock`、`AstraTabViewTests` 的 `IconHost` 各有一条 `Assert.Same` 身份读数），
   `PART_ScrollViewer` 只需打开下拉就能从宿主覆盖层按名取到，而提示那两处**从 `PART_Popup.Child` 往下走就取到**
   ——带 placement target 的 Popup 把内容长在它自己的顶层 `PopupWindow` 里（`spike/TeachingTipProbe`），
   它不是宿主的子节点，所以"从宿主窗口找"这一种走法注定空手；原来的"取不到"是走法写错，不是部件没实化。
   三处新的到达读数落在 `AstraForegroundArrivalTests`。
2b. 上面那三条"等于令牌"的读数，牙只在一处：**改指另一个键**（三条逐条试过，各红自己那一条、其余四条绿，
   部件读到 `#5C000000`，`spike/ForegroundArrival/mut-swap-*.log`）。**删掉整行是看不见的**：三种形状都试过
   （裸身份、先在控件上写一品红 carrier、最终那版五条事实），三行逐条删除后 5/5 恒绿（`mut-bare-*.log`、
   `mut-carrier-*.log`、`mut-del-*.log`）。原因是量出来的：这个运行时里"什么都没写上去的 `TextBlock`"既不无墨、
   也不继承，它带的就是**当前档的 `TextFillColorPrimaryBrush` 同一个实例**（Light `#E4000000` / Dark `#FFFFFFFF`，
   `spike/ForegroundArrival/probe-default-ink.log`），而这三行转录的正是那支刷。所以"删了就看得见"这种断言
   在这里做不出来，能做的只有"指着谁"；这些行是否**真的印出墨**照旧不在断言里（#50）。
3. 兄弟图元判底用的是"重叠布局面板 + 尺寸同阶"的启发式，不是几何命中（本运行时没有可用的跨要素变换读数），
   所以它可能漏判异形叠色。
4. 文本是否**真的印出墨**仍不在断言里（#50）；#46 的五处 1 DIP `Rectangle` 无墨仍未结。
5. 高对比档下这些前景是否走 `HighContrast` 映射，本段没测——那是逐键映射闸（已存在）之外的一层。
6. 状态格子（hover/pressed/disabled）的 Setter 只测了"键能解析"，没测"该状态下这一格确实被画上"。
   这一层的行数要说清尺子才不重复第 4 节那次事故（`spike/ForegroundRoutingCensus/census-2026-09-23.txt`，
   同一棵树三种尺子）：`TargetName` 写入 569 行、`Property="Foreground"` 的 Setter 237 行（其中 29 行两者皆是）、
   全库 `<Setter>` 1507 行。第 4 节第 3 条当年数的 228 是**当时那棵树**的 `Property="Foreground"` 计数，
   其后各批又添了行，现在同尺子是 237。
6b. 上面那条"没测被画上"在 2026-09-23 #88 结掉四个 owner，但**结出来的不是"都画上了"**：四条事实落在
   `AstraStateCellArrivalTests`，每条配两种突变（把那一格删掉 / 把它改指另一支刷），十个突变各一次重建一轮测点（4 删 + 4 改指 + 2 复合），
   每次 revert 后 `git diff` 对相关样式文件为空（`spike/ForegroundArrival/mut-{point,cell,mask}-*.log`）。
   逐格读数（Light，控件自身的 `Foreground`）：

   | owner / 那一格 | 格子住在哪 | 删掉 | 改指成白（`AccentButtonForeground`） | 谁在写 |
   |---|---|---|---|---|
   | `SubtleButtonForegroundDisabled`（`Common.jalxaml:68`） | `Style.Triggers` | 4/4 恒绿 | 只本条红，读到 `#FFFFFFFF` | **我们这格**压得过框架那次写 |
   | `RepeatButtonForegroundDisabled`（`Common.jalxaml:102`） | `Style.Triggers` | 恒绿 | 只本条红，读到 `#FFFFFFFF` | 同上 |
   | `CheckBoxForegroundUncheckedDisabled`（`Selection.jalxaml:39`） | `ControlTemplate.Triggers` | 恒绿 | **也恒绿** | 只有框架那次写（见下） |
   | `CheckBoxForegroundChecked`（`Selection.jalxaml:33`） | `ControlTemplate.Triggers` | 恒绿 | 只本条红，读到 `#5C000000` | **我们这格** |

   两格同状态、同属性，只因住在不同层就一个能压过框架的写、一个不能——这是本批量出来的不对称，不是推断。
   而"删掉恒看不见"在禁用格上还多了一层理由：把 `TextDisabled` 这个名字改指二级墨（`#9E000000`）之后，
   抽掉自己那格的按钮与勾选框**才**跟着读到 `#9E000000`（`mut-mask-subtlecellmask.log`、
   `mut-mask-checkboxcellmask.log`，各只红自己那一条）。所以 #12 那条"框架从名字 `TextDisabled` 现查禁用墨"
   的通路不止长在生成标签上，控件自身的 `Foreground` 也是它写的；这也是为什么第四条事实那句主张钉的是调色板刷
   而不是别名键——钉别名会跟着突变一起动，什么都证明不了。
6c. 承上，落在 `ControlTemplate.Triggers` 里的禁用前景格今天**改不动**：单独重写
   `CheckBoxForegroundUncheckedDisabled`（以及同组 `…CheckedDisabled`、`…IndeterminateDisabled`）不改变任何像素，
   上游会认这个覆盖。今天看不见是因为这三行转录的正是 `TextDisabled` 指向的那一支刷；一旦有人只改这几族键就是用户可见的失效。
   候选修法已有证据：把该状态的 `Foreground` 行提到 `Style.Triggers`（同表前两行证明那里压得过）。
   本批没动产品标记，改动登记成 #89。同一条判据**不外推**：`TabView.jalxaml:38`、`:71` 两行也是模板触发器，
   但本批只量到 CheckBox 一个 owner，那两处照旧是"只测了键能解析"。
6c-bis. #89 那条候选修法（把禁用 `Foreground` 行提到 `Style.Triggers`）**已判掉，不做**，理由是把两段已量的读数放在一起：
   提到样式触发器只救得到**控件自身的** `Foreground`（7.6b 前两行证得了它压得过框架那次写），而用户真正看见的那片墨是
   **生成的标签**，标签上带的是框架自己的局部值，#12 量过它从名字 `TextDisabled` 现查（本批 7.6b 又在控件自身上量到同一条
   路）。也就是说：单独重写 `CheckBoxForegroundUncheckedDisabled` 这一族键，无论那一格住在样式层还是模板层，
   都到不了标签的像素——这不是"我们少写了一处"，是**该面没有可用的标记杠杆**（局部值压在一切格子之上，而我们既不能
   反射框架私有字段，也不做逐窗修表）。所以它按 Known Gap 记在这里，而不是拿一次"提到 Style.Triggers"的改动冒充结清；
   真要覆盖那族键，今天唯一通的路是改 `TextDisabled` 指向的那支调色板刷本身（那会同时改掉所有控件的禁用墨）。
   顺带一条风险记录：那一次"提升"还会把 hover/pressed 的格子间关系卷进来（跨层先后本段没量），在没有可见收益的前提下
   引入这份不确定，不值得。
6d. 普查的尺子本身在本批错过一次，先记下来：`spike/StateCellCensus/census.py` 第一版只认 `<Trigger>` 与
   `<ConditionGroup>`，而库里的多条件格子写成 `<MultiTrigger><MultiTrigger.Conditions><Condition …/>`——
   `<Trigger` 匹配不上 `<MultiTrigger`、`ConditionGroup` 匹配不上 `Condition`，开合两头都不匹配，
   所以**带勾选条件的 39 行是整批缺席**，不是被错分。修好后同尺子是 **176 条状态格子、91 条无指针可驱动、
   37 条的键名在测点里没出现过**，其中"无指针 × 无读者"13 条。这条弱信号也要一起记：`readers=` 判的是
   "键名字面量在测点里出现过"，本批那条勾选框事实**故意**不写键名（它钉的是调色板刷，理由在 7.6b），
   于是普查把那一行仍报成"无读者"——它能提示"要不要看第二眼"，不是到达性的证据。
6e. 那 13 行逐条处置（能不能出事实，取决于该行转录的刷与"没有这格时读到的刷"是不是同一支）：

   | 那一行 | 位置 | 转录到 | 能否出事实 | 处置 |
   |---|---|---|---|---|
   | `CheckBoxForegroundUncheckedDisabled` | `Selection.jalxaml:39` | 禁用刷 | 删、改指都看不见 | 本批事实 + 复合突变（7.6b） |
   | `CheckBoxForegroundCheckedDisabled` / `…IndeterminateDisabled` | `:40` / `:41` | 禁用刷 | 同上 | 与上一行同判据，归 #89 |
   | `CheckBoxForegroundIndeterminate` | `:36` | 主文字刷 | 与静态格同实例 | 值不可判，不出事实 |
   | `TabViewButtonForegroundDisabled` ×2 | `TabView.jalxaml:38` / `:71` | 禁用刷 | 预测同 7.6c，**未量** | 留账，不外推 |
   | `TabViewItemHeaderForegroundSelected`、`TabViewItemIconForegroundSelected` | `:203` | 主文字刷（静态是二级刷） | 删行就看得见 | 已出事实（#91，见 7.6f） |
   | `TabViewItemHeaderSelectedCloseButtonForeground` | `:203` | 主文字刷（静态也是主文字刷） | 同实例 | 值不可判 |
   | `TabViewItemIconForegroundDisabled`、`TabViewItemHeaderDisabledCloseButtonForeground` | `:210` | 禁用刷 | 只删看不见 | 需改指 + 复合两种突变才说得出 |
   | `InfoBarSuccess/WarningSeverityIconForeground` | `Surfaces.jalxaml:199` / `:205` | 反色刷 | 四行严重度全指同一支刷 | `AstraForegroundAuditTests` 已读该实例，不重复出事实 |

6f. #91（2026-09-23）把上表那两行**选中态**的 TabView 格量完，读数与前面几格相反：**这一格删掉就看得见**，不必借复合突变。
   `Styles/TabView.jalxaml:203` 同一行里的两处 `Foreground` 各出一条事实（`AstraStateCellArrivalTests` 第五、六条），
   四种突变（每格各"删掉 / 改指成白"）逐轮一次重建、每轮只红自己那一条、revert 后 `TabView.jalxaml` 对 HEAD 干净：

   | 那一格 | 载体 | 删掉 | 改指成白 | 干净树上的读数 |
   |---|---|---|---|---|
   | `TabViewItemHeaderForegroundSelected` | 控件自身 `Foreground` | 只本条红，回落 `#9E000000` | 只本条红，读到 `#FFFFFFFF` | 选中 `#E4000000` / 未选中那片仍 `#9E000000` |
   | `TabViewItemIconForegroundSelected`（`TargetName="IconHost"`） | 部件 `ContentControl.Foreground` | 只本条红，回落 `#9E000000` | 只本条红，读到 `#FFFFFFFF` | 选中部件 `#E4000000` |

   两处值得单记：① 回落值是**静态那格**的二级墨，不是控件在选中态的主文字墨——#51 那条"模板部件不继承前景"在这里
   又一次成立，所以部件这格与其父格不是互为不可判读者（7.6b 里 `…Checked` 与 `…Indeterminate` 那种同实例关系在这里没出现）；
   ② 事实必须成对读（选中那片 + 旁边那片），只断言选中项的话，"该行对所有项都写"也会满足它。
   判据**不外推**：同文件 `:38`、`:71` 两行禁用格本批仍没量，`TabViewItemHeaderSelectedCloseButtonForeground`
   与静态格同实例所以依旧出不了事实（见上表），而这几格是否**真的印出墨**照旧不在断言里（#50）。
   普查是这批之前的快照，`readers=` 那两格当时报"无读者"；#91 之后按同一脚本重跑，键名无读者的数从 37 降到 **35**
   （两条测点各把一行键名写进了断言里），指针可驱动性与状态格子总数不变（仍 176 / 91）。快照文件
   `spike/StateCellCensus/census-2026-09-23.txt` 记的是它当时那一次，不追改。

