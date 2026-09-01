using FluentValidation;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hall_rent.Controllers;

/// <summary>
/// Booking reports: revenue by day and top services by revenue for a given period.
/// </summary>
[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IValidator<DateRangeRequest> _dateRangeValidator;
    private readonly IValidator<AnalyticsTopFavorRequest> _topFavorValidator;

    public AnalyticsController(IAnalyticsService analyticsService,
        IValidator<DateRangeRequest> dateRangeValidator,
        IValidator<AnalyticsTopFavorRequest> topFavorValidator)
    {
        _analyticsService = analyticsService;
        _dateRangeValidator = dateRangeValidator;
        _topFavorValidator = topFavorValidator;
    }

    /// <summary>
    /// Revenue and booking count report, grouped by day, for the period [From, To).
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RevenueReportResponse>> GetRevenue([FromQuery] DateRangeRequest request)
    {
        await ValidatorUtils.Validate(_dateRangeValidator, request);

        return Ok(await _analyticsService.GetRevenueReportAsync(new DateRangeDto { From = request.From, To = request.To }));
    }

    /// <summary>
    /// Top services (favors) by revenue for the period [From, To), limited by the Limit parameter (1..100).
    /// </summary>
    [HttpGet("favors/top")]
    [ProducesResponseType(typeof(FavorReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FavorReportResponse>> GetTopFavors([FromQuery] AnalyticsTopFavorRequest request)
    {
        await ValidatorUtils.Validate(_topFavorValidator, request);

        return Ok(await _analyticsService.GetTopFavorsAsync(new DateRangeDto { From = request.From, To = request.To }, request.Limit));
    }
}
