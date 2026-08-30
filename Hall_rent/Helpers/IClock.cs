namespace Hall_rent.Helpers;

public interface IClock
{
    DateTime UtcNow { get; }
}