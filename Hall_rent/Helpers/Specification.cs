using System.Linq.Expressions;
using Hall_rent.Entity;

namespace Hall_rent.Helpers;

public static class Specification
{
    public static Expression<Func<HallBookingEntity, bool>> OverlapsBooking(
        Guid hallId, DateTime from, DateTime to)
    {
        return b => b.HallId == hallId && b.From < to && b.To > from;
    }
}