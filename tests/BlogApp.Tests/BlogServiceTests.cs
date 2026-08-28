using BlogApp.Infrastructure.Services;

namespace BlogApp.Tests;

public class BlogServiceTests : IDisposable
{
    private readonly SqliteFixture _fixture = new();
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 3, 1, 8, 0, 0, TimeSpan.Zero));

    private BlogService NewService() => new(_fixture.NewContext(), _clock);

    [Fact]
    public async Task CreatePostAsync_persists_the_entry_with_its_date()
    {
        var id = await NewService().CreatePostAsync("First light", "Alex", "The body of the entry.");

        var post = await NewService().GetPostAsync(id);

        Assert.NotNull(post);
        Assert.Equal("First light", post.Title);
        Assert.Equal("Alex", post.Author);
        Assert.Equal(_clock.UtcNow, post.PublishedOn);
        Assert.Empty(post.Comments);
    }

    [Fact]
    public async Task GetRecentPostsAsync_returns_the_newest_entries_first_and_honours_the_limit()
    {
        for (var i = 1; i <= 7; i++)
        {
            await NewService().CreatePostAsync($"Entry {i}", "Alex", "A body long enough to store.");
            _clock.Advance(TimeSpan.FromHours(1));
        }

        var recent = await NewService().GetRecentPostsAsync(5);

        Assert.Equal(5, recent.Count);
        Assert.Equal(
            ["Entry 7", "Entry 6", "Entry 5", "Entry 4", "Entry 3"],
            recent.Select(p => p.Title));
        Assert.Equal(7, await NewService().CountPostsAsync());
    }

    [Fact]
    public async Task GetRecentPostsAsync_summarises_long_bodies_on_a_word_boundary()
    {
        var body = string.Join(' ', Enumerable.Repeat("word", 200));
        await NewService().CreatePostAsync("Long", "Alex", body);

        var summary = (await NewService().GetRecentPostsAsync(1)).Single();

        Assert.True(summary.Excerpt.Length <= 221);
        Assert.EndsWith("…", summary.Excerpt);
        Assert.DoesNotContain("wor…", summary.Excerpt);
    }

    [Fact]
    public async Task AddCommentAsync_stores_comments_oldest_first_and_counts_them()
    {
        var id = await NewService().CreatePostAsync("Entry", "Alex", "A body long enough to store.");

        _clock.Advance(TimeSpan.FromMinutes(10));
        Assert.True(await NewService().AddCommentAsync(id, "Sam", "First reply"));

        _clock.Advance(TimeSpan.FromMinutes(10));
        Assert.True(await NewService().AddCommentAsync(id, "Lee", "Second reply"));

        var detail = await NewService().GetPostAsync(id);
        Assert.NotNull(detail);
        Assert.Equal(["First reply", "Second reply"], detail.Comments.Select(c => c.Body));

        var summary = (await NewService().GetRecentPostsAsync(5)).Single();
        Assert.Equal(2, summary.CommentCount);
    }

    [Fact]
    public async Task AddCommentAsync_returns_false_for_an_unknown_entry()
    {
        Assert.False(await NewService().AddCommentAsync(404, "Sam", "Hello?"));
    }

    [Fact]
    public async Task GetPostAsync_returns_null_for_an_unknown_entry()
    {
        Assert.Null(await NewService().GetPostAsync(404));
    }

    [Fact]
    public async Task GetRecentPostsAsync_rejects_a_non_positive_count()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => NewService().GetRecentPostsAsync(0));
    }

    public void Dispose() => _fixture.Dispose();
}
