using System.Data;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Helpers;
using Hall_rent.Mappers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service.Interface;

namespace Hall_rent.Service;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFavorResolver _favorResolver;
    private readonly IHallRepository _hallRepository;
    private readonly ITransactionRunner _transactionRunner;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(
        IHallRepository hallRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IFavorResolver favorResolver,
        ITransactionRunner transactionRunner)
    {
        _hallRepository = hallRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _favorResolver = favorResolver;
        _transactionRunner = transactionRunner;
    }

// Serializable is the strictest isolation level: it is needed so that two parallel requests
// trying to book the same hall for overlapping time ranges cannot both pass the
// IsHallAvailableAsync check (which is just a regular SELECT, not a lock) and then both insert
// a booking. One of the competing requests will get a serialization failure from SQL Server
// on commit, which SerializationConflictResolver turns into 409 ConcurrencyConflict — the client
// must be prepared to retry the request.
    public Task<HallBookResponse> BookAsync(BookHallDto request)
    {
        return _transactionRunner.RunInTransactionAsync(
            IsolationLevel.Serializable,
            () => BookInternalAsync(request));
    }

    private async Task<HallBookResponse> BookInternalAsync(BookHallDto request)
    {
        var hall = await _hallRepository.GetByIdWithFavorsAsync(request.HallId) ?? throw new NotFoundException($"Hall {request.HallId} not found");

        if (request.Persons > hall.Persons)
            throw new HallCapacityExceededException(hall.Id, hall.Persons, request.Persons);

        // Hall occupancy check for the [StartAt, EndAt) interval — see Specification.OverlapsBooking
        // for the interval overlap formula..
        var available = await _bookingRepository.IsHallAvailableAsync(hall.Id, request.StartAt, request.EndAt);

        if (!available) throw new HallNotAvailableException(hall.Id, request.StartAt, request.EndAt);

        var favorIds = request.Favors.Distinct().ToList();
        var offeredFavorIds = hall.Favors.Select(x => x.FavorId).ToHashSet();
        var notOfferedFavorIds = favorIds.Where(id => !offeredFavorIds.Contains(id)).ToList();

        if (notOfferedFavorIds.Count > 0)
            throw new FavorsNotOfferedException(hall.Id, notOfferedFavorIds);

        var favors = await _favorResolver.ResolveOrThrowAsync(favorIds);

        var booking = new HallBookingEntity
        {
            HallId = hall.Id,
            From = request.StartAt,
            To = request.EndAt,
            Price = FavorCalculator.Calculate(hall.Price, favors.Select(FavorMapper.ToDto).ToList())
        };

        // Each service price is "frozen" in HallBookingFavorEntity.PriceAtBooking at booking time
        // (see FavorMapper.ToEntity) — any later price changes in the service catalog must not retroactively
        // change the cost of already created bookings.
        booking.Favors = favors.Select(f => FavorMapper.ToEntity(f, booking)).ToList();

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return new HallBookResponse
        {
            Id = booking.Id,
            Price = booking.Price
        };
    }
}
