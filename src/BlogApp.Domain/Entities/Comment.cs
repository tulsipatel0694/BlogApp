using BlogApp.Domain.Common;

namespace BlogApp.Domain.Entities;

/// <summary>A reader's comment attached to a single <see cref="BlogPost"/>.</summary>
public sealed class Comment
{
    private Comment(int blogPostId, string author, string body, DateTimeOffset postedOn)
    {
        BlogPostId = blogPostId;
        Author = author;
        Body = body;
        PostedOn = postedOn;
    }

    // Required by EF Core's materialiser.
    private Comment()
    {
        Author = string.Empty;
        Body = string.Empty;
    }

    public int Id { get; private set; }

    public int BlogPostId { get; private set; }

    public BlogPost? Post { get; private set; }

    public string Author { get; private set; }

    public string Body { get; private set; }

    /// <summary>The blog comment date.</summary>
    public DateTimeOffset PostedOn { get; private set; }

    internal static Comment Create(BlogPost post, string? author, string? body, DateTimeOffset postedOn) =>
        new(
            post.Id,
            Guard.AgainstNullOrWhiteSpace(author, 1, FieldLimits.AuthorNameMax),
            Guard.AgainstNullOrWhiteSpace(body, FieldLimits.CommentBodyMin, FieldLimits.CommentBodyMax),
            postedOn)
        {
            Post = post
        };
}
