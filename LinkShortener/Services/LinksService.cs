using LinkShortener.Repositories;

namespace LinkShortener.Services;

public class LinksService
{   
    private const int NUMBER_GENERATION_ATTEMPTS = 10;
    
    private readonly ICodeGenerator _generator;
    private readonly LinksRepository _repository;

    public LinksService(ICodeGenerator generator, LinksRepository linksRepository)
    {
        _generator = generator;
        _repository = linksRepository;
    }

    public async Task<LinkServiceResponse> TryCreateLink(string originalUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsValidUrl(originalUrl) == false)
            return new LinkServiceResponse(GenerateStatus.InvalidUrl, null);

        for (int i = 0; i < NUMBER_GENERATION_ATTEMPTS; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string code = _generator.Generate();

            bool linkAdded = await _repository.TryAddAsync(code, originalUrl, cancellationToken);

            if (linkAdded)
                return new LinkServiceResponse(GenerateStatus.Success, code);
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

    public async Task<string?> TryGetLink(string code, CancellationToken cancellationToken)
    {
         return await _repository.GetOriginalLinkByCodeAsync(code, cancellationToken);
    }
    
    public async Task<Link?> TryGetStatistics(string code, CancellationToken cancellationToken)
    {
        Link? statistics = await _repository.GetStatisticsByCodeAsync(code, cancellationToken);
        return statistics;
    }
    
    public async Task<bool> DeleteLink(string code, CancellationToken cancellationToken)
    {   
        return await _repository.DeleteAsync(code,cancellationToken);
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
    GenerationFailed = 3
}

public struct LinkServiceResponse
{
    public readonly GenerateStatus Status;
    public readonly string? Code;

    public LinkServiceResponse(GenerateStatus status, string? code)
    {
        Status = status;
        Code = code;
    }
}