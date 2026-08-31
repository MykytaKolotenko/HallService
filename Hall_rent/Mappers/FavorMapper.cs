using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Request;
using Hall_rent.Response;

namespace Hall_rent.Mappers;

public static class FavorMapper
{
    public static List<FavorResponse> ToResponse(List<FavorEntity> favors)
    {
        return favors.Select(ToResponse).ToList();
    }

    public static HallBookingFavorEntity ToEntity(FavorEntity favor, HallBookingEntity booking)
    {
        return new HallBookingFavorEntity
        {
            Favor = favor,
            Booking = booking,
            PriceAtBooking = favor.Price
        };
    }

    public static FavorCreateDto ToDto(FavorCreateRequest request)
    {
        return new FavorCreateDto
        {
            Name = request.Name,
            Price = request.Price
        };
    }

    public static UpdateFavorDto ToDto(FavorUpdateRequest request, Guid favorId)
    {
        return new UpdateFavorDto
        {
            Id = favorId,
            Name = request.Name,
            Price = request.Price
        };
    }

    public static FavorDto ToDto(FavorEntity favor)
    {
        return new FavorDto
        {
            Id = favor.Id,
            Name = favor.Name,
            Price = favor.Price
        };
    }

    private static FavorResponse ToResponse(FavorEntity favor)
    {
        return new FavorResponse
        {
            Id = favor.Id,
            Name = favor.Name,
            Price = favor.Price
        };
    }
}
