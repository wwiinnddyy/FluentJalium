using FluentJalium.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using ShapePointCollection = Jalium.UI.Controls.Shapes.PointCollection;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentVisualControlsTests
{
    [Fact]
    public void FWPersonPicture_ShouldResolveDisplayAndBadgeState()
    {
        var pixels = CreateSamplePixels(4, 4);
        var source = BitmapImage.FromPixels(pixels, 4, 4);
        var avatar = new FWPersonPicture
        {
            DisplayName = "Ada Lovelace",
            BadgeNumber = 128
        };

        Assert.IsAssignableFrom<IFluentJaliumControl>(avatar);
        Assert.Equal(48, avatar.Width);
        Assert.Equal(48, avatar.Height);
        Assert.False(avatar.Focusable);
        Assert.Equal(string.Empty, avatar.Initials);
        Assert.Equal("AL", avatar.DisplayInitials);
        Assert.Equal(FWPersonPictureDisplayKind.Initials, avatar.DisplayKind);
        Assert.False(avatar.HasProfilePicture);
        Assert.True(avatar.HasBadge);
        Assert.Equal(FWPersonPictureBadgeKind.Number, avatar.BadgeKind);
        Assert.Equal("99+", avatar.BadgeDisplayText);

        avatar.Initials = "gh";
        avatar.DisplayName = "Grace Hopper";

        Assert.Equal("GH", avatar.DisplayInitials);
        Assert.Equal("gh", avatar.Initials);

        avatar.IsGroup = true;

        Assert.Equal(FWPersonPictureDisplayKind.Group, avatar.DisplayKind);
        Assert.Equal("GH", avatar.DisplayInitials);

        avatar.ProfilePicture = source;

        Assert.True(avatar.HasProfilePicture);
        Assert.Equal(FWPersonPictureDisplayKind.Image, avatar.DisplayKind);

        avatar.BadgeNumber = 0;
        avatar.BadgeGlyph = "!";

        Assert.True(avatar.HasBadge);
        Assert.Equal(FWPersonPictureBadgeKind.Glyph, avatar.BadgeKind);
        Assert.Equal("!", avatar.BadgeDisplayText);

        avatar.BadgeImageSource = source;

        Assert.Equal(FWPersonPictureBadgeKind.Image, avatar.BadgeKind);
        Assert.Equal(string.Empty, avatar.BadgeDisplayText);
    }

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
        var bitmapIcon = new FWBitmapIcon
        {
            Source = source,
            ShowAsMonochrome = false,
            Width = 20,
            Height = 20
        };
        var imageIcon = new FWImageIcon
        {
            Source = source,
            Stretch = Stretch.Uniform
        };
        var personPicture = new FWPersonPicture
        {
            DisplayName = "Grace Hopper",
            BadgeNumber = 7,
            Width = 56,
            Height = 56
        };
        var fluentIcon = FluentIconFactory.Regular(FluentIconRegular.Image24, 24);
        var filledIcon = FluentIconFactory.Filled(FluentIconRegular.Share24, 24);
        var markdownBaseUri = new Uri("https://jalium.dev/");
        var markdown = new FWMarkdown
        {
            Text = "# FluentJalium\n\n[Gallery](/gallery)",
            BaseUri = markdownBaseUri,
            OpenLinksExternally = false
        };
        var richTextBlock = new FWRichTextBlock
        {
            Text = "Rich text compatibility",
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };
        var qrCode = new FWQRCode
        {
            Text = "https://jalium.dev/fluent",
            QuietZoneModules = 3,
            ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.H,
            Version = 2,
            Mask = 3,
            Encoding = QRCodeEncoding.Utf8,
            ModuleShape = QRModuleShape.RoundedSquare,
            EyeShape = QREyeShape.Rounded,
            LogoSizeRatio = 0.2,
            IsForegroundGradient = false
        };
        var fontIcon = new FWFontIcon
        {
            Glyph = FluentIconRegular.Share24.GetString(),
            FontFamily = FluentIcon.RegularFontFamily,
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
        var shapeStroke = new SolidColorBrush(Colors.DeepSkyBlue);
        var shapeFill = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x78, 0xD4));
        var rectangle = new FWRectangle
        {
            Width = 64,
            Height = 32,
            RadiusX = 6,
            RadiusY = 6,
            Fill = shapeFill,
            Stroke = shapeStroke,
            StrokeThickness = 1
        };
        var ellipse = new FWEllipse
        {
            Width = 32,
            Height = 32,
            Fill = shapeFill,
            Stroke = shapeStroke,
            StrokeThickness = 1
        };
        var line = new FWLine
        {
            X1 = 0,
            Y1 = 0,
            X2 = 48,
            Y2 = 18,
            Stroke = shapeStroke,
            StrokeThickness = 2,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        var polylinePoints = ShapePointCollection.Parse("0,24 16,4 32,24 48,8");
        var polyline = new FWPolyline
        {
            Points = polylinePoints,
            Stroke = shapeStroke,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round
        };
        var polygonPoints = ShapePointCollection.Parse("0,24 16,0 32,24");
        var polygon = new FWPolygon
        {
            Points = polygonPoints,
            Fill = shapeFill,
            Stroke = shapeStroke,
            StrokeThickness = 1,
            FillRule = FillRule.Nonzero
        };
        var path = new FWPath
        {
            Data = "M 0,16 C 8,0 24,0 32,16 S 56,32 64,16",
            Fill = shapeFill,
            Stroke = shapeStroke,
            StrokeThickness = 1.5,
            Stretch = Stretch.Uniform
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
                bitmapIcon,
                imageIcon,
                personPicture,
                fluentIcon,
                filledIcon,
                markdown,
                richTextBlock,
                qrCode,
                fontIcon,
                symbolIcon,
                pathIcon,
                rectangle,
                ellipse,
                line,
                polyline,
                polygon,
                path,
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
        Assert.Same(source, bitmapIcon.Source);
        Assert.False(bitmapIcon.ShowAsMonochrome);
        Assert.Equal(20, bitmapIcon.Width);
        Assert.Same(source, imageIcon.Source);
        Assert.Equal(Stretch.Uniform, imageIcon.Stretch);
        Assert.Equal("GH", personPicture.DisplayInitials);
        Assert.Equal(FWPersonPictureDisplayKind.Initials, personPicture.DisplayKind);
        Assert.Equal(FWPersonPictureBadgeKind.Number, personPicture.BadgeKind);
        Assert.Equal("7", personPicture.BadgeDisplayText);
        Assert.Equal(FluentIconRegular.Image24, fluentIcon.Icon);
        Assert.Equal(24, fluentIcon.Size);
        Assert.False(fluentIcon.Filled);
        Assert.Equal(FluentIconRegular.Share24, filledIcon.Icon);
        Assert.True(filledIcon.Filled);
        Assert.Equal("# FluentJalium\n\n[Gallery](/gallery)", markdown.Text);
        Assert.Same(markdownBaseUri, markdown.BaseUri);
        Assert.False(markdown.OpenLinksExternally);
        Assert.Equal("Rich text compatibility", richTextBlock.Text);
        Assert.Equal(TextWrapping.Wrap, richTextBlock.TextWrapping);
        Assert.True(richTextBlock.IsTextSelectionEnabled);
        Assert.Equal("https://jalium.dev/fluent", qrCode.Text);
        Assert.Equal(3, qrCode.QuietZoneModules);
        Assert.Equal(QRCodeErrorCorrectionLevel.H, qrCode.ErrorCorrectionLevel);
        Assert.Equal(2, qrCode.Version);
        Assert.Equal(3, qrCode.Mask);
        Assert.Equal(QRCodeEncoding.Utf8, qrCode.Encoding);
        Assert.Equal(QRModuleShape.RoundedSquare, qrCode.ModuleShape);
        Assert.Equal(QREyeShape.Rounded, qrCode.EyeShape);
        Assert.Equal(0.2, qrCode.LogoSizeRatio);
        Assert.False(qrCode.IsForegroundGradient);
        Assert.Equal(FluentIconRegular.Share24.GetString(), fontIcon.Glyph);
        Assert.Equal(FluentIcon.RegularFontFamily, fontIcon.FontFamily?.ToString());
        Assert.Equal(24, fontIcon.FontSize);
        Assert.Equal(Symbol.Save, symbolIcon.Symbol);
        Assert.Same(geometry, pathIcon.Data);
        Assert.Equal(24, pathIcon.Width);
        Assert.Equal(24, pathIcon.Height);
        Assert.Equal(6, rectangle.RadiusX);
        Assert.Equal(6, rectangle.RadiusY);
        Assert.Same(shapeFill, rectangle.Fill);
        Assert.Same(shapeStroke, rectangle.Stroke);
        Assert.Same(shapeFill, ellipse.Fill);
        Assert.Equal(48, line.X2);
        Assert.Equal(18, line.Y2);
        Assert.Equal(PenLineCap.Round, line.StrokeStartLineCap);
        Assert.Same(polylinePoints, polyline.Points);
        Assert.Equal(4, polyline.Points?.Count);
        Assert.Equal(PenLineJoin.Round, polyline.StrokeLineJoin);
        Assert.Same(polygonPoints, polygon.Points);
        Assert.Equal(FillRule.Nonzero, polygon.FillRule);
        Assert.Equal("M 0,16 C 8,0 24,0 32,16 S 56,32 64,16", path.Data);
        Assert.Equal(Stretch.Uniform, path.Stretch);
        Assert.Equal("Name", label.Content);
        Assert.Same(target, label.Target);
        Assert.Equal('N', label.AccessKey);
        Assert.Equal(Orientation.Vertical, separator.Orientation);
        Assert.Equal(24, separator.Height);
        Assert.Same(viewboxChild, viewbox.Child);
        Assert.Equal(Stretch.Uniform, viewbox.Stretch);
        Assert.Equal(StretchDirection.DownOnly, viewbox.StretchDirection);
        Assert.Equal(10, stack.Spacing);
        Assert.Equal(21, stack.Children.Count);
        Assert.Equal(FWFluentMaterialKind.LiquidGlass, surface.MaterialKind);
        Assert.True(surface.LiquidGlass);
        Assert.Equal(70, surface.RefractionAmount);
        Assert.Equal(0.42, surface.ChromaticAberration);
        Assert.Equal(24, surface.FusionRadius);
        Assert.Equal(BorderShape.SuperEllipse, surface.Shape);
        Assert.Equal(4, surface.SuperEllipseN);
        Assert.Same(stack, surface.Child);
    }

    [Fact]
    public void GalleryVisualsPage_ShouldFormatShapeControlsVisualQaSnapshot()
    {
        var shapeStroke = new SolidColorBrush(Colors.DeepSkyBlue);
        var shapeFill = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x78, 0xD4));
        var rectangle = new FWRectangle
        {
            RadiusX = 8,
            RadiusY = 8,
            Fill = shapeFill,
            Stroke = shapeStroke
        };
        var ellipse = new FWEllipse
        {
            Fill = shapeFill,
            Stroke = shapeStroke
        };
        var line = new FWLine
        {
            Stroke = shapeStroke
        };
        var polyline = new FWPolyline
        {
            Points = ShapePointCollection.Parse("0,40 18,14 38,34 58,8 82,28"),
            Stroke = shapeStroke
        };
        var polygon = new FWPolygon
        {
            Points = ShapePointCollection.Parse("8,44 28,8 52,22 74,44"),
            Fill = shapeFill,
            Stroke = shapeStroke
        };
        var path = new FWPath
        {
            Data = "M 4,36 C 16,6 38,6 48,30 S 74,54 84,20",
            Fill = shapeFill,
            Stroke = shapeStroke
        };

        var snapshot = GalleryVisualsPage.CreateShapeControlsQaSnapshot(
            rectangle,
            ellipse,
            line,
            polyline,
            polygon,
            path);
        var text = GalleryVisualsPage.FormatShapeControlsVisualQa("Shape controls QA", snapshot);

        Assert.Equal(6, snapshot.ShapeCount);
        Assert.Equal(6, snapshot.EnabledCount);
        Assert.True(snapshot.HasAccentStroke);
        Assert.True(snapshot.HasFilledShapes);
        Assert.Equal(8, snapshot.RectangleRadiusX);
        Assert.Equal(8, snapshot.RectangleRadiusY);
        Assert.Equal(5, snapshot.PolylinePointCount);
        Assert.Equal(4, snapshot.PolygonPointCount);
        Assert.True(snapshot.HasPathData);
        Assert.Equal("curve", snapshot.PathMode);

        Assert.Contains("Shape controls QA", text, StringComparison.Ordinal);
        Assert.Contains("Shapes: 6", text, StringComparison.Ordinal);
        Assert.Contains("Enabled: 6/6", text, StringComparison.Ordinal);
        Assert.Contains("Accent stroke: on", text, StringComparison.Ordinal);
        Assert.Contains("Filled: on", text, StringComparison.Ordinal);
        Assert.Contains("Rectangle radius: 8/8", text, StringComparison.Ordinal);
        Assert.Contains("Polyline points: 5", text, StringComparison.Ordinal);
        Assert.Contains("Polygon points: 4", text, StringComparison.Ordinal);
        Assert.Contains("Path data: on (curve)", text, StringComparison.Ordinal);

        rectangle.IsEnabled = false;
        ellipse.IsEnabled = false;
        line.IsEnabled = false;
        polyline.IsEnabled = false;
        polygon.IsEnabled = false;
        path.IsEnabled = false;
        rectangle.RadiusX = 22;
        rectangle.RadiusY = 22;
        path.Data = "M 6,44 L 22,10 L 42,38 L 62,10 L 82,44 Z";

        var disabledSnapshot = GalleryVisualsPage.CreateShapeControlsQaSnapshot(
            rectangle,
            ellipse,
            line,
            polyline,
            polygon,
            path);
        var disabledText = GalleryVisualsPage.FormatShapeControlsVisualQa("Shape enabled state changed", disabledSnapshot);

        Assert.Equal(0, disabledSnapshot.EnabledCount);
        Assert.Equal(22, disabledSnapshot.RectangleRadiusX);
        Assert.Equal(22, disabledSnapshot.RectangleRadiusY);
        Assert.Equal("polygon", disabledSnapshot.PathMode);
        Assert.Contains("Enabled: 0/6", disabledText, StringComparison.Ordinal);
        Assert.Contains("Rectangle radius: 22/22", disabledText, StringComparison.Ordinal);
        Assert.Contains("Path data: on (polygon)", disabledText, StringComparison.Ordinal);
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
