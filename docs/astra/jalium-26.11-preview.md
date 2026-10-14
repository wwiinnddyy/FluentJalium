# Jalium 26.11.0 预览跟进

## 发包状态与升级

核查日期：2026-10-05。这里的“11”指 Jalium **26.11** 版本线，目标框架仍是 .NET 10。

上游 [PR #174](https://github.com/VeryJokerJal/Jalium.UI/pull/174) 已在
2026-10-04 合并；预览分支提交为 `58b5ec18561f3bbfc5661e74d91407acb7987feb`，
合并提交为 `660e7f6088e7cdfbb776c018eb42cedcc0640e7c`。
分支的 `Directory.Build.props` 已写入 `26.11.0`，但这不代表已发布同名 NuGet 包。

核查时 NuGet 包内容索引（包含 Preview 和 unlisted 版本）与 GitHub Releases 的最新包仍为
`26.10.9`，没有可还原的 `26.11.*`。因此 FluentJalium 当前运行时 pin 保持不变。

```powershell
# 只查询：包含 Preview，取所有实际引用包都已发布的最高共同版本。
powershell -File tools/Update-JaliumPackages.ps1 -Check

# 发包后升级：先用版本覆盖参数还原、串行 Release 构建，成功后才写回 pin。
powershell -File tools/Update-JaliumPackages.ps1
```

默认跟进 `26.11.*`，包括预览版。脚本按 NuGet 版本规则排序，不使用字符串排序；
没有共同版本时不修改文件。产品、Localization、Gallery、Launcher 和工具继续共享
`Directory.Build.props` 的 `JaliumPackageVersion`。`spike` 中显式固定版本的历史探针保留
原始测量基准。升级完成后应运行现有 Astra 行为、像素和宿主检查，并重新核查
原先针对 `26.10.9` 的行为缺口与 Gallery 文案。

## CSS 支持

以下结论来自上述固定提交的源码；本项目尚未对 26.11 的运行时做验证。

| 能力 | 已有源码入口 |
|---|---|
| 内联声明、class、子树样式表 | `Css.Style`、`Css.Class`、`Css.StyleSheet`、`Css.StyleSheets` |
| 应用级样式表 | `Application.StyleSheets` |
| 选择器与层叠 | 类型/class/ID、组合关系、伪类、`:is()` / `:where()` / `:not()` / `:has()`、`nth-*`；支持层叠层与作用域 |
| 条件与资源规则 | `@media`、`@supports`、`@container`、`@scope`、`@layer`、`@import`、`@font-face` |
| 变量和值计算 | 自定义属性、`var()`、`@property`、长度/颜色/数学表达式；新增 `random()`、`random-item()`、`progress()`、`calc-mix()`、`first-valid()`、兄弟元素计数函数 |
| 布局 | `FlexPanel` 与 CSS Flex；CSS Grid、流布局、绝对定位等实现 |
| 绘制与排印 | 多层背景、渐变、椭圆圆角、边框、outline、文本装饰/阴影/间距/换行等 |
| 动画 | transitions、`@keyframes`、`@starting-style` |
| 构建 | `JaliumCss` 自动收集 `.css`；静态 CSS 与 JALXAML 内联 CSS 可生成 C#，并报告 CSS 编译诊断 |

例如，预览 API 可以把内联 CSS 声明赋给原生元素：

```csharp
using Jalium.UI.Styling;

Css.SetClass(button, "primary");
Css.SetStyle(button, "background-color: #0078d4; padding: 8px 16px;");
```

CSS 引擎将声明映射到 Jalium 的依赖属性、布局与绘制体系；页面级 CSS 不匹配控件模板的内部部件，
`IncludeTemplateParts` 在这一版仍是保留开关。属性表区分支持、未知、不支持以及目标元素缺少属性，
并有诊断与有损转换提示。因此不能把“支持 CSS 语法”解读为完整浏览器兼容，也不能据此认定
FluentJalium 的 `ControlTemplate` 可以直接由 CSS 替代。

## 其他变化

PR #174 还包括控件与输入、滚动和主题回归修复，文本排版增强、SVG 文本与裁剪能力，
渲染/媒体/平台集成修改，以及构建和源生成工具调整。新增 Apple 构建工作流、平台目标与
Metal 代码，但该提交的 Apple CI 失败，不能据此声称 macOS/iOS 已可稳定发布。

上游 PR 的验证记录仅声明 Core 构建成功（8 个警告）和 348 个本地 CSS 回归用例通过，
其中 31 个随机值用例；这些测试文件未包含在 PR 中。Linux 分支 CI 成功不等于
FluentJalium 的 Windows 行为与外观已经验证。

## 本次验证

- 当前 `26.10.9`：解决方案 Release 串行构建、产品库 `net10.0-windows` Release 构建通过，
  均为 0 个警告、0 个错误。
- 升级脚本：真实源查询 `26.10` 返回 `26.10.9`；查询/尝试升级 `26.11` 正确报告尚未发布，
  并通过文件哈希确认版本配置和两份根文档未被修改。
- 模拟源检查：`preview.10` 排在 `preview.9` 之后，正式版排在同版本预览版之后。
- 没有 26.11 包，尚未执行 26.11 的还原、构建、行为、像素或硬件输入验证。

## 来源

- [NuGet Jalium.UI 版本索引](https://api.nuget.org/v3-flatcontainer/jalium.ui/index.json)
- [GitHub Releases](https://github.com/VeryJokerJal/Jalium.UI/releases)
- [预览 PR #174](https://github.com/VeryJokerJal/Jalium.UI/pull/174)
- [固定提交的版本配置](https://github.com/VeryJokerJal/Jalium.UI/blob/58b5ec18561f3bbfc5661e74d91407acb7987feb/Directory.Build.props)
- [CSS 附加属性 API](https://github.com/VeryJokerJal/Jalium.UI/blob/58b5ec18561f3bbfc5661e74d91407acb7987feb/src/managed/Jalium.UI.Core/Css/Css.cs)
- [CSS 规则解析器](https://github.com/VeryJokerJal/Jalium.UI/blob/58b5ec18561f3bbfc5661e74d91407acb7987feb/src/managed/Jalium.UI.Core/Css/Parsing/CssParser.cs)
- [CSS 属性注册表](https://github.com/VeryJokerJal/Jalium.UI/blob/58b5ec18561f3bbfc5661e74d91407acb7987feb/src/managed/Jalium.UI.Core/Css/CssPropertyRegistry.cs)
- [CSS 构建集成](https://github.com/VeryJokerJal/Jalium.UI/blob/58b5ec18561f3bbfc5661e74d91407acb7987feb/src/managed/Jalium.UI.Build/build/Jalium.UI.Css.targets)
- [Linux CI](https://github.com/VeryJokerJal/Jalium.UI/actions/runs/36429066964)、[Apple CI](https://github.com/VeryJokerJal/Jalium.UI/actions/runs/36429066750)
