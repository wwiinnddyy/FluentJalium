using FluentJalium.Localization;
using Jalium.UI.Controls;

namespace FluentJalium.Tests.Localization;

/// <summary>
/// The catalog's own contract: English is the key, an untranslated string degrades to English rather than to
/// nothing, and a second translation for a key is a mistake rather than an override.
/// </summary>
/// <remarks>
/// <see cref="UiText"/> is process-wide static state, so this class shares the localization collection with
/// <see cref="LocalizationCoverageTests"/>: the coverage gate registers both hosts' catalogs, and a parallel
/// <see cref="UiText.ResetForTests"/> from here would wipe them mid-assertion.
/// </remarks>
[Collection(nameof(LocalizationCoverageTests))]
public class UiTextTests : IDisposable
{
    public UiTextTests() => UiText.ResetForTests();

    public void Dispose() => UiText.ResetForTests();

    [Fact]
    public void English_is_the_key_so_an_untranslated_string_still_reads()
    {
        Assert.Equal("Nothing here yet", UiText.T("Nothing here yet"));
        Assert.False(UiText.HasChinese("Nothing here yet"));
    }

    [Fact]
    public void A_registered_key_switches_with_the_language_and_back()
    {
        UiText.Add("Renderer", "渲染器");

        UiText.SetLanguage(UiLanguage.ZhHans);
        Assert.Equal("渲染器", UiText.T("Renderer"));
        Assert.True(UiText.HasChinese("Renderer"));

        UiText.SetLanguage(UiLanguage.English);
        Assert.Equal("Renderer", UiText.T("Renderer"));
    }

    [Fact]
    public void An_empty_key_is_returned_untouched_rather_than_throwing()
    {
        Assert.Equal(string.Empty, UiText.T(string.Empty));
        Assert.Null(UiText.T(null!));
    }

    [Fact]
    public void Registering_the_same_translation_twice_is_accepted()
    {
        UiText.Add("Start", "开始");
        UiText.Add("Start", "开始");

        Assert.Single(UiText.RegisteredKeys, key => key == "Start");
    }

    [Fact]
    public void Registering_a_second_translation_for_one_key_is_rejected()
    {
        UiText.Add("Start", "开始");

        var error = Assert.Throws<InvalidOperationException>(() => UiText.Add("Start", "启动"));
        Assert.Contains("Start", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LanguageChanged_fires_on_a_real_change_only()
    {
        var raised = 0;
        UiText.LanguageChanged += () => raised++;

        UiText.SetLanguage(UiLanguage.English);
        Assert.Equal(0, raised);

        UiText.SetLanguage(UiLanguage.ZhHans);
        UiText.SetLanguage(UiLanguage.ZhHans);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void The_format_overload_translates_the_template_and_fills_the_hole()
    {
        UiText.Add("Renderer {0}, engine {1}", "渲染器 {0}，实现器 {1}");
        UiText.SetLanguage(UiLanguage.ZhHans);

        Assert.Equal("渲染器 D3D12，实现器 Vello", UiText.T("Renderer {0}, engine {1}", "D3D12", "Vello"));
    }

    [Fact]
    public void A_translated_template_may_reorder_its_arguments_so_Format_is_the_only_safe_next_step()
    {
        UiText.Add("{0} of {1}.", "{1} 中的 {0}。");
        UiText.SetLanguage(UiLanguage.ZhHans);

        // The two orders differ, which is why the translated template is handed to Format rather than to a
        // formatter that still assumes the arguments are in source order.
        Assert.Equal("100 中的 40。", UiText.Format(UiText.T("{0} of {1}.", 40, 100)));
    }

    [Fact]
    public void The_sweeper_writes_the_translation_and_restores_the_authored_text()
    {
        UiText.Add("Start", "开始启动");
        var label = new TextBlock { Text = "Start" };
        var host = new StackPanel();
        host.Children.Add(label);
        host.Children.Add(new Button { Content = "Start" });

        var sweeper = new TextSweeper();
        UiText.SetLanguage(UiLanguage.ZhHans);
        sweeper.Apply(host);
        Assert.Equal("开始启动", label.Text);
        Assert.Equal("开始启动", ((Button)host.Children[1]).Content);

        // Switching back has to come from the text the markup authored rather than from the translation on
        // screen, or the second sweep would translate the Chinese and English would be unreachable.
        UiText.SetLanguage(UiLanguage.English);
        sweeper.Apply(host);
        Assert.Equal("Start", label.Text);
        Assert.Equal("Start", ((Button)host.Children[1]).Content);
    }

    [Fact]
    public void The_sweeper_names_every_string_it_has_seen_and_the_ones_it_cannot_translate()
    {
        UiText.Add("Live rendering", "实时渲染");
        var host = new StackPanel();
        host.Children.Add(new TextBlock { Text = "Live rendering" });
        host.Children.Add(new TextBlock { Text = "Page 1." });

        var sweeper = new TextSweeper();
        sweeper.Apply(host);

        Assert.Contains("Live rendering", sweeper.AuthoredStrings);
        Assert.Equal(["Page 1."], sweeper.MissingTranslations);
    }

    [Fact]
    public void A_sweeping_the_same_element_twice_does_not_double_count_its_strings()
    {
        var host = new StackPanel();
        host.Children.Add(new TextBlock { Text = "Live rendering" });

        var sweeper = new TextSweeper();
        sweeper.Apply(host);
        sweeper.Apply(host);

        Assert.Single(sweeper.AuthoredStrings);
    }

    [Fact]
    public void A_detached_page_is_swept_when_it_is_offered_as_its_own_root()
    {
        UiText.Add("Only on page four", "只在第四页");
        UiText.SetLanguage(UiLanguage.ZhHans);
        var detached = new StackPanel();
        detached.Children.Add(new TextBlock { Text = "Only on page four" });

        new TextSweeper().Apply([detached]);

        // The Gallery builds all thirteen pages in its constructor and attaches one at a time, so this is the
        // shape a language switch has to handle: a walk from the window alone would miss twelve pages.
        Assert.Equal("只在第四页", ((TextBlock)detached.Children[0]).Text);
    }
}
