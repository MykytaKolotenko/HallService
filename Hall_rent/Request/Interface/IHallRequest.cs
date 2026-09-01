namespace Hall_rent.Request.Interface;

public interface IHallRequest
{
    string Name { get; }
    int Persons { get; }
    decimal Price { get; }
}