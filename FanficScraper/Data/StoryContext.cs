using Microsoft.EntityFrameworkCore;

namespace FanficScraper.Data;

public class StoryContext : DbContext
{
    public StoryContext(
        DbContextOptions<StoryContext> options) :
        base(options)
    {
        
    }
    
    public DbSet<Story> Stories { get; set; }
    
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        {
            var storyBuilder = modelBuilder.Entity<Story>();
            storyBuilder.Property(story => story.AuthorName)
                .UseCollation("NOCASE");
            storyBuilder.Property(story => story.StoryName)
                .UseCollation("NOCASE");
            storyBuilder.HasIndex(story => story.FileName);
            storyBuilder.HasIndex(story => story.StoryUpdated);
            storyBuilder.HasIndex(story => story.LastUpdated);
            storyBuilder.HasIndex(story => story.StoryName);
            storyBuilder.HasIndex(story => story.StoryAdded);
        }
        
        {
            var userBuilder = modelBuilder.Entity<User>();
            userBuilder.HasIndex(user => user.Login).IsUnique();
            userBuilder.HasIndex(user => new { user.CreationDate, user.IsActivated });
        }
    }
}