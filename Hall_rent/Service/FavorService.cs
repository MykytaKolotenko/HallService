using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Mappers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service.Interface;

namespace Hall_rent.Service;

public class FavorService : IFavorService
{
    private readonly IFavorRepository _favorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FavorService(IFavorRepository favorRepository, IUnitOfWork unitOfWork)
    {
        _favorRepository = favorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FavorResponse>> GetFavors()
    {
        var favors = await _favorRepository.GetAllAsync();

        return FavorMapper.ToResponse(favors);
    }

    public async Task<FavorCreateResponse> AddFavor(FavorCreateDto request)
    {
        var favor = new FavorEntity
        {
            Name = request.Name,
            Price = request.Price
        };

        await _favorRepository.AddAsync(favor);
        await _unitOfWork.SaveChangesAsync();

        return new FavorCreateResponse(favor.Id);
    }

    public async Task UpdateFavor(UpdateFavorDto request)
    {
        var favorEntity = await GetFavor(request.Id);

        favorEntity.Name = request.Name;
        favorEntity.Price = request.Price;

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<FavorEntity> GetFavor(Guid favorId)
    {
        var favorEntity = await _favorRepository.GetByIdAsync(favorId);

        return favorEntity ?? throw new NotFoundException($"Favor {favorId} not found");
    }
}