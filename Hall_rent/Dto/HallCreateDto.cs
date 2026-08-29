namespace Hall_rent.Dto;

public struct HallCreateDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Persons { get; set; }
    public List<Guid>? Favors { get; set; }

    public static HallCreateDto Clone(HallCreateDto data)
    {
        return new HallCreateDto()
        {
            Name = data.Name,
            Price = data.Price,
            Persons = data.Persons,
            Favors = data.Favors
        };
    }
}
