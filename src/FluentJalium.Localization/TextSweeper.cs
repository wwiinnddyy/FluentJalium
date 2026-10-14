using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Localization;

/// <summary>Which property on an element carries a user-visible string.</summary>
public enum TextSlot
{
    /// <summary><c>TextBlock.Text</c>, <c>MenuFlyoutItem.Text</c>.</summary>
    Text,

    /// <summary><c>ContentControl.Content</c> - buttons, combo items, navigation items, teaching tips.</summary>
    Content,

    /// <summary><c>HeaderedContentControl.Header</c>, <c>MenuItem.Header</c>.</summary>
    Header,

    /// <summary><c>Window.Title</c>, <c>MenuBarItem.Title</c>, <c>InfoBar.Title</c>.</summary>
    Title,

    /// <summary><c>AppBarButton.Label</c>, <c>AppBarToggleButton.Label</c>.</summary>
    Label,

    /// <summary><c>InfoBar.Message</c> - the second line under <see cref="Title"/>.</summary>
    Message,
}

/// <summary>
/// Localizes an already-built element tree by matching the text the markup authored, rather than by element name.
/// </summary>
/// <remarks>
/// <para>
/// The alternative - an <c>x:Name</c> to string table - would have meant naming and registering every one of the
/// Gallery's 266 text-bearing elements and editing that many attributes in an 885-line page file. Keying on the
/// authored English instead leaves the English where it belongs, next to the control it describes, and localizes the
/// code-behind's generated menu and tab labels through the same <see cref="UiText"/> catalog without a second
/// mechanism.
/// </para>
/// <para>
/// Each element's authored text is captured on the first sweep and remembered, so a language change writes from
/// the original rather than translating an already-translated string - which is what makes switching back to
/// English lossless, and what makes a second sweep in the same language a no-op. Elements outside the tree
/// (a <c>MenuFlyout</c>'s items, a detached page) are reached by sweeping the object that owns them.
/// </para>
/// <para>
/// <see cref="MissingTranslations"/> reports every authored string with no 简体中文 entry, so the coverage test
/// names the gap instead of leaving one page in the wrong language.
/// </para>
/// </remarks>
public sealed class TextSweeper
{
    private static readonly TextSlot[] Slots =
        [TextSlot.Text, TextSlot.Content, TextSlot.Header, TextSlot.Title, TextSlot.Label, TextSlot.Message];

    private readonly Dictionary<FrameworkElement, string?[]> _authored = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> _authoredStrings = new(StringComparer.Ordinal);
    private readonly HashSet<FrameworkElement> _seen = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Every distinct English string this sweeper has seen, whether or not it has a 简体中文 entry. The coverage
    /// test pairs this with <see cref="UiText.HasChinese"/> to name the exact strings still untranslated.
    /// </summary>
    public IReadOnlyCollection<string> AuthoredStrings => _authoredStrings;

    /// <summary>The subset of <see cref="AuthoredStrings"/> with no 简体中文 entry.</summary>
    public IReadOnlyCollection<string> MissingTranslations =>
        [.. _authoredStrings.Where(source => !UiText.HasChinese(source))];

    /// <summary>
    /// Writes the current language's text over <paramref name="root"/> and everything reachable from it.
    /// Call once per tree after it is built, and again on every language change.
    /// </summary>
    public void Apply(FrameworkElement root)
    {
        ArgumentNullException.ThrowIfNull(root);
        ApplyTo(LogicalTreeWalker.DescendantsAndSelf(root));
    }

    /// <summary>
    /// Sweeps a set of roots that are not reachable from one another - the Gallery's twelve detached pages, for
    /// instance, which exist as built elements but are attached one at a time.
    /// </summary>
    public void Apply(IEnumerable<FrameworkElement> roots)
    {
        ArgumentNullException.ThrowIfNull(roots);
        foreach (var root in roots)
        {
            Apply(root);
        }
    }

    /// <summary>Forgets the captured text so the next sweep re-reads the tree. Tests only.</summary>
    internal void Reset()
    {
        _authored.Clear();
        _authoredStrings.Clear();
        _seen.Clear();
    }

    private void ApplyTo(IEnumerable<DependencyObject> nodes)
    {
        _seen.Clear();
        foreach (var node in nodes)
        {
            if (node is not FrameworkElement element)
            {
                continue;
            }

            _seen.Add(element);
            var authored = Capture(element);
            for (var index = 0; index < Slots.Length; index++)
            {
                if (authored[index] is { } source)
                {
                    Write(element, Slots[index], UiText.T(source));
                }
            }
        }

        // An element that has gone away - a page closed, a dialog torn down - must not keep its captured text, or
        // the sweeper would grow without bound over a long session.
        foreach (var stale in _authored.Keys.ToList())
        {
            if (!_seen.Contains(stale))
            {
                _authored.Remove(stale);
            }
        }
    }

    private string?[] Capture(FrameworkElement element)
    {
        if (_authored.TryGetValue(element, out var existing))
        {
            return existing;
        }

        var authored = new string?[Slots.Length];
        for (var index = 0; index < Slots.Length; index++)
        {
            if (Read(element, Slots[index]) is not { Length: > 0 } value)
            {
                continue;
            }

            authored[index] = value;
            _authoredStrings.Add(value);
        }

        _authored[element] = authored;
        return authored;
    }

    private static string? Read(FrameworkElement element, TextSlot slot) => slot switch
    {
        TextSlot.Text when element is TextBlock textBlock => textBlock.Text,
        TextSlot.Text when element is MenuFlyoutItem flyoutItem => flyoutItem.Text,
        TextSlot.Content when element is ContentControl contentControl => contentControl.Content as string,
        TextSlot.Header when element is HeaderedContentControl headered => headered.Header as string,
        TextSlot.Header when element is MenuItem menuItem => menuItem.Header as string,
        TextSlot.Title when element is Window window => window.Title,
        TextSlot.Title when element is MenuBarItem menuBarItem => menuBarItem.Title,
        TextSlot.Title when element is InfoBar infoBar => infoBar.Title,
        TextSlot.Label when element is AppBarButton appBarButton => appBarButton.Label,
        TextSlot.Label when element is AppBarToggleButton appBarToggleButton => appBarToggleButton.Label,
        TextSlot.Message when element is InfoBar messageBar => messageBar.Message,
        _ => null,
    };

    private static void Write(FrameworkElement element, TextSlot slot, string value)
    {
        switch (slot)
        {
            case TextSlot.Text when element is TextBlock textBlock:
                textBlock.Text = value;
                break;
            case TextSlot.Text when element is MenuFlyoutItem flyoutItem:
                flyoutItem.Text = value;
                break;
            case TextSlot.Content when element is ContentControl contentControl:
                contentControl.Content = value;
                break;
            case TextSlot.Header when element is HeaderedContentControl headered:
                headered.Header = value;
                break;
            case TextSlot.Header when element is MenuItem menuItem:
                menuItem.Header = value;
                break;
            case TextSlot.Title when element is Window window:
                window.Title = value;
                break;
            case TextSlot.Title when element is MenuBarItem menuBarItem:
                menuBarItem.Title = value;
                break;
            case TextSlot.Title when element is InfoBar infoBar:
                infoBar.Title = value;
                break;
            case TextSlot.Label when element is AppBarButton appBarButton:
                appBarButton.Label = value;
                break;
            case TextSlot.Label when element is AppBarToggleButton appBarToggleButton:
                appBarToggleButton.Label = value;
                break;
            case TextSlot.Message when element is InfoBar messageBar:
                messageBar.Message = value;
                break;
        }
    }
}
