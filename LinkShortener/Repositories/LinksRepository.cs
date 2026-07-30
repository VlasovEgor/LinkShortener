using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Repositories;

public class LinksRepository
{
    private readonly LinkDbContext _dbContext;
    
    public LinksRepository(LinkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Link?> GetStatisticsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _dbContext.ShortLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.Code == code, cancellationToken);
    }
    
    public async Task<string?> GetOriginalLinkByCodeAsync(string code, CancellationToken cancellationToken)
    {
        Link? linkStatistics = await _dbContext.ShortLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.Code == code, cancellationToken);

        return linkStatistics?.OriginalUrl;
    }

    public async Task<bool> TryAddAsync(string shortCode, string url, CancellationToken cancellationToken)
    {   
        Link link = new Link
        {
            Id = Guid.NewGuid(),
            Code = shortCode,
            OriginalUrl = url,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClickCount = 0
        };
        
        _dbContext.ShortLinks.Add(link);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqliteException
                                                   && sqliteException.SqliteExtendedErrorCode == 2067)
        {   
            _dbContext.Entry(link).State = EntityState.Detached;
            return false;
        }
    }
    
    public async Task<bool> DeleteAsync(string code, CancellationToken cancellationToken)
    {   
        int numberDeleted = await _dbContext.ShortLinks.
            Where(link => link.Code == code).
            ExecuteDeleteAsync(cancellationToken);
        
        return numberDeleted > 0;
    }

    public async Task IncreaseClickCount(string code, CancellationToken cancellationToken)
    {
        await _dbContext.ShortLinks.Where(link => link.Code == code).
            ExecuteUpdateAsync(s => s.SetProperty(l => l.ClickCount, l => l.ClickCount + 1), cancellationToken: cancellationToken);
        
    }
}