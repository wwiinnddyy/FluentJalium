using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// The one layout <see cref="FluentRadioButtons"/> needs and the runtime has: uniform cells sized to the largest
/// item, filled down the first column before the second. That is upstream's
/// <c>ColumnMajorUniformToLargestGridLayout</c> (<c>RadioButtonsPrimitives.idl:7</c>), and its three inputs are
/// here as property rows an app can set.
/// </summary>
/// <remarks>
/// Own type because the value cannot be handed to a stock panel. <c>ItemsPanelTemplate</c> on this runtime carries
/// only a <c>PanelType</c> (<c>DataTemplateSelector.cs</c> - and its <c>FrameworkElementFactory</c> constructor,
/// the one API that could set a property on the panel, copies <c>root.Type</c> and drops every value it was told
/// to set), so <c>UniformGrid.Columns</c> has no route to the host's <c>MaxColumns</c>. The panel therefore reads
/// its own column count from the control it sits in, which is how <c>ItemsPresenter</c> hands a template's root
/// to the items host anyway.
/// </remarks>
public class FluentRadioButtonsPanel : Panel
{
    // Upstream RadioButtons_themeresources.xaml:17-18 hold these two metrics as x:Double rows, which this reader
    // cannot publish at all, so 7 and 8 arrive as the defaults of properties named after them.
    /// <summary>Identifies the <see cref="ColumnSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing), typeof(double), typeof(FluentRadioButtonsPanel),
        new PropertyMetadata(7d, OnGeometryChanged));

    /// <summary>Identifies the <see cref="RowSpacing"/> dependency property.</summary>
    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing), typeof(double), typeof(FluentRadioButtonsPanel),
        new PropertyMetadata(8d, OnGeometryChanged));

    private int _rows = 1;
    private int _columns = 1;
    private double _cellWidth;
    private double _cellHeight;

    /// <summary>Gets or sets the horizontal gap between two cells.</summary>
    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty)!;
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>Gets or sets the vertical gap between two cells.</summary>
    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty)!;
        set => SetValue(RowSpacingProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = Columns();
        var count = Children.Count;
        _columns = Math.Max(1, columns);
        _rows = count == 0 ? 1 : (int)Math.Ceiling(count / (double)_columns);

        var width = 0d;
        var height = 0d;

        // Children enumerates as object, so the framework elements are taken through one typed pass.
        foreach (var child in Children.OfType<FrameworkElement>())
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desired = child.DesiredSize;
            if (desired.Width > width)
            {
                width = desired.Width;
            }

            if (desired.Height > height)
            {
                height = desired.Height;
            }
        }

        // One cell size for the whole grid, at the largest item - upstream's "UniformToLargest".
        _cellWidth = width;
        _cellHeight = height;

        var spacing = new Size(
            Math.Max(0, _columns - 1) * ColumnSpacing,
            Math.Max(0, _rows - 1) * RowSpacing);
        return new Size(_columns * _cellWidth + spacing.Width, _rows * _cellHeight + spacing.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var step = new Size(_cellWidth + ColumnSpacing, _cellHeight + RowSpacing);
        var originX = Math.Max(0, (finalSize.Width - (_columns * _cellWidth + Math.Max(0, _columns - 1) * ColumnSpacing)) / 2);
        var index = 0;
        foreach (var child in Children.OfType<FrameworkElement>())
        {
            // Filling down the column first is the whole point of this panel: item 0, 1, 2 go in the left column
            // when two columns are in use, exactly as upstream's layout orders them.
            var column = index / _rows;
            var row = index % _rows;
            child.Arrange(new Rect(
                originX + (column * step.Width),
                row * step.Height,
                _cellWidth,
                _cellHeight));
            index++;
        }

        return finalSize;
    }

    private static void OnGeometryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var panel = (FluentRadioButtonsPanel)sender;
        panel.InvalidateMeasure();
        panel.InvalidateArrange();
    }

    private int Columns()
    {
        // Measured, not assumed: the items panel a presenter builds carries no TemplatedParent of its own
        // (AstraRadioButtonsTests.MaxColumns_reaches_the_layout... fails at one column if this stops at
        // TemplatedParent), so the host is found through the same parent walk ItemsPresenter does.
        return ResolveHost()?.MaxColumns is { } maxColumns ? Math.Max(1, maxColumns) : 1;
    }

    private FluentRadioButtons? ResolveHost()
    {
        if (TemplatedParent is FluentRadioButtons direct)
        {
            return direct;
        }

        var node = (FrameworkElement?)Parent;
        for (var hop = 0; node is not null && hop < 8; hop++)
        {
            if (node is FluentRadioButtons host)
            {
                return host;
            }

            node = node.Parent as FrameworkElement;
        }

        return null;
    }
}
