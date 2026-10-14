using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Localization;

/// <summary>
/// Enumerates a framework element's logical children, including subtrees that are built but not currently
/// attached to a live visual tree.
/// </summary>
/// <remarks>
/// <see cref="VisualTreeHelper"/> was the obvious choice and is the wrong one: the Gallery builds all thirteen of
/// its pages in the constructor and then attaches only the selected one, so a visual walk from the window reaches
/// one page and misses the other twelve. Logical enumeration reaches them all, which is what a language switch
/// needs. Every branch here reads a container's own child accessor, so the walk cannot cycle.
/// </remarks>
public static class LogicalTreeWalker
{
    /// <summary>Yields <paramref name="node"/>'s logical children. Non-element content is skipped.</summary>
    public static IEnumerable<DependencyObject> Children(DependencyObject node)
    {
        ArgumentNullException.ThrowIfNull(node);

        switch (node)
        {
            // Panel first: Border and ScrollViewer are not Panels, but Panel is the widest and most common case.
            // UIElementCollection is a non-generic IList, so the element type is restored by the loop variable.
            case Panel panel:
                foreach (UIElement child in panel.Children)
                {
                    if (child is not null)
                    {
                        yield return child;
                    }
                }

                break;

            case Decorator decorator when decorator.Child is { } decorated:
                yield return decorated;
                break;

            case ContentPresenter presenter when presenter.Content is DependencyObject presented:
                yield return presented;
                break;

            // ContentControl after Decorator and ContentPresenter: Window and ScrollViewer land here, and both
            // expose the content the host actually wants localized.
            case ContentControl host when host.Content is DependencyObject content:
                yield return content;
                break;

            // Items last, and only for containers that are not ContentControls (a ComboBox is a plain ItemsControl,
            // so its declared ComboBoxItems are still reached; a Button's Content was already taken above).
            case ItemsControl items when items.Items.Count > 0:
                foreach (var item in items.Items)
                {
                    if (item is DependencyObject element)
                    {
                        yield return element;
                    }
                }

                break;
        }
    }

    /// <summary>Yields <paramref name="root"/> and every logical descendant, depth first.</summary>
    public static IEnumerable<DependencyObject> DescendantsAndSelf(DependencyObject root)
    {
        var pending = new Stack<DependencyObject>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            yield return current;

            // Reversed so the walk reports siblings in declaration order.
            var children = new List<DependencyObject>();
            foreach (var child in Children(current))
            {
                children.Add(child);
            }

            for (var index = children.Count - 1; index >= 0; index--)
            {
                pending.Push(children[index]);
            }
        }
    }
}
