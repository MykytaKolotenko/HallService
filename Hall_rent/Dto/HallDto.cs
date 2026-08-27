namespace Hall_rent.Dto;

public struct HallDto
{
    public Guid Id;
    public int Persons;
    public decimal Price;
    public List<FavorDto> Favors;
}