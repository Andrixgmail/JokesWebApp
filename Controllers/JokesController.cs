using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JokesWebApp.Data;
using JokesWebApp.Models;
using JokesWebApp.Models.ViewModels;

namespace JokesWebApp.Controllers
{
    public class JokesController : Controller
    {
        private const int PageSize = 12;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public JokesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        // GET: Jokes?q=...&category=Riddle&sort=popular&page=2
        public async Task<IActionResult> Index(string? q, JokeCategory? category, string sort = "newest", int page = 1)
        {
            var model = await BuildListAsync(_context.Joke.AsNoTracking(), q, category, sort, page);
            model.Heading = category switch
            {
                JokeCategory.Riddle => "Riddles to puzzle over",
                JokeCategory.Joke => "Jokes to brighten your day",
                _ => string.IsNullOrWhiteSpace(q) ? "Browse jokes & riddles" : $"Results for \"{q}\""
            };
            return View(model);
        }

        // GET: Jokes/Riddles  (handy short link)
        public IActionResult Riddles() => RedirectToAction(nameof(Index), new { category = JokeCategory.Riddle });

        // GET: Jokes/Mine
        [Authorize]
        public async Task<IActionResult> Mine(string sort = "newest", int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var model = await BuildListAsync(_context.Joke.AsNoTracking().Where(j => j.AuthorId == userId), null, null, sort, page);
            model.MineOnly = true;
            model.Heading = "My submissions";
            return View("Index", model);
        }

        // GET: Jokes/ShowSearchForm
        public IActionResult ShowSearchForm()
        {
            return View();
        }

        // Kept so old links/bookmarks still work; search now lives on the Index page.
        public IActionResult ShowSearchFormResults(string? SearchPhrase)
        {
            return RedirectToAction(nameof(Index), new { q = SearchPhrase });
        }

        // GET: Jokes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var joke = await _context.Joke.FirstOrDefaultAsync(m => m.Id == id);
            if (joke == null)
            {
                return NotFound();
            }

            joke.Views++;
            await _context.SaveChangesAsync();

            var model = new JokeDetailsViewModel
            {
                Joke = joke,
                CanManage = CanManage(joke),
                PreviousId = await _context.Joke.Where(j => j.Id < joke.Id && j.Category == joke.Category)
                    .OrderByDescending(j => j.Id).Select(j => (int?)j.Id).FirstOrDefaultAsync(),
                NextId = await _context.Joke.Where(j => j.Id > joke.Id && j.Category == joke.Category)
                    .OrderBy(j => j.Id).Select(j => (int?)j.Id).FirstOrDefaultAsync(),
                MoreLikeThis = await _context.Joke.AsNoTracking()
                    .Where(j => j.Id != joke.Id && j.Category == joke.Category)
                    .OrderBy(j => Guid.NewGuid()).Take(3).ToListAsync()
            };

            return View(model);
        }

        // GET: Jokes/Random?category=Riddle
        public async Task<IActionResult> Random(JokeCategory? category)
        {
            var query = _context.Joke.AsQueryable();
            if (category.HasValue) query = query.Where(j => j.Category == category.Value);

            var count = await query.CountAsync();
            if (count == 0) return RedirectToAction(nameof(Index));

            var id = await query.OrderBy(j => j.Id)
                .Skip(System.Random.Shared.Next(count))
                .Select(j => j.Id)
                .FirstAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Jokes/Laugh/5   (called from JavaScript; returns the new laugh count)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Laugh(int id)
        {
            var updated = await _context.Joke
                .Where(j => j.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Likes, j => j.Likes + 1));
            if (updated == 0) return NotFound();

            var likes = await _context.Joke.Where(j => j.Id == id).Select(j => j.Likes).FirstAsync();
            return Json(new { likes });
        }

        // GET: Jokes/Create
        [Authorize]
        public IActionResult Create(JokeCategory category = JokeCategory.Joke)
        {
            return View(new Joke { Category = category });
        }

