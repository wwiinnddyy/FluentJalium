using FluentJalium.Gallery.Resources;
using System.Text.Json;

namespace FluentJalium.Gallery.Services;

/// <summary>
/// Service for loading dynamic localized content from JSON files.
/// Used for control descriptions, example explanations, and other dynamic content.
/// </summary>
public sealed class DynamicResourceService
{
    private readonly Dictionary<string, JsonDocument> _resources = new();
    private string _currentLanguage = "en-US";

    /// <summary>
    /// Loads a JSON resource file for the current language.
    /// </summary>
    public void LoadResource(string resourceName, string jsonPath)
    {
        try
        {
            var json = File.ReadAllText(jsonPath);
            var doc = JsonDocument.Parse(json);
            _resources[resourceName] = doc;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load resource {resourceName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a localized string from a JSON resource.
    /// </summary>
    public string GetString(string resourceName, string path)
    {
        if (!_resources.TryGetValue(resourceName, out var doc))
            return path;

        try
        {
            var element = doc.RootElement;

            // Navigate to current language
            if (element.TryGetProperty(_currentLanguage, out var langElement))
            {
                element = langElement;
            }
            else if (element.TryGetProperty("en-US", out var fallbackElement))
            {
                element = fallbackElement;
            }
            else
            {
                return path;
            }

            // Navigate path (e.g., "Buttons.Title")
            foreach (var segment in path.Split('.'))
            {
                if (!element.TryGetProperty(segment, out element))
                    return path;
            }

            return element.GetString() ?? path;
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// Changes the current language.
    /// </summary>
    public void SetLanguage(string cultureName)
    {
        _currentLanguage = cultureName;
    }

    /// <summary>
    /// Gets the current language.
    /// </summary>
    public string CurrentLanguage => _currentLanguage;
}
