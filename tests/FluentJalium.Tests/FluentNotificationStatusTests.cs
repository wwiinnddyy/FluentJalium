using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Markup;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentNotificationStatusTests
{
    private static readonly string[] NotificationStatusResourceKeys =
    [
        "InfoBarInformationalBackground",
        "InfoBarSuccessBackground",
        "InfoBarWarningBackground",
        "InfoBarErrorBackground",
        "InfoBarInfoBrush",
        "InfoBarSuccessBrush",
        "InfoBarWarningBrush",
        "InfoBarErrorBrush",
        "InfoBarForeground",
        "ToastForeground",
        "ToastInformationBackground",
        "ToastSuccessBackground",
        "ToastWarningBackground",
        "ToastErrorBackground",
        "ToastInformationIcon",
        "ToastSuccessIcon",
        "ToastWarningIcon",
        "ToastErrorIcon",
        "InfoBadgeAttentionBackground",
        "InfoBadgeInformationalBackground",
        "InfoBadgeSuccessBackground",
        "InfoBadgeCautionBackground",
        "InfoBadgeCriticalBackground",
        "InfoBadgeAttentionForeground",
        "InfoBadgeInformationalForeground",
        "InfoBadgeSuccessForeground",
        "InfoBadgeCautionForeground",
        "InfoBadgeCriticalForeground",
        "StatusBarBackground",
        "StatusBarForeground",
        "StatusBarForegroundDisabled",
        "StatusBarBorderBrush",
        "StatusBarSeparatorForeground"
    ];

    [Theory]
    [InlineData(FluentThemeVariant.Dark)]
    [InlineData(FluentThemeVariant.Light)]
    [InlineData(FluentThemeVariant.HighContrast)]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void ThemeDictionary_ShouldExposeNotificationStatusTokensForEveryTheme(FluentThemeVariant theme)
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = theme.ToString();

        var dictionary = LoadGenericThemeDictionary();

        foreach (var key in NotificationStatusResourceKeys)
        {
            Assert.True(dictionary.TryGetValue(key, out var value), $"{key} was not found in the {theme} theme.");
            Assert.IsType<SolidColorBrush>(value);
        }

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void Apply_ShouldRegisterFwNotificationStatusStyles()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);

            AssertBasedOnStyle<FWInfoBar, InfoBar>(app.Resources);
            AssertOwnedStyle<FWInfoBadge>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationItem, ToastNotificationItem>(app.Resources);
            AssertBasedOnStyle<FWToastNotificationHost, ToastNotificationHost>(app.Resources);
            AssertBasedOnStyle<FWStatusBar, StatusBar>(app.Resources);
            AssertBasedOnStyle<FWStatusBarItem, StatusBarItem>(app.Resources);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void GenericTheme_ShouldDefineNotificationStatusBaseStylesAndToastDefaults()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        ResourceDictionary.CurrentThemeKey = FluentThemeVariant.Dark.ToString();

        var dictionary = LoadGenericThemeDictionary();

        AssertContainsStyle<InfoBar>(dictionary);
        AssertContainsStyle<ToastNotificationItem>(dictionary);
        AssertContainsStyle<ToastNotificationHost>(dictionary);
        AssertContainsStyle<StatusBar>(dictionary);
        AssertContainsStyle<StatusBarItem>(dictionary);

        var badgeStyle = AssertStyle<FWInfoBadge>(dictionary);
        Assert.DoesNotContain(badgeStyle.Setters, setter => setter.Property == Control.BackgroundProperty);
        Assert.DoesNotContain(badgeStyle.Setters, setter => setter.Property == Control.ForegroundProperty);
        AssertSetter(badgeStyle, Control.CornerRadiusProperty);
        AssertSetter(badgeStyle, Control.PaddingProperty);

        var toastHostStyle = AssertStyle<ToastNotificationHost>(dictionary);
        AssertSetter(toastHostStyle, ToastNotificationHost.MaxVisibleToastsProperty, 3);
        AssertSetter(toastHostStyle, ToastNotificationHost.SpacingProperty, 8.0);
        AssertSetter(toastHostStyle, ToastNotificationHost.ToastWidthProperty, 400.0);

        ResetApplicationState();
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FWInfoBadge_DefaultStyleShouldNotOverrideSeverityResourceResolution()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            var critical = new FWInfoBadge
            {
                Value = 1,
                Severity = FWInfoBadgeSeverity.Critical
            };
            var caution = new FWInfoBadge
            {
                Value = 1,
                Severity = FWInfoBadgeSeverity.Caution
            };

            critical.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            caution.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.False(critical.HasLocalValue(Control.BackgroundProperty));
            Assert.False(critical.HasLocalValue(Control.ForegroundProperty));
            Assert.False(caution.HasLocalValue(Control.BackgroundProperty));
            Assert.False(caution.HasLocalValue(Control.ForegroundProperty));
            Assert.Equal(FWInfoBadgeDisplayKind.Value, critical.DisplayKind);
            Assert.Equal("1", critical.DisplayValueText);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void FWToastNotificationHost_ShouldExposeFluentDefaultsAndDismissVisibleItems()
    {
        var host = new FWToastNotificationHost
        {
            MaxVisibleToasts = 3,
            Position = ToastPosition.BottomRight,
            ToastWidth = 400,
            Spacing = 8
        };

        var info = host.ShowInformation("Info", "First", TimeSpan.FromSeconds(12));
        var success = host.ShowSuccess("Success", "Second", TimeSpan.FromSeconds(12));
        var warning = host.ShowWarning("Warning", "Third", TimeSpan.FromSeconds(12));
        var error = host.ShowError("Error", "Fourth", TimeSpan.FromSeconds(12));

        Assert.IsType<FWToastNotificationItem>(info);
        Assert.DoesNotContain(info, host.Children);
        Assert.Contains(success, host.Children);
        Assert.Contains(warning, host.Children);
        Assert.Contains(error, host.Children);
        Assert.Equal(3, host.Children.Count);
        Assert.Equal(ToastPosition.BottomRight, host.Position);
        Assert.Equal(400, host.ToastWidth);
        Assert.Equal(8, host.Spacing);

        success.IsOpen = false;

        Assert.Equal(2, host.Children.Count);
        Assert.DoesNotContain(success, host.Children);

        host.DismissAll();
        Assert.Equal(2, host.Children.Count);
    }

    [Fact]
    public void FWInfoBar_ClosableAndIconStateShouldRemainInteractive()
    {
        var infoBar = new FWInfoBar
        {
            Title = "Review required",
            Message = "Long message",
            Severity = InfoBarSeverity.Warning,
            IsClosable = true,
            IsIconVisible = false,
            IsOpen = true
        };
        var closed = 0;
        infoBar.Closed += (_, _) => closed++;

        infoBar.IsOpen = false;

        Assert.Equal(InfoBarSeverity.Warning, infoBar.Severity);
        Assert.True(infoBar.IsClosable);
        Assert.False(infoBar.IsIconVisible);
        Assert.False(infoBar.IsOpen);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void FWStatusBar_ShouldGenerateFwContainersForMixedStatusItems()
    {
        var statusBar = new FWStatusBar();
        statusBar.Items.Add("Ready");
        statusBar.Items.Add(new FWStatusBarItem { Content = "UTF-8" });
        statusBar.Items.Add(new FWStatusBarItem
        {
            Content = new FWInfoBadge
            {
                Severity = FWInfoBadgeSeverity.Success
            }
        });

        statusBar.Measure(new Size(480, 24));

        var itemsHost = Assert.IsAssignableFrom<Panel>(statusBar.GetVisualChild(0));
        Assert.Equal(3, itemsHost.Children.Count);
        Assert.All(itemsHost.Children, child => Assert.IsType<FWStatusBarItem>(child));
    }

    private static ResourceDictionary LoadGenericThemeDictionary()
    {
        var loaded = ResourceDictionary.SourceLoader?.Invoke(
            new ResourceDictionary(),
            new Uri("/FluentJalium;component/Themes/Generic.jalxaml", UriKind.Relative),
            FluentThemeManager.ThemeAssembly);

        return Assert.IsType<ResourceDictionary>(loaded);
    }

    private static void AssertContainsStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        Assert.IsType<Style>(value);
    }

    private static Style AssertStyle<TControl>(ResourceDictionary dictionary)
        where TControl : FrameworkElement
    {
        Assert.True(dictionary.TryGetValue(typeof(TControl), out var value), $"{typeof(TControl).Name} style was not found.");
        return Assert.IsType<Style>(value);
    }

    private static void AssertBasedOnStyle<TFluentControl, TJaliumControl>(ResourceDictionary dictionary)
        where TFluentControl : TJaliumControl, IFluentJaliumControl
        where TJaliumControl : FrameworkElement
    {
        var baseStyle = AssertStyle<TJaliumControl>(dictionary);
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Same(baseStyle, fluentStyle.BasedOn);
    }

    private static void AssertOwnedStyle<TFluentControl>(ResourceDictionary dictionary)
        where TFluentControl : FrameworkElement, IFluentJaliumControl
    {
        var fluentStyle = AssertStyle<TFluentControl>(dictionary);

        Assert.Equal(typeof(TFluentControl), fluentStyle.TargetType);
        Assert.Null(fluentStyle.BasedOn);
    }

    private static void AssertSetter(Style style, DependencyProperty property, object? expectedValue = null)
    {
        var setter = Assert.Single(style.Setters, s => s.Property == property);
        if (expectedValue != null)
        {
            if (expectedValue is int expectedInt)
            {
                Assert.Equal(expectedInt, Convert.ToInt32(setter.Value));
            }
            else if (expectedValue is double expectedDouble)
            {
                Assert.Equal(expectedDouble, Convert.ToDouble(setter.Value));
            }
            else
            {
                Assert.Equal(expectedValue, setter.Value);
            }
        }
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
