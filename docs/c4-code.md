# C4 Level 4 — Code (Код Course Service)

Внутренняя структура компонента **Course Service** (.NET 10, ASP.NET Core Minimal API).

```mermaid
classDiagram
    class Program {
        <<entry point>>
        +Main(args)
        +ConfigureServices()
        +Configure(app)
        +MapGetCourses()
        +MapGetCourseById()
        +EnsureDbCreated()
        +SeedData()
    }

    class CourseDbContext {
        <<EF Core DbContext>>
        +DbSet<Course> Courses
        +OnModelCreating(modelBuilder)
    }

    class Course {
        +int Id
        +string Name
        +int TotalHours
        +int CompletedHours
        +bool Completed
        +double ProgressPercent
    }

    class CourseService {
        <<business logic>>
        +GetAllCourses()
        +GetCourseById(id)
        +CalculateProgress(course)
    }

    class CourseDTO {
        +int Id
        +string Name
        +int TotalHours
        +int CompletedHours
        +bool Completed
        +double ProgressPercent
    }

    class NpgsqlConnection {
        <<database driver>>
        +OpenAsync()
        +ExecuteAsync(sql)
    }

    Program --> CourseDbContext : injects
    Program --> CourseService : calls
    CourseDbContext --> Course : DbSet
    CourseDbContext --> NpgsqlConnection : connects
    CourseService --> Course : maps
    CourseService --> CourseDTO : converts
    Course --> CourseDTO : fields
```

**Связи:**
- `Program` — точка входа, настраивает DI, подключает EF Core, регистрирует Minimal API эндпоинты.
- `CourseDbContext` — контекст базы данных, таблица `Courses`.
- `Course` — доменная модель курса с вычисляемым свойством `ProgressPercent`.
- `CourseService` — бизнес-логика: получение курсов, расчёт прогресса.
- `CourseDTO` — объект передачи данных для API-ответа.
- `NpgsqlConnection` — драйвер PostgreSQL, выполняет SQL-запросы.
