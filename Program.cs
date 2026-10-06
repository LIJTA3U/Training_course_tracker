var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var courses = new List<Course>
{
    new(1, "C# и .NET", 72, 35, false),
    new(2, "Базы данных", 48, 20, false),
    new(3, "Docker", 24, 24, true)
};

app.MapGet("/api/courses", () => Results.Ok(courses));

app.MapGet("/api/courses/{id:int}", (int id) =>
{
    var course = courses.FirstOrDefault(c => c.Id == id);
    return course is null ? Results.NotFound() : Results.Ok(course);
});

app.MapGet("/", () => Results.Ok(new
{
    message = "Course Tracker API is running",
    endpoints = new[] { "/api/courses", "/api/courses/{id}" }
}));

app.Run("http://0.0.0.0:8080");

public record Course(
    int Id,
    string Name,
    int TotalHours,
    int CompletedHours,
    bool Completed);
