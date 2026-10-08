namespace NexusIPTV.Core.Models;

public sealed class PlaylistEntry
{
    public string TvgId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public IReadOnlyList<string> Groups { get; init; } = [];

    public string StreamUrl { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Attributes { get; init; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
