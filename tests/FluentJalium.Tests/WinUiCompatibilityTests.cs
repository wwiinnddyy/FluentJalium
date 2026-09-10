using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.WinUI;
using Jalium.UI;
using Jalium.UI.Markup;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

/// <summary>
/// The load-bearing claim behind the WinUI facade: Jalium resolves markup elements by simple
/// name, so an unmodified WinUI document — default <c>xmlns</c>, WinUI type names — can be
/// redirected at FluentJalium's controls with no rewrite pass and no framework changes.
/// </summary>
[Collection("Application")]
public sealed class WinUiCompatibilityTests
{
    private const string WinUiRoot =
        @"xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""";

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime XamlReader type resolution.")]
    public void Initialize_AliasesWinUiNamesToFwControlsAndLeavesLayoutAlone()
    {
        WinUICompatibility.Initialize();

        Assert.Same(typeof(FWButton), XamlTypeRegistry.GetType("Button"));
        Assert.Same(typeof(FWNumberBox), XamlTypeRegistry.GetType("NumberBox"));
        Assert.Same(typeof(FWNavigationView), XamlTypeRegistry.GetType("NavigationView"));
        Assert.Same(typeof(FWItemsRepeater), XamlTypeRegistry.GetType("ItemsRepeater"));
        Assert.Same(typeof(FWRadioButtons), XamlTypeRegistry.GetType("RadioButtons"));

        // Layout and decoration names are deliberately not overridden — doing so would reach
        // into every framework template that names them.
        Assert.Same(typeof(Jalium.UI.Controls.Grid), XamlTypeRegistry.GetType("Grid"));
        Assert.Same(typeof(Jalium.UI.Controls.StackPanel), XamlTypeRegistry.GetType("StackPanel"));
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime XamlReader.Load.")]
    public void Parse_UnprefixedWinUiElement_YieldsFluentJaliumControl()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            JaliumThemeManager.Initialize(app);
            WinUICompatibility.Initialize();

            var button = XamlReader.Parse($"<Button {WinUiRoot} Content=\"Hello\" />");

            Assert.IsType<FWButton>(button);
            Assert.Equal("Hello", ((FWButton)button).Content);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime XamlReader.Load.")]
    public void Parse_WinUiUsingPrefixedNamespace_YieldsFluentJaliumControl()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            JaliumThemeManager.Initialize(app);
            WinUICompatibility.Initialize();

            // The dominant WinUI spelling: muxc: mapped through the `using:` scheme, which
            // Jalium's resolver has no special handling for.
            var page = $"""
                <StackPanel {WinUiRoot}
                            xmlns:muxc="using:Microsoft.UI.Xaml.Controls">
                    <muxc:NumberBox Value="3" />
                </StackPanel>
                """;

            var panel = (Jalium.UI.Controls.StackPanel)XamlReader.Parse(page);
            Assert.IsType<FWNumberBox>(Assert.Single(panel.Children));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    [RequiresUnreferencedCode("Exercises runtime XamlReader.Load.")]
    public void Parse_UnsupportedWinUiName_FailsLoudlyInsteadOfSilentlySubstituting()
    {
        ResetApplicationState();
        ThemeLoader.Initialize();
        var app = new Application();

        try
        {
            JaliumThemeManager.Initialize(app);
            WinUICompatibility.Initialize();

            // Pivot has no implementation in either FluentJalium or Jalium yet. A missing type
            // must be a parse error, not a wrong-but-quiet substitution.
            Assert.ThrowsAny<Exception>(() =>
                XamlReader.Parse($"<Pivot {WinUiRoot} />"));
        }
        finally
        {
            ResetApplicationState();
        }
    }

    [Fact]
    public void Initialize_IsIdempotent()
    {
        WinUICompatibility.Initialize();
        var before = XamlTypeRegistry.GetType("Button");

        WinUICompatibility.Initialize();

        Assert.True(WinUICompatibility.IsInitialized);
        Assert.Same(before, XamlTypeRegistry.GetType("Button"));
    }
}
