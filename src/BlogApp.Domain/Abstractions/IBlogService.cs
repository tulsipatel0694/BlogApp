namespace BlogApp.Domain.Abstractions;

/// <summary>
/// The single seam the web layer talks to. Keeping it here means the UI depends
/// on the domain, never on Entity Framework.
/// </summary>
public interface IBlogService
{
    /// <summary>Most recent entries first, newest <paramref name="count"/> only.</summary>
    Task<IReadOnlyList<PostSummary>> GetRecentPostsAsync(int count, CancellationToken ct = default);

    Task<PostDetail?> GetPostAsync(int postId, CancellationToken ct = default);

    Task<int> CreatePostAsync(string title, string author, string body, CancellationToken ct = default);

    /// <summary>Returns false when the post no longer exists.</summary>
    Task<bool> AddCommentAsync(int postId, string author, string body, CancellationToken ct = default);

    Task<int> CountPostsAsync(CancellationToken ct = default);
}
