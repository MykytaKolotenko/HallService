namespace Hall_rent.Helpers;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}