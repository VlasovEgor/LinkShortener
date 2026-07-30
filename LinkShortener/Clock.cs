namespace LinkShortener;

public class Clock: IClock
{
    public DateTimeOffset GetUtsNow()
    {
        return DateTimeOffset.UtcNow;
    }
}