CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "BlogPosts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BlogPosts" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Author" TEXT NOT NULL,
    "Body" TEXT NOT NULL,
    "PublishedOn" TEXT NOT NULL
);

CREATE TABLE "Comments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Comments" PRIMARY KEY AUTOINCREMENT,
    "BlogPostId" INTEGER NOT NULL,
    "Author" TEXT NOT NULL,
    "Body" TEXT NOT NULL,
    "PostedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Comments_BlogPosts_BlogPostId" FOREIGN KEY ("BlogPostId") REFERENCES "BlogPosts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BlogPosts_PublishedOn" ON "BlogPosts" ("PublishedOn");

CREATE INDEX "IX_Comments_BlogPostId_PostedOn" ON "Comments" ("BlogPostId", "PostedOn");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260827181040_InitialSchema', '10.0.11');

COMMIT;

