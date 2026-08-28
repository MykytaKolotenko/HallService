using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IFavorService
{
    public Task<List<FavorResponse>> GetFavours();
    public Task<Guid> AddFavour(FavorCreateRequest request);
    public Task UpdateFavour(UpdateFavorDto request);
    public Task DeleteFavour(Guid id);
}