using System.Windows;
using System.Windows.Input;
using LibVLCSharp.Shared;

namespace NexusIPTV.App;

public partial class FullscreenWindow : Window
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;
    private readonly string _streamUrl;

    public bool PlaybackStarted
    {
        get; private set;
    }

    public FullscreenWindow ( string streamUrl )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamUrl);

        InitializeComponent();

        _streamUrl=streamUrl;

        LibVLCSharp.Shared.Core.Initialize();

        _libVlc=new LibVLC(
            "--no-video-title-show",
            "--network-caching=1500");

        _mediaPlayer=new MediaPlayer(_libVlc);

        FullscreenVideoView.MediaPlayer=_mediaPlayer;

        Loaded+=FullscreenWindow_Loaded;
        Closed+=FullscreenWindow_Closed;
    }

    private void FullscreenWindow_Loaded (
        object sender,
        RoutedEventArgs e )
    {
        try
        {
            WindowStyle=WindowStyle.None;
            ResizeMode=ResizeMode.NoResize;
            WindowState=WindowState.Maximized;
            ShowInTaskbar=false;
            Topmost=true;

            Activate();
            Focus();
            Keyboard.Focus(this);

            using var media = new Media(
                _libVlc,
                new Uri(_streamUrl));

            PlaybackStarted=_mediaPlayer.Play(media);
        }
        catch
        {
            PlaybackStarted=false;
            Close();
        }
    }

    private void FullscreenWindow_KeyDown (
        object sender,
        System.Windows.Input.KeyEventArgs e )
    {
        if ( e.Key==Key.Escape )
        {
            e.Handled=true;
            Close();
        }
    }

    private void FullscreenWindow_Closed (
        object? sender,
        EventArgs e )
    {
        FullscreenVideoView.MediaPlayer=null;

        try
        {
            _mediaPlayer.Stop();
        }
        catch
        {
        }

        _mediaPlayer.Dispose();
        _libVlc.Dispose();
    }
}