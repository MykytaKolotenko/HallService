using FluentValidation;
using Hall_rent.Mappers;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
    private readonly IValidator<HallBookRequest> _bookValidator;
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService, IValidator<HallBookRequest> bookValidator)
    {
        _bookingService = bookingService;
        _bookValidator = bookValidator;
    }

    [HttpPost("{hallId}", Name = "BookHall")]
    public async Task<ActionResult<HallBookResponse>> BookHall(Guid hallId, [FromBody] HallBookRequest request)
    {
        await ValidatorUtils.Validate(_bookValidator, request);

        var bookingResponse = await _bookingService.BookAsync(HallMapper.ToDto(request, hallId));

        return Ok(bookingResponse);
    }
}