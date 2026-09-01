using FluentValidation;
using Hall_rent.Mappers;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

/// <summary>
/// Books hall.
/// </summary>
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

    /// <summary>
    /// Books a hall for the specified interval with the given set of additional services (favors).
    /// </summary>
    /// <remarks>
    /// The operation runs in a transaction with Serializable isolation level (see BookingService.BookAsync)
    /// to prevent a race condition when two parallel requests try to book the same hall for overlapping
    /// time ranges at the same time. Possible reasons for failure: the hall was not found, capacity
    /// (Persons) was exceeded, the interval is already occupied, or one of the specified Favors
    /// is not offered by this hall.
    /// </remarks>
    [HttpPost("{hallId}", Name = "BookHall")]
    [ProducesResponseType(typeof(HallBookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HallBookResponse>> BookHall(Guid hallId, [FromBody] HallBookRequest request)
    {
        await ValidatorUtils.Validate(_bookValidator, request);

        var bookingResponse = await _bookingService.BookAsync(HallMapper.ToDto(request, hallId));

        return Ok(bookingResponse);
    }
}
