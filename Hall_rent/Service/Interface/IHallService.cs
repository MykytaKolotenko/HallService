using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IHallService
{
    public Task<HallCreateResponse> AddHall(HallCreateDto hall);
    public Task<UpdateHallResponse> UpdateHall(UpdateHallDto request);
    public Task DeleteHall(Guid id);
    public Task<HallSearchResponse> FindAvailableHallIdsAsync(HallSearchDto request);
}
