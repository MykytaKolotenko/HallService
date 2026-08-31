using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Mappers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service.Interface;

namespace Hall_rent.Service;

public class HallService : IHallService
{
    private readonly IFavorResolver _favorResolver;
    private readonly IHallRepository _hallRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HallService(
        IHallRepository hallRepository,
        IUnitOfWork unitOfWork,
        IFavorResolver favorResolver)
    {
        _hallRepository = hallRepository;
        _unitOfWork = unitOfWork;
        _favorResolver = favorResolver;
    }

    public async Task<HallCreateResponse> CreateHall(HallCreateDto hall)
    {
        var favors = await _favorResolver.ResolveOrThrowAsync(hall.Favors);
        var hallEntity = HallMapper.CreateDtoToEntity(hall, favors);

        await _hallRepository.AddAsync(hallEntity);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (UniqueConstraintException ex)
        {
            throw new HallNameAlreadyExistsException(hallEntity.Name, ex);
        }

        return new HallCreateResponse(hallEntity.Id);
    }

    public async Task<UpdateHallResponse> UpdateHall(UpdateHallDto request)
    {
        var hall = await GetHall(request.Id);
        var favors = await _favorResolver.ResolveOrThrowAsync(request.Favors);

        hall.Persons = request.Persons;
        hall.Price = request.Price;
        hall.Name = request.Name;

        hall.Favors.Clear();
        foreach (var favor in favors)
        {
            hall.Favors.Add(new HallFavorEntity { HallId = hall.Id, FavorId = favor.Id });
        }

        await _unitOfWork.SaveChangesAsync();

        return new UpdateHallResponse(hall.Id);
    }

    //There will be bug with deleting hall in analytics
    public async Task DeleteHall(Guid id)
    {
        var hall = await GetHall(id);
        _hallRepository.Remove(hall);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<HallSearchResponse> SearchAvailableHallIdsAsync(HallSearchDto request)
    {
        var halls = await _hallRepository.FindAvailableHallsAsync(request.From, request.To, request.Persons);

        if (halls.Count == 0)
            throw new NotFoundException(
                $"No halls available for {request.Persons} persons from {request.From:yyyy-MM-dd HH:mm} to {request.To:yyyy-MM-dd HH:mm}.");

        return new HallSearchResponse(halls.Select(h => h.Id).ToList());
    }

    private async Task<HallEntity> GetHall(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdWithFavorsAsync(hallId);
        return hall ?? throw new NotFoundException($"Hall {hallId} not found");
    }
}