using System.Data;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Helpers;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFavorRepository _favorRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IHallUnitOfWork _unitOfWork;

    public BookingService(
        IHallRepository hallRepository,
        IBookingRepository bookingRepository,
        IFavorRepository favorRepository,
        IHallUnitOfWork unitOfWork)
    {
        _hallRepository = hallRepository;
        _bookingRepository = bookingRepository;
        _favorRepository = favorRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<HallBookResponse> BookAsync(BookHallDto request)
    {
        return _unitOfWork.RunInTransactionAsync(
            IsolationLevel.Serializable,
            () => BookInternalAsync(request),
            $"BookHall({request.HallId})");
    }

    private async Task<HallBookResponse> BookInternalAsync(BookHallDto request)
    {
        HallEntity hall = await _hallRepository.GetByIdAsync(request.HallId)
                          ?? throw new NotFoundException(
                              $"Hall {request.HallId} not found");

        if (request.Persons > hall.Persons)
            throw new HallCapacityExceededException(
                hall.Id,
                hall.Persons,
                request.Persons);

        if (!await _bookingRepository.IsHallAvailableAsync(
                request.HallId,
                request.StartAt,
                request.EndAt))
            throw new HallNotAvailableException(
                request.HallId,
                request.StartAt,
                request.EndAt);

        List<Guid> favorIds = request.Favors?.Distinct().ToList() ?? [];

        List<Guid> unsupported = favorIds
            .Except(hall.Favors)
            .ToList();

        if (unsupported.Count > 0)
            throw new FavorsNotOfferedException(
                hall.Id,
                unsupported);

        List<FavorEntity> favors = await _favorRepository.GetByIdsAsync(favorIds);

        if (favors.Count != favorIds.Count)
        {
            List<Guid> missing = favorIds
                .Except(favors.Select(x => x.Id))
                .ToList();

            throw new NotFoundException(
                $"Favors not found: {string.Join(", ", missing)}");
        }

        HallBookingEntity booking = new HallBookingEntity
        {
            HallId = hall.Id,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Favors = favors.Select(x => x.Id).ToList(),
            Price = FavorCalculator.Calculate(
                hall.Price,
                FavorMapper.ToDto(favors))
        };

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return new HallBookResponse
        {
            Id = booking.Id,
            Price = booking.Price
        };
    }
}
