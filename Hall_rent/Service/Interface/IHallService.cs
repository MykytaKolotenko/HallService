using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service.Interface;

public interface IHallService
{
    public Task<HallCreateResponse> CreateHall(HallCreateDto hall);
    public Task<UpdateHallResponse> UpdateHall(UpdateHallDto request);
    public Task DeleteHall(Guid id);
    public Task<HallSearchResponse> SearchAvailableHallIdsAsync(HallSearchDto request);
}