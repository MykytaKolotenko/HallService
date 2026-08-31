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

    [HttpGet(Name = "GetFavors")]
    public async Task<ActionResult<List<FavorResponse>>> GetFavors()
    {
        var favors = await _favorService.GetFavors();

        return Ok(favors);
    }

    [HttpPost(Name = "CreateFavor")]
    public async Task<ActionResult<FavorCreateResponse>> CreateFavors([FromBody] FavorCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        var response = await _favorService.AddFavor(request);

        return CreatedAtAction(nameof(CreateFavors), response);
    }

    [HttpPatch("{id}", Name = "UpdateFavor")]
    public async Task<IActionResult> UpdateFavors(Guid id, [FromBody] FavorUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        await _favorService.UpdateFavor(new UpdateFavorDto
        {
            Id = id,
            Name = request.Name,
            Price = request.Price
        });

        return Ok();
    }

    [HttpDelete("{id}", Name = "DeleteFavor")]
    public async Task<IActionResult> DeleteFavors(Guid id)
    {
        await _favorService.DeleteFavor(id);

        return Ok();
    }
}
