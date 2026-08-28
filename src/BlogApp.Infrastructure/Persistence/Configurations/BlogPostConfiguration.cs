using BlogApp.Domain.Common;
using BlogApp.Infrastructure.Persistence.Converters;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(FieldLimits.PostTitleMax);

        builder.Property(p => p.Author)
            .IsRequired()
            .HasMaxLength(FieldLimits.AuthorNameMax);

        builder.Property(p => p.Body)
            .IsRequired()
            .HasMaxLength(FieldLimits.PostBodyMax);

        builder.Property(p => p.PublishedOn)
            .HasConversion(UtcDateTimeOffsetConverter.Instance)
            .IsRequired();

        // The landing page always asks for "the newest N entries".
        builder.HasIndex(p => p.PublishedOn)
            .HasDatabaseName("IX_BlogPosts_PublishedOn");

        builder.Metadata
            .FindNavigation(nameof(BlogPost.Comments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
