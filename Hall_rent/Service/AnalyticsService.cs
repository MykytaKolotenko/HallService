using Hall_rent.Dto;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service.Interface;

namespace Hall_rent.Service;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsService(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<RevenueReportResponse> GetRevenueReportAsync(DateRangeDto request)
    {
        var rowReport = await _analyticsRepository.GetByPeriodAsync(request.From, request.To);
        var totalBookings = rowReport.Sum(x => x.BookingsCount);
        var totalRevenue = rowReport.Sum(x => x.Revenue);
        var report = rowReport.Select(r => new HallRevenueResponse
            {
                Day = r.Day,
                BookingsCount = r.BookingsCount,
                Revenue = r.Revenue
            }
        ).ToList();

        return new RevenueReportResponse { TotalRevenue = totalRevenue, TotalBookings = totalBookings, RevenuePerDay = report };
    }

    public async Task<FavorReportResponse> GetTopFavorsAsync(DateRangeDto request, int limit = 10)
    {
        var rowReport = await _analyticsRepository.GetTopFavorsAsync(request.From, request.To, limit);
        var totalBookings = rowReport.Sum(x => x.BookingsCount);
        var totalRevenue = rowReport.Sum(x => x.Revenue);
        var report = rowReport.Select(r => new FavorRevenueResponse
            {
                Id = r.Id,
                BookingsCount = r.BookingsCount,
                Name = r.Name,
                Revenue = r.Revenue
            }
        ).ToList();

        return new FavorReportResponse { Revenue = totalRevenue, BookingsCount = totalBookings, Favors = report };
    }
}