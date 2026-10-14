using System.Diagnostics;
using FluentJalium.Controls;
using FluentJalium.Gallery;
using FluentJalium.Localization;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Hosting;
using Jalium.UI.Interop;
using Jalium.UI.Threading;

namespace FluentJalium.Launcher;

/// <summary>
/// The launcher window: pick a renderer, a path engine, a quality mode, a theme and a language, then hand them
/// to the Gallery. The choices are dropdowns rather than radio rows because an open list is the shape that can
/// carry a name and what it means; the row of three sits immediately left of the launch button.
/// </summary>
/// <remarks>
/// <para>
/// The window is near-borderless by construction, not by imitation. <c>TitleBarStyle=Custom</c> keeps
/// <c>WS_CAPTION</c> and removes the visual caption in <c>WM_NCCALCSIZE</c> (<c>Window.cs:5426</c>), so the top band
/// is a real drag region with snap layouts and touch still working, and this window's own header content lives in
/// <c>LeftWindowCommands</c> / <c>RightWindowCommands</c> inside it. The band is painted the same solid fill as the
/// client area, so there is no seam to read as a separate bar.
/// </para>
/// <para>
/// The five settings divide by when they can take effect, and that split is the runtime's:
/// <see cref="Window.SetRenderingEngineOverride"/>, the theme manager and the text catalog all act on a live
/// window, while <see cref="RenderBackend"/> and <see cref="PathAntiAliasing"/> are read once when a render target
/// is created. The status line therefore shows the running configuration and the pending one side by side.
/// </para>
/// </remarks>
public partial class LauncherWindow : Window
{
    private static readonly TimeSpan StatusInterval = TimeSpan.FromMilliseconds(400);

    // One order for every list, so an index means the same thing in the UI and in the mapping below.
    private static readonly string[] BackendLabels = ["Auto", "Vulkan", "DirectX (D3D12)"];
    private static readonly string[] EngineLabels = ["Auto", "Vello", "Impeller"];
    private static readonly string[] LanguageLabels = ["Simplified Chinese", "English"];
    private static readonly string[] QualityLabels = ["Analytic coverage", "MSAA 4x", "MSAA 8x", "MSAA 16x"];
    private static readonly string[] ThemeLabels = ["Follow system", "Light", "Dark"];

    private readonly HostSettings _settings;
    private readonly TextSweeper _sweeper = new();
    private readonly DispatcherTimer _statusTimer;
    private readonly string? _startPageId;

    private bool _syncing;
    private bool _handoffInProgress;

    private ComboBox BackendBox => (ComboBox)BackendChoice!;
    private ComboBox EngineBox => (ComboBox)EngineChoice!;
    private ComboBox LanguageBox => (ComboBox)LanguageChoice!;
    private ComboBox QualityBox => (ComboBox)QualityChoice!;
    private ComboBox ThemeBox => (ComboBox)ThemeChoice!;
    private FluentToggleSwitch MotionToggle => (FluentToggleSwitch)MotionToggleElement!;
    private TextBlock RuntimeStatus => (TextBlock)RuntimeStatusText!;
    private TextBlock PendingStatus => (TextBlock)PendingStatusText!;
    private Button Start => (Button)StartButton!;

