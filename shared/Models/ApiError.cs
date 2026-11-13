namespace CourseTracker.Shared.Models;

/// <summary>
/// Унифицированная модель ошибки API.
/// </summary>
public class ApiError
{
    public string Error { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
