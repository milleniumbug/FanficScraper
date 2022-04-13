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
}