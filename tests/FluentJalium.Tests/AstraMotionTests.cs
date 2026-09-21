using System.Xml.Linq;
using FluentJalium.Controls;
using FluentJalium.Themes;
using FluentJalium.Tests.Pixel;
using Jalium.UI;

namespace FluentJalium.Tests;

/// <summary>
/// The motion slice of the parallel work: whether a template transition's duration is a resource the layer publishes
/// and can rewrite, and whether <see cref="FluentThemeManager.ReduceMotion"/> reaches it.
/// </summary>
/// <remarks>
/// <para>
/// Upstream keeps its durations in <c>&lt;x:String&gt;</c> rows
/// (<c>Common_themeresources_any.xaml:603-606</c>, <c>SplitView_themeresources.xaml:10</c> at commit
/// <c>19e3bdc3ccf3361393d623d3a5d2667cb8f33229</c>). This runtime stores such a row without complaint and then hands
/// the consumer the framework default instead of the value: <c>spike/MotionProbe</c> [2] read 180 ms -
/// <c>UIElement.TransitionDuration</c>'s own default - off all four consumer shapes for both of upstream's time
/// forms. The names and numbers are therefore kept and only the row type moves, to the framework's <c>Duration</c>,
/// which the probe measured landing at 83 ms through the same four shapes, including inside a control template.
/// </para>
/// <para>
/// A zero row is the lever, not a decoration: the framework reads <c>TransitionDuration</c> at the moment a
/// transition would start and starts nothing when the value has no <c>TimeSpan</c> or is at or below zero. That is
/// why the assertions below read a realised template part rather than a rendered frame - whether a transition runs
/// also depends on the machine's accessibility settings, which the probe recorded as on
/// (<c>ClientAreaAnimation=True, UIEffects=True</c>) and which a CI box is free to disagree with.
/// </para>
/// <para>
/// <c>default(Duration)</c> reports <c>Automatic</c> while an intentional zero reports <c>00:00:00</c>, so a row that
/// was dropped and a row that was zeroed on purpose are distinguishable from the property alone. Every check below
/// names which of the two it expects.
/// </para>
/// </remarks>
[Collection(AstraThemeRuntimeCollection.Name)]
public sealed class AstraMotionTests
{
    private const string MotionFile = "src/FluentJalium/ThemeResources/Motion.jalxaml";

    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>Key, upstream's literal text, and the milliseconds that text names.</summary>
    private static readonly (string Key, string Text, int Milliseconds)[] Published =
    [
        ("ControlFasterAnimationDuration", "00:00:00.083", 83),
        ("ControlFastAnimationDuration", "00:00:00.167", 167),
        ("SplitViewPaneAnimationOpenDuration", "00:00:00.2", 200),
    ];

    private readonly AstraThemeRuntimeFixture _fixture;

