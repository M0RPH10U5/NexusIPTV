using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NexusIPTV.Core.Epg;
using NexusIPTV.Core.M3U;
using NexusIPTV.Core.Matching;
using NexusIPTV.Core.Models;
using NexusIPTV.Vlc;

namespace NexusIPTV.App;

public partial class MainWindow : Window
{
    private VlcController? _vlc;
    private FullscreenWindow? _fullscreenWindow;

    private IReadOnlyList<PlaylistEntry> _playlistEntries =
        Array.Empty<PlaylistEntry>();

    private IReadOnlyList<Programme> _programmes =
        Array.Empty<Programme>();

    private IReadOnlyDictionary<string, string> _xmlTvChannels =
        new Dictionary<string, string>();

    private readonly DispatcherTimer _uiTimer;

    private bool _isMuted;
    private int _volumeBeforeMute = 100;

    public MainWindow ()
    {
        InitializeComponent();

        _vlc=new VlcController();

        VideoView.MediaPlayer=_vlc.MediaPlayer;

        ChannelView.ChannelSelected+=ChannelView_ChannelSelected;

        Closed+=MainWindow_Closed;

        _uiTimer=new DispatcherTimer
        {
            Interval=TimeSpan.FromSeconds(1)
        };

        _uiTimer.Tick+=UiTimer_Tick;
        _uiTimer.Start();
    }

    private void UiTimer_Tick (
        object? sender,
        EventArgs e )
    {
        UpdatePlaybackTime();
        UpdateProgrammeProgress();
        EpgView.UpdateCurrentProgramme(DateTimeOffset.Now);
    }

    private void UpdatePlaybackTime ()
    {
        if ( _vlc is null )
            return;

        var player = _vlc.MediaPlayer;

        if ( !player.IsPlaying&&player.Time<=0 )
        {
            PlaybackTimeText.Text="--:-- / --:--";
            return;
        }

        if ( player.Length<=0 )
        {
            PlaybackTimeText.Text="--:-- / --:--";
            return;
        }

        var current = TimeSpan.FromMilliseconds(
            Math.Max(0, player.Time));

        var total = TimeSpan.FromMilliseconds(
            Math.Max(0, player.Length));

        PlaybackTimeText.Text=
            $"{FormatPlaybackTime(current)} / {FormatPlaybackTime(total)}";
    }

    private static string FormatPlaybackTime ( TimeSpan time )
    {
        if ( time.TotalHours>=1 )
            return time.ToString(@"h\:mm\:ss");

        return time.ToString(@"m\:ss");
    }

    private void UpdateProgrammeProgress ()
    {
        var programme = EpgView.CurrentProgramme;

        if ( programme is null )
        {
            PlaybackProgressBar.Value=0;
            return;
        }

        var now = DateTimeOffset.Now;

        if ( now<=programme.Start )
        {
            PlaybackProgressBar.Value=0;
            return;
        }

        if ( now>=programme.Stop )
        {
            PlaybackProgressBar.Value=100;
            return;
        }

        var duration =
            (programme.Stop-programme.Start).TotalSeconds;

        if ( duration<=0 )
        {
            PlaybackProgressBar.Value=0;
            return;
        }

        var elapsed =
            (now-programme.Start).TotalSeconds;

        PlaybackProgressBar.Value=
            Math.Clamp(
                elapsed/duration*100,
                0,
                100);
    }

    private void TitleBar_MouseLeftButtonDown (
        object sender,
        MouseButtonEventArgs e )
    {
        if ( e.ClickCount==2 )
        {
            ToggleMaximize();
            return;
        }

        if ( e.LeftButton==MouseButtonState.Pressed )
            DragMove();
    }

