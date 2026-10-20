using Microsoft.EntityFrameworkCore;
using TrainingCourseApi.Contracts;
using TrainingCourseApi.Data;
using TrainingCourseApi.Mapping;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Endpoints;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses/{courseId:int}/enrollments").WithTags("Enrollments");

        // Сводка по всем курсам — верхнеуровневый маршрут.
        app.MapGet("/api/enrollments", async (AppDbContext db, CancellationToken ct) =>
        {
            var enrollments = await db.Enrollments
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ToListAsync(ct);

            return Results.Ok(enrollments.Select(e => e.ToResponse()).ToList());
        })
        .WithTags("Enrollments")
        .WithName("GetAllEnrollments")
        .WithSummary("Список всех записей на курсы")
        .Produces<List<EnrollmentResponse>>();

        group.MapGet("/", async (int courseId, AppDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.AsNoTracking().AnyAsync(c => c.Id == courseId, ct);
            if (!course)
            {
                return Results.NotFound(new { message = $"Курс с id={courseId} не найден." });
            }

            var enrollments = await db.Enrollments
                .AsNoTracking()
                .Where(e => e.CourseId == courseId)
                .OrderBy(e => e.Id)
                .ToListAsync(ct);

            return Results.Ok(enrollments.Select(e => e.ToResponse()).ToList());
        })
        .WithName("GetCourseEnrollments")
        .WithSummary("Список записей на курс")
        .Produces<List<EnrollmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{enrollmentId:int}", async (int courseId, int enrollmentId, AppDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.CourseId == courseId, ct);

            return enrollment is null
                ? Results.NotFound(new { message = $"Запись с id={enrollmentId} в курсе {courseId} не найдена." })
                : Results.Ok(enrollment.ToResponse());
        })
        .WithName("GetCourseEnrollmentById")
        .WithSummary("Получить запись на курс по идентификатору")
        .Produces<EnrollmentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (int courseId, EnrollmentCreateRequest request, AppDbContext db, CancellationToken ct) =>
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

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentName = request.StudentName.Trim(),
                StudentEmail = request.StudentEmail.Trim(),
                Status = request.Status,
                ProgressPercent = request.ProgressPercent,
                EnrolledAt = DateTimeOffset.UtcNow
            };

            db.Enrollments.Add(enrollment);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/courses/{courseId}/enrollments/{enrollment.Id}", enrollment.ToResponse());
        })
        .WithName("CreateCourseEnrollment")
        .WithSummary("Записать студента на курс")
        .Produces<EnrollmentResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{enrollmentId:int}", async (int courseId, int enrollmentId, EnrollmentUpdateRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidationHelper.Validate(request) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var enrollment = await db.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.CourseId == courseId, ct);

            if (enrollment is null)
            {
                return Results.NotFound(new { message = $"Запись с id={enrollmentId} в курсе {courseId} не найдена." });
            }

            enrollment.StudentName = request.StudentName.Trim();
            enrollment.StudentEmail = request.StudentEmail.Trim();
            enrollment.Status = request.Status;
            enrollment.ProgressPercent = request.ProgressPercent;

            await db.SaveChangesAsync(ct);

            return Results.Ok(enrollment.ToResponse());
        })
        .WithName("UpdateCourseEnrollment")
        .WithSummary("Обновить запись на курс")
        .Produces<EnrollmentResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{enrollmentId:int}", async (int courseId, int enrollmentId, AppDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.CourseId == courseId, ct);

            if (enrollment is null)
            {
                return Results.NotFound(new { message = $"Запись с id={enrollmentId} в курсе {courseId} не найдена." });
            }

            db.Enrollments.Remove(enrollment);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteCourseEnrollment")
        .WithSummary("Удалить запись на курс")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}