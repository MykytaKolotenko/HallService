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

    [HttpPost(Name = "AddHall")]
    public async Task<ActionResult<HallCreateResponse>> CreateHall([FromBody] HallCreateRequest request)
    {
        await ValidatorUtils.Validate(_createValidator, request);
        var response = await _hallService.CreateHall(HallMapper.ToDto(request));

        return Ok(response);
    }

    [HttpPatch("{id}", Name = "UpdateHall")]
    public async Task<ActionResult<UpdateHallResponse>> PatchHall(Guid id, [FromBody] HallUpdateRequest request)
    {
        await ValidatorUtils.Validate(_updateValidator, request);
        var response = await _hallService.UpdateHall(HallMapper.ToDto(request, id));

        return Ok(response);
    }

    [HttpDelete("{id}", Name = "DeleteHall")]
    public async Task<IActionResult> DeleteHall(Guid id)
    {
        await _hallService.DeleteHall(id);

        return Ok();
    }

    [HttpGet("search")]
    public async Task<ActionResult<HallSearchResponse>> SearchHalls([FromQuery] HallSearchRequest request)
    {
        await ValidatorUtils.Validate(_searchValidator, request);

        var halls = await _hallService.SearchAvailableHallIdsAsync(HallMapper.ToDto(request));

        return Ok(halls);
    }
}