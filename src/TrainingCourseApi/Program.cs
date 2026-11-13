using System.Text.Json;
using System.Text.Json.Serialization;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TrainingCourseApi.Data;
using TrainingCourseApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TrainingDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задана строка подключения ConnectionStrings:TrainingDb. " +
        "Укажите её в appsettings.json или в переменной окружения ConnectionStrings__TrainingDb.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSnakeCaseNamingConvention();
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(5));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Трекер учебных курсов — API",
        Version = "v1",
        Description = "CRUD API для учёта учебных курсов, уроков и записей студентов. Хранилище — PostgreSQL 16."
    });

    var xml = Path.Combine(AppContext.BaseDirectory, "TrainingCourseApi.xml");
    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml);
    }
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrainingCourseApi v1");
    options.DocumentTitle = "Трекер учебных курсов — API";
});

app.MapGet("/", () => Results.Ok(new
{
    message = "Course Tracker API is running",
    version = "2.0",
    storage = "PostgreSQL",
    documentation = "/swagger",
    health = "/health",
    endpoints = new[]
    {
        "GET    /api/courses",
        "GET    /api/courses/{id}",
        "POST   /api/courses",
        "PUT    /api/courses/{id}",
        "PATCH  /api/courses/{id}/status",
        "DELETE /api/courses/{id}",
        "GET    /api/courses/{courseId}/lessons",
        "POST   /api/courses/{courseId}/lessons",
        "GET    /api/courses/{courseId}/lessons/{lessonId}",
        "PUT    /api/courses/{courseId}/lessons/{lessonId}",
        "DELETE /api/courses/{courseId}/lessons/{lessonId}",
        "GET    /api/courses/{courseId}/enrollments",
        "POST   /api/courses/{courseId}/enrollments",
        "GET    /api/courses/{courseId}/enrollments/{enrollmentId}",
        "PUT    /api/courses/{courseId}/enrollments/{enrollmentId}",
        "DELETE /api/courses/{courseId}/enrollments/{enrollmentId}",
        "GET    /api/enrollments"
    }
}))
.WithTags("Health")
.WithName("GetApiInfo")
.WithSummary("Информация об API и список маршрутов")
.Produces(StatusCodes.Status200OK);

app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
{
    var canConnect = await db.Database.CanConnectAsync(ct);

    return canConnect
        ? Results.Ok(new { status = "Healthy", database = "PostgreSQL", utc = DateTimeOffset.UtcNow })
        : Results.Json(
            new { status = "Unhealthy", database = "PostgreSQL", error = "Нет соединения с базой данных." },
            statusCode: StatusCodes.Status503ServiceUnavailable);
})
.WithTags("Health")
.WithName("HealthCheck")
.WithSummary("Проверка состояния API и подключения к БД")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status503ServiceUnavailable);

await InitializeDatabaseAsync(app);

app.MapCourseEndpoints();
app.MapLessonEndpoints();
app.MapEnrollmentEndpoints();

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    const int maxAttempts = 30;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Миграции PostgreSQL применены успешно.");

            if (app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
            {
                await DatabaseSeeder.SeedAsync(db);
                logger.LogInformation("Демонстрационные данные загружены.");
            }

            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning("БД недоступна (попытка {Attempt}/{MaxAttempts}): {Message}", attempt, maxAttempts, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None);
        }
    }

    throw new InvalidOperationException(
        $"Не удалось применить миграции после {maxAttempts} попыток подключения к PostgreSQL.");
}

/// <summary>Точка входа помечена явно, чтобы интеграционные тесты могли ссылаться на сборку.</summary>
public partial class Program;