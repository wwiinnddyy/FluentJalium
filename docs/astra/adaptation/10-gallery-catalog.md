# 10 · Gallery 目录闸口（E1）

对应 `ROADMAP.md` E 段第一条：**反射枚举被我们赋了隐式样式的 `TargetType`，与目录求差集，差集非空即失败**。
这一条之所以是整套架构的必需件而不是"顺手做个演示页"：Astra 没有 Generic 主题，
漏样式的控件不会报错，只会静默露出框架外观 —— 目视能看到的差异，就必须有一条断言兜着。

落地位置：

| 角色 | 文件 |
|---|---|
| 目录（唯一真值源） | `samples/FluentJalium.Gallery/Catalog.json` |
| Gallery 侧读取器 | `samples/FluentJalium.Gallery/GalleryCatalog.cs` |
| 闸口（另一套独立解析器） | `tests/FluentJalium.Tests/Gallery/AstraGalleryCatalogTests.cs` |

两套解析器是故意的：共用一个模型会让"目录"和"闸口"朝同一个方向一起漂移，
而这条闸口的全部价值就在于两个读者必须对同一份文件达成一致。

## 实测的隐式样式宇宙是 17 个，不是我数出来的 16 个

按目录翻 `src/FluentJalium/Styles/*.jalxaml`，顶层无 `x:Key` 的 `<Style TargetType=…>` 有 16 条。
闸口第一次跑就报 `17 restyled, 16 catalogued` —— 第 17 条在**调色板目录**里：

```
ThemeResources/Typography.jalxaml:25  <Style TargetType="TextBlock" BasedOn="{StaticResource BodyTextBlockStyle}" />
```

这正是这条闸口要抓的东西：一个全局隐式样式写在资源字典而不是样式字典里，
按目录结构去数就会漏。它现在在目录里有自己的条目、自己的 parity 档、自己的三条欠账。

17 个的分布（parity 三档的含义见下一节）：

| parity | 数量 | 成员 |
|---|---|---|
| `audited` | 6 | Button、ToggleButton、RepeatButton、HyperlinkButton、ScrollViewer、ToolTip |
| `ported` | 8 | TextBlock、TextBox、PasswordBox、Slider、CheckBox、RadioButton、ComboBox、ComboBoxItem |
| `own-type` | 3 | FluentToggleSwitch、FluentNavigationView、FluentNavigationItem |

## 目录的三条不变量

1. **差集为空**（双向）：`Styles/` + `ThemeResources/` 里每一条顶层隐式样式都必须在目录里，
   目录里每一条都必须能在产物字典里找到；数量再叠一道相等断言，防同名重复。
2. **名字必须是真类型**：目录里的 `markup` 要么是不带点的裸名（默认命名空间），
   要么是 `clr-namespace` 解析后的全名；每条都必须在已加载程序集里命中**恰好一个**
   `FrameworkElement` 子类。写错一个字母、或者样式改名而目录没跟上，都会在这里断。
3. **每条声明都要指着存在的证据**：`evidence` 里的路径必须真实存在；
   自称 `audited` 的还必须同时引一条 `docs/astra/audits/` 和一条 `tests/`。
   这一条把"parity 状态"从形容词变成可核对的引用。

## 反向那一半：Gallery 自己不许用没样式的控件

差集只覆盖"我们改了外观的东西"，它看不见"我们根本没改的东西"。
所以另加一条：解析 `MainWindow.jalxaml` 的每个元素，能解析成 `Control` 的，
要么在目录里，要么在豁免表里 —— 豁免必须写理由，理由是"哪一批欠它"：

| 豁免 | 理由 |
|---|---|
| `Jalium.UI.Window` | 外壳从来不走样式：靠标题栏钩子与背衬选择，见 `audits/window-shell.md` |
| `Jalium.UI.Controls.ListBox` | 列表批欠：宿主与容器同批改；容器通路已在 `adaptation/09` 量过 |
| `Jalium.UI.Controls.ListBoxItem` | 列表批欠：上游 ListItem 键清单尚未逐字转录 |

也就是说：Gallery 现在页面上那个 ListBox 是**框架外观**，这不是我漏了，是它被显式记成欠账。

## 一条框架事实（省掉一次反射）

`XDocument` 读 JALXAML 时，`xmlns:fluent="clr-namespace:FluentJalium.Controls;assembly=FluentJalium"`
这种声明里的 `clr-namespace:` 串会**原样成为元素的 `NamespaceName`**（它不是被映射过的 URI）。
所以"前缀 → CLR 命名空间"直接从文档里读即可，不需要反射 `XmlnsDefinition`，
也不需要把 `fluent:FluentNavigationView` 与 `Button` 用两套规则处理。
闸口里 `Jalium.UI.Window` 被识别出来、`FluentJalium.Controls.FluentSettingsRow` 被正确判为
`Panel`（因此不算控件），都走的是这条路。

