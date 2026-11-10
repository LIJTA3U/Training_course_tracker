# Трекер учебных курсов — архитектурная документация (C4, Mermaid)

## 1. Описание системы

Трекер учебных курсов — это REST API на ASP.NET Core (.NET 10) для учёта образовательных курсов, уроков и прогресса обучения. Преподаватели и кураторы создают и редактируют курсы и уроки, записывают студентов и следят за их прогрессом и учебными часами; студенты просматривают каталог курсов с фильтрами и пагинацией, записываются на курсы и отслеживают свой прогресс. Данные хранятся в PostgreSQL 16 через Entity Framework Core, приложение разворачивается через Docker Compose и документируется Swagger UI.

---

## 2. Основной пользовательский сценарий

**Сценарий: «Студент выбирает курс и отслеживает свой прогресс обучения»**

1. Студент открывает Swagger UI (или вызывает `GET /api/courses`) и получает список курсов с пагинацией, поиском и фильтрами по категории/активности.
2. Студент открывает карточку курса (`GET /api/courses/{id}`) и видит список уроков и свои записи.
3. Студент записывается на курс (`POST /api/courses/{courseId}/enrollments`) — создаётся запись со статусом `Active` и прогрессом 0 %.
4. По мере прохождения уроков студент обновляет свой прогресс (`PUT /api/courses/{courseId}/enrollments/{enrollmentId}`).
5. Система пересчитывает агрегаты курса (`lessonCount`, `enrollmentCount`, `averageProgress`); при достижении 100 % статус записи меняется на `Completed`.

---

## 3. C4 Level 1 — Context

Система, пользователи и внешние системы. Уровень не смешивается с контейнерами и компонентами.

```mermaid
%%{init: {'theme': 'base'}}%%
C4Context
    title C4 Level 1 — Context: Трекер учебных курсов

    Person(student, "Студент / слушатель", "Просматривает каталог курсов, записывается на курс, отслеживает прогресс и учебные часы")
    Person(tutor, "Преподаватель / куратор", "Создаёт и редактирует курсы и уроки, управляет записями студентов")

    System(tracker, "Трекер учебных курсов (REST API)", "CRUD-веб-сервис для учёта курсов, уроков и прогресса обучения; хранилище — PostgreSQL 16")

    System_Ext(swagger, "Swagger UI / OpenAPI", "Документация и ручное тестирование эндпоинтов API", "External")
    System_Ext(scripts, "Скрипты проверки (verify-crud, verify-persistence)", "Внешние клиенты, прогоняющие сценарии CRUD и сохранности данных по HTTP", "External")
    System_Ext(dbAdmin, "psql / pgAdmin", "Прямое администрирование и отладка базы системы", "External")

    Rel(student, tracker, "Вызывает REST API (JSON)", "HTTPS")
    Rel(tutor, tracker, "Вызывает REST API (JSON)", "HTTPS")
    Rel(tracker, swagger, "Предоставляет спецификацию OpenAPI и Swagger UI", "HTTP")
    Rel(scripts, tracker, "Выполняет проверочные запросы к API", "HTTPS")
    Rel(dbAdmin, tracker, "Читает/пишет данные базы напрямую для отладки", "TCP/5432")

    UpdateLayoutParams($c4ShapeInRow="4")
```

**Направления взаимодействия:** Студент и Преподаватель → система (REST API, HTTPS); система → Swagger UI (отдаёт спецификацию); Внешние скрипты → система (проверочные HTTP-запросы); Инструмент администрирования БД ↔ данные системы (TCP/5432).

---

## 4. C4 Level 2 — Container

Контейнеры: шлюз/балансировщик, API-сервис, PostgreSQL и дополнительный компонент. Уровень не смешивается с контекстом и компонентами.

