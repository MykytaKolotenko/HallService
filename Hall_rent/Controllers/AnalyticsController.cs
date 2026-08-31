using FluentValidation;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IValidator<DateRangeRequest> _dateRangeValidator;

    public AnalyticsController(IAnalyticsService analyticsService, IValidator<DateRangeRequest> dateRangeValidator)
    {
        _analyticsService = analyticsService;
        _dateRangeValidator = dateRangeValidator;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportResponse>> GetRevenue([FromQuery] DateRangeRequest request)
    {
        await ValidatorUtils.Validate(_dateRangeValidator, request);

        return Ok(await _analyticsService.GetRevenueReportAsync(new DateRangeDto { From = request.From, To = request.To }));
    }

    [HttpGet("favors/top")]
    public async Task<ActionResult<FavorResponse>> GetTopFavors([FromQuery] DateRangeRequest request, [FromQuery] int limit = 10)
    {
        await ValidatorUtils.Validate(_dateRangeValidator, request);

        return Ok(await _analyticsService.GetTopFavorsAsync(new DateRangeDto { From = request.From, To = request.To }, limit));
    }
}
