namespace LinkShortener;

public class ShortLink()
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public int ClickCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}