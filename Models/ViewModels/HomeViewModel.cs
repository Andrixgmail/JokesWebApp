namespace JokesWebApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public Joke? JokeOfTheDay { get; set; }
        public Joke? RiddleOfTheDay { get; set; }
        public IReadOnlyList<Joke> Latest { get; set; } = new List<Joke>();
        public IReadOnlyList<Joke> MostLaughed { get; set; } = new List<Joke>();
        public IReadOnlyList<ContributorStat> TopContributors { get; set; } = new List<ContributorStat>();
        public SiteStats Stats { get; set; } = new();
    }

    public class ContributorStat
    {
        public string Name { get; set; } = "";
        public int Submissions { get; set; }
        public int Laughs { get; set; }
    }

    public class SiteStats
    {
        public int Jokes { get; set; }
        public int Riddles { get; set; }
        public int Laughs { get; set; }
        public int Views { get; set; }
    }
}
