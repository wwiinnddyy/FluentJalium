using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// WinUI Gallery's item-page layout: PageHeader (title, API details, docs/source menus,
/// theme toggle, copy-link, favorite), description, sample content, and related controls.
/// Links copy to the clipboard: Jalium exposes no launcher API, so opening a browser is
/// a documented adaptation rather than a silent failure.
/// </summary>
public sealed partial class GalleryItemView : UserControl
{
    private static readonly string FavoriteGlyphOutline = char.ConvertFromUtf32((int)SegoeFluentIcon.FavoriteStar);
    private static readonly string FavoriteGlyphFilled = char.ConvertFromUtf32((int)SegoeFluentIcon.FavoriteStarFill);

    private readonly GalleryLocalizationService _localization = new();
    private GalleryPage? _page;
    private bool _copyTipShown;

    internal bool IsHeaderVisible => HeaderGrid.Visibility == Visibility.Visible;

    public GalleryItemView()
    {
        InitializeComponent();

        ThemeButton.Click += (_, _) => CycleTheme();
        CopyButton.Click += (_, _) => CopySampleLink();
        FavoriteToggle.Click += (_, _) => ToggleFavorite();
    }

    public void ApplyNavigationParameter(object? parameter)
    {
        if (parameter is not GalleryPage page)
        {
            return;
        }

        _page = page;
        _copyTipShown = false;

        // WinUI Gallery's Home is not an item page: no PageHeader, just content.
        var showHeader = !string.Equals(page.GroupId, GalleryNavigationGroup.Home, StringComparison.Ordinal);
        HeaderGrid.Visibility = showHeader ? Visibility.Visible : Visibility.Collapsed;
        LinkRow.Visibility = showHeader ? Visibility.Visible : Visibility.Collapsed;

        TitlePresenter.Text = page.Title;
        DescriptionPresenter.Text = page.Description;
        SampleHost.Content = page.CreateContent();

        StatusPill.Visibility = page.Status == GalleryPageStatus.Stable
            ? Visibility.Collapsed
            : Visibility.Visible;
        StatusText.Text = page.Status.ToString();

        ThemeButton.Content = _localization.Text("item.theme");
        CopyButton.Content = _localization.Text("item.copyLink");
        InfoDrop.ToolTip = _localization.Text("item.apiDetails");
        DocsDrop.Content = _localization.Text("item.docs");
        SourceDrop.Content = _localization.Text("item.source");

        InfoDrop.Flyout = CreateApiFlyout(page);
        DocsDrop.Flyout = CreateDocsMenu(page);
        SourceDrop.Flyout = CreateSourceMenu(page);
        DocsDrop.Visibility = page.DocumentationLinks.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        RefreshFavoriteToggle();

        RelatedPanel.Children.Clear();
        foreach (var related in page.RelatedControls)
        {
            var chip = new TextBlock
            {
                Text = related,
                FontSize = 12
            };
            chip.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorSecondaryBrush");
            RelatedPanel.Children.Add(chip);
        }

        RelatedExpander.Visibility = page.RelatedControls.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        RelatedExpander.IsExpanded = false;
    }

    private void RefreshFavoriteToggle()
    {
        if (_page is null)
        {
            return;
        }

        var isFavorite = GalleryFavoritesService.Instance.IsFavorite(_page.UniqueId);
        FavoriteToggle.IsChecked = isFavorite;
        FavoriteGlyph.Text = isFavorite ? FavoriteGlyphFilled : FavoriteGlyphOutline;
    }

    private void ToggleFavorite()
    {
        if (_page is null)
        {
            return;
        }

        var isFavorite = GalleryFavoritesService.Instance.ToggleFavorite(_page.UniqueId);
        FavoriteToggle.IsChecked = isFavorite;
        FavoriteGlyph.Text = isFavorite ? FavoriteGlyphFilled : FavoriteGlyphOutline;
        GalleryFeedback.Info(_localization.Text(isFavorite ? "home.favoriteAdded" : "home.favoriteRemoved"));
    }

