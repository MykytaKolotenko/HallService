using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

[ApiController]
[Route("[controller]")]
public class FavorController : ControllerBase
{
    private readonly IFavorService _favorService;

    public FavorController(IFavorService favorService)
    {
        _favorService = favorService;
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
        Guid id = await _favorService.AddFavour(request);

        return CreatedAtAction(nameof(CreateFavours), new { id }, id);
    }

    [HttpPatch("{id}", Name = "UpdateFavour")]
    public async Task<IActionResult> UpdateFavours(Guid id, [FromBody] FavorUpdateRequest request)
    {
        var updateFavourData = new UpdateFavorDto(id, request.Name, request.Price);
        await _favorService.UpdateFavour(updateFavourData);

        return Ok();
    }

    [HttpDelete("{id}", Name = "DeleteFavour")]
    public async Task<IActionResult> DeleteFavours(Guid id)
    {
        await _favorService.DeleteFavour(id);

        return Ok();
    }
}