namespace TrainingCourseApi.Models;

/// <summary>
/// Учебный курс — центральная сущность трекера.
/// </summary>
public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Instructor { get; set; } = string.Empty;

    public CourseCategory Category { get; set; } = CourseCategory.Other;

    /// <summary>Количество учебных кредитов.</summary>
    public int Credits { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<Lesson> Lessons { get; set; } = [];

    public List<Enrollment> Enrollments { get; set; } = [];
}