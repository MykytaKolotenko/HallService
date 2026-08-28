namespace Hall_rent.Dto;

public struct HallSearchDto
{
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int Persons { get; private set; }

    public HallSearchDto(DateTime startAt, DateTime endAt, int persons)
    {
        StartAt = startAt;
        EndAt = endAt;
        Persons = persons;
    }
}