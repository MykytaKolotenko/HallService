using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Helpers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Request;
using Hall_rent.Response;

namespace Hall_rent.Service;

public class FavorService : IFavorService
{
    private readonly IFavorRepository _favorRepository;
    private readonly IHallUnitOfWork _hallUnitOfWork;

    public FavorService(IFavorRepository favorRepository, IHallUnitOfWork hallUnitOfWork)
    {
        _favorRepository = favorRepository;
        _hallUnitOfWork = hallUnitOfWork;
    }

    public async Task<List<FavorResponse>> GetFavours()
    {
        var favours = await _favorRepository.GetAllAsync();

        return FavorMapper.ToResponse(favours);
    }

    public async Task<Guid> AddFavour(FavorCreateRequest request)
    {
        FavorEntity favor = new FavorEntity
        {
            Name = request.Name,
            Price = request.Price
        };

        await _favorRepository.AddAsync(favor);
        await _hallUnitOfWork.SaveChangesAsync();

        return favor.Id;
    }

    public async Task UpdateFavour(UpdateFavorDto request)
    {
        var favour = await GetFavour(request.Id);

        favour.Name = request.Name;
        favour.Price = request.Price;

        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task DeleteFavour(Guid id)
    {
        var favour = await GetFavour(id);

        _favorRepository.Remove(favour);
        await _hallUnitOfWork.SaveChangesAsync();
    }

    private async Task<FavorEntity> GetFavour(Guid favourId)
    {
        var favour = await _favorRepository.GetByIdAsync(favourId);

        return favour ?? throw new NotFoundException($"Favour {favourId} not found");
    }
}