```mermaid
%%{init: {'theme': 'base'}}%%
C4Container
    title C4 Level 2 — Container: Трекер учебных курсов

    Person(student, "Студент / слушатель", "Просматривает курсы, записывается, отслеживает прогресс")
    Person(tutor, "Преподаватель / куратор", "Создаёт и редактирует курсы и уроки")

    Container_Boundary(gw, "API Gateway / Load Balancer") {
        Container(gateway, "Шлюз / балансировщик (Nginx или Traefik)", "Точка входа: терминация TLS, роутинг запросов на API, ограничение частоты (rate limiting)", "Nginx / Traefik")
    }

    Container_Boundary(api, "Training Course API") {
        Container(apiApp, "Training Course API (ASP.NET Core Minimal API)", "REST API: курсы, уроки, записи; валидация, маппинг, EF Core, миграции, Swagger UI, health-check", "C# / .NET 10")
    }

    Container(db, "PostgreSQL 16", "Реляционное хранилище: курсы, уроки, записи со связями (ON DELETE CASCADE); том db_data сохраняет данные между перезапусками", "PostgreSQL 16")

    Container(cache, "Redis", "Дополнительный компонент — распределённый кэш горячих списков курсов и агрегированных показателей (lessonCount, averageProgress)", "Redis")

    Rel(student, gateway, "REST API (JSON)", "HTTPS")
    Rel(tutor, gateway, "REST API (JSON)", "HTTPS")
    Rel(gateway, apiApp, "Пересылает запросы внутреннему API", "HTTP")
    Rel(apiApp, db, "Чтение/запись данных через EF Core (Npgsql); миграции применяются при старте", "TCP/5432")
    Rel(apiApp, cache, "Чтение и обновление кэша списков и агрегатов", "TCP/6379")

    UpdateLayoutParams($c4ShapeInRow="4")
```

**Роли компонентов:**

| Компонент | Роль |
|---|---|
| Студент / Преподаватель | Пользователи системы (люди). |
| API Gateway / Load Balancer | Точка входа: TLS, роутинг, защита от перегрузки (rate limiting). |
| Training Course API | Основной сервис: бизнес-логика, валидация, работа с БД, Swagger, health-check. |
| PostgreSQL 16 | Хранилище данных (курсы, уроки, записи, каскадное удаление, тома для сохранности). |
| Redis (дополнительный) | Кэш часто запрашиваемых списков курсов и агрегированных метрик — снижает нагрузку на БД и ускоряет чтение каталога. |

**Почему выбран именно Redis:** сценарий чтения доминирует (каталог курсов с пагинацией и фильтрами запрашивается гораздо чаще, чем пишется). Распределённый кэш списков и агрегатов (`lessonCount`, `averageProgress`) даёт наибольший отклик при минимальных изменениях: API можно оставить без изменений, добавив слой кэширования перед запросами к EF Core.

> Примечание: текущий `docker-compose.yml` в репозитории содержит только `api` + `db`. Шлюз и кэш показаны как целевые элементы архитектуры уровня 2 — их можно добавить перед API без изменения кода приложения.

---

## 5. C4 Level 3 — Component (контейнер «Training Course API»)

Внутренние части одного сервиса: контроллеры (Endpoints), валидатор, маппер, репозиторий/контекст данных, модели, контракты. Уровень не смешивается с контекстом и контейнерами.

