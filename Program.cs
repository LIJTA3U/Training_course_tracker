using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=coursetracker;Username=postgres;Password=postgres";

builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();

    await db.Database.EnsureCreatedAsync();

    if (!await db.Courses.AnyAsync())
    {
        db.Courses.AddRange(
            new Course
            {
                Id = 1,
                Name = "C# и .NET",
                TotalHours = 72,
                CompletedHours = 35,
                Completed = false
            },
            new Course
            {
                Id = 2,
                Name = "Базы данных",
                TotalHours = 48,
                CompletedHours = 20,
                Completed = false
            },
            new Course
            {
                Id = 3,
                Name = "Docker",
                TotalHours = 24,
                CompletedHours = 24,
                Completed = true
            }
        );

        await db.SaveChangesAsync();
    }
}

app.MapGet("/api/courses", async (CourseDbContext db) =>
{
    var courses = await db.Courses
        .OrderBy(c => c.Id)
        .ToListAsync();

    return Results.Ok(courses);
});

app.MapGet("/api/courses/{id:int}", async (int id, CourseDbContext db) =>
{
    var course = await db.Courses.FindAsync(id);

    return course is null
        ? Results.NotFound()
        : Results.Ok(course);
});

app.MapGet("/", () => Results.Ok(new
{
    message = "Course Tracker API is running",
    endpoints = new[] { "/api/courses", "/api/courses/{id}" }
}));

app.Run("http://0.0.0.0:8080");

public class CourseDbContext(DbContextOptions<CourseDbContext> options)
    : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();
}

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int CompletedHours { get; set; }
    public bool Completed { get; set; }
}
