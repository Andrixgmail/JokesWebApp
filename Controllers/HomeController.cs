using JokesWebApp.Data;
using JokesWebApp.Models;
using JokesWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace JokesWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var jokes = _context.Joke.AsNoTracking();

            var model = new HomeViewModel
            {
                JokeOfTheDay = await PickOfTheDayAsync(JokeCategory.Joke),
                RiddleOfTheDay = await PickOfTheDayAsync(JokeCategory.Riddle),
                Latest = await jokes.OrderByDescending(j => j.CreatedAt).ThenByDescending(j => j.Id).Take(3).ToListAsync(),
                MostLaughed = await jokes.Where(j => j.Likes > 0).OrderByDescending(j => j.Likes).Take(3).ToListAsync(),
                TopContributors = await jokes
                    .Where(j => j.AuthorName != null && j.AuthorName != SeedData.TeamName)
                    .GroupBy(j => j.AuthorName!)
                    .Select(g => new ContributorStat { Name = g.Key, Submissions = g.Count(), Laughs = g.Sum(j => j.Likes) })
                    .OrderByDescending(c => c.Laughs).ThenByDescending(c => c.Submissions)
                    .Take(5).ToListAsync(),
                Stats = await GetStatsAsync(),
            };

            return View(model);
        }

        // GET: Home/Stats. Polled by the home page so the counters update live.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Stats()
        {
            return Json(await GetStatsAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<SiteStats> GetStatsAsync()
        {
            var jokes = _context.Joke.AsNoTracking();
            return new SiteStats
            {
                Jokes = await jokes.CountAsync(j => j.Category == JokeCategory.Joke),
                Riddles = await jokes.CountAsync(j => j.Category == JokeCategory.Riddle),
                Laughs = await jokes.SumAsync(j => (int?)j.Likes) ?? 0,
                Views = await jokes.SumAsync(j => (int?)j.Views) ?? 0,
            };
        }

        // Same pick for everyone for the whole day, changes at midnight (UTC).
        private async Task<Joke?> PickOfTheDayAsync(JokeCategory category)
        {
            var query = _context.Joke.AsNoTracking().Where(j => j.Category == category);
            var count = await query.CountAsync();
            if (count == 0) return null;
            var dayIndex = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber % count;
            return await query.OrderBy(j => j.Id).Skip(dayIndex).FirstOrDefaultAsync();
        }
    }
}
