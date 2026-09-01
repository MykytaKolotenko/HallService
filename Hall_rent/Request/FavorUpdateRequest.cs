using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record FavorUpdateRequest : IFavorRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}