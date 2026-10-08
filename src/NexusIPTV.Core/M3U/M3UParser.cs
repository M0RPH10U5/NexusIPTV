using System.Text.RegularExpressions;
using NexusIPTV.Core.Models;

namespace NexusIPTV.Core.M3U;

public static class M3UParser
{
    private static readonly Regex AttributeRegex =
        new(
            @"(?<key>[\w-]+)=(?:""(?<quoted>[^""]*)""|(?<unquoted>[^\s]+))",
            RegexOptions.Compiled);

    public static IReadOnlyList<PlaylistEntry> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var entries = new List<PlaylistEntry>();
        var lines = content
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (!line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                continue;

            var attributes = ParseAttributes(line);

            var commaIndex = line.IndexOf(',');

            var name = commaIndex >= 0
                ? line[(commaIndex + 1)..].Trim()
                : string.Empty;

            // The stream URL is normally the next non-empty, non-comment line.
            string? streamUrl = null;

            for (var j = i + 1; j < lines.Length; j++)
            {
                var candidate = lines[j].Trim();

                if (candidate.Length == 0)
                    continue;

                if (candidate.StartsWith('#'))
                    continue;

                streamUrl = candidate;
                i = j;
                break;
            }

            if (string.IsNullOrWhiteSpace(streamUrl))
                continue;

            var tvgId = GetAttribute(attributes, "tvg-id");
            var logoUrl = GetAttribute(attributes, "tvg-logo");
            var groupTitle = GetAttribute(attributes, "group-title");

            var groups = string.IsNullOrWhiteSpace(groupTitle)
                ? Array.Empty<string>()
                : groupTitle
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

            entries.Add(
                new PlaylistEntry
                {
                    TvgId = tvgId ?? string.Empty,
                    Name = name,
                    LogoUrl = logoUrl,
                    Groups = groups,
                    StreamUrl = streamUrl,
                    Attributes = attributes
                });
        }

        return entries;
    }

    public static IReadOnlyList<PlaylistEntry> ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var content = File.ReadAllText(path);
        return Parse(content);
    }

    private static Dictionary<string, string> ParseAttributes(string line)
    {
        var attributes =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        var commaIndex = line.IndexOf(',');

        var attributePart = commaIndex >= 0
            ? line[..commaIndex]
            : line;

        foreach (Match match in AttributeRegex.Matches(attributePart))
        {
            var key = match.Groups["key"].Value;

            var value = match.Groups["quoted"].Success
                ? match.Groups["quoted"].Value
                : match.Groups["unquoted"].Value;

            attributes[key] = value;
        }

        return attributes;
    }

    private static string? GetAttribute(
        IReadOnlyDictionary<string, string> attributes,
        string key)
    {
        return attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }
}
