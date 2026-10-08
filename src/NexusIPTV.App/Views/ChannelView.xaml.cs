using System.Windows;
using System.Windows.Controls;
using NexusIPTV.Core.Models;
using System.Text.Json;
using System.Windows.Input;
using System.IO;

namespace NexusIPTV.App.Views;

public partial class ChannelView : UserControl
{
    private const string FavoritesFilter = "__FAVORITES__";

    private IReadOnlyList<Channel> _allChannels =
        Array.Empty<Channel>();

    private readonly HashSet<string> _favoriteKeys =
        new(StringComparer.OrdinalIgnoreCase);

    private string FavoritesFilePath =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "NexusIPTV",
            "favorites.json");

    public event EventHandler<Channel?>? ChannelSelected;

    public ChannelView ()
    {
        InitializeComponent();

        LoadFavorites();
    }

    public void SetChannels (
        IReadOnlyList<Channel> channels )
    {
        _allChannels=channels;

        ApplyFavoriteState();

        PopulateGroups();

        ApplyFilters();
    }

    private void ApplyFavoriteState ()
    {
        foreach ( var channel in _allChannels )
        {
            channel.IsFavorite=
                _favoriteKeys.Contains(
                    GetFavoriteKey(channel));
        }
    }

    private static string GetFavoriteKey (
        Channel channel )
    {
        if ( !string.IsNullOrWhiteSpace(channel.TvgId) )
        {
            return $"id:{channel.TvgId.Trim()}";
        }

        return $"url:{channel.StreamUrl.Trim()}";
    }

    private void PopulateGroups ()
    {
        GroupComboBox.SelectionChanged-=
            GroupFilter_SelectionChanged;

        GroupComboBox.Items.Clear();

        GroupComboBox.Items.Add(
            new ComboBoxItem
            {
                Content="All Groups",
                Tag=string.Empty,
                IsSelected=true
            });

        GroupComboBox.Items.Add(
            new ComboBoxItem
            {
                Content="★ Favorites",
                Tag=FavoritesFilter
            });

        var groups = _allChannels
            .SelectMany(channel => channel.Groups)
            .Where(group => !string.IsNullOrWhiteSpace(group))
            .Select(group => group.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach ( var group in groups )
        {
            GroupComboBox.Items.Add(
                new ComboBoxItem
                {
                    Content=group,
                    Tag=group
                });
        }

        GroupComboBox.SelectedIndex=0;

        GroupComboBox.SelectionChanged+=
            GroupFilter_SelectionChanged;
    }

    private void SearchBox_TextChanged (
        object sender,
        TextChangedEventArgs e )
    {
        ApplyFilters();
    }

    private void GroupFilter_SelectionChanged (
        object sender,
        SelectionChangedEventArgs e )
    {
        ApplyFilters();
    }

    private void ApplyFilters ()
    {
        if ( !IsInitialized )
            return;

        var searchText =
            SearchTextBox.Text
                .Trim();

        var selectedItem =
            GroupComboBox.SelectedItem as ComboBoxItem;

        var selectedFilter =
            selectedItem?.Tag as string
            ??string.Empty;

        // var selectedGroup =
           // selectedItem?.Tag as string??string.Empty;

        IEnumerable<Channel> filtered =
            _allChannels;

        if ( !string.IsNullOrWhiteSpace(searchText) )
        {
            filtered=filtered.Where(channel =>
                channel.Name.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase)
                ||
                channel.TvgId.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase));
        }

        if ( selectedFilter==FavoritesFilter )
        {
            filtered=filtered.Where(
                channel => channel.IsFavorite);
        }
        else if ( !string.IsNullOrWhiteSpace(selectedFilter) )
        {
            filtered=filtered.Where(channel =>
                channel.Groups.Any(group =>
                    string.Equals(
                        group.Trim(),
                        selectedFilter,
                        StringComparison.OrdinalIgnoreCase)));
        }

        var results =
            filtered.ToList();

        ChannelList.ItemsSource=results;

        NoResultsText.Text=
            selectedFilter==FavoritesFilter
            ? "No Favorites Channels."
            : "No channels match your search.";

        NoResultsText.Visibility=
            results.Count==0
                ? Visibility.Visible
                : Visibility.Collapsed;

        if ( results.Count==0 )
        {
            ChannelList.SelectedItem=null;
        }
    }

    private void ChannelList_SelectionChanged (
        object sender,
        SelectionChangedEventArgs e )
    {
        if ( ChannelList.SelectedItem is Channel channel )
        {
            ChannelList.ScrollIntoView(channel);
        }
    }

    private void ChannelList_PreviewMouseLeftButtonUp (
        object sender,
        MouseButtonEventArgs e )
    {
        if ( ChannelList.SelectedItem is not Channel channel )
            return;

        ChannelSelected?.Invoke(
            this,
            channel);
    }

    private void FavoriteButton_PreviewMouseDown (
        object sender,
        MouseButtonEventArgs e )
    {
        if ( sender is not Button button )
            return;

        if ( button.Tag is not Channel channel )
            return;

        ToggleFavorite(channel);

        e.Handled=true;
    }

    private void FavoriteButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        e.Handled=true;
    }

    private void ToggleFavorite (
        Channel channel )
    {
        var key =
            GetFavoriteKey(channel);

        if ( _favoriteKeys.Contains(key) )
        {
            _favoriteKeys.Remove(key);
            channel.IsFavorite = false;
        }
        else
        {
            _favoriteKeys.Add(key);
            channel.IsFavorite = true;
        }

        SaveFavorites();

        var SelectedItem =
            GroupComboBox.SelectedItem as ComboBoxItem;

        var selectedFilter =
            SelectedItem?.Tag as string
            ??string.Empty;

        if ( selectedFilter==FavoritesFilter )
        {
            ApplyFilters();
        }
        else
        {
            ChannelList.Items.Refresh();
        }
    }

    private void ChannelList_PreviewKeyDown (
        object sender,
        KeyEventArgs e )
    {
        if (ChannelList.SelectedItem is not Channel selectedChannel)
            return;

        if (e.Key == Key.Space)
        {
            ToggleFavorite(selectedChannel);

            e.Handled=true;
            return;
        }

        if ( e.Key == Key.Enter )
        {
            ChannelSelected?.Invoke(
                this,
                selectedChannel);

            e.Handled=true;
        }
    }

    private void LoadFavorites ()
    {
        try
        {
            if ( !File.Exists(FavoritesFilePath) )
                return;

            var json =
                File.ReadAllText(
                    FavoritesFilePath);

            var favorites =
                JsonSerializer.Deserialize<List<string>>(
                    json);

            if ( favorites is null )
                return;

            foreach ( var favorite in favorites )
            {
                if ( !string.IsNullOrWhiteSpace(favorite) )
                {
                    _favoriteKeys.Add(favorite);
                }
            }
        }
        catch
        {
            _favoriteKeys.Clear();
        }
    }

    private void SaveFavorites ()
    {
        try
        {
            var directory =
                Path.GetDirectoryName(
                    FavoritesFilePath);

            if ( !string.IsNullOrWhiteSpace(directory) )
            {
                Directory.CreateDirectory(directory);
            }

            var Favorites =
                _favoriteKeys
                .OrderBy(
                    key => key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            var json =
                JsonSerializer.Serialize(
                    Favorites,
                    new JsonSerializerOptions
                    {
                        WriteIndented=true
                    });

            File.WriteAllText(
                FavoritesFilePath,
                json);
        }
        catch
        {
            // Favorite state remains active for the
            // current session even if persistence fails.
        }
    }
}