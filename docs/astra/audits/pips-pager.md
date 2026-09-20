# PipsPager 审计：一页一个圆点，以及"能看到几个"是谁决定的

阶段 5 收尾批第九段。上游基线 `microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`，
运行时权威 NuGet Jalium.UI **26.10.9**（AGENTS.md 钉住），方法对照 ModernWpf @ `23555a6c0`。
本批原始读数全部在 `docs/astra/adaptation/s1m-pips-pager-raw.txt`（下称 s1m），本文只写结论与出处。

## 0 · 依据

| 上游文件 | blob | 用在哪里 |
| --- | --- | --- |
| `controls/dev/PipsPager/PipsPager_themeresources.xaml` | `0d93a289b39a22b7e859a2ecea964575826e52f7` | `ThemeResources/PipsPager.jalxaml`（29 行）+ `Styles/PipsPager.jalxaml`（6 个样式） |
| `controls/dev/PipsPager/PipsPager.xaml` | `42c1ab313201354cb4d77897acfc36b8b867cc25` | `Styles/PipsPager.jalxaml` 的控件默认样式 |
| `controls/dev/PipsPager/PipsPager.idl` | 公开面 6-88 | `Controls/Navigation/FluentPipsPager.cs` 的属性/事件面 |
| `controls/dev/PipsPager/PipsPager.cpp` | 310-346、274-308、418-447、522-565、161-191 | 页数生成、视口夹紧、钳位与事件、翻页、按键 |
| `controls/dev/PipsPager/PipsPagerAutomationPeer.cpp` | 19-52 | 未照搬，见第 5 节 |
| `controls/dev/PipsPager/Strings/en-us/Resources.resw` | 120-135 | 四处无障碍名（Pager / Previous Page / Next Page / Page {n}） |

同一目录另有 `PipsPager_themeresources_perf2026.xaml`，与本批键面同形换值，处理与 RadioButtons 批一致。

## 1 · 键清单（逐字照抄的结果）

上游这一份共 51 行：**27 行画刷别名**在 ThemeDictionaries 内（Light 5-31 与 Default 34-60 名称与目标**逐行相同**；
HighContrast 63-89 把 27 行全部重指到 `SystemColor*`），字典外 **12 行度量**（10 个 `x:Double` + 2 个 `Thickness`，
:92-99、:104-109）、**4 行 `x:String` 字形** + **2 行 `x:String` 字号**（:100-103、:107-108）、**6 个 Style**（:110-290）。

本批发布：27 行画刷别名（一字不改，全部 `{StaticResource … ResourceKey=…}` 指向调色板实例）+ 2 行 `Thickness`
= **29 行**，落在 `ThemeResources/PipsPager.jalxaml`；6 个 Style 用上游同名键落在 `Styles/PipsPager.jalxaml`
（本层把样式行与令牌行分文件）。

不发布 16 行：10 个 `x:Double` 与 6 个 `x:String` —— 本运行时资源读取器**解析不了这两类行**，而且会连带毁掉整份字典
（adaptation/00 S1-c）。其中 `PipsPagerButtonWidth` 20 / `PipsPagerButtonHeight` 12 上游自己也没读：整棵参考树里
没有任何模板或代码点名它们（grep 只命中 `PipsPager_themeresources*.xaml` 与 TestUI 的控件命名）。这 16 个键名由
`AstraPipsPagerTests.Rows_this_runtime_cannot_carry_stay_out_of_the_public_set` 钉住"不得出现"。

替代位置：四个方向脚印（12/24、24/12）成为 `FluentPipsPager` 里的私有常数并由控件写到自己生成的容器上；
导航按钮 24×24、`CornerRadius`、两个 `Thickness` 留在样式里；两个圆点直径 3.8 / 5.6 是字形 4 / 6 的
0.938 em 墨迹（s1m [B]），作为字面量留在模板并写明来源。

## 2 · 运行时面差异（s1m [A]）

