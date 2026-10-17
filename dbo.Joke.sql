-- Reference only: the table is created and updated by the EF Core migrations in Data/Migrations.
CREATE TABLE [dbo].[Joke] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [JokeQuestion] NVARCHAR (MAX) NOT NULL,
    [JokeAnswer]   NVARCHAR (MAX) NOT NULL,
    [Category]     INT            NOT NULL DEFAULT 0,   -- 0 = Joke, 1 = Riddle
    [AuthorId]     NVARCHAR (450) NULL,
    [AuthorName]   NVARCHAR (256) NULL,
    [CreatedAt]    DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [Likes]        INT            NOT NULL DEFAULT 0,
    [Views]        INT            NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Joke] PRIMARY KEY CLUSTERED ([Id] ASC)
);
