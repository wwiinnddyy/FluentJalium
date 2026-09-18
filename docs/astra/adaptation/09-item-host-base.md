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

## 实测的坑：裸 `ItemsControl` 会挂住推帧循环

`new ItemsControl { ItemsSource = ["a","b","c"] }` 直接放宿主里捕获，
UI 线程 60 秒没有回应（fixture 超时），和 `adaptation/07` 记的
"裸 `ScrollBar` 会让推帧循环回不来"是同一族。派生控件（`ListBox`）不受影响。

后果与纪律：
- 测试与 Gallery 里**不要放裸 `ItemsControl`**；要验证条目通路就用派生控件。
- 这也是"框架没有通用主题"的又一次现身：`ItemsControl` 的 `Template` 为 null，
  它没有可呈现的结构，框架代码在这种状态下走进了不产出帧的路径。
  机制未证（没有可读的内部实现），只记现象；不要在产品代码里依赖它。

## 这一项交付什么

- 交付：本文件 + `AstraItemHostTests` 的 3 条断言（容器类型与面板、容器样式通路、
  宿主样式惠及内嵌 ScrollViewer）。
- **不交付**：通用条目样式、`ContentPresenter` 基础样式。上游对
  `ItemsControl`/`ContentPresenter` 没有主题键面（`ContentPresenter` 在 WinUI 里不带样式），
  自造一个只会多出一层没有出处的约定。
- 阶段 5 的输入：每个列表控件的条目样式属于那个控件自己的审计，
  入口是"本地 `ItemContainerStyle` + 我们的宿主样式已在底座就位"。
