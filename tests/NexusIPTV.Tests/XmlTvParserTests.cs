using NexusIPTV.Core.XMLTV;

namespace NexusIPTV.Tests;

public class XmlTvParserTests
{
    [Fact]
    public void Parse_ReadsProgramme()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tv>
              <channel id="example">
                <display-name>Example Channel</display-name>
              </channel>
              <programme
                start="20261006132900 +0000"
                stop="20261006135800 +0000"
                channel="example">
                <title>Dream Eater Merry</title>
                <desc>Yumeji dreams that he is being pursued.</desc>
              </programme>
            </tv>
            """;

        var programmes = XmlTvParser.Parse(xml);

        var programme = Assert.Single(programmes);

        Assert.Equal("example", programme.ChannelId);
        Assert.Equal("Dream Eater Merry", programme.Title);
        Assert.Equal(
            "Yumeji dreams that he is being pursued.",
            programme.Description);

        Assert.Equal(
            new DateTimeOffset(
                2026, 10, 6, 13, 29, 0,
                TimeSpan.Zero),
            programme.Start);

        Assert.Equal(
            new DateTimeOffset(
                2026, 10, 6, 13, 58, 0,
                TimeSpan.Zero),
            programme.Stop);
    }

    [Fact]
    public void ParseChannels_ReadsChannelNames()
    {
        const string xml = """
            <tv>
              <channel id="example">
                <display-name>Example Channel</display-name>
              </channel>
              <channel id="second">
                <display-name>Second Channel</display-name>
              </channel>
            </tv>
            """;

        var channels = XmlTvParser.ParseChannels(xml);

        Assert.Equal(2, channels.Count);
        Assert.Equal("Example Channel", channels["example"]);
        Assert.Equal("Second Channel", channels["second"]);
    }

    [Fact]
    public void ParseFile_ReadsRealGuide()
    {
        var root = FindProjectRoot();

        var path = Path.Combine(
            root,
            "samples",
            "guide.xml");

        Assert.True(
            File.Exists(path),
            $"Sample guide was not found: {path}");

        var programmes = XmlTvParser.ParseFile(path);

        Assert.NotEmpty(programmes);

        var anime = programmes
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
    public void ParseFile_ReadsAnimeChannel()
    {
        var root = FindProjectRoot();

        var path = Path.Combine(
            root,
            "samples",
            "guide.xml");

        var channels =
            XmlTvParser.ParseChannelsFile(path);

        Assert.True(
            channels.TryGetValue(
                "PLEX1#plex.tv.ANIME.x.HIDIVE.plex",
                out var name));

        Assert.Equal(
            "ANIME x HIDIVE",
            name);
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