```mermaid
%%{init: {'theme': 'base'}}%%
C4Component
    title C4 Level 3 — Component: контейнер «Training Course API»

    Person(client, "Клиент API (студент, куратор или шлюз)", "Вызывает эндпоинты REST API")

    Container_Boundary(api, "Training Course API (ASP.NET Core)") {
        Component(epCourses, "CourseEndpoints", "Контроллер курсов (Minimal API endpoints)", "GET/POST/PUT/PATCH/DELETE /api/courses — пагинация, фильтры, каскадное удаление")
        Component(epLessons, "LessonEndpoints", "Контроллер уроков (Minimal API endpoints)", "CRUD уроков внутри курса")
        Component(epEnrollments, "EnrollmentEndpoints", "Контроллер записей студентов (Minimal API endpoints)", "Запись на курс, чтение, обновление прогресса, удаление")
        Component(validation, "ValidationHelper", "Валидатор", "Проверка входящих DTO; ответ 400 со списком ошибок по полям")
        Component(mapping, "MappingExtensions", "Маппер", "Преобразование доменных сущностей ↔ DTO контрактов ответов")
        Component(dbCtx, "AppDbContext", "Репозиторий доступа к данным (EF Core DbContext)", "Загрузка и сохранение сущностей Course, Lesson, Enrollment; запуск миграций при старте")
        Component(models, "Модели (Course, Lesson, Enrollment, CourseCategory)", "Доменные модели", "Сущности и перечисления, отображённые в таблицах БД")
        Component(contracts, "Контракты (Contracts.cs, DTO)", "Контракты API", "Схемы входных и выходных данных (CourseCreateRequest, PagedResponse и т.д.)")
    }

    Container(db, "PostgreSQL 16", "База данных системы", "PostgreSQL 16")

    Rel(client, epCourses, "Маршруты курсов", "HTTP/JSON")
    Rel(client, epLessons, "Маршруты уроков", "HTTP/JSON")
    Rel(client, epEnrollments, "Маршруты записей", "HTTP/JSON")
    Rel(epCourses, validation, "Валидация DTO", "in-process")
    Rel(epLessons, validation, "Валидация DTO", "in-process")
    Rel(epEnrollments, validation, "Валидация DTO", "in-process")
    Rel(epCourses, mapping, "Маппинг сущность ↔ DTO", "in-process")
    Rel(epLessons, mapping, "Маппинг сущность ↔ DTO", "in-process")
    Rel(epEnrollments, mapping, "Маппинг сущность ↔ DTO", "in-process")
    Rel(epCourses, contracts, "Входной/выходной контракт", "in-process")
    Rel(epLessons, contracts, "Входной/выходной контракт", "in-process")
    Rel(epEnrollments, contracts, "Входной/выходной контракт", "in-process")
    Rel(epCourses, dbCtx, "Сохранение и загрузка курсов", "EF Core / Npgsql")
    Rel(epLessons, dbCtx, "Сохранение и загрузка уроков", "EF Core / Npgsql")
    Rel(epEnrollments, dbCtx, "Сохранение и загрузка записей", "EF Core / Npgsql")
    Rel(dbCtx, models, "Работа с доменными сущностями", "in-process")
    Rel(dbCtx, db, "SQL и применение миграций", "TCP/5432")
    Rel(mapping, models, "Преобразование моделей", "in-process")

    UpdateLayoutParams($c4ShapeInRow="4")
```

**Роли компонентов (уровень 3):**

| Компонент | Роль |
|---|---|
| CourseEndpoints / LessonEndpoints / EnrollmentEndpoints | Контроллеры Minimal API — принимают HTTP-запросы, вызывают валидацию, маппинг и репозиторий, возвращают ответы. |
| ValidationHelper | Валидация входящих DTO; при ошибке — ответ `400` со списком полей. |
| MappingExtensions | Маппинг между доменными сущностями (EF Core) и DTO контрактов ответов. |
| AppDbContext | Репозиторий доступа к данным: запросы, сохранение, применение миграций EF Core при старте. |
| Модели (Course, Lesson, Enrollment, CourseCategory) | Доменные модели, отображённые на таблицы БД (snake_case). |
| Контракты (Contracts.cs) | DTO запросов и ответов API. |
| PostgreSQL 16 | Хранилище, к которому обращается AppDbContext через Npgsql. |

**Направления:** Клиент → Endpoints → ValidationHelper / MappingExtensions / AppDbContext → PostgreSQL; MappingExtensions ↔ Модели; AppDbContext ↔ Модели.

---

## 6. Исходные схемы и экспорт

**Редактируемый исходник:** три Mermaid-блока выше (уровни 1, 2 и 3) — это и есть исходники схем. Каждый блок расположен в отдельном fence-коде, уровни не смешаны, все подписи простые, стрелки показывают направление взаимодействия.

Рекомендуемые имена файлов для локального хранения исходников:

- `c4-context.mmd` — схема уровня 1 (Context)
- `c4-container.mmd` — схема уровня 2 (Container)
- `c4-component-api.mmd` — схема уровня 3 (Component для API)

**Экспорт в .png / .pdf:**

```bash
# через mermaid-cli
npx @mermaid-js/mmdc -i c4-context.mmd -o c4-context.png
npx @mermaid-js/mmdc -i c4-container.mmd -o c4-container.png
npx @mermaid-js/mmdc -i c4-component-api.mmd -o c4-component-api.png

# или в PDF
npx @mermaid-js/mmdc -i c4-context.mmd -o c4-context.pdf
```

Альтернатива без установки: скопировать код любого блока выше в [mermaid.live](https://mermaid.live) → кнопка «Download PNG/PDF». Также любую IDE с плагином Mermaid (VS Code, IntelliJ и др.) можно использовать для просмотра и экспорта.
