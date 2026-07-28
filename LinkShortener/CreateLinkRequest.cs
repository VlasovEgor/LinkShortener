namespace LinkShortener;

public class CreateLinkRequest
{
    public CreateLinkRequest(string url)
    {
        Url = url;
    }

    public string Url { get; }
    
}