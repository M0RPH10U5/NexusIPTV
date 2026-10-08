using LibVLCSharp.Shared;

namespace NexusIPTV.Vlc;

public sealed class VlcController : IDisposable
{
    private readonly LibVLC _libVlc;

    public MediaPlayer MediaPlayer
    {
        get;
    }

    public string? CurrentStreamUrl
    {
        get; private set;
    }

    public VlcController ()
    {
        LibVLCSharp.Shared.Core.Initialize();

        _libVlc=new LibVLC(
            "--no-video-title-show",
            "--network-caching=1500");

        MediaPlayer=new MediaPlayer(_libVlc);
    }

    public void Play ( string streamUrl )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamUrl);

        CurrentStreamUrl=streamUrl;

        using var media = new Media(
            _libVlc,
            new Uri(streamUrl));

        MediaPlayer.Play(media);
    }

    public void Stop ()
    {
        MediaPlayer.Stop();
    }

    public void Pause ()
    {
        MediaPlayer.Pause();
    }

    public void Dispose ()
    {
        MediaPlayer.Dispose();
        _libVlc.Dispose();
    }
}