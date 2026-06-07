# Getting Started with FluentJalium

A comprehensive guide to integrating and using FluentJalium in your Jalium.UI applications.

---

## Table of Contents

1. [Installation](#installation)
2. [Basic Setup](#basic-setup)
3. [Your First Control](#your-first-control)
4. [Common Scenarios](#common-scenarios)
5. [Theming and Styling](#theming-and-styling)
6. [Migration Guide](#migration-guide)
7. [Troubleshooting](#troubleshooting)

---

## Installation

### Prerequisites

- **.NET 10 SDK** or later
- **Jalium.UI Framework** installed and configured
- **Visual Studio 2025** or **Visual Studio Code** with C# extension

### NuGet Package

```bash
dotnet add package FluentJalium --version 0.1.0-preview.1
```

Or via Package Manager Console:

```powershell
Install-Package FluentJalium -Version 0.1.0-preview.1
```

### Manual Installation

1. Clone the repository:
```bash
git clone https://github.com/your-org/FluentJalium.git
```

2. Add project reference:
```bash
dotnet add reference path/to/FluentJalium/src/FluentJalium/FluentJalium.csproj
```

---

## Basic Setup

### 1. Update Your App.jalxaml

Add FluentJalium namespace and apply the theme:

```xml
<Application x:Class="MyApp.App"
             xmlns="http://jalium.org/winfx/2024/jalxaml"
             xmlns:x="http://schemas.jalium.org/winfx/2024/jalxaml"
             xmlns:fw="using:FluentJalium.Controls">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Import FluentJalium themes -->
                <ResourceDictionary Source="pack://application:,,,/FluentJalium;component/Themes/Generic.jalxaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 2. Configure in Code-Behind

In your `App.jalxaml.cs`:

```csharp
using FluentJalium;
using FluentJalium.Theming;
using Jalium.UI;

namespace MyApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Apply FluentJalium theme
        FluentJaliumTheme.Apply(this);
        
        // Optional: Set theme variant
        FluentJaliumTheme.SetThemeVariant(ThemeVariant.Dark);
        
        // Optional: Set density
        FluentJaliumTheme.SetDensity(Density.Comfortable);
    }
}
```

---

## Your First Control

### Example 1: FWButton

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://jalium.org/winfx/2024/jalxaml"
        xmlns:fw="using:FluentJalium.Controls"
        Title="My First FluentJalium App"
        Width="800"
        Height="600">
    
    <StackPanel Margin="24" Spacing="16">
        <fw:FWButton Content="Primary Button" 
                     Appearance="Primary"
                     Click="OnButtonClick" />
        
        <fw:FWButton Content="Secondary Button" 
                     Appearance="Secondary" />
        
        <fw:FWButton Content="Accent Button" 
                     Appearance="Accent"
                     Icon="" />
    </StackPanel>
</Window>
```

Code-behind:

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
        MessageBox.Show($"You clicked: {button.Content}");
    }
}
```

### Example 2: FWCard with Content

```xml
<fw:FWCard Width="400" Height="200">
    <StackPanel Spacing="12">
        <TextBlock Text="Welcome to FluentJalium" 
                   FontSize="24" 
                   FontWeight="SemiBold" />
        
        <TextBlock Text="Modern Fluent Design System controls for Jalium.UI" 
                   TextWrapping="Wrap"
                   Opacity="0.7" />
        
        <fw:FWButton Content="Get Started" 
                     Appearance="Primary" 
                     HorizontalAlignment="Left" />
    </StackPanel>
</fw:FWCard>
```

---

## Common Scenarios

### Scenario 1: Creating a Form with Validation

```xml
<StackPanel Spacing="16" Margin="24">
    <!-- Text Input -->
    <fw:FWTextBox Header="Username"
                  PlaceholderText="Enter your username"
                  IsRequired="True"
                  ErrorMessage="Username is required" />
    
    <!-- Password Input -->
    <fw:FWPasswordBox Header="Password"
                      PlaceholderText="Enter your password"
                      IsRequired="True"
                      PasswordRevealMode="Peek" />
    
    <!-- Dropdown -->
    <fw:FWComboBox Header="Country"
                   PlaceholderText="Select your country"
                   ItemsSource="{Binding Countries}" />
    
    <!-- Checkbox -->
    <fw:FWCheckBox Content="I agree to the terms and conditions"
                   IsChecked="{Binding AgreeToTerms}" />
    
    <!-- Submit Button -->
    <fw:FWButton Content="Submit"
                 Appearance="Primary"
                 Click="OnSubmitClick" />
</StackPanel>
```

### Scenario 2: Building a Navigation Menu

```xml
<fw:FWNavigationView>
    <fw:FWNavigationView.MenuItems>
        <fw:FWNavigationViewItem Content="Home" 
                                 Icon="" 
                                 Tag="HomePage" />
        
        <fw:FWNavigationViewItem Content="Documents" 
                                 Icon="" 
                                 Tag="DocumentsPage" />
        
        <fw:FWNavigationViewItem Content="Settings" 
                                 Icon="" 
                                 Tag="SettingsPage" />
    </fw:FWNavigationView.MenuItems>
    
    <fw:FWNavigationView.Content>
        <Frame x:Name="ContentFrame" />
    </fw:FWNavigationView.Content>
</fw:FWNavigationView>
```

Code-behind:

```csharp
private void NavigationView_SelectionChanged(FWNavigationView sender, FWNavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItem is FWNavigationViewItem item)
    {
        var pageTag = item.Tag?.ToString();
        NavigateToPage(pageTag);
    }
}

private void NavigateToPage(string pageTag)
{
    var pageType = pageTag switch
    {
        "HomePage" => typeof(HomePage),
        "DocumentsPage" => typeof(DocumentsPage),
        "SettingsPage" => typeof(SettingsPage),
        _ => null
    };
    
    if (pageType != null)
    {
        ContentFrame.Navigate(pageType);
    }
}
```

### Scenario 3: Displaying Data in a List

```xml
<fw:FWItemsRepeater ItemsSource="{Binding Products}"
                    HorizontalCacheLength="400"
                    VerticalCacheLength="400">
    <fw:FWItemsRepeater.Layout>
        <fw:StackLayout Orientation="Vertical" Spacing="8" />
    </fw:FWItemsRepeater.Layout>
    
    <fw:FWItemsRepeater.ItemTemplate>
        <DataTemplate>
            <fw:FWCard>
                <Grid ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    
                    <Image Source="{Binding ImageUrl}" 
                           Width="60" 
                           Height="60" />
                    
                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                        <TextBlock Text="{Binding Name}" 
                                   FontWeight="Medium" />
                        <TextBlock Text="{Binding Description}" 
                                   Opacity="0.7" />
                    </StackPanel>
                    
                    <TextBlock Grid.Column="2" 
                               Text="{Binding Price, StringFormat='C'}" 
                               FontSize="18"
                               FontWeight="SemiBold"
                               VerticalAlignment="Center" />
                </Grid>
            </fw:FWCard>
        </DataTemplate>
    </fw:FWItemsRepeater.ItemTemplate>
</fw:FWItemsRepeater>
```

### Scenario 4: Creating a Settings Page

```xml
<ScrollViewer>
    <StackPanel Spacing="24" Margin="24">
        <!-- Appearance Section -->
        <fw:FWExpander Header="Appearance">
            <StackPanel Spacing="12">
                <fw:FWRadioButton Content="Light Theme" 
                                  GroupName="Theme"
                                  IsChecked="{Binding IsLightTheme}" />
                
                <fw:FWRadioButton Content="Dark Theme" 
                                  GroupName="Theme"
                                  IsChecked="{Binding IsDarkTheme}" />
                
                <fw:FWRadioButton Content="System Default" 
                                  GroupName="Theme"
                                  IsChecked="{Binding IsSystemTheme}" />
            </StackPanel>
        </fw:FWExpander>
        
        <!-- Notifications Section -->
        <fw:FWExpander Header="Notifications">
            <StackPanel Spacing="12">
                <fw:FWToggleSwitch Header="Push Notifications"
                                  IsOn="{Binding PushNotificationsEnabled}" />
                
                <fw:FWToggleSwitch Header="Email Notifications"
                                  IsOn="{Binding EmailNotificationsEnabled}" />
                
                <fw:FWToggleSwitch Header="Sound Alerts"
                                  IsOn="{Binding SoundAlertsEnabled}" />
            </StackPanel>
        </fw:FWExpander>
        
        <!-- Privacy Section -->
        <fw:FWExpander Header="Privacy">
            <StackPanel Spacing="12">
                <fw:FWToggleSwitch Header="Analytics"
                                  IsOn="{Binding AnalyticsEnabled}" />
                
                <fw:FWToggleSwitch Header="Crash Reports"
                                  IsOn="{Binding CrashReportsEnabled}" />
            </StackPanel>
        </fw:FWExpander>
    </StackPanel>
</ScrollViewer>
```

---

## Theming and Styling

### Applying Global Theme

```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    // Light theme
    FluentJaliumTheme.SetThemeVariant(ThemeVariant.Light);
    
    // Dark theme
    FluentJaliumTheme.SetThemeVariant(ThemeVariant.Dark);
    
    // System theme (follows OS setting)
    FluentJaliumTheme.SetThemeVariant(ThemeVariant.System);
}
```

### Customizing Accent Color

```csharp
// Set custom accent color
FluentJaliumTheme.SetAccentColor(Color.FromRgb(0x00, 0x78, 0xD4));

// Or use predefined accents
FluentJaliumTheme.SetAccentColor(FluentJaliumAccents.Blue);
FluentJaliumTheme.SetAccentColor(FluentJaliumAccents.Purple);
FluentJaliumTheme.SetAccentColor(FluentJaliumAccents.Green);
```

### Density Settings

```csharp
// Compact: Denser layout, smaller controls
FluentJaliumTheme.SetDensity(Density.Compact);

// Comfortable: Default balanced spacing
FluentJaliumTheme.SetDensity(Density.Comfortable);

// Spacious: More padding and spacing
FluentJaliumTheme.SetDensity(Density.Spacious);
```

### Per-Control Styling

```xml
<fw:FWButton Content="Custom Styled Button">
    <fw:FWButton.Style>
        <Style TargetType="fw:FWButton">
            <Setter Property="Background" Value="#0078D4" />
            <Setter Property="Foreground" Value="White" />
            <Setter Property="CornerRadius" Value="8" />
            <Setter Property="Padding" Value="16,8" />
        </Style>
    </fw:FWButton.Style>
</fw:FWButton>
```

---

## Migration Guide

### From Standard Jalium.UI Controls

Replace standard controls with FluentJalium equivalents:

| Jalium.UI | FluentJalium | Notes |
|-----------|--------------|-------|
| `Button` | `FWButton` | Add `Appearance` property |
| `TextBox` | `FWTextBox` | Add `Header`, `PlaceholderText` |
| `CheckBox` | `FWCheckBox` | Compatible, enhanced styling |
| `ComboBox` | `FWComboBox` | Add `Header`, `PlaceholderText` |
| `ListBox` | `FWListView` | More features, better performance |
| `TabControl` | `FWTabView` | Modern tab interface |
| `Expander` | `FWExpander` | Enhanced animations |

### Example Migration

**Before** (Standard Jalium.UI):
```xml
<Button Content="Click Me" 
        Background="Blue"
        Foreground="White"
        Click="OnButtonClick" />
```

**After** (FluentJalium):
```xml
<fw:FWButton Content="Click Me" 
             Appearance="Primary"
             Click="OnButtonClick" />
```

---

## Troubleshooting

### Issue: Controls Not Showing Fluent Styling

**Solution**: Ensure you've imported the theme resources in App.jalxaml:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/FluentJalium;component/Themes/Generic.jalxaml" />
</ResourceDictionary.MergedDictionaries>
```

### Issue: Namespace Not Found

**Solution**: Add the NuGet package or project reference:

```bash
dotnet add package FluentJalium
```

And add the namespace in XAML:

```xml
xmlns:fw="using:FluentJalium.Controls"
```

### Issue: Performance Issues with Large Lists

**Solution**: Use `FWItemsRepeater` instead of `FWListView` and enable virtualization:

```xml
<fw:FWItemsRepeater ItemsSource="{Binding Items}"
                    HorizontalCacheLength="400"
                    VerticalCacheLength="400">
    <fw:FWItemsRepeater.Layout>
        <fw:StackLayout Spacing="8" />
    </fw:FWItemsRepeater.Layout>
</fw:FWItemsRepeater>
```

### Issue: Theme Not Applying

**Solution**: Call `FluentJaliumTheme.Apply()` in `App.OnStartup()`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    FluentJaliumTheme.Apply(this);
}
```

### Issue: Material Effects Not Working

**Solution**: Check if your system supports the required effects. Use fallback mode:

```csharp
var backdrop = new FWBackdrop
{
    Type = FWBackdropType.Acrylic,
    AlwaysUseFallback = SystemInfo.IsLowEndHardware() // Your detection logic
};
```

---

## Next Steps

- 📖 Read the [API Reference](API_REFERENCE.md) for detailed control documentation
- 🎨 Explore the [Gallery Sample](../samples/FluentJalium.Gallery) for interactive examples
- ⚡ Review [Performance Guidelines](../PERFORMANCE.md) for optimization tips
- 🚀 Check [PROGRESS.md](../PROGRESS.md) for the development roadmap

---

## Getting Help

- **Issues**: [GitHub Issues](https://github.com/your-org/FluentJalium/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/FluentJalium/discussions)
- **Documentation**: [Official Docs](https://fluentjalium.dev)

---

**Last Updated**: 2026-06-07  
**Version**: 0.1.0-preview.1
