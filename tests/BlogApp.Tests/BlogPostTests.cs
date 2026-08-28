using BlogApp.Domain.Common;
using BlogApp.Domain.Entities;

namespace BlogApp.Tests;

public class BlogPostTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_trims_whitespace_from_every_field()
    {
        var post = BlogPost.Create("  Hello  ", "  Alex ", "  A body long enough to pass.  ", Now);

        Assert.Equal("Hello", post.Title);
        Assert.Equal("Alex", post.Author);
        Assert.Equal("A body long enough to pass.", post.Body);
        Assert.Equal(Now, post.PublishedOn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_missing_title(string? title)
    {
        var ex = Assert.Throws<DomainValidationException>(
            () => BlogPost.Create(title, "Alex", "A body long enough.", Now));

        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_rejects_a_title_over_the_limit()
    {
        var title = new string('x', FieldLimits.PostTitleMax + 1);

        Assert.Throws<DomainValidationException>(
            () => BlogPost.Create(title, "Alex", "A body long enough.", Now));
    }

    [Fact]
    public void Create_rejects_a_body_under_the_minimum()
    {
        Assert.Throws<DomainValidationException>(
            () => BlogPost.Create("Title", "Alex", "short", Now));
    }

    [Fact]
    public void AddComment_appends_to_the_thread()
    {
        var post = BlogPost.Create("Title", "Alex", "A body long enough.", Now);

        post.AddComment("Sam", "Nice one", Now.AddHours(1));
        post.AddComment("Lee", "Agreed", Now.AddHours(2));

        Assert.Equal(2, post.Comments.Count);
        Assert.Collection(post.Comments,
            first => Assert.Equal("Sam", first.Author),
            second => Assert.Equal("Agreed", second.Body));
    }

    [Fact]
    public void AddComment_rejects_an_empty_body()
    {
        var post = BlogPost.Create("Title", "Alex", "A body long enough.", Now);

        Assert.Throws<DomainValidationException>(() => post.AddComment("Sam", " ", Now));
        Assert.Empty(post.Comments);
    }
}