| 上游用法 | 26.10.9 面 | 本批处理 |
| --- | --- | --- |
| `ItemsRepeater` + `StackLayout` + `TemplateSettings.PipsPagerItems`（`IVector<Int32>`，装 1 基页号） | 三者皆无 | 控件自己生成 `Button` 并放进 `#PipsPagerItemsHost`；`TemplateSettings` 不发布——没有可数据绑定的列表宿主可喂 |
| `Selection` 靠容器套哪个 Style 表达 | 一致 | `ApplyPipStyles()` 换 Style，另在 `Tag` 上带"选中"，由模板触发器切两个已画好的圆点 |
| `ScrollViewer` 的 `VerticalScrollMode/HorizontalScrollMode/Is*ScrollChainingEnabled` | **全部不存在** | 只保留 `ScrollBarVisibility="Hidden"` 两个属性；链式滚动这条契约在本运行时没有对象 |
| `ScrollViewer` 尺寸由 `GetDesiredPipSize` 量 realized 容器（cpp:144-159、274-308） | 能量，但**会自锁**：夹紧作用于被量的那次 arrange，一次夹到 12 之后 host 的 SizeChanged 永不再来（s1m [E].4） | 夹紧改用控件写给容器的同一对常数；这条推翻写进 00 文档，防止后来人再量一次 |
| `FontIcon` + `MirroredWhenRightToLeft` | `FontIcon` 存在（`< IconElement`），但字形墨迹到不了任何进程内捕获（s1m [C]） | pip 画成 `Ellipse`，导航按钮画成 `Path`——沿用本层 ComboBox/NumberBox 的既有做法 |
| `UseSystemFocusVisuals` / `FocusVisualMargin`（themeresources :117-118） | **两个成员都不存在** | 不写。本运行时对未知 markup 属性是静默丢弃而非报错，写了只会制造第二条静默账 |
| `AutomationProperties.PositionInSet/SizeOfSet/Name` | 三个附着属性都在 | 逐个写到容器与导航按钮上，名取上游 RESW 英文值 |
| `AutomationProperties.AccessibilityView="Raw"`（字形上） | 无该属性 | 不适用（本批没有字形） |
| `GettingFocus`（cpp:590-611 把 Tab 进入重定向到选中 pip） | 只有 `GotFocus/LostFocus` | 未做重定向，见第 5 节 |
| `FocusManager.TryMoveFocus`（跨整棵 XamlRoot） | `FocusManager` 类型存在，方法面未量 | 方向键改为在相邻 pip 上 `Focus()`；两端不越界时交给框架遍历 |

`WrapMode`（idl:83-88）在 `MUX_PUBLIC_V7` 段内，不在本次钉住的公开面里，**不实现**：`IsPreviousButtonEnabled`
一类属性上游也没有——禁用是视觉状态写 `IsEnabled=False`（PipsPager.xaml:30-36、51-57），本批照抄这条：由控件按
上游同一条门式算式写按钮的 `IsEnabled`。

## 3 · VisualState → 触发器映射

| 上游组 / 状态 | 上游写什么 | 本批落点 |
| --- | --- | --- |
| 控件 `PreviousPageButtonVisibilityStates`：Visible / Hidden / Collapsed | Opacity=0 / Visibility=Collapsed | 控件模板 `Trigger PreviousButtonVisibility=VisibleOnPointerOver → Opacity 0`；`=Collapsed → Visibility Collapsed` |
| 控件 `NextPageButtonVisibilityStates` | 同上 | 同上，另一侧 |
| 控件 `PreviousPageButtonIsEnabledStates` / `NextPageButtonIsEnabledStates` | `IsEnabled=False` | 代码 `UpdateNavigationButtons()`，门式与 cpp:193-238 同：`!在端点 && pages != 0 && MaxVisiblePips > 0` |
| 控件 `RootPanelOrientationStates`：Horizontal 旋转 -90 | 两个导航按钮 `RenderTransform` | 模板 `Trigger Orientation=Horizontal`：previous `-90`，next `+90`（一个已画的上箭头要分别指左、指右）；竖排时触发器撤销，恢复部件自带的 `ScaleY=-1` |
| pip `CommonStates`：Normal / PointerOver / Pressed / Disabled | 三个画刷 + 字号 6/4 互换 | pip 模板触发器：三个画刷行照抄；字号互换改为切 `NormalDot`/`SelectedDot` 的 `Visibility` |
| pip `OrientationStates`：Vertical → RootGrid 24×12 | 容器脚印 | 控件写给容器（`ApplyPipFootprint`），因为条目面板取不到方向状态 |
| 导航按钮 `CommonStates` 的 Pressed | 只有 0.875 缩放（四行画刷全是透明） | 模板触发器写 `ScaleHost.RenderTransform`，值即上游 0.875 |
| 上游 pip 的选中差异 = 字号 6 vs 4 | 同一枚字形 | 两颗已画圆点 5.6 vs 3.8（0.938 em 墨迹），由 `Tag` 触发器切换 |

## 4 · 四类证据（分开记）

- **构建**：`tools/Test-AstraGates.ps1` 串行两遍同一棵树——第一遍 exit 1（**1166 通过 / 1 失败**，失败的是
  `AstraDataGridTests.The_grid_surface_paints_its_background_row` 的"capture never settled"，调色板步骤因此没跑；
  该类单跑 56/56、与两个洋红哨兵类同跑 159/159 都绿，归到任务 #35 那条序列 flake，不当成"已验"）、
  第二遍 exit 0（Debug **0 警告 / 0 错误**，**1167/1167 通过、0 skip**，5 m 35 s，比上段 1137 多 30 条 ＝ 新测 29 +
  资源键闸口那份新字典行 1；调色板三行 `checked=True`）——原始读数在 s1m [F]；
  `src/FluentJalium` 与 Gallery 工程真重编 0 警告 0 错误。
