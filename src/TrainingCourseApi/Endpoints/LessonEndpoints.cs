using Microsoft.EntityFrameworkCore;
using TrainingCourseApi.Contracts;
using TrainingCourseApi.Data;
using TrainingCourseApi.Mapping;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Endpoints;

public static class LessonEndpoints
{
    public static IEndpointRouteBuilder MapLessonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses/{courseId:int}/lessons").WithTags("Lessons");

        group.MapGet("/", async (int courseId, AppDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.AsNoTracking().AnyAsync(c => c.Id == courseId, ct);
            if (!course)
            {
                return Results.NotFound(new { message = $"Курс с id={courseId} не найден." });
            }

            var lessons = await db.Lessons
                .AsNoTracking()
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ThenBy(l => l.Id)
                .ToListAsync(ct);

            return Results.Ok(lessons.Select(l => l.ToResponse()).ToList());
        })
        .WithName("GetCourseLessons")
        .WithSummary("Список уроков курса")
        .Produces<List<LessonResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{lessonId:int}", async (int courseId, int lessonId, AppDbContext db, CancellationToken ct) =>
        {
            var lesson = await db.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId, ct);

            return lesson is null
                ? Results.NotFound(new { message = $"Урок с id={lessonId} в курсе {courseId} не найден." })
                : Results.Ok(lesson.ToResponse());
        })
        .WithName("GetCourseLessonById")
        .WithSummary("Получить урок курса по идентификатору")
        .Produces<LessonResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (int courseId, LessonCreateRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidationHelper.Validate(request) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var courseExists = await db.Courses.AsNoTracking().AnyAsync(c => c.Id == courseId, ct);
            if (!courseExists)
            {
                return Results.NotFound(new { message = $"Курс с id={courseId} не найден." });
            }

            int order;
            if (request.Order is { } explicitOrder)
            {
                order = explicitOrder;
            }
            else
            {
                var maxOrder = await db.Lessons
                    .Where(l => l.CourseId == courseId)
                    .Select(l => (int?)l.Order)
                    .MaxAsync(ct);

                order = (maxOrder ?? 0) + 1;
            }

            var lesson = new Lesson
            {
                CourseId = courseId,
                Title = request.Title.Trim(),
                Content = request.Content?.Trim(),
                DurationMinutes = request.DurationMinutes,
                Order = order,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Lessons.Add(lesson);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/courses/{courseId}/lessons/{lesson.Id}", lesson.ToResponse());
        })
        .WithName("CreateCourseLesson")
        .WithSummary("Добавить урок в курс")
        .Produces<LessonResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{lessonId:int}", async (int courseId, int lessonId, LessonUpdateRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidationHelper.Validate(request) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId, ct);
            if (lesson is null)
            {
                return Results.NotFound(new { message = $"Урок с id={lessonId} в курсе {courseId} не найден." });
            }

            lesson.Title = request.Title.Trim();
            lesson.Content = request.Content?.Trim();
            lesson.DurationMinutes = request.DurationMinutes;

            if (request.Order is { } order)
            {
                lesson.Order = order;
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(lesson.ToResponse());
        })
        .WithName("UpdateCourseLesson")
        .WithSummary("Обновить урок курса")
        .Produces<LessonResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{lessonId:int}", async (int courseId, int lessonId, AppDbContext db, CancellationToken ct) =>
        {
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId, ct);
            if (lesson is null)
            {
                return Results.NotFound(new { message = $"Урок с id={lessonId} в курсе {courseId} не найден." });
            }

            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteCourseLesson")
        .WithSummary("Удалить урок курса")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}