# C4 Level 1 — Context (Контекст)

```mermaid
%%{init: {'theme': 'base'}}%%
C4Context
    title System Context diagram — Training Course Tracker

    Person(user, "Пользователь", "Студент, отслеживает прогресс обучения")

    System_Boundary(c1, "Training Course Tracker") {
        System(webFrontend, "Веб-интерфейс", "Минималистичный сайт на HTML/CSS/JS", "Nginx (статика)")
        System(courseService, "Course Service", "Каталог курсов и учебных часов", "C# / ASP.NET Core Minimal API")
        System(authService, "Auth Service", "Регистрация, логин, JWT-токены", "Node.js / Express")
        System(progressService, "Progress Service", "Запись на курс и прогресс уроков", "Node.js / Express")
    }

    System_Ext(db, "PostgreSQL", "Реляционная база данных курсов", "Postgres 17")

    Rel(user, webFrontend, "Открывает сайт в браузере", "HTTPS / HTTP")
    Rel(webFrontend, courseService, "Запрашивает список курсов", "HTTP/JSON через nginx")
    Rel(webFrontend, authService, "Регистрация / вход", "HTTP/JSON через nginx")
    Rel(webFrontend, progressService, "Запись и прогресс", "HTTP/JSON через nginx")
    Rel(courseService, db, "Читает и пишет курсы", "SQL / Npgsql")

    Rel_None(user, db, "", "")
```
