using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media.Animation;
using AnimationDuration = Jalium.UI.Media.Animation.Duration;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentConnectedAnimationTests
{
    [Fact]
    public void Options_ShouldExposeWinUIStyleMotionDefaults()
    {
        var options = new FWConnectedAnimationOptions();

        Assert.Equal(TimeSpan.FromMilliseconds(320), options.Duration);
        Assert.IsType<CubicEase>(options.EasingFunction);
        Assert.Equal(0.72, options.InitialOpacity);
        Assert.True(options.AnimateScale);
        Assert.True(options.AnimateOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, options.Configuration);
    }

    [Fact]
    public void Options_ShouldValidateDurationAndClampOpacity()
    {
        var options = new FWConnectedAnimationOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Duration = TimeSpan.FromMilliseconds(-1));
        Assert.Throws<ArgumentNullException>(() => options.EasingFunction = null!);

        options.InitialOpacity = -2;
        Assert.Equal(0, options.InitialOpacity);

        options.InitialOpacity = 8;
        Assert.Equal(1, options.InitialOpacity);

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Configuration = (FWConnectedAnimationConfiguration)42);
    }

    [Fact]
    public void Options_ShouldCreateDefaultsFromMotionResources()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMotionConnectedAnimationDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(460)),
            ["FluentMotionConnectedAnimationInitialOpacity"] = "0.58"
        };

        var options = FWConnectedAnimationOptions.CreateDefault(resources);

        Assert.Equal(TimeSpan.FromMilliseconds(460), options.Duration);
        Assert.Equal(0.58, options.InitialOpacity);
        Assert.IsType<CubicEase>(options.EasingFunction);
        Assert.True(options.AnimateScale);
        Assert.True(options.AnimateOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, options.Configuration);
    }

    [Fact]
    public void Options_ShouldCreateProfileDefaultsFromMotionResources()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMotionConnectedAnimationDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(320)),
            ["FluentMotionConnectedAnimationInitialOpacity"] = 0.72,
            ["FluentMotionConnectedAnimationNavigationDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(380)),
            ["FluentMotionConnectedAnimationNavigationInitialOpacity"] = "0.68",
            ["FluentMotionConnectedAnimationEntranceDuration"] = 220,
            ["FluentMotionConnectedAnimationEntranceInitialOpacity"] = "0",
            ["FluentMotionConnectedAnimationDirectDuration"] = TimeSpan.FromMilliseconds(300),
            ["FluentMotionConnectedAnimationDirectInitialOpacity"] = "0.74",
            ["FluentMotionConnectedAnimationGravityDuration"] = "0:0:0.41",
            ["FluentMotionConnectedAnimationGravityInitialOpacity"] = 0.61
        };

        var navigation = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Navigation, resources);
        var entrance = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Entrance, resources);
        var direct = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Direct, resources);
        var gravity = FWConnectedAnimationOptions.CreateProfile(FWConnectedAnimationProfile.Gravity, resources);

        Assert.Equal(TimeSpan.FromMilliseconds(380), navigation.Duration);
        Assert.Equal(0.68, navigation.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, navigation.Configuration);
        Assert.Equal(TimeSpan.FromMilliseconds(220), entrance.Duration);
        Assert.Equal(0, entrance.InitialOpacity);
        Assert.Equal(TimeSpan.FromMilliseconds(300), direct.Duration);
        Assert.Equal(0.74, direct.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, direct.Configuration);
        Assert.Equal(TimeSpan.FromMilliseconds(410), gravity.Duration);
        Assert.Equal(0.61, gravity.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, gravity.Configuration);
        Assert.False(gravity.AnimateScale);
    }

    [Fact]
    public void Options_ShouldFallbackWhenMotionResourcesAreInvalid()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMotionConnectedAnimationDuration"] = AnimationDuration.Automatic,
            ["FluentMotionConnectedAnimationInitialOpacity"] = "not-a-number"
        };

        var options = FWConnectedAnimationOptions.CreateDefault(resources);

        Assert.Equal(TimeSpan.FromMilliseconds(320), options.Duration);
        Assert.Equal(0.72, options.InitialOpacity);
    }

    [Fact]
    public void Options_ShouldValidateProfile()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FWConnectedAnimationOptions.CreateProfile((FWConnectedAnimationProfile)42, new ResourceDictionary()));
    }

    [Fact]
    public void TryCreatePlan_ShouldMapSourceBoundsToDestinationStartTransform()
    {
        var options = new FWConnectedAnimationOptions { InitialOpacity = 0.5 };

        var created = FWConnectedAnimationService.TryCreatePlan(
            new Rect(12, 24, 80, 40),
            new Rect(52, 64, 160, 100),
            options,
            out var plan);

        Assert.True(created);
        Assert.Equal(-40, plan.TranslateX);
        Assert.Equal(-40, plan.TranslateY);
        Assert.Equal(0.5, plan.ScaleX);
        Assert.Equal(0.4, plan.ScaleY);
        Assert.Equal(0.5, plan.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, plan.Configuration);
    }

    [Fact]
    public void TryCreatePlan_ShouldRespectDisabledScaleAndOpacity()
    {
        var options = new FWConnectedAnimationOptions
        {
            AnimateScale = false,
            AnimateOpacity = false,
            InitialOpacity = 0.25
        };

        var created = FWConnectedAnimationService.TryCreatePlan(
            new Rect(10, 20, 80, 40),
            new Rect(30, 45, 160, 100),
            options,
            out var plan);

        Assert.True(created);
        Assert.Equal(-20, plan.TranslateX);
        Assert.Equal(-25, plan.TranslateY);
        Assert.Equal(1, plan.ScaleX);
        Assert.Equal(1, plan.ScaleY);
        Assert.Equal(1, plan.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, plan.Configuration);
    }

    [Fact]
    public void TryCreatePlan_ShouldUseGravityConfigurationForCenterAlignedMotion()
    {
        var options = new FWConnectedAnimationOptions
        {
            Configuration = FWConnectedAnimationConfiguration.Gravity,
            InitialOpacity = 0.64
        };

        var created = FWConnectedAnimationService.TryCreatePlan(
            new Rect(10, 20, 80, 40),
            new Rect(100, 140, 160, 120),
            options,
            out var plan);

        Assert.True(created);
        Assert.Equal(-130, plan.TranslateX);
        Assert.Equal(-160, plan.TranslateY);
        Assert.Equal(1, plan.ScaleX);
        Assert.Equal(1, plan.ScaleY);
        Assert.Equal(0.64, plan.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, plan.Configuration);
    }

    [Fact]
    public void TryCreatePlan_ShouldUseDefaultOptionsWhenOptionsAreNull()
    {
        var created = FWConnectedAnimationService.TryCreatePlan(
            new Rect(0, 0, 80, 40),
            new Rect(20, 10, 160, 80),
            null,
            out var plan);

        Assert.True(created);
        Assert.Equal(-20, plan.TranslateX);
        Assert.Equal(-10, plan.TranslateY);
        Assert.Equal(0.5, plan.ScaleX);
        Assert.Equal(0.5, plan.ScaleY);
        Assert.Equal(0.72, plan.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, plan.Configuration);
    }

    [Fact]
    public void TryCreatePlan_ShouldRejectEmptyBounds()
    {
        var created = FWConnectedAnimationService.TryCreatePlan(
            new Rect(0, 0, 0, 40),
            new Rect(10, 10, 100, 60),
            null,
            out var plan);

        Assert.False(created);
        Assert.Equal(default, plan);
    }

    [Fact]
    public void Service_ShouldStoreAndCancelPreparedAnimation()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(12, 24, 80, 40);

        Assert.True(service.PrepareToAnimate("hero", source));
        Assert.True(service.IsPrepared("hero"));
        Assert.True(service.Cancel("hero"));
        Assert.False(service.IsPrepared("hero"));
    }

    [Fact]
    public void Service_ShouldFailGracefullyWhenElementHasNoLayoutBounds()
    {
        var service = new FWConnectedAnimationService();
        var source = new Border();

        Assert.False(service.PrepareToAnimate("hero", source));
        Assert.False(service.IsPrepared("hero"));
        Assert.False(service.TryStart("missing", new Border()));
    }

    [Fact]
    public void TryStart_ShouldKeepPreparedAnimationWhenDestinationHasNoLayoutBounds()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(12, 24, 80, 40);

        Assert.True(service.PrepareToAnimate("hero", source));
        Assert.False(service.TryStart("hero", new Border()));
        Assert.True(service.IsPrepared("hero"));
    }

    [Fact]
    public void TryCreatePreparedPlan_ShouldPreviewPreparedAnimationWithoutConsumingIt()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(12, 24, 80, 40);
        var destination = CreateArrangedElement(52, 64, 160, 100);

        Assert.True(service.PrepareToAnimate("hero", source));
        Assert.True(service.TryCreatePreparedPlan("hero", destination, out var plan));

        Assert.Equal(FWConnectedAnimationConfiguration.Direct, plan.Configuration);
        Assert.True(service.IsPrepared("hero"));
    }

    [Fact]
    public void Service_ShouldSnapshotOptionsWhenPreparingAnimation()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(12, 24, 80, 40);
        var destination = CreateArrangedElement(52, 64, 160, 100);
        var options = new FWConnectedAnimationOptions
        {
            Configuration = FWConnectedAnimationConfiguration.Gravity
        };

        Assert.True(service.PrepareToAnimate("hero", source, options));

        options.Configuration = FWConnectedAnimationConfiguration.Direct;

        Assert.True(service.TryCreatePreparedPlan("hero", destination, out var plan));
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, plan.Configuration);
        Assert.True(service.IsPrepared("hero"));
        Assert.True(service.Cancel("hero"));
    }

    [Fact]
    public void AttachedProperties_ShouldExposeDeclarativeConnectedAnimationSettings()
    {
        var element = new Border();

        FWConnectedAnimation.SetKey(element, "hero");
        FWConnectedAnimation.SetProfile(element, FWConnectedAnimationProfile.Gravity);
        FWConnectedAnimation.SetConfiguration(element, FWConnectedAnimationConfiguration.Gravity);
        FWConnectedAnimation.SetIsEnabled(element, false);

        Assert.Equal("hero", FWConnectedAnimation.GetKey(element));
        Assert.Equal(FWConnectedAnimationProfile.Gravity, FWConnectedAnimation.GetProfile(element));
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, FWConnectedAnimation.GetConfiguration(element));
        Assert.False(FWConnectedAnimation.GetIsEnabled(element));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FWConnectedAnimation.SetProfile(element, (FWConnectedAnimationProfile)42));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FWConnectedAnimation.SetConfiguration(element, (FWConnectedAnimationConfiguration)42));
    }

    [Fact]
    public void AttachedCreateOptions_ShouldRespectProfileUnlessConfigurationIsExplicit()
    {
        var resources = new ResourceDictionary
        {
            ["FluentMotionConnectedAnimationDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(320)),
            ["FluentMotionConnectedAnimationInitialOpacity"] = 0.72,
            ["FluentMotionConnectedAnimationGravityDuration"] = new AnimationDuration(TimeSpan.FromMilliseconds(360)),
            ["FluentMotionConnectedAnimationGravityInitialOpacity"] = 0.64
        };
        var gravityElement = new Border();
        var directOverrideElement = new Border();

        FWConnectedAnimation.SetProfile(gravityElement, FWConnectedAnimationProfile.Gravity);
        FWConnectedAnimation.SetProfile(directOverrideElement, FWConnectedAnimationProfile.Gravity);
        FWConnectedAnimation.SetConfiguration(directOverrideElement, FWConnectedAnimationConfiguration.Direct);

        var gravity = FWConnectedAnimation.CreateOptions(gravityElement, resources);
        var directOverride = FWConnectedAnimation.CreateOptions(directOverrideElement, resources);

        Assert.Equal(TimeSpan.FromMilliseconds(360), gravity.Duration);
        Assert.Equal(0.64, gravity.InitialOpacity);
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, gravity.Configuration);
        Assert.False(gravity.AnimateScale);
        Assert.Equal(FWConnectedAnimationConfiguration.Direct, directOverride.Configuration);
        Assert.True(directOverride.AnimateScale);
    }

    [Fact]
    public void AttachedPrepareAndPreview_ShouldUseElementKeyAndProfile()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(10, 20, 80, 40);
        var destination = CreateArrangedElement(100, 140, 160, 120);

        FWConnectedAnimation.SetKey(source, "hero");
        FWConnectedAnimation.SetProfile(source, FWConnectedAnimationProfile.Gravity);
        FWConnectedAnimation.SetKey(destination, "hero");

        Assert.True(FWConnectedAnimation.PrepareToAnimate(source, service));
        Assert.True(FWConnectedAnimation.TryCreatePreparedPlan(destination, service, out var plan));
        Assert.Equal(FWConnectedAnimationConfiguration.Gravity, plan.Configuration);
        Assert.Equal(-130, plan.TranslateX);
        Assert.Equal(-160, plan.TranslateY);
        Assert.True(service.IsPrepared("hero"));
    }

    [Fact]
    public void AttachedHelpers_ShouldNoOpWhenDisabledOrMissingKey()
    {
        var service = new FWConnectedAnimationService();
        var source = CreateArrangedElement(12, 24, 80, 40);
        var destination = CreateArrangedElement(52, 64, 160, 100);

        Assert.False(FWConnectedAnimation.PrepareToAnimate(source, service));

        FWConnectedAnimation.SetKey(source, "hero");
        FWConnectedAnimation.SetIsEnabled(source, false);
        Assert.False(FWConnectedAnimation.PrepareToAnimate(source, service));

        FWConnectedAnimation.SetIsEnabled(source, true);
        Assert.True(FWConnectedAnimation.PrepareToAnimate(source, service));

        FWConnectedAnimation.SetKey(destination, "hero");
        FWConnectedAnimation.SetIsEnabled(destination, false);
        Assert.False(FWConnectedAnimation.TryCreatePreparedPlan(destination, service, out var plan));
        Assert.Equal(default, plan);
        Assert.True(service.IsPrepared("hero"));
    }

    private static Border CreateArrangedElement(double x, double y, double width, double height)
    {
        var element = new Border
        {
            Width = width,
            Height = height
        };

        element.Measure(new Size(width, height));
        element.Arrange(new Rect(x, y, width, height));
        return element;
    }
}
