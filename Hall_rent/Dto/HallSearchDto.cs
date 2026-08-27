namespace Hall_rent.Dto;

public struct HallSearchDto
{
    public DateTime StartAt;
    public DateTime EndAt;
    public int Persons;

    public HallSearchDto(DateTime startAt, DateTime endAt, int persons)
    {
        StartAt = startAt;
        EndAt = endAt;
        Persons = persons;
    }
}