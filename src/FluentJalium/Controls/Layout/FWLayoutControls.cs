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
