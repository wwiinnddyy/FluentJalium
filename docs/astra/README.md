# Astra 文档索引

本目录记录 Astra 实现相对上游与宿主的偏差。约定来自 `AGENTS.md`：资源与公开 API 变更、
上游文件证据、适配手法都要在这里留痕，并且构建/行为/视觉/硬件输入四类证据分开记录。

| 位置 | 内容 | 状态 |
|---|---|---|
| `adaptation/` | Jalium 宿主能力与偏差 | `00-jalium-theme-capabilities.md`（主题/令牌/VSM/加载路径实测）、`01-jalium-control-census.md`（163 控件普查），含两份原始输出 |
| `audits/` | 逐控件的 WinUI 3 源码审计（上游文件 + commit + blob） | 待阶段 2 起逐控件填写 |
| `resources/` | 公开资源键清单 | 待阶段 3 |
| `testing/` | 证据链定义与人工核对清单 | 待阶段 2 |

## 阶段 0：让树可构建、可运行

### 加载清单成为唯一 authority

`FluentThemeManager` 原先硬编码一份字典数组，与磁盘上的文件没有任何一致性约束，
因此里面长期挂着两个**根本不存在**的条目（`Controls/Collections.jalxaml`、
`Controls/Feedback.jalxaml`），令每个宿主在 `Apply()` 时抛 `Missing Astra resource` 直接崩溃。

现在顺序与成员来自内嵌资源 `Resources/Manifest.txt`（一行一项，`#` 起注释），并且加载时
双向校验：

- 清单指向不存在的内嵌资源 → 抛错并给出行号；
- 存在却没被清单登记的内嵌字典 → 同样抛错（否则它会静静地永不加载，比崩溃更难发现）；
- 同一项重复登记 → 抛错。

校验放在运行时而不是脚本里，是因为脚本可以被忘记，而这个 bug 正是被忘记的校验养出来的。
等价断言同时存在于 `tests/FluentJalium.Tests/AstraGateTests.cs`。

**公开 API 变更**：`FluentThemeManager.DictionaryNames` 由固定数组改为按清单惰性解析
（类型与语义不变，成员集合可能抛 `InvalidOperationException`）。

### 内嵌资源名分隔符缺陷

csproj 的 `LogicalName` 用 `%(RecursiveDir)` 拼接，而 MSBuild 在 Windows 上给出的
`RecursiveDir` 带**反斜杠**，于是子目录字典实际内嵌为 `Resources/Controls\Common.jalxaml`。
原先的 `Load()` 把查找键规整成正斜杠，两边永远对不上——**这意味着 `Controls/` 下的五个
字典在此之前从未被成功加载过**，只是崩溃点更靠前所以没人发现。

现在所有内嵌资源查找统一走 `OpenResource`，按正斜杠键查一张预先构建的名字表，
分隔符不再可能在调用点出错。

### 构建与打包恢复

- `FluentJalium.slnx` 引用的 `tests/FluentJalium.Tests` 文件已被前一次重构删除，
  解决方案整体无法构建。现已重建最小测试工程（只引用库；不再引用已退役的
  `FluentJalium.WinUI` 外观层，也不引用 WinExe 的 Gallery）。
- csproj 引用了不存在的根 `README.md` 与 `THIRD-PARTY-NOTICES.md`，`dotnet pack` 失败。
  两份文件已补齐，署名覆盖 WinUI@`19e3bdc3c`、ModernWpf@`23555a6c`、Fluent System Icons。
  `FluentJalium.0.2.0-astra.1.nupkg` 可正常产出。
- `tools/Test-AstraGates.ps1` 是**编排器不是断言处**：清残留进程 → restore → 串行 build →
  结构闸口测试 → 调色板漂移 `-Check`。顺序按最快失败排列。

### 证据

| 类别 | 结果 |
|---|---|
| 构建 | `dotnet build FluentJalium.slnx` 三项目 0 警告 0 错误；`dotnet pack -c Release` 成功 |
| 闸口 | `AstraGateTests` 3 通过 0 失败；`Sync-AstraPalette.ps1 -Check` 绿（Light/Dark 各 83 源色 → 101 刷子） |
| 行为 | Gallery 启动、窗口存在 12 秒无异常、`CloseMainWindow()` 优雅退出、stderr 为空 |
| 视觉 | **未核对**。本次改动只恢复加载，未调整任何视觉值；具体渲染正确性待阶段 2 逐项验证 |
| 硬件输入 | **未核对** |
