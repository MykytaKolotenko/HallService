using System.Reflection;
using Hall_rent;
using Hall_rent.Middleware;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hall Rent API",
        Version = "v1",
        Description = "API для бронирования залов: поиск свободных залов, бронирование, " +
                      "услуги (favors) и аналитика по выручке."
    });

    // Подхватываем XML-комментарии (///-summary над контроллерами/экшенами/DTO), сгенерированные
    // за счёт <GenerateDocumentationFile> в csproj, — так они появляются в Swagger UI как описания.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // В маршрутах вида /Hall/{id} и /Favor/{id} экшены различаются по HTTP-методу, но Swagger
    // по умолчанию генерирует operationId только по имени экшена — включаем полный путь+метод,
    // чтобы избежать коллизий при кодогенерации клиентов.
    options.CustomSchemaIds(type => type.FullName);
});

builder.Services.SetUp();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hall Rent API v1"));
}

app.UseMiddleware<CustomExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();