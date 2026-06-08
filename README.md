# FluentJalium

<div align="center">

**Modern Fluent Design System for Jalium.UI**

[English](#english) | [中文](#chinese)

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-Preview-orange.svg)](https://nuget.org/)

</div>

---

<a name="english"></a>

## 🎨 Overview

FluentJalium is a comprehensive Fluent Design System implementation for Jalium.UI, bringing modern Windows 11-style controls and theming to cross-platform .NET applications. Built on .NET 10, it provides 150+ production-ready controls with native performance and seamless integration.

### ✨ Key Features

- **🎯 150+ Fluent Controls** - Complete WinUI 3-inspired control library with FW prefix
- **🎨 Dynamic Theming** - Light/Dark themes with customizable accent colors
- **⚡ High Performance** - Native rendering with AOT and trimming support
- **🌐 Cross-Platform** - Windows, macOS, and Linux support
- **♿ Accessibility** - WCAG 2.1 AA compliant with screen reader support
- **🔧 Easy Integration** - Drop-in replacement for standard Jalium.UI controls

## 🚀 Quick Start

### Installation

```bash
dotnet add package FluentJalium
```

### Basic Setup

```csharp
using FluentJalium;
using Jalium.UI;

// Create your application
var app = new Application();

// Apply Fluent theme
FluentThemeManager.Apply(app);

// Create and show main window
var window = new MainWindow();
app.Run(window);
```

### Using Controls

```xml
xmlns:fw="clr-namespace:FluentJalium.Controls;assembly=FluentJalium"

<fw:FWButton Content="Click Me" />
<fw:FWTextBox PlaceholderText="Enter text..." />
<fw:FWToggleSwitch Header="Enable notifications" IsOn="True" />
<fw:FWProgressRing IsIndeterminate="True" />
```

## 📦 Control Categories

### Input & Forms
- **Buttons**: FWButton, FWRepeatButton, FWHyperlinkButton, FWDropDownButton, FWSplitButton, FWToggleSplitButton
- **Text Input**: FWTextBox, FWPasswordBox, FWNumberBox, FWAutoCompleteBox, FWRichTextBox
- **Selection**: FWCheckBox, FWRadioButton, FWComboBox, FWToggleSwitch, FWSlider, FWRangeSlider

### Collections & Data
- **Lists**: FWListBox, FWListView, FWTreeView, FWDataGrid, FWTreeDataGrid, FWItemsRepeater
- **Navigation**: FWNavigationView, FWTabControl, FWFrame

### Layout & Containers
- **Panels**: FWStackPanel, FWWrapPanel, FWGrid
- **Containers**: FWBorder, FWContentControl, FWScrollViewer, FWSplitView, FWExpander, FWGroupBox

### Interaction & Feedback
- **Progress**: FWProgressBar, FWProgressRing
- **Notifications**: FWInfoBar, FWInfoBadge, FWToastNotificationItem
- **Dialogs**: FWContentDialog, FWToolTip, FWTeachingTip

### Advanced Controls
- **Animation**: FWAnimatedIcon, FWAnimatedVisualPlayer
- **Scrolling**: FWScroller, FWAnnotatedScrollBar, FWRefreshContainer
- **Materials**: FWBackdrop (Acrylic, Mica, MicaAlt), FWAcrylicBrush

## 🎨 Theming

### Switching Themes

```csharp
// Apply Light theme
FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

// Apply Dark theme
FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

// Apply custom accent color
FluentThemeManager.ApplyAccent(Color.FromRgb(0, 120, 215));
```

### Custom Theme Resources

```xml
<ResourceDictionary Source="/FluentJalium;component/Themes/Generic.jalxaml" />
```

## 📚 Documentation

- **[Getting Started](docs/GETTING_STARTED.md)** - Installation and basic usage
- **[Getting Started (中文)](docs/GETTING_STARTED.zh-CN.md)** - 安装和基础使用
- **[API Reference](docs/API_REFERENCE.md)** - Complete API documentation
- **[API Reference (中文)](docs/API_REFERENCE.zh-CN.md)** - 完整 API 文档
- **[Theming Guide](docs/THEMING.md)** - Customization and styling
- **[Migration Guide](docs/MIGRATION.md)** - Migrating from Jalium.UI
- **[Performance Guide](docs/PERFORMANCE.md)** - Optimization best practices
- **[Contributing](CONTRIBUTING.md)** - How to contribute

## 🏗️ Build & Development

### Prerequisites
- .NET 10 SDK or later
- Visual Studio 2025 or JetBrains Rider (optional)

### Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/FluentJalium.git
cd FluentJalium

# Build the library
dotnet build src/FluentJalium/FluentJalium.csproj

# Run tests
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj

# Run the gallery app
dotnet run --project samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj
```

### Using Local Jalium.UI Source

```bash
dotnet build FluentJalium.slnx -c Debug /p:UseJaliumSourceReferences=true
```

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **[Jalium.UI](https://github.com/jalium/Jalium.UI)** - Core UI framework
- **[WinUI 3](https://github.com/microsoft/microsoft-ui-xaml)** - Design inspiration
- **[FluentAvalonia](https://github.com/amwx/FluentAvalonia)** - Reference implementation

## 📊 Project Status

**Current Version**: v0.1.0-preview.1  
**Status**: Active Development

- ✅ 150+ controls implemented
- ✅ Batch 1-16 complete
- ✅ Unit tests coverage
- ✅ Gallery demo application
- 🚧 API stabilization in progress
- 🚧 Performance optimization ongoing

See [PROGRESS.md](PROGRESS.md) for detailed roadmap.

---

<a name="chinese"></a>

# FluentJalium

## 🎨 概述

FluentJalium 是为 Jalium.UI 打造的全面 Fluent Design System 实现，为跨平台 .NET 应用程序带来现代化的 Windows 11 风格控件和主题。基于 .NET 10 构建，提供 150+ 生产就绪的控件，具有原生性能和无缝集成。

### ✨ 核心特性

- **🎯 150+ Fluent 控件** - 完整的 WinUI 3 风格控件库，使用 FW 前缀
- **🎨 动态主题** - 亮色/暗色主题，支持自定义强调色
- **⚡ 高性能** - 原生渲染，支持 AOT 和裁剪
- **🌐 跨平台** - 支持 Windows、macOS 和 Linux
- **♿ 无障碍** - 符合 WCAG 2.1 AA 标准，支持屏幕阅读器
- **🔧 易于集成** - 可直接替换标准 Jalium.UI 控件

## 🚀 快速开始

### 安装

```bash
dotnet add package FluentJalium
```

### 基础设置

```csharp
using FluentJalium;
using Jalium.UI;

// 创建应用程序
var app = new Application();

// 应用 Fluent 主题
FluentThemeManager.Apply(app);

// 创建并显示主窗口
var window = new MainWindow();
app.Run(window);
```

### 使用控件

```xml
xmlns:fw="clr-namespace:FluentJalium.Controls;assembly=FluentJalium"

<fw:FWButton Content="点击我" />
<fw:FWTextBox PlaceholderText="输入文本..." />
<fw:FWToggleSwitch Header="启用通知" IsOn="True" />
<fw:FWProgressRing IsIndeterminate="True" />
```

## 📦 控件分类

### 输入与表单
- **按钮**: FWButton, FWRepeatButton, FWHyperlinkButton, FWDropDownButton, FWSplitButton, FWToggleSplitButton
- **文本输入**: FWTextBox, FWPasswordBox, FWNumberBox, FWAutoCompleteBox, FWRichTextBox
- **选择控件**: FWCheckBox, FWRadioButton, FWComboBox, FWToggleSwitch, FWSlider, FWRangeSlider

### 集合与数据
- **列表**: FWListBox, FWListView, FWTreeView, FWDataGrid, FWTreeDataGrid, FWItemsRepeater
- **导航**: FWNavigationView, FWTabControl, FWFrame

### 布局与容器
- **面板**: FWStackPanel, FWWrapPanel, FWGrid
- **容器**: FWBorder, FWContentControl, FWScrollViewer, FWSplitView, FWExpander, FWGroupBox

### 交互与反馈
- **进度**: FWProgressBar, FWProgressRing
- **通知**: FWInfoBar, FWInfoBadge, FWToastNotificationItem
- **对话框**: FWContentDialog, FWToolTip, FWTeachingTip

### 高级控件
- **动画**: FWAnimatedIcon, FWAnimatedVisualPlayer
- **滚动**: FWScroller, FWAnnotatedScrollBar, FWRefreshContainer
- **材质**: FWBackdrop (Acrylic, Mica, MicaAlt), FWAcrylicBrush

## 🎨 主题设置

### 切换主题

```csharp
// 应用亮色主题
FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

// 应用暗色主题
FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);

