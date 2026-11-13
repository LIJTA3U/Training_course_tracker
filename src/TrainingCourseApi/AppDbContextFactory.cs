using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TrainingCourseApi.Data;

namespace TrainingCourseApi;

/// <summary>
/// Фабрика контекста для design-time (генерация миграций: dotnet ef migrations add).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TrainingDb")
                               ?? "Host=localhost;Port=5432;Database=training_courses;Username=training_user;Password=training_password";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSnakeCaseNamingConvention()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}