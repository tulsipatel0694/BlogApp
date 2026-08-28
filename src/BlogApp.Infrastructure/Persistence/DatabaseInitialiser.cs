using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogApp.Infrastructure.Persistence;

/// <summary>
/// Applies migrations at start-up and, on a brand new database, seeds a few
/// entries so the landing page has something to show on first run.
/// </summary>
public static class DatabaseInitialiser
{
    public static async Task InitialiseAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitialiser));

        await db.Database.MigrateAsync(ct);

        if (await db.Posts.AnyAsync(ct))
            return;

        logger.LogInformation("Empty database detected — seeding sample blog entries.");

        var now = DateTimeOffset.UtcNow;

        var seed = new (string Title, string Author, string Body, int DaysAgo, (string Author, string Body)[] Comments)[]
        {
            ("Designing a schema you will not regret", "Priya",
                "Start from the questions the page has to answer, not from the tables. This blog only ever asks two things: give me the newest entries, and give me one thread of comments. Two indexes answer both, and everything else stays boring — which is exactly what you want from a schema at 2am.",
                14,
                [("Marcus", "The \"start from the queries\" framing finally made indexing click for me."),
                 ("Ada", "Boring schemas age well. Clever ones age like milk.")]),

            ("Validation belongs in more than one place", "Marcus",
                "Client-side validation is a courtesy, server-side validation is the contract, and database constraints are the last line of defence. Skipping any one of them just moves the failure somewhere less convenient. Here the field limits live in a single constants class that all three layers read.",
                9,
                [("Priya", "Sharing the limits as constants is such a small change for how much drift it prevents.")]),

            ("Entity Framework without the mystery", "Ada",
                "Projections instead of eager loading, no-tracking for reads, and a single SaveChanges per request will carry you a very long way. The trouble usually starts when the ORM is asked to be an architecture rather than a mapper.",
                6,
                [("Lin", "AsNoTracking on read paths is the cheapest performance win in most codebases.")]),

            ("Small apps deserve real layering too", "Lin",
                "A domain project, an infrastructure project and a web project is not ceremony at this size — it is the thing that lets you swap SQLite for SQL Server by editing one line of start-up code, because nothing above the infrastructure layer ever learned which database it was talking to.",
                3,
                []),

            ("Writing the empty state first", "Priya",
                "The first person to open your app sees zero rows. Designing that screen before the happy path forces you to explain what the app is for, and it usually improves the populated view as well.",
                1,
                [("Marcus", "Empty states are the most-read screen nobody designs."),
                 ("Ada", "Guilty. Adding this to my checklist.")])
        };

        foreach (var item in seed)
        {
            var publishedOn = now.AddDays(-item.DaysAgo);
            var post = BlogPost.Create(item.Title, item.Author, item.Body, publishedOn);

            var offset = 1;
            foreach (var comment in item.Comments)
                post.AddComment(comment.Author, comment.Body, publishedOn.AddHours(offset++ * 5));

            db.Posts.Add(post);
        }

        await db.SaveChangesAsync(ct);
    }
}
