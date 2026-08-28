using System.Runtime.CompilerServices;

namespace BlogApp.Domain.Common;

/// <summary>Small guard helpers that keep entity constructors readable.</summary>
internal static class Guard
{
    public static string AgainstNullOrWhiteSpace(
        string? value,
        int minLength,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string field = "")
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
            throw new DomainValidationException($"{Humanise(field)} is required.");

        if (trimmed.Length < minLength)
            throw new DomainValidationException(
                $"{Humanise(field)} must be at least {minLength} characters.");

        if (trimmed.Length > maxLength)
            throw new DomainValidationException(
                $"{Humanise(field)} must be {maxLength} characters or fewer.");

        return trimmed;
    }

    private static string Humanise(string field) =>
        string.Concat(field.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? " " + char.ToLowerInvariant(c) : c.ToString()));
}
