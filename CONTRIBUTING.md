# Contributing to FluentJalium

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## 🎉 Welcome!

Thank you for your interest in contributing to FluentJalium! We welcome contributions from the community and are grateful for any help you can provide.

## 📋 Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [How to Contribute](#how-to-contribute)
4. [Development Guidelines](#development-guidelines)
5. [Pull Request Process](#pull-request-process)
6. [Coding Standards](#coding-standards)
7. [Testing Guidelines](#testing-guidelines)

## 📜 Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please be respectful, professional, and inclusive in all interactions.

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK or later
- Git
- Visual Studio 2025, JetBrains Rider, or Visual Studio Code
- Basic knowledge of C#, XAML, and Jalium.UI

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/FluentJalium.git
   cd FluentJalium
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/FluentJalium.git
   ```
4. **Build the project**:
   ```bash
   dotnet build FluentJalium.slnx
   ```
5. **Run tests**:
   ```bash
   dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj
   ```

## 🤝 How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in [Issues](https://github.com/your-org/FluentJalium/issues)
2. If not, create a new issue using the **Bug Report** template
3. Provide detailed information including:
   - Clear description of the bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details
   - Screenshots if applicable

### Suggesting Features

1. Check [Issues](https://github.com/your-org/FluentJalium/issues) for existing feature requests
2. Create a new issue using the **Feature Request** template
3. Clearly describe:
   - The problem you're trying to solve
   - Your proposed solution
   - Use cases and examples

### Contributing Code

1. **Find or create an issue** for the work you want to do
2. **Comment on the issue** to let others know you're working on it
3. **Create a feature branch**:
   ```bash
   git checkout -b feature/my-feature-name
   ```
4. **Make your changes** following our coding standards
5. **Write or update tests** for your changes
6. **Run tests locally** to ensure they pass
7. **Commit your changes** with clear, descriptive messages
8. **Push to your fork**:
   ```bash
   git push origin feature/my-feature-name
   ```
9. **Create a Pull Request** using our PR template

## 📐 Development Guidelines

### Project Structure

```
FluentJalium/
├── src/
│   └── FluentJalium/           # Main library
│       ├── Controls/            # Control implementations
│       ├── Themes/              # Theme resources
│       └── ...
├── samples/
│   └── FluentJalium.Gallery/   # Demo application
├── tests/
│   └── FluentJalium.Tests/     # Unit tests
└── docs/                        # Documentation
```

### Naming Conventions

- **Controls**: Prefix with `FW` (e.g., `FWButton`, `FWTextBox`)
- **Classes**: PascalCase (e.g., `FluentThemeManager`)
- **Methods**: PascalCase (e.g., `ApplyTheme`)
- **Properties**: PascalCase (e.g., `IsEnabled`)
- **Private fields**: camelCase with underscore (e.g., `_privateField`)
- **Constants**: PascalCase (e.g., `DefaultTimeout`)

## 🔄 Pull Request Process

1. **Update documentation** if you're adding new features
2. **Ensure all tests pass** locally
3. **Update the CHANGELOG.md** with your changes
4. **Fill out the PR template** completely
5. **Link related issues** in the PR description
6. **Wait for review** - maintainers will review your PR
7. **Address feedback** if requested
8. **Squash commits** if requested before merging

### PR Review Criteria

- Code quality and readability
- Test coverage
- Documentation completeness
- Performance impact
- Breaking changes (must be justified)
- Adherence to coding standards

## 💻 Coding Standards

### C# Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use nullable reference types
- Avoid `var` for non-obvious types
- Use expression-bodied members where appropriate
- Keep methods focused and concise

### XAML Guidelines

- Use proper indentation (4 spaces)
- Order attributes logically (Name, Style, Layout, Content, Events)
- Use `x:Name` for controls that need code-behind access
- Prefer data binding over code-behind

### Documentation

- Add XML documentation comments for public APIs
- Include code examples in documentation
- Update README.md for significant changes

## 🧪 Testing Guidelines

- Write unit tests for all new functionality
- Aim for high test coverage (>80%)
- Test edge cases and error conditions
- Use descriptive test names
- Follow Arrange-Act-Assert pattern

```csharp
[Fact]
public void FWButton_Click_RaisesClickEvent()
{
    // Arrange
    var button = new FWButton();
    var eventRaised = false;
    button.Click += (s, e) => eventRaised = true;
    
    // Act
    button.PerformClick();
    
    // Assert
    Assert.True(eventRaised);
}
```

## 📝 Commit Message Guidelines

Use clear, descriptive commit messages following this format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(FWButton): add support for icon positioning

Added IconPosition property to allow icons on left or right side of button content.

Closes #123
```

```
fix(FWTextBox): resolve placeholder text visibility issue

Fixed bug where placeholder text remained visible after user input in certain scenarios.

Fixes #456
```

## 🆘 Getting Help

- Ask questions in [GitHub Discussions](https://github.com/your-org/FluentJalium/discussions)
- Join our community chat (if available)
- Check existing documentation and issues

## 📜 License

By contributing to FluentJalium, you agree that your contributions will be licensed under the MIT License.

---

<a name="chinese"></a>

# 为 FluentJalium 做贡献

## 🎉 欢迎！

感谢您有兴趣为 FluentJalium 做贡献！我们欢迎社区的贡献，并感谢您提供的任何帮助。

## 📋 目录

1. [行为准则](#行为准则)
2. [开始使用](#开始使用)
3. [如何贡献](#如何贡献)
4. [开发指南](#开发指南)
5. [Pull Request 流程](#pull-request-流程)
6. [编码规范](#编码规范)
7. [测试指南](#测试指南)

## 📜 行为准则

本项目遵守行为准则。参与时，您应遵守此准则。请在所有互动中保持尊重、专业和包容。

## 🚀 开始使用

### 前置要求

- .NET 10 SDK 或更高版本
- Git
- Visual Studio 2025、JetBrains Rider 或 Visual Studio Code
- C#、XAML 和 Jalium.UI 的基础知识

### 设置开发环境

1. **Fork 仓库** 在 GitHub 上
2. **克隆您的 fork** 到本地：
   ```bash
   git clone https://github.com/YOUR_USERNAME/FluentJalium.git
   cd FluentJalium
   ```
3. **添加上游远程**：
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/FluentJalium.git
   ```
4. **构建项目**：
   ```bash
   dotnet build FluentJalium.slnx
   ```
5. **运行测试**：
   ```bash
   dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj
   ```

## 🤝 如何贡献

### 报告错误

1. 检查 [Issues](https://github.com/your-org/FluentJalium/issues) 中是否已报告该错误
2. 如果没有，使用 **Bug Report** 模板创建新问题
3. 提供详细信息，包括：
   - 错误的清晰描述
   - 重现步骤
   - 期望行为与实际行为
   - 环境详情
   - 截图（如适用）

### 建议功能

1. 检查 [Issues](https://github.com/your-org/FluentJalium/issues) 中是否已有功能请求
2. 使用 **Feature Request** 模板创建新问题
3. 清楚描述：
   - 您尝试解决的问题
   - 您提议的解决方案
   - 使用场景和示例

### 贡献代码

1. **查找或创建问题** 以跟踪您要做的工作
2. **在问题上评论** 让其他人知道您正在处理它
3. **创建功能分支**：
   ```bash
   git checkout -b feature/my-feature-name
   ```
4. **进行更改** 遵循我们的编码规范
5. **编写或更新测试** 以覆盖您的更改
6. **本地运行测试** 确保它们通过
7. **提交更改** 使用清晰、描述性的消息
8. **推送到您的 fork**：
   ```bash
   git push origin feature/my-feature-name
   ```
9. **创建 Pull Request** 使用我们的 PR 模板

## 📐 开发指南

### 命名约定

- **控件**：使用 `FW` 前缀（例如 `FWButton`、`FWTextBox`）
- **类**：PascalCase（例如 `FluentThemeManager`）
- **方法**：PascalCase（例如 `ApplyTheme`）
- **属性**：PascalCase（例如 `IsEnabled`）
- **私有字段**：camelCase 带下划线（例如 `_privateField`）
- **常量**：PascalCase（例如 `DefaultTimeout`）

## 🔄 Pull Request 流程

1. **更新文档** 如果您添加新功能
2. **确保所有测试通过** 在本地
3. **更新 CHANGELOG.md** 记录您的更改
4. **完整填写 PR 模板**
5. **链接相关问题** 在 PR 描述中
6. **等待审查** - 维护者将审查您的 PR
7. **处理反馈** 如果有要求
8. **合并前压缩提交** 如果被要求

## 💻 编码规范

### C# 指南

- 遵循 [C# 编码约定](https://docs.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用可空引用类型
- 对于非明显类型避免使用 `var`
- 在适当的地方使用表达式体成员
- 保持方法专注和简洁

### XAML 指南

- 使用正确的缩进（4 个空格）
- 按逻辑顺序排列属性（Name、Style、Layout、Content、Events）
- 对需要代码后置访问的控件使用 `x:Name`
- 优先使用数据绑定而不是代码后置

### 文档

- 为公共 API 添加 XML 文档注释
- 在文档中包含代码示例
- 对重要更改更新 README.md

## 🧪 测试指南

- 为所有新功能编写单元测试
- 追求高测试覆盖率（>80%）
- 测试边缘情况和错误条件
- 使用描述性的测试名称
- 遵循 Arrange-Act-Assert 模式

## 📝 提交消息指南

使用清晰、描述性的提交消息，遵循此格式：

```
<type>(<scope>): <subject>

<body>

<footer>
```

**类型：**
- `feat`：新功能
- `fix`：错误修复
- `docs`：文档更改
- `style`：代码样式更改（格式化等）
- `refactor`：代码重构
- `test`：添加或更新测试
- `chore`：维护任务

**示例：**
```
feat(FWButton): 添加图标位置支持

添加了 IconPosition 属性以允许图标位于按钮内容的左侧或右侧。

Closes #123
```

## 🆘 获取帮助

- 在 [GitHub Discussions](https://github.com/your-org/FluentJalium/discussions) 中提问
- 查看现有文档和问题

## 📜 许可证

通过为 FluentJalium 做贡献，您同意您的贡献将根据 MIT 许可证授权。

---

<div align="center">

**感谢您的贡献！ / Thank you for your contributions!**

</div>

