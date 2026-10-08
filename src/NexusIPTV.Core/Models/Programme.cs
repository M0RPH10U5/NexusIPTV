namespace NexusIPTV.Core.Models;

public sealed class Programme
{
    public string ChannelId { get; init; } = string.Empty;

    public DateTimeOffset Start { get; init; }

    public DateTimeOffset Stop { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Language { get; init; }

    public bool IsCurrentlyAiring(DateTimeOffset instant)
        => Start <= instant && instant < Stop;
}
