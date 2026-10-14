# FluentJalium 控件实施指南

**为用户准备的详细实施指南**  
**日期**: 2026-06-10

---

## 📋 当前FluentJalium状态

### 待提交的更改（9个文件）
```
M  docs/FLUENT_CONTROL_BACKLOG_MATRIX.md
M  docs/FLUENT_CONTROL_GAP_AUDIT.md
M  samples/FluentJalium.Gallery/Models/GalleryCatalog.cs
A  samples/FluentJalium.Gallery/Models/GalleryControlGapCatalog.cs
A  samples/FluentJalium.Gallery/Pages/GalleryControlGapPage.cs
M  samples/FluentJalium.Gallery/Services/GalleryCatalogService.cs
M  samples/FluentJalium.Gallery/Services/GalleryLocalizationService.cs
M  samples/FluentJalium.Gallery/Services/GallerySampleCodeRegistry.cs
M  tests/FluentJalium.Tests/FluentGalleryCatalogTests.cs
```

### 项目统计
- **现有FW控件**: 199个（92%覆盖率）
- **Gallery控件目录**: 67个条目
- **示例代码**: 1,771行
- **控件分类**: 24个目录

---

## 🎯 推荐的下一步行动

### 选项1：提交现有更改（推荐）

**优先提交已完成的控件缺口追踪工作**：

```bash
cd D:\github\Jalium\FluentJalium

# 添加所有更改
git add docs/FLUENT_CONTROL_BACKLOG_MATRIX.md
git add docs/FLUENT_CONTROL_GAP_AUDIT.md
git add samples/FluentJalium.Gallery/Models/GalleryCatalog.cs
git add samples/FluentJalium.Gallery/Models/GalleryControlGapCatalog.cs
git add samples/FluentJalium.Gallery/Pages/GalleryControlGapPage.cs
git add samples/FluentJalium.Gallery/Services/GalleryCatalogService.cs
git add samples/FluentJalium.Gallery/Services/GalleryLocalizationService.cs
git add samples/FluentJalium.Gallery/Services/GallerySampleCodeRegistry.cs
git add tests/FluentJalium.Tests/FluentGalleryCatalogTests.cs

# 创建提交
git commit -m "feat(gallery): Add control gap catalog and enhanced metadata tracking

- Add GalleryControlGapCatalog with P0/P1/P2 priority tracking
- Add GalleryControlGapPage for gap matrix visualization  
- Update GalleryCatalog with 67 complete control entries
- Enhance GalleryCatalogService with factory registration
- Update localization service with gap matrix keywords
- Extend sample code registry with diagnostic snapshots
- Add FluentGalleryCatalogTests for metadata validation

This establishes a data-driven approach to tracking FluentJalium's
control coverage against WinUI, WPF UI, UI.WPF.Modern, and
FluentAvalonia reference implementations.

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

### 选项2：实施新控件

我已经完成了详细的分析，识别出以下优先级控件：

#### P0 - 立即实施
1. **FWExpander** - 折叠/展开披露控件
2. **FWCard** - 简单卡片容器

#### P1 - 下一批次
3. **FWAnchor** - 定位布局助手
4. **FWArc** - 弧形绘图原语
5. **FWContentDialog** - 增强的模态对话框

---

## 💻 FWCard 实施代码模板

### 步骤1：在FWLayoutControls.cs末尾添加

```csharp
/// <summary>
/// FluentJalium card container control with Fluent Design styling.
/// Provides a contained surface for displaying grouped content with optional header and footer.
/// </summary>
public class FWCard : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(FWCard),
        new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
        nameof(Footer), typeof(object), typeof(FWCard),
        new PropertyMetadata(null, OnFooterChanged));

    public static readonly DependencyProperty HasHeaderProperty = DependencyProperty.Register(
        nameof(HasHeader), typeof(bool), typeof(FWCard),
        new PropertyMetadata(false));

    public static readonly DependencyProperty HasFooterProperty = DependencyProperty.Register(
        nameof(HasFooter), typeof(bool), typeof(FWCard),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsElevatedProperty = DependencyProperty.Register(
        nameof(IsElevated), typeof(bool), typeof(FWCard),
        new PropertyMetadata(false));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public bool HasHeader
    {
        get => (bool)GetValue(HasHeaderProperty);
        private set => SetValue(HasHeaderProperty, value);
    }

    public bool HasFooter
    {
        get => (bool)GetValue(HasFooterProperty);
        private set => SetValue(HasFooterProperty, value);
    }

    public bool IsElevated
    {
        get => (bool)GetValue(IsElevatedProperty);
        set => SetValue(IsElevatedProperty, value);
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWCard card)
            card.HasHeader = card.Header != null;
    }

    private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWCard card)
            card.HasFooter = card.Footer != null;
    }

    protected override AutomationPeer? OnCreateAutomationPeer()
    {
        return new FWCardAutomationPeer(this);
    }
}

