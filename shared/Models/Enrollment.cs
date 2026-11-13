namespace CourseTracker.Shared.Models;

/// <summary>
/// Запись пользователя на курс.
/// </summary>
public class Enrollment
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
}
