using FluentValidation;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

[ApiController]
[Route("[controller]")]
public class HallController : ControllerBase
{
    private readonly IValidator<HallBookRequest> _bookValidator;
    private readonly IBookingService _bookingService;
    private readonly IValidator<HallCreateRequest> _createValidator;
    private readonly IHallService _hallService;
    private readonly IValidator<HallSearchRequest> _searchValidator;
    private readonly IValidator<HallUpdateRequest> _updateValidator;

    public HallController(
        IHallService hallService,
        IBookingService bookingService,
        IValidator<HallCreateRequest> createValidator,
        IValidator<HallSearchRequest> searchValidator,
        IValidator<HallUpdateRequest> updateValidator,
        IValidator<HallBookRequest> bookValidator
    )
    {
        _hallService = hallService;
        _bookingService = bookingService;
        _createValidator = createValidator;
        _searchValidator = searchValidator;
        _updateValidator = updateValidator;
        _bookValidator = bookValidator;
    }

    [HttpPost(Name = "AddHall")]
    public async Task<ActionResult<Guid>> CreateHall([FromBody] HallCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        Guid id = await _hallService.AddHall(new HallCreateDto
            {
                Name = request.Name,
                Price = request.Price,
                Persons = request.Persons,
                Favors = request.Favors
            }
        );

        return Ok(new { id });
    }

    [HttpPatch("{id}", Name = "UpdateHall")]
    public async Task<ActionResult<Guid>> PatchHall(Guid id, [FromBody] HallUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        await _hallService.UpdateHall(new UpdateHallDto
            {
                Id = id,
                Price = request.Price,
                Persons = request.Persons,
                Favors = request.Favors
            }
        );

        return Ok();
    }

    [HttpDelete("{id}", Name = "DeleteHall")]
    public async Task<IActionResult> DeleteHall(Guid id)
    {
        await _hallService.DeleteHall(id);

        return Ok();
    }

    [HttpPost("{hallId}/book", Name = "BookHall")]
    public async Task<ActionResult<HallBookResponse>> BookHall(Guid hallId, [FromBody] HallBookRequest request)
    {
        await ValidatorUtils.Validate(_bookValidator, request);

        var bookingResponse = await _bookingService.BookAsync(new BookHallDto
            {
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Favors = request.Favors,
                HallId = hallId,
                Persons = request.Persons
            }
        );

        return Ok(bookingResponse);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Guid>>> SearchHalls([FromQuery] HallSearchRequest request)
    {
        await ValidatorUtils.Validate(_searchValidator, request);

        var halls = await _hallService.FindAvailableHallIdsAsync(new HallSearchDto
            {
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Persons = request.Persons
            }
        );

        return Ok(halls);
    }
}