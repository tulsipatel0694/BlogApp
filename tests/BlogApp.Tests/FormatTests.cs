using BlogApp.Web;

namespace BlogApp.Tests;

public class FormatTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 10, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, "just now")]
    [InlineData(1, "1 minute ago")]
    [InlineData(45, "45 minutes ago")]
    [InlineData(60, "1 hour ago")]
    [InlineData(60 * 5, "5 hours ago")]
    [InlineData(60 * 24, "1 day ago")]
    [InlineData(60 * 24 * 3, "3 days ago")]
    public void Friendly_uses_relative_time_for_recent_items(int minutesAgo, string expected)
    {
        Assert.Equal(expected, Format.Friendly(Now.AddMinutes(-minutesAgo), Now));
    }

    [Fact]
    public void Friendly_falls_back_to_an_absolute_date_after_a_week()
    {
        Assert.Contains("2026", Format.Friendly(Now.AddDays(-30), Now));
    }

    [Fact]
    public void Paragraphs_splits_on_blank_lines()
    {
        Assert.Equal(["One.", "Two."], Format.Paragraphs("One.\n\nTwo."));
    }

    [Fact]
    public void Paragraphs_returns_the_whole_body_when_there_are_no_breaks()
    {
        Assert.Equal(["Single paragraph."], Format.Paragraphs("Single paragraph."));
    }
}
