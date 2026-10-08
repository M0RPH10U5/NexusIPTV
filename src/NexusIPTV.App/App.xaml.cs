using System.Windows;

namespace NexusIPTV.App;

public partial class App : Application
{
    public static bool IsDarkTheme { get; private set; } = true;

    public static void ApplyTheme(bool dark)
    {
        var app = Current;

        if (app == null)
            return;

        IsDarkTheme = dark;

        var dictionaries = app.Resources.MergedDictionaries;

        var existingTheme = dictionaries
            .FirstOrDefault(d =>
                d.Source?.OriginalString.Contains(
                    "DarkTheme.xaml",
                    StringComparison.OrdinalIgnoreCase) == true
                ||
                d.Source?.OriginalString.Contains(
                    "LightTheme.xaml",
                    StringComparison.OrdinalIgnoreCase) == true);

        if (existingTheme != null)
            dictionaries.Remove(existingTheme);

        var themeUri = dark
            ? new Uri(
                "Themes/DarkTheme.xaml",
                UriKind.Relative)
            : new Uri(
                "Themes/LightTheme.xaml",
                UriKind.Relative);

        dictionaries.Add(
            new ResourceDictionary
            {
                Source = themeUri
            });
    }
}