namespace TrainingCourseApi.Models;

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public int DurationMinutes { get; set; }

    public int Order { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}