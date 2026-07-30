using LinkShortener.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener;

public class LinkDbContext(DbContextOptions<LinkDbContext>  options) : DbContext(options)
{
    public DbSet<Link> ShortLinks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new LinkConfiguration());
        
        base.OnModelCreating(builder);
    }
}