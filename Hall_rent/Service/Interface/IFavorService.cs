using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IFavorService
{
    public Task<List<FavorResponse>> GetFavors();
    public Task<FavorCreateResponse> AddFavor(FavorCreateRequest request);
    public Task UpdateFavor(UpdateFavorDto request);
    public Task DeleteFavor(Guid id);
}