// 应用自定义强调色
FluentThemeManager.ApplyAccent(Color.FromRgb(0, 120, 215));
```

## 📚 文档

- **[入门指南](docs/GETTING_STARTED.zh-CN.md)** - 安装和基础使用
- **[API 参考](docs/API_REFERENCE.zh-CN.md)** - 完整 API 文档
- **[主题指南](docs/THEMING.zh-CN.md)** - 自定义和样式
- **[迁移指南](docs/MIGRATION.zh-CN.md)** - 从 Jalium.UI 迁移
- **[性能指南](docs/PERFORMANCE.zh-CN.md)** - 优化最佳实践
- **[贡献指南](CONTRIBUTING.zh-CN.md)** - 如何贡献

## 🏗️ 构建与开发

### 前置要求
- .NET 10 SDK 或更高版本
- Visual Studio 2025 或 JetBrains Rider（可选）

### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/yourusername/FluentJalium.git
cd FluentJalium

# 构建库
dotnet build src/FluentJalium/FluentJalium.csproj

# 运行测试
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj

# 运行示例应用
dotnet run --project samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj
```

## 🤝 贡献

我们欢迎贡献！请查看 [CONTRIBUTING.zh-CN.md](CONTRIBUTING.zh-CN.md) 了解详情。

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m '添加新特性'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

## 📝 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 🙏 致谢

- **[Jalium.UI](https://github.com/jalium/Jalium.UI)** - 核心 UI 框架
- **[WinUI 3](https://github.com/microsoft/microsoft-ui-xaml)** - 设计灵感
- **[FluentAvalonia](https://github.com/amwx/FluentAvalonia)** - 参考实现

## 📊 项目状态

**当前版本**: v0.1.0-preview.1  
**状态**: 活跃开发中

- ✅ 150+ 控件已实现
- ✅ Batch 1-16 完成
- ✅ 单元测试覆盖
- ✅ 示例应用
- 🚧 API 稳定化进行中
- 🚧 性能优化进行中

详见 [PROGRESS.md](PROGRESS.md) 了解详细路线图。

---

<div align="center">

**Made with ❤️ for the .NET community**

[Report Bug](https://github.com/yourusername/FluentJalium/issues) · 
[Request Feature](https://github.com/yourusername/FluentJalium/issues) · 
[Documentation](docs/)

</div>