- **行为**：`tests/FluentJalium.Tests/AstraPipsPagerTests.cs` 13 条事实 + 16 条形参 = 29 条，0 失败。覆盖：部件名齐
  （`RootPanel`/`PreviousPageButton`/`PipsPagerScrollViewer`/`PipsPagerItemsHost`/`NextPageButton`）、页数=容器数、
  脚印真落到生成的 `Button`（12×24，禁 MinHeight 会成 12×32）、Invoke 一个 pip 即选中并换 Style、越界钳位只发一次事件
  （含"落回原值也要发一次"这条上游语义）、`NumberOfPages=-1` 只增不减、0 页清空并禁用两侧、翻页一次一步并在两端自禁、
  `MaxVisiblePips` 夹紧 = 12×k（10 页 5 窗 → 60）、方向切换同时换脚印与箭头指向、三种可见性各自占位或让位、
  两颗圆点的可见性与画刷身份、公开/不公开键闸门。
- **视觉**：一条像素断言。先量到"宿主窗口底色是黑、Light 的 pip 令牌是 37% 黑"，于是 `PixelKey(令牌)` 把整张捕获
  都数进去了（9600 像素的主题数出 105966），且加一个 pip 零变化；改到白底上后同一个差分为 19 像素，再加一个 pip 又增长，
  且"选中那颗"的墨迹严格大于"普通那颗"。品牌绿 `#207245` 在直方图里不存在；Light↔Dark 的令牌实例不同色且各自上屏。
- **硬件输入**：**零**。指针与键盘都没有通路（任务 #13）。因此 hover 放大、按下 0.875 缩放、`VisibleOnPointerOver`
  的揭示臂、方向键移焦点、Enter/Space 激活全部只有代码/模板证据，没有输入证据。

## 5 · Known Gaps（不许用相邻证据替代）

1. 无真指针/键盘输入：hover 与 pressed 两支触发器、`VisibleOnPointerOver` 的揭示臂、方向键移焦、Enter/Space 选中
   都没有驱动。
2. 无障碍：上游 peer 暴露 `Selection` 图案且控件类型是 **Menu**（`PipsPagerAutomationPeer.cpp:49-52`）；本批不造
   peer，pip 只带 Name/PositionInSet/SizeOfSet，可点性靠 `Button` 自带的 Invoke 图案。
3. 无"滚回可见区"：上游把选中 pip 居中进视口（cpp:240-272）。本运行时没有量过的偏移 API，夹出窗口外的 pip 就留在外面。
4. `WrapMode` 不实现（V7 面）；因此端点禁用规则里"环绕时不禁用"那半条不存在。
5. 字形通路整体不可断言：EA3B/EDDB/EDDC 在 cmap 里都是真命中且布局框正确，但墨迹到不了捕获。像素层面的"字形是否
   上屏"只能用截图人眼判，本批不主张。
6. 高对比：27 行在上游 HC 段全量重指到 `SystemColor*`；本层走调色板别名链，HC 下这些令牌被 `HighContrast.map`
   重指到的系统色与上游逐行不同（例：pip 前景上游 HC 用 `SystemColorButtonTextColor`，本层
   `ControlStrongFillColorDefaultBrush` → `SystemColorButtonFaceColor`）。
7. `FocusVisualMargin` / `UseSystemFocusVisuals` / `GettingFocus` 三个成员本运行时没有，相应细节无处落。
8. Tab 进入不重定向到选中 pip。
9. 无 RTL：上游只有 `MirroredWhenRightToLeft`（字形上）与模板旋转，本批的旋转与上游同样不看方向。

## 6 · 不声称

不声称 WinUI 逐像素一致（pip 是画出来的圆点，箭头是 Path；两者尺寸来自量出的墨迹盒而非字形排版）；
不声称触摸/键盘路径已验证；不声称高对比与上游一致；不声称 `MaxVisiblePips` 的滚动行为一致（只有夹紧一致）；
不声称 `TemplateSettings`、`WrapMode`、自动化图案存在。

## 7 · 自有类型判定

运行时不导出 `PipsPager`（s1m [A]），也不导出它的模板赖以成立的 `ItemsRepeater`/`StackLayout`，
因此这不是"能不能换模板"的问题而是"有没有可换的控件"。判定：起自有类型 `FluentPipsPager`，
基类取 `Control`（与上游同），**不**继承 `ItemsControl`——上游公开面里没有 `ItemsSource`/`ItemContainerStyle`，
继承会把它们加进公开 API，而这里需要的只是"按页数生成 N 个按钮"。这一点与上一段 RadioButtons 的结论不冲突：
那边条目本身就是数据，继承条目管线是省事；这边没有数据。
