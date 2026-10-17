using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JokesWebApp.Models;

namespace JokesWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JokesWebApp.Models.Joke> Joke { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Joke>(e =>
            {
                e.Property(j => j.AuthorId).HasMaxLength(450);
                e.Property(j => j.AuthorName).HasMaxLength(256);
                e.Property(j => j.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasIndex(j => j.Category);
                e.HasIndex(j => j.AuthorId);
            });
        }
    }
}
