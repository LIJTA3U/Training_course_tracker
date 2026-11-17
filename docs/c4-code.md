# C4 Level 4 — Code (Код Course Service)

Внутренняя структура компонента **Course Service** (.NET 10, ASP.NET Core Minimal API).

```mermaid
classDiagram
    %% Точка входа
    class Program {
        <<entry point>>
        +Main(string[] args)
        +ConfigureServices()
        +Configure(WebApplication app)
        +MapCourseEndpoints(WebApplication app)
        +EnsureDatabaseCreated()
        +SeedDemoData()
    }

    %% Endpoints Minimal API
    class CourseEndpoints {
        <<static class>>
        +MapGetCourses(WebApplication app)
        +MapGetCourseById(WebApplication app)
        +MapCreateCourse(WebApplication app)
        +MapUpdateCourse(WebApplication app)
        +MapDeleteCourse(WebApplication app)
        +MapGetCourseLessons(WebApplication app)
        +MapGetCourseEnrollments(WebApplication app)
    }

    class AuthEndpoints {
        <<static class>>
        +MapRegister(WebApplication app)
        +MapLogin(WebApplication app)
        +MapMe(WebApplication app)
    }

    class ProgressEndpoints {
        <<static class>>
        +MapEnroll(WebApplication app)
        +MapLesson(WebApplication app)
        +MapGetProgress(WebApplication app)
    }

    %% Бизнес-логика Services
    class ICourseService {
        <<interface>>
        +GetAllCourses()
        +GetCourseById(int id)
        +CreateCourse(CreateCourseRequest request)
        +UpdateCourse(int id, UpdateCourseRequest request)
        +DeleteCourse(int id)
        +CalculateProgress(int courseId)
    }

    class CourseService : ICourseService {
        -readonly CourseDbContext _db
        -readonly ILogger<CourseService> _logger
        +CourseService(CourseDbContext db, ILogger logger)
        +GetAllCourses()
        +GetCourseById(int id)
        +CreateCourse(CreateCourseRequest request)
        +UpdateCourse(int id, UpdateCourseRequest request)
        +DeleteCourse(int id)
        +CalculateProgress(int courseId)
        -MapToDto(Course course)
        -MapToEntity(CreateCourseRequest request)
    }

    class IAuthService {
        <<interface>>
        +Register(RegisterRequest request)
        +Login(LoginRequest request)
        +GetCurrentUser(int userId)
    }

    class AuthService : IAuthService {
        -readonly CourseDbContext _db
        -readonly IJwtTokenService _jwt
        -readonly IPasswordHasher _passwordHasher
        +Register(RegisterRequest request)
        +Login(LoginRequest request)
        +GetCurrentUser(int userId)
        -VerifyPassword(string input, string hash)
        -GenerateToken(User user)
    }

    class IProgressService {
        <<interface>>
        +Enroll(int userId, int courseId)
        +MarkLesson(int userId, int courseId, int lessonId, bool completed)
        +GetProgress(int userId)
    }

    class ProgressService : IProgressService {
        -readonly CourseDbContext _db
        +ProgressService(CourseDbContext db)
        +Enroll(int userId, int courseId)
        +MarkLesson(int userId, int courseId, int lessonId, bool completed)
        +GetProgress(int userId)
    }

    class IJwtTokenService {
        <<interface>>
        +GenerateToken(User user)
        +ValidateToken(string token)
    }

    class JwtTokenService : IJwtTokenService {
        -readonly IConfiguration _config
        +GenerateToken(User user)
        +ValidateToken(string token)
    }

    class IPasswordHasher {
        <<interface>>
        +HashPassword(string password)
        +Verify(string password, string hash)
    }

    class PasswordHasher : IPasswordHasher {
        +HashPassword(string password)
        +Verify(string password, string hash)
    }

    %% Репозитории
    class ICourseRepository {
        <<interface>>
        +GetAll()
        +GetById(int id)
        +Add(Course course)
        +Update(Course course)
        +Delete(int id)
    }

    class CourseRepository : ICourseRepository {
        -readonly CourseDbContext _db
        +CourseRepository(CourseDbContext db)
        +GetAll()
        +GetById(int id)
        +Add(Course course)
        +Update(Course course)
        +Delete(int id)
    }

    class IEnrollmentRepository {
        <<interface>>
        +GetByUserId(int userId)
        +Get(int userId, int courseId)
        +Add(Enrollment enrollment)
    }

    class EnrollmentRepository : IEnrollmentRepository {
        -readonly CourseDbContext _db
        +EnrollmentRepository(CourseDbContext db)
        +GetByUserId(int userId)
        +Get(int userId, int courseId)
        +Add(Enrollment enrollment)
    }

    class ILessonRepository {
        <<interface>>
        +GetUserProgress(int userId, int courseId)
        +Upsert(LessonProgress progress)
    }

    class LessonRepository : ILessonRepository {
        -readonly CourseDbContext _db
        +LessonRepository(CourseDbContext db)
        +GetUserProgress(int userId, int courseId)
        +Upsert(LessonProgress progress)
    }

    %% Доменные модели
    class Course {
        +int Id
        +string Title
        +string Description
        +string Instructor
        +string Category
        +int Credits
        +DateTime StartDate
        +bool IsActive
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +List Lesson Lessons
        +List Enrollment Enrollments
    }

    class Lesson {
        +int Id
        +string Title
        +string Content
        +int DurationMinutes
        +int Order
        +int CourseId
        +Course Course
    }

    class Enrollment {
        +int Id
        +string StudentName
        +string StudentEmail
        +string Status
        +int ProgressPercent
        +int CourseId
        +Course Course
        +List LessonProgress LessonProgresses
    }

    class LessonProgress {
        +int Id
        +int UserId
        +int CourseId
        +int LessonId
        +bool Completed
        +DateTime UpdatedAt
    }

    class User {
        +int Id
        +string Username
        +string PasswordHash
        +string Role
        +DateTime CreatedAt
    }

    %% DTO
    class CourseDTO {
        +int Id
        +string Title
        +string Description
        +string Instructor
        +string Category
        +int Credits
        +DateTime StartDate
        +bool IsActive
        +double ProgressPercent
        +int LessonCount
        +int EnrollmentCount
    }

    class CreateCourseRequest {
        +string Title
        +string Description
        +string Instructor
        +string Category
        +int Credits
        +DateTime StartDate
        +bool IsActive
    }

    class UpdateCourseRequest {
        +string Title
        +string Description
        +string Instructor
        +string Category
        +int Credits
        +DateTime StartDate
        +bool IsActive
    }

    class RegisterRequest {
        +string Username
        +string Password
    }

    class LoginRequest {
        +string Username
        +string Password
    }

    class AuthResponse {
        +string Token
        +UserDto User
        +DateTime ExpiresAt
    }

    class UserDto {
        +int Id
        +string Username
        +string Role
    }

    class EnrollmentDto {
        +int Id
        +int UserId
        +int CourseId
        +DateTime EnrolledAt
    }

    class LessonProgressDto {
        +int LessonId
        +bool Completed
        +DateTime UpdatedAt
    }

    class ProgressDto {
        +int UserId
        +int CourseId
        +double ProgressPercent
        +int LessonsCompleted
        +int LessonsTotal
    }

    %% База данных
    class CourseDbContext {
        <<EF Core DbContext>>
        +DbSet Course Courses
        +DbSet Lesson Lessons
        +DbSet Enrollment Enrollments
        +DbSet LessonProgress LessonProgresses
        +DbSet User Users
        +CourseDbContext(DbContextOptions options)
        +int SaveChanges()
        +Task int SaveChangesAsync()
        +OnModelCreating(ModelBuilder modelBuilder)
    }

    class MigrationInitialCreate {
        <<EF Core Migration>>
        +Up(MigrationBuilder migrationBuilder)
        +Down(MigrationBuilder migrationBuilder)
    }

    %% Исключения
    class ValidationException {
        <<exception>>
        +List string Errors
        +ValidationException(List string errors)
    }

    class NotFoundException {
        <<exception>>
        +string Message
        +NotFoundException(string message)
    }

    class GlobalExceptionMiddleware {
        <<middleware>>
        +InvokeAsync(HttpContext context, RequestDelegate next)
    }

    %% Связи
    Program --> CourseEndpoints : registers
    Program --> AuthEndpoints : registers
    Program --> ProgressEndpoints : registers
    Program --> CourseDbContext : injects
    Program --> ICourseService : injects
    Program --> IAuthService : injects
    Program --> IProgressService : injects

    CourseEndpoints --> ICourseService : calls
    CourseEndpoints --> IProgressService : calls
    AuthEndpoints --> IAuthService : calls
    ProgressEndpoints --> IProgressService : calls

    ICourseService <|.. CourseService : implements
    CourseService --> ICourseRepository : uses
    CourseService --> CourseRepository : uses
    CourseService --> CourseDTO : returns

    ICourseRepository <|.. CourseRepository : implements
    CourseRepository --> CourseDbContext : uses
    CourseRepository --> Course : manages

    IEnrollmentRepository <|.. EnrollmentRepository : implements
    EnrollmentRepository --> CourseDbContext : uses
    EnrollmentRepository --> Enrollment : manages

    ILessonRepository <|.. LessonRepository : implements
    LessonRepository --> CourseDbContext : uses
    LessonRepository --> LessonProgress : manages

    IAuthService <|.. AuthService : implements
    AuthService --> CourseDbContext : uses
    AuthService --> IJwtTokenService : uses
    AuthService --> IPasswordHasher : uses
    AuthService --> User : manages
    AuthService --> AuthResponse : returns

    IJwtTokenService <|.. JwtTokenService : implements
    IPasswordHasher <|.. PasswordHasher : implements

    CourseDbContext --> Course : DbSet
    CourseDbContext --> Lesson : DbSet
    CourseDbContext --> Enrollment : DbSet
    CourseDbContext --> LessonProgress : DbSet
    CourseDbContext --> User : DbSet

    Course --> Lesson : 1:N cascade
    Course --> Enrollment : 1:N cascade
    Enrollment --> LessonProgress : 1:N cascade

    MigrationInitialCreate --> CourseDbContext : applies to

    GlobalExceptionMiddleware --> ValidationException : catches
    GlobalExceptionMiddleware --> NotFoundException : catches

    Course --> CourseDTO : maps to
    CreateCourseRequest --> Course : maps to
    User --> UserDto : maps to
```

