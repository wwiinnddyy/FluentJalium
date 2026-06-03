using System.Reflection;
using FluentJalium.Icon;
using Jalium.UI.Controls;
using Jalium.UI.Markup;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentIconTests
{
    [Fact]
    public void FluentSystemIcons_ShouldConvertRegularAndFilledGlyphs()
    {
        Assert.Equal(char.ConvertFromUtf32((int)FluentIconRegular.Home24), FluentIconRegular.Home24.GetGlyph());
        Assert.Equal(char.ConvertFromUtf32((int)FluentIconFilled.Home24), FluentIconFilled.Home24.GetString());
        Assert.Equal(string.Empty, FluentIconRegular.Empty.GetString());
        Assert.NotEqual(FluentIconRegular.Home24.GetString(), FluentIconFilled.Home24.GetString());
    }

    [Fact]
    public void SegoeFluentIcons_ShouldConvertCompatibilityGlyphs()
    {
        Assert.Equal("\uE80F", SegoeFluentIcon.Home.GetGlyph());
        Assert.Equal("\uE72D", SegoeFluentIcon.Share.GetString());
        Assert.Equal("\uE7BA", SegoeFluentIcon.Badge.GetString());
    }

    [Fact]
    public void FluentIcon_ShouldUseRegularFontAndUpdateSizeByDefault()
    {
        var icon = new FluentIcon
        {
            Icon = FluentIconRegular.Home24,
            Size = 32
        };

        Assert.Equal(FluentIconRegular.Home24.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.RegularFontFamily, icon.FontFamily?.ToString());
        Assert.Equal(32, icon.Size);
        Assert.Equal(32, icon.FontSize);
        Assert.Equal(32, icon.Width);
        Assert.Equal(32, icon.Height);
    }

    [Fact]
    public void FluentIcon_ShouldSwitchRegularIconToFilledGlyph()
    {
        var icon = new FluentIcon
        {
            Icon = FluentIconRegular.Home24,
            Filled = true
        };

        Assert.Equal(FluentIconFilled.Home24.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.FilledFontFamily, icon.FontFamily?.ToString());

        icon.Filled = false;

        Assert.Equal(FluentIconRegular.Home24.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.RegularFontFamily, icon.FontFamily?.ToString());
        Assert.Equal(FluentIconSet.Regular, icon.IconSet);
    }

    [Fact]
    public void FluentIcon_ShouldUseFilledAndSegoeIconsDirectly()
    {
        var icon = new FluentIcon
        {
            Icon = FluentIconFilled.Save24
        };

        Assert.Equal(FluentIconFilled.Save24.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.FilledFontFamily, icon.FontFamily?.ToString());

        icon.Icon = SegoeFluentIcon.Save;

        Assert.Equal(SegoeFluentIcon.Save.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.SegoeFontFamily, icon.FontFamily?.ToString());
    }

    [Fact]
    public void FluentIcon_ShouldKeepRegularSystemIconInSystemFontWhenIconSetIsSegoe()
    {
        var icon = new FluentIcon
        {
            Icon = FluentIconRegular.Home24,
            IconSet = FluentIconSet.Segoe
        };

        Assert.Equal(FluentIconRegular.Home24.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.RegularFontFamily, icon.FontFamily?.ToString());

        icon.Icon = SegoeFluentIcon.Home;

        Assert.Equal(SegoeFluentIcon.Home.GetString(), icon.Glyph);
        Assert.Equal(FluentIcon.SegoeFontFamily, icon.FontFamily?.ToString());
    }

    [Fact]
    public void FluentIconFactory_ShouldReturnUsableFontIcons()
    {
        var foreground = new SolidColorBrush(Colors.Red);
        FontIcon regular = FluentIconFactory.Regular(FluentIconRegular.Save24, 18, foreground);
        FontIcon filled = FluentIconFactory.Filled(FluentIconFilled.Save24, 22);
        FontIcon segoe = FluentIconFactory.Segoe(SegoeFluentIcon.Save, 24);

        Assert.IsType<FluentIcon>(regular);
        Assert.Equal(FluentIconRegular.Save24.GetString(), regular.Glyph);
        Assert.Equal(18, regular.FontSize);
        Assert.Same(foreground, regular.Foreground);

        Assert.IsType<FluentIcon>(filled);
        Assert.Equal(FluentIconFilled.Save24.GetString(), filled.Glyph);
        Assert.Equal(FluentIcon.FilledFontFamily, filled.FontFamily?.ToString());

        Assert.IsType<FluentIcon>(segoe);
        Assert.Equal(SegoeFluentIcon.Save.GetString(), segoe.Glyph);
        Assert.Equal(FluentIcon.SegoeFontFamily, segoe.FontFamily?.ToString());
    }

    [Fact]
    public void AssemblyMetadata_ShouldExposeIconXmlNamespace()
    {
        var assembly = typeof(FluentIcon).Assembly;
        var definitions = assembly.GetCustomAttributes<XmlnsDefinitionAttribute>().ToArray();
        var prefixes = assembly.GetCustomAttributes<XmlnsPrefixAttribute>().ToArray();

        Assert.Contains(definitions, attribute =>
            attribute.XmlNamespace == "https://jalium.dev/fluent"
            && attribute.ClrNamespace == "FluentJalium.Controls");
        Assert.Contains(definitions, attribute =>
            attribute.XmlNamespace == "https://jalium.dev/fluent/icon"
            && attribute.ClrNamespace == "FluentJalium.Icon");
        Assert.Contains(prefixes, attribute =>
            attribute.XmlNamespace == "https://jalium.dev/fluent/icon"
            && attribute.Prefix == "fluentIcon");
    }
}
