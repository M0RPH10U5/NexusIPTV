using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace NexusIPTV.App;

public partial class SettingsWindow : Window
{
    public event Action<string>? PlaylistSelected;

    public event Action<string>? EpgSelected;

    public SettingsWindow()
    {
        InitializeComponent();

        ThemeToggle.IsChecked = App.IsDarkTheme;
    }

    private void TitleBar_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }

    private void ThemeToggle_Click(
        object sender,
        RoutedEventArgs e)
    {
        App.ApplyTheme(
            ThemeToggle.IsChecked == true);
    }

    private void LoadPlaylistButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open IPTV Playlist",
            Filter =
                "M3U Playlists (*.m3u;*.m3u8)|*.m3u;*.m3u8|" +
                "All Files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
            return;

        PlaylistPathText.Text = dialog.FileName;

        PlaylistSelected?.Invoke(dialog.FileName);
    }

    private void LoadEpgButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open XMLTV EPG Guide",
            Filter =
                "XMLTV Guide (*.xml)|*.xml|" +
                "All Files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
            return;

        EpgPathText.Text = dialog.FileName;

        EpgSelected?.Invoke(dialog.FileName);
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}