using System.ComponentModel.DataAnnotations;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Contracts;

// ---------- Courses ----------

public class CourseCreateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title обязателен.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title: от 3 до 200 символов.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description: максимум 2000 символов.")]
    public string? Description { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Instructor обязателен.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Instructor: от 2 до 150 символов.")]
    public string Instructor { get; set; } = string.Empty;

    [EnumDataType(typeof(CourseCategory), ErrorMessage = "Неизвестная категория курса.")]
    public CourseCategory Category { get; set; } = CourseCategory.Other;

    [Range(0, 60, ErrorMessage = "Credits: от 0 до 60.")]
    public int Credits { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CourseUpdateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title обязателен.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title: от 3 до 200 символов.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description: максимум 2000 символов.")]
    public string? Description { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Instructor обязателен.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Instructor: от 2 до 150 символов.")]
    public string Instructor { get; set; } = string.Empty;

    [EnumDataType(typeof(CourseCategory), ErrorMessage = "Неизвестная категория курса.")]
    public CourseCategory Category { get; set; } = CourseCategory.Other;

    [Range(0, 60, ErrorMessage = "Credits: от 0 до 60.")]
    public int Credits { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Instructor { get; set; } = string.Empty;

    public CourseCategory Category { get; set; }

    public int Credits { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int LessonCount { get; set; }

    public int EnrollmentCount { get; set; }

    public double AverageProgress { get; set; }
}

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;
}

// ---------- Lessons ----------

public class LessonCreateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title обязателен.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title: от 2 до 200 символов.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Content: максимум 4000 символов.")]
    public string? Content { get; set; }

    [Range(1, 600, ErrorMessage = "DurationMinutes: от 1 до 600.")]
    public int DurationMinutes { get; set; } = 45;

    public int? Order { get; set; }
}

public class LessonUpdateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title обязателен.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title: от 2 до 200 символов.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Content: максимум 4000 символов.")]
    public string? Content { get; set; }

    [Range(1, 600, ErrorMessage = "DurationMinutes: от 1 до 600.")]
    public int DurationMinutes { get; set; } = 45;

    public int? Order { get; set; }
}

public class LessonResponse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public int DurationMinutes { get; set; }

    public int Order { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

// ---------- Enrollments ----------

public class EnrollmentCreateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "StudentName обязателен.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "StudentName: от 2 до 150 символов.")]
    public string StudentName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "StudentEmail обязателен.")]
    [EmailAddress(ErrorMessage = "StudentEmail: некорректный e-mail.")]
    [StringLength(200)]
    public string StudentEmail { get; set; } = string.Empty;

    [EnumDataType(typeof(EnrollmentStatus), ErrorMessage = "Неизвестный статус записи.")]
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [Range(0, 100, ErrorMessage = "ProgressPercent: от 0 до 100.")]
    public int ProgressPercent { get; set; }
}

public class EnrollmentUpdateRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "StudentName обязателен.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "StudentName: от 2 до 150 символов.")]
    public string StudentName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "StudentEmail обязателен.")]
    [EmailAddress(ErrorMessage = "StudentEmail: некорректный e-mail.")]
    [StringLength(200)]
    public string StudentEmail { get; set; } = string.Empty;

    [EnumDataType(typeof(EnrollmentStatus), ErrorMessage = "Неизвестный статус записи.")]
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [Range(0, 100, ErrorMessage = "ProgressPercent: от 0 до 100.")]
    public int ProgressPercent { get; set; }
}

public class EnrollmentResponse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public EnrollmentStatus Status { get; set; }

    public int ProgressPercent { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}

// ---------- Errors ----------

public class ValidationProblemResponse
{
    public string Title { get; set; } = "Ошибка валидации";

    public int Status { get; set; }

    public Dictionary<string, string[]> Errors { get; set; } = [];
}