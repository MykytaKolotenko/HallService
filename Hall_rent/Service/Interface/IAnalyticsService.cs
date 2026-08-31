using Hall_rent.Dto;
using Hall_rent.Response;

namespace Hall_rent.Service.Interface;

public interface IAnalyticsService
{
    public Task<RevenueReportResponse> GetRevenueReportAsync(DateRangeDto request);
    public Task<FavorsReportResponse> GetTopFavorsAsync(DateRangeDto request, int limit);
}