namespace BlogApp.Domain.Abstractions;

/// <summary>Row shown in the "recent entries" list — projected, never lazily loaded.</summary>
public sealed record PostSummary(
    int Id,
    string Title,
    string Author,
    DateTimeOffset PublishedOn,
    string Excerpt,
    int CommentCount);

/// <summary>A single comment as rendered under an open post.</summary>
public sealed record CommentView(
    int Id,
    string Author,
    string Body,
    DateTimeOffset PostedOn);

/// <summary>A post together with its comment thread.</summary>
public sealed record PostDetail(
    int Id,
    string Title,
    string Author,
    string Body,
    DateTimeOffset PublishedOn,
    IReadOnlyList<CommentView> Comments);
