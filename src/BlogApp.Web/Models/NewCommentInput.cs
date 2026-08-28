using System.ComponentModel.DataAnnotations;
using BlogApp.Domain.Common;

namespace BlogApp.Web.Models;

/// <summary>Bound by the comment form shown under an opened entry.</summary>
public sealed class NewCommentInput
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PostId { get; set; }

    [Display(Name = "Your name")]
    [Required(ErrorMessage = "Tell us who is commenting.")]
    [StringLength(FieldLimits.AuthorNameMax,
        ErrorMessage = "Names are limited to {1} characters.")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Comment")]
    [Required(ErrorMessage = "Write something before posting.")]
    [StringLength(FieldLimits.CommentBodyMax, MinimumLength = FieldLimits.CommentBodyMin,
        ErrorMessage = "Comments run from {2} to {1} characters.")]
    public string Body { get; set; } = string.Empty;
}
