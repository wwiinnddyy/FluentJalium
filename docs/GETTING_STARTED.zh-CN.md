# FluentJalium 入门指南

本指南将帮助您在 Jalium.UI 应用程序中开始使用 FluentJalium。

## 目录

1. [安装](#安装)
2. [基础设置](#基础设置)
3. [第一个控件](#第一个控件)
4. [常见场景](#常见场景)
5. [主题和样式](#主题和样式)
6. [迁移指南](#迁移指南)
7. [故障排除](#故障排除)

---

## 安装

### 前置要求

- **.NET 10 SDK** 或更高版本
- **Jalium.UI 框架** 已安装并配置
- **Visual Studio 2025** 或 **Visual Studio Code** 配合 C# 扩展

### NuGet 包

```bash
dotnet add package FluentJalium --version 0.1.0-preview.1
```

或通过包管理器控制台：

```powershell
Install-Package FluentJalium -Version 0.1.0-preview.1
```

### 手动安装

1. 克隆仓库：
```bash
git clone https://github.com/your-org/FluentJalium.git
```

2. 添加项目引用：
```bash
dotnet add reference path/to/FluentJalium/src/FluentJalium/FluentJalium.csproj
```

---

## 基础设置

### 1. 更新您的 App.jalxaml

添加 FluentJalium 命名空间并应用主题：

```xml
<Application x:Class="MyApp.App"
             xmlns="http://jalium.org/winfx/2024/jalxaml"
             xmlns:x="http://schemas.jalium.org/winfx/2024/jalxaml"
             xmlns:fw="using:FluentJalium.Controls">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- 导入 FluentJalium 主题 -->
                <ResourceDictionary Source="pack://application:,,,/FluentJalium;component/Themes/Generic.jalxaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 2. 在代码中配置

在您的 `App.jalxaml.cs` 中：

```csharp
using FluentJalium;
using Jalium.UI;

namespace MyApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // 应用 FluentJalium 主题
        FluentThemeManager.Apply(this);
        
        // 可选：设置主题变体
        FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
    }
}
```

---

## 第一个控件

### 示例 1：FWButton

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://jalium.org/winfx/2024/jalxaml"
        xmlns:fw="using:FluentJalium.Controls"
        Title="我的第一个 FluentJalium 应用"
        Width="800"
        Height="600">
    
    <StackPanel Margin="24" Spacing="16">
        <fw:FWButton Content="主要按钮" 
                     Click="OnButtonClick" />
        
        <fw:FWButton Content="次要按钮" />
        
        <fw:FWButton Content="强调按钮" />
    </StackPanel>
</Window>
```

代码后置：

```csharp
using FluentJalium.Controls;
using Jalium.UI;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (FWButton)sender;
        MessageBox.Show($"您点击了：{button.Content}");
    }
}
```

---

## 常见场景

### 场景 1：创建带验证的表单

```xml
<StackPanel Spacing="16" Margin="24">
    <!-- 文本输入 -->
    <fw:FWTextBox Header="用户名"
                  PlaceholderText="请输入用户名" />
    
    <!-- 密码输入 -->
    <fw:FWPasswordBox Header="密码"
                      PlaceholderText="请输入密码" />
    
    <!-- 下拉框 -->
    <fw:FWComboBox Header="国家/地区"
                   PlaceholderText="选择国家" />
    
    <!-- 复选框 -->
    <fw:FWCheckBox Content="我同意服务条款" />
    
    <!-- 提交按钮 -->
    <fw:FWButton Content="提交" Click="OnSubmitClick" />
</StackPanel>
```

### 场景 2：构建导航菜单

```xml
<fw:FWNavigationView>
    <fw:FWNavigationView.MenuItems>
        <fw:FWNavigationViewItem Content="首页" Tag="HomePage" />
        <fw:FWNavigationViewItem Content="文档" Tag="DocumentsPage" />
        <fw:FWNavigationViewItem Content="设置" Tag="SettingsPage" />
    </fw:FWNavigationView.MenuItems>
    
    <fw:FWNavigationView.Content>
        <Frame x:Name="ContentFrame" />
    </fw:FWNavigationView.Content>
</fw:FWNavigationView>
```

---

## 主题和样式

### 应用全局主题

```csharp
// 在 App.xaml.cs 中
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    // 亮色主题
    FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
    
    // 暗色主题
    FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
}
```

### 自定义强调色

```csharp
// 设置自定义强调色
FluentThemeManager.ApplyAccent(Color.FromRgb(0x00, 0x78, 0xD4));
```

---

## 迁移指南

### 从标准 Jalium.UI 控件迁移

| Jalium.UI | FluentJalium | 说明 |
|-----------|--------------|------|
| `Button` | `FWButton` | 增强样式 |
| `TextBox` | `FWTextBox` | 添加 `Header`, `PlaceholderText` |
| `CheckBox` | `FWCheckBox` | 兼容，增强样式 |
| `ComboBox` | `FWComboBox` | 添加 `Header`, `PlaceholderText` |

---

## 故障排除

### 问题：控件未显示 Fluent 样式

**解决方案**：确保在 App.jalxaml 中导入了主题资源：

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/FluentJalium;component/Themes/Generic.jalxaml" />
</ResourceDictionary.MergedDictionaries>
```

### 问题：找不到命名空间

**解决方案**：添加 NuGet 包或项目引用：

```bash
dotnet add package FluentJalium
```

并在 XAML 中添加命名空间：

```xml
xmlns:fw="using:FluentJalium.Controls"
```

### 问题：主题未应用

**解决方案**：在 `App.OnStartup()` 中调用 `FluentThemeManager.Apply()`：

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    FluentThemeManager.Apply(this);
}
```

---

## 下一步

- 📖 阅读 [API 参考](API_REFERENCE.zh-CN.md) 了解详细的控件文档
- 🎨 探索 [示例应用](../samples/FluentJalium.Gallery) 查看交互式示例
- ⚡ 查看 [性能指南](PERFORMANCE.zh-CN.md) 了解优化技巧
- 🚀 查看 [PROGRESS.md](../PROGRESS.md) 了解开发路线图

---

## 获取帮助

- **问题反馈**: [GitHub Issues](https://github.com/your-org/FluentJalium/issues)
- **讨论**: [GitHub Discussions](https://github.com/your-org/FluentJalium/discussions)

---

**最后更新**: 2026-06-08  
**版本**: 0.1.0-preview.1

