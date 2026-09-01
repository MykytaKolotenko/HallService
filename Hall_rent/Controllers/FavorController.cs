using FluentValidation;
using Hall_rent.Mappers;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

/// <summary>
/// Catalog of additional services (Favors) that can be offered in halls and added to a booking.
/// </summary>
[ApiController]
[Route("[controller]")]
public class FavorController : ControllerBase
{
    private readonly IValidator<FavorCreateRequest> _createValidator;
    private readonly IFavorService _favorService;
    private readonly IValidator<FavorUpdateRequest> _updateValidator;

    public FavorController(
        IFavorService favorService,
        IValidator<FavorUpdateRequest> updateValidator,
        IValidator<FavorCreateRequest> createValidator
    )
    {
        _favorService = favorService;
        _updateValidator = updateValidator;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Returns all favors.
    /// </summary>
    [HttpGet(Name = "GetFavors")]
    [ProducesResponseType(typeof(List<FavorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FavorResponse>>> GetFavors()
    {
        var favors = await _favorService.GetFavors();

        return Ok(favors);
    }

    /// <summary>
    /// Creates a new favor.
    /// </summary>
    ///  /// <remarks>
    /// Full data validation is performed by the service.
    /// </remarks>
    [HttpPost(Name = "CreateFavor")]
    [ProducesResponseType(typeof(FavorCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FavorCreateResponse>> CreateFavors([FromBody] FavorCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        var response = await _favorService.AddFavor(FavorMapper.ToDto(request));

        return Created(nameof(CreateFavors), response);
    }

    /// <summary>
    /// Updates the name and price of an existing service.
    /// </summary>
    /// <remarks>
    /// Changing a service price does not recalculate already created bookings — they store
    /// a price snapshot taken at booking time (see HallBookingFavorEntity.PriceAtBooking / FavorMapper.ToEntity).
    /// </remarks>
    [HttpPatch("{id}", Name = "UpdateFavor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFavors(Guid id, [FromBody] FavorUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        await _favorService.UpdateFavor(FavorMapper.ToDto(request, id));

        return Ok();
    }
}