        // POST: Jokes/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JokeQuestion,JokeAnswer,Category")] Joke joke)
        {
            Tidy(joke);

            if (await _context.Joke.AnyAsync(j => j.JokeQuestion == joke.JokeQuestion))
            {
                ModelState.AddModelError(nameof(Joke.JokeQuestion), "Someone has already submitted that one. Try another!");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                joke.AuthorId = user?.Id;
                joke.AuthorName = PublicName(user?.UserName);
                joke.CreatedAt = DateTime.UtcNow;

                _context.Add(joke);
                await _context.SaveChangesAsync();
                TempData["Toast"] = joke.Category == JokeCategory.Riddle
                    ? "🧩 Your riddle is live! Let's see who can solve it."
                    : "🎉 Your joke is live! Thanks for sharing the laughs.";
                return RedirectToAction(nameof(Details), new { id = joke.Id });
            }
            return View(joke);
        }

        // GET: Jokes/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var joke = await _context.Joke.FindAsync(id);
            if (joke == null)
            {
                return NotFound();
            }
            if (!CanManage(joke))
            {
                return Forbid();
            }
            return View(joke);
        }

        // POST: Jokes/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JokeQuestion,JokeAnswer,Category")] Joke input)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            var joke = await _context.Joke.FindAsync(id);
            if (joke == null)
            {
                return NotFound();
            }
            if (!CanManage(joke))
            {
                return Forbid();
            }

            Tidy(input);
            if (ModelState.IsValid)
            {
                // Only copy the editable fields so author, laughs and dates can't be tampered with.
                joke.JokeQuestion = input.JokeQuestion;
                joke.JokeAnswer = input.JokeAnswer;
                joke.Category = input.Category;
                await _context.SaveChangesAsync();
                TempData["Toast"] = "✏️ Changes saved.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(input);
        }

        // GET: Jokes/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var joke = await _context.Joke.FirstOrDefaultAsync(m => m.Id == id);
            if (joke == null)
            {
                return NotFound();
            }
            if (!CanManage(joke))
            {
                return Forbid();
            }

            return View(joke);
        }

        // POST: Jokes/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var joke = await _context.Joke.FindAsync(id);
            if (joke != null)
            {
                if (!CanManage(joke))
                {
                    return Forbid();
                }
                _context.Joke.Remove(joke);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "🗑️ Deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------- helpers

        private async Task<JokeListViewModel> BuildListAsync(IQueryable<Joke> query, string? q, JokeCategory? category, string sort, int page)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(j => j.JokeQuestion.Contains(term) || j.JokeAnswer.Contains(term)
                                         || (j.AuthorName != null && j.AuthorName.Contains(term)));
            }
            if (category.HasValue)
            {
                query = query.Where(j => j.Category == category.Value);
            }

            sort = (sort ?? "newest").ToLowerInvariant();
            query = sort switch
            {
                "popular" => query.OrderByDescending(j => j.Likes).ThenByDescending(j => j.Views).ThenByDescending(j => j.Id),
                "oldest" => query.OrderBy(j => j.CreatedAt).ThenBy(j => j.Id),
                "az" => query.OrderBy(j => j.JokeQuestion),
                _ => query.OrderByDescending(j => j.CreatedAt).ThenByDescending(j => j.Id),
            };

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            return new JokeListViewModel
            {
                Jokes = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                Query = q,
                Category = category,
                Sort = sort,
                Page = page,
                TotalPages = totalPages,
                TotalCount = total,
                CurrentUserId = _userManager.GetUserId(User),
                IsAdmin = IsAdmin(),
            };
        }

        private bool IsAdmin()
        {
            var name = User.Identity?.Name;
            if (string.IsNullOrEmpty(name)) return false;
            var admins = _config.GetSection("SiteAdmins").Get<string[]>() ?? Array.Empty<string>();
            return admins.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        private bool CanManage(Joke joke)
        {
            if (User.Identity?.IsAuthenticated != true) return false;
            if (IsAdmin()) return true;
            var userId = _userManager.GetUserId(User);
            return joke.AuthorId != null && joke.AuthorId == userId;
        }

        private static void Tidy(Joke joke)
        {
            joke.JokeQuestion = (joke.JokeQuestion ?? "").Trim();
            joke.JokeAnswer = (joke.JokeAnswer ?? "").Trim();
        }

        // Show only the part of an email before the @ so addresses aren't published.
        private static string PublicName(string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return "Anonymous";
            var at = userName.IndexOf('@');
            return at > 0 ? userName[..at] : userName;
        }
    }
}
