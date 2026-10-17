using JokesWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JokesWebApp.Data
{
    /// <summary>
    /// Adds a starter set of jokes and riddles so the site never looks empty.
    /// Safe to run on every start-up: an item is only added if a joke with the
    /// same question isn't already in the database.
    /// </summary>
    public static class SeedData
    {
        public const string TeamName = "JokesWebApp Team";

        private static readonly (JokeCategory Category, string Question, string Answer)[] Starters =
        {
            // ---------- Riddles ----------
            (JokeCategory.Riddle, "What goes up and comes down but never moves?", "A staircase."),
            (JokeCategory.Riddle, "What goes up but never comes down?", "Your age."),
            (JokeCategory.Riddle, "What has keys but can't open a single lock?", "A piano."),
            (JokeCategory.Riddle, "What has to be broken before you can use it?", "An egg."),
            (JokeCategory.Riddle, "What has hands but can't clap?", "A clock."),
            (JokeCategory.Riddle, "What gets wetter the more it dries?", "A towel."),
            (JokeCategory.Riddle, "I'm tall when I'm young and short when I'm old. What am I?", "A candle."),
            (JokeCategory.Riddle, "What has a neck but no head?", "A bottle."),
            (JokeCategory.Riddle, "What can you catch but never throw?", "A cold."),
            (JokeCategory.Riddle, "What has lots of teeth but can't bite?", "A comb."),
            (JokeCategory.Riddle, "Some months have 31 days, others have 30. How many have 28?", "All of them."),
            (JokeCategory.Riddle, "The more you take, the more you leave behind. What are they?", "Footsteps."),
            (JokeCategory.Riddle, "What has one eye but can't see?", "A needle."),
            (JokeCategory.Riddle, "What can travel around the world while staying in one corner?", "A stamp."),
            (JokeCategory.Riddle, "What runs but never walks, and has a mouth but never talks?", "A river."),
            (JokeCategory.Riddle, "If you have me, you want to share me. If you share me, you no longer have me. What am I?", "A secret."),

            // ---------- Jokes ----------
            (JokeCategory.Joke, "Why don't skeletons fight each other?", "They don't have the guts."),
            (JokeCategory.Joke, "Why did the scarecrow win an award?", "Because he was outstanding in his field."),
            (JokeCategory.Joke, "What do you call a fake noodle?", "An impasta."),
            (JokeCategory.Joke, "Why can't a bicycle stand up by itself?", "It's two-tired."),
            (JokeCategory.Joke, "Why did the maths book look so sad?", "It had too many problems."),
            (JokeCategory.Joke, "What do you call a bear with no teeth?", "A gummy bear."),
            (JokeCategory.Joke, "Why don't eggs tell jokes?", "They'd crack each other up."),
            (JokeCategory.Joke, "How does a penguin build its house?", "Igloos it together."),
            (JokeCategory.Joke, "Why did the golfer bring two pairs of trousers?", "In case he got a hole in one."),
            (JokeCategory.Joke, "What do you call cheese that isn't yours?", "Nacho cheese."),
            (JokeCategory.Joke, "Why was the computer cold?", "It left its Windows open."),
            (JokeCategory.Joke, "Why did the cookie go to the doctor?", "Because it felt crummy."),
            (JokeCategory.Joke, "What did the ocean say to the beach?", "Nothing, it just waved."),
            (JokeCategory.Joke, "Why do programmers prefer dark mode?", "Because light attracts bugs."),
            (JokeCategory.Joke, "What do you call a sleeping dinosaur?", "A dino-snore."),
            (JokeCategory.Joke, "Why did the tomato blush?", "Because it saw the salad dressing."),
        };

        public static async Task EnsureSeededAsync(ApplicationDbContext db)
        {
            var existing = await db.Joke.Select(j => j.JokeQuestion).ToListAsync();
            var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

            var start = DateTime.UtcNow.AddDays(-Starters.Length);
            var added = 0;
            for (var i = 0; i < Starters.Length; i++)
            {
                var (category, question, answer) = Starters[i];
                if (existingSet.Contains(question)) continue;

                db.Joke.Add(new Joke
                {
                    Category = category,
                    JokeQuestion = question,
                    JokeAnswer = answer,
                    AuthorName = TeamName,
                    CreatedAt = start.AddDays(i),
                });
                added++;
            }

            if (added > 0)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}
