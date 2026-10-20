namespace TrainingCourseApi.Models;

/// <summary>
/// Статус записи студента на курс.
/// </summary>
public enum EnrollmentStatus
{
    Active = 0,
    Completed = 1,
    Dropped = 2
}

/// <summary>
/// Запись студента на курс.
/// </summary>
public class Enrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    /// <summary>Прогресс прохождения курса, 0-100.</summary>
    public int ProgressPercent { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
}