using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IHallService
{
    public Task<Guid> AddHall(HallCreateRequest hall);
    public Task UpdateHall(UpdateHallDto request);
    public Task DeleteHall(Guid id);
    public Task<HallBookResponse> BookHall(BookHallDto request);
    public Task<List<Guid>> FindAvailableHallIdsAsync(HallSearchDto request);
}