**Компоненты и связи:**

| Компонент | Назначение |
|-----------|------------|
| `Program` | Точка входа, настройка DI, регистрация Minimal API эндпоинтов, инициализация БД |
| `CourseEndpoints` / `AuthEndpoints` / `ProgressEndpoints` | HTTP-маршруты (Minimal API) |
| `CourseService` / `AuthService` / `ProgressService` | Бизнес-логика, реализуют интерфейсы `ICourseService`, `IAuthService`, `IProgressService` |
| `CourseRepository` / `EnrollmentRepository` / `LessonRepository` | Доступ к данным через EF Core |
| `CourseDbContext` | Контекст EF Core, таблицы `Courses`, `Lessons`, `Enrollments`, `LessonProgresses`, `Users` |
| `Course`, `Lesson`, `Enrollment`, `LessonProgress`, `User` | Доменные сущности с каскадными связями |
| `CourseDTO`, `CreateCourseRequest`, `AuthResponse`, `ProgressDto` | DTO для запросов и ответов API |
| `JwtTokenService`, `PasswordHasher` | Аутентификация и хеширование паролей |
| `GlobalExceptionMiddleware` | Глобальная обработка исключений (`ValidationException`, `NotFoundException`) |
| `MigrationInitialCreate` | Первая миграция EF Core |
