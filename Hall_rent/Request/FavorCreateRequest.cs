using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record FavorCreateRequest : IFavorRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}