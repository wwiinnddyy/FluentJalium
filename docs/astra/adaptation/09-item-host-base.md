# 09 条目宿主基面：容器是谁生成的、样式能不能落到它身上

底座批（ROADMAP D1）里"条目容器"这一项的开工前摸底。范围只到**宿主与容器基面**，
不含任何具体列表控件的样式——那些属于阶段 5。

- 运行时权威：NuGet Jalium.UI **26.10.9**（AGENTS.md 钉住）。
- 上游对照：`microsoft-ui-xaml` @ `19e3bdc3c`。
- 判据来源：`adaptation/06`（像素归因）+ `audits/scrollviewer.md`（宿主能力边界）。
- 证据落在 `tests/FluentJalium.Tests/AstraItemHostTests.cs`（3 条，全绿）。

## 实测：ListBox 的部件树

把一个 `ListBox{ItemsSource=3 项}` 放进已上屏宿主、推帧、走可视树：

```
Grid
└─ Border
   └─ ScrollViewer
      ├─ ItemsPresenter
      │  └─ VirtualizingStackPanel
      │     ├─ ListBoxItem
      │     ├─ ListBoxItem
      │     └─ ListBoxItem
      ├─ ScrollBar  (RepeatButton / Track>Thumb / RepeatButton …)
      └─ ScrollBar
```

三点结论：

1. **容器是每种控件自己的 CLR 类型**（这里是 `ListBoxItem`）。框架**没有**一个共用的
   "`ItemContainer` 基样式"可以让我们在一次底座工作里全部搞定——
   上游 WinUI 也不是那样组织的（`ListViewItem`、`SettingsStoreItem`、`NavigationViewItem`…
   各自一份 `*_themeresources.xaml`）。所以底座批这一项的**正确交付是摸底 + 打通通路**，
   不是产出一份通用条目样式。
2. **默认面板是 `VirtualizingStackPanel`**，且经由 `ItemsPresenter` 挂进来，
   与上游 ListBox 的默认 `ItemsPanel` 一致——阶段 5 不用为"要不要虚拟化"做架构决定。
3. **列表自己内嵌一个 `ScrollViewer`**，而它吃的是我们的宿主隐式样式：
   实测 `IsTabStop` 从框架默认的 `True` 变成我们设的 `False`，
   `VerticalScrollBarVisibility` 保持 `Auto`（我们**故意不抄**上游的 `Visible`，理由见
   `audits/scrollviewer.md`）。也就是说底座批那一项会自动惠及所有列表，不需要逐控件重复。

## 实测：容器样式能落上

`list.ItemContainerStyle = new Style(typeof(ListBoxItem)) { Padding = 11 }`（**本地赋值**）
之后，建出来的第一个 `ListBoxItem.Padding` 读回就是 `11`。
这条路是阶段 5 每个列表样式的入口，因此它必须有断言而不是靠"应该可以"。

约束（同轮量的，见 `audits/scrollviewer.md`）：**不要靠
`Application.Resources[typeof(ListBoxItem)] = style` 这条路做验证**。
往应用级字典增删条目会让框架重新解析 `{ThemeResource}`，从此不指向我们调色板那些实例，
同一次运行里后面的像素覆盖断言会无声失败。底座测试一律走本地赋值。

## 撤回：所谓"裸 `ItemsControl` 会挂住推帧循环"

本节原来写着：`new ItemsControl { ItemsSource = [...] }` 上屏捕获会让 UI 线程 60 秒不回应，
并把机制读成"`ItemsControl` 的 `Template` 为 null，框架在这种状态下走进了不产出帧的路径"，
进而立了一条纪律——自造条目宿主不要继承 `ItemsControl`。**那条机制解释是错的，纪律也没有依据。**
`spike/ItemHostProbe`（mode `mount`/`capture`，2026-09-20）在同一台机器、同一运行时、
同一个已上屏窗口里量到：

- **无模板的 `ItemsControl` 照样生成容器**：挂载 4 帧后读到的树是
  `ItemsControl > StackPanel > ContentPresenter×3 > TextBlock×3`，尺寸 `420x19.78` 每项。
  框架有一条回退条目宿主（属性面里读得到 `UsesFallbackItemsHost`（protected virtual）与
  `ItemsHostInternal`），`Template` 为 null 不等于"没有可呈现的结构"。
- **派生类型也走同一条路**：探针里的 `ProbeItemsControl : ItemsControl` 什么都没设，
  同样生成 3 个 `ContentPresenter`。也就是说隐式样式按精确类型查（派生类拿不到 `ItemsControl` 的行），
  但条目管线照常工作。
