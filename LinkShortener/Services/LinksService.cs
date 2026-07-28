using System.Collections.Concurrent;

namespace LinkShortener.Services;

public class LinksService
{   
    private const int NUMBER_GENERATION_ATTEMPTS = 10;
    
    private readonly ConcurrentDictionary<string, string> _links = new();
    private readonly ICodeGenerator _generator;

    public LinksService(ICodeGenerator generator)
    {
        _generator = generator;
    }

    public bool TryCreateLink(string originalUrl, CancellationToken cancellationToken,
        out string code, out GenerateStatus result)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (IsValidUrl(originalUrl) == false)
        {
            code = null;
            result = GenerateStatus.InvalidUrl;
            return false;
        }
             
        for (int i = 0; i < NUMBER_GENERATION_ATTEMPTS; i++)
        {   
            cancellationToken.ThrowIfCancellationRequested();
            
            code = _generator.Generate();
            if (_links.TryAdd(code, originalUrl))
            {
                result = GenerateStatus.Success;
                return true;
            }
        }
        
        code = null;
        result = GenerateStatus.GenerationFailed;
        return false;
    }

    private bool IsValidUrl(string originalUrl)
    {
       if(!Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri? uri))
           return false;
        
       if(uri.Scheme != Uri.UriSchemeHttps &&  uri.Scheme != Uri.UriSchemeHttp)
           return false;
       
       return true;
    }

    public bool TryGetLink(string code, out string originalUrl)
    {
        return _links.TryGetValue(code, out originalUrl!);
    }
    
    public bool DeleteLink(string code)
    {
        return _links.TryRemove(code, out _);
    }
}

public enum GenerateStatus
{   
    None = 0,
    Success = 1,
    InvalidUrl = 2,
    GenerationFailed = 3
}