namespace JokesWebApp.Models.ViewModels
{
    public class JokeDetailsViewModel
    {
        public Joke Joke { get; set; } = new();
        public int? PreviousId { get; set; }
        public int? NextId { get; set; }
        public bool CanManage { get; set; }
        public IReadOnlyList<Joke> MoreLikeThis { get; set; } = new List<Joke>();
    }
}
