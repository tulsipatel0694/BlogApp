using BlogApp.Domain.Common;
using BlogApp.Infrastructure.Persistence.Converters;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Author)
            .IsRequired()
            .HasMaxLength(FieldLimits.AuthorNameMax);

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(FieldLimits.CommentBodyMax);

        builder.Property(c => c.PostedOn)
            .HasConversion(UtcDateTimeOffsetConverter.Instance)
            .IsRequired();

        builder.HasOne(c => c.Post)
            .WithMany(nameof(BlogPost.Comments))
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comments are only ever read one thread at a time, oldest first.
        builder.HasIndex(c => new { c.BlogPostId, c.PostedOn })
            .HasDatabaseName("IX_Comments_BlogPostId_PostedOn");
    }
}