    private void CopySampleLink()
    {
        if (_page is null)
        {
            return;
        }

        CopyText(GalleryDeepLink.FormatSampleLink(_page));

        if (!_copyTipShown)
        {
            _copyTipShown = true;
            var tip = new FWTeachingTip
            {
                Target = CopyButton,
                Title = _localization.Text("item.copyLink"),
                Subtitle = _localization.Text("item.copyLinkHint"),
                IsLightDismissEnabled = true,
                PreferredPlacement = TeachingTipPlacementMode.Bottom
            };
            tip.IsOpen = true;
            ContentRoot.Children.Add(tip);
        }
    }

    private static void CopyText(string text)
    {
        try
        {
            Clipboard.SetText(text);
            GalleryFeedback.Copied(text);
        }
        catch
        {
            // Clipboard access can fail headless or under a locked session.
        }
    }

    private FWFlyout CreateApiFlyout(GalleryPage page)
    {
        var content = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };

        content.Children.Add(new FWTextBlock
        {
            Text = _localization.Text("item.namespace"),
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        });
        content.Children.Add(new FWTextBlock
        {
            Text = string.IsNullOrWhiteSpace(page.ApiNamespace)
                ? _localization.Text("item.noApiNamespace")
                : page.ApiNamespace,
            FontFamily = "Consolas",
            FontSize = 12,
            Foreground = ThemeBrush("TextPrimary")
        });

        content.Children.Add(new FWTextBlock
        {
            Text = _localization.Text("item.inheritance"),
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary")
        });

        if (page.BaseClasses.Count > 0)
        {
            content.Children.Add(new FWBreadcrumbBar
            {
                ItemsSource = page.BaseClasses,
                IsHitTestVisible = false
            });
        }
        else
        {
            content.Children.Add(new FWTextBlock
            {
                Text = _localization.Text("item.noBaseClasses"),
                FontSize = 12,
                Foreground = ThemeBrush("TextSecondary")
            });
        }

        return new FWFlyout { Content = content };
    }

    private FWMenuFlyout CreateDocsMenu(GalleryPage page)
    {
        var menu = new FWMenuFlyout();
        foreach (var link in page.DocumentationLinks)
        {
            var uri = link.Uri;
            var item = new FWMenuFlyoutItem { Text = link.Title };
            item.Click += (_, _) => CopyText(uri);
            menu.Items.Add(item);
        }

        return menu;
    }

    private FWMenuFlyout CreateSourceMenu(GalleryPage page)
    {
        var menu = new FWMenuFlyout();

        var firstControl = page.RelatedControls.FirstOrDefault(IsControlName) ?? page.Title;
        var controlItem = new FWMenuFlyoutItem
        {
            Text = $"{_localization.Text("item.controlSource")}: {firstControl}"
        };
        controlItem.Click += (_, _) => CopyText(GalleryDeepLink.CodeSearchUrl(firstControl));
        menu.Items.Add(controlItem);

        menu.Items.Add(new FWMenuFlyoutSeparator());

        foreach (var fileName in GallerySampleSourceRegistry.CandidateFileNames(page.Info))
        {
            var sampleItem = new FWMenuFlyoutItem
            {
                Text = $"{_localization.Text("item.sampleSource")}: {fileName}"
            };
            sampleItem.Click += (_, _) => CopyText(GalleryDeepLink.CodeSearchUrl(fileName));
            menu.Items.Add(sampleItem);
        }

        return menu;
    }

    private static bool IsControlName(string value)
    {
        return value.StartsWith("FW", StringComparison.Ordinal) && value.Length > 2;
    }

    private static void CycleTheme()
    {
        FluentThemeManager.ApplyTheme(FluentThemeManager.CurrentTheme switch
        {
            FluentThemeVariant.Dark => FluentThemeVariant.Light,
            FluentThemeVariant.Light => FluentThemeVariant.HighContrast,
            _ => FluentThemeVariant.Dark
        });
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