    /// <summary>Builds the window against a settings snapshot and an optional Gallery page to start on.</summary>
    public LauncherWindow(HostSettings settings, string? startPageId)
    {
        _settings = settings;
        _startPageId = startPageId;

        InitializeComponent();

        _statusTimer = new DispatcherTimer(
            StatusInterval, DispatcherPriority.Background, (_, _) => RefreshStatus(), Dispatcher);

        BackendBox.SelectionChanged += OnBackendSelected;
        EngineBox.SelectionChanged += OnEngineSelected;
        LanguageBox.SelectionChanged += OnLanguageSelected;
        QualityBox.SelectionChanged += OnQualitySelected;
        ThemeBox.SelectionChanged += OnThemeSelected;
        MotionToggle.Checked += OnMotionToggled;
        MotionToggle.Unchecked += OnMotionToggled;
        Start.Click += OnStartClicked;

        ((Button)MinimizeButton!).Click += (_, _) => WindowState = WindowState.Minimized;
        ((Button)MaximizeButton!).Click += (_, _) => ToggleMaximized();
        ((Button)CloseButton!).Click += (_, _) => Close();

        UiText.LanguageChanged += OnLanguageChanged;

        // Both of these are read while this window's render target is created, so they have to be in place before
        // the first frame rather than when a dropdown is touched.
        RenderQualityOptions.Current.PathAntiAliasing = _settings.PathAntiAliasing;
        FluentThemeManager.ReduceMotion = _settings.ReduceMotion;
        ApplyTheme(_settings.Theme);

        RebuildChoices();
        ApplyLanguage(_settings.Language);

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object? sender, RoutedEventArgs args)
    {
        // The render target exists by Loaded, so this is the first point at which the window can apply an engine
        // override and report what it is actually running.
        SetRenderingEngineOverride(_settings.Engine);
        ApplyAppearance();
        _statusTimer.Start();
        RefreshStatus();
    }

    private void OnClosed(object? sender, EventArgs args)
    {
        _statusTimer.Stop();
        UiText.LanguageChanged -= OnLanguageChanged;
    }

    /// <summary>
    /// Paints the header band the same solid fill as the client area. The band is the framework's title bar, and
    /// its own theme row is a separate colour, so without this the "no border" window still reads as two surfaces
    /// with a line between them.
    /// </summary>
    private void ApplyAppearance()
    {
        Background = FluentThemeManager.GetBrush("SolidBackgroundFillColorBaseBrush");
        Foreground = FluentThemeManager.GetBrush("TextFillColorPrimaryBrush");

        if (TitleBar is { } titleBar)
        {
            titleBar.Background = Background;
            titleBar.Foreground = Foreground;
        }
    }

    private void ToggleMaximized()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        ((Button)MaximizeButton!).Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    /// <summary>
    /// Rebuilds every list from the catalog and restores the selected indexes. Called on construction and on every
    /// language change: a dropdown's rows are generated items, so a language switch is a new list rather than a walk
    /// over the realized rows.
    /// </summary>
    private void RebuildChoices()
    {
        _syncing = true;
        try
        {
            Fill(BackendBox, BackendIndex(_settings.Backend), BackendLabels);
            Fill(EngineBox, EngineIndex(_settings.Engine), EngineLabels);
            Fill(LanguageBox, _settings.Language == UiLanguage.ZhHans ? 0 : 1, LanguageLabels);
            Fill(QualityBox, QualityIndex(_settings.PathAntiAliasing), QualityLabels);
            Fill(ThemeBox, ThemeIndex(_settings.Theme), ThemeLabels);
            MotionToggle.IsChecked = _settings.ReduceMotion;
        }
        finally
        {
            _syncing = false;
        }
    }

    private static void Fill(ComboBox host, int selectedIndex, string[] englishLabels)
    {
        // -1 first, deliberately. A ComboBox resolves its closed-state text when the selection changes, so
        // repopulating the rows and re-selecting the same index leaves "Auto" on screen while the list below it
        // says 自动. Dropping the selection makes the change real and the displayed text is resolved again.
        host.SelectedIndex = -1;
        host.Items.Clear();
        foreach (var label in englishLabels)
        {
            host.Items.Add(new ComboBoxItem { Content = UiText.T(label) });
        }

        host.SelectedIndex = selectedIndex >= 0 && selectedIndex < host.Items.Count ? selectedIndex : 0;
    }

    private static int BackendIndex(RenderBackend backend) => backend switch
    {
        RenderBackend.Vulkan => 1,
        RenderBackend.D3D12 => 2,
        _ => 0,
    };

    private static RenderBackend BackendFromIndex(int index) => index switch
    {
        1 => RenderBackend.Vulkan,
        2 => RenderBackend.D3D12,
        _ => RenderBackend.Auto,
    };

    private static int EngineIndex(RenderingEngine engine) => engine switch
    {
        RenderingEngine.Vello => 1,
        RenderingEngine.Impeller => 2,
        _ => 0,
    };

