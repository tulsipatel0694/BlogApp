# Inkwell — a small blog in ASP.NET Core

A single-page blog: read the latest entries, publish a new one, and comment on
any of them. No accounts, no sign-in — just writing and replies. Data lives in a
relational (SQL) database accessed through Entity Framework Core.

```
┌─ BlogApp.Web ───────────┐   Razor Pages, one page, two forms
│  Pages/Index.cshtml     │   post/redirect/get, antiforgery, validation
└──────────┬──────────────┘
           │ IBlogService (defined in the domain)
┌──────────▼──────────────┐
│  BlogApp.Infrastructure │   EF Core + SQLite, migrations, seeding
└──────────┬──────────────┘
           │
┌──────────▼──────────────┐
│  BlogApp.Domain         │   BlogPost / Comment entities and their rules
└─────────────────────────┘   no framework dependencies at all
```

## Run it

```bash
dotnet run --project src/BlogApp.Web
```

Then open the URL printed in the console (http://localhost:5093 by default).

The database is created and migrated automatically on start-up, and an empty
database is seeded with five sample entries and their comments, so the page has
something to show on first run.

## Test it

```bash
dotnet test
```

30 tests: domain rules, the service against a real in-memory SQLite database,
the date/paragraph formatting helpers, and end-to-end page tests that host the
app and submit both forms the way a browser does (antiforgery token included).

## What is on the page

**Section 1 — existing entries.** The newest five entries, newest first, each
with its author, entry date, an excerpt trimmed at a word boundary, and a
comment count. *Show earlier entries* widens the list; an empty database gets a
purpose-built empty state instead of a blank column.

**Section 2 — writing.** A compose form for a new entry (title, name, body) and,
under whichever entry is open, its full text, its comment thread oldest-first,
and a form to add a comment. Opening an entry is a plain link (`/?post=5`), so
every state of the page is linkable, bookmarkable and back-button friendly.

## Data model

| BlogPosts   |                                          |
|-------------|------------------------------------------|
| Id          | INTEGER, primary key                     |
| Title       | required, ≤ 120 chars                    |
| Author      | required, ≤ 60 chars (a display name)    |
| Body        | required, 10–8000 chars                  |
| PublishedOn | **blog entry date**, ISO-8601 UTC        |

| Comments    |                                                   |
|-------------|---------------------------------------------------|
| Id          | INTEGER, primary key                              |
| BlogPostId  | FK → BlogPosts.Id, `ON DELETE CASCADE`            |
| Author      | required, ≤ 60 chars                              |
| Body        | required, 2–1000 chars                            |
| PostedOn    | **blog comment date**, ISO-8601 UTC               |

Two indexes, one per query the page actually runs:
`IX_BlogPosts_PublishedOn` for "the newest N entries" and
`IX_Comments_BlogPostId_PostedOn` for "this entry's thread, oldest first".

The generated DDL is in [docs/schema.sql](docs/schema.sql); the migration that
produces it is in `src/BlogApp.Infrastructure/Persistence/Migrations`.

## Notes on the design

- **One source of truth for validation.** `FieldLimits` holds every size; the
  EF configuration, the `DataAnnotations` on the view models and the `maxlength`
  attributes in the markup all read those constants, so the browser, the server
  and the schema cannot drift apart. The domain entities re-check the same rules
  in their factory methods — an invalid `BlogPost` cannot be constructed at all.
- **Timestamps.** SQLite cannot sort a native `DateTimeOffset`, so a value
  converter stores them as fixed-width ISO-8601 UTC text, which sorts
  lexicographically in chronological order and keeps the ordering indexed.
- **Reads are projections.** List and thread queries are `AsNoTracking` and
  project straight into read models, so no entity graph is loaded to render a
  page and there is no lazy-loading N+1.
- **Swapping the database.** Nothing above `BlogApp.Infrastructure` knows the
  provider. Moving to SQL Server is changing `UseSqlite` to `UseSqlServer` in
  `DependencyInjection.cs`, the connection string, and regenerating the
  migration. SQLite was chosen so the project runs anywhere with nothing to
  install. (SQLite ignores column length declarations, which is why the lengths
  are also enforced in the model and the domain.)
- **Progressive, script-free UI.** The page works with JavaScript disabled:
  every interaction is a link or a form post.
