using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service.Interface;

public interface IFavorService
{
    public Task<List<FavorResponse>> GetFavors();
    public Task<FavorCreateResponse> AddFavor(FavorCreateDto request);
    public Task UpdateFavor(UpdateFavorDto request);
}