using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// The single row of crumbs: upstream's <c>BreadcrumbLayout</c> measured in the same order it uses - sum the
/// crumbs, and when they do not fit, keep a contiguous suffix counted from the last one and hand the head slot to
/// the ellipsis.
/// </summary>
/// <remarks>
/// <para>
/// Own type for two separate reasons. The row cannot be a stock <see cref="StackPanel"/> because
/// <c>ItemsPanelTemplate</c> on this runtime carries only a panel type, so <c>Orientation</c> has no route to it
/// (the same finding that forced <see cref="FluentRadioButtonsPanel"/>). The collapse cannot be upstream's
/// mechanism because a zero arrange rect sets the offset but not the extent here: a child that carries its own
/// size keeps it and re-stacks at the origin, still painted and still hit-testable, which
/// <c>spike/BreadcrumbProbe</c> pass C measured and pass D replaced with <see cref="Visibility"/> = Collapsed
/// (adaptation/s1n [G4], [G5]).
/// </para>
/// <para>
/// The ellipsis is not this panel's child - it is a part of the bar's template, in the column to the left of the
/// items host. Upstream's slot 0 is a synthetic crumb peer instead. The two differ in one observable way that is
/// recorded rather than hidden: nothing here can make a crumb's width depend on whether the ellipsis is shown, so
/// the fit is a fixed point on the first pass instead of one that has to settle.
/// </para>
/// </remarks>
public class FluentBreadcrumbPanel : Panel
{
    private FrameworkElement[] _crumbs = [];

    /// <summary>Gets the index of the first crumb the last arrange pass kept, or the crumb count while nothing is
    /// laid out.</summary>
    public int FirstVisibleIndex { get; private set; }

    /// <summary>Gets the width the last arrange pass compared against - the row it was given.</summary>
    public double ArrangedWidth { get; private set; }

    /// <summary>Gets what the crumbs of the last arrange pass summed to at their measured widths.</summary>
    public double MeasuredWidth { get; private set; }

    /// <summary>Gets the width the last pass judged the row against, which is the host's row rather than this
    /// panel's own column.</summary>
    public double ComparedAgainst { get; private set; }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Every child is measured unconstrained along the row, including one a previous pass collapsed: a crumb
        // that would be clipped has to be compared at its real width, and the framework leaves a collapsed
        // element's DesiredSize at zero, which would make the fit below see an empty crumb rather than a wide one.
        var crumbs = Children.OfType<FrameworkElement>().ToArray();
        _crumbs = crumbs;
        var width = 0d;
        var height = 0d;
        foreach (var crumb in crumbs)
        {
            crumb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            width += crumb.DesiredSize.Width;
            height = Math.Max(height, crumb.DesiredSize.Height);
        }

        // The same sum upstream's measure pass returns (BreadcrumbLayout.cpp:41-71): every crumb counts, and the
        // row never reserves room for a crumb it is about to drop.
        return new Size(width, height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var crumbs = _crumbs;
        var count = crumbs.Length;
        if (count == 0)
        {
            FirstVisibleIndex = 0;
            Host()?.ReportBreadcrumbOverflow(0, crumbs);
            return finalSize;
        }

        var ellipsis = Host()?.EllipsisDesired() ?? default;
        var measured = MeasureWidth();
        var host = Host();

        // The row is judged against the width the bar was given, not against the width this panel was handed: the
        // ellipsis sits in the column to the left, so the panel's own width is what the answer is about to change.
        // Comparing against the host's width keeps the decision a fixed point - measured/s1n [G3].
        var available = host?.RowWidth ?? finalSize.Width;
        ArrangedWidth = finalSize.Width;
        MeasuredWidth = measured;
        ComparedAgainst = available;
        var overflow = measured > available;
        var first = 0;
        if (overflow)
        {
            // BreadcrumbLayout.cpp:101-118: seed with the last crumb plus the ellipsis and walk leftwards,
            // stopping at the first crumb that would not fit. The floor is the ellipsis and the last crumb, so the
            // visible set is always a contiguous suffix and never a crumb with a gap to its right.
            first = count - 1;
            var accumulated = crumbs[count - 1].DesiredSize.Width + ellipsis.Width;
            for (var index = count - 2; index >= 0; index--)
            {
                var next = accumulated + crumbs[index].DesiredSize.Width;
                if (next > available)
                {
                    break;
                }

                accumulated = next;
                first = index;
            }
        }

        // BreadcrumbLayout.cpp:120-135: the row is as tall as the tallest rendered crumb, and the ellipsis joins
        // that maximum only while it renders, so a tall ellipsis is never clipped by a short crumb row.
        var rowHeight = overflow ? ellipsis.Height : 0d;
        for (var index = first; index < count; index++)
        {
            rowHeight = Math.Max(rowHeight, crumbs[index].DesiredSize.Height);
        }

        var x = 0d;
        for (var index = 0; index < count; index++)
        {
            var crumb = crumbs[index];
            if (index < first)
            {
                // Hidden, and not Collapsed, for a measured reason: a collapsed container leaves the panel's
                // Children entirely (spike/BreadcrumbProbe mode narrow read the row back at three of four crumbs
                // after the first hide, which turned the next fit into "everything fits"), so the next pass would
                // no longer know what was dropped. Hidden keeps it in the list and keeps its DesiredSize, and this
                // method is what refuses it a place in the row.
                if (crumb.Visibility != Visibility.Hidden)
                {
                    crumb.Visibility = Visibility.Hidden;
                }

                continue;
            }

            if (crumb.Visibility != Visibility.Visible)
            {
                crumb.Visibility = Visibility.Visible;
            }

            // Each crumb is arranged at its own measured width with no spacing (BreadcrumbLayout.cpp:74-81 adds
            // only the width), and stretches to the row height, which is what centers its chevron.
            crumb.Arrange(new Rect(x, 0, crumb.DesiredSize.Width, rowHeight));
            x += crumb.DesiredSize.Width;
        }

        FirstVisibleIndex = first;
        Host()?.ReportBreadcrumbOverflow(first, crumbs);
        return finalSize;
    }

    private double MeasureWidth()
    {
        var width = 0d;
        foreach (var crumb in _crumbs)
        {
            width += crumb.DesiredSize.Width;
        }

        return width;
    }

    private FluentBreadcrumbBar? Host()
    {
        if (TemplatedParent is FluentBreadcrumbBar direct)
        {
            return direct;
        }

        // Measured: the items panel a presenter builds has no TemplatedParent of its own, so the host is one
        // Parent step away (spike/BreadcrumbProbe pass B, the same asymmetry FluentRadioButtonsPanel records).
        var node = Parent as FrameworkElement;
        for (var hop = 0; node is not null && hop < 8; hop++)
        {
            if (node is FluentBreadcrumbBar host)
            {
                return host;
            }

            node = node.Parent as FrameworkElement;
        }

        return null;
    }
}
