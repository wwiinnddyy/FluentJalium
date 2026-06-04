using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using System.Text.RegularExpressions;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Controls;

internal static class GallerySampleCard
{
    public static FWBorder Create(
        FluentIconRegular icon,
        string title,
        string description,
        UIElement sample,
        UIElement? states = null,
        UIElement? properties = null,
        string? code = null,
        double width = 570)
    {
        var statesContent = states ?? CreateDefaultStates(title, code);
        var propertiesContent = properties ?? CreateDefaultProperties(title, code);

        var sections = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateHeader(icon, title),
                new FWTextBlock
                {
                    Text = description,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                },
                CreateRegion("Example", sample)
            }
        };

        sections.Children.Add(CreateRegion("States", statesContent));
        sections.Children.Add(CreateRegion("Properties", propertiesContent));

        if (!string.IsNullOrWhiteSpace(code))
        {
            sections.Children.Add(CreateRegion("Code / Notes", CreateCodeBlock(code)));
        }

        return new FWBorder
        {
            Width = width,
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = sections
        };
    }

    private static UIElement CreateHeader(FluentIconRegular icon, string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                FluentIconFactory.Regular(icon, 20, GalleryThemeResources.Brush("TextPrimary")),
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 15,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateRegion(string label, UIElement content)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    FontSize = 11,
                    Foreground = GalleryThemeResources.Brush("TextSecondary")
                },
                content
            }
        };
    }

    private static UIElement CreateCodeBlock(string code)
    {
        return new Border
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWTextBlock
            {
                Text = code,
                FontFamily = "Cascadia Code",
                FontSize = 12,
                Foreground = GalleryThemeResources.Brush("TextPrimary"),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }

    private static UIElement CreateDefaultStates(string title, string? code)
    {
        var states = new List<string> { "Normal", "Pointer over", "Pressed/focused", "Disabled" };
        var source = $"{title} {code}";

        if (ContainsAny(source, "Toggle", "Switch", "CheckBox", "RadioButton", "Selected", "Selection", "Rating", "List", "Tree", "Tab", "Navigation"))
        {
            states.Insert(3, "Selected/checked");
        }

        if (ContainsAny(source, "DropDown", "Split", "ComboBox", "Menu", "Flyout", "Expander", "Dialog", "Calendar", "Picker"))
        {
            states.Insert(3, "Opened");
        }

        if (ContainsAny(source, "Progress", "Loading", "Media", "Toast", "InfoBar", "Status"))
        {
            states.Insert(3, "Active");
        }

        return CreateTokenRow(states);
    }

    private static UIElement CreateDefaultProperties(string title, string? code)
    {
        var properties = new List<string>();

        foreach (var controlName in ExtractMatches(code, @"<\s*(FW[A-Za-z0-9_]+)", 1, 2))
        {
            properties.Add(controlName);
        }

        foreach (var propertyName in ExtractMatches(code, @"\b([A-Za-z][A-Za-z0-9_.:-]*)\s*=", 1, 5))
        {
            if (!propertyName.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(propertyName, "x:Name", StringComparison.OrdinalIgnoreCase))
            {
                properties.Add(propertyName);
            }
        }

        if (properties.Count == 0)
        {
            properties.AddRange(ContainsAny(title, "Color", "Typography", "Geometry", "Motion", "Material", "Backdrop")
                ? ["Theme resources", "Accent", "Variant"]
                : ["Content", "IsEnabled", "Theme resources"]);
        }

        return CreateTokenRow(properties.Distinct(StringComparer.OrdinalIgnoreCase).Take(6));
    }

    private static UIElement CreateTokenRow(IEnumerable<string> tokens)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 6,
            VerticalSpacing = 6
        };

        foreach (var token in tokens)
        {
            row.Children.Add(CreateToken(token));
        }

        return row;
    }

    private static UIElement CreateToken(string text)
    {
        return new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 3, 8, 3),
            Child = new FWTextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = GalleryThemeResources.Brush("TextSecondary")
            }
        };
    }

    private static IEnumerable<string> ExtractMatches(string? source, string pattern, int groupIndex, int maxCount)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            yield break;
        }

        foreach (Match match in Regex.Matches(source, pattern))
        {
            if (match.Groups.Count > groupIndex)
            {
                yield return match.Groups[groupIndex].Value;
            }

            if (--maxCount == 0)
            {
                yield break;
            }
        }
    }

    private static bool ContainsAny(string source, params string[] values)
    {
        return values.Any(value => source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
