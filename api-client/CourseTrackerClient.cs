using System.Net.Http.Json;
using CourseTracker.Shared.Models;

namespace CourseTracker.Client;

/// <summary>
/// Типизированный C#-клиент для взаимодействия с микросервисами Training Course Tracker
/// через общий балансировщик нагрузки (nginx).
/// </summary>
public class CourseTrackerClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private string? _jwtToken;

    public CourseTrackerClient(HttpClient httpClient, string baseUrl = "http://localhost")
    {
        _http = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Установить JWT-токен (например, после логина).
    /// </summary>
    public void SetToken(string? token) => _jwtToken = token;

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}{path}");
        if (_jwtToken is not null)
            request.Headers.Authorization = new("Bearer", _jwtToken);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _http.SendAsync(request);
    }

    /// <summary>
    /// Получить все курсы.
    /// </summary>
    public Task<List<Course>> GetCoursesAsync() =>
        SendAsync(HttpMethod.Get, "/api/courses")
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<List<Course>>() ?? new List<Course>())
            .Unwrap();

    /// <summary>
    /// Получить курс по идентификатору.
    /// </summary>
    public Task<Course?> GetCourseAsync(int id) =>
        SendAsync(HttpMethod.Get, $"/api/courses/{id}")
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<Course>())
            .Unwrap();

    /// <summary>
    /// Зарегистрировать нового пользователя.
    /// </summary>
    public Task<HttpResponseMessage> RegisterAsync(string userName, string password) =>
        SendAsync(HttpMethod.Post, "/api/auth/register", new { userName, password });

    /// <summary>
    /// Выполнить вход и сохранить полученный JWT.
    /// </summary>
    public async Task<AuthResponse?> LoginAsync(string userName, string password)
    {
        var response = await SendAsync(HttpMethod.Post, "/api/auth/login", new { userName, password });
        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result is not null)
            _jwtToken = result.Token;

        return new AuthResponse
        {
            Token = result.Token,
            UserId = result.User.Id,
            UserName = result.User.UserName,
            ExpiresAt = DateTime.UtcNow.AddHours(12)
        };
    }

    /// <summary>
    /// Записать текущего пользователя на курс.
    /// </summary>
    public Task<HttpResponseMessage> EnrollAsync(int userId, int courseId) =>
        SendAsync(HttpMethod.Post, "/api/progress/enroll", new { userId, courseId });

    public void Dispose() => _http.Dispose();

    private sealed class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public LoginUser User { get; set; } = new();
    }

    private sealed class LoginUser
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
