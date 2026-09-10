using FluentJalium.Controls.Themes;
using FluentJalium.Controls;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Gallery.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls.Themes;

namespace FluentJalium.Gallery;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);
        app.Resources.MergedDictionaries.Add(new GalleryStyles());
        if (args.Contains("--selftest", StringComparer.OrdinalIgnoreCase))
        {
            var iconCount = 0;
            var entries = GalleryPages.All.Append(GalleryPages.About).Append(GalleryPages.Settings).ToArray();
            foreach (var entry in entries)
            {
                var icon = GalleryIcon.Create(entry.Icon);
                if (string.IsNullOrEmpty(icon.Glyph) || icon.FontFamily?.Source != FluentIcon.SegoeFontFamily)
                {
                    throw new InvalidOperationException($"Gallery icon '{entry.Key}' did not resolve to a visible Segoe glyph.");
                }
                iconCount++;
            }

            // Materialize the Jalxaml components as a headless smoke test. This catches missing
            // x:Name fields, resource keys, and constructor wiring before a window is shown.
            _ = new GalleryPageFrame("Smoke", "Jalxaml page frame");
            _ = new GalleryHero();
            _ = new GalleryExampleCard("Smoke", "Example card", new FWButton { Content = "Ready" }, "<fw:FWButton Content=\"Ready\" />");
            _ = new GalleryLinkCard(GalleryPages.All[0], _ => { });
            _ = new GalleryControlCatalogCard(typeof(FWButton), "Buttons");
            var inputSample = new BasicInputSample();
            var workspaces = inputSample.WorkspaceItems?.Cast<object?>().Select(value => value?.ToString()).ToArray();
            if (workspaces is not ["Design", "Engineering", "Research"])
            {
                throw new InvalidOperationException("BasicInputSample did not expose its typed workspace ItemsSource.");
            }

            // Verify that the application-level Jalxaml dictionary is available to controls created
            // after startup. This catches a common integration error where a page is materialized
            // before GalleryStyles or Fluent theme resources are merged.
            var requiredResources = new[]
            {
                "GalleryPageTitle", "GalleryCardTitle", "GalleryCodeText",
                "TextFillColorPrimaryBrush", "TextFillColorSecondaryBrush",
                "AccentFillColorDefaultBrush"
            };
            foreach (var key in requiredResources)
            {
                if (Application.Current?.Resources.TryGetValue(key, out var value) != true || value is null)
                {
                    throw new InvalidOperationException($"Gallery resource '{key}' was not resolved during Jalxaml self-test.");
                }
            }

            foreach (var entry in entries)
            {
                try
                {
                    _ = entry.CreateContent(_ => { });
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Gallery page '{entry.Key}' failed to materialize.", ex);
                }
            }

            var controlCount = typeof(FWButton).Assembly.GetTypes()
                .Count(type => !type.IsAbstract && typeof(FrameworkElement).IsAssignableFrom(type) && typeof(IFluentJaliumControl).IsAssignableFrom(type));
            Console.WriteLine($"FluentJalium Gallery initialized; {iconCount} icons, {entries.Length} pages, {controlCount} controls validated");
            return;
        }
        app.Run(new MainWindow());
    }
}
