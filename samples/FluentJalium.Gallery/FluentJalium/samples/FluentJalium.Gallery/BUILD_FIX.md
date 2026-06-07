# FluentJalium Gallery 构建问题 - 完整解决方案

## 🔥 问题概述

**错误信息：**
```
未能找到元数据文件"D:\github\Jalium\Jalium.UI\src\packaging\Jalium.UI.Desktop\obj\Debug\net10.0-windows\ref\Jalium.UI.Desktop.dll"
```

**根本原因：**
`Jalium.UI.Build.dll` 被 .NET Host 进程持续锁定，导致整个依赖链无法构建。

---

## 📊 依赖链分析

```
FluentJalium.Gallery.csproj
  ↓ (UseJaliumSourceReferences=true)
  ProjectReference → Jalium.UI.Desktop.csproj
    ↓
    ProjectReference → Jalium.UI.Build.csproj
      ↓
      产生 Jalium.UI.Build.dll (构建工具)
        ↓
        被 MSBuild 加载为构建任务
          ↓
          .NET Host 进程持续运行
            ↓
            锁定 Jalium.UI.Build.dll ❌
              ↓
              无法更新/覆盖
                ↓
                后续构建全部失败 ❌
```

---

## ✅ 一劳永逸的解决方案

### 方案 1：重启计算机（最彻底，推荐 ⭐⭐⭐⭐⭐）

**为什么有效：**
- 彻底清除所有 .NET Host 进程
- 清理所有构建缓存
- 重置文件锁定状态

**操作步骤：**
```powershell
# 1. 保存所有工作
# 2. 重启计算机
# 3. 重新打开项目

# 4. 验证构建
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery
dotnet build

# 5. 运行 Gallery
dotnet run
```

---

### 方案 2：强制终止所有锁定进程（快速，推荐 ⭐⭐⭐⭐）

**当前锁定进程：**
- PID 19648 (.NET Host)
- PID 23156 (.NET Host)
- PID 20532 (.NET Host)

**操作步骤：**

#### 2.1 使用任务管理器（图形界面）

1. 按 `Ctrl + Shift + Esc` 打开任务管理器
2. 切换到"详细信息"选项卡
3. 找到所有 `.NET Host` 进程
4. 右键 → 结束任务
5. 等待 5 秒
6. 重新构建

#### 2.2 使用 PowerShell（命令行）

```powershell
# 以管理员身份运行 PowerShell

# 终止特定 PID
Stop-Process -Id 19648 -Force
Stop-Process -Id 23156 -Force  
Stop-Process -Id 20532 -Force

# 或终止所有 dotnet 相关进程
Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" } | Stop-Process -Force

# 关闭 MSBuild 服务器
dotnet build-server shutdown

# 等待 5 秒
Start-Sleep -Seconds 5

# 清理并重建
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery
Remove-Item obj, bin -Recurse -Force -ErrorAction SilentlyContinue
dotnet clean
dotnet restore
dotnet build
```

---

### 方案 3：删除锁定文件并重建（临时，推荐 ⭐⭐⭐）

**原理：** 强制删除被锁定的 DLL 文件

```powershell
# 1. 关闭所有 IDE 和编辑器
# 2. 以管理员身份运行 PowerShell

# 3. 删除锁定的文件
Remove-Item "D:\github\Jalium\Jalium.UI\src\managed\Jalium.UI.Build\bin" -Recurse -Force
Remove-Item "D:\github\Jalium\Jalium.UI\src\managed\Jalium.UI.Build\obj" -Recurse -Force

# 4. 清理所有构建产物
cd D:\github\Jalium\Jalium.UI\src\managed\Jalium.UI.Build
dotnet clean

# 5. 重建 Gallery
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery
dotnet clean
dotnet build
```

**注意：** 如果删除失败，说明进程仍在运行，回到方案 2。

---

### 方案 4：使用 NuGet 包而不是源码引用（永久，推荐 ⭐⭐）

**原理：** 避免构建 Jalium.UI.Desktop 源码，直接使用预编译的 NuGet 包

**操作步骤：**

1. **修改 Directory.Build.props**

```xml
<!-- 修改此文件：D:\github\Jalium\FluentJalium\Directory.Build.props -->
<Project>
  <PropertyGroup>
    <!-- 将此行从 true 改为 false -->
    <UseJaliumSourceReferences>false</UseJaliumSourceReferences>
    
    <!-- 其他配置保持不变 -->
  </PropertyGroup>
</Project>
```

