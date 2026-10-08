using NexusIPTV.Core.Models;
using NexusIPTV.Core.XMLTV;

namespace NexusIPTV.Core.Epg;

public static class EpgServices
{
    public static IReadOnlyList<Programme> LoadProgrammes(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return XmlTvParser.ParseFile(path);
    }

    public static IReadOnlyDictionary<string, string> LoadChannels(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return XmlTvParser.ParseChannelsFile(path);
    }

    public static (
        IReadOnlyList<Programme> Programmes,
        IReadOnlyDictionary<string, string> Channels)
        Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var programmes = XmlTvParser.ParseFile(path);
        var channels = XmlTvParser.ParseChannelsFile(path);

        return (programmes, channels);
    }
}