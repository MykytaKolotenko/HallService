using System.Data;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Helpers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;

namespace Hall_rent.Service;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFavorResolver _favorResolver;
    private readonly IHallRepository _hallRepository;
    private readonly IHallUnitOfWork _unitOfWork;

    public BookingService(
        IHallRepository hallRepository,
        IBookingRepository bookingRepository,
        IHallUnitOfWork unitOfWork,
        IFavorResolver favorResolver)
    {
        _hallRepository = hallRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _favorResolver = favorResolver;
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
        var hall = await _hallRepository.GetByIdAsync(request.HallId)
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

        var favors = await _favorResolver.ResolveOrThrowAsync(request.Favors?.Distinct().ToList());

        var booking = new HallBookingEntity
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