- **推帧循环没有挂**：同一次运行里挂载 5 就是当年那个形状（裸 `ItemsControl`、无模板、无样式），
  4 帧返回，`420x140`。
- **捕获也不是成本**：`RenderTargetBitmap.Render` 计时——`ListBox` 0.03s、裸 `ItemsControl` 0.00s。

还活着的解释在 `tests/FluentJalium.Tests/Pixel/PixelHarness.cs:259-268` 自己记着：
静态场景本来就不发 `CompositionTarget.Rendering`，而当年的看门狗把释放操作排到了线程池的
dispatcher 上（没人泵它），于是"等帧"变成死等——**这是 harness 的 bug，不是 `ItemsControl` 的性质**。
那条评论还举了同族的例子（焦点切到 Slider 也是 60 秒超时），修看门狗之后都返回了。
本轮没有去复现当年的 60 秒（修复已落地，复现它需要故意把看门狗写坏），
所以这句话的强度是：机制断言被三处读数推翻，残余现象归因到有文档的 harness 缺陷。

## 实测：条目管线能给我们的自造控件用

同一轮量的，直接影响阶段 5 收尾批（GridView 判断 + BreadcrumbBar/RadioButtons/PipsPager）的选型：

| 读数 | 结果 |
| --- | --- |
| 容器生成的覆写面 | `GetContainerForItemOverride()`、`IsItemItsOwnContainerOverride(Object)`、`PrepareContainerForItemOverride(DependencyObject, Object)`、`ClearContainerForItemOverride(...)`、`GetContainerForItem(Object)` **全是 protected/virtual**（签名与 WPF 一致），我们程序集里的派生类可以直接 override |
| 能否换我们的模板 | 能。把 `Border > ItemsPresenter` 的 `ControlTemplate` 以本地值或 `Style` 的 `Template` setter 交给 `ItemsControl`，两条路都跑通：树变成 `Border #ProbeHostRoot > ItemsPresenter #ProbeItems > StackPanel > ContentPresenter×3`，我们造的部件在树里 |
| 能否换条目面板 | 能。`ListBox.ItemsPanel` 赋 `WrapPanel` → 条目按内容宽换行（每项 `68.36x40.78`）；赋 `UniformGrid Columns=4` → 每项正好 `105x80`（420/4、160/2）；赋横向 `StackPanel` → `68.36x160` |
| `GridView` 到底是什么 | `Jalium.UI.Controls.GridView : ViewBase`——**不是控件**，是 WPF 那套 `ListView.View` 装饰器，同族有 `GridViewColumn`/`GridViewColumnHeader`/`GridViewRowPresenter(Base)`，14 个含 "GridView" 的导出类型里没有 `GridViewRow`/`GridViewItem`。WinUI 的 `GridView` 是换行条目宿主，运行时没有对应类型 |
| 阶段 5 收尾要的三个名字 | `BreadcrumbBar`、`PipsPager`、`RadioButtons` 在 26.10.9 里 0 命中（`ItemsRepeater`、`CardAction` 同样 0 命中） |

后果：自造条目宿主**可以**继承 `ItemsControl`（拿容器生成、`ItemTemplate`、`ItemContainerStyle`、
面板替换这套现成管线），不必再手写"把子元素塞进命名面板"。哪些控件真走这条路，等
`audits/` 里那三个控件的上游基类型量完再定，不在这节里预先下结论。

## 这一项交付什么

- 交付：本文件 + `AstraItemHostTests` 的 3 条断言（容器类型与面板、容器样式通路、
  宿主样式惠及内嵌 ScrollViewer）。
- **不交付**：通用条目样式、`ContentPresenter` 基础样式。上游对
  `ItemsControl`/`ContentPresenter` 没有主题键面（`ContentPresenter` 在 WinUI 里不带样式），
  自造一个只会多出一层没有出处的约定。
- 阶段 5 的输入：每个列表控件的条目样式属于那个控件自己的审计，
  入口是"本地 `ItemContainerStyle` + 我们的宿主样式已在底座就位"。
- 2026-09-20 修订：撤回了"不要继承 `ItemsControl`"这条纪律，原始读数与表格在
  `spike/ItemHostProbe`（`itemhost-probe-all.txt` / `itemhost-probe-capture.txt`），
  转录在 `adaptation/s1l-itemhost-raw.txt`。

