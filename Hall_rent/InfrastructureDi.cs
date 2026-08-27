using Hall_rent.Repository;
using Hall_rent.Repository.Hall;
using Hall_rent.Service;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IHallUnitOfWork, HallUnitOfWork>();
        services.AddScoped<HallService>();

        return services;
    }
}
