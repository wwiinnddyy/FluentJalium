using FluentJalium.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentVisualControlsTests
{
    [Fact]
    public void FWVisualControls_ShouldComposeInsideLiquidGlassSurface()
    {
        var pixels = CreateSamplePixels(12, 8);
        var source = BitmapImage.FromPixels(pixels, 12, 8);
        var image = new FWImage
        {
            Source = source,
            Stretch = Stretch.UniformToFill,
            StretchDirection = StretchDirection.Both,
            IsZoomEnabled = true,
            MinZoom = 0.75,
            MaxZoom = 4
        };
        var fluentIcon = FluentIconFactory.Regular(FluentIconRegular.Image24, 24);
        var filledIcon = FluentIconFactory.Filled(FluentIconRegular.Share24, 24);
        var fontIcon = new FWFontIcon
        {
            Glyph = "\uE72D",
            FontFamily = FluentIcon.SegoeFontFamily,
            FontSize = 24
        };
        var symbolIcon = new FWSymbolIcon
        {
            Symbol = Symbol.Save
        };
        var geometry = Geometry.Parse("M 0,0 L 20,0 L 20,20 L 0,20 Z");
        var pathIcon = new FWPathIcon
        {
            Data = geometry,
            Width = 24,
            Height = 24
        };
        var target = new FWTextBox
        {
            Text = "Visual target"
        };
        var label = new FWLabel
        {
            Content = "Name",
            Target = target,
            AccessKey = 'N'
        };
        var separator = new FWSeparator
        {
            Orientation = Orientation.Vertical,
            Height = 24
        };
        var viewboxChild = new FWBorder
        {
            Width = 120,
            Height = 60,
            Child = new FWTextBlock
            {
                Text = "Scaled"
            }
        };
        var viewbox = new FWViewbox
        {
            Child = viewboxChild,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            Width = 180,
            Height = 90
        };
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                image,
                fluentIcon,
                filledIcon,
                fontIcon,
                symbolIcon,
                pathIcon,
                label,
                separator,
                viewbox
            }
        };
        var surface = new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = stack
        };

        Assert.Same(source, image.Source);
        Assert.Equal(Stretch.UniformToFill, image.Stretch);
        Assert.Equal(StretchDirection.Both, image.StretchDirection);
        Assert.True(image.IsZoomEnabled);
        Assert.Equal(0.75, image.MinZoom);
        Assert.Equal(4, image.MaxZoom);
        Assert.Equal(FluentIconRegular.Image24, fluentIcon.Icon);
        Assert.Equal(24, fluentIcon.Size);
        Assert.False(fluentIcon.Filled);
        Assert.Equal(FluentIconRegular.Share24, filledIcon.Icon);
        Assert.True(filledIcon.Filled);
        Assert.Equal("\uE72D", fontIcon.Glyph);
        Assert.Equal(FluentIcon.SegoeFontFamily, fontIcon.FontFamily?.ToString());
        Assert.Equal(24, fontIcon.FontSize);
        Assert.Equal(Symbol.Save, symbolIcon.Symbol);
        Assert.Same(geometry, pathIcon.Data);
        Assert.Equal(24, pathIcon.Width);
        Assert.Equal(24, pathIcon.Height);
        Assert.Equal("Name", label.Content);
        Assert.Same(target, label.Target);
        Assert.Equal('N', label.AccessKey);
        Assert.Equal(Orientation.Vertical, separator.Orientation);
        Assert.Equal(24, separator.Height);
        Assert.Same(viewboxChild, viewbox.Child);
        Assert.Equal(Stretch.Uniform, viewbox.Stretch);
        Assert.Equal(StretchDirection.DownOnly, viewbox.StretchDirection);
        Assert.Equal(10, stack.Spacing);
        Assert.Equal(9, stack.Children.Count);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(stack, surface.Child);
    }

    private static byte[] CreateSamplePixels(int width, int height)
    {
        var pixels = new byte[width * height * 4];
        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = 0x72;
            pixels[index + 1] = 0x45;
            pixels[index + 2] = 0xD4;
            pixels[index + 3] = 0xFF;
        }

        return pixels;
    }
}
