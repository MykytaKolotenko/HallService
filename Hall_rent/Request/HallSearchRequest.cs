namespace Hall_rent.Request;

public record HallSearchRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Persons { get; set; }
}
