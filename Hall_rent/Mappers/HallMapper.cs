using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Request;

namespace Hall_rent.Mappers;

public static class HallMapper
{
    public static HallEntity CreateDtoToEntity(HallCreateDto createData, List<FavorEntity> favors)
    {
        return new HallEntity
        {
            Name = createData.Name,
            Price = createData.Price,
            Persons = createData.Persons,
            Favors = favors.Select(f => new HallFavorEntity { FavorId = f.Id }).ToList()
        };
    }

    public static UpdateHallDto ToDto(HallUpdateRequest data, Guid hallId)
    {
        return new UpdateHallDto
        {
            Id = hallId,
            Persons = data.Persons,
            Price = data.Price,
            Favors = data.Favors ?? [],
            Name = data.Name
        };
    }

    public static HallCreateDto ToDto(HallCreateRequest data)
    {
        return new HallCreateDto
        {
            Persons = data.Persons,
            Price = data.Price,
            Name = data.Name,
            Favors = data.Favors ?? []
        };
    }

    public static BookHallDto ToDto(HallBookRequest data, Guid hallId)
    {
        return new BookHallDto
        {
            HallId = hallId,
            Favors = data.Favors ?? [],
            Persons = data.Persons,
            StartAt = data.From,
            EndAt = data.To
        };
    }

    public static HallSearchDto ToDto(HallSearchRequest data)
    {
        return new HallSearchDto
        {
            From = data.From,
            To = data.To,
            Persons = data.Persons
        };
    }
}