using Microsoft.EntityFrameworkCore;

namespace FanficScraper.Data;

public class StoryContext : DbContext
{
    public StoryContext(
        DbContextOptions<StoryContext> options) :
        base(options)
    {
        
    }
    
    public DbSet<StoryData> StoryData { get; set; }
    
    public DbSet<Story> Stories { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<DownloadJob> DownloadJobs { get; set; }
    
    public DbSet<DiscordUser> DiscordUsers { get; set; }

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
            storyBuilder.HasIndex(story => story.NextUpdateIn);
            storyBuilder.HasIndex(story => story.StoryName);
            storyBuilder.HasIndex(story => story.StoryAdded);

            storyBuilder
                .HasOne(story => story.StoryData)
                .WithOne(storyData => storyData.Story)
                .HasForeignKey<StoryData>(storyData => storyData.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        {
            var userBuilder = modelBuilder.Entity<User>();
            userBuilder.HasIndex(user => user.Login).IsUnique();
            userBuilder.HasIndex(user => new { user.CreationDate, user.IsActivated });
        }
        
        {
            var downloadJobBuilder = modelBuilder.Entity<DownloadJob>();
            downloadJobBuilder.HasIndex(user => new { user.Status, user.AddedDate });
            downloadJobBuilder.HasIndex(user => user.AddedDate);
        }
        
        {
            var userBuilder = modelBuilder.Entity<DiscordUser>();
            userBuilder.HasKey(user => user.Id);
        }
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<IReadOnlyList<string>>()
            .HaveConversion<StringArrayConverter, StringArrayComparer>();
    }
}