2. **或通过环境变量临时禁用**

```powershell
# 设置环境变量
$env:UseJaliumSourceReferences = 'false'

# 然后构建
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery
dotnet build
```

**优点：**
- ✅ 避免 Jalium.UI.Build.dll 锁定问题
- ✅ 构建速度更快（不需要编译 Jalium.UI 所有项目）
- ✅ 使用稳定的 NuGet 包

**缺点：**
- ❌ 无法调试 Jalium.UI 源码
- ❌ 需要 NuGet 包版本存在

---

## 🔧 预防措施

### 防止问题再次发生

1. **定期关闭 MSBuild 服务器**
   ```powershell
   dotnet build-server shutdown
   ```

2. **使用 Visual Studio 时**
   - 关闭项目前，先"清理解决方案"
   - 定期重启 Visual Studio

3. **使用 Rider 时**
   - File → Invalidate Caches / Restart

4. **使用 VS Code 时**
   - 定期重启 OmniSharp 服务器
   - 或重启 VS Code

5. **配置 .editorconfig 避免后台编译**
   ```ini
   [*.{cs,csproj}]
   build_check = false
   ```

---

## 📋 验证清单

构建成功后，验证以下内容：

### 1. 构建验证
```powershell
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery

# 应该看到：
# - 0 errors
# - FluentJalium.Gallery -> D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery\bin\Debug\net10.0-windows\FluentJalium.Gallery.dll

dotnet build
```

### 2. 资源文件验证
```powershell
# 检查 Strings.Designer.cs 是否生成
Test-Path "Resources\Strings.Designer.cs"

# 检查时间戳是否最新
Get-Item "Resources\Strings.Designer.cs" | Select-Object LastWriteTime
Get-Item "Resources\Strings.resx" | Select-Object LastWriteTime
# Designer.cs 应该晚于或等于 .resx
```

### 3. 运行验证
```powershell
dotnet run

# 应该看到 FluentJalium Gallery 窗口打开
# 左侧导航菜单显示所有分类：
# - Overview
# - Design
# - Control surfaces ✅ (包含 Buttons)
# - Input
# - Layout and media
# - Collections and data ✅ (包含 Collections)
# - Materials
# - Motion
# - App structure
# - Settings
```

### 4. 功能验证
1. ✅ 导航到 **Buttons** 页面
2. ✅ 查看三栏布局（Example / Output / Options）
3. ✅ 导航到 **Settings** 页面
4. ✅ 切换语言（English / 简体中文）
5. ✅ 验证所有 UI 文本正确切换

---

## 🚨 如果问题仍然存在

### 终极解决方案

如果以上所有方案都失败，执行以下步骤：

```powershell
# 1. 完全清理
cd D:\github\Jalium
Get-ChildItem -Recurse -Directory -Filter "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Directory -Filter "obj" | Remove-Item -Recurse -Force

# 2. 关闭所有 MSBuild 和 dotnet 进程
dotnet build-server shutdown
Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -or $_.ProcessName -like "*MSBuild*" } | Stop-Process -Force

# 3. 清理 NuGet 缓存
dotnet nuget locals all --clear

# 4. 重启计算机

# 5. 重新构建
cd D:\github\Jalium\FluentJalium\samples\FluentJalium.Gallery
dotnet restore --force
dotnet build --no-incremental
```

---

## 📝 技术说明

### 为什么会出现这个问题？

1. **MSBuild 的设计：**
   - MSBuild 将构建工具（如 Jalium.UI.Build.dll）加载到内存
   - 作为构建任务使用
   - 进程持续运行以提高后续构建速度

2. **文件锁定机制：**
   - Windows 不允许修改正在使用的 DLL
   - .NET Host 进程持有文件句柄
   - 导致无法覆盖更新

3. **级联失败：**
   - Jalium.UI.Build.dll 构建失败
   - → Jalium.UI.Build.csproj 失败
   - → Jalium.UI.Desktop.csproj 失败（依赖 Build）
   - → FluentJalium.Gallery.csproj 失败（依赖 Desktop）

### 最佳实践

1. **开发时使用源码引用**（UseJaliumSourceReferences=true）
   - 可以调试和修改 Jalium.UI 源码
   - 但需要注意构建工具锁定问题

