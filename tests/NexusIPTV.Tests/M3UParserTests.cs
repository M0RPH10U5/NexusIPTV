using NexusIPTV.Core.M3U;

namespace NexusIPTV.Tests;

public class M3UParserTests
{
    [Fact]
    public void Parse_ReadsChannelMetadataAndStreamUrl()
    {
        const string m3u = """
            #EXTM3U
            #EXTINF:-1 tvg-id="Example.us@HD" tvg-logo="https://example.com/logo.png" group-title="News;HD",Example News
            https://example.com/live/example.m3u8
            """;

        var entries = M3UParser.Parse(m3u);

        var entry = Assert.Single(entries);

        Assert.Equal("Example.us@HD", entry.TvgId);
        Assert.Equal("Example News", entry.Name);
        Assert.Equal("https://example.com/logo.png", entry.LogoUrl);
        Assert.Equal("https://example.com/live/example.m3u8", entry.StreamUrl);

        Assert.Equal(
            ["News", "HD"],
            entry.Groups);
    }

    [Fact]
    public void Parse_HandlesMultipleChannels()
    {
        const string m3u = """
            #EXTM3U
            #EXTINF:-1 tvg-id="Channel1",Channel One
            https://example.com/one.m3u8
            #EXTINF:-1 tvg-id="Channel2",Channel Two
            https://example.com/two.m3u8
            """;

        var entries = M3UParser.Parse(m3u);

        Assert.Equal(2, entries.Count);

        Assert.Equal("Channel1", entries[0].TvgId);
        Assert.Equal("Channel One", entries[0].Name);

        Assert.Equal("Channel2", entries[1].TvgId);
        Assert.Equal("Channel Two", entries[1].Name);
    }

    [Fact]
    public void ParseFile_ReadsRealSamplePlaylist()
    {
        var root = FindProjectRoot();

        var path = Path.Combine(
            root,
            "samples",
            "us.m3u");

        Assert.True(
            File.Exists(path),
            $"Sample playlist was not found: {path}");

        var entries = M3UParser.ParseFile(path);

        Assert.NotEmpty(entries);

        Assert.Contains(
            entries,
            entry => entry.Name == "00s Replay");

        Assert.Contains(
            entries,
            entry => entry.TvgId == "3ABNEnglish.us@SD");
    }

    private static string FindProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "NexusIPTV.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the NexusIPTV project root.");
    }
}
