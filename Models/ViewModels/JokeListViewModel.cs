namespace JokesWebApp.Models.ViewModels
{
    public class JokeListViewModel
    {
        public IReadOnlyList<Joke> Jokes { get; set; } = new List<Joke>();
        public string Heading { get; set; } = "Browse jokes & riddles";
        public string? Query { get; set; }
        public JokeCategory? Category { get; set; }
        public string Sort { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public bool MineOnly { get; set; }
        public string? CurrentUserId { get; set; }
        public bool IsAdmin { get; set; }

        public bool CanManage(Joke j) =>
            IsAdmin || (CurrentUserId != null && j.AuthorId == CurrentUserId);
    }
}
