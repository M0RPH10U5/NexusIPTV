using NexusIPTV.Core.Epg;

namespace NexusIPTV.Tests;

public class EpgServicesTests
{
    [Fact]
    public void Load_ReadsRealGuide()
    {
        var root = FindProjectRoot();

        var path =
            Path.Combine(
                root,
                "samples",
                "guide.xml");

        Assert.True(
            File.Exists(path),
            $"Sample guide was not found: {path}");

        var result = EpgServices.Load(path);

        Assert.NotEmpty(result.Programmes);
        Assert.NotEmpty(result.Channels);
    }

    [Fact]
    public void Load_RealGuideContainsAnimeChannel()
    {
        var root = FindProjectRoot();

        var path =
            Path.Combine(
                root,
                "samples",
                "guide.xml");

        var result = EpgServices.Load(path);

        Assert.True(
            result.Channels.TryGetValue(
                "PLEX1#plex.tv.ANIME.x.HIDIVE.plex",
                out var channelName));

        Assert.Equal(
            "ANIME x HIDIVE",
            channelName);
    }

    [Fact]
    public void Load_RealGuideContainsAnimeProgrammes()
    {
        var root = FindProjectRoot();

        var path =
            Path.Combine(
                root,
                "samples",
                "guide.xml");

        var result = EpgServices.Load(path);

        var anime =
            result.Programmes
                .Where(x =>
                    x.ChannelId ==
                    "PLEX1#plex.tv.ANIME.x.HIDIVE.plex")
                .ToList();

        Assert.NotEmpty(anime);

        Assert.Contains(
            anime,
            x => x.Title == "Dream Eater Merry");

        Assert.Contains(
            anime,
            x => x.Title == "RahXephon");

        Assert.Contains(
            anime,
            x => x.Title == "Dusk Maiden of Amnesia");
    }

    [Fact]
    public void Load_ParsesProgrammeTimestamps()
    {
        var root = FindProjectRoot();

        var path =
            Path.Combine(
                root,
                "samples",
                "guide.xml");

        var result = EpgServices.Load(path);

        var programme =
            result.Programmes
                .First(x => x.Title == "Dream Eater Merry");

        Assert.True(
            programme.Stop > programme.Start);

        Assert.Equal(
            TimeSpan.Zero,
            programme.Start.Offset);

        Assert.Equal(
            TimeSpan.Zero,
            programme.Stop.Offset);
    }

    private static string FindProjectRoot()
    {
        var directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(
                    Path.Combine(
                        directory.FullName,
                        "NexusIPTV.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the NexusIPTV project root.");
    }
}