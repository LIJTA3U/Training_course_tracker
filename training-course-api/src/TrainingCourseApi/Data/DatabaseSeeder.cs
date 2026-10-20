using Microsoft.EntityFrameworkCore;
using TrainingCourseApi.Data;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Data;

/// <summary>
/// Необязательное наполнение БД демонстрационными данными.
/// Включается флагом Database:SeedOnStartup (env: Database__SeedOnStartup=true).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Courses.AnyAsync(ct))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var courses = new List<Course>
        {
            new()
            {
                Title = "Основы C# и .NET",
                Description = "Вводный курс по языку C# и платформе .NET: типы, ООП, асинхронность.",
                Instructor = "Иван Петров",
                Category = CourseCategory.Programming,
                Credits = 6,
                StartDate = new DateOnly(2026, 9, 1),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                Lessons =
                [
                    new() { Title = "Введение и настройка среды", DurationMinutes = 45, Order = 1 },
                    new() { Title = "Типы данных и переменные", DurationMinutes = 60, Order = 2 },
                    new() { Title = "Асинхронное программирование", DurationMinutes = 75, Order = 3 }
                ],
                Enrollments =
                [
                    new()
                    {
                        StudentName = "Анна Смирнова",
                        StudentEmail = "anna.smirnova@example.com",
                        Status = EnrollmentStatus.Active,
                        ProgressPercent = 40,
                        EnrolledAt = now
                    }
                ]
            },
            new()
            {
                Title = "SQL и проектирование баз данных",
                Description = "Реляционные модели, нормализация, индексы и работа с PostgreSQL.",
                Instructor = "Мария Кузнецова",
                Category = CourseCategory.DataScience,
                Credits = 4,
                StartDate = new DateOnly(2026, 9, 15),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                Lessons =
                [
                    new() { Title = "Моделирование данных", DurationMinutes = 60, Order = 1 },
                    new() { Title = "Индексы и производительность", DurationMinutes = 90, Order = 2 }
                ]
            },
            new()
            {
                Title = "Docker для разработчиков",
                Description = "Контейнеры, Dockerfile, docker compose и сетевые взаимодействия.",
                Instructor = "Пётр Сидоров",
                Category = CourseCategory.Programming,
                Credits = 3,
                StartDate = new DateOnly(2026, 10, 1),
                IsActive = false,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await db.Courses.AddRangeAsync(courses, ct);
        await db.SaveChangesAsync(ct);
    }
}