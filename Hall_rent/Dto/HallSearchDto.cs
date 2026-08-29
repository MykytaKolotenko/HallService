namespace Hall_rent.Dto;

public struct HallSearchDto
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Persons { get; set; }
}
