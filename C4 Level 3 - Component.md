# C4 Level 3 — Component (Компоненты Course Service)

```mermaid
%%{init: {'theme': 'base'}}%%
C4Component
    title Component diagram — Course Service (.NET)

    Person_Boundary(user, "Пользователь / Веб-интерфейс") {
        Container_Boundary(cs, "Course Service (контейнер)", ".NET 10, ASP.NET Core Minimal API") {
            Component(apiControllers, "API Endpoints (Minimal API)", "C#", "GET /api/courses, GET /api/courses/{id}")
            Component(service, "CourseService (бизнес-логика)", "C#", "Формирование списка курсов, подсчёт процента")
            Component(repository, "CourseRepository (доступ к данным)", "C# / EF Core", "Queries к таблице Courses")
            Component(sharedModels, "Shared Models (Course, DTO)", "C#", "Курс, часы, статус завершения")
        }
    }

    ContainerDb(db, "PostgreSQL (таблица Courses)", "Postgres 17")

    Rel(user, apiControllers, "Запрашивает курсы", "HTTP/JSON")
    Rel(apiControllers, service, "Вызывает логику", "Внутри процесса (C#)")
    Rel(service, repository, "Запрашивает данные", "Внутри процесса (C#)")
    Rel(repository, db, "SQL-запросы", "Npgsql")

    Rel_None(apiControllers, sharedModels, "", "")
    Rel_None(service, sharedModels, "", "")
    Rel_None(repository, sharedModels, "", "")
```
