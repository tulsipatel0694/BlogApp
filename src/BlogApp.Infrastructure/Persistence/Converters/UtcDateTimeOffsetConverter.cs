using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogApp.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores timestamps as ISO-8601 UTC text. SQLite cannot sort a native
/// <see cref="DateTimeOffset"/>, but a fixed-width UTC string sorts
/// lexicographically in exactly chronological order — so the "newest entries
/// first" query stays a plain indexed ORDER BY.
/// </summary>
internal sealed class UtcDateTimeOffsetConverter()
    : ValueConverter<DateTimeOffset, string>(
        value => value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture),
        text => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))
{
    public static readonly UtcDateTimeOffsetConverter Instance = new();
}
