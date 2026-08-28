namespace BlogApp.Domain.Common;

/// <summary>
/// Single source of truth for field sizes. The database schema, the domain
/// guards and the UI validation attributes all read these constants so the
/// three layers can never drift apart.
/// </summary>
public static class FieldLimits
{
    public const int PostTitleMax = 120;
    public const int PostBodyMin = 10;
    public const int PostBodyMax = 8000;
    public const int AuthorNameMax = 60;
    public const int CommentBodyMin = 2;
    public const int CommentBodyMax = 1000;
}
