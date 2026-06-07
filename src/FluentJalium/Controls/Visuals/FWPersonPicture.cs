using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using Jalium.UI.Media.Imaging;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium PersonPicture control for displaying user avatars.
/// </summary>
public class FWPersonPicture : Control, IFluentJaliumControl
{
    public static readonly DependencyProperty ProfilePictureProperty =
        DependencyProperty.Register(nameof(ProfilePicture), typeof(ImageSource), typeof(FWPersonPicture),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(nameof(Initials), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty, OnDisplayNameChanged));

    public static readonly DependencyProperty IsGroupProperty =
        DependencyProperty.Register(nameof(IsGroup), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false));

    public static readonly DependencyProperty BadgeNumberProperty =
        DependencyProperty.Register(nameof(BadgeNumber), typeof(int), typeof(FWPersonPicture),
            new PropertyMetadata(0));

    public static readonly DependencyProperty BadgeGlyphProperty =
        DependencyProperty.Register(nameof(BadgeGlyph), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeImageSourceProperty =
        DependencyProperty.Register(nameof(BadgeImageSource), typeof(ImageSource), typeof(FWPersonPicture),
            new PropertyMetadata(null));

    public static readonly DependencyProperty PreferSmallImageProperty =
        DependencyProperty.Register(nameof(PreferSmallImage), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWPersonPicture"/> class.
    /// </summary>
    public FWPersonPicture()
    {
        Width = 48;
        Height = 48;
    }

    /// <summary>
    /// Gets or sets the profile picture image source.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ImageSource? ProfilePicture
    {
        get => (ImageSource?)GetValue(ProfilePictureProperty);
        set => SetValue(ProfilePictureProperty, value);
    }

    /// <summary>
    /// Gets or sets the initials to display when no profile picture is available.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string Initials
    {
        get => (string)GetValue(InitialsProperty)!;
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>
    /// Gets or sets the display name used to generate initials.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty)!;
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this represents a group rather than an individual.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool IsGroup
    {
        get => (bool)GetValue(IsGroupProperty)!;
        set => SetValue(IsGroupProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge number to display (0 to hide badge).
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int BadgeNumber
    {
        get => (int)GetValue(BadgeNumberProperty)!;
        set => SetValue(BadgeNumberProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge glyph to display.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string BadgeGlyph
    {
        get => (string)GetValue(BadgeGlyphProperty)!;
        set => SetValue(BadgeGlyphProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge image source.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ImageSource? BadgeImageSource
    {
        get => (ImageSource?)GetValue(BadgeImageSourceProperty);
        set => SetValue(BadgeImageSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to prefer smaller image sizes for better performance.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool PreferSmallImage
    {
        get => (bool)GetValue(PreferSmallImageProperty)!;
        set => SetValue(PreferSmallImageProperty, value);
    }

    private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPersonPicture personPicture && e.NewValue is string displayName)
        {
            // Auto-generate initials from display name if Initials property is not set
            if (string.IsNullOrEmpty(personPicture.Initials))
            {
                personPicture.Initials = GenerateInitials(displayName);
            }
        }
    }

    private static string GenerateInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return string.Empty;

        var parts = displayName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return string.Empty;

        if (parts.Length == 1)
        {
            // Single name: take first two characters
            return parts[0].Length >= 2
                ? parts[0].Substring(0, 2).ToUpper()
                : parts[0].ToUpper();
        }

        // Multiple names: take first character of first and last name
        return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
    }
}
