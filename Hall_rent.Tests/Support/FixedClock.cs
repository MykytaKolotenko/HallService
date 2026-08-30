using Hall_rent.Helpers;

public sealed class FixedClock : IClock
{
    public FixedClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}