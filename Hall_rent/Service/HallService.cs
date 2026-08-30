using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Helpers;
using Hall_rent.Repository.Interfaces;

namespace Hall_rent.Service;

public class HallService : IHallService
{
    private readonly IFavorResolver _favorResolver;
    private readonly IHallRepository _hallRepository;
    private readonly IHallUnitOfWork _hallUnitOfWork;

    public HallService(
        IHallRepository hallRepository,
        IHallUnitOfWork hallUnitOfWork,
        IFavorResolver favorResolver)
    {
        _hallRepository = hallRepository;
        _hallUnitOfWork = hallUnitOfWork;
        _favorResolver = favorResolver;
    }

    public async Task<Guid> AddHall(HallCreateDto hall)
    {
        var favors = await _favorResolver.ResolveOrThrowAsync(hall.Favors);

        var hallEntity = new HallEntity
        {
            Persons = hall.Persons,
            Price = hall.Price,
            Name = hall.Name,
            Favors = favors.Select(f => new HallFavorEntity { FavorId = f.Id }).ToList()
        };

        await _hallRepository.AddAsync(hallEntity);

        try
        {
            await _hallUnitOfWork.SaveChangesAsync();
        }
        catch (UniqueConstraintException ex)
        {
            throw new HallNameAlreadyExistsException(hallEntity.Name, ex);
        }

        return hallEntity.Id;
    }

    public async Task UpdateHall(UpdateHallDto request)
    {
        var hall = await GetHall(request.Id);
        var favors = await _favorResolver.ResolveOrThrowAsync(request.Favors);

        hall.Persons = request.Persons;
        hall.Price = request.Price;

        hall.Favors.Clear();
        foreach (var favor in favors)
        {
            hall.Favors.Add(new HallFavorEntity { HallId = hall.Id, FavorId = favor.Id });
        }

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
            throw new NotFoundException(
                $"No halls available for {request.Persons} persons from {request.StartAt:yyyy-MM-dd HH:mm} to {request.EndAt:yyyy-MM-dd HH:mm}.");

        return halls.Select(h => h.Id).ToList();
    }

    private async Task<HallEntity> GetHall(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        return hall ?? throw new NotFoundException($"Hall {hallId} not found");
    }
}
