namespace TQI.Infrastructure.Entity
{
    public enum AppEnvironment
    {
        Production,
        Debug
    }

    public enum ScrapeStatus
    {
        Failed = -1,
        Pending = 0,
        InProgress = 1,
        Done = 2
    }

    public enum ScrapeType
    {
        Competition = 0,
        PlayerOverUnder = 1,
        PlayerHeadToHead = 2
    }
}