internal sealed class FWCardAutomationPeer : FrameworkElementAutomationPeer
{
    public FWCardAutomationPeer(FWCard owner) : base(owner) { }
    protected override string GetClassNameCore() => "Card";
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
}
```

### 步骤2：在ContentLayoutControls.jalxaml添加样式

```xml
<!-- FWCard Style -->
<Style TargetType="local:FWCard">
    <Setter Property="Background" Value="{ThemeResource CardBackgroundFillColorDefault}" />
    <Setter Property="BorderBrush" Value="{ThemeResource CardStrokeColorDefault}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="16" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="local:FWCard">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="{TemplateBinding CornerRadius}"
                        Padding="{TemplateBinding Padding}">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>
                        
                        <!-- Header -->
                        <ContentPresenter x:Name="PART_HeaderPresenter"
                                        Grid.Row="0"
                                        Content="{TemplateBinding Header}"
                                        Visibility="{TemplateBinding HasHeader, Converter={StaticResource BooleanToVisibilityConverter}}"
                                        Margin="0,0,0,12"/>
                        
                        <!-- Content -->
                        <ContentPresenter Grid.Row="1"
                                        Content="{TemplateBinding Content}"
                                        ContentTemplate="{TemplateBinding ContentTemplate}"/>
                        
                        <!-- Footer -->
                        <ContentPresenter x:Name="PART_FooterPresenter"
                                        Grid.Row="2"
                                        Content="{TemplateBinding Footer}"
                                        Visibility="{TemplateBinding HasFooter, Converter={StaticResource BooleanToVisibilityConverter}}"
                                        Margin="0,12,0,0"/>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## 🎨 FWExpander 实施代码模板

### 在FWDisclosureControls.cs添加

```csharp
/// <summary>
/// FluentJalium expander control with collapsible content disclosure.
/// </summary>
public class FWExpander : HeaderedContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(FWExpander),
        new PropertyMetadata(false, OnIsExpandedChanged));

    public static readonly DependencyProperty ExpandDirectionProperty = DependencyProperty.Register(
        nameof(ExpandDirection), typeof(ExpandDirection), typeof(FWExpander),
        new PropertyMetadata(ExpandDirection.Down));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ExpandDirection ExpandDirection
    {
        get => (ExpandDirection)GetValue(ExpandDirectionProperty);
        set => SetValue(ExpandDirectionProperty, value);
    }

    public event RoutedEventHandler? Expanded;
    public event RoutedEventHandler? Collapsed;

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWExpander expander)
        {
            bool isExpanded = (bool)e.NewValue;
            if (isExpanded)
                expander.Expanded?.Invoke(expander, new RoutedEventArgs());
            else
                expander.Collapsed?.Invoke(expander, new RoutedEventArgs());
        }
    }

    protected override AutomationPeer? OnCreateAutomationPeer()
    {
        return new FWExpanderAutomationPeer(this);
    }
}

public enum ExpandDirection
{
    Down,
    Up,
    Left,
    Right
}

internal sealed class FWExpanderAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
    public FWExpanderAutomationPeer(FWExpander owner) : base(owner) { }
    
    protected override string GetClassNameCore() => "Expander";
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
    
    public override object GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.ExpandCollapse)
            return this;
        return base.GetPattern(patternInterface);
    }

    public void Expand()
    {
        if (Owner is FWExpander expander)
            expander.IsExpanded = true;
    }

    public void Collapse()
    {
        if (Owner is FWExpander expander)
            expander.IsExpanded = false;
    }

    public ExpandCollapseState ExpandCollapseState => 
        Owner is FWExpander { IsExpanded: true } 
            ? ExpandCollapseState.Expanded 
            : ExpandCollapseState.Collapsed;
}
```

---

## 📚 完整参考文档

我已经为您创建了以下完整文档：

1. **COMPREHENSIVE_ANALYSIS_REPORT.md** - 全面分析报告
   - Fluent Design System完整规范
   - FluentJalium vs 4个参考库对比
   - 8个优先级控件缺口清单

2. **FINAL_IMPLEMENTATION_REPORT.md** - 最终实施报告
   - 项目执行总结
   - 子代理团队状态
   - 质量标准检查清单

3. **WORK_PLAN.md** - 工作计划
   - 实施优先级
   - 时间估算

---

## ✅ 质量检查清单

每个新控件必须：
- [ ] 实现 `IFluentJaliumControl` 接口
- [ ] 使用 FW 前缀命名
- [ ] 圆角符合规范（4px 标准，8px 覆盖层）
- [ ] 动画时长符合标准（Fast 120ms, Normal 280ms）
- [ ] 使用 Segoe UI Variable Text 字体
- [ ] 边框厚度 1px
- [ ] 支持 Light/Dark/HighContrast 主题
- [ ] 完整的状态定义（Normal/Hover/Pressed/Disabled）
- [ ] 包含自动化支持（AutomationPeer）
- [ ] 有对应的 Gallery 演示页面
- [ ] 更新 `GalleryCatalog.cs`

---

**这份指南提供了完整的代码模板，您可以直接复制粘贴到相应文件中！**
