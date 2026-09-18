# FluentJalium

WinUI 3 风格的 Fluent 主题与控件库，构建在 **Jalium.UI** 之上。

这不是 WPF、不是 WinUI、不是 Avalonia。运行时对象模型沿用 WPF 血统（`DependencyObject`、依赖属性、
可视树、路由事件、数据绑定），标记语言是 JALXAML（`.jalxaml`），渲染走 GPU 后端。

## 当前实现：Astra

`Astra` 是对旧 FW 外观层的**破坏性替换**，这个名字只指这次重写，不是目录。产品代码全部在
`src/FluentJalium/` 下，按 ModernWpf 的两层分工摆放；其中不再有退役的 FW 类型，
也不再有伪装成 WinUI 的 `x:` 命名空间/`x:Bind` 兼容解析层。

| 组成 | 位置 | 作用 |
|---|---|---|
| 主题门面 | `Themes/FluentThemeManager.cs` | `Apply` / `ApplyTheme` / `ApplyAccent` / `OverrideBrush` / `ReduceMotion` |
| 调色板 | `ThemeResources/Light.jalxaml`、`Dark.jalxaml` | 由 `tools/Sync-AstraPalette.ps1` 从上游生成，键名逐字照抄 WinUI |
| 度量与排印 | `ThemeResources/Metrics.jalxaml`、`Typography.jalxaml` | 圆角、厚度、`Type*` 文本样式 |
| 控件样式 | `Styles/*.jalxaml` | 样式与 `ControlTemplate`（按 ModernWpf 的 `Styles/` 一层放，暂不拆成一控件一文件） |
| 加载清单 | `Themes/Manifest.txt` | 样式字典的依赖顺序，唯一 authority |
| 自有类型 | `Controls/**`、`Motion/**` | 仅用于已证明的框架行为缺口 |

## 启动顺序

`ThemeLoader.Initialize()` 必须在任何 JALXAML 解析之前调用；一个进程只能有一个 `Application`；
入口方法需要 `[STAThread]`。

```csharp
RenderContext.GetOrCreateCurrent(RenderBackendType.Auto);
Jalium.UI.Rendering.RenderingEngine.DefaultRenderingEngine = Backend.Impeller;
ThemeLoader.Initialize();
var application = new Application();
FluentThemeManager.Apply(application);            // 装载 Astra 字典并确定深浅
var window = new MainWindow();
application.Run(window);
```

## 构建与验证

串行构建；并行构建会锁住共享的 `obj` 目录。

```powershell
dotnet restore FluentJalium.slnx
dotnet build FluentJalium.slnx --configuration Release --no-restore
dotnet test  tests/FluentJalium.Tests --configuration Release --no-build
dotnet run --project samples/FluentJalium.Gallery

powershell -File tools/Sync-AstraPalette.ps1 -Check   # 调色板与上游 pin 的漂移闸口
```

若构建报 MSB3021/MSB3027「文件正被另一进程使用」，那不是编译错误：先结束残留的
`testhost` 与 `FluentJalium.Gallery` 进程。

## 上游 authority

- 行为与外观基准：`microsoft-ui-xaml` commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。
- 适配方法参考：`ModernWpf` commit `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`（`v1.0.0-rc.1`）。
- 运行时基准是 NuGet `Jalium.UI` **26.10.9**；同级的源码树只是参考，可能更新。

逐控件的上游文件证据、适配偏差与已知缺口记录在 `docs/astra/`。署名见 `THIRD-PARTY-NOTICES.md`。

License: MIT
