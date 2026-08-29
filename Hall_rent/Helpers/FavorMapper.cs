using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Response;

namespace Hall_rent.Helpers;

public static class FavorMapper
{
    public static List<FavorDto> ToDto(List<FavorEntity> favours)
    {
        return favours.Select(ToDto).ToList();
    }

    public static List<FavorResponse> ToResponse(List<FavorEntity> favours)
    {
        return favours.Select(ToResponse).ToList();
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

    private static FavorDto ToDto(FavorEntity favor)
    {
        return new FavorDto
        {
            Id = favor.Id,
            Name = favor.Name,
            Price = favor.Price
        };
    }
}
