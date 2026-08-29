using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service;

public interface IBookingService
{
    Task<HallBookResponse> BookAsync(BookHallDto request);
}