using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media.Animation;
using System.Globalization;
using AnimationDuration = Jalium.UI.Media.Animation.Duration;
using AnimationTransitionMode = Jalium.UI.Media.Animation.TransitionMode;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium content transition presets inspired by WinUI navigation motion.
/// </summary>
public enum FWContentTransitionProfile
{
    /// <summary>
    /// Uses a calm crossfade for ordinary content replacement.
    /// </summary>
    Default,

    /// <summary>
    /// Uses a slide-in entrance profile for forward navigation.
    /// </summary>
    Entrance,

    /// <summary>
    /// Uses a stronger zoom-in profile for drill-in navigation.
    /// </summary>
    DrillIn,

    /// <summary>
    /// Uses a slide-from-left profile for back navigation.
    /// </summary>
    BackNavigation,

    /// <summary>
    /// Uses Jalium's HLSL liquid morph transition for high-material surfaces.
    /// </summary>
    LiquidMorph,

    /// <summary>
    /// Disables content transition animation.
    /// </summary>
    Suppress
}

/// <summary>
/// Resolved FluentJalium content transition settings.
/// </summary>
public readonly record struct FWContentTransitionRecipe(
    FWContentTransitionProfile Profile,
    AnimationTransitionMode? TransitionMode,
    AnimationDuration Duration,
    TransitionTimingFunction TimingFunction)
{
    /// <summary>
    /// Creates a transition recipe for the supplied profile.
    /// </summary>
    public static FWContentTransitionRecipe Create(FWContentTransitionProfile profile)
    {
        return Create(profile, Application.Current?.Resources);
    }

    /// <summary>
    /// Creates a transition recipe for the supplied profile from a resource scope.
    /// </summary>
    public static FWContentTransitionRecipe Create(FWContentTransitionProfile profile, ResourceDictionary? resources)
    {
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile), "Unknown content transition profile.");
        }

        return profile switch
        {
            FWContentTransitionProfile.Default => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionDefaultMode", AnimationTransitionMode.Crossfade),
                ResourceDuration(resources, "FluentMotionContentTransitionDefaultDuration", TimeSpan.FromMilliseconds(280)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionDefaultTimingFunction", TransitionTimingFunction.Recommended)),
            FWContentTransitionProfile.Entrance => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionEntranceMode", AnimationTransitionMode.SlideLeft),
                ResourceDuration(resources, "FluentMotionContentTransitionEntranceDuration", TimeSpan.FromMilliseconds(320)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionEntranceTimingFunction", TransitionTimingFunction.EaseOut)),
            FWContentTransitionProfile.DrillIn => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionDrillInMode", AnimationTransitionMode.ZoomIn),
                ResourceDuration(resources, "FluentMotionContentTransitionDrillInDuration", TimeSpan.FromMilliseconds(360)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionDrillInTimingFunction", TransitionTimingFunction.EaseInOut)),
            FWContentTransitionProfile.BackNavigation => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionBackNavigationMode", AnimationTransitionMode.SlideRight),
                ResourceDuration(resources, "FluentMotionContentTransitionBackNavigationDuration", TimeSpan.FromMilliseconds(280)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionBackNavigationTimingFunction", TransitionTimingFunction.EaseOut)),
            FWContentTransitionProfile.LiquidMorph => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionLiquidMorphMode", AnimationTransitionMode.LiquidMorph),
                ResourceDuration(resources, "FluentMotionContentTransitionLiquidMorphDuration", TimeSpan.FromMilliseconds(420)),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionLiquidMorphTimingFunction", TransitionTimingFunction.EaseInOut)),
            FWContentTransitionProfile.Suppress => new(
                profile,
                ResourceTransitionMode(resources, "FluentMotionContentTransitionSuppressMode", null),
                ResourceDuration(resources, "FluentMotionContentTransitionSuppressDuration", TimeSpan.Zero),
                ResourceTimingFunction(resources, "FluentMotionContentTransitionSuppressTimingFunction", TransitionTimingFunction.Linear)),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), "Unknown content transition profile.")
        };
    }

    private static AnimationDuration ResourceDuration(ResourceDictionary? resources, string key, TimeSpan fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return new AnimationDuration(fallback);
        }

        return value switch
        {
            AnimationDuration duration when duration.HasTimeSpan && duration.TimeSpan >= TimeSpan.Zero => duration,
            TimeSpan timeSpan when timeSpan >= TimeSpan.Zero => new AnimationDuration(timeSpan),
            double milliseconds when double.IsFinite(milliseconds) && milliseconds >= 0 => new AnimationDuration(TimeSpan.FromMilliseconds(milliseconds)),
            int milliseconds when milliseconds >= 0 => new AnimationDuration(TimeSpan.FromMilliseconds(milliseconds)),
            string text when TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var timeSpan) && timeSpan >= TimeSpan.Zero => new AnimationDuration(timeSpan),
            _ => new AnimationDuration(fallback)
        };
    }

    private static AnimationTransitionMode? ResourceTransitionMode(ResourceDictionary? resources, string key, AnimationTransitionMode? fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            null => null,
            AnimationTransitionMode mode => mode,
            string text when string.Equals(text, "None", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Suppress", StringComparison.OrdinalIgnoreCase) => null,
            string text when Enum.TryParse<AnimationTransitionMode>(text, ignoreCase: true, out var mode) => mode,
            _ => fallback
        };
    }

    private static TransitionTimingFunction ResourceTimingFunction(ResourceDictionary? resources, string key, TransitionTimingFunction fallback)
    {
        if (resources?.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            TransitionTimingFunction timingFunction => timingFunction,
            string text when Enum.TryParse<TransitionTimingFunction>(text, ignoreCase: true, out var timingFunction) => timingFunction,
            _ => fallback
        };
    }
}

