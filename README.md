# Andrea's Jokes & Riddles 😜

An ASP.NET Core 8 MVC site where people can read jokes, solve riddles, and share their own.

## Features

- **Jokes and riddles:** every post is either a joke (set-up + punchline) or a riddle (question + answer).
- **32 starter jokes & riddles** (e.g. *"What goes up and comes down but never moves?" Answer: A staircase.*) are added automatically the first time the app starts.
- **Home page** with a live, animated hero (floating emojis that dodge your mouse, click for confetti), a typewriter headline,
  live counters that refresh every 10 seconds, a *Joke of the day* and *Riddle of the day* flip card, the most-laughed-at list and a top-contributors board.
- **Browse** with filters (All / Jokes / Riddles), search across questions, answers and authors, sorting (newest, most laughs, oldest, A to Z) and paging.
- **Joke page** with a big reveal + confetti, a *guess the answer* box for riddles, a 😂 *That's funny!* button (saved in the database),
  Share button, previous/next/random navigation and keyboard shortcuts (Space, ←, →, R).
- **Register / log in** to submit. The submit form has a live flip-card preview and tips.
- **Ownership:** people can only edit or delete their own posts. Only the part of an email before the `@` is shown publicly.
- **My jokes** page, light/dark mode toggle, toast messages, mobile-friendly layout, and reduced motion for people who prefer it.

## Running it

1. Open `JokesWebApp.sln` in Visual Studio and press **F5**.
2. On start-up the app applies database migrations and adds the starter jokes, so no manual `Update-Database` is needed
   (running it yourself still works).

## Making yourself an admin

Admins can edit or delete any post (including the starter ones and anything posted before authors were tracked).
Add your login email to `appsettings.json`:

```json
"SiteAdmins": [ "you@example.com" ]
```

## Ideas for next steps

- Comments or "groan" reactions alongside laughs
- Tags (animals, food, school, tech…) and a "for kids" filter
- A report button and moderation queue
- Email confirmation with a real email sender (SendGrid, etc.)

---

Developed by [www.andreaswebstudio.co.za](https://www.andreaswebstudio.co.za)
