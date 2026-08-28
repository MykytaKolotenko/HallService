using FluentValidation;
using FluentValidation.AspNetCore;
using Hall_rent.Context;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Hall_rent.Validation;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent;

public static class InfrastructureDi
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        AddValidation(services);
        AddRepository(services);
        AddExceptions(services);
        AddServices(services);

        services.AddScoped<IHallUnitOfWork, HallUnitOfWork>();

        return services;
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<FavorUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<FavorCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<HallCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<HallUpdateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<HallBookRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<HallSearchRequestValidator>();
        services.AddFluentValidationAutoValidation();
    }

    private static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IFavorRepository, FavorRepository>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IFavorService, FavorService>();
    }

    private static void AddExceptions(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();

        services.AddSingleton<SerializationConflictResolver>();
        services.AddSingleton<AppExceptionResolver>();
        services.AddSingleton<FallbackExceptionResolver>();

        services.AddSingleton<ExceptionDispatcher>(sp => new ExceptionDispatcher([
            sp.GetRequiredService<SerializationConflictResolver>(),
            sp.GetRequiredService<AppExceptionResolver>(),
            sp.GetRequiredService<FallbackExceptionResolver>()
        ]));
    }
}
