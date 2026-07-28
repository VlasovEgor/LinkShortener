using LinkShortener.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener;

public class LinkDbContext(DbContextOptions<LinkDbContext>  options) : DbContext(options)
{
    public DbSet<ShortLink> ShortLinks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ShortLinkConfiguration());
        
        base.OnModelCreating(builder);
    }
}