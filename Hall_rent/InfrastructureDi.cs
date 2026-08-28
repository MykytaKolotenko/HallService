using Hall_rent.Context;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent;

public static class InfrastructureDi
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IFavorRepository, FavorRepository>();
        services.AddScoped<IHallUnitOfWork, HallUnitOfWork>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IFavorService, FavorService>();

        // ↓ вот этого не хватало — регистрируем сами классы-обработчики
        services.AddScoped<SerializationConflictHandler>();
        services.AddScoped<AppExceptionHandler>();
        services.AddScoped<FallbackExceptionHandler>();

        services.AddScoped<ExceptionDispatcher>(sp => new ExceptionDispatcher(new IExceptionHandler[]
        {
            sp.GetRequiredService<SerializationConflictHandler>(),
            sp.GetRequiredService<AppExceptionHandler>(),
            sp.GetRequiredService<FallbackExceptionHandler>(),
        }));

        return services;
    }
}
