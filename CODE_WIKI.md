# FluentJalium Code Wiki

> FluentJalium 是基于 .NET 10 的 Fluent Design System 主题与控件库，为 Jalium.UI 提供 Fluent 视觉风格层和 FW 前缀控件集。

---

## 目录

- [1. 项目概述](#1-项目概述)
- [2. 项目架构](#2-项目架构)
- [3. 目录结构](#3-目录结构)
- [4. 核心模块详解](#4-核心模块详解)
  - [4.1 主题管理层 (FluentThemeManager)](#41-主题管理层-fluentthememanager)
  - [4.2 控件层 (Controls)](#42-控件层-controls)
  - [4.3 图标层 (Icon)](#43-图标层-icon)
  - [4.4 主题资源层 (Themes)](#44-主题资源层-themes)
- [5. Gallery 示例应用](#5-gallery-示例应用)
- [6. 测试体系](#6-测试体系)
- [7. 依赖关系](#7-依赖关系)
- [8. 构建与运行](#8-构建与运行)
- [9. 设计模式与约定](#9-设计模式与约定)
- [10. FW 控件完整清单](#10-fw-控件完整清单)

---

## 1. 项目概述

FluentJalium 是一个 Fluent Design System 主题和控件库，运行在 Jalium.UI 框架之上，面向 .NET 10 平台。项目采用双轨开发策略：

| 轨道 | 说明 |
|------|------|
| **主题覆盖层** | 通过 `FluentThemeManager.Apply(app)` 追加 Fluent 资源字典，替换 Jalium.UI 默认控件视觉样式，无需修改 Jalium.UI 本身 |
| **FW 控件层** | 以 `FW` 前缀命名的自有控件（如 `FWButton`），实现 `IFluentJaliumControl` 接口，旨在成为 WinUI / Community Toolkit / UI.WPF.Modern 风格的 Fluent 控件表面 |

**关键元数据：**

| 属性 | 值 |
|------|------|
| 版本 | 0.1.0-preview.1 |
| 目标框架 | net10.0 / net10.0-windows |
| SDK | .NET 10.0.201 |
| 许可证 | MIT |
| Jalium.UI 包版本 | 26.10.4 |

---

## 2. 项目架构

```
┌─────────────────────────────────────────────────────────────┐
│                    FluentJalium.Gallery                      │
│              (示例应用 / 视觉验证)                             │
├─────────────────────────────────────────────────────────────┤
│                      FluentJalium                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │   Controls   │  │    Icon      │  │     Themes       │  │
│  │  (FW 控件)   │  │  (图标系统)  │  │  (主题/样式)     │  │
│  └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘  │
│         │                 │                    │            │
│         └────────┬────────┴────────────────────┘            │
│                  │                                          │
│         FluentThemeManager (主题管理核心)                     │
├─────────────────────────────────────────────────────────────┤
│                      Jalium.UI                              │
│  UI.Core │ UI.Media │ UI.Input │ UI.Controls │ UI.Xaml     │
├─────────────────────────────────────────────────────────────┤
│                    FluentJalium.Tests                        │
│              (xUnit 单元测试)                                │
└─────────────────────────────────────────────────────────────┘
```

**架构核心原则：**

1. **不修改 Jalium.UI** — FluentJalium 通过资源字典覆盖和控件子类化实现 Fluent 风格，不直接修改上游框架
2. **FW 前缀约定** — 所有自有控件使用 `FW` 前缀，避免与 Jalium.UI 原生控件命名冲突
3. **IFluentJaliumControl 标记接口** — 所有 FW 控件实现此接口，用于主题系统识别和样式绑定
4. **Density 密度系统** — 每个控件类别定义独立的密度枚举（Compact / Comfortable / Spacious），支持响应式布局

---

## 3. 目录结构

```
FluentJalium/
├── FluentJalium.slnx                    # 解决方案文件
├── Directory.Build.props                # 全局构建属性
├── global.json                          # SDK 版本锁定
├── nuget.config                         # NuGet 源配置
├── LICENSE                              # MIT 许可证
├── README.md                            # 项目说明
│
├── src/
│   └── FluentJalium/                    # 核心库
│       ├── FluentJalium.csproj
│       ├── Controls/                    # FW 控件定义
│       │   ├── FluentControlMarker.cs   # XML 命名空间标记类
│       │   ├── IFluentJaliumControl.cs  # 控件标记接口
│       │   ├── Buttons/                 # 按钮类控件
│       │   ├── Switches/                # 开关类控件
│       │   ├── Range/                   # 范围/进度控件
│       │   ├── Selection/               # 选择类控件
│       │   ├── Collections/             # 集合/表格控件
│       │   ├── Navigation/              # 导航类控件
│       │   ├── DateTime/                # 日期时间控件
│       │   ├── Disclosure/              # 展开/对话框控件
│       │   ├── TextInput/               # 文本输入控件
│       │   ├── Menus/                   # 菜单/弹出控件
│       │   ├── Status/                  # 状态/通知控件
│       │   ├── Visuals/                 # 视觉/图标基础控件
│       │   ├── Interaction/             # 交互/滚动控件
│       │   ├── Input/                   # 输入/媒体控件
│       │   ├── Layout/                  # 布局基础控件
│       │   ├── Materials/               # 材质效果控件
│       │   ├── Selectors/               # 选择器控件
│       │   ├── DataInspectors/          # 数据检查器控件
│       │   └── Motion/                  # 动画控件
│       ├── Icon/                        # 图标系统
│       │   ├── FluentIcon.cs            # 图标控件
│       │   ├── FluentIconFactory.cs     # 图标工厂
│       │   ├── FluentIconExtensions.cs  # 图标扩展方法
│       │   ├── FluentIconSet.cs         # 图标集枚举
│       │   ├── FluentIconRegular.cs     # Regular 图标枚举
│       │   ├── FluentIconFilled.cs      # Filled 图标枚举
│       │   └── SegoeFluentIcon.cs       # Segoe 兼容图标枚举
│       ├── Themes/                      # 主题资源
│       │   ├── FluentThemeManager.cs    # 主题管理器
│       │   ├── Controls/                # 控件样式 (.jalxaml)
│       │   ├── FluentColors.jalxaml     # 颜色定义
│       │   ├── FluentGeometry.jalxaml   # 几何图形定义
│       │   ├── FluentMaterials.jalxaml  # 材质定义
│       │   ├── FluentMotion.jalxaml     # 动效定义
│       │   ├── FluentResources.jalxaml  # 基础资源
│       │   ├── FluentTypography.jalxaml # 排版定义
│       │   └── Generic.jalxaml          # 通用主题入口
│       └── Properties/
│           └── AssemblyInfo.cs
│
├── samples/
│   └── FluentJalium.Gallery/            # 示例应用
│       ├── FluentJalium.Gallery.csproj
│       ├── Program.cs                   # 入口点
│       ├── MainWindow.cs                # 主窗口
│       ├── Shell/                       # 外壳导航
│       ├── Pages/                       # 各控件示例页
│       ├── Models/                      # 数据模型
│       ├── Services/                    # 服务层
│       └── Controls/                    # Gallery 专用控件
│
└── tests/
    └── FluentJalium.Tests/              # 单元测试
        ├── FluentJalium.Tests.csproj
        ├── ApplicationCollectionFixture.cs
        └── *.Tests.cs                   # 各模块测试文件
```

---

## 4. 核心模块详解

### 4.1 主题管理层 (FluentThemeManager)

**文件位置：** [FluentThemeManager.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Themes/FluentThemeManager.cs)

FluentThemeManager 是整个库的核心入口，负责将 Fluent 资源层注入到 Jalium 应用程序中。

#### 关键类型

| 类型 | 说明 |
|------|------|
| `FluentThemeManager` | 静态类，主题管理的核心入口 |
| `FluentThemeOptions` | 主题配置选项 |
| `FluentThemeVariant` | 主题变体枚举 (Dark / Light / HighContrast) |

#### FluentThemeVariant 枚举

```csharp
public enum FluentThemeVariant
{
    Dark,          // 深色主题
    Light,         // 浅色主题
    HighContrast   // 高对比度主题
}
```

#### FluentThemeOptions 类

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Theme` | `FluentThemeVariant` | `Dark` | 主题变体 |
| `AccentColor` | `Color` | `#0078D4` | 强调色 |
| `DisplayFontFamily` | `string?` | `null` | 显示字体族 |
| `BodyFontFamily` | `string?` | `null` | 正文字体族 |
| `MonoFontFamily` | `string?` | `null` | 等宽字体族 |

#### FluentThemeManager 核心方法

| 方法 | 说明 |
|------|------|
| `Apply(Application)` | 使用默认选项应用 Fluent 主题 |
| `Apply(Application, FluentThemeOptions)` | 使用自定义选项应用 Fluent 主题 |
| `ApplyTheme(FluentThemeVariant)` | 运行时切换主题变体 |
| `ApplyAccent(Color)` | 运行时切换强调色 |
| `ApplyTypography(string, string, string)` | 运行时切换排版字体 |
| `Reset()` | 重置静态状态（测试用） |

#### 核心静态属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `DefaultAccentColor` | `Color` | `#0078D4` | 默认强调色 |
| `CurrentTheme` | `FluentThemeVariant` | `Dark` | 当前主题变体 |
| `CurrentAccentColor` | `Color` | `#0078D4` | 当前强调色 |
| `CurrentDisplayFontFamily` | `string` | `Segoe UI Variable Display` | 当前显示字体 |
| `CurrentBodyFontFamily` | `string` | `Segoe UI Variable Text` | 当前正文字体 |
| `CurrentMonoFontFamily` | `string` | `Cascadia Code` | 当前等宽字体 |
| `ThemeAssembly` | `Assembly` | — | 主题资源所在程序集 |

#### 主题应用流程

```
Apply(app, options)
  │
  ├─ 1. 保存选项到静态字段
  ├─ 2. 设置 ResourceDictionary.CurrentThemeKey
  ├─ 3. LoadGenericTheme() → 加载 Generic.jalxaml
  │     └─ AddFluentControlStyleAliases() → 为 FW 控件创建样式别名
  ├─ 4. BuildAccentDictionary() → 构建强调色资源字典
  │     └─ 生成 SystemAccentColor / AccentBrush 等资源
  ├─ 5. BuildTypographyDictionary() → 构建排版资源字典
  └─ 6. ForceRefresh() → 刷新资源缓存
```

#### 样式别名机制 (AliasStyle)

`AddFluentControlStyleAliases` 方法为每个 FW 控件创建样式别名，将 Jalium.UI 原生控件的默认样式映射为 FW 控件的 `BasedOn` 样式：

```csharp
// 将 FWButton 的样式 BasedOn 设为 Button 的样式
AliasStyle<FWButton, Button>(dictionary);
```

这意味着 FW 控件自动继承 Jalium.UI 原生控件的 Fluent 样式，同时可以添加自己的样式覆盖。

#### 强调色计算

`BuildAccentDictionary` 从基础强调色派生出完整的强调色系统：

| 资源键 | 计算方式 |
|--------|----------|
| `SystemAccentColorLight1-3` | 与白色混合 (20%/36%/52%) |
| `SystemAccentColorDark1-3` | 与黑色混合 (14%/28%/42%) |
| `FluentAccentFillColorDefault` | 原始强调色 |
| `FluentAccentFillColorSecondary` | 深色模式 = Light1，浅色模式 = Dark1 |
| `FluentAccentFillColorTertiary` | 深色模式 = Dark1，浅色模式 = Dark2 |
| `FluentAccentFillColorDisabled` | 深色模式 = 半透明白，浅色模式 = 半透明黑 |

---

### 4.2 控件层 (Controls)

**文件位置：** [src/FluentJalium/Controls/](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/)

#### 基础类型

| 类型 | 文件 | 说明 |
|------|------|------|
| `IFluentJaliumControl` | IFluentJaliumControl.cs | 标记接口，所有 FW 控件必须实现 |
| `FluentControlMarker` | FluentControlMarker.cs | XML 命名空间标记类，用于 XAML 命名空间映射 |

#### 控件设计模式

所有 FW 控件遵循统一的设计模式：

1. **继承 Jalium.UI 对应控件** — 如 `FWButton : Button`
2. **实现 IFluentJaliumControl** — 标记为 FluentJalium 控件
3. **添加 Density 属性** — 通过依赖属性支持密度预设
4. **构造函数中应用默认密度** — `ApplyDensity(this, Density)`
5. **密度变更回调** — `OnDensityChanged` 重新应用密度指标

#### 密度系统

每个控件类别定义独立的密度枚举，均包含三级预设：

| 枚举 | 适用控件 | Compact | Comfortable | Spacious |
|------|----------|---------|-------------|----------|
| `FWButtonDensity` | Button / RepeatButton / HyperlinkButton / SplitButton / AppBarButton | 最小高度 30px | 最小高度 32px | 最小高度 40px |
| `FWCommandSurfaceDensity` | CommandBar / ToolBar / ToolBarTray | 最小高度 40px | 最小高度 48px | 最小高度 56px |
| `FWSwitchDensity` | ToggleButton / ToggleSwitch | 最小高度 30px | 最小高度 32px | 最小高度 36px |
| `FWRangeDensity` | Slider / RangeSlider / ProgressBar | 最小高度 28px | 最小高度 32px | 最小高度 40px |
| `FWSelectionDensity` | CheckBox / RadioButton / ComboBox | 最小高度 22px | 最小高度 24px | 最小高度 32px |
| `FWCollectionDensity` | ListBox / ListView / TreeView | 最小高度 32px | 最小高度 40px | 最小高度 48px |
| `FWDataGridDensity` | DataGrid / TreeDataGrid | 行高 26px | 行高 32px | 行高 40px |
| `FWNavigationDensity` | NavigationView / TabControl | 面板宽 240px | 面板宽 280px | 面板宽 320px |
| `FWDateTimePickerDensity` | DatePicker / TimePicker | 最小高度 30px | 最小高度 34px | 最小高度 40px |
| `FWDisclosureDensity` | Expander / GroupBox | 最小高度 32px | 最小高度 36px | 最小高度 44px |
| `FWTextInputDensity` | TextBox / PasswordBox / AutoCompleteBox / RichTextBox | 最小高度 28px | 最小高度 32px | 最小高度 38px |
| `FWNumberBoxDensity` | NumberBox | 最小高度 28px | 最小高度 32px | 最小高度 38px |
| `FWMenuDensity` | MenuBar / Menu / MenuItem | 最小高度 28px | 最小高度 32px | 最小高度 38px |

#### 按类别详解

##### Buttons (按钮类)

**文件：** [FWButtonControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Buttons/FWButtonControls.cs), [FWDropDownButton.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Buttons/FWDropDownButton.cs), [FWToggleSplitButton.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Buttons/FWToggleSplitButton.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWButton` | `Button` | `Density` |
| `FWRepeatButton` | `RepeatButton` | `Density` |
| `FWHyperlinkButton` | `HyperlinkButton` | `Density` |
| `FWSplitButton` | `SplitButton` | `Density` |
| `FWCommandBar` | `CommandBar` | `Density` (FWCommandSurfaceDensity) |
| `FWAppBarButton` | `AppBarButton` | `Density`，支持 IsCompact 联动 |
| `FWAppBarToggleButton` | `AppBarToggleButton` | `Density`，支持 IsCompact 联动 |
| `FWAppBarSeparator` | `AppBarSeparator` | `Density`，自定义 MeasureOverride |
| `FWToolBar` | `ToolBar` | `Density`，内置 ItemsPanel |
| `FWToolBarTray` | `ToolBarTray` | `Density` |
| `FWDropDownButton` | `Button` | `Density`, `Flyout`, `ShowChevronArrow` |
| `FWToggleSplitButton` | `SplitButton` | `Density`, `IsChecked` |

`FWDropDownButton` 特殊方法：
- `OpenFlyout()` — 打开关联的 Flyout
- `CloseFlyout()` — 关闭关联的 Flyout

`FWToggleSplitButton` 特殊方法：
- `Toggle()` — 切换 IsChecked 状态
- 事件：`IsCheckedChanged`

##### Switches (开关类)

**文件：** [FWSwitchControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Switches/FWSwitchControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWToggleButton` | `ToggleButton` | `Density` |
| `FWToggleSwitch` | `ToggleSwitch` | `Density`, `Description` |

`FWToggleSwitch.Description` — 在开关标题下方显示的辅助说明文本。

##### Range (范围/进度类)

**文件：** [FWRangeControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Range/FWRangeControls.cs), [FWProgressRing.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Range/FWProgressRing.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWSlider` | `Slider` | `Density` |
| `FWRangeSlider` | `RangeSlider` | `Density` |
| `FWProgressBar` | `ProgressBar` | `Density`, `ShowPaused`, `ShowError` |
| `FWProgressRing` | `RangeBase` | `RingSize`, `IsActive`, `IsIndeterminate`, `StrokeThickness`, `ProgressBrush` |

`FWProgressRing` 是 FluentJalium 自有控件（Jalium.UI 未提供 ProgressRing），特点：
- 继承 `RangeBase`，支持 Value/Minimum/Maximum
- 自定义 `OnRender` 绘制弧线
- 支持 `FWProgressRingSize` 预设 (Small: 24px / Medium: 32px / Large: 48px)
- 不确定模式通过 `CompositionTarget.Rendering` 驱动旋转动画
- 最小弧度 42°，不确定弧度 96°

##### Selection (选择类)

**文件：** [FWSelectionControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Selection/FWSelectionControls.cs), [FWRatingControl.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Selection/FWRatingControl.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWCheckBox` | `CheckBox` | `Density` |
| `FWRadioButton` | `RadioButton` | `Density` |
| `FWComboBox` | `ComboBox` | `Density`，自动同步子项密度 |
| `FWComboBoxItem` | `ComboBoxItem` | `Density` |
| `FWRatingControl` | `Control` | `Value`, `MaxRating`, `PlaceholderValue`, `Caption`, `IsClearEnabled`, `IsReadOnly`, `ItemSpacing`, `RatingItemFontSize`, `RatingSize`, `Glyph`, `UnsetGlyph`, `GlyphFontFamily` |

`FWRatingControl` 是 FluentJalium 自有控件，特点：
- 自定义 `OnRender` 绘制星级 Glyph
- 支持鼠标悬停预览 (`_pointerPreviewValue`)
- 支持键盘导航 (Left/Right/Up/Down/Home/End/Delete/Back)
- `IsClearEnabled` 允许点击已选值取消选择
- `FWRatingControlSize` 预设 (Small / Medium / Large)
- 事件：`ValueChanged`

##### Collections (集合/表格类)

**文件：** [FWCollectionControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Collections/FWCollectionControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWListBox` | `ListBox` | `Density`，自动生成 FWListBoxItem |
| `FWListBoxItem` | `ListBoxItem` | `Density` |
| `FWListView` | `ListView` | `Density`，自动生成 FWListViewItem |
| `FWListViewItem` | `ListViewItem` | `Density` |
| `FWTreeView` | `TreeView` | `Density`，自动生成 FWTreeViewItem |
| `FWTreeViewItem` | `TreeViewItem` | `Density`，递归生成子项 |
| `FWDataGrid` | `DataGrid` | `Density` (FWDataGridDensity) |
| `FWTreeDataGrid` | `TreeDataGrid` | `Density` (FWDataGridDensity) |

集合控件密度分为列表指标 (`GetListDensityMetrics`) 和树形指标 (`GetTreeDensityMetrics`)，树形项通常更紧凑。

##### Navigation (导航类)

**文件：** [FWNavigationControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Navigation/FWNavigationControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWNavigationView` | `NavigationView` | `Density`，控制 OpenPaneLength / CompactPaneLength |
| `FWNavigationViewItem` | `NavigationViewItem` | `Density` |
| `FWNavigationViewItemHeader` | `NavigationViewItemHeader` | — |
| `FWNavigationViewItemSeparator` | `NavigationViewItemSeparator` | — |
| `FWTabControl` | `TabControl` | `Density`，控制 TabStripHeight |
| `FWTabItem` | `TabItem` | `Density` |
| `FWFrame` | `Frame` | — |

##### DateTime (日期时间类)

**文件：** [FWDateTimeControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/DateTime/FWDateTimeControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWDatePicker` | `DatePicker` | `Density` (FWDateTimePickerDensity) |
| `FWTimePicker` | `TimePicker` | `Density` (FWDateTimePickerDensity) |
| `FWCalendar` | `Calendar` | — |

##### Disclosure (展开/对话框类)

**文件：** [FWDisclosureControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Disclosure/FWDisclosureControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWExpander` | `Expander` | `Density` (FWDisclosureDensity) |
| `FWToolTip` | `ToolTip` | — |
| `FWContentDialog` | `ContentDialog` | — |
| `FWGroupBox` | `GroupBox` | `Density` |

##### TextInput (文本输入类)

**文件：** [FWTextInputControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/TextInput/FWTextInputControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWTextBox` | `TextBox` | `Density` (FWTextInputDensity) |
| `FWPasswordBox` | `PasswordBox` | `Density` |
| `FWNumberBox` | `NumberBox` | `Density` (FWNumberBoxDensity) |
| `FWAutoCompleteBox` | `AutoCompleteBox` | `Density` |
| `FWRichTextBox` | `RichTextBox` | `Density` |

##### Menus (菜单/弹出类)

**文件：** [FWMenuControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Menus/FWMenuControls.cs), [FWFlyouts.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Menus/FWFlyouts.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWMenuBar` | `MenuBar` | `Density` (FWMenuDensity) |
| `FWMenuBarItem` | `MenuBarItem` | `Density` |
| `FWMenu` | `Menu` | `Density` |
| `FWMenuItem` | `MenuItem` | `Density` |
| `FWContextMenu` | `ContextMenu` | — |
| `FWMenuFlyoutItem` | `MenuFlyoutItem` | `Density` |
| `FWToggleMenuFlyoutItem` | `ToggleMenuFlyoutItem` | `Density` |
| `FWMenuFlyoutSeparator` | `MenuFlyoutSeparator` | — |
| `FWMenuFlyout` | Flyout 基类 | — |
| `FWCommandBarFlyout` | Flyout 基类 | — |
| `FWMenuFlyoutSubItem` | `MenuFlyoutItem` | — |

##### Status (状态/通知类)

**文件：** [FWInfoBadge.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Status/FWInfoBadge.cs), [FWNotificationStatusControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Status/FWNotificationStatusControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWInfoBadge` | `Control` | `Value`, `MaxValue`, `IconGlyph` 等 |
| `FWInfoBar` | `InfoBar` | — |
| `FWToastNotificationItem` | `ToastNotificationItem` | — |
| `FWToastNotificationHost` | `ToastNotificationHost` | — |
| `FWStatusBar` | `StatusBar` | — |
| `FWStatusBarItem` | `StatusBarItem` | — |

`FWInfoBadge` 是 FluentJalium 自有控件，用于显示数值或图标角标。

##### Visuals (视觉/图标基础类)

**文件：** [FWVisualControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Visuals/FWVisualControls.cs)

| 控件 | 基类 |
|------|------|
| `FWImage` | `Image` |
| `FWFontIcon` | `FontIcon` |
| `FWSymbolIcon` | `SymbolIcon` |
| `FWPathIcon` | `PathIcon` |
| `FWViewbox` | `Viewbox` |
| `FWLabel` | `Label` |
| `FWSeparator` | `Separator` |

##### Interaction (交互/滚动类)

**文件：** [FWInteractionControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Interaction/FWInteractionControls.cs)

| 控件 | 基类 |
|------|------|
| `FWScrollViewer` | `ScrollViewer` |
| `FWSwipeControl` | `SwipeControl` |
| `FWGridSplitter` | `GridSplitter` |

##### Input (输入/媒体类)

**文件：** [FWInputMediaControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Input/FWInputMediaControls.cs)

| 控件 | 基类 | 特有属性 |
|------|------|----------|
| `FWColorPicker` | `ColorPicker` | — |
| `FWInkCanvas` | `InkCanvas` | `BorderBrush`, `BorderThickness`, `CornerRadius` |
| `FWInkPresenter` | `InkPresenter` | — |
| `FWMediaElement` | `MediaElement` | `BorderBrush`, `BorderThickness`, `CornerRadius` |

##### Layout (布局基础类)

**文件：** [FWLayoutControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Layout/FWLayoutControls.cs)

| 控件 | 基类 |
|------|------|
| `FWTextBlock` | `TextBlock` |
| `FWAccessText` | `AccessText` |
| `FWBorder` | `Border` |
| `FWContentControl` | `ContentControl` |
| `FWTransitioningContentControl` | `TransitioningContentControl` |
| `FWContentPresenter` | `ContentPresenter` |
| `FWStackPanel` | `StackPanel` |
| `FWWrapPanel` | `WrapPanel` |
| `FWGrid` | `Grid` |

##### Materials (材质效果类)

**文件：** [FWMaterials.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Materials/FWMaterials.cs)

| 类型 | 说明 |
|------|------|
| `FWFluentMaterialKind` | 材质类型枚举 |
| `FWFluentWindowBackdropKind` | 窗口背景类型枚举 |
| `FWFluentMaterialRecipe` | 材质配方，定义材质参数 |
| `FWFluentMaterialSurface` | 材质表面控件，继承 `Border`，实现 `IFluentJaliumControl` |

`FWFluentMaterialSurface` 关键属性：`MaterialKind`, `TintColor`, `TintOpacity`, `BackgroundOpacity` 等。

##### Selectors (选择器类)

**文件：** [FWSelectorControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Selectors/FWSelectorControls.cs)

| 控件 | 基类 |
|------|------|
| `FWTreeSelector` | `TreeSelector` |
| `FWTreeSelectorItem` | `TreeSelectorItem` |

##### DataInspectors (数据检查器类)

**文件：** [FWDataInspectorControls.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/DataInspectors/FWDataInspectorControls.cs)

| 控件 | 基类 |
|------|------|
| `FWPropertyGrid` | `PropertyGrid` |
| `FWDiffViewer` | `DiffViewer` |
| `FWHexEditor` | `HexEditor` |
| `FWJsonTreeViewer` | `JsonTreeViewer` |

##### Motion (动画类)

**文件：** [FWConnectedAnimation.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Controls/Motion/FWConnectedAnimation.cs)

| 类型 | 说明 |
|------|------|
| `FWConnectedAnimationPlan` | 动画计划，定义源/目标元素 |
| `FWConnectedAnimationConfiguration` | 动画配置 |
| `FWConnectedAnimationOptions` | 动画选项 |
| `FWConnectedAnimationService` | 动画服务，提供 `PrepareAnimation` 和 `StartAnimation` |

---

### 4.3 图标层 (Icon)

**文件位置：** [src/FluentJalium/Icon/](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Icon/)

FluentJalium 提供了完整的图标系统，支持三种图标字体：

| 字体族 | 用途 |
|--------|------|
| `FluentSystemIcons-Regular` | Fluent UI System Icons 常规样式 |
| `FluentSystemIcons-Filled` | Fluent UI System Icons 填充样式 |
| `Segoe Fluent Icons` | Windows Segoe 兼容图标 |

#### FluentIcon 控件

**文件：** [FluentIcon.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Icon/FluentIcon.cs)

继承自 `FontIcon`，核心图标显示控件。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Icon` | `object?` | `FluentIconRegular.Empty` | 图标枚举值，支持 FluentIconRegular / FluentIconFilled / SegoeFluentIcon / int |
| `Filled` | `bool` | `false` | 是否使用填充字形 |
| `IconSet` | `FluentIconSet` | `Regular` | 图标集选择 |
| `Size` | `double` | `20.0` | 图标渲染尺寸 |

图标解析逻辑 (`ResolveIcon`)：

```
Icon 值类型判断:
  ├─ FluentIconFilled → 使用 FilledFontFamily
  ├─ SegoeFluentIcon → 使用 SegoeFontFamily
  ├─ FluentIconRegular → 根据 Filled/IconSet 决定 Regular 或 Filled
  ├─ int (码点) → 根据 IconSet/Filled 决定字体族
  └─ 其他 → 空字形
```

#### FluentIconFactory 工厂

**文件：** [FluentIconFactory.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Icon/FluentIconFactory.cs)

提供便捷的静态工厂方法创建 FluentIcon 实例：

| 方法 | 说明 |
|------|------|
| `Create(FluentIconRegular, ...)` | 创建常规图标 |
| `Create(FluentIconFilled, ...)` | 创建填充图标 |
| `Create(SegoeFluentIcon, ...)` | 创建 Segoe 兼容图标 |
| `Regular(...)` | 显式创建常规图标 |
| `Filled(...)` | 显式创建填充图标 |
| `Segoe(...)` | 显式创建 Segoe 图标 |

#### FluentIconExtensions 扩展方法

**文件：** [FluentIconExtensions.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Icon/FluentIconExtensions.cs)

| 方法 | 说明 |
|------|------|
| `GetGlyph(this FluentIconRegular)` | 获取常规图标字形字符串 |
| `GetGlyph(this FluentIconFilled)` | 获取填充图标字形字符串 |
| `GetGlyph(this SegoeFluentIcon)` | 获取 Segoe 图标字形字符串 |
| `GetString(...)` | GetGlyph 的别名 |
| `ToIcon(...)` | 将枚举值转换为 FluentIcon 元素 |

#### FluentIconSet 枚举

```csharp
public enum FluentIconSet
{
    Regular,  // Fluent UI System Icons Regular
    Filled,   // Fluent UI System Icons Filled
    Segoe     // Windows Segoe Fluent Icons
}
```

#### SegoeFluentIcon 枚举

**文件：** [SegoeFluentIcon.cs](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Icon/SegoeFluentIcon.cs)

提供 Windows Segoe Fluent Icons 兼容字形，包含约 50 个常用图标码点（如 `GlobalNavigationButton = 0xE700`, `Save = 0xE74E` 等）。

---

### 4.4 主题资源层 (Themes)

**文件位置：** [src/FluentJalium/Themes/](file:///d:/github/Jalium/FluentJalium/src/FluentJalium/Themes/)

主题资源使用 `.jalxaml` 格式（Jalium.UI 的 XAML 变体），由 `jalxamlc` 编译器处理。

#### 资源文件清单

| 文件 | 说明 |
|------|------|
| `Generic.jalxaml` | 通用主题入口，合并所有子资源 |
| `FluentResources.jalxaml` | 基础资源定义 |
| `FluentColors.jalxaml` | 颜色令牌定义 |
| `FluentTypography.jalxaml` | 排版令牌定义 |
| `FluentGeometry.jalxaml` | 几何图形定义 |
| `FluentMaterials.jalxaml` | 材质效果定义 |
| `FluentMotion.jalxaml` | 动效定义 |
| `Controls/FluentControls.jalxaml` | 控件样式总入口 |
| `Controls/Button.jalxaml` | 按钮样式 |
| `Controls/ToggleControls.jalxaml` | 开关样式 |
| `Controls/RangeControls.jalxaml` | 范围控件样式 |
| `Controls/SelectionControls.jalxaml` | 选择控件样式 |
| `Controls/CollectionControls.jalxaml` | 集合控件样式 |
| `Controls/NavigationControls.jalxaml` | 导航控件样式 |
| `Controls/DateTimeControls.jalxaml` | 日期时间控件样式 |
| `Controls/DisclosureControls.jalxaml` | 展开控件样式 |
| `Controls/TextControls.jalxaml` | 文本输入控件样式 |
| `Controls/MenuControls.jalxaml` | 菜单控件样式 |
| `Controls/StatusControls.jalxaml` | 状态控件样式 |
| `Controls/VisualControls.jalxaml` | 视觉控件样式 |
| `Controls/InputMediaControls.jalxaml` | 输入媒体控件样式 |
| `Controls/InteractionControls.jalxaml` | 交互控件样式 |
| `Controls/ContentLayoutControls.jalxaml` | 内容布局控件样式 |
| `Controls/AdvancedControls.jalxaml` | 高级控件样式 |

#### JALXAML 构建集成

FluentJalium.csproj 中配置了 JALXAML 页面项和编译器路径：

```xml
<ItemGroup>
  <JalxamlPage Include="Themes\**\*.jalxaml" />
</ItemGroup>
```

当 `UseJaliumSourceReferences=true` 时，使用本地 Jalium.UI 源码中的 `jalxamlc.exe`；否则使用 NuGet 包中的编译器。

---

## 5. Gallery 示例应用

**文件位置：** [samples/FluentJalium.Gallery/](file:///d:/github/Jalium/FluentJalium/samples/FluentJalium.Gallery/)

Gallery 是一个 WPF 桌面应用，用于视觉验证所有 FW 控件的 Fluent 样式效果。

### 应用架构

```
Program.cs (入口)
  └─ Application + FluentThemeManager.Apply()
      └─ MainWindow
          └─ GalleryShell (导航外壳)
              ├─ FWNavigationView (侧边栏)
              │   └─ GalleryNavigationGroup / GalleryPage
              └─ FWFrame (内容区)
                  └─ GalleryHostPage
                      └─ 具体 Gallery 页面
```

### 核心组件

| 组件 | 文件 | 职责 |
|------|------|------|
| `Program` | Program.cs | 应用入口，初始化 Jalium Application 并应用主题 |
| `MainWindow` | MainWindow.cs | 主窗口，承载 GalleryShell，处理主题切换 |
| `GalleryShell` | Shell/GalleryShell.cs | 导航外壳，管理 NavigationView + Frame + 搜索 |
| `GalleryHostPage` | Shell/GalleryHostPage.cs | 页面宿主，根据参数创建具体页面内容 |
| `GallerySearchEmptyPage` | Shell/GallerySearchEmptyPage.cs | 搜索无结果时的空状态页面 |
| `GalleryCatalogService` | Services/GalleryCatalogService.cs | 页面目录服务，创建 GalleryCatalog |
| `GalleryThemeResources` | Services/GalleryThemeResources.cs | Gallery 专用主题资源（笔刷/颜色） |

### 数据模型

| 模型 | 文件 | 说明 |
|------|------|------|
| `GalleryCatalog` | Models/GalleryCatalog.cs | 页面目录，包含所有导航组和页面 |
| `GalleryNavigationGroup` | Models/GalleryNavigationGroup.cs | 导航菜单中的页面组 |
| `GalleryPage` | Models/GalleryPage.cs | 单个页面定义（标题、描述、图标、创建方法） |
| `GalleryPageInfo` | Models/GalleryPageInfo.cs | 页面元信息 |
| `GalleryPageStatus` | Models/GalleryPageStatus.cs | 页面状态枚举 (Stable / Experimental / Prerelease) |
| `GallerySampleData` | Models/GallerySampleData.cs | 示例数据提供者 |
| `GallerySampleCard` | Controls/GallerySampleCard.cs | 示例卡片控件 |

### Gallery 页面清单

| 页面 | 说明 |
|------|------|
| GalleryButtonsPage | 按钮控件示例 |
| GallerySwitchesPage | 开关控件示例 |
| GalleryRangePage | 范围/进度控件示例 |
| GallerySelectionPage | 选择控件示例 |
| GalleryCollectionsPage | 集合/表格控件示例 |
| GalleryNavigationPage | 导航控件示例 |
| GalleryDateTimePage | 日期时间控件示例 |
| GalleryDisclosurePage | 展开控件示例 |
| GalleryTextInputPage | 文本输入控件示例 |
| GalleryMenusPage | 菜单控件示例 |
| GalleryStatusPage | 状态/通知控件示例 |
| GalleryVisualsPage | 视觉控件示例 |
| GalleryInteractionPage | 交互控件示例 |
| GalleryInputMediaPage | 输入/媒体控件示例 |
| GalleryContentLayoutPage | 内容布局控件示例 |
| GalleryMaterialsPage | 材质效果示例 |
| GalleryColorsPage | 颜色令牌示例 |
| GalleryTypographyPage | 排版令牌示例 |
| GalleryGeometryPage | 几何图形示例 |
| GalleryMotionPage | 动效示例 |
| GalleryMotionTokensPage | 动效令牌示例 |
| GalleryDataInspectorsPage | 数据检查器示例 |
| GallerySelectorsPropertiesPage | 选择器/属性控件示例 |
| GalleryStateMatrixPage | 状态矩阵示例 |
| GalleryThemeArchitecturePage | 主题架构说明 |
| GalleryOverviewPage | 总览页面 |
| GalleryWindowBackdropsPage | 窗口背景示例 |

---

## 6. 测试体系

**文件位置：** [tests/FluentJalium.Tests/](file:///d:/github/Jalium/FluentJalium/tests/FluentJalium.Tests/)

### 测试框架

| 组件 | 版本 |
|------|------|
| xUnit | 2.9.3 |
| xunit.runner.visualstudio | 3.1.5 |
| Microsoft.NET.Test.Sdk | 18.3.0 |
| coverlet.collector | 8.0.1 |

### 测试基础设施

`ApplicationCollectionFixture` 定义了测试集合固定项，确保所有测试共享同一个 Jalium Application 实例，避免重复初始化。

### 测试文件清单

| 测试文件 | 测试目标 |
|----------|----------|
| FluentThemeManagerTests | 主题应用、变体切换、强调色、样式继承 |
| FluentIconTests | 图标转换、字体族切换、属性设置 |
| FluentButtonControlsTests | 按钮控件密度和样式 |
| FluentSwitchControlsTests | 开关控件密度和样式 |
| FluentRangeProgressTests | 范围/进度控件密度、资源管理 |
| FluentSelectionControlsTests | 选择控件密度、状态管理 |
| FluentCollectionTableTests | 集合/表格控件密度 |
| FluentNavigationControlsTests | 导航控件密度和功能 |
| FluentCommandToolbarTests | 命令栏控件 |
| FluentDateTimeControlsTests | 日期时间控件 |
| FluentDisclosureControlsTests | 展开控件 |
| FluentTextInputControlsTests | 文本输入控件 |
| FluentMenuControlsTests | 菜单控件 |
| FluentNotificationStatusTests | 通知状态控件 |
| FluentVisualControlsTests | 视觉控件 |
| FluentDataInspectorTests | 数据检查器 |
| FluentInputMediaControlsTests | 输入媒体控件 |
| FluentInteractionControlsTests | 交互控件 |
| FluentMaterialRecipeTests | 材质配方 |
| FluentConnectedAnimationTests | 连接动画 |
| FluentAdvancedSelectionPropertyTests | 高级选择/属性控件 |
| FluentStateMatrixControlsTests | 状态矩阵控件 |
| FluentOverviewThemeTests | 总览主题 |
| FluentStateMatrixControlsTests | 状态矩阵控件 |

---

## 7. 依赖关系

### 项目间依赖

```
FluentJalium.Gallery ──→ FluentJalium ──→ Jalium.UI (Core/Media/Input/Controls/Xaml/Interop)
                                                    ↑
FluentJalium.Tests ──→ FluentJalium ────────────────┘
```

### FluentJalium 对 Jalium.UI 的依赖

FluentJalium 支持两种引用模式，由 `UseJaliumSourceReferences` 属性控制：

| 模式 | 条件 | 说明 |
|------|------|------|
| **源码引用** | `UseJaliumSourceReferences=true`（默认） | 直接引用 `../Jalium.UI/` 源码树中的项目 |
| **包引用** | `UseJaliumSourceReferences=false` | 引用 NuGet 上的 Jalium.UI 包 (26.10.4) |

源码引用模式下引用的 Jalium.UI 项目：

| 项目 | 说明 |
|------|------|
| Jalium.UI.Core | 核心基础类型 |
| Jalium.UI.Media | 媒体和绘图类型 |
| Jalium.UI.Input | 输入相关类型 |
| Jalium.UI.Interop | 互操作层 |
| Jalium.UI.Controls | 控件基类 |
| Jalium.UI.Xaml | XAML 基础设施 |
| Jalium.UI.Xaml.SourceGenerator | XAML 源生成器 (Analyzer) |
| Jalium.UI.Build | 构建工具 (jalxamlc) |

### Gallery 应用的额外依赖

| 依赖 | 说明 |
|------|------|
| Jalium.UI.Desktop | 桌面应用宿主（源码引用模式） |
| Jalium.UI.Interop (NuGet) | 原生运行时二进制（用于复制 native DLL） |

### 测试项目依赖

| 依赖 | 版本 | 说明 |
|------|------|------|
| Microsoft.NET.Test.Sdk | 18.3.0 | 测试 SDK |
| xUnit | 2.9.3 | 测试框架 |
| xunit.runner.visualstudio | 3.1.5 | VS 测试运行器 |
| coverlet.collector | 8.0.1 | 代码覆盖率 |

---

## 8. 构建与运行

### 前置条件

- .NET 10 SDK (10.0.201+)
- 如使用源码引用模式：`../Jalium.UI/` 源码树需存在

### 构建命令

```powershell
# 运行测试
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj -c Debug

# 构建 Gallery 示例应用
dotnet build samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj -c Debug

# 使用 NuGet 包引用构建（不需要 Jalium.UI 源码）
dotnet build FluentJalium.slnx -c Debug /p:UseJaliumSourceReferences=false
```

### 运行 Gallery

```powershell
# 默认运行（源码引用 + NuGet 原生运行时）
dotnet run --project samples/FluentJalium.Gallery -c Debug

# 禁用 NuGet 原生运行时回退（重建 Jalium.UI 原生二进制后使用）
dotnet run --project samples/FluentJalium.Gallery -c Debug /p:UseJaliumPackageNativeRuntime=false
```

### 关键构建属性

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `JaliumPlatform` | 空 | 设置为 `windows` 时目标框架变为 `net10.0-windows` |
| `UseJaliumSourceReferences` | `true` | 是否使用 Jalium.UI 源码引用 |
| `JaliumSourceRoot` | `../Jalium.UI/` | Jalium.UI 源码根目录 |
| `JaliumPackageVersion` | `26.10.4` | Jalium.UI NuGet 包版本 |
| `UseJaliumPackageNativeRuntime` | `true`（源码引用时） | 是否从 NuGet 包复制原生运行时 |
| `JaliumNativePackRid` | `win-x64` | 原生运行时 RID |
| `EnableJalxamlCodeGeneration` | `false` | JALXAML 代码生成 |
| `EnableJalxamlRazorTransform` | `false` | JALXAML Razor 转换 |

### 主题使用方式

**代码方式（推荐）：**

```csharp
var app = new Application();
FluentThemeManager.Apply(app);

// 或使用自定义选项
FluentThemeManager.Apply(app, new FluentThemeOptions
{
    Theme = FluentThemeVariant.Light,
    AccentColor = Color.FromRgb(0x00, 0x78, 0xD4),
    DisplayFontFamily = "Segoe UI Variable Display",
    BodyFontFamily = "Segoe UI Variable Text"
});
```

**JALXAML 资源字典方式：**

```xml
<ResourceDictionary Source="/FluentJalium;component/Themes/Generic.jalxaml" />
```

**运行时切换主题：**

```csharp
FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
FluentThemeManager.ApplyAccent(Color.FromRgb(0xFF, 0x00, 0x00));
FluentThemeManager.ApplyTypography("Segoe UI", "Segoe UI", "Cascadia Code");
```

---

## 9. 设计模式与约定

### 控件命名约定

| 约定 | 示例 | 说明 |
|------|------|------|
| FW 前缀 | `FWButton`, `FWComboBox` | 所有 FluentJalium 控件使用 FW 前缀 |
| 密度枚举 | `FWButtonDensity`, `FWSelectionDensity` | 按控件类别定义密度枚举 |
| 密度属性 | `Density` | 所有支持密度的控件统一使用 `Density` 属性名 |
| 密度指标方法 | `GetDensityMetrics(...)` | 静态方法，返回密度指标元组 |

### 控件实现模板

```csharp
public class FWXxxControl : XxxControl, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWXxxDensity),
            typeof(FWXxxControl), new PropertyMetadata(FWXxxDensity.Comfortable, OnDensityChanged));

    public FWXxxControl()
    {
        ApplyDensity(this, Density);
    }

    public FWXxxDensity Density
    {
        get => (FWXxxDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWXxxControl control && e.NewValue is FWXxxDensity density)
            ApplyDensity(control, density);
    }

    private static void ApplyDensity(FWXxxControl control, FWXxxDensity density)
    {
        // 应用密度指标到控件属性
    }
}
```

### 容器控件模式

集合类控件（如 `FWComboBox`, `FWListBox`）通常重写 `GetContainerForItem` 以生成对应的 FW 子项：

```csharp
protected override FrameworkElement GetContainerForItem(object item)
{
    return new FWComboBoxItem { Density = Density };
}
```

### 密度指标共享

相关控件共享密度指标计算方法，避免重复定义：

```csharp
// FWRangeSlider 复用 FWSlider 的指标
private static void ApplyDensity(FWRangeSlider slider, FWRangeDensity density)
{
    var (minHeight, height) = FWSlider.GetDensityMetrics(density);
    slider.MinHeight = minHeight;
    slider.Height = height;
}
```

---

## 10. FW 控件完整清单

| 批次 | 控件 | 基类 |
|------|------|------|
| 1 | FWButton, FWRepeatButton, FWHyperlinkButton, FWDropDownButton, FWSplitButton, FWToggleSplitButton, FWAppBarButton, FWAppBarToggleButton, FWAppBarSeparator | Button / RepeatButton / HyperlinkButton / Button / SplitButton / SplitButton / AppBarButton / AppBarToggleButton / AppBarSeparator |
| 2 | FWToggleButton, FWToggleSwitch | ToggleButton / ToggleSwitch |
| 3 | FWSlider, FWRangeSlider, FWProgressBar, FWProgressRing | Slider / RangeSlider / ProgressBar / RangeBase |
| 4 | FWCheckBox, FWRadioButton, FWComboBox, FWComboBoxItem | CheckBox / RadioButton / ComboBox / ComboBoxItem |
| 5 | FWListBox, FWListBoxItem, FWListView, FWListViewItem, FWTreeView, FWTreeViewItem, FWDataGrid, FWTreeDataGrid | ListBox / ListBoxItem / ListView / ListViewItem / TreeView / TreeViewItem / DataGrid / TreeDataGrid |
| 6 | FWNavigationView, FWNavigationViewItem, FWNavigationViewItemHeader, FWNavigationViewItemSeparator, FWTabControl, FWTabItem, FWFrame | NavigationView / NavigationViewItem / NavigationViewItemHeader / NavigationViewItemSeparator / TabControl / TabItem / Frame |
| 7 | FWDatePicker, FWTimePicker, FWCalendar | DatePicker / TimePicker / Calendar |
| 8 | FWInfoBar, FWInfoBadge, FWToastNotificationItem, FWToastNotificationHost, FWStatusBar, FWStatusBarItem | InfoBar / Control / ToastNotificationItem / ToastNotificationHost / StatusBar / StatusBarItem |
| 9 | FWTextBox, FWPasswordBox, FWNumberBox, FWAutoCompleteBox, FWRichTextBox | TextBox / PasswordBox / NumberBox / AutoCompleteBox / RichTextBox |
| 10 | FWMenuBar, FWMenuBarItem, FWMenu, FWMenuItem, FWContextMenu, FWMenuFlyoutItem, FWToggleMenuFlyoutItem, FWMenuFlyoutSeparator, FWMenuFlyoutSubItem | MenuBar / MenuBarItem / Menu / MenuItem / ContextMenu / MenuFlyoutItem / ToggleMenuFlyoutItem / MenuFlyoutSeparator / MenuFlyoutItem |
| 11 | FWExpander, FWToolTip, FWContentDialog, FWGroupBox | Expander / ToolTip / ContentDialog / GroupBox |
| 12 | FWImage, FWFontIcon, FWSymbolIcon, FWPathIcon, FWViewbox, FWLabel, FWSeparator | Image / FontIcon / SymbolIcon / PathIcon / Viewbox / Label / Separator |
| 13 | FWScrollViewer, FWSwipeControl, FWGridSplitter | ScrollViewer / SwipeControl / GridSplitter |
| 14 | FWColorPicker, FWInkCanvas, FWInkPresenter, FWMediaElement | ColorPicker / InkCanvas / InkPresenter / MediaElement |
| 15 | FWTextBlock, FWAccessText, FWBorder, FWContentControl, FWTransitioningContentControl, FWContentPresenter, FWStackPanel, FWWrapPanel, FWGrid | TextBlock / AccessText / Border / ContentControl / TransitioningContentControl / ContentPresenter / StackPanel / WrapPanel / Grid |
| — | FWFluentMaterialSurface, FWTreeSelector, FWTreeSelectorItem, FWPropertyGrid, FWDiffViewer, FWHexEditor, FWJsonTreeViewer, FWCommandBar, FWToolBar, FWToolBarTray | Border / TreeSelector / TreeSelectorItem / PropertyGrid / DiffViewer / HexEditor / JsonTreeViewer / CommandBar / ToolBar / ToolBarTray |