/// <summary>
/// FluentJalium TextBlock control.
/// </summary>
public class FWTextBlock : TextBlock, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium AccessText control.
/// </summary>
public class FWAccessText : AccessText, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Canvas control.
/// </summary>
public class FWCanvas : Canvas, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Border control.
/// </summary>
public class FWBorder : Border, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium ContentControl control.
/// </summary>
public class FWContentControl : ContentControl, IFluentJaliumControl
{
}

/// <summary>
/// Describes how a <see cref="FWTwoPaneView"/> arranges its panes.
/// </summary>
public enum FWTwoPaneViewMode
{
    SinglePane,
    Wide,
    Tall
}

/// <summary>
/// Describes which pane remains visible when <see cref="FWTwoPaneView"/> is in single-pane mode.
/// </summary>
public enum FWTwoPaneViewPriority
{
    Pane1,
    Pane2
}

/// <summary>
/// FluentJalium TwoPaneView control for foldable and adaptive master-detail layouts.
/// </summary>
public class FWTwoPaneView : Control, IFluentJaliumControl
{
    public static readonly DependencyProperty Pane1Property =
        DependencyProperty.Register(nameof(Pane1), typeof(object), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane1TemplateProperty =
        DependencyProperty.Register(nameof(Pane1Template), typeof(DataTemplate), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane2Property =
        DependencyProperty.Register(nameof(Pane2), typeof(object), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty Pane2TemplateProperty =
        DependencyProperty.Register(nameof(Pane2Template), typeof(DataTemplate), typeof(FWTwoPaneView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(FWTwoPaneViewMode), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewMode.Wide, OnLayoutStateChanged), IsValidMode);

    public static readonly DependencyProperty PanePriorityProperty =
        DependencyProperty.Register(nameof(PanePriority), typeof(FWTwoPaneViewPriority), typeof(FWTwoPaneView),
            new PropertyMetadata(FWTwoPaneViewPriority.Pane1, OnLayoutStateChanged), IsValidPriority);

    public static readonly DependencyProperty MinWideModeWidthProperty =
        DependencyProperty.Register(nameof(MinWideModeWidth), typeof(double), typeof(FWTwoPaneView),
            new PropertyMetadata(641.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty MinTallModeHeightProperty =
        DependencyProperty.Register(nameof(MinTallModeHeight), typeof(double), typeof(FWTwoPaneView),
            new PropertyMetadata(641.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Pane1
    {
        get => GetValue(Pane1Property);
        set => SetValue(Pane1Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? Pane1Template
    {
        get => (DataTemplate?)GetValue(Pane1TemplateProperty);
        set => SetValue(Pane1TemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Pane2
    {
        get => GetValue(Pane2Property);
        set => SetValue(Pane2Property, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? Pane2Template
    {
        get => (DataTemplate?)GetValue(Pane2TemplateProperty);
        set => SetValue(Pane2TemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTwoPaneViewMode Mode
    {
        get => (FWTwoPaneViewMode)GetValue(ModeProperty)!;
        set => SetValue(ModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTwoPaneViewPriority PanePriority
    {
        get => (FWTwoPaneViewPriority)GetValue(PanePriorityProperty)!;
        set => SetValue(PanePriorityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double MinWideModeWidth
    {
        get => (double)GetValue(MinWideModeWidthProperty)!;
        set => SetValue(MinWideModeWidthProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double MinTallModeHeight
    {
        get => (double)GetValue(MinTallModeHeightProperty)!;
        set => SetValue(MinTallModeHeightProperty, value);
    }

    public object? ActivePane => PanePriority == FWTwoPaneViewPriority.Pane2 ? Pane2 : Pane1;

    private static void OnLayoutStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTwoPaneView twoPaneView)
        {
            twoPaneView.InvalidateMeasure();
            twoPaneView.InvalidateVisual();
        }
    }

    private static bool IsValidMode(object? value)
    {
        return value is FWTwoPaneViewMode mode && Enum.IsDefined(mode);
    }

    private static bool IsValidPriority(object? value)
    {
        return value is FWTwoPaneViewPriority priority && Enum.IsDefined(priority);
    }

    private static bool IsValidNonNegativeDouble(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0;
    }
}

/// <summary>
/// FluentJalium ParallaxView control for expressing scroll-linked depth.
/// </summary>
public class FWParallaxView : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(object), typeof(FWParallaxView),
            new PropertyMetadata(null, OnParallaxPropertyChanged));

    public static readonly DependencyProperty HorizontalShiftProperty =
        DependencyProperty.Register(nameof(HorizontalShift), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(0.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty VerticalShiftProperty =
        DependencyProperty.Register(nameof(VerticalShift), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(32.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty StartOffsetProperty =
        DependencyProperty.Register(nameof(StartOffset), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(0.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty EndOffsetProperty =
        DependencyProperty.Register(nameof(EndOffset), typeof(double), typeof(FWParallaxView),
            new PropertyMetadata(1.0, OnParallaxPropertyChanged), IsValidFiniteDouble);

    public static readonly DependencyProperty IsVerticalShiftEnabledProperty =
        DependencyProperty.Register(nameof(IsVerticalShiftEnabled), typeof(bool), typeof(FWParallaxView),
            new PropertyMetadata(true, OnParallaxPropertyChanged));

    public static readonly DependencyProperty IsHorizontalShiftEnabledProperty =
        DependencyProperty.Register(nameof(IsHorizontalShiftEnabled), typeof(bool), typeof(FWParallaxView),
            new PropertyMetadata(false, OnParallaxPropertyChanged));

    public FWParallaxView()
    {
        UseTemplateContentManagement();
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public object? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double HorizontalShift
    {
        get => (double)GetValue(HorizontalShiftProperty)!;
        set => SetValue(HorizontalShiftProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double VerticalShift
    {
        get => (double)GetValue(VerticalShiftProperty)!;
        set => SetValue(VerticalShiftProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double StartOffset
    {
        get => (double)GetValue(StartOffsetProperty)!;
        set => SetValue(StartOffsetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double EndOffset
    {
        get => (double)GetValue(EndOffsetProperty)!;
        set => SetValue(EndOffsetProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsVerticalShiftEnabled
    {
        get => (bool)GetValue(IsVerticalShiftEnabledProperty)!;
        set => SetValue(IsVerticalShiftEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsHorizontalShiftEnabled
    {
        get => (bool)GetValue(IsHorizontalShiftEnabledProperty)!;
        set => SetValue(IsHorizontalShiftEnabledProperty, value);
    }

    public Point GetParallaxOffset(double progress)
    {
        var normalized = Math.Clamp(progress, 0.0, 1.0);
        var span = EndOffset - StartOffset;
        var position = StartOffset + span * normalized;
        return new Point(
            IsHorizontalShiftEnabled ? HorizontalShift * position : 0.0,
            IsVerticalShiftEnabled ? VerticalShift * position : 0.0);
    }

    private static void OnParallaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWParallaxView parallaxView)
        {
            parallaxView.InvalidateMeasure();
            parallaxView.InvalidateVisual();
        }
    }

    private static bool IsValidFiniteDouble(object? value)
    {
        return value is double number && double.IsFinite(number);
    }
}

/// <summary>
/// FluentJalium RelativePanel compatibility surface.
/// </summary>
public class FWRelativePanel : Grid, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium SettingsCard control for compact settings rows.
/// </summary>
public class FWSettingsCard : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionTemplateProperty =
        DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty ActionIconProperty =
        DependencyProperty.Register(nameof(ActionIcon), typeof(object), typeof(FWSettingsCard),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty IsClickEnabledProperty =
        DependencyProperty.Register(nameof(IsClickEnabled), typeof(bool), typeof(FWSettingsCard),
            new PropertyMetadata(false));

    public FWSettingsCard()
    {
        UseTemplateContentManagement();
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? DescriptionTemplate
    {
        get => (DataTemplate?)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? ActionIcon
    {
        get => GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsClickEnabled
    {
        get => (bool)GetValue(IsClickEnabledProperty)!;
        set => SetValue(IsClickEnabledProperty, value);
    }

    private static void OnSettingsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsCard card)
        {
            card.InvalidateMeasure();
            card.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium SettingsExpander control for grouped settings rows.
/// </summary>
public class FWSettingsExpander : Expander, IFluentJaliumControl
{
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty DescriptionTemplateProperty =
        DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(FWSettingsExpander),
            new PropertyMetadata(null, OnSettingsPropertyChanged));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? DescriptionTemplate
    {
        get => (DataTemplate?)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    private static void OnSettingsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSettingsExpander expander)
        {
            expander.InvalidateMeasure();
            expander.InvalidateVisual();
        }
    }
}

/// <summary>
/// FluentJalium TransitioningContentControl control.
/// </summary>
public class FWTransitioningContentControl : TransitioningContentControl, IFluentJaliumControl
{
    private bool _applyingTransitionRecipe;

    public static readonly DependencyProperty TransitionProfileProperty =
        DependencyProperty.Register(nameof(TransitionProfile), typeof(FWContentTransitionProfile), typeof(FWTransitioningContentControl),
            new PropertyMetadata(FWContentTransitionProfile.Default, OnTransitionProfileChanged), IsValidTransitionProfile);

    public FWTransitioningContentControl()
    {
        ApplyTransitionRecipeCore(FWContentTransitionRecipe.Create(FWContentTransitionProfile.Default), updateProfile: false, useCurrentValue: true);
    }

    /// <summary>
    /// Gets or sets the FluentJalium transition profile applied to content changes.
    /// </summary>
    public FWContentTransitionProfile TransitionProfile
    {
        get => (FWContentTransitionProfile)GetValue(TransitionProfileProperty)!;
        set => SetValue(TransitionProfileProperty, value);
    }

    /// <summary>
    /// Applies the supplied FluentJalium content transition profile.
    /// </summary>
    public void ApplyTransitionProfile(FWContentTransitionProfile profile)
    {
        ApplyTransitionRecipe(FWContentTransitionRecipe.Create(profile));
    }

    /// <summary>
    /// Applies the supplied FluentJalium content transition profile from a resource scope.
    /// </summary>
    public void ApplyTransitionProfile(FWContentTransitionProfile profile, ResourceDictionary? resources)
    {
        ApplyTransitionRecipe(FWContentTransitionRecipe.Create(profile, resources));
    }

    /// <summary>
    /// Applies an already resolved FluentJalium content transition recipe.
    /// </summary>
    public void ApplyTransitionRecipe(FWContentTransitionRecipe recipe)
    {
        ApplyTransitionRecipeCore(recipe, updateProfile: true, useCurrentValue: false);
    }

    private void ApplyTransitionRecipeCore(FWContentTransitionRecipe recipe, bool updateProfile, bool useCurrentValue)
    {
        _applyingTransitionRecipe = true;
        try
        {
            if (useCurrentValue)
            {
                SetCurrentValue(TransitionModeProperty, recipe.TransitionMode);
                SetCurrentValue(TransitionDurationProperty, recipe.Duration);
                SetCurrentValue(TransitionTimingFunctionProperty, recipe.TimingFunction);
                if (updateProfile)
                {
                    SetCurrentValue(TransitionProfileProperty, recipe.Profile);
                }
            }
            else
            {
                TransitionMode = recipe.TransitionMode;
                TransitionDuration = recipe.Duration;
                TransitionTimingFunction = recipe.TimingFunction;
                if (updateProfile)
                {
                    TransitionProfile = recipe.Profile;
                }
            }
        }
        finally
        {
            _applyingTransitionRecipe = false;
        }
    }

    private static void OnTransitionProfileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTransitioningContentControl control &&
            !control._applyingTransitionRecipe &&
            e.NewValue is FWContentTransitionProfile profile)
        {
            control.ApplyTransitionRecipeCore(FWContentTransitionRecipe.Create(profile), updateProfile: false, useCurrentValue: true);
        }
    }

    private static bool IsValidTransitionProfile(object? value)
    {
        return value is FWContentTransitionProfile profile && Enum.IsDefined(profile);
    }
}

/// <summary>
/// FluentJalium ContentPresenter control.
/// </summary>
public class FWContentPresenter : ContentPresenter, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium StackPanel control.
/// </summary>
public class FWStackPanel : StackPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium WrapPanel control.
/// </summary>
public class FWWrapPanel : WrapPanel, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium Grid control.
/// </summary>
public class FWGrid : Grid, IFluentJaliumControl
{
}