    private void MinimizeButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        WindowState=WindowState.Minimized;
    }

    private void MaximizeButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        ToggleMaximize();
    }

    private void CloseButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        Close();
    }

    private void ToggleMaximize ()
    {
        WindowState=
            WindowState==WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
    }

    private void SettingsButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        var window = new SettingsWindow
        {
            Owner=this
        };

        window.PlaylistSelected+=
            SettingsWindow_PlaylistSelected;

        window.EpgSelected+=
            SettingsWindow_EpgSelected;

        window.ShowDialog();
    }

    private void SettingsWindow_PlaylistSelected (
        string path )
    {
        try
        {
            LoadPlaylist(path);
        }
        catch ( Exception ex )
        {
            MessageBox.Show(
                this,
                $"The playlist could not be loaded.\n\n{ex.Message}",
                "Playlist Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SettingsWindow_EpgSelected (
        string path )
    {
        try
        {
            var result = EpgServices.Load(path);

            _programmes=result.Programmes;
            _xmlTvChannels=result.Channels;

            RefreshChannels();

            MessageBox.Show(
                this,
                $"EPG loaded successfully.\n\n"+
                $"Channels: {_xmlTvChannels.Count:N0}\n"+
                $"Programmes: {_programmes.Count:N0}",
                "EPG Loaded",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch ( Exception ex )
        {
            MessageBox.Show(
                this,
                $"The EPG could not be loaded.\n\n{ex.Message}",
                "EPG Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadPlaylist ( string path )
    {
        _playlistEntries=M3UParser.ParseFile(path);
        RefreshChannels();
    }

    private void RefreshChannels ()
    {
        if ( _playlistEntries.Count==0 )
        {
            ChannelView.SetChannels(Array.Empty<Channel>());
            ChannelCountText.Text="0 channels";
            PlayerEmptyText.Visibility=Visibility.Visible;
            EpgView.SetChannel(null);
            PlaybackProgressBar.Value=0;
            PlaybackTimeText.Text="--:-- / --:--";
            return;
        }

        IReadOnlyList<Channel> channels;

        if ( _programmes.Count>0 )
        {
            channels=ChannelMatcher.Match(
                _playlistEntries,
                _programmes,
                _xmlTvChannels);
        }
        else
        {
            channels=_playlistEntries
                .Select(entry => new Channel
                {
                    TvgId=entry.TvgId,
                    Name=string.IsNullOrWhiteSpace(entry.Name)
                        ? "Unnamed Channel"
                        : entry.Name,
                    LogoUrl=entry.LogoUrl,
                    StreamUrl=entry.StreamUrl,
                    Groups=entry.Groups
                })
                .ToList();
        }

        ChannelView.SetChannels(channels);

        ChannelCountText.Text=
            channels.Count==1
                ? "1 channel"
                : $"{channels.Count:N0} channels";

        PlayerEmptyText.Visibility=Visibility.Visible;

        EpgView.SetChannel(null);

        PlaybackProgressBar.Value=0;
        PlaybackTimeText.Text="--:-- / --:--";

        Title=
            $"VLC IPTV — {channels.Count:N0} channels";
    }

    private void ChannelView_ChannelSelected (
        object? sender,
        Channel? channel )
    {
        if ( channel is null||
            string.IsNullOrWhiteSpace(channel.StreamUrl) )
            return;

        try
        {
            ExitFullscreen();

            if ( _vlc is null )
                return;

            _vlc.Play(channel.StreamUrl);

            PlayerEmptyText.Visibility=Visibility.Collapsed;
            EpgView.SetChannel(channel);

            PlaybackProgressBar.Value=0;
            PlaybackTimeText.Text="--:-- / --:--";

            PlayPauseButton.Content="Pause";

            Title=$"VLC IPTV — {channel.Name}";
        }
        catch ( Exception ex )
        {
            MessageBox.Show(
                this,
                $"The stream could not be started.\n\n{ex.Message}",
                "Playback Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void PlayPauseButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        if ( _vlc is null )
            return;

        var player = _vlc.MediaPlayer;

        if ( player.IsPlaying )
        {
            _vlc.Pause();
            PlayPauseButton.Content="Play";
        }
        else if ( player.Media is not null )
        {
            player.Play();
            PlayPauseButton.Content="Pause";
        }
    }

    private void StopButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        if ( _vlc is null )
            return;

        _vlc.Stop();

        PlayPauseButton.Content="Play";
        PlaybackTimeText.Text="--:-- / --:--";
        PlaybackProgressBar.Value=0;
    }

    private void MuteButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        if ( _vlc is null )
            return;

        var player = _vlc.MediaPlayer;

        if ( !_isMuted )
        {
            _volumeBeforeMute=player.Volume;
            player.Volume=0;

            _isMuted=true;
            MuteButton.Content="Unmute";
        }
        else
        {
            player.Volume=
                Math.Clamp(_volumeBeforeMute, 0, 100);

            VolumeSlider.Value=player.Volume;

            _isMuted=false;
            MuteButton.Content="Mute";
        }
    }

    private void VolumeSlider_ValueChanged (
        object sender,
        RoutedPropertyChangedEventArgs<double> e )
    {
        if ( _vlc is null )
            return;

        var volume = ( int ) Math.Round(e.NewValue);

        _vlc.MediaPlayer.Volume=
            Math.Clamp(volume, 0, 100);

        if ( _isMuted&&volume>0 )
        {
            _isMuted=false;
            MuteButton.Content="Mute";
        }
    }

    private void FullscreenButton_Click (
        object sender,
        RoutedEventArgs e )
    {
        if ( _fullscreenWindow is not null )
        {
            ExitFullscreen();
            return;
        }

        EnterFullscreen();
    }

    private void EnterFullscreen ()
    {
        if ( _vlc is null||
            _fullscreenWindow is not null )
            return;

        var streamUrl = _vlc.CurrentStreamUrl;

        if ( string.IsNullOrWhiteSpace(streamUrl) )
            return;

        try
        {
            _vlc.Stop();

            _fullscreenWindow=
                new FullscreenWindow(streamUrl);

            _fullscreenWindow.Closed+=
                FullscreenWindow_Closed;

            _fullscreenWindow.Show();

            FullscreenButton.Content="Exit Fullscreen";
        }
        catch ( Exception ex )
        {
            if ( _fullscreenWindow is not null )
            {
                _fullscreenWindow.Closed-=
                    FullscreenWindow_Closed;

                _fullscreenWindow=null;
            }

            MessageBox.Show(
                this,
                $"Fullscreen could not be started.\n\n{ex.Message}",
                "Fullscreen Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            RestoreMainPlayback();
        }
    }

    private void ExitFullscreen ()
    {
        if ( _fullscreenWindow is null )
            return;

        var window = _fullscreenWindow;

        _fullscreenWindow=null;

        window.Closed-=
            FullscreenWindow_Closed;

        window.Close();

        FullscreenButton.Content="Fullscreen";

        RestoreMainPlayback();
    }

    private void FullscreenWindow_Closed (
        object? sender,
        EventArgs e )
    {
        if ( _fullscreenWindow is not null )
        {
            _fullscreenWindow.Closed-=
                FullscreenWindow_Closed;

            _fullscreenWindow=null;
        }

        FullscreenButton.Content="Fullscreen";

        RestoreMainPlayback();
    }

    private void RestoreMainPlayback ()
    {
        if ( _vlc is null )
            return;

        var streamUrl = _vlc.CurrentStreamUrl;

        if ( string.IsNullOrWhiteSpace(streamUrl) )
            return;

        try
        {
            _vlc.Play(streamUrl);

            PlayerEmptyText.Visibility=
                Visibility.Collapsed;

            PlayPauseButton.Content="Pause";
        }
        catch
        {
            PlayerEmptyText.Visibility=
                Visibility.Visible;

            PlayPauseButton.Content="Play";
        }
    }

    private void MainWindow_Closed (
        object? sender,
        EventArgs e )
    {
        _uiTimer.Stop();
        _uiTimer.Tick-=UiTimer_Tick;

        if ( _fullscreenWindow is not null )
        {
            _fullscreenWindow.Closed-=
                FullscreenWindow_Closed;

            _fullscreenWindow.Close();

            _fullscreenWindow=null;
        }

        if ( _vlc is not null )
        {
            _vlc.Dispose();
            _vlc=null;
        }
    }
}