    private static RenderingEngine EngineFromIndex(int index) => index switch
    {
        1 => RenderingEngine.Vello,
        2 => RenderingEngine.Impeller,
        _ => RenderingEngine.Auto,
    };

    private static int QualityIndex(PathAntiAliasing mode) => mode switch
    {
        PathAntiAliasing.Analytic => 0,
        PathAntiAliasing.Msaa4x => 1,
        PathAntiAliasing.Msaa16x => 3,
        _ => 2,
    };

    private static PathAntiAliasing QualityFromIndex(int index) => index switch
    {
        0 => PathAntiAliasing.Analytic,
        1 => PathAntiAliasing.Msaa4x,
        3 => PathAntiAliasing.Msaa16x,
        _ => PathAntiAliasing.Msaa8x,
    };

    private static int ThemeIndex(AppThemeMode mode) => mode switch
    {
        AppThemeMode.Light => 1,
        AppThemeMode.Dark => 2,
        _ => 0,
    };

    private static AppThemeMode ThemeFromIndex(int index) => index switch
    {
        1 => AppThemeMode.Light,
        2 => AppThemeMode.Dark,
        _ => AppThemeMode.System,
    };

    private void OnBackendSelected(object? sender, SelectionChangedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        // Stored, not applied: the backend is process-wide and this window's render target pins the current one.
        _settings.Backend = BackendFromIndex(SelectedIndex(BackendBox));
        RefreshStatus();
    }

    private void OnEngineSelected(object? sender, SelectionChangedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        _settings.Engine = EngineFromIndex(SelectedIndex(EngineBox));
        SetRenderingEngineOverride(_settings.Engine);
        RefreshStatus();
    }

    private void OnLanguageSelected(object? sender, SelectionChangedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        _settings.Language = SelectedIndex(LanguageBox) == 0 ? UiLanguage.ZhHans : UiLanguage.English;
        ApplyLanguage(_settings.Language);
        Persist();
    }

    private void OnQualitySelected(object? sender, SelectionChangedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        _settings.PathAntiAliasing = QualityFromIndex(SelectedIndex(QualityBox));
        RefreshStatus();
    }

    private void OnThemeSelected(object? sender, SelectionChangedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        _settings.Theme = ThemeFromIndex(SelectedIndex(ThemeBox));
        ApplyTheme(_settings.Theme);
        RefreshStatus();
    }

    private void OnMotionToggled(object? sender, RoutedEventArgs args)
    {
        if (_syncing)
        {
            return;
        }

        _settings.ReduceMotion = MotionToggle.IsChecked == true;
        FluentThemeManager.ReduceMotion = _settings.ReduceMotion;
        RefreshStatus();
    }

    private void OnLanguageChanged() => ApplyLanguage(UiText.Language);

    private void ApplyLanguage(UiLanguage language)
    {
        UiText.SetLanguage(language);

        // Markup text first. The dropdown rows read the catalog, so they are rebuilt after the language has
        // actually changed rather than alongside it - rebuilding before the switch would re-emit the old language.
        _sweeper.Apply(this);
        NameMotionToggle();
        RebuildChoices();
        RefreshStatus();
    }

    /// <summary>
    /// A switch carries no text of its own, so its element name is what a screen reader would otherwise announce.
    /// It is written from the same catalog as the label above it, and on every language change, so the two never
    /// disagree.
    /// </summary>
    private void NameMotionToggle() =>
        AutomationProperties.SetName(MotionToggle, UiText.T("Reduce motion"));

    private static void ApplyTheme(AppThemeMode mode) => FluentThemeManager.ApplyTheme(mode switch
    {
        AppThemeMode.Light => FluentThemeVariant.Light,
        AppThemeMode.Dark => FluentThemeVariant.Dark,
        _ => FluentThemeVariant.System,
    });

