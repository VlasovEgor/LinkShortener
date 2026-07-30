using LinkShortener.Repositories;
using LRU_Cache;

namespace LinkShortener.Services;

public class LinksService
{   
    private const int NUMBER_GENERATION_ATTEMPTS = 10;
    
    private readonly ICodeGenerator _generator;
    private readonly LinksRepository _repository;
    private readonly LruCache<string, string> _cache;

    public LinksService(ICodeGenerator generator, LinksRepository linksRepository, LruCache<string, string> cache)
    {
        _generator = generator;
        _repository = linksRepository;
        _cache = cache;
    }

    public async Task<LinkServiceResponse> TryCreateLink(string originalUrl, string? idempotencyKey, CancellationToken cancellationToken)
    {  
        cancellationToken.ThrowIfCancellationRequested();
        
        string? normalizedIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
                ? null
                : idempotencyKey.Trim();
        
        string? code;
        
        if (IsValidUrl(originalUrl) == false)
            return new LinkServiceResponse(GenerateStatus.InvalidUrl, null);
        
        if (normalizedIdempotencyKey != null)
        {
            code = await GetExistingResponse(normalizedIdempotencyKey, cancellationToken);
            if(code != null)
                return new LinkServiceResponse(GenerateStatus.Returned, code);
        }

        for (int i = 0; i < NUMBER_GENERATION_ATTEMPTS; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            code = _generator.Generate();

            bool linkAdded = await _repository.TryAddAsync(code, originalUrl, normalizedIdempotencyKey,cancellationToken);

            if (linkAdded)
                return new LinkServiceResponse(GenerateStatus.Success, code);
            
            if (normalizedIdempotencyKey != null)
            {
                code = await GetExistingResponse(normalizedIdempotencyKey, cancellationToken);
                if(code != null)
                    return new LinkServiceResponse(GenerateStatus.Returned, code);
            }
        }

        return new LinkServiceResponse(GenerateStatus.GenerationFailed, null);
    }

    private bool IsValidUrl(string originalUrl)
    {
       if(!Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri? uri))
           return false;
        
       if(uri.Scheme != Uri.UriSchemeHttps &&  uri.Scheme != Uri.UriSchemeHttp)
           return false;
       
       return true;
    }

    private async Task<string?> GetExistingResponse(string? idempotencyKey, CancellationToken cancellationToken)
    {
        return await _repository.GetCodeByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
    }

    public async Task<string?> TryGetLink(string code, CancellationToken cancellationToken)
    {   
        if (_cache.TryGet(code, out string cachedUrl))
            return cachedUrl;

        string? url = await _repository.GetOriginalLinkByCodeAsync(code, cancellationToken);

        if (url != null)
            _cache.Set(code, url);
        
        return url;
    }
    
    public async Task<Link?> TryGetStatistics(string code, CancellationToken cancellationToken)
    {
        Link? statistics = await _repository.GetStatisticsByCodeAsync(code, cancellationToken);
        return statistics;
    }
    
    public async Task<bool> DeleteLink(string code, CancellationToken cancellationToken)
    {   
        bool deleted = await _repository.DeleteAsync(code, cancellationToken);

        if (deleted)
            _cache.Remove(code);

        return deleted;
    }

    public async Task IncreaseClickCount(string code, CancellationToken cancellationToken)
    {
       await _repository.IncreaseClickCount(code, cancellationToken);
    }
}

public enum GenerateStatus
{   
    None = 0,
    Success = 1,
    InvalidUrl = 2,
    GenerationFailed = 3,
    Returned = 4
}

public record LinkServiceResponse
{
    public GenerateStatus Status { get; }
    public string? Code { get; }

    public LinkServiceResponse(GenerateStatus status, string? code)
    {
        Status = status;
        Code = code;
    }
}