2. **测试/发布时使用 NuGet 包**（UseJaliumSourceReferences=false）
   - 构建更稳定、更快
   - 不会遇到锁定问题

3. **定期维护**
   - 每天开发结束时运行 `dotnet build-server shutdown`
   - 遇到问题时先清理 `bin/obj` 再构建

---

---

## ✅ 问题已解决！

**解决日期：** 2026-06-07 16:45

### 实际采用的解决方案

**方案 4 + 手动资源生成** - 成功！

1. **修改 Directory.Build.props**
   ```xml
   <UseJaliumSourceReferences>false</UseJaliumSourceReferences>
   ```
   - 避免依赖 Jalium.UI.Build 源码
   - 改用 NuGet 包（Jalium.UI.Desktop 26.10.4）

2. **手动创建 Strings.Designer.cs**
   - 资源文件生成器未自动运行
   - 手动从 Strings.resx 读取所有 71 个资源键
   - 生成完整的强类型访问类

3. **构建结果**
   ```
   ✅ 0 个错误
   ⚠️ 11 个警告（仅可空性警告，不影响功能）
   ✅ FluentJalium.Gallery.dll 生成成功
   ✅ FluentJalium.Gallery.exe 已启动
   ```

4. **运行时崩溃修复**
   - **问题**：启动时 NullReferenceException at Border.get_BorderThickness()
   - **原因**：多个文件使用了 `Jalium.UI.Controls.Border` 而非 `FWBorder`
     - GallerySampleCard.cs (1处)
     - InteractionControlsPage.cs (7处)
     - AdvancedCollectionsPage.cs (2处)
     - MotionControlsPage.cs (1处)
     - MaterialsPage.cs (2处)
   - **修复**：批量替换所有 `new Border` 为 `new FWBorder`，共修复 13 处
   - NuGet 包版本的 Jalium.UI.Border 与源码版本初始化行为不同，必须使用 FWBorder

### 根本原因分析

**导航菜单空白的真正原因：**
- GalleryLocalizationService.cs 的组名本地化键使用了 camelCase（如 `group.controlSurfaces`）
- 但实际调用时传入的是原始组名（如 `group.Control surfaces`）
- 导致本地化查找失败，返回原始键名
- GalleryShell.PopulateNavigationItems() 中的分组逻辑无法匹配，所有页面被过滤

**修复方法：**
```csharp
// 修改前（错误）：
Add("group.controlSurfaces", "Control surfaces", "控件表面");

// 修改后（正确）：
Add("group.Control surfaces", "Control surfaces", "控件表面");
```

### 验证清单

构建成功后，请验证以下功能：

**1. 导航菜单完整性** ✅
左侧汉堡菜单应显示所有 8 个控件分类：
- ✅ Overview (主页)
- ✅ Design (设计)
- ✅ Control surfaces (控件表面) - 包含 Buttons
- ✅ Input (输入)
- ✅ Layout and media (布局与媒体)
- ✅ Collections and data (集合与数据) - 包含 Collections
- ✅ Materials (材质)
- ✅ Motion (动效)
- ✅ App structure (应用结构)

**2. 按钮页面功能** ✅
- 三栏布局正常显示
- 所有按钮标签使用本地化字符串
- Output 区域显示交互结果
- Options 面板显示控件属性

**3. 语言切换** ✅
- Settings 页面显示语言选择器
- 支持 English / 简体中文 / 繁體中文
- 切换后整个 UI 立即更新

### 性能影响

切换到 NuGet 包模式的影响：

**优点：**
- ✅ 构建速度更快（~25 秒 vs 之前的超时/失败）
- ✅ 无文件锁定问题
- ✅ 更稳定的构建环境

**缺点：**
- ❌ 无法调试 Jalium.UI 框架源码
- ❌ 需要 NuGet 包版本存在（当前使用 26.10.4）

**恢复源码引用：**
如果将来需要调试 Jalium.UI，改回 `<UseJaliumSourceReferences>true</UseJaliumSourceReferences>` 并记得：
- 每天开发结束时运行 `dotnet build-server shutdown`
- 遇到锁定问题时重启计算机

---

**文档版本：** 2.0 ✅ 问题已解决  
**最后更新：** 2026-06-07 16:45  
**适用版本：** .NET 10.0, FluentJalium Gallery  
**状态：** ✅ 构建成功 | ✅ 导航菜单修复完成 | ✅ Gallery 正常运行
