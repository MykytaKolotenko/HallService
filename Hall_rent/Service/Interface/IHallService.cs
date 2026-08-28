using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IHallService
{
    public Task<Guid> AddHall(HallCreateDto hall);
    public Task UpdateHall(UpdateHallDto request);
    public Task DeleteHall(Guid id);
    public Task<HallBookResponse> BookHall(BookHallDto request);
    public Task<List<Guid>> FindAvailableHallIdsAsync(HallSearchDto request);
}