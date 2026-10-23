# Трекер учебных курсов (Training Course Tracker)

**REST API для отслеживания учебных курсов, учебных часов и прогресса обучения.**

Проект разработан на C# с использованием ASP.NET Core Minimal API. Данные хранятся в **PostgreSQL 16** через Entity Framework Core, схема создаётся миграциями автоматически при старте. Запуск возможен локально через `dotnet run` или в Docker-контейнерах через `docker compose up`. **Версия 2.0**.

---

## Содержание

1. [О проекте](#о-проекте)
2. [Возможности](#возможности)
3. [Технологии](#технологии)
4. [Быстрый старт](#быстрый-старт)
5. [Настройки подключения через environment](#настройки-подключения-через-environment)
6. [Модель данных](#модель-данных)
7. [API Endpoints](#api-endpoints)
8. [Примеры запросов](#примеры-запросов)
9. [Изменения версии 2.0 относительно 1.0](#изменения-версии-20-относительно-10)
10. [Проверка CRUD-запросов](#проверка-crud-запросов)
11. [Проверка сохранения данных после перезапуска](#проверка-сохранения-данных-после-перезапуска)
12. [Локальный запуск без Docker](#локальный-запуск-без-docker)
13. [Запуск в Docker](#запуск-в-docker)
14. [Управление контейнерами](#управление-контейнерами)
15. [Работа с миграциями](#работа-с-миграциями)
16. [Структура проекта](#структура-проекта)
17. [Решение проблем](#решение-проблем)
18. [Статус](#статус)

---

## О проекте

Учебный проект для отслеживания прохождения образовательных курсов. API позволяет управлять курсами, уроками и записями студентов: создавать, изменять, удалять и читать данные, отслеживать прогресс обучения, считать учебные часы и определять статус завершения курса.

Версия 2.0 заменяет начальную версию 1.0 (данные в памяти, только чтение): курсы теперь хранятся в PostgreSQL, реализован полный CRUD, добавлены уроки и записи студентов с каскадным удалением, валидация, Swagger UI и health-check. Маршруты `GET /` и `GET /api/courses/{id}` сохранены, но формат ответа `GET /api/courses` изменился — вместо массива возвращается объект с пагинацией.

---

## Возможности

- Полный CRUD для курсов, уроков и записей студентов.
- Список курсов с пагинацией, поиском по названию/описанию/преподавателю, фильтрами по категории и активности.
- Учёт учебных часов: длительность уроков суммируется, агрегируются `lessonCount`, `enrollmentCount`, `averageProgress`.
- Каскадное удаление: вместе с курсом удаляются его уроки и записи (`ON DELETE CASCADE`).
- Валидация входных данных с ответом `400` и списком ошибок по полям.
- Health-check API и подключения к БД, Swagger UI с описанием всех маршрутов.
- Сохранение данных между перезапусками (Docker volume `db_data`).
- Автоматическое применение миграций EF Core при старте приложения.
- Настройки через переменные окружения (`.env`), Docker Compose.

---

## Технологии

| Технология | Назначение |
| --- | --- |
| C# | Основной язык |
| ASP.NET Core Minimal API | REST API |
| .NET 10 | Платформа выполнения |
| Entity Framework Core 10 | ORM и миграции |
| PostgreSQL 16 | Хранилище (Npgsql) |
| Swashbuckle | Swagger UI / OpenAPI |
| Docker, Docker Compose | Контейнеризация |

---

## Быстрый старт

```bash
# 1. Поднять API и PostgreSQL
docker compose up -d --build

# 2. Проверить, что контейнеры работают
docker compose ps

# 3. Проверить API
curl http://localhost:8080/health
```

Ожидаемый ответ: `{"status":"Healthy","database":"PostgreSQL",...}`

- API: <http://localhost:8080>
- Swagger UI: <http://localhost:8080/swagger>
- Курсы: <http://localhost:8080/api/courses>

Приложение само применяет миграции при старте (`Database.MigrateAsync()`), отдельная инициализация базы не требуется.

---

## Настройки подключения через environment

Все настройки передаются через **environment**. Скопируйте пример и при необходимости отредактируйте:

```bash
cp .env.example .env
```

| Переменная | Назначение | По умолчанию |
| --- | --- | --- |
| `POSTGRES_DB` | Имя базы данных | `training_courses` |
| `POSTGRES_USER` | Пользователь PostgreSQL | `training_user` |
| `POSTGRES_PASSWORD` | Пароль PostgreSQL | `training_password` |
| `POSTGRES_HOST_PORT` | Порт БД на хосте | `55432` |
| `API_HOST_PORT` | Порт API на хосте | `8080` |
| `SEED_ON_STARTUP` | Демо-данные при пустой БД | `false` |

Строка подключения для приложения (`ConnectionStrings__TrainingDb`):

```yaml
Host=db;Port=5432;Database=${POSTGRES_DB:-training_courses};Username=${POSTGRES_USER:-training_user};Password=${POSTGRES_PASSWORD:-training_password}
```

Внутри compose-сети PostgreSQL слушает на `db:5432`; на хост пробрасывается только `POSTGRES_HOST_PORT` (по умолчанию `55432`, так как `5432` часто занят локальным PostgreSQL).

Проверка, что настройки дошли до контейнеров:

```bash
docker compose exec api printenv ConnectionStrings__TrainingDb
docker compose exec db printenv POSTGRES_DB POSTGRES_USER
```

Демо-данные (опционально, только если таблица `courses` пуста):

```bash
SEED_ON_STARTUP=true docker compose up -d --force-recreate api
```

---

## Модель данных

```
courses 1 ────< lessons (ON DELETE CASCADE)
         └─────< enrollments   (ON DELETE CASCADE)
```

**`courses`** — учебный курс

| Поле | Тип | Описание |
| --- | --- | --- |
| `id` | integer | PK, генерируется БД |
| `title` | varchar(200) | Название, обязательное |
| `description` | varchar(2000) | Описание |
| `instructor` | varchar(150) | Преподаватель, обязательное |
| `category` | varchar(50) | `Programming`, `DataScience`, `Design`, `Management`, `Languages`, `SoftSkills`, `Other` |
| `credits` | integer | Кредиты, 0–60 |
| `start_date` | date | Дата старта |
| `is_active` | boolean | Активен ли курс |
| `created_at` / `updated_at` | timestamptz | Аудит |

**`lessons`** — уроки курса: `title`, `content`, `duration_minutes`, `order`, `course_id`

**`enrollments`** — записи студентов: `student_name`, `student_email`, `status` (`Active` / `Completed` / `Dropped`), `progress_percent` (0–100), `course_id`

Имена таблиц, колонок, индексов и внешних ключей — в стиле **snake_case**. Перечисления хранятся текстом.

---

## API Endpoints

Базовый адрес: `http://localhost:8080`

| Метод | Endpoint | Описание |
| --- | --- | --- |
| `GET` | `/` | Проверка API и список маршрутов |
| `GET` | `/health` | Состояние API и подключения к БД |
| `GET` | `/api/courses` | Список курсов (пагинация, фильтры) |
| `POST` | `/api/courses` | Создать курс |
| `GET` | `/api/courses/{id}` | Получить курс по id |
| `PUT` | `/api/courses/{id}` | Обновить курс |
| `PATCH` | `/api/courses/{id}/status` | Активировать / деактивировать |
| `DELETE` | `/api/courses/{id}` | Удалить курс (каскадно) |
| `GET` | `/api/courses/{courseId}/lessons` | Уроки курса |
| `POST` | `/api/courses/{courseId}/lessons` | Добавить урок |
| `GET` | `/api/courses/{courseId}/lessons/{lessonId}` | Получить урок |
| `PUT` | `/api/courses/{courseId}/lessons/{lessonId}` | Обновить урок |
| `DELETE` | `/api/courses/{courseId}/lessons/{lessonId}` | Удалить урок |
| `GET` | `/api/courses/{courseId}/enrollments` | Записи на курс |
| `POST` | `/api/courses/{courseId}/enrollments` | Записать студента |
| `GET` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Получить запись |
| `PUT` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Обновить запись |
| `DELETE` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Удалить запись |
| `GET` | `/api/enrollments` | Все записи по всем курсам |

Параметры `GET /api/courses`:

| Параметр | Тип | По умолчанию | Описание |
| --- | --- | --- | --- |
| `page` | int | `1` | Номер страницы |
| `pageSize` | int | `10` | Размер страницы (1–100) |
| `search` | string | — | Поиск по названию, описанию, преподавателю |
| `category` | `CourseCategory` | — | Фильтр по категории |
| `isActive` | bool | — | Фильтр по активности |

Коды ответов: `201 Created` (с `Location`), `200 OK`, `204 No Content` (удаление), `400 Bad Request` (валидация), `404 Not Found`, `503 Service Unavailable` (нет БД).

---

## Примеры запросов

Проверка работоспособности:

```bash
curl http://localhost:8080/health
```

Создание курса:

```bash
curl -X POST http://localhost:8080/api/courses \
  -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET","description":"Вводный курс",
       "instructor":"Иван Петров","category":"Programming",
       "credits":6,"startDate":"2026-09-01","isActive":true}'
```

Остальные примеры запросов и ответов см. в Swagger UI: <http://localhost:8080/swagger>

---

## Изменения версии 2.0 относительно 1.0

| | Версия 1.0 | Версия 2.0 |
| --- | --- | --- |
| Хранилище | Массив в коде | PostgreSQL 16, EF Core, миграции |
| Операции | Только чтение | Полный CRUD |
| Связанные сущности | Нет | Уроки и записи с каскадным удалением |
| Валидация | Нет | DataAnnotations, ответ `400` с ошибками |
| Формат `GET /api/courses` | Массив `[...]` | Объект `{items, page, ... hasNextPage}` |
| Документация | Нет | Swagger UI, health-check |
| Сохранность данных | Терялась при перезапуске | Volume `db_data` |

---

## Проверка CRUD-запросов

В репозитории есть скрипт, который прогонит полный набор операций (создание, чтение, обновление, удаление, валидация, уроки, записи, фильтрация, Swagger):

```bash
./scripts/verify-crud.sh
```

Ожидаемый итог: все проверки успешно, 0 неуспешно.

Краткий набор вручную:

```bash
# CREATE
curl -X POST http://localhost:8080/api/courses -H 'Content-Type: application/json' \
  -d '{"title":"Курс","instructor":"Преподаватель"}'

# READ
curl http://localhost:8080/api/courses/1
curl http://localhost:8080/api/courses

# UPDATE
curl -X PUT http://localhost:8080/api/courses/1 -H 'Content-Type: application/json' \
  -d '{"title":"Курс 2","instructor":"Преподаватель","category":"Programming","credits":5}'

# DELETE (каскадно удалит уроки и записи)
curl -X DELETE http://localhost:8080/api/courses/1
```

---

## Проверка сохранения данных после перезапуска

Скрипт создаёт эталонные данные, проверяет их после `docker compose restart` и после `docker compose down` + `up` (volume `db_data` сохраняется), затем проверяет каскадное удаление в PostgreSQL:

```bash
./scripts/verify-persistence.sh
```

Проверка вручную:

```bash
# 1. Создать курс и запомнить id
ID=$(curl -s -X POST http://localhost:8080/api/courses \
  -H 'Content-Type: application/json' \
  -d '{"title":"Проверка сохранности","instructor":"Тест"}' | jq -r .id)

# 2. Пересоздать контейнеры (volume сохраняется)
docker compose down
docker compose up -d

# 3. Убедиться, что данные на месте
curl -s http://localhost:8080/api/courses/$ID | jq
```

Прямой доступ к БД:

```bash
docker compose exec db psql -U training_user -d training_courses -c 'SELECT * FROM courses;'
```

> Важно: `docker compose down` удаляет контейнеры и сеть, но volume `db_data` остаётся. Данные исчезают только при `docker compose down -v`.

---

## Локальный запуск без Docker

```bash
cd training-course-api
dotnet restore
export ConnectionStrings__TrainingDb="Host=localhost;Port=5432;Database=training_courses;Username=training_user;Password=training_password"
dotnet run --project src/TrainingCourseApi
```

API доступен по адресу <http://localhost:5287>, Swagger — <http://localhost:5287/swagger>.

Проверка:

```bash
curl http://localhost:5287/health
curl http://localhost:5287/api/courses
```

Для остановки нажмите `Ctrl + C`. Миграции применяются автоматически при старте. База должна быть создана заранее (см. раздел «Настройки подключения»).

---

## Запуск в Docker

Основной способ — Docker Compose (поднимает API и БД вместе):

```bash
docker compose up -d --build
```

Запуск только API-образа (без базы — приложение будет повторно пытаться подключиться к БД):

```bash
docker build -t training-course-api:latest .
docker run -d -p 8080:8080 \
  -e ConnectionStrings__TrainingDb="Host=хост;Port=5432;Database=training_courses;Username=training_user;Password=training_password" \
  training-course-api:latest
```

---

## Управление контейнерами

```bash
docker compose ps             # состояние сервисов
docker compose logs -f api    # логи API (Ctrl+C для выхода)
docker compose logs -f db     # логи БД
docker compose restart        # перезапуск обоих сервисов
docker compose stop           # остановить, volume сохраняется
docker compose down           # удалить контейнеры и сеть, volume сохраняется
docker compose down -v        # удалить и volume — данные будут потеряны
```

---

## Работа с миграциями

`dotnet-ef` подключён как локальный инструмент (`dotnet-tools.json`):

```bash
dotnet tool restore

# Новая миграция
dotnet ef migrations add <ИмяМиграции> \
  --project src/TrainingCourseApi --output-dir Data/Migrations

# Применить вручную (в контейнере применяется автоматически при старте)
dotnet ef database update --project src/TrainingCourseApi

# Откатить последнюю миграцию
dotnet ef migrations remove --project src/TrainingCourseApi

# Список применённых миграций
dotnet ef migrations list --project src/TrainingCourseApi
```

При старте приложение вызывает `Database.MigrateAsync()` и повторяет попытку подключения до 30 раз с интервалом 2 секунды — БД успевает подняться, даже если API стартует раньше неё.

---

## Структура проекта

```
training-course-api/
├── Dockerfile                      # multi-stage образ API
├── docker-compose.yml              # сервисы api + db, сеть, volume db_data
├── .env.example                    # пример переменных окружения
├── .dockerignore / .gitignore
├── dotnet-tools.json               # локальный инструмент dotnet-ef
├── TrainingCourseApi.slnx          # файл решения
├── scripts/
│   ├── verify-crud.sh              # автопроверка CRUD-операций
│   └── verify-persistence.sh       # проверка сохранности данных
└── src/TrainingCourseApi/
    ├── Program.cs                  # DI, БД, Swagger, health-check
    ├── AppDbContextFactory.cs      # фабрика для dotnet ef
    ├── TrainingCourseApi.csproj
    ├── appsettings.json / appsettings.Development.json
    ├── Models/                     # Course, Lesson, Enrollment, перечисления
    ├── Data/
    │   ├── AppDbContext.cs         # контекст EF и конфигурация сущностей
    │   ├── DatabaseSeeder.cs       # опциональные демо-данные
    │   └── Migrations/             # миграции EF Core
    ├── Contracts/Contracts.cs      # DTO запросов и ответов
    ├── Mapping/MappingExtensions.cs
    └── Endpoints/                  # CourseEndpoints, LessonEndpoints, EnrollmentEndpoints
```

Каталоги `bin/` и `obj/` создаются при сборке и исключены в `.gitignore`.

---

## Решение проблем

**Порт 8080 или 55432 уже занят**

```bash
ss -ltn | grep -E ':8080|:55432'
API_HOST_PORT=9090 POSTGRES_HOST_PORT=55433 docker compose up -d
```

**API перезапускается по кругу (`Npgsql.NpgsqlException`)**

Проверьте, что `db` в статусе `healthy`: `docker compose ps`. Если unhealthy, смотрите логи `docker compose logs db`. Чаще всего причина — неверный `POSTGRES_PASSWORD` в `.env`; параметры кластера применяются только при первом создании volume. После смены пароля выполните `docker compose down -v && docker compose up -d`.

**`docker compose down` удалил базу**

Нет — `down` удаляет контейнеры и сеть, но volume `db_data` остаётся. Данные исчезают только при `docker compose down -v`.

**База создалась, но таблиц нет**

Таблицы создаёт приложение при старте. Проверьте логи: `docker compose logs api | grep -i миграции`. Если в БД таблицы со старой схемой, пересоздайте volume: `docker compose down -v && docker compose up -d`.

**Ошибка сборки `error MSB3552: Resource file "**/*.resx" cannot be found`**

На Linux инструмент `dotnet-ef` может создать папку с обратным слэшем (например `bin\Debug`). Удалите её:

```bash
rm -rf 'src/TrainingCourseApi/bin\Debug'
```

---

## Статус

- **Версия 1.0** — начальная реализация, данные в памяти, только чтение.
- **Версия 2.0** — текущая: PostgreSQL, полный CRUD, Docker Compose, Swagger, автоматические миграции, каскадное удаление.