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

    [HttpGet(Name = "GetFavours")]
    public async Task<ActionResult<List<FavorResponse>>> GetFavours()
    {
        var favours = await _favorService.GetFavours();

        return Ok(favours);
    }

    [HttpPost(Name = "CreateFavour")]
    public async Task<ActionResult<Guid>> CreateFavours([FromBody] FavorCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        Guid id = await _favorService.AddFavour(request);

        return CreatedAtAction(nameof(CreateFavours), new { id }, id);
    }

    [HttpPatch("{id}", Name = "UpdateFavour")]
    public async Task<IActionResult> UpdateFavours(Guid id, [FromBody] FavorUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        await _favorService.UpdateFavour(new UpdateFavorDto(id, request.Name, request.Price));

        return Ok();
    }

    [HttpDelete("{id}", Name = "DeleteFavour")]
    public async Task<IActionResult> DeleteFavours(Guid id)
    {
        await _favorService.DeleteFavour(id);

        return Ok();
    }
}