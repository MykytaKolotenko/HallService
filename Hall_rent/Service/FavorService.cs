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

    public async Task<List<FavorResponse>> GetFavors()
    {
        var favors = await _favorRepository.GetAllAsync();

        return FavorMapper.ToResponse(favors);
    }

    public async Task<FavorCreateResponse> AddFavor(FavorCreateRequest request)
    {
        var favor = new FavorEntity
        {
            Name = request.Name,
            Price = request.Price
        };

        await _favorRepository.AddAsync(favor);
        await _hallUnitOfWork.SaveChangesAsync();

        return new FavorCreateResponse(favor.Id);
    }

    public async Task UpdateFavor(UpdateFavorDto request)
    {
        var favorEntity = await GetFavor(request.Id);

        favorEntity.Name = request.Name;
        favorEntity.Price = request.Price;

        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task DeleteFavor(Guid id)
    {
        var favor = await GetFavor(id);

        _favorRepository.Remove(favor);
        await _hallUnitOfWork.SaveChangesAsync();
    }

    private async Task<FavorEntity> GetFavor(Guid favorId)
    {
        var favorEntity = await _favorRepository.GetByIdAsync(favorId);

        return favorEntity ?? throw new NotFoundException($"Favor {favorId} not found");
    }
}
