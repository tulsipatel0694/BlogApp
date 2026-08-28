using System.Globalization;

namespace BlogApp.Web;

/// <summary>Presentation-only helpers used by the Razor views.</summary>
public static class Format
{
    /// <summary>
    /// "just now" / "3 hours ago" for fresh items, an absolute date for older
    /// ones — relative time stops being useful after about a week.
    /// </summary>
    public static string Friendly(DateTimeOffset value) => Friendly(value, DateTimeOffset.UtcNow);

    /// <summary>Overload taking "now" explicitly, which keeps the rules testable.</summary>
    public static string Friendly(DateTimeOffset value, DateTimeOffset now)
    {
        var elapsed = now - value;

        return elapsed switch
        {
            { TotalSeconds: < 0 } => Absolute(value),
            { TotalMinutes: < 1 } => "just now",
            { TotalMinutes: < 60 } => Plural((int)elapsed.TotalMinutes, "minute"),
            { TotalHours: < 24 } => Plural((int)elapsed.TotalHours, "hour"),
            { TotalDays: < 7 } => Plural((int)elapsed.TotalDays, "day"),
            _ => Absolute(value)
        };
    }

    /// <summary>Splits a body on blank lines so the view can render real paragraphs.</summary>
    public static IEnumerable<string> Paragraphs(string body) =>
        body.ReplaceLineEndings("\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .DefaultIfEmpty(body.Trim());

    private static string Absolute(DateTimeOffset value) =>
        value.ToLocalTime().ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    private static string Plural(int count, string unit) =>
        count == 1 ? $"1 {unit} ago" : $"{count} {unit}s ago";
}
