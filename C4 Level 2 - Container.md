# C4 Level 2 — Container (Контейнеры)

```mermaid
%%{init: {'theme': 'base'}}%%
C4Container
    title Container diagram — Training Course Tracker

    Person(user, "Пользователь", "Студент")

    Container_Boundary(c1, "Training Course Tracker") {
        Container(webFrontend, "Веб-интерфейс", "HTML, CSS, JS", "Nginx Alpine, порт 8080 (внутри), 80 (снаружи)")
        Container(nginxLb, "Балансировщик / Reverse Proxy", "Маршрутизация /api/* на микросервисы", "Nginx Alpine, порт 80")
        Container(courseService, "Course Service", "ASP.NET Core Minimal API, EF Core, Npgsql", ".NET 10, порт 8080")
        Container(authService, "Auth Service", "Регистрация, логин, JWT (jsonwebtoken, bcryptjs)", "Node.js 20, порт 3000")
        Container(progressService, "Progress Service", "Запись на курс, уроки, прогресс пользователя", "Node.js 20, порт 3000")
        ContainerDb(db, "База данных курсов", "PostgreSQL 17", "Таблица Courses, порт 5432 (внутри контейнера)")
    }

    Rel(user, webFrontend, "Открывает сайт", "HTTP, порт 80 хоста")
    Rel(webFrontend, nginxLb, "Запросы статики и API", "HTTP")
    Rel(nginxLb, webFrontend, "Отдаёт index.html, css, js", "HTTP")
    Rel(nginxLb, courseService, "Проксирует /api/courses*", "HTTP")
    Rel(nginxLb, authService, "Проксирует /api/auth*", "HTTP")
    Rel(nginxLb, progressService, "Проксирует /api/progress*", "HTTP")
    Rel(courseService, db, "Читает и пишет курсы", "SQL (Npgsql)")

    BiDiRel(user, authService, "Регистрация и вход (JWT)", "HTTP/JSON")
    BiDiRel(user, progressService, "Запись и просмотр прогресса", "HTTP/JSON")
```
