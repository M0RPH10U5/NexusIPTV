using System.Windows;

namespace NexusIPTV.App;

public enum AppTheme
{
    Dark,
    Light
}

public static class ThemeManager
{
    private static readonly Uri DarkThemeUri =
        new("/NexusIPTV.App;component/Themes/DarkTheme.xaml", UriKind.Relative);

    private static readonly Uri LightThemeUri =
        new("/NexusIPTV.App;component/Themes/LightTheme.xaml", UriKind.Relative);

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void ApplyTheme(AppTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;

        var existingTheme = dictionaries.FirstOrDefault(
            dictionary =>
                dictionary.Source?.OriginalString.Contains(
                    "/Themes/DarkTheme.xaml",
                    StringComparison.OrdinalIgnoreCase) == true ||
                dictionary.Source?.OriginalString.Contains(
                    "/Themes/LightTheme.xaml",
                    StringComparison.OrdinalIgnoreCase) == true);

        if (existingTheme is not null)
            dictionaries.Remove(existingTheme);

        var themeDictionary = new ResourceDictionary
        {
            Source = theme == AppTheme.Dark
                ? DarkThemeUri
                : LightThemeUri
        };

        dictionaries.Add(themeDictionary);

        CurrentTheme = theme;
    }
}
