using Microsoft.AspNetCore.Mvc;

namespace Hall_rent;

public static class Settings
{
    public static void SetUp(this IServiceCollection services)
    {
        ValidationExceptionSettings(services);
        RoutingSettings(services);
    }

    private static void ValidationExceptionSettings(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage));

                    return new BadRequestObjectResult(new
                    {
                        title = "ValidationError",
                        status = 400,
                        errors,
                        traceId = context.HttpContext.TraceIdentifier
                    });
                };
            });
    }

    private static void RoutingSettings(this IServiceCollection services)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
    }
}
