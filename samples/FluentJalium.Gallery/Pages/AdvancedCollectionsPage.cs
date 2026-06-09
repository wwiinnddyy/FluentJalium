using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Data;
using FluentJalium.Controls;
using FWBorder = FluentJalium.Controls.FWBorder;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Gallery page demonstrating advanced collection controls.
/// </summary>
public class AdvancedCollectionsPage : Page
{
    private FWItemsRepeater? _repeater;
    private FWScroller? _repeaterScroller;
    private ScrollViewer? _repeaterScrollViewer;
    private TextBlock? _repeaterScenarioText;
    private TextBlock? _repeaterDiagnosticsText;
    private FWItemsRepeater? _itemsViewRecipeRepeater;
    private FWItemsRepeater? _flipViewRecipeRepeater;
    private FWItemsRepeater? _semanticZoomRecipeRepeater;
    private FWPipsPager? _flipViewPager;
    private TextBlock? _itemsViewRecipeText;
    private TextBlock? _flipViewRecipeText;
    private TextBlock? _flipViewPreviewText;
    private TextBlock? _semanticZoomRecipeText;
    private TextBlock? _semanticZoomPreviewText;
    private ObservableCollection<SampleItem> _items;
    private ItemsRepeaterGalleryProfile _currentProfile;
    private CollectionRecipeState _itemsViewRecipeState;
    private CollectionRecipeState _flipViewRecipeState;
    private CollectionRecipeState _semanticZoomRecipeState;
    private int _cacheProfileIndex;
    private bool _suppressFlipPagerChange;
    private string _lastQaAction = "Initialized";

    internal enum ItemsRepeaterGalleryScenario
    {
        Baseline,
        LargeListStress,
        HorizontalVirtualization
    }

    internal readonly record struct ItemsRepeaterGalleryProfile(
        ItemsRepeaterGalleryScenario Scenario,
        string Name,
        string Intent,
        int ItemCount,
        string ItemTitlePrefix,
        Orientation Orientation,
        bool UseUniformGridLayout,
        double EstimatedItemExtent,
        double HorizontalCacheLength,
        double VerticalCacheLength,
        double ViewportStart,
        double ViewportLength,
        FWItemsRepeaterViewportSource PreferredViewportSource)
    {
        public double ActiveCacheLength => Orientation == Orientation.Horizontal
            ? HorizontalCacheLength
            : VerticalCacheLength;
    }

    internal readonly record struct ItemsRepeaterCacheProfile(
        string Name,
        double HorizontalCacheLength,
        double VerticalCacheLength);

    internal readonly record struct ItemsRepeaterVisualQaSnapshot(
        string ProfileName,
        string LastAction,
        string ViewportSource,
        string RealizedRange,
        string RequestedRange,
        string CacheSummary,
        bool IsVirtualized,
        bool HasViewportWindow,
        bool HasStableRange,
        bool HasCacheCoverage,
        bool MatchesProfileAxis,
        bool MatchesProfileScale,
        bool HasAttachedViewport,
        bool HasRecyclerEvidence)
    {
        public bool IsVisualQaReady =>
            IsVirtualized
            && HasViewportWindow
            && HasStableRange
            && HasCacheCoverage
            && MatchesProfileAxis
            && MatchesProfileScale
            && HasAttachedViewport;
    }

    internal enum CollectionRecipeKind
    {
        ItemsViewSelection,
        FlipViewPaging,
        SemanticZoomGrouping
    }

    internal enum CollectionRecipeCommand
    {
        Previous,
        Next,
        Home,
        End,
        Invoke,
        SelectIndex,
        ToggleZoom,
        SelectPreviousGroup,
        SelectNextGroup
    }

    internal readonly record struct CollectionRecipeItem(
        int Index,
        string Group,
        string Title,
        string Description);

    internal readonly record struct CollectionRecipeState(
        CollectionRecipeKind Kind,
        string Name,
        int ItemCount,
        int SelectedIndex,
        int InvokedIndex,
        int PageIndex,
        int GroupIndex,
        bool IsZoomedOut,
        string LastInput)
    {
        public bool HasInvocation => InvokedIndex >= 0;
    }

    internal readonly record struct CollectionNavigationEvaluation(
        CollectionRecipeKind Kind,
        string CandidateControl,
        string RecommendedSurface,
        bool HasKeyboardNavigation,
        bool HasSelectionSemantics,
        bool HasViewportBehavior,
        bool HasVirtualizationBehavior,
        string[] RecipeEvidence,
        string[] MissingPublicApiEvidence,
        string[] ProvenSemantics,
        string[] RemainingRisks)
    {
        public int ProvenSemanticCount =>
            Convert.ToInt32(HasKeyboardNavigation)
            + Convert.ToInt32(HasSelectionSemantics)
            + Convert.ToInt32(HasViewportBehavior)
            + Convert.ToInt32(HasVirtualizationBehavior);

        public bool HasRecipeEvidence => RecipeEvidence.Length > 0 && ProvenSemanticCount == 4;

        public bool HasMissingPublicApiEvidence => MissingPublicApiEvidence.Length > 0;

        public bool IsPublicApiReady =>
            RemainingRisks.Length == 0
            && MissingPublicApiEvidence.Length == 0
            && ProvenSemanticCount == 4;
    }

    public AdvancedCollectionsPage()
    {
        Title = "Advanced Collections";
        _currentProfile = CreateItemsRepeaterQaProfile(ItemsRepeaterGalleryScenario.Baseline);
        _items = CreateItemsRepeaterSampleItems(_currentProfile);
        _itemsViewRecipeState = CreateCollectionRecipeState(CollectionRecipeKind.ItemsViewSelection);
        _flipViewRecipeState = CreateCollectionRecipeState(CollectionRecipeKind.FlipViewPaging);
        _semanticZoomRecipeState = CreateCollectionRecipeState(CollectionRecipeKind.SemanticZoomGrouping);
        InitializeComponent();
    }

    public UIElement CreateContent()
    {
        return Content is UIElement element
            ? element
            : new StackPanel();
    }

