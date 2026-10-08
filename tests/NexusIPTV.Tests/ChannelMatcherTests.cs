using NexusIPTV.Core.M3U;
using NexusIPTV.Core.Matching;
using NexusIPTV.Core.XMLTV;

namespace NexusIPTV.Tests;

public class ChannelMatcherTests
{
    [Fact]
    public void Match_ExactId_AttachesProgrammes()
    {
        const string m3u = """
            #EXTM3U
            #EXTINF:-1 tvg-id="example",Example Channel
            https://example.com/example.m3u8
            """;

        const string xml = """
            <tv>
              <channel id="example">
                <display-name>Example Channel</display-name>
              </channel>
              <programme
                start="20261006100000 +0000"
                stop="20261006103000 +0000"
                channel="example">
                <title>Test Programme</title>
              </programme>
            </tv>
            """;

        var playlist = M3UParser.Parse(m3u);
        var programmes = XmlTvParser.Parse(xml);
        var xmlChannels = XmlTvParser.ParseChannels(xml);

        var channels =
            ChannelMatcher.Match(
                playlist,
                programmes,
                xmlChannels);

        var channel = Assert.Single(channels);
        var programme = Assert.Single(channel.Programmes);

        Assert.Equal("Example Channel", channel.Name);
        Assert.Equal("Test Programme", programme.Title);
    }

    [Fact]
    public void Match_NameFallback_AttachesProgrammes()
    {
        const string m3u = """
            #EXTM3U
            #EXTINF:-1 tvg-id="playlist-id",ANIME x HIDIVE
            https://example.com/anime.m3u8
            """;

        const string xml = """
            <tv>
              <channel id="xmltv-id">
                <display-name>ANIME x HIDIVE</display-name>
              </channel>
              <programme
                start="20261006132900 +0000"
                stop="20261006135800 +0000"
                channel="xmltv-id">
                <title>Dream Eater Merry</title>
              </programme>
            </tv>
            """;

        var playlist = M3UParser.Parse(m3u);
        var programmes = XmlTvParser.Parse(xml);
        var xmlChannels = XmlTvParser.ParseChannels(xml);

        var channels =
            ChannelMatcher.Match(
                playlist,
                programmes,
                xmlChannels);

        var channel = Assert.Single(channels);

        Assert.Equal(
            "Dream Eater Merry",
            Assert.Single(channel.Programmes).Title);
    }

    [Fact]
    public void Match_UnmatchedChannel_RemainsUsable()
    {
        const string m3u = """
            #EXTM3U
            #EXTINF:-1 tvg-id="no-guide",No Guide Channel
            https://example.com/no-guide.m3u8
            """;

        var playlist = M3UParser.Parse(m3u);

        var channels =
            ChannelMatcher.Match(
                playlist,
                [],
                new Dictionary<string, string>());

        var channel = Assert.Single(channels);

        Assert.Equal(
            "No Guide Channel",
            channel.Name);

        Assert.Equal(
            "https://example.com/no-guide.m3u8",
            channel.StreamUrl);

        Assert.Empty(channel.Programmes);
    }

    [Fact]
    public void Match_RealFiles_CreatesChannels()
    {
        var root = FindProjectRoot();

        var m3uPath =
            Path.Combine(root, "samples", "us.m3u");

        var xmlPath =
            Path.Combine(root, "samples", "guide.xml");

        var playlist =
            M3UParser.ParseFile(m3uPath);

        var programmes =
            XmlTvParser.ParseFile(xmlPath);

        var xmlChannels =
            XmlTvParser.ParseChannelsFile(xmlPath);

        var channels =
            ChannelMatcher.Match(
                playlist,
                programmes,
                xmlChannels);

        Assert.NotEmpty(channels);

        Assert.Equal(
            playlist.Count,
            channels.Count);
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
