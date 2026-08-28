namespace BlogApp.Domain.Abstractions;

/// <summary>Abstracts "now" so entry/comment dates are deterministic under test.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
