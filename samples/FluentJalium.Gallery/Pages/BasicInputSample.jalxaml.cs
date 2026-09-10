using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Pages;

public sealed partial class BasicInputSample : UserControl
{
    internal System.Collections.IEnumerable? WorkspaceItems => WorkspaceBox.ItemsSource;

    public BasicInputSample()
    {
        InitializeComponent();

        // ItemsSource is an IEnumerable dependency property. Keep the sample data strongly typed
        // in code-behind instead of relying on a comma-delimited attribute (which would either be
        // treated as a scalar string or be ignored by the Jalxaml value converter).
        WorkspaceBox.ItemsSource = new[] { "Design", "Engineering", "Research" };
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        SavedInfo.Title = "Saved";
        SavedInfo.Message = string.IsNullOrWhiteSpace(NameBox.Text)
            ? "Add a display name when you are ready."
            : $"Preferences saved for {NameBox.Text}.";
        SavedInfo.Severity = InfoBarSeverity.Success;
    }
}
