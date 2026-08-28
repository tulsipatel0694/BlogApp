using BlogApp.Domain.Abstractions;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Infrastructure.Services;

/// <summary>
/// Entity Framework implementation of <see cref="IBlogService"/>. Reads are
/// projected and no-tracking; writes go through the domain factories so the
/// invariants are enforced in exactly one place.
/// </summary>
public sealed class BlogService(BlogDbContext db, IClock clock) : IBlogService
{
    public async Task<IReadOnlyList<PostSummary>> GetRecentPostsAsync(int count, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var posts = await db.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedOn)
            .ThenByDescending(p => p.Id)
            .Take(count)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Author,
                p.Body,
                p.PublishedOn,
                CommentCount = p.Comments.Count
            })
            .ToListAsync(ct);

        return posts
            .Select(p => new PostSummary(
                p.Id,
                p.Title,
                p.Author,
                p.PublishedOn,
                Summarise(p.Body),
                p.CommentCount))
            .ToList();
    }

    public async Task<PostDetail?> GetPostAsync(int postId, CancellationToken ct = default)
    {
        var post = await db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Author,
                p.Body,
                p.PublishedOn,
                Comments = p.Comments
                    .OrderBy(c => c.PostedOn)
                    .ThenBy(c => c.Id)
                    .Select(c => new CommentView(c.Id, c.Author, c.Body, c.PostedOn))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return post is null
            ? null
            : new PostDetail(post.Id, post.Title, post.Author, post.Body, post.PublishedOn, post.Comments);
    }

    public async Task<int> CreatePostAsync(string title, string author, string body, CancellationToken ct = default)
    {
        var post = BlogPost.Create(title, author, body, clock.UtcNow);

        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);

        return post.Id;
    }

    public async Task<bool> AddCommentAsync(int postId, string author, string body, CancellationToken ct = default)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == postId, ct);
        if (post is null)
            return false;

        post.AddComment(author, body, clock.UtcNow);
        await db.SaveChangesAsync(ct);

        return true;
    }

    public Task<int> CountPostsAsync(CancellationToken ct = default) => db.Posts.CountAsync(ct);

    /// <summary>Trims a body to a clean word boundary for the list view.</summary>
    private static string Summarise(string body, int maxLength = 220)
    {
        var normalised = body.ReplaceLineEndings(" ").Trim();
        if (normalised.Length <= maxLength)
            return normalised;

        var cut = normalised.LastIndexOf(' ', maxLength - 1);
        if (cut <= 0)
            cut = maxLength - 1;

        return normalised[..cut].TrimEnd(' ', ',', '.', ';', ':') + "…";
    }
}
