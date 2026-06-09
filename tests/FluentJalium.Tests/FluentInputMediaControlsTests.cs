using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Ink;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentInputMediaControlsTests
{
    [Fact]
    public void FWInputMediaControls_ShouldComposeInsideLiquidGlassSurface()
    {
        var picker = new FWColorPicker
        {
            Color = Color.FromRgb(0x00, 0x78, 0xD4),
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true,
            IsCompact = true,
            ColorSpectrumShape = ColorSpectrumShape.Ring
        };
        var drawingAttributes = new DrawingAttributes
        {
            Color = Color.FromRgb(0x4C, 0xC2, 0xFF),
            Width = 4,
            Height = 4,
            BrushType = BrushType.Pen,
            FitToCurve = true
        };
        var strokes = new StrokeCollection
        {
            CreateStroke(Color.FromRgb(0x4C, 0xC2, 0xFF)),
            CreateStroke(Color.FromRgb(0xD8, 0x3B, 0x01))
        };
        var canvasBackground = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
        var canvasBorder = new SolidColorBrush(Color.FromRgb(0x4C, 0xC2, 0xFF));
        var canvas = new FWInkCanvas
        {
            Background = canvasBackground,
            BorderBrush = canvasBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Strokes = new StrokeCollection(),
            DefaultDrawingAttributes = drawingAttributes,
            DefaultStrokeTaperMode = StrokeTaperMode.TaperedEnd,
            EditingMode = InkCanvasEditingMode.Ink,
            EraserDiameter = 18
        };
        var presenter = new FWInkPresenter
        {
            Strokes = strokes
        };
        using var media = new FWMediaElement
        {
            Background = new SolidColorBrush(Color.Black),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Close,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            ScrubbingEnabled = true,
            IsMuted = true,
            SpeedRatio = 1.25,
            Position = TimeSpan.FromSeconds(7)
        };
        using var webView = new FWWebView
        {
            Width = 240,
            Height = 140,
            Source = new Uri("https://example.com/"),
            DefaultBackgroundColor = Colors.White,
            ZoomFactor = 1.25
        };
        var stack = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                picker,
                canvas,
                presenter,
                media,
                webView
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

        Assert.Equal(Color.FromRgb(0x00, 0x78, 0xD4), picker.Color);
        Assert.True(picker.IsAlphaEnabled);
        Assert.True(picker.IsColorPreviewVisible);
        Assert.True(picker.IsHexInputVisible);
        Assert.True(picker.IsCompact);
        Assert.Equal(ColorSpectrumShape.Ring, picker.ColorSpectrumShape);
        Assert.Same(canvasBackground, canvas.Background);
        Assert.Same(canvasBorder, canvas.BorderBrush);
        Assert.Equal(1, canvas.BorderThickness.Left);
        Assert.Equal(6, canvas.CornerRadius.TopLeft);
        Assert.Same(drawingAttributes, canvas.DefaultDrawingAttributes);
        Assert.Equal(StrokeTaperMode.TaperedEnd, canvas.DefaultStrokeTaperMode);
        Assert.Equal(InkCanvasEditingMode.Ink, canvas.EditingMode);
        Assert.Equal(18, canvas.EraserDiameter);
        Assert.Same(strokes, presenter.Strokes);
        Assert.Equal(2, presenter.Strokes!.Count);
        Assert.Equal(MediaState.Manual, media.LoadedBehavior);
        Assert.Equal(MediaState.Close, media.UnloadedBehavior);
        Assert.Equal(Stretch.Uniform, media.Stretch);
        Assert.Equal(StretchDirection.Both, media.StretchDirection);
        Assert.True(media.ScrubbingEnabled);
        Assert.True(media.IsMuted);
        Assert.Equal(1.25, media.SpeedRatio);
        media.Stop();
        Assert.Equal(TimeSpan.Zero, media.Position);
        Assert.Equal(1, media.BorderThickness.Left);
        Assert.Equal(6, media.CornerRadius.TopLeft);
        var webViewDiagnostics = webView.GetDiagnostics();
        Assert.Equal(new Uri("https://example.com/"), webViewDiagnostics.Source);
        Assert.False(webViewDiagnostics.IsInitialized);
        Assert.False(webViewDiagnostics.IsNavigating);
        Assert.Equal(1.25, webViewDiagnostics.ZoomFactor);
        Assert.Equal(Colors.White, webViewDiagnostics.DefaultBackgroundColor);
        Assert.Null(webViewDiagnostics.InitializationError);
        Assert.Equal(10, stack.Spacing);
        Assert.Equal(5, stack.Children.Count);
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
    public void FWWebView_ShouldExposeSafeDiagnosticsBeforeWebView2Initialization()
    {
        using var webView = new FWWebView
        {
            Width = 320,
            Height = 180,
            DefaultBackgroundColor = Color.FromRgb(0xF9, 0xF9, 0xF9),
            ZoomFactor = 8
        };

        webView.Navigate("https://example.com/");
        webView.NavigateToString("<html><title>Inline</title></html>");
        var diagnostics = webView.GetDiagnostics();

        Assert.IsAssignableFrom<WebView>(webView);
        Assert.IsAssignableFrom<IFluentJaliumControl>(webView);
        Assert.Equal(new Uri("https://example.com/"), diagnostics.Source);
        Assert.Equal(string.Empty, diagnostics.DocumentTitle);
        Assert.False(diagnostics.CanGoBack);
        Assert.False(diagnostics.CanGoForward);
        Assert.False(diagnostics.IsInitialized);
        Assert.False(diagnostics.IsNavigating);
        Assert.Equal(4.0, diagnostics.ZoomFactor);
        Assert.Equal(Color.FromRgb(0xF9, 0xF9, 0xF9), diagnostics.DefaultBackgroundColor);
        Assert.Null(diagnostics.InitializationError);
    }

    private static Stroke CreateStroke(Color color)
    {
        return new Stroke(
            new StylusPointCollection(new[]
            {
                new StylusPoint(12, 24),
                new StylusPoint(52, 38),
                new StylusPoint(96, 20)
            }),
            new DrawingAttributes
            {
                Color = color,
                Width = 4,
                Height = 4,
                BrushType = BrushType.Pen,
                FitToCurve = true
            })
        {
            TaperMode = StrokeTaperMode.TaperedEnd
        };
    }
}
