using BlogApp.Domain.Common;

namespace BlogApp.Domain.Entities;

/// <summary>
/// A blog entry. State changes go through the factory / methods below so an
/// invalid post can never be handed to the persistence layer.
/// </summary>
public sealed class BlogPost
{
    private readonly List<Comment> _comments = [];

    private BlogPost(string title, string author, string body, DateTimeOffset publishedOn)
    {
        Title = title;
        Author = author;
        Body = body;
        PublishedOn = publishedOn;
    }

    // Required by EF Core's materialiser.
    private BlogPost()
    {
        Title = string.Empty;
        Author = string.Empty;
        Body = string.Empty;
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    /// <summary>Display name typed by the author; there is no user management.</summary>
    public string Author { get; private set; }

    public string Body { get; private set; }

    /// <summary>The blog entry date.</summary>
    public DateTimeOffset PublishedOn { get; private set; }

    public IReadOnlyCollection<Comment> Comments => _comments;

    public static BlogPost Create(string? title, string? author, string? body, DateTimeOffset publishedOn) =>
        new(
            Guard.AgainstNullOrWhiteSpace(title, 1, FieldLimits.PostTitleMax),
            Guard.AgainstNullOrWhiteSpace(author, 1, FieldLimits.AuthorNameMax),
            Guard.AgainstNullOrWhiteSpace(body, FieldLimits.PostBodyMin, FieldLimits.PostBodyMax),
            publishedOn);

    public Comment AddComment(string? author, string? body, DateTimeOffset postedOn)
    {
        var comment = Comment.Create(this, author, body, postedOn);
        _comments.Add(comment);
        return comment;
    }
}
