using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

[ApiController]
[Route("[controller]")]
public class HallController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallController(IHallService hallService)
    {
        _hallService = hallService;
    }

    [HttpPost(Name = "AddHall")]
    public async Task<ActionResult<Guid>> CreateHall([FromBody] HallCreateRequest request)
    {
        Guid id = await _hallService.AddHall(request);

        return CreatedAtAction(nameof(CreateHall), new { id }, id);
    }

    [HttpPatch("{id}", Name = "UpdateHall")]
    public async Task<ActionResult<Guid>> PatchHall(Guid id, [FromBody] HallUpdateRequest request)
    {
        var updateHallData = new UpdateHallDto(id, request.Price, request.Persons, request.Favors);
        await _hallService.UpdateHall(updateHallData);

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
        var bookHall = new BookHallDto(request.StartAt, request.EndAt, request.Favors, hallId);
        var bookingResponse = await _hallService.BookHall(bookHall);

        return CreatedAtAction(nameof(BookHall), bookingResponse);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Guid>>> SearchHalls([FromQuery] HallSearchRequest request)
    {
        var searchData = new HallSearchDto(request.StartAt, request.EndAt, request.Persons);
        var halls = await _hallService.FindAvailableHallIdsAsync(searchData);

        return Ok(halls);
    }
}