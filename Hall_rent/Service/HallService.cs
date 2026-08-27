using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Repository.Hall;
using Hall_rent.Request;

namespace Hall_rent.Service;

public class HallService
{
    private readonly IHallRepository _hallRepository;
    private readonly IHallUnitOfWork _hallUnitOfWork;
    private readonly IBookingRepository _bookingRepository;

    public HallService(IHallRepository hallHallRepository, IHallUnitOfWork hallHallUnitOfWork, IBookingRepository bookingRepository)
    {
        _hallRepository = hallHallRepository;
        _hallUnitOfWork = hallHallUnitOfWork;
        _bookingRepository = bookingRepository;
    }

    public async Task<Guid> AddHall(HallCreateRequest hall)
    {
        HallEntity hallEntity = new HallEntity(hall.Persons, hall.Price, hall.Favors);

        await _hallRepository.AddAsync(hallEntity);
        await _hallUnitOfWork.SaveChangesAsync();

        return hallEntity.Id;
    }

    public async Task UpdateHall(UpdateHallDto request)
    {
        var hall = await GetHall(request.Id);

        hall.Persons = request.Persons;
        hall.Price = request.Price;
        hall.Favors = request.Favors;

        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task DeleteHall(Guid id)
    {
        var hall = await GetHall(id);

        _hallRepository.Remove(hall);
        await _hallUnitOfWork.SaveChangesAsync();
    }

    public async Task<Decimal> BookHall(BookHallDto request)
    {
        var hall = await GetHall(request.HallId);

        //TODO add check to availability

        var booking = new HallBookingEntity
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Favors = hall.Favors,
        };

        await _bookingRepository.AddAsync(booking);
        await _hallUnitOfWork.SaveChangesAsync();

        //TODO add price calculation
        return hall.Price;
    }

    public async Task<List<Guid>> FindAvailableHallIdsAsync(HallSearchDto request)
    {
        var halls = await _hallRepository.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons);

        return halls.Select(h => h.Id).ToList();
    }

    private async Task<HallEntity> GetHall(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);

        return hall ?? throw new KeyNotFoundException($"Hall {hallId} not found");
    }
}