using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Repositories;

public class LinksRepository
{
    private readonly LinkDbContext _dbContext;
    
    public LinksRepository(LinkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShortLink?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _dbContext.ShortLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.Code == code, cancellationToken);
    }

    public async Task AddAsync(string shortCode, string url, CancellationToken cancellationToken)
    {   
       // bool exists = await _dbContext.ShortLinks.AnyAsync(s => s.Code == shortCode, cancellationToken);
//
       // if (exists)
       // {
       //     throw new Exception("Такая короткая ссылка уже существует.");
       // }
        
        var shortLink = new ShortLink
        {
            Id = Guid.NewGuid(),
            Code = shortCode,
            OriginalUrl = url,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClickCount = 0
        };
        
        _dbContext.ShortLinks.Add(shortLink);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(string code, CancellationToken cancellationToken)
    {   
        await _dbContext.ShortLinks.
            Where(link => link.Code == code).
            ExecuteDeleteAsync(cancellationToken);
    }
}