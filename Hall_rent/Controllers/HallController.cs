using FluentValidation;
using Hall_rent.Mappers;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

/// <summary>
/// CRUD operations and hall search (Hall). Hall booking is handled in a separate <see cref="BookingController"/>.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HallController : ControllerBase
{
    private readonly IValidator<HallCreateRequest> _createValidator;
    private readonly IHallService _hallService;
    private readonly IValidator<HallSearchRequest> _searchValidator;
    private readonly IValidator<HallUpdateRequest> _updateValidator;

    public HallController(
        IHallService hallService,
        IValidator<HallCreateRequest> createValidator,
        IValidator<HallSearchRequest> searchValidator,
        IValidator<HallUpdateRequest> updateValidator
    )
    {
        _hallService = hallService;
        _createValidator = createValidator;
        _searchValidator = searchValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Creates a new hall.
    /// </summary>
    /// <remarks>
    /// The hall name must be unique — if a conflict occurs, <see cref="Hall_rent.Exceptions.HallNameAlreadyExistsException"/>
    /// is translated by the exception middleware into a 409 Conflict (see UnitOfWork/UniqueConstraintExceptionFactory).
    /// </remarks>
    [HttpPost(Name = "AddHall")]
    [ProducesResponseType(typeof(HallCreateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HallCreateResponse>> CreateHall([FromBody] HallCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        var response = await _hallService.CreateHall(HallMapper.ToDto(request));

        return Ok(response);
    }

    /// <summary>
    /// Updates the hall data (name, capacity, price, and the full list of provided services).
    /// </summary>
    /// <remarks>
    /// Despite using the HTTP PATCH verb, the semantics are a full replacement: the Favors list
    /// in the request completely replaces the hall's current set of services (see HallService.UpdateHall),
    /// rather than merging with it.
    /// </remarks>
    [HttpPatch("{id}", Name = "UpdateHall")]
    [ProducesResponseType(typeof(UpdateHallResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateHallResponse>> PatchHall(Guid id, [FromBody] HallUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        var response = await _hallService.UpdateHall(HallMapper.ToDto(request, id));

        return Ok(response);
    }

    /// <summary>
    /// Deletes a hall.
    /// </summary>
    /// <remarks>
    /// WARNING: deleting a hall that already has bookings will affect analytics
    /// (see the "There will be bug with deleting hall in analytics" note in HallService.DeleteHall) —
    /// historical Bookings/Favors for that hall will remain, but the hall itself will no longer resolve.
    /// </remarks>
    [HttpDelete("{id}", Name = "DeleteHall")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHall(Guid id)
    {
        await _hallService.DeleteHall(id);

        return Ok();
    }

    /// <summary>
    /// Finds the IDs of halls that are available for the specified time interval and can accommodate
    /// the given number of persons.
    /// </summary>
    /// <remarks>
    /// Returns IDs only (see <see cref="HallSearchResponse"/>), not full hall details — if the client
    /// needs more information, it must request a specific hall separately.
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(HallSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HallSearchResponse>> SearchHalls([FromQuery] HallSearchRequest request)
    {
        await ValidatorUtils.Validate(_searchValidator, request);

        var halls = await _hallService.SearchAvailableHallIdsAsync(HallMapper.ToDto(request));

        return Ok(halls);
    }
}