    private void RefreshStatus()
    {
        if (_handoffInProgress)
        {
            return;
        }

        ApplyAppearance();
        RuntimeStatus.Text = UiText.T("Running {0} with {1}.", CurrentRenderBackend, CurrentRenderingEngine);

        // "What will be asked for" and "what will run" are different questions, so Auto is spelled out rather than
        // printed as an enum member, and an unchanged renderer says so instead of restating the resolved backend.
        var requested = _settings.Backend == RenderBackend.Auto ? UiText.T("Auto") : _settings.Backend.ToString();
        var requestedEngine = _settings.Engine == RenderingEngine.Auto ? UiText.T("Auto") : _settings.Engine.ToString();
        PendingStatus.Text = _settings.Backend == RenderBackend.Auto && _settings.Engine == RenderingEngine.Auto
            ? UiText.T("The main window opens with the same settings.")
            : UiText.T("The main window opens on {0} with {1}.", requested, requestedEngine);
    }

    /// <summary>The selected row's index, or -1 while a list is empty.</summary>
    private static int SelectedIndex(ComboBox host) => host.SelectedIndex;

    private void Persist()
    {
        if (HostSettingsStore.TrySave(_settings))
        {
            return;
        }

        // A read-only install directory is a normal deployment, not a failure worth blocking on: the preference
        // still holds for this session, and the status line says once where it tried to write.
        PendingStatus.Text = UiText.T(
            "Could not save preferences to {0}; they will not survive a restart.",
            HostSettingsStore.DefaultPath);
    }

    private void OnStartClicked(object? sender, RoutedEventArgs args) => Handoff();

    /// <summary>
    /// Closes the launcher and brings up the Gallery on the chosen renderer and engine.
    /// </summary>
    /// <remarks>
    /// The order is load-bearing. The Gallery window is constructed and made the application's main window
    /// <em>before</em> this one closes, because the application runs with
    /// <see cref="ShutdownMode.OnMainWindowClose"/> and a window count of zero would end the process. This
    /// window's <c>Close</c> is what stops its render thread and disposes its render target, and that render target
    /// is the pin holding the current render context alive - so the backend swap has to follow it and cannot be
    /// moved earlier to make the switch look instant.
    /// </remarks>
    private void Handoff()
    {
        if (_handoffInProgress)
        {
            return;
        }

        _handoffInProgress = true;
        Start.IsEnabled = false;

        Persist();
        _statusTimer.Stop();

        var gallery = new MainWindow();
        gallery.SetTheme(_settings.Theme switch
        {
            AppThemeMode.Light => FluentThemeVariant.Light,
            AppThemeMode.Dark => FluentThemeVariant.Dark,
            _ => FluentThemeVariant.System,
        });
        if (_startPageId is { Length: > 0 } pageId)
        {
            gallery.SetStartPage(pageId);
        }

        // Before Close: this is what keeps the process alive across the gap where no window exists.
        if (Application.Current is { } application)
        {
            application.MainWindow = gallery;
        }

        PendingStatus.Text = UiText.T("Switching renderer...");
        Close();

        ApplyPendingRenderer();
        RenderQualityOptions.Current.PathAntiAliasing = _settings.PathAntiAliasing;
        if (RenderContext.Current is { } context)
        {
            context.DefaultRenderingEngine = _settings.Engine;
        }

        gallery.Show();
        gallery.Activate();
    }

    private void ApplyPendingRenderer()
    {
        // Auto is deliberately not forced: forcing a replace would retire a working context and rebuild the device
        // for nothing. An explicit request replaces a context running a different backend, and leaves the current
        // one untouched when the requested backend turns out to be unavailable rather than downgrading to software.
        if (_settings.Backend == RenderBackend.Auto)
        {
            return;
        }

        var context = RenderContext.GetOrCreateCurrent(
            _settings.Backend, GpuPreference.Auto, forceReplace: true);

        if (context is not null && context.Backend != _settings.Backend)
        {
            // GetOrCreateCurrent only enforces a backend it found available, so a mismatch here is the honest
            // report that the machine could not provide what was asked for.
            Debug.WriteLine(
                $"[Launcher] requested {_settings.Backend}, running {context.Backend}; " +
                "the runtime kept the context it already had.");
        }
    }
}
