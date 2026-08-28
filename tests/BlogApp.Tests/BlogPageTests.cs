using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlogApp.Tests;

/// <summary>
/// End-to-end coverage of the single page: the app is hosted for real against a
/// throwaway SQLite file, and the forms are submitted the way a browser would.
/// </summary>
public sealed class BlogPageTests : IClassFixture<BlogAppFactory>
{
    private readonly BlogAppFactory _factory;

    public BlogPageTests(BlogAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Landing_page_lists_the_seeded_entries_and_the_compose_form()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Latest entries", html);
        Assert.Contains("Write an entry", html);
        Assert.Contains("Writing the empty state first", html);
    }

    [Fact]
    public async Task Landing_page_shows_at_most_five_entries_until_asked_for_more()
    {
        var client = _factory.CreateClient();

        var five = CountEntryCards(await client.GetStringAsync("/"));
        var all = CountEntryCards(await client.GetStringAsync("/?all=true"));

        Assert.Equal(5, five);
        Assert.True(all >= five);
    }

    [Fact]
    public async Task Publishing_an_entry_then_commenting_on_it_round_trips()
    {
        var client = NoRedirectClient();

        var created = await client.PostAsync("/?handler=CreateEntry", await FormAsync(client, "/", new()
        {
            ["NewPost.Title"] = "A test entry",
            ["NewPost.Author"] = "Tester",
            ["NewPost.Body"] = "Written by the integration test suite."
        }));

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);

        var postUrl = created.Headers.Location!.ToString();
        var postId = int.Parse(Regex.Match(postUrl, @"post=(\d+)").Groups[1].Value);

        var commented = await client.PostAsync($"/?handler=AddComment&post={postId}",
            await FormAsync(client, postUrl, new()
            {
                ["NewComment.PostId"] = postId.ToString(),
                ["NewComment.Author"] = "Reader",
                ["NewComment.Body"] = "Reading it back."
            }));

        Assert.Equal(HttpStatusCode.Redirect, commented.StatusCode);

        var page = await client.GetStringAsync($"/?post={postId}");
        Assert.Contains("A test entry", page);
        Assert.Contains("Written by the integration test suite.", page);
        Assert.Contains("Reading it back.", page);
    }

    [Fact]
    public async Task An_invalid_entry_is_rejected_and_the_message_is_shown()
    {
        var client = NoRedirectClient();

        var response = await client.PostAsync("/?handler=CreateEntry", await FormAsync(client, "/", new()
        {
            ["NewPost.Title"] = "",
            ["NewPost.Author"] = "Tester",
            ["NewPost.Body"] = "too short"
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Give your entry a title.", html);
        Assert.Contains("Entries run from", html);
    }

    [Fact]
    public async Task A_link_to_a_missing_entry_falls_back_to_the_list()
    {
        var response = await NoRedirectClient().GetAsync("/?post=987654");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    /// <summary>Redirects are the assertion in the write tests, so never follow them.</summary>
    private HttpClient NoRedirectClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static int CountEntryCards(string html) =>
        Regex.Matches(html, "class=\"entry-card").Count;

    /// <summary>Builds a form body carrying the antiforgery token from <paramref name="pageUrl"/>.</summary>
    private static async Task<FormUrlEncodedContent> FormAsync(
        HttpClient client, string pageUrl, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(pageUrl);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

        fields["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(fields);
    }
}
