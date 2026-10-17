using System.ComponentModel.DataAnnotations;

namespace JokesWebApp.Models
{
    public enum JokeCategory
    {
        [Display(Name = "Joke")]
        Joke = 0,

        [Display(Name = "Riddle")]
        Riddle = 1
    }

    public class Joke
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please write the question or set-up.")]
        [Display(Name = "Question / set-up")]
        public string JokeQuestion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Every joke needs a punchline (and every riddle an answer)!")]
        [Display(Name = "Answer / punchline")]
        public string JokeAnswer { get; set; } = string.Empty;

        [Display(Name = "Type")]
        public JokeCategory Category { get; set; } = JokeCategory.Joke;

        // Who submitted it. Null for jokes that existed before accounts were tracked
        // and for the built-in starter jokes.
        public string? AuthorId { get; set; }

        [Display(Name = "Submitted by")]
        public string? AuthorName { get; set; }

        [Display(Name = "Added")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Number of times someone pressed "That's funny!"
        public int Likes { get; set; }

        public int Views { get; set; }

        public string Emoji => Category == JokeCategory.Riddle ? "🧩" : "😂";

        public string DisplayAuthor => string.IsNullOrWhiteSpace(AuthorName) ? "Anonymous" : AuthorName!;

        public Joke() { }
    }
}
