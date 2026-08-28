using System.ComponentModel.DataAnnotations;
using BlogApp.Domain.Common;

namespace BlogApp.Web.Models;

/// <summary>Bound by the "write an entry" form in section 2.</summary>
public sealed class NewPostInput
{
    [Display(Name = "Title")]
    [Required(ErrorMessage = "Give your entry a title.")]
    [StringLength(FieldLimits.PostTitleMax,
        ErrorMessage = "Titles are limited to {1} characters.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Your name")]
    [Required(ErrorMessage = "Tell us who is writing.")]
    [StringLength(FieldLimits.AuthorNameMax,
        ErrorMessage = "Names are limited to {1} characters.")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Entry")]
    [Required(ErrorMessage = "An entry needs some words in it.")]
    [StringLength(FieldLimits.PostBodyMax, MinimumLength = FieldLimits.PostBodyMin,
        ErrorMessage = "Entries run from {2} to {1} characters.")]
    public string Body { get; set; } = string.Empty;
}
