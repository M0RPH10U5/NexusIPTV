using System.Globalization;
using System.Xml.Linq;
using NexusIPTV.Core.Models;
using System.Text.RegularExpressions;

namespace NexusIPTV.Core.XMLTV;

public static class XmlTvParser
{
    public static IReadOnlyList<Programme> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var document = XDocument.Parse(content);

        return document
            .Descendants("programme")
            .Select(ParseProgramme)
            .ToList();
    }

    public static IReadOnlyList<Programme> ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Parse(File.ReadAllText(path));
    }

    public static IReadOnlyDictionary<string, string> ParseChannels(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var document = XDocument.Parse(content);

        return document
            .Descendants("channel")
            .Select(channel =>
            {
                var id = (string?)channel.Attribute("id") ?? string.Empty;

                var name =
                    channel
                        .Elements("display-name")
                        .Select(x => x.Value.Trim())
                        .FirstOrDefault()
                    ?? string.Empty;

                return new
                {
                    Id = id,
                    Name = name
                };
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(
                x => x.Id,
                x => x.Name,
                StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string> ParseChannelsFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ParseChannels(File.ReadAllText(path));
    }

    private static Programme ParseProgramme(XElement element)
    {
        var channelId =
            (string?)element.Attribute("channel")
            ?? string.Empty;

        var start =
            ParseXmlTvDate(
                (string?)element.Attribute("start"),
                "start");

        var stop =
            ParseXmlTvDate(
                (string?)element.Attribute("stop"),
                "stop");

        var title =
            element
                .Elements("title")
                .Select(x => x.Value.Trim())
                .FirstOrDefault()
            ?? string.Empty;

        var description =
            element
                .Elements("desc")
                .Select(x => x.Value.Trim())
                .FirstOrDefault();

        return new Programme
        {
            ChannelId = channelId,
            Start = start,
            Stop = stop,
            Title = title,
            Description = description
        };
    }

    private static DateTimeOffset ParseXmlTvDate(
        string? value,
        string attributeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException(
                $"XMLTV programme is missing the '{attributeName}' timestamp.");
        }

        var normalized = value.Trim();

        // XMLTV commonly uses offsets such as:
        //   20261006132900 +0000
        //   20261006132900 -0500
        //
        // Convert the offset to the ISO-style form expected by
        // DateTimeOffset parsing:
        //   20261006132900 -05:00
        normalized = Regex.Replace(
            normalized,
            @"([+-]\d{2})(\d{2})$",
            "$1:$2");

        if (DateTimeOffset.TryParseExact(
                normalized,
                "yyyyMMddHHmmss zzz",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            return result;
        }

        throw new FormatException(
            $"Invalid XMLTV timestamp '{value}'.");
    }
}
