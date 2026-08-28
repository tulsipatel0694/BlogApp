using BlogApp.Domain.Abstractions;
using BlogApp.Domain.Common;
using BlogApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Web.Pages;

/// <summary>
/// The whole application lives on this one page: the entry list (section 1),
/// the "write an entry" form and the comment thread of whichever entry is open
/// (section 2). Every write follows post/redirect/get so a refresh never
/// re-submits.
/// </summary>
public sealed class IndexModel(IBlogService blog, ILogger<IndexModel> logger) : PageModel
{
    private const int DefaultPageSize = 5;
    private const int ExpandedPageSize = 20;

    [BindProperty]
    public NewPostInput NewPost { get; set; } = new();

    [BindProperty]
    public NewCommentInput NewComment { get; set; } = new();

    /// <summary>Id of the entry whose full text and comments are expanded.</summary>
    [BindProperty(SupportsGet = true, Name = "post")]
    public int? OpenPostId { get; set; }

    /// <summary>Set when the reader asked to see more than the newest five.</summary>
    [BindProperty(SupportsGet = true, Name = "all")]
    public bool ShowAll { get; set; }

    public IReadOnlyList<PostSummary> RecentPosts { get; private set; } = [];

    public PostDetail? OpenPost { get; private set; }

    public int TotalPosts { get; private set; }

    public int PageSize => ShowAll ? ExpandedPageSize : DefaultPageSize;

    public bool HasMoreToShow => !ShowAll && TotalPosts > DefaultPageSize;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);

        // A stale bookmark to a deleted entry should still render the page.
        if (OpenPostId is not null && OpenPost is null)
            return RedirectToPage();

        return Page();
    }

    public async Task<IActionResult> OnPostCreateEntryAsync(CancellationToken ct)
    {
        // Only the compose form was submitted; discard the comment form's state.
        KeepOnlyForm(nameof(NewPost));
        NewComment = new NewCommentInput();

        if (!ModelState.IsValid)
            return await ReloadWithErrorsAsync(ct);

        try
        {
            var id = await blog.CreatePostAsync(NewPost.Title, NewPost.Author, NewPost.Body, ct);

            StatusMessage = "Your entry is published.";
            return RedirectToPage(new { post = id });
        }
        catch (DomainValidationException ex)
        {
            // Belt and braces: the domain is the final authority on validity.
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReloadWithErrorsAsync(ct);
        }
    }

    public async Task<IActionResult> OnPostAddCommentAsync(CancellationToken ct)
    {
        KeepOnlyForm(nameof(NewComment));
        NewPost = new NewPostInput();

        OpenPostId = NewComment.PostId;

        if (!ModelState.IsValid)
            return await ReloadWithErrorsAsync(ct);

        try
        {
            var added = await blog.AddCommentAsync(
                NewComment.PostId, NewComment.Author, NewComment.Body, ct);

            if (!added)
            {
                logger.LogWarning("Comment posted against missing entry {PostId}.", NewComment.PostId);
                StatusMessage = "That entry no longer exists.";
                return RedirectToPage();
            }

            StatusMessage = "Comment added.";
            return RedirectToPage(new { post = NewComment.PostId });
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReloadWithErrorsAsync(ct);
        }
    }

    /// <summary>
    /// Both forms live on the same page, so both models bind on every submit —
    /// and the one that was not submitted binds at the empty prefix, producing
    /// spurious "required" errors. Keeping only the submitted form's entries
    /// leaves ModelState describing exactly what the user sent.
    /// </summary>
    private void KeepOnlyForm(string prefix)
    {
        var unrelated = ModelState.Keys
            .Where(key => !key.StartsWith(prefix + ".", StringComparison.Ordinal))
            .ToList();

        foreach (var key in unrelated)
            ModelState.Remove(key);
    }

    private async Task<IActionResult> ReloadWithErrorsAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        return Page();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        TotalPosts = await blog.CountPostsAsync(ct);
        RecentPosts = await blog.GetRecentPostsAsync(PageSize, ct);

        if (OpenPostId is > 0)
            OpenPost = await blog.GetPostAsync(OpenPostId.Value, ct);
    }
}
