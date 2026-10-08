using System.Text.RegularExpressions;
using NexusIPTV.Core.Models;

namespace NexusIPTV.Core.Matching;

public static class ChannelMatcher
{
    public static IReadOnlyList<Channel> Match(
        IReadOnlyList<PlaylistEntry> playlistEntries,
        IReadOnlyList<Programme> programmes,
        IReadOnlyDictionary<string, string>? xmlTvChannels = null)
    {
        ArgumentNullException.ThrowIfNull(playlistEntries);
        ArgumentNullException.ThrowIfNull(programmes);

        var programmesByChannelId =
            programmes
                .GroupBy(
                    x => x.ChannelId,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderBy(p => p.Start)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

        var channels =
            xmlTvChannels is null
                ? new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(
                    xmlTvChannels,
                    StringComparer.OrdinalIgnoreCase);

        var result = new List<Channel>();

        foreach (var entry in playlistEntries)
        {
            var matchedProgrammes =
                FindProgrammes(
                    entry,
                    programmesByChannelId,
                    channels);

            result.Add(
                new Channel
                {
                    TvgId = entry.TvgId,
                    Name = entry.Name,
                    LogoUrl = entry.LogoUrl,
                    StreamUrl = entry.StreamUrl,
                    Groups = entry.Groups,
                    Programmes = matchedProgrammes
                });
        }

        return result;
    }

    private static IReadOnlyList<Programme> FindProgrammes(
        PlaylistEntry entry,
        IReadOnlyDictionary<string, List<Programme>> programmesByChannelId,
        IReadOnlyDictionary<string, string> xmlTvChannels)
    {
        // 1. Exact tvg-id -> XMLTV channel id.
        if (!string.IsNullOrWhiteSpace(entry.TvgId) &&
            programmesByChannelId.TryGetValue(
                entry.TvgId,
                out var exact))
        {
            return exact;
        }

        // 2. Normalized ID comparison.
        var normalizedId = Normalize(entry.TvgId);

        if (!string.IsNullOrWhiteSpace(normalizedId))
        {
            foreach (var pair in programmesByChannelId)
            {
                if (Normalize(pair.Key) == normalizedId)
                    return pair.Value;
            }
        }

        // 3. Match playlist name against XMLTV display name.
        if (!string.IsNullOrWhiteSpace(entry.Name))
        {
            var normalizedName = Normalize(entry.Name);

            foreach (var channel in xmlTvChannels)
            {
                if (Normalize(channel.Value) != normalizedName)
                    continue;

                if (programmesByChannelId.TryGetValue(
                        channel.Key,
                        out var matched))
                {
                    return matched;
                }
            }
        }

        // 4. No EPG is perfectly valid.
        return Array.Empty<Programme>();
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(
                value.Trim().ToLowerInvariant(),
                @"[^a-z0-9]+",
                string.Empty);
    }
}