    public AstraMotionTests(AstraThemeRuntimeFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(PublishedRows))]
    public void A_published_row_carries_the_duration_upstream_publishes(string key, int milliseconds)
    {
        _fixture.Run(() =>
        {
            // A row that stored a string instead of a duration would still resolve, to the property's own default, so
            // the assertion names the type the lookup returned rather than only the number.
            var duration = Assert.IsType<Duration>(Resource(key));
            Assert.True(duration.HasTimeSpan, $"{key} resolved to {duration}, which is a dropped row rather than a duration.");
            Assert.Equal(milliseconds, duration.TimeSpan.TotalMilliseconds, 1);
        });
    }

    public static TheoryData<string, int> PublishedRows()
    {
        var data = new TheoryData<string, int>();
        foreach (var (key, _, milliseconds) in Published) data.Add(key, milliseconds);
        return data;
    }

    /// <summary>
    /// The transcription gate on the file itself: upstream's names, upstream's literal text, and a row type this
    /// reader can carry. Reverting a row to <c>&lt;x:String&gt;</c> would keep the first two and lose the value, which
    /// is the failure this test exists to make loud.
    /// </summary>
    [Fact]
    public void The_rows_keep_upstream_names_and_move_only_the_row_type()
    {
        var root = XDocument.Load(Locate(RepositoryRoot(), MotionFile)).Root!;
        var rows = root.Descendants().Where(static element => element.Attribute(Xaml + "Key") is not null).ToList();

        var expected = Published.Select(static row => row.Key).ToHashSet(StringComparer.Ordinal);
        var found = rows.Select(element => element.Attribute(Xaml + "Key")!.Value).ToHashSet(StringComparer.Ordinal);
        Assert.True(found.SetEquals(expected),
            $"the published rows are {string.Join(", ", found.OrderBy(static name => name, StringComparer.Ordinal))}.");

        foreach (var element in rows)
        {
            var key = element.Attribute(Xaml + "Key")!.Value;
            var (_, text, _) = Published.First(row => row.Key == key);
            Assert.Equal(nameof(Duration), element.Name.LocalName);
            Assert.Equal(text, element.Value.Trim());
        }
    }

    /// <summary>
    /// The reading that matters is the one off a realised template: a <c>{ThemeResource}</c> inside a control template
    /// resolves at instantiation, so a row that reached a bare element could still miss this. Each part is asked for
    /// its own property, and the two rows the switch uses are read separately.
    /// </summary>
    [Fact]
    public void A_realised_template_reads_its_duration_from_the_row()
    {
        _fixture.Run(() =>
        {
            var toggle = ToggleSwitch();

            Assert.Equal(83d, Milliseconds(Part(toggle, "SwitchTrack")));
            Assert.Equal(167d, Milliseconds(Part(toggle, "ThumbHost")));
            Assert.Equal(83d, Milliseconds(Part(toggle, "Knob")));
        });
    }

    /// <summary>
    /// ReduceMotion rewrites the live row and a template realised before the flip follows it, without a visual-tree
    /// walk. This is the point of keying the durations, and the reading that goes red if the flip writes into a copy
    /// of the dictionary instead of the one the application merged.
    /// </summary>
    [Fact]
    public void ReduceMotion_zeroes_the_row_and_a_realised_template_follows()
    {
        _fixture.Run(() =>
        {
            var toggle = ToggleSwitch();
            try
            {
                Assert.Equal(83d, Milliseconds(Part(toggle, "SwitchTrack")));

                FluentThemeManager.ReduceMotion = true;
                PixelHarness.Settle(4);

                var zeroed = Part(toggle, "SwitchTrack").TransitionDuration;
                Assert.True(zeroed.HasTimeSpan, "a reduced duration must read as zero, not as the dropped row's Automatic state");
                Assert.Equal(0d, zeroed.TimeSpan.TotalMilliseconds, 1);
                Assert.Equal(0d, Milliseconds(Part(toggle, "ThumbHost")));
                Assert.Equal(0d, Milliseconds(Part(toggle, "Knob")));
            }
            finally
            {
                FluentThemeManager.ReduceMotion = false;
                PixelHarness.Settle(4);
            }
        });
    }

    /// <summary>
    /// A control built while reduced is born still: the row is read at instantiation, so the state does not depend on
    /// when the theme was applied. Built in its own test because <see cref="PixelHarness.Build"/> puts one subject in
    /// the host window, and a second subject would detach the first - and a detached element is outside the reach of
    /// the resource broadcast, which is a different claim from this one.
    /// </summary>
    [Fact]
    public void A_template_realised_while_reduced_is_born_still()
    {
        _fixture.Run(() =>
        {
            FluentThemeManager.ReduceMotion = true;
            try
            {
                Assert.Equal(0d, Milliseconds(Part(ToggleSwitch(), "SwitchTrack")));
            }
            finally
            {
                FluentThemeManager.ReduceMotion = false;
                PixelHarness.Settle(4);
            }
        });
    }

    /// <summary>
    /// A theme switch must not hand the animations back to someone who turned them off. The palette is rebuilt from
    /// the Light and Dark dictionaries on every switch, so a duration that lived in the palette would come back here
    /// and the user's setting would silently stop working mid-session.
    /// </summary>
    [Fact]
    public void A_theme_switch_does_not_restore_a_zeroed_duration()
    {
        _fixture.Run(() =>
        {
            var toggle = ToggleSwitch();
            try
            {
                FluentThemeManager.ReduceMotion = true;
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
                PixelHarness.Settle(6);

                var row = Assert.IsType<Duration>(Resource("ControlFasterAnimationDuration"));
                Assert.Equal(0d, row.TimeSpan.TotalMilliseconds, 1);
                Assert.Equal(0d, Milliseconds(Part(toggle, "SwitchTrack")));
            }
            finally
            {
                FluentThemeManager.ReduceMotion = false;
                FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
                PixelHarness.Settle(6);
            }
        });
    }

    /// <summary>
    /// The drift gate: no transition may carry a literal duration again, because a literal is invisible to
    /// ReduceMotion. Every element that declares properties to transition is checked for a duration naming a published
    /// row, and a duration without a TransitionProperty is an offender too - it reads as motion while nothing moves.
    /// </summary>
    [Fact]
    public void Every_transition_declaration_reads_a_published_duration()
    {
        var keys = Published.Select(static row => row.Key).ToHashSet(StringComparer.Ordinal);
        var offenders = new List<string>();
        var declarations = 0;
        var root = RepositoryRoot();

        foreach (var file in Directory.EnumerateFiles(Locate(root, "src/FluentJalium"), "*.jalxaml", SearchOption.AllDirectories))
        {
            var relative = System.IO.Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (var element in XDocument.Load(file).Descendants())
            {
                var property = Attribute(element, "TransitionProperty");
                var duration = Attribute(element, "TransitionDuration");
                if (property is null && duration is null) continue;
                declarations++;

                if (property is null)
                {
                    offenders.Add($"{relative}: a duration with no TransitionProperty, so nothing transitions");
                    continue;
                }

                if (duration is null)
                {
                    offenders.Add($"{relative}: transitions {property} at the framework's own default duration");
                    continue;
                }

                var reference = duration.Trim();
                if (!reference.StartsWith("{ThemeResource ", StringComparison.Ordinal) ||
                    !keys.Contains(reference["{ThemeResource ".Length..^1]))
                {
                    offenders.Add($"{relative}: duration {duration} is a literal or an unpublished key");
                }
            }
        }

        Assert.True(declarations >= 20, $"only {declarations} transition declarations were read; the gate has gone vacuous.");
        offenders.Sort(StringComparer.Ordinal);
        Assert.False(offenders.Count > 0, $"Durations ReduceMotion cannot reach ({offenders.Count}):" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static FluentToggleSwitch ToggleSwitch()
    {
        var toggle = new FluentToggleSwitch { Style = FluentThemeManager.GetStyle("FluentToggleSwitchStyle") };
        PixelHarness.Build(toggle, 160, 32);
        return toggle;
    }

    private static double Milliseconds(UIElement element)
    {
        var duration = element.TransitionDuration;
        Assert.True(duration.HasTimeSpan, "the property resolved to Automatic, which is a dropped row rather than a duration");
        return duration.TimeSpan.TotalMilliseconds;
    }

    private static UIElement Part(DependencyObject root, string name) =>
        PixelHarness.Named(root, name) as UIElement ?? throw new InvalidOperationException($"No part named {name}.");

    private static string? Attribute(XElement element, string name) =>
        element.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == name)?.Value;

    private static object? Resource(string key) => Application.Current?.TryFindResource(key);

    private static string Locate(string root, string relative) =>
        System.IO.Path.Combine(root, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "FluentJalium.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Cannot locate the repository root.");
    }
}