## Gallery 运行时确实读这份文件（冒烟）

`Catalog.json` 以 `CopyToOutputDirectory=PreserveNewest` 随 Gallery 输出走，
`MainWindow` 在每次导航时把当前页的 parity 行与"未声称"行填到底部条上。截图上那两行是：

```
Overview · ScrollViewer audited, TextBlock ported, ToolTip audited · 3 of 17 restyled types
Not claimed: drag, inertia and touch panning have no evidence · ScrollBar restyling is limited to the
two generated-brush hooks · the size ramp is not transcribed key for key against upstream · font
fallback is the operating system's, not measured · text colour never reaches the capture path ·
show delay, auto-hide and placement are framework behavior, unmeasured
```

这段文案只有从 JSON 解析出来才可能长这样，所以"运行时读取通路成立"是**看得到**的结论；
进程 `CloseMainWindow` 后 `code=0` 退出、无残留进程。闸口另有一条断言：输出目录里那份
`Catalog.json` 必须与被闸口核对的那份逐字节相同（防"改了源、跑的是旧的"）。

## 闸口现状

`tools/Test-AstraGates.ps1` 串行全绿：restore → build 0 警告 → **60 通过 / 0 跳过 / 0 失败**（本轮 +6）
→ 调色板漂移 checked=True（102 刷）。

## 这些证据不支持什么

1. **不支持"每一页都画对了"（仍未结，2026-09-22 试过并把结果记在 `ROADMAP.md` 的"#10 缺口②"）**。冒烟只看到
   Overview 一页，新加的家族卡与内嵌表面卡既没目视也没有断言。当天确实把逐页像素闸建起来了并且单独 13/13 绿
   （`Catalog.json` 的 13 页逐页在真 `MainWindow` 里挂载、Light/Dark 各拍一张，再卸载该页拍一张同窗口底板，
   断言"挂页的颜色数严格多于不挂页"），但它一进顺序全量就把 `AstraAutoSuggestBoxTests` 的三条建议列表测点弄红，
   机制没查到，于是整条闸被撤出工作树（实现留在 `spike/GalleryRender/AstraGalleryRenderTests.cs.parked`）。
   所以"每页渲染到像素"至今**没有断言**，只有那份记录在案的 52 帧测量。
2. **屏幕坐标点击不可用，且有风险**。本轮一次冒烟脚本想用 `SetCursorPos`+`mouse_event`
   点"Open buttons"，但 `BringWindowToTop`/`SetForegroundWindow` 在 Windows 前景锁下不保证生效，
   截图证明那个矩形里当时盖着用户另一个窗口 —— 点击落到了别人的应用上。
   结论：**Gallery 冒烟只做"启动 → 截图 → 关窗"，一律不做坐标注入**；
   输入类证据只能在进程内做（`spike/PointerProbe` 那类），`press` 模式仍未经同意不跑。
3. **不支持"目录里的 gaps 是全集"**。gaps 是我手写的当前欠账快照，闸口只验证它存在、
   不验证它完整。控件推进时 gaps 要人手动减项，这是这套机制的已知弱点。
4. **顺带一条未定位的观察**：把窗口最大化后，导航窗格仍是图标态（`FluentNavigationView`
   的自适应只在 `SizeChanged` 里跑，而 `AdaptPane` 第一次进来时 `_lastNarrow` 为空且
   `IsPaneOpen` 无本地值 → 不应用自动态）。这条**只有一张截图，没有断言，不当结论**，
   归到导航批（D 段）里查。

## 这份文件里的"17"是当天的实测值，不是现值（2026-09-22 标注）

第 18 行那句"隐式样式宇宙是 17 个"是闸口**第一次跑那天**数出来的（当天还报过 `17 restyled, 16 catalogued`，
第 21 行），第 78 行的 "3 of 17 restyled types" 是 parity 条在**那一天**的读数。两者都不是今天的数：今天
`Catalog.json` 是 **13 页 / 60 条控件行**（2026-09-22 数出来的：`pages` 数组 13 项，含新加的 Tokens、Materials、
Motion；`controls` 数组 60 项），隐式样式类型数随每批补样式一直在长。

隐式样式类型数**故意不往这份文档里抄**：闸口
`AstraGalleryCatalogTests.The_catalog_covers_every_type_astra_restyles_implicitly` 每次都从产品字典现算并做双向差集，
抄一个常数到这里只会再次变成一条看起来权威的过期数字。要当下的值就跑那条测点，或看它的失败消息。
