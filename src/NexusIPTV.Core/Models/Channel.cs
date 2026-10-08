using NexusIPTV.Core.Models;

namespace NexusIPTV.Core.Models;

public class Channel
{
    public string TvgId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string StreamUrl { get; set; } = string.Empty;

    public IReadOnlyList<string> Groups { get; set; }
        = Array.Empty<string>();

    public IReadOnlyList<Programme> Programmes { get; set; }
        = Array.Empty<Programme>();

    public bool IsFavorite { get; set; }
    
}
