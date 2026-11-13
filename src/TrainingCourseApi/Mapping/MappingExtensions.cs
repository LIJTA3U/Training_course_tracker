using TrainingCourseApi.Contracts;
using TrainingCourseApi.Models;

namespace TrainingCourseApi.Mapping;

public static class MappingExtensions
{
    public static CourseResponse ToResponse(this Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Instructor = course.Instructor,
        Category = course.Category,
        Credits = course.Credits,
        StartDate = course.StartDate,
        IsActive = course.IsActive,
        CreatedAt = course.CreatedAt,
        UpdatedAt = course.UpdatedAt,
        LessonCount = course.Lessons.Count,
        EnrollmentCount = course.Enrollments.Count,
        AverageProgress = course.Enrollments.Count == 0
            ? 0
            : Math.Round(course.Enrollments.Average(e => e.ProgressPercent), 2)
    };

    public static LessonResponse ToResponse(this Lesson lesson) => new()
    {
        Id = lesson.Id,
        CourseId = lesson.CourseId,
        Title = lesson.Title,
        Content = lesson.Content,
        DurationMinutes = lesson.DurationMinutes,
        Order = lesson.Order,
        CreatedAt = lesson.CreatedAt
    };

    public static EnrollmentResponse ToResponse(this Enrollment enrollment) => new()
    {
        Id = enrollment.Id,
        CourseId = enrollment.CourseId,
        StudentName = enrollment.StudentName,
        StudentEmail = enrollment.StudentEmail,
        Status = enrollment.Status,
        ProgressPercent = enrollment.ProgressPercent,
        EnrolledAt = enrollment.EnrolledAt
    };
}