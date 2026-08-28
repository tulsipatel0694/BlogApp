using BlogApp.Domain.Abstractions;
using BlogApp.Infrastructure.Persistence;
using BlogApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence and the blog service. SQLite is used so the app
    /// runs anywhere with no server to install; swapping in SQL Server is a
    /// one-line change here because nothing above this layer knows the provider.
    /// </summary>
    public static IServiceCollection AddBlogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BlogDatabase")
            ?? "Data Source=blog.db";

        services.AddDbContext<BlogDbContext>(options => options.UseSqlite(connectionString));

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IBlogService, BlogService>();

        return services;
    }
}
