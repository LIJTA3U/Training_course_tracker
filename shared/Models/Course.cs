namespace CourseTracker.Shared.Models;

/// <summary>
/// Учебный курс — общая модель, используемая .NET-сервисами.
/// </summary>
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalHours { get; set; }
    public int CompletedHours { get; set; }
    public bool Completed { get; set; }

    /// <summary>
    /// Процент прохождения курса (0–100). Вычисляемое свойство.
    /// </summary>
    public double ProgressPercent =>
        TotalHours > 0
            ? Math.Round((double)CompletedHours / TotalHours * 100, 2)
            : 0;
}
