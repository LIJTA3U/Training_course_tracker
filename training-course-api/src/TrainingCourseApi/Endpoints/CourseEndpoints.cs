using Microsoft.EntityFrameworkCore;
using TrainingCourseApi.Contracts;
using TrainingCourseApi.Data;
using TrainingCourseApi.Mapping;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        // GET /api/courses — список с пагинацией, поиском и фильтрами.
        group.MapGet("/", async (
            AppDbContext db,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            CourseCategory? category = null,
            bool? isActive = null,
            CancellationToken ct = default) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.Courses
                .AsNoTracking()
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Where(c => EF.Functions.ILike(c.Title, pattern)
                                         || (c.Description != null && EF.Functions.ILike(c.Description, pattern))
                                         || EF.Functions.ILike(c.Instructor, pattern));
            }

            if (category is not null)
            {
                query = query.Where(c => c.Category == category);
            }

            if (isActive is not null)
            {
                query = query.Where(c => c.IsActive == isActive);
            }

            var total = await query.CountAsync(ct);

            var items = (await query
                    .OrderBy(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct))
                .Select(c => c.ToResponse())
                .ToList();

            return Results.Ok(new PagedResponse<CourseResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            });
        })
        .WithName("GetCourses")
        .WithSummary("Список курсов с пагинацией и фильтрами")
        .Produces<PagedResponse<CourseResponse>>();

        // GET /api/courses/{id} — один курс вместе с уроками и записями.
        group.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses
                .AsNoTracking()
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            return course is null
                ? Results.NotFound(new { message = $"Курс с id={id} не найден." })
                : Results.Ok(course.ToResponse());
        })
        .WithName("GetCourseById")
        .WithSummary("Получить курс по идентификатору")
        .Produces<CourseResponse>()
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/courses — создание курса.
        group.MapPost("/", async (CourseCreateRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidationHelper.Validate(request) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var now = DateTimeOffset.UtcNow;

            var course = new Course
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Instructor = request.Instructor.Trim(),
                Category = request.Category,
                Credits = request.Credits,
                StartDate = request.StartDate,
                IsActive = request.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/courses/{course.Id}", course.ToResponse());
        })
        .WithName("CreateCourse")
        .WithSummary("Создать курс")
        .Produces<CourseResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        // PUT /api/courses/{id} — полное обновление курса.
        group.MapPut("/{id:int}", async (int id, CourseUpdateRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (ValidationHelper.Validate(request) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null)
            {
                return Results.NotFound(new { message = $"Курс с id={id} не найден." });
            }

            course.Title = request.Title.Trim();
            course.Description = request.Description?.Trim();
            course.Instructor = request.Instructor.Trim();
            course.Category = request.Category;
            course.Credits = request.Credits;
            course.StartDate = request.StartDate;
            course.IsActive = request.IsActive;
            course.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(ct);

            var full = await db.Courses
                .AsNoTracking()
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .FirstAsync(c => c.Id == id, ct);

            return Results.Ok(full.ToResponse());
        })
        .WithName("UpdateCourse")
        .WithSummary("Обновить курс")
        .Produces<CourseResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        // PATCH /api/courses/{id}/status — быстрое переключение активности курса.
        group.MapPatch("/{id:int}/status", async (int id, bool isActive, AppDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course is null)
            {
                return Results.NotFound(new { message = $"Курс с id={id} не найден." });
            }

            course.IsActive = isActive;
            course.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);

            return Results.Ok(course.ToResponse());
        })
        .WithName("SetCourseStatus")
        .WithSummary("Активировать/деактивировать курс")
        .Produces<CourseResponse>()
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/courses/{id} — удаление курса вместе с уроками и записями.
        group.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var course = await db.Courses.FindAsync([id], ct);
            if (course is null)
            {
                return Results.NotFound(new { message = $"Курс с id={id} не найден." });
            }

            db.Courses.Remove(course);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteCourse")
        .WithSummary("Удалить курс")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}