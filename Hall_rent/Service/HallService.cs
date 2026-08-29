using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;

namespace Hall_rent.Service;

public class HallService : IHallService
{
    // private readonly IFavorRepository _favorRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IHallUnitOfWork _hallUnitOfWork;

    public HallService(
        IHallRepository hallHallRepository,
        IHallUnitOfWork hallHallUnitOfWork
        // IFavorRepository favorRepository
    )
    {
        _hallRepository = hallHallRepository;
        _hallUnitOfWork = hallHallUnitOfWork;
        // _favorRepository = favorRepository;
    }

    public async Task<Guid> AddHall(HallCreateDto hall)
    {
        HallEntity hallEntity = new HallEntity
        {
            Persons = hall.Persons,
            Price = hall.Price,
            Favors = hall.Favors ?? [],
            Name = hall.Name
        };

        await _hallRepository.AddAsync(hallEntity);

        try
        {
            await _hallUnitOfWork.SaveChangesAsync();
        }
        catch (UniqueConstraintException ex)
        {
            throw new HallNameAlreadyExistsException(
                hallEntity.Name,
                ex);
        }

        return hallEntity.Id;
    }

    public async Task UpdateHall(UpdateHallDto request)
    {
        var hall = await GetHall(request.Id);

        hall.Persons = request.Persons;
        hall.Price = request.Price;
        hall.Favors = request.Favors ?? [];

        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task DeleteHall(Guid id)
    {
        var hall = await GetHall(id);

        _hallRepository.Remove(hall);
        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task<List<Guid>> FindAvailableHallIdsAsync(HallSearchDto request)
    {
        var halls = await _hallRepository.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons);

        if (halls.Count == 0)
        {
            throw new NotFoundException(
                $"No halls available for {request.Persons} persons from {request.StartAt:yyyy-MM-dd HH:mm} to {request.EndAt:yyyy-MM-dd HH:mm}.");
        }

        return halls.Select(h => h.Id).ToList();
    }

    // private async Task<List<FavorEntity>> GetHallFavours(HallEntity hall, List<Guid>? requestedFavourIds)
    // {
    //     var favourIds = requestedFavourIds?.Distinct().ToList() ?? [];
    //
    //     if (favourIds.Count == 0)
    //     {
    //         return new List<FavorEntity>();
    //     }
    //
    //     var notOfferedByHall = favourIds.Except(hall.Favors).ToList();
    //
    //     if (notOfferedByHall.Count > 0)
    //     {
    //         throw new FavoursNotOfferedException(hall.Id, notOfferedByHall);
    //     }
    //
    //     var favours = await _favorRepository.GetByIdsAsync(favourIds);
    //
    //     if (favours.Count != favourIds.Count)
    //     {
    //         var missing = favourIds.Except(favours.Select(f => f.Id)).ToList();
    //         throw new NotFoundException($"Favours not found: {string.Join(", ", missing)}");
    //     }
    //
    //     return favours;
    // }

    private async Task<HallEntity> GetHall(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);

        return hall ?? throw new NotFoundException($"Hall {hallId} not found");
    }
}