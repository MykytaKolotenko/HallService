using FluentValidation;
using Hall_rent.Context;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Helpers;
using Hall_rent.Repository;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent;

public static class InfrastructureDi
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddValidation();
        services.AddRepository();
        services.AddExceptions();
        services.AddServices();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();

        return services;
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<HallCreateRequestValidator>();
    }

    private static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IFavorRepository, FavorRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        services.AddScoped<IFavorResolver, FavorResolver>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IFavorService, FavorService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IClock, SystemClock>();
    }

    private static void AddExceptions(this IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddSingleton<ValidationExceptionResolver>();
        services.AddSingleton<SerializationConflictResolver>();
        services.AddSingleton<UniqueViolationResolver>();
        services.AddSingleton<AppExceptionResolver>();
        services.AddSingleton<FallbackExceptionResolver>();

        services.AddSingleton<ExceptionDispatcher>(sp => new ExceptionDispatcher([
            sp.GetRequiredService<ValidationExceptionResolver>(),
            sp.GetRequiredService<SerializationConflictResolver>(),
            sp.GetRequiredService<UniqueViolationResolver>(),
            sp.GetRequiredService<AppExceptionResolver>(),
            sp.GetRequiredService<FallbackExceptionResolver>()
        ]));
    }
}