    private void InitializeComponent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24)
        };

        var mainStack = new StackPanel { Spacing = 32 };

        // FWItemsRepeater Section
        mainStack.Children.Add(CreateSectionHeader("FWItemsRepeater",
            "High-performance virtualizing list with flexible layouts"));
        mainStack.Children.Add(CreateItemsRepeaterSection());

        mainStack.Children.Add(CreateSectionHeader("Collection navigation recipes",
            "ItemsView-like selection, FlipView-like paging, and SemanticZoom-like grouping built from existing primitives"));
        mainStack.Children.Add(CreateCollectionRecipesSection());

        scrollViewer.Content = mainStack;
        Content = scrollViewer;
    }

    private UIElement CreateSectionHeader(string title, string description)
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 24,
            FontWeight = FontWeights.SemiBold
        });

        stack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 14,
            Opacity = 0.7
        });

        return stack;
    }

    private UIElement CreateItemsRepeaterSection()
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(400) }
            },
            RowSpacing = 16
        };

        // Layout controls
        var controlPanel = CreateLayoutControls();
        Grid.SetRow(controlPanel, 0);

        // ItemsRepeater container
        var repeaterContainer = CreateItemsRepeaterDemo();
        Grid.SetRow(repeaterContainer, 1);

        mainGrid.Children.Add(controlPanel);
        mainGrid.Children.Add(repeaterContainer);

        return mainGrid;
    }

    private UIElement CreateCollectionRecipesSection()
    {
        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                CreateItemsViewRecipe(),
                CreateFlipViewRecipe(),
                CreateSemanticZoomRecipe(),
                CreateCollectionNavigationEvaluationPanel()
            }
        };
    }

    private UIElement CreateItemsViewRecipe()
    {
        _itemsViewRecipeText = CreateRecipeOutput();
        _itemsViewRecipeRepeater = CreateRecipeRepeater(_itemsViewRecipeState);
        var scrollViewer = CreateRecipeViewport(_itemsViewRecipeRepeater, 220);
        _itemsViewRecipeRepeater.AttachViewport(scrollViewer);

        var content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                CreateRecipeButtonRow(
                    CreateRecipeButton("Previous", () => ApplyItemsViewRecipeCommand(CollectionRecipeCommand.Previous)),
                    CreateRecipeButton("Next", () => ApplyItemsViewRecipeCommand(CollectionRecipeCommand.Next)),
                    CreateRecipeButton("Home", () => ApplyItemsViewRecipeCommand(CollectionRecipeCommand.Home)),
                    CreateRecipeButton("End", () => ApplyItemsViewRecipeCommand(CollectionRecipeCommand.End)),
                    CreateRecipeButton("Invoke", () => ApplyItemsViewRecipeCommand(CollectionRecipeCommand.Invoke))),
                scrollViewer,
                _itemsViewRecipeText
            }
        };

        UpdateCollectionRecipeVisuals(_itemsViewRecipeRepeater, _itemsViewRecipeState, _itemsViewRecipeText);

        return CreateRecipeSurface(
            "ItemsView-like selection recipe",
            "Proves selected item, invocation, keyboard-style movement, and viewport realization without publishing a new ItemsView API.",
            content);
    }

    private UIElement CreateFlipViewRecipe()
    {
        _flipViewRecipeText = CreateRecipeOutput();
        _flipViewPreviewText = CreateRecipeOutput(fontSize: 16, opacity: 0.9);
        _flipViewRecipeRepeater = CreateRecipeRepeater(_flipViewRecipeState);
        _flipViewPager = new FWPipsPager
        {
            Width = 500,
            Height = 40,
            NumberOfPages = _flipViewRecipeState.ItemCount,
            MaxVisiblePips = 5,
            SelectedPageIndex = _flipViewRecipeState.PageIndex
        };
        _flipViewPager.SelectedIndexChanged += (_, e) =>
        {
            if (_suppressFlipPagerChange)
            {
                return;
            }

            ApplyFlipViewRecipeCommand(CollectionRecipeCommand.SelectIndex, e.NewIndex);
        };

        var scrollViewer = CreateRecipeViewport(_flipViewRecipeRepeater, 128);
        _flipViewRecipeRepeater.AttachViewport(scrollViewer, Orientation.Horizontal);

        var content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                CreatePreviewSurface(_flipViewPreviewText),
                _flipViewPager,
                CreateRecipeButtonRow(
                    CreateRecipeButton("Previous page", () => ApplyFlipViewRecipeCommand(CollectionRecipeCommand.Previous)),
                    CreateRecipeButton("Next page", () => ApplyFlipViewRecipeCommand(CollectionRecipeCommand.Next)),
                    CreateRecipeButton("First", () => ApplyFlipViewRecipeCommand(CollectionRecipeCommand.Home)),
                    CreateRecipeButton("Last", () => ApplyFlipViewRecipeCommand(CollectionRecipeCommand.End)),
                    CreateRecipeButton("Invoke page", () => ApplyFlipViewRecipeCommand(CollectionRecipeCommand.Invoke))),
                scrollViewer,
                _flipViewRecipeText
            }
        };

        UpdateFlipViewRecipeVisuals();

        return CreateRecipeSurface(
            "FlipView-like paging recipe",
            "Uses FWPipsPager plus FWItemsRepeater viewport windows to validate paged navigation before a dedicated FlipView surface exists.",
            content);
    }

    private UIElement CreateSemanticZoomRecipe()
    {
        _semanticZoomRecipeText = CreateRecipeOutput();
        _semanticZoomPreviewText = CreateRecipeOutput(fontSize: 16, opacity: 0.9);
        _semanticZoomRecipeRepeater = CreateRecipeRepeater(_semanticZoomRecipeState);
        var scrollViewer = CreateRecipeViewport(_semanticZoomRecipeRepeater, 190);
        _semanticZoomRecipeRepeater.AttachViewport(scrollViewer);

        var content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                CreatePreviewSurface(_semanticZoomPreviewText),
                CreateRecipeButtonRow(
                    CreateRecipeButton("Previous group", () => ApplySemanticZoomRecipeCommand(CollectionRecipeCommand.SelectPreviousGroup)),
                    CreateRecipeButton("Next group", () => ApplySemanticZoomRecipeCommand(CollectionRecipeCommand.SelectNextGroup)),
                    CreateRecipeButton("Overview/details", () => ApplySemanticZoomRecipeCommand(CollectionRecipeCommand.ToggleZoom)),
                    CreateRecipeButton("Open group", () => ApplySemanticZoomRecipeCommand(CollectionRecipeCommand.Invoke))),
                scrollViewer,
                _semanticZoomRecipeText
            }
        };

        UpdateSemanticZoomRecipeVisuals();

        return CreateRecipeSurface(
            "SemanticZoom-like grouping recipe",
            "Keeps grouped overview/detail navigation as a Gallery recipe while Jalium lacks a native SemanticZoom base.",
            content);
    }

    private UIElement CreateCollectionNavigationEvaluationPanel()
    {
        var evaluations = CreateCollectionNavigationEvaluations();
        var content = new StackPanel
        {
            Spacing = 10
        };

        content.Children.Add(new TextBlock
        {
            Text = CreateCollectionNavigationEvaluationSummary(evaluations),
            FontSize = 13,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(new TextBlock
        {
            Text = CreateCollectionNavigationEvidenceSummary(evaluations),
            FontSize = 12,
            Opacity = 0.72,
            TextWrapping = TextWrapping.Wrap
        });

        foreach (var evaluation in evaluations)
        {
            content.Children.Add(CreateCollectionNavigationEvaluationRow(evaluation));
        }

        return CreateRecipeSurface(
            "Collection navigation API evaluation",
            "Keeps ItemsView, FlipView, and SemanticZoom candidates honest: Gallery recipes can prove behavior before FluentJalium publishes a stable FW control API.",
            content);
    }

    private static UIElement CreateCollectionNavigationEvaluationRow(CollectionNavigationEvaluation evaluation)
    {
        var stack = new StackPanel
        {
            Spacing = 6
        };

        stack.Children.Add(new TextBlock
        {
            Text = evaluation.CandidateControl,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text = FormatCollectionNavigationEvaluation(evaluation),
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = FormatCollectionNavigationEvidence(evaluation),
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap
        });

        return new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xCF, 0xDA, 0xEE)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = stack
        };
    }

    private UIElement CreateRecipeSurface(string title, string description, UIElement content)
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 13,
            Opacity = 0.72,
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(content);

        return new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14),
            Child = stack
        };
    }

    private UIElement CreateLayoutControls()
    {
        var stack = new StackPanel
        {
            Spacing = 10
        };

        var panel = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        var stackLayoutButton = new Button
        {
            Content = "Stack Layout",
            MinWidth = 120
        };
        stackLayoutButton.Click += (s, e) => SwitchToStackLayout();

        var gridLayoutButton = new Button
        {
            Content = "Grid Layout",
            MinWidth = 120
        };
        gridLayoutButton.Click += (s, e) => SwitchToGridLayout();

        var addItemButton = new Button
        {
            Content = "Add Item",
            MinWidth = 100
        };
        addItemButton.Click += (s, e) => AddItem();

        var removeItemButton = new Button
        {
            Content = "Remove Item",
            MinWidth = 100
        };
        removeItemButton.Click += (s, e) => RemoveItem();

        var baselineButton = new Button
        {
            Content = "Baseline 20",
            MinWidth = 110
        };
        baselineButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.Baseline);

        var largeListButton = new Button
        {
            Content = "Large list",
            MinWidth = 110
        };
        largeListButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.LargeListStress);

        var horizontalButton = new Button
        {
            Content = "Horizontal",
            MinWidth = 110
        };
        horizontalButton.Click += (s, e) => ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario.HorizontalVirtualization);

        var firstWindowButton = new Button
        {
            Content = "Window 0-5",
            MinWidth = 100
        };
        firstWindowButton.Click += (s, e) => RealizeWindow(0, 5);

        var laterWindowButton = new Button
        {
            Content = "Window 8-5",
            MinWidth = 100
        };
        laterWindowButton.Click += (s, e) => RealizeWindow(8, 5);

        var allItemsButton = new Button
        {
            Content = "All",
            MinWidth = 80
        };
        allItemsButton.Click += (s, e) => ResetRealizationWindow();

        var viewportButton = new Button
        {
            Content = "Viewport",
            MinWidth = 100
        };
        viewportButton.Click += (s, e) => AttachViewportSource(_currentProfile.PreferredViewportSource, "Restored seeded viewport");

        var scrollViewerButton = new Button
        {
            Content = "Attach ScrollViewer",
            MinWidth = 150
        };
        scrollViewerButton.Click += (s, e) => AttachViewportSource(FWItemsRepeaterViewportSource.ScrollViewer, "Attached raw ScrollViewer");

        var scrollerButton = new Button
        {
            Content = "Attach FWScroller",
            MinWidth = 140
        };
        scrollerButton.Click += (s, e) => AttachViewportSource(FWItemsRepeaterViewportSource.Scroller, "Attached FWScroller");

        var reattachButton = new Button
        {
            Content = "Reattach swap",
            MinWidth = 130
        };
        reattachButton.Click += (s, e) => ReattachViewportSource();

        var cacheButton = new Button
        {
            Content = "Cycle cache",
            MinWidth = 120
        };
        cacheButton.Click += (s, e) => CycleViewportCache();

        _repeaterScenarioText = new TextBlock
        {
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Opacity = 0.86,
            TextWrapping = TextWrapping.Wrap
        };

        _repeaterDiagnosticsText = new TextBlock
        {
            FontSize = 13,
            Opacity = 0.72,
            TextWrapping = TextWrapping.Wrap
        };

        panel.Children.Add(stackLayoutButton);
        panel.Children.Add(gridLayoutButton);
        panel.Children.Add(addItemButton);
        panel.Children.Add(removeItemButton);
        panel.Children.Add(baselineButton);
        panel.Children.Add(largeListButton);
        panel.Children.Add(horizontalButton);
        panel.Children.Add(firstWindowButton);
        panel.Children.Add(laterWindowButton);
        panel.Children.Add(allItemsButton);
        panel.Children.Add(viewportButton);
        panel.Children.Add(scrollViewerButton);
        panel.Children.Add(scrollerButton);
        panel.Children.Add(reattachButton);
        panel.Children.Add(cacheButton);

        stack.Children.Add(panel);
        stack.Children.Add(_repeaterScenarioText);
        stack.Children.Add(_repeaterDiagnosticsText);

        return stack;
    }

    private UIElement CreateItemsRepeaterDemo()
    {
        var border = new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF9, 0xF9)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12)
        };

        _repeaterScroller = new FWScroller
        {
            VerticalScrollMode = ScrollMode.Auto,
            HorizontalScrollMode = ScrollMode.Disabled
        };

        _repeaterScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        _repeater = new FWItemsRepeater
        {
            ItemsSource = _items,
            ItemTemplate = CreateItemTemplate(),
            Layout = CreateItemsRepeaterLayout(_currentProfile),
            HorizontalCacheLength = _currentProfile.HorizontalCacheLength,
            VerticalCacheLength = _currentProfile.VerticalCacheLength,
            EstimatedItemExtent = _currentProfile.EstimatedItemExtent
        };

        _repeaterScrollViewer.Content = _repeater;
        _repeaterScroller.AttachScrollViewer(_repeaterScrollViewer);
        border.Child = _repeaterScrollViewer;
        UpdateScrollModes(_currentProfile);
        AttachViewportSource(_currentProfile.PreferredViewportSource, "Initialized seeded viewport");
        UpdateRepeaterDiagnostics();

        return border;
    }

    private DataTemplate CreateItemTemplate()
    {
        var template = new DataTemplate();

        template.SetVisualTree(() =>
        {
            var border = new FWBorder
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                MinWidth = 168
            };

            var stack = new StackPanel { Spacing = 4 };

            var titleBlock = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Medium
            };
            titleBlock.SetBinding(TextBlock.TextProperty, new Binding("Title"));

            var descBlock = new TextBlock
            {
                FontSize = 14,
                Opacity = 0.7
            };
            descBlock.SetBinding(TextBlock.TextProperty, new Binding("Description"));

            var statusBlock = new TextBlock
            {
                FontSize = 12,
                Opacity = 0.64
            };
            statusBlock.SetBinding(TextBlock.TextProperty, new Binding("Status"));

            stack.Children.Add(titleBlock);
            stack.Children.Add(descBlock);
            stack.Children.Add(statusBlock);
            border.Child = stack;

            return border;
        });

        template.Seal();
        return template;
    }

    private void SwitchToStackLayout()
    {
        if (_repeater != null)
        {
            _repeater.Layout = new StackLayout
            {
                Orientation = _currentProfile.Orientation,
                Spacing = 8
            };
            _lastQaAction = "Applied stack layout";
            UpdateRepeaterDiagnostics();
        }
    }

    private void SwitchToGridLayout()
    {
        if (_repeater != null)
        {
            _repeater.Layout = new UniformGridLayout
            {
                Orientation = _currentProfile.Orientation,
                MinItemWidth = 250,
                MinItemHeight = 80,
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
            _lastQaAction = "Applied grid layout";
            UpdateRepeaterDiagnostics();
        }
    }

    private void AddItem()
    {
        _items.Add(CreateItemsRepeaterSampleItem(_currentProfile, _items.Count + 1));
        _lastQaAction = "Added one item";
        UpdateRepeaterDiagnostics();
    }

    private void RemoveItem()
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
            _lastQaAction = "Removed one item";
            UpdateRepeaterDiagnostics();
        }
    }

    private void RealizeWindow(int startIndex, int itemCount)
    {
        _repeater?.RealizeRange(startIndex, itemCount);
        _lastQaAction = $"Manual range {startIndex}-{startIndex + itemCount - 1}";
        UpdateRepeaterDiagnostics();
    }

    private void ResetRealizationWindow()
    {
        _repeater?.DetachViewport();
        _repeater?.ResetRealizationWindow();
        _lastQaAction = "Reset to all items";
        UpdateRepeaterDiagnostics();
    }

    private void ApplyItemsRepeaterProfile(ItemsRepeaterGalleryScenario scenario)
    {
        _currentProfile = CreateItemsRepeaterQaProfile(scenario);
        _items = CreateItemsRepeaterSampleItems(_currentProfile);
        _cacheProfileIndex = 0;

        if (_repeater == null)
        {
            return;
        }

        _repeater.DetachViewport();
        _repeater.ItemsSource = _items;
        _repeater.Layout = CreateItemsRepeaterLayout(_currentProfile);
        _repeater.EstimatedItemExtent = _currentProfile.EstimatedItemExtent;
        _repeater.HorizontalCacheLength = _currentProfile.HorizontalCacheLength;
        _repeater.VerticalCacheLength = _currentProfile.VerticalCacheLength;
        UpdateScrollModes(_currentProfile);
        AttachViewportSource(_currentProfile.PreferredViewportSource, $"Applied {_currentProfile.Name}");
        UpdateRepeaterDiagnostics();
    }

    private void AttachViewportSource(FWItemsRepeaterViewportSource source, string action)
    {
        if (_repeater == null || _repeaterScrollViewer == null)
        {
            return;
        }

        if (source == FWItemsRepeaterViewportSource.Scroller)
        {
            if (_repeaterScroller == null)
            {
                return;
            }

            _repeaterScroller.AttachScrollViewer(_repeaterScrollViewer);
            _repeater.AttachViewport(_repeaterScroller, _currentProfile.Orientation);
        }
        else
        {
            _repeater.AttachViewport(_repeaterScrollViewer, _currentProfile.Orientation);
        }

        SeedViewportFromCurrentProfile();
        _lastQaAction = action;
        UpdateRepeaterDiagnostics();
    }

    private void ReattachViewportSource()
    {
        if (_repeater == null)
        {
            return;
        }

        var previousSource = _repeater.AttachedViewportSource;
        var nextSource = previousSource == FWItemsRepeaterViewportSource.Scroller
            ? FWItemsRepeaterViewportSource.ScrollViewer
            : FWItemsRepeaterViewportSource.Scroller;
        AttachViewportSource(nextSource, $"Reattached {previousSource} -> {nextSource}");
    }

    private void CycleViewportCache()
    {
        if (_repeater == null)
        {
            return;
        }

        var cacheProfiles = CreateItemsRepeaterCacheProfiles();
        _cacheProfileIndex = (_cacheProfileIndex + 1) % cacheProfiles.Count;
        var cacheProfile = cacheProfiles[_cacheProfileIndex];
        _repeater.HorizontalCacheLength = cacheProfile.HorizontalCacheLength;
        _repeater.VerticalCacheLength = cacheProfile.VerticalCacheLength;
        SeedViewportFromCurrentProfile();
        _lastQaAction = $"Cache profile {cacheProfile.Name}";
        UpdateRepeaterDiagnostics();
    }

    private void ApplyItemsViewRecipeCommand(CollectionRecipeCommand command, int? index = null)
    {
        _itemsViewRecipeState = ApplyCollectionRecipeCommand(_itemsViewRecipeState, command, index);
        UpdateCollectionRecipeVisuals(_itemsViewRecipeRepeater, _itemsViewRecipeState, _itemsViewRecipeText);
    }

    private void ApplyFlipViewRecipeCommand(CollectionRecipeCommand command, int? index = null)
    {
        _flipViewRecipeState = ApplyCollectionRecipeCommand(_flipViewRecipeState, command, index);
        UpdateFlipViewRecipeVisuals();
    }

    private void ApplySemanticZoomRecipeCommand(CollectionRecipeCommand command, int? index = null)
    {
        _semanticZoomRecipeState = ApplyCollectionRecipeCommand(_semanticZoomRecipeState, command, index);
        UpdateSemanticZoomRecipeVisuals();
    }

    private void UpdateFlipViewRecipeVisuals()
    {
        if (_flipViewPager != null)
        {
            _suppressFlipPagerChange = true;
            _flipViewPager.SelectedPageIndex = _flipViewRecipeState.PageIndex;
            _suppressFlipPagerChange = false;
        }

        if (_flipViewPreviewText != null)
        {
            var item = GetCollectionRecipeItem(_flipViewRecipeState);
            _flipViewPreviewText.Text = $"{item.Title}: {item.Description}";
        }

        UpdateCollectionRecipeVisuals(_flipViewRecipeRepeater, _flipViewRecipeState, _flipViewRecipeText);
    }

    private void UpdateSemanticZoomRecipeVisuals()
    {
        if (_semanticZoomPreviewText != null)
        {
            var groups = CreateCollectionRecipeGroups();
            var group = groups[Math.Min(_semanticZoomRecipeState.GroupIndex, groups.Count - 1)];
            _semanticZoomPreviewText.Text = _semanticZoomRecipeState.IsZoomedOut
                ? $"Overview: {group} group selected. Toggle to inspect its first item."
                : $"Details: {GetCollectionRecipeItem(_semanticZoomRecipeState).Title} in {group}.";
        }

        UpdateCollectionRecipeVisuals(_semanticZoomRecipeRepeater, _semanticZoomRecipeState, _semanticZoomRecipeText);
    }

    private static void UpdateCollectionRecipeVisuals(
        FWItemsRepeater? repeater,
        CollectionRecipeState state,
        TextBlock? output)
    {
        if (repeater == null || output == null)
        {
            return;
        }

        var viewportStart = GetRecipeViewportStart(state);
        var viewportLength = GetRecipeViewportLength(state);
        var orientation = state.Kind == CollectionRecipeKind.FlipViewPaging
            ? Orientation.Horizontal
            : Orientation.Vertical;

        repeater.ItemsSource = CreateCollectionRecipeItems(state);
        repeater.Layout = CreateCollectionRecipeLayout(state);
        repeater.EstimatedItemExtent = GetRecipeEstimatedItemExtent(state);
        repeater.HorizontalCacheLength = state.Kind == CollectionRecipeKind.FlipViewPaging ? 96 : 0;
        repeater.VerticalCacheLength = state.Kind == CollectionRecipeKind.FlipViewPaging ? 0 : 96;
        repeater.ApplyViewport(viewportStart, viewportLength, orientation);
        output.Text = CreateCollectionRecipeDiagnosticsText(state, repeater.GetDiagnostics());
    }

    private void UpdateRepeaterDiagnostics()
    {
        if (_repeater == null || _repeaterDiagnosticsText == null)
        {
            return;
        }

        var diagnostics = _repeater.GetDiagnostics();
        if (_repeaterScenarioText != null)
        {
            _repeaterScenarioText.Text = CreateItemsRepeaterScenarioText(_currentProfile, diagnostics, _lastQaAction);
        }

        var visualQaSnapshot = CreateItemsRepeaterVisualQaSnapshot(_currentProfile, diagnostics, _lastQaAction);
        _repeaterDiagnosticsText.Text =
            $"{CreateItemsRepeaterDiagnosticsText(diagnostics)}{Environment.NewLine}{FormatItemsRepeaterVisualQa(visualQaSnapshot)}";
    }

    private void SeedViewportFromCurrentProfile()
    {
        if (_repeater == null || _repeaterScrollViewer == null)
        {
            return;
        }

        if (_currentProfile.Orientation == Orientation.Horizontal)
        {
            _repeaterScrollViewer.ScrollToHorizontalOffset(_currentProfile.ViewportStart);
        }
        else
        {
            _repeaterScrollViewer.ScrollToVerticalOffset(_currentProfile.ViewportStart);
        }

        _repeater.ApplyViewport(
            _currentProfile.ViewportStart,
            _currentProfile.ViewportLength,
            _currentProfile.Orientation);
    }

    private void UpdateScrollModes(ItemsRepeaterGalleryProfile profile)
    {
        if (_repeaterScrollViewer != null)
        {
            _repeaterScrollViewer.HorizontalScrollBarVisibility = profile.Orientation == Orientation.Horizontal
                ? ScrollBarVisibility.Auto
                : ScrollBarVisibility.Disabled;
            _repeaterScrollViewer.VerticalScrollBarVisibility = profile.Orientation == Orientation.Horizontal
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;
        }

        if (_repeaterScroller != null)
        {
            _repeaterScroller.HorizontalScrollMode = profile.Orientation == Orientation.Horizontal
                ? ScrollMode.Auto
                : ScrollMode.Disabled;
            _repeaterScroller.VerticalScrollMode = profile.Orientation == Orientation.Horizontal
                ? ScrollMode.Disabled
                : ScrollMode.Auto;
        }
    }

    internal static ItemsRepeaterGalleryProfile CreateItemsRepeaterQaProfile(ItemsRepeaterGalleryScenario scenario)
    {
        return scenario switch
        {
            ItemsRepeaterGalleryScenario.LargeListStress => new ItemsRepeaterGalleryProfile(
                scenario,
                "Large-list stress",
                "1500 items seeded into a small viewport to verify recycler stability.",
                1500,
                "Stress Row",
                Orientation.Vertical,
                false,
                64,
                240,
                320,
                2560,
                480,
                FWItemsRepeaterViewportSource.Scroller),
            ItemsRepeaterGalleryScenario.HorizontalVirtualization => new ItemsRepeaterGalleryProfile(
                scenario,
                "Horizontal virtualization",
                "Horizontal stack window with horizontal cache and ScrollViewer reattachment checks.",
                96,
                "Lane",
                Orientation.Horizontal,
                false,
                180,
                360,
                80,
                720,
                540,
                FWItemsRepeaterViewportSource.ScrollViewer),
            _ => new ItemsRepeaterGalleryProfile(
                ItemsRepeaterGalleryScenario.Baseline,
                "Baseline viewport",
                "20 items with a seeded FWScroller viewport for quick visual QA.",
                20,
                "Sample Item",
                Orientation.Vertical,
                false,
                98,
                200,
                80,
                0,
                320,
                FWItemsRepeaterViewportSource.Scroller)
        };
    }

    internal static IReadOnlyList<ItemsRepeaterCacheProfile> CreateItemsRepeaterCacheProfiles()
    {
        return new[]
        {
            new ItemsRepeaterCacheProfile("Balanced", 200, 80),
            new ItemsRepeaterCacheProfile("Tight", 80, 40),
            new ItemsRepeaterCacheProfile("Stress buffer", 360, 260)
        };
    }

    internal static ObservableCollection<SampleItem> CreateItemsRepeaterSampleItems(ItemsRepeaterGalleryProfile profile)
    {
        if (profile.ItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(profile), "Item count cannot be negative.");
        }

        var items = new ObservableCollection<SampleItem>();
        for (var index = 1; index <= profile.ItemCount; index++)
        {
            items.Add(CreateItemsRepeaterSampleItem(profile, index));
        }

        return items;
    }

    internal static SampleItem CreateItemsRepeaterSampleItem(ItemsRepeaterGalleryProfile profile, int index)
    {
        if (index <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Item index must be positive.");
        }

        var paddedIndex = index.ToString("0000");
        return new SampleItem
        {
            Title = profile.Scenario == ItemsRepeaterGalleryScenario.Baseline
                ? $"{profile.ItemTitlePrefix} {index}"
                : $"{profile.ItemTitlePrefix} {paddedIndex}",
            Description = $"{profile.Name} QA item {index} of {profile.ItemCount}. {profile.Intent}",
            Status = $"{profile.Orientation} | extent {profile.EstimatedItemExtent:0}px | cache H{profile.HorizontalCacheLength:0}/V{profile.VerticalCacheLength:0}"
        };
    }

    internal static VirtualizingLayout CreateItemsRepeaterLayout(ItemsRepeaterGalleryProfile profile)
    {
        if (profile.UseUniformGridLayout)
        {
            return new UniformGridLayout
            {
                Orientation = profile.Orientation,
                MinItemWidth = profile.Orientation == Orientation.Horizontal ? 180 : 250,
                MinItemHeight = 80,
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
        }

        return new StackLayout
        {
            Orientation = profile.Orientation,
            Spacing = profile.Orientation == Orientation.Horizontal ? 10 : 8
        };
    }

    internal static string CreateItemsRepeaterScenarioText(
        ItemsRepeaterGalleryProfile profile,
        FWItemsRepeaterDiagnostics diagnostics,
        string lastAction)
    {
        var attachment = diagnostics.IsViewportAttached
            ? $"{diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}"
            : "detached";

        return
            $"Scenario: {profile.Name} | Goal: {profile.Intent} | Source: {attachment} | Items: {diagnostics.ItemCount} | Seed viewport: {profile.ViewportStart:0}-{profile.ViewportStart + profile.ViewportLength:0} | Last action: {lastAction}";
    }

    internal static string CreateItemsRepeaterDiagnosticsText(FWItemsRepeaterDiagnostics diagnostics)
    {
        var range = diagnostics.HasRealizedElements
            ? $"{diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}"
            : "none";
        var requested = diagnostics.RequestedRealizedItemCount > 0
            ? $"{diagnostics.RequestedFirstRealizedIndex}-{diagnostics.RequestedFirstRealizedIndex + diagnostics.RequestedRealizedItemCount - 1}"
            : "none";
        var viewportState = diagnostics.IsViewportAttached
            ? $"attached/{diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}"
            : "detached/manual";
        var virtualizationState = diagnostics.ItemCount > 0 && diagnostics.RealizedElementCount < diagnostics.ItemCount
            ? "virtualized"
            : "all-realized";

        return
            $"Mode: {diagnostics.RealizationMode}/{diagnostics.RealizationSource} ({viewportState}) | QA: {virtualizationState} | Axis: {diagnostics.ViewportOrientation} | Items: {diagnostics.ItemCount} | Realized: {diagnostics.RealizedElementCount} | Requested: {requested} | Range: {range} | Viewport: {diagnostics.ViewportStart:0}-{diagnostics.ViewportStart + diagnostics.ViewportLength:0} @ {diagnostics.EstimatedItemExtent:0}px | Reused: {diagnostics.LastReusedElementCount} | Pool: {diagnostics.RecycledElementCount} | Cache: active {diagnostics.ActiveCacheLength:0}, H{diagnostics.HorizontalCacheLength:0}/V{diagnostics.VerticalCacheLength:0}";
    }

    internal static ItemsRepeaterVisualQaSnapshot CreateItemsRepeaterVisualQaSnapshot(
        ItemsRepeaterGalleryProfile profile,
        FWItemsRepeaterDiagnostics diagnostics,
        string lastAction)
    {
        var realizedRange = diagnostics.HasRealizedElements
            ? $"{diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}"
            : "none";
        var requestedRange = diagnostics.RequestedRealizedItemCount > 0
            ? $"{diagnostics.RequestedFirstRealizedIndex}-{diagnostics.RequestedFirstRealizedIndex + diagnostics.RequestedRealizedItemCount - 1}"
            : "none";
        var viewportSource = diagnostics.IsViewportAttached
            ? $"{diagnostics.AttachedViewportSource}/{diagnostics.AttachedViewportOrientation}"
            : "detached";
        var expectedActiveCache = diagnostics.ViewportOrientation == Orientation.Horizontal
            ? diagnostics.HorizontalCacheLength
            : diagnostics.VerticalCacheLength;
        var hasStableRange =
            diagnostics.HasRealizedElements
            && diagnostics.FirstRealizedIndex >= 0
            && diagnostics.LastRealizedIndex >= diagnostics.FirstRealizedIndex
            && diagnostics.LastRealizedIndex < diagnostics.ItemCount;
        var hasAttachedViewport =
            diagnostics.IsViewportAttached
            && diagnostics.AttachedViewportSource != FWItemsRepeaterViewportSource.None
            && diagnostics.AttachedViewportOrientation == profile.Orientation;

        return new ItemsRepeaterVisualQaSnapshot(
            profile.Name,
            lastAction,
            viewportSource,
            realizedRange,
            requestedRange,
            $"active {diagnostics.ActiveCacheLength:0}, H{diagnostics.HorizontalCacheLength:0}/V{diagnostics.VerticalCacheLength:0}",
            diagnostics.ItemCount > 0
                && diagnostics.RealizedElementCount > 0
                && diagnostics.RealizedElementCount < diagnostics.ItemCount,
            diagnostics.RealizationSource == FWItemsRepeaterRealizationSource.Viewport
                && diagnostics.RequestedRealizedItemCount > 0
                && diagnostics.ViewportLength > 0,
            hasStableRange,
            Math.Abs(diagnostics.ActiveCacheLength - expectedActiveCache) < 0.1
                && diagnostics.EstimatedItemExtent > 0,
            diagnostics.ViewportOrientation == profile.Orientation,
            diagnostics.ItemCount == profile.ItemCount,
            hasAttachedViewport,
            diagnostics.LastReusedElementCount > 0 || diagnostics.RecycledElementCount > 0);
    }

    internal static string FormatItemsRepeaterVisualQa(ItemsRepeaterVisualQaSnapshot snapshot)
    {
        return
            $"ItemsRepeater visual QA: ready {FormatOnOff(snapshot.IsVisualQaReady)} | profile {snapshot.ProfileName} | virtualized {FormatOnOff(snapshot.IsVirtualized)} | viewport {FormatOnOff(snapshot.HasViewportWindow)} | range {FormatOnOff(snapshot.HasStableRange)} ({snapshot.RealizedRange}) | requested {snapshot.RequestedRange} | cache {FormatOnOff(snapshot.HasCacheCoverage)} ({snapshot.CacheSummary}) | axis {FormatOnOff(snapshot.MatchesProfileAxis)} | scale {FormatOnOff(snapshot.MatchesProfileScale)} | attached {FormatOnOff(snapshot.HasAttachedViewport)} | recycler {(snapshot.HasRecyclerEvidence ? "seen" : "pending")} | source {snapshot.ViewportSource} | action {snapshot.LastAction}";
    }

    internal static CollectionRecipeState CreateCollectionRecipeState(CollectionRecipeKind kind)
    {
        var itemCount = kind == CollectionRecipeKind.FlipViewPaging
            ? 6
            : 18;
        var state = new CollectionRecipeState(
            kind,
            kind switch
            {
                CollectionRecipeKind.FlipViewPaging => "FlipView-like paging",
                CollectionRecipeKind.SemanticZoomGrouping => "SemanticZoom-like grouping",
                _ => "ItemsView-like selection"
            },
            itemCount,
            0,
            -1,
            0,
            0,
            kind == CollectionRecipeKind.SemanticZoomGrouping,
            "Initialized");

        return NormalizeCollectionRecipeState(state);
    }

    internal static IReadOnlyList<CollectionNavigationEvaluation> CreateCollectionNavigationEvaluations()
    {
        return new[]
        {
            new CollectionNavigationEvaluation(
                CollectionRecipeKind.ItemsViewSelection,
                "FWItemsView",
                "Keep as Gallery recipe before public API",
                HasKeyboardNavigation: true,
                HasSelectionSemantics: true,
                HasViewportBehavior: true,
                HasVirtualizationBehavior: true,
                ["command recipe updates selected index", "invoke command records InvokedIndex", "FWItemsRepeater diagnostics prove viewport range and recycling"],
                ["owned selection model evidence", "item container automation metadata", "multi-select gesture parity"],
                ["Previous/Next/Home/End movement", "selected item and invoke tracking", "viewport-derived realization", "repeater-backed item recycling"],
                ["dedicated item container contract", "multi-select selection model", "automation peer contract"]),
            new CollectionNavigationEvaluation(
                CollectionRecipeKind.FlipViewPaging,
                "FWFlipView",
                "Keep as Gallery recipe before public API",
                HasKeyboardNavigation: true,
                HasSelectionSemantics: true,
                HasViewportBehavior: true,
                HasVirtualizationBehavior: true,
                ["FWPipsPager synchronizes page state", "horizontal viewport realization", "single-page command invocation"],
                ["pointer and touch swipe gesture trace", "page transition animation snapshot", "looping and edge behavior contract"],
                ["pips pager synchronization", "page selection and invocation", "horizontal viewport windows", "single-page realization"],
                ["touch swipe gesture host", "page transition animation contract", "looping and edge behavior"]),
            new CollectionNavigationEvaluation(
                CollectionRecipeKind.SemanticZoomGrouping,
                "FWSemanticZoom",
                "Keep as Gallery recipe before public API",
                HasKeyboardNavigation: true,
                HasSelectionSemantics: true,
                HasViewportBehavior: true,
                HasVirtualizationBehavior: true,
                ["group movement updates selected group", "overview/details toggle state", "group-aware viewport window"],
                ["two-view source synchronization trace", "zoom transition animation snapshot", "group automation focus contract"],
                ["group previous/next movement", "overview/details toggle", "first item selection per group", "group-aware viewport windows"],
                ["two-view synchronized source API", "zoom transition choreography", "group automation and focus contract"])
        };
    }

    internal static string CreateCollectionNavigationEvaluationSummary(IEnumerable<CollectionNavigationEvaluation> evaluations)
    {
        var items = evaluations.ToArray();
        var publicReady = items.Count(evaluation => evaluation.IsPublicApiReady);
        var recipeFirst = items.Length - publicReady;

        return $"Collection navigation evaluation: {items.Length} candidates, {recipeFirst} recipe-first, {publicReady} public-ready. Publish only after the remaining API, automation, gesture, and animation risks are closed.";
    }

    internal static string CreateCollectionNavigationEvidenceSummary(IEnumerable<CollectionNavigationEvaluation> evaluations)
    {
        var items = evaluations.ToArray();
        var recipeEvidence = items.Count(evaluation => evaluation.HasRecipeEvidence);
        var missingPublicApiEvidence = items.Sum(evaluation => evaluation.MissingPublicApiEvidence.Length);

        return $"Collection navigation evidence: {recipeEvidence}/{items.Length} candidates have Gallery recipe evidence; {missingPublicApiEvidence} missing public API evidence items remain. Keep FWItemsView, FWFlipView, and FWSemanticZoom recipe-first until those evidence gaps close.";
    }

    internal static string FormatCollectionNavigationEvaluation(CollectionNavigationEvaluation evaluation)
    {
        var proven = string.Join(", ", evaluation.ProvenSemantics);
        var risks = evaluation.RemainingRisks.Length == 0
            ? "none"
            : string.Join(", ", evaluation.RemainingRisks);
        var readiness = evaluation.IsPublicApiReady ? "public API ready" : "recipe/prototype";

        return $"{evaluation.CandidateControl}: {readiness}; surface: {evaluation.RecommendedSurface}; semantics keyboard {FormatOnOff(evaluation.HasKeyboardNavigation)}, selection {FormatOnOff(evaluation.HasSelectionSemantics)}, viewport {FormatOnOff(evaluation.HasViewportBehavior)}, virtualization {FormatOnOff(evaluation.HasVirtualizationBehavior)}; proven {proven}; remaining {risks}.";
    }

    internal static string FormatCollectionNavigationEvidence(CollectionNavigationEvaluation evaluation)
    {
        var recipeEvidence = string.Join(", ", evaluation.RecipeEvidence);
        var missingEvidence = evaluation.MissingPublicApiEvidence.Length == 0
            ? "none"
            : string.Join(", ", evaluation.MissingPublicApiEvidence);

        return $"{evaluation.CandidateControl} evidence: Gallery recipe evidence {FormatOnOff(evaluation.HasRecipeEvidence)} ({recipeEvidence}); missing public API evidence {missingEvidence}.";
    }

    private static string FormatOnOff(bool value) => value ? "on" : "off";

    internal static IReadOnlyList<string> CreateCollectionRecipeGroups()
    {
        return new[] { "Inbox", "Review", "Archive" };
    }

    internal static IReadOnlyList<CollectionRecipeItem> CreateCollectionRecipeItems(CollectionRecipeState state)
    {
        var groups = CreateCollectionRecipeGroups();
        var items = new List<CollectionRecipeItem>();

        for (var index = 0; index < state.ItemCount; index++)
        {
            var group = groups[index % groups.Count];
            items.Add(new CollectionRecipeItem(
                index,
                group,
                $"{group} item {index + 1:00}",
                state.Kind switch
                {
                    CollectionRecipeKind.FlipViewPaging => $"Paged item {index + 1} of {state.ItemCount}.",
                    CollectionRecipeKind.SemanticZoomGrouping => $"Grouped item for {group} overview/detail navigation.",
                    _ => $"Selectable item {index + 1} with keyboard and invocation semantics."
                }));
        }

        return items;
    }

    internal static CollectionRecipeState ApplyCollectionRecipeCommand(
        CollectionRecipeState state,
        CollectionRecipeCommand command,
        int? index = null)
    {
        var selectedIndex = state.SelectedIndex;
        var invokedIndex = state.InvokedIndex;
        var pageIndex = state.PageIndex;
        var groupIndex = state.GroupIndex;
        var isZoomedOut = state.IsZoomedOut;
        var lastInput = command.ToString();

        switch (command)
        {
            case CollectionRecipeCommand.Previous:
                selectedIndex--;
                pageIndex--;
                break;
            case CollectionRecipeCommand.Next:
                selectedIndex++;
                pageIndex++;
                break;
            case CollectionRecipeCommand.Home:
                selectedIndex = 0;
                pageIndex = 0;
                break;
            case CollectionRecipeCommand.End:
                selectedIndex = state.ItemCount - 1;
                pageIndex = state.ItemCount - 1;
                break;
            case CollectionRecipeCommand.Invoke:
                invokedIndex = state.Kind == CollectionRecipeKind.FlipViewPaging
                    ? state.PageIndex
                    : state.SelectedIndex;
                isZoomedOut = state.Kind == CollectionRecipeKind.SemanticZoomGrouping && state.IsZoomedOut
                    ? false
                    : state.IsZoomedOut;
                break;
            case CollectionRecipeCommand.SelectIndex:
                selectedIndex = index ?? selectedIndex;
                pageIndex = index ?? pageIndex;
                break;
            case CollectionRecipeCommand.ToggleZoom:
                isZoomedOut = !isZoomedOut;
                break;
            case CollectionRecipeCommand.SelectPreviousGroup:
                groupIndex--;
                break;
            case CollectionRecipeCommand.SelectNextGroup:
                groupIndex++;
                break;
        }

        return NormalizeCollectionRecipeState(state with
        {
            SelectedIndex = selectedIndex,
            InvokedIndex = invokedIndex,
            PageIndex = pageIndex,
            GroupIndex = groupIndex,
            IsZoomedOut = isZoomedOut,
            LastInput = lastInput
        });
    }

    internal static CollectionRecipeState NormalizeCollectionRecipeState(CollectionRecipeState state)
    {
        var itemCount = Math.Max(0, state.ItemCount);
        var lastIndex = Math.Max(0, itemCount - 1);
        var selectedIndex = itemCount == 0 ? -1 : Math.Clamp(state.SelectedIndex, 0, lastIndex);
        var pageIndex = itemCount == 0 ? -1 : Math.Clamp(state.PageIndex, 0, lastIndex);
        var groups = CreateCollectionRecipeGroups();
        var groupIndex = Math.Clamp(state.GroupIndex, 0, groups.Count - 1);

        if (state.Kind == CollectionRecipeKind.FlipViewPaging)
        {
            selectedIndex = pageIndex;
        }

        if (state.Kind == CollectionRecipeKind.SemanticZoomGrouping)
        {
            var selectedGroup = groups[groupIndex];
            var firstGroupItem = CreateCollectionRecipeItems(state with { ItemCount = itemCount })
                .Where(item => string.Equals(item.Group, selectedGroup, StringComparison.Ordinal))
                .Cast<CollectionRecipeItem?>()
                .FirstOrDefault();
            if (firstGroupItem is CollectionRecipeItem item)
            {
                selectedIndex = item.Index;
                pageIndex = item.Index;
            }
        }

        return state with
        {
            ItemCount = itemCount,
            SelectedIndex = selectedIndex,
            PageIndex = pageIndex,
            GroupIndex = groupIndex
        };
    }

    internal static string CreateCollectionRecipeDiagnosticsText(
        CollectionRecipeState state,
        FWItemsRepeaterDiagnostics diagnostics)
    {
        var realizedRange = diagnostics.HasRealizedElements
            ? $"{diagnostics.FirstRealizedIndex}-{diagnostics.LastRealizedIndex}"
            : "none";
        var item = GetCollectionRecipeItem(state);
        var invocation = state.HasInvocation
            ? state.InvokedIndex.ToString()
            : "none";
        var zoom = state.Kind == CollectionRecipeKind.SemanticZoomGrouping
            ? $" | Zoom: {(state.IsZoomedOut ? "overview" : "details")} group {CreateCollectionRecipeGroups()[state.GroupIndex]}"
            : string.Empty;

        return
            $"Recipe: {state.Name} | Selected: {state.SelectedIndex} ({item.Title}) | Invoked: {invocation} | Page: {state.PageIndex + 1}/{state.ItemCount} | Last input: {state.LastInput}{zoom} | Viewport: {diagnostics.ViewportStart:0}-{diagnostics.ViewportStart + diagnostics.ViewportLength:0} | Realized: {realizedRange}/{diagnostics.ItemCount} | Source: {diagnostics.RealizationSource}";
    }

    internal static VirtualizingLayout CreateCollectionRecipeLayout(CollectionRecipeState state)
    {
        return new StackLayout
        {
            Orientation = state.Kind == CollectionRecipeKind.FlipViewPaging
                ? Orientation.Horizontal
                : Orientation.Vertical,
            Spacing = 8
        };
    }

    internal static double GetRecipeEstimatedItemExtent(CollectionRecipeState state)
    {
        return state.Kind switch
        {
            CollectionRecipeKind.FlipViewPaging => 240,
            CollectionRecipeKind.SemanticZoomGrouping => state.IsZoomedOut ? 58 : 72,
            _ => 64
        };
    }

    internal static double GetRecipeViewportStart(CollectionRecipeState state)
    {
        var focusIndex = state.Kind == CollectionRecipeKind.FlipViewPaging
            ? state.PageIndex
            : state.SelectedIndex;
        return Math.Max(0, focusIndex) * GetRecipeEstimatedItemExtent(state);
    }

    internal static double GetRecipeViewportLength(CollectionRecipeState state)
    {
        return state.Kind == CollectionRecipeKind.FlipViewPaging
            ? GetRecipeEstimatedItemExtent(state)
            : GetRecipeEstimatedItemExtent(state) * 3;
    }

    internal static CollectionRecipeItem GetCollectionRecipeItem(CollectionRecipeState state)
    {
        var items = CreateCollectionRecipeItems(state);
        if (items.Count == 0 || state.SelectedIndex < 0)
        {
            return new CollectionRecipeItem(-1, string.Empty, "No item", string.Empty);
        }

        return items[Math.Clamp(state.SelectedIndex, 0, items.Count - 1)];
    }

    private static FWItemsRepeater CreateRecipeRepeater(CollectionRecipeState state)
    {
        return new FWItemsRepeater
        {
            ItemsSource = CreateCollectionRecipeItems(state),
            ItemTemplate = CreateRecipeItemTemplate(),
            Layout = CreateCollectionRecipeLayout(state),
            EstimatedItemExtent = GetRecipeEstimatedItemExtent(state),
            HorizontalCacheLength = state.Kind == CollectionRecipeKind.FlipViewPaging ? 96 : 0,
            VerticalCacheLength = state.Kind == CollectionRecipeKind.FlipViewPaging ? 0 : 96
        };
    }

    private static DataTemplate CreateRecipeItemTemplate()
    {
        var template = new DataTemplate();

        template.SetVisualTree(() =>
        {
            var border = new FWBorder
            {
                Background = new SolidColorBrush(Color.FromRgb(0xF7, 0xFA, 0xFF)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0xD8, 0xF2)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 8, 8),
                MinWidth = 220
            };

            var stack = new StackPanel { Spacing = 3 };
            var title = new TextBlock
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            title.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            var description = new TextBlock
            {
                FontSize = 12,
                Opacity = 0.7,
                TextWrapping = TextWrapping.Wrap
            };
            description.SetBinding(TextBlock.TextProperty, new Binding("Description"));
            stack.Children.Add(title);
            stack.Children.Add(description);
            border.Child = stack;

            return border;
        });

        template.Seal();
        return template;
    }

    private static ScrollViewer CreateRecipeViewport(UIElement content, double height)
    {
        return new ScrollViewer
        {
            Height = height,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = content
        };
    }

    private static FWBorder CreatePreviewSurface(UIElement content)
    {
        return new FWBorder
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF8, 0xFD)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0xE4, 0xF4)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = content
        };
    }

    private static TextBlock CreateRecipeOutput(double fontSize = 13, double opacity = 0.76)
    {
        return new TextBlock
        {
            FontSize = fontSize,
            Opacity = opacity,
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWWrapPanel CreateRecipeButtonRow(params UIElement[] buttons)
    {
        var panel = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        return panel;
    }

    private static Button CreateRecipeButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 104
        };
        button.Click += (_, _) => action();
        return button;
    }

    internal sealed class SampleItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
