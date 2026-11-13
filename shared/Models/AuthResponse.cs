namespace CourseTracker.Shared.Models;

/// <summary>
/// Ответ сервиса аутентификации после успешного логина.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
