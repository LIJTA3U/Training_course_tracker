# Трекер учебных курсов (Training Course Tracker)

**REST API для отслеживания учебных курсов, учебных часов и прогресса обучения.**

Проект разработан на C# с использованием ASP.NET Core Minimal API. Приложение поддерживает локальный запуск и запуск в Docker-контейнерах. **Версия 2.0** продолжает версию 1.0: вместо статичных данные курсы хранятся в PostgreSQL, а вместо трёх read-only маршрутов API реализует полный CRUD.

---

## Содержание

1. [О проекте](#о-проекте)
2. [Возможности](#возможности)
3. [Технологии](#технологии)
4. [Команды для создания проекта](#команды-для-создания-проекта)
5. [Команды для создания базы данных](#команды-для-создания-базы-данных)
6. [Быстрый старт](#быстрый-старт)
7. [Настройки подключения через environment](#настройки-подключения-через-environment)
8. [Модель данных](#модель-данных)
9. [API Endpoints](#api-endpoints)
10. [Примеры запросов и ответов](#примеры-запросов-и-ответов)
11. [Изменения версии 2.0 относительно 1.0](#изменения-версии-20-относительно-10)
12. [Проверка CRUD-запросов](#проверка-crud-запросов)
13. [Проверка сохранения данных после перезапуска](#проверка-сохранения-данных-после-перезапуска)
14. [Локальный запуск без Docker](#локальный-запуск-без-docker)
15. [Запуск в Docker](#запуск-в-docker)
16. [Проверка и управление контейнерами](#проверка-и-управление-контейнерами)
17. [Полный сценарий демонстрации](#полный-сценарий-демонстрации)
18. [Работа с миграциями](#работа-с-миграциями)
19. [Структура проекта](#структура-проекта)
20. [Решение проблем](#решение-проблем)

---

## О проекте

### Версия 2.0 — текущая

Учебный проект для отслеживания прохождения образовательных курсов.

API позволяет управлять учебными курсами, их уроками и записями студентов: создавать, изменять, удалять и читать данные, отслеживать прогресс обучения, считать учебные часы и определять статус завершения курса.

Приложение построено на ASP.NET Core Minimal API, данные хранятся в **PostgreSQL 16** через Entity Framework Core, схема создаётся миграциями автоматически при старте. Запуск возможен локально через `dotnet run` или в Docker-контейнерах через `docker compose up`.

### Версия 1.0 — начальная

Первая версия проекта возвращала список курсов из захардкоженного массива. Она остаётся в истории как основание для текущей: те же маршруты `GET /` и `GET /api/courses/{id}` сохранены, но данные теперь поступают из базы, а формат ответа изменился (см. [изменения версии 2.0](#изменения-версии-20-относительно-10)).

---

## Возможности

- Полный CRUD для сущностей темы: курсы, уроки, записи студентов.
- Список курсов с пагинацией, поиском по названию/описанию/преподавателю, фильтрами по категории и активности.
- Учёт учебных часов: длительность уроков суммируется, прогресс студентов агрегируется (`lessonCount`, `enrollmentCount`, `averageProgress`).
- Определение статуса завершения курса: `status = Completed` и `progressPercent = 100` у записей, `isActive` у курса.
- Каскадное удаление: вместе с курсом удаляются его уроки и записи (`ON DELETE CASCADE`).
- Валидация входных данных с ответом `400` и списком ошибок по полям.
- Health-check API и подключения к БД, Swagger UI с описанием всех маршрутов.
- Хранение данных в PostgreSQL с сохранением между перезапусками (Docker volume).
- Автоматическое применение миграций при старте приложения.
- Сборка и запуск в Docker и Docker Compose, настройки через переменные окружения.

---

## Технологии

| Технология | Назначение |
| --- | --- |
| C# | Основной язык программирования |
| ASP.NET Core Minimal API | Реализация REST API |
| .NET 10 | Платформа выполнения приложения |
| Entity Framework Core 10 | ORM и миграции схемы БД |
| PostgreSQL 16 | Хранилище данных (драйвер Npgsql) |
| Swashbuckle | Swagger UI / OpenAPI |
| Docker, Docker Compose | Контейнеризация приложения и БД |
| Ubuntu Linux | Операционная система для запуска и тестирования |

---

## Команды для создания проекта

Полная последовательность, которой создан этот репозиторий (проверена на .NET SDK 10.0.112).

```bash
# 1. Каталог проекта и файл решения
mkdir -p training-course-api
cd training-course-api
dotnet new sln -n TrainingCourseApi

# 2. Web API проект
dotnet new webapi -n TrainingCourseApi -o src/TrainingCourseApi --no-https
dotnet sln add src/TrainingCourseApi/TrainingCourseApi.csproj

# 3. Пакеты: PostgreSQL, миграции, Swagger, конвенция имён
cd src/TrainingCourseApi
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package EFCore.NamingConventions
cd ../..

# 4. Локальный инструмент для миграций
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 10.0.3

# 5. Миграция (схему создаёт Npgsql, имена — в snake_case)
dotnet ef migrations add InitialCreate \
  --project src/TrainingCourseApi \
  --output-dir Data/Migrations

# 6. Сборка и проверка
dotnet build
```

После этого добавляются файлы `Models/`, `Data/AppDbContext.cs`, `Contracts/`, `Endpoints/`, `Dockerfile`, `docker-compose.yml`, `.env.example` — структура приведена ниже.

---

## Команды для создания базы данных

### Вариант 1 — автоматически (рекомендуется)

Отдельная база вручную не создаётся. Официальный образ `postgres:16` создаёт пользователя и базу при первом старте контейнера из переменных `POSTGRES_*`, а таблицы создаёт приложение, применяя миграции EF Core при старте.

```bash
docker compose up -d --build
```

Или тот же контейнер вручную:

```bash
docker run -d \
  --name training-db \
  -e POSTGRES_USER=training_user \
  -e POSTGRES_PASSWORD=training_password \
  -e POSTGRES_DB=training_courses \
  -p 55432:5432 \
  -v training_db_data:/var/lib/postgresql/data \
  postgres:16
```

Проверка, что база и таблицы созданы:

```bash
docker compose exec db psql -U training_user -d postgres \
  -c "SELECT datname FROM pg_database WHERE datname='training_courses';"

docker compose exec db psql -U training_user -d training_courses -c '\dt'
```

### Вариант 2 — вручную в установленном PostgreSQL

```bash
# Войти под системным пользователем postgres
sudo -u postgres psql
```

В `psql`:

```sql
CREATE USER training_user WITH PASSWORD 'training_password';
CREATE DATABASE training_courses OWNER training_user;
GRANT ALL PRIVILEGES ON DATABASE training_courses TO training_user;
\q
```

Одной строкой:

```bash
sudo -u postgres psql -c "CREATE USER training_user WITH PASSWORD 'training_password';"
sudo -u postgres psql -c "CREATE DATABASE training_courses OWNER training_user;"
```

### Вариант 3 — восстановление пустой базы

Полезно, если нужно начать с нуля при сохранённом volume:

```bash
docker compose exec db psql -U training_user -d postgres \
  -c 'DROP DATABASE IF EXISTS training_courses;' \
  -c 'CREATE DATABASE training_courses OWNER training_user;'
```

> **Важно:** параметры `POSTGRES_*` применяются только при **первом** создании volume.
> После смены `POSTGRES_PASSWORD` выполните `docker compose down -v && docker compose up -d`.

---

## Быстрый старт

```bash
# 1. Поднять оба сервиса
docker compose up -d --build

# 2. Убедиться, что контейнеры работают (у db — Healthy)
docker compose ps

# 3. Проверить API
curl http://localhost:8080/health
```

Ожидаемый ответ:

```json
{"status":"Healthy","database":"PostgreSQL","utc":"2026-10-16T12:12:32.4577932+00:00"}
```

- API: <http://localhost:8080>
- Swagger UI: <http://localhost:8080/swagger>
- Курсы: <http://localhost:8080/api/courses>

### Что происходит при `docker compose up`

1. Поднимается контейнер `db` на образе **`postgres:16`**, создаёт пользователя и базу из `POSTGRES_*`.
2. Volume `db_data` монтируется в `/var/lib/postgresql/data`.
3. Health-check БД (`pg_isready`) должен отработать успешно.
4. Контейнер `api` стартует только после этого — задано условием `depends_on: db: condition: service_healthy`.
5. Приложение подключается к БД по сети `training_net` как `Host=db;Port=5432`.
6. Приложение применяет миграции EF Core (`Database.MigrateAsync()`) с повторными попытками подключения — отдельный шаг инициализации не нужен.

---

## Настройки подключения через environment

Все настройки подключения передаются в контейнеры через **environment**. `docker-compose.yml` читает их из `.env` (или из переменных окружения), подставляя значения по умолчанию, — править сам compose-файл не нужно.

```bash
cp .env.example .env
```

| Переменная | Назначение | По умолчанию |
| --- | --- | --- |
| `POSTGRES_DB` | Имя базы данных | `training_courses` |
| `POSTGRES_USER` | Пользователь PostgreSQL | `training_user` |
| `POSTGRES_PASSWORD` | Пароль PostgreSQL | `training_password` |
| `POSTGRES_HOST_PORT` | Порт БД **на хосте** | `55432` |
| `API_HOST_PORT` | Порт API **на хосте** | `8080` |
| `SEED_ON_STARTUP` | Демо-данные при пустой БД (`true`/`false`) | `false` |

Параметры БД для приложения собираются в строку подключения и передаются в контейнер `api` как переменная `ConnectionStrings__TrainingDb`:

```yaml
environment:
  ConnectionStrings__TrainingDb: >-
    Host=db;Port=5432;Database=${POSTGRES_DB:-training_courses};Username=${POSTGRES_USER:-training_user};Password=${POSTGRES_PASSWORD:-training_password}
```

Двойное подчёркивание `__` — стандартный разделитель уровней конфигурации .NET, поэтому `ConnectionStrings__TrainingDb` читается приложением как `ConnectionStrings:TrainingDb` (тот же ключ, что в `appsettings.json`).

Проверка, что настройки дошли до контейнеров:

```bash
docker compose exec api printenv ConnectionStrings__TrainingDb
docker compose exec db printenv POSTGRES_DB POSTGRES_USER
```

> **Почему БД пробрасывается на порт `55432`, а не `5432`?** Внутри compose-сети PostgreSQL всегда слушает `5432`, и API обращается именно к `db:5432`. Порт на хосте нужен только для ручного доступа через `psql`, а `5432` на машине разработчика обычно занят локальным PostgreSQL. Значение меняется одной переменной `POSTGRES_HOST_PORT`.

### Демо-данные (по желанию)

```bash
SEED_ON_STARTUP=true docker compose up -d --force-recreate api
```

Сработает, только если таблица `courses` пуста: 3 курса, 5 уроков, 1 запись.

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

Имена таблиц, колонок, индексов и внешних ключей — в стиле **snake_case** (конвенция PostgreSQL). Перечисления хранятся текстом, а не числами.

---

## API Endpoints

Базовый адрес API:

```
http://localhost:8080
```

| Метод | Endpoint | Описание |
| --- | --- | --- |
| `GET` | `/` | Проверка работоспособности API и список маршрутов |
| `GET` | `/health` | Состояние API и подключения к БД |
| `GET` | `/api/courses` | Список курсов с пагинацией и фильтрами |
| `POST` | `/api/courses` | Создать курс |
| `GET` | `/api/courses/{id}` | Получить курс по идентификатору |
| `PUT` | `/api/courses/{id}` | Обновить курс |
| `PATCH` | `/api/courses/{id}/status` | Активировать / деактивировать курс |
| `DELETE` | `/api/courses/{id}` | Удалить курс (каскадно) |
| `GET` | `/api/courses/{courseId}/lessons` | Уроки курса |
| `POST` | `/api/courses/{courseId}/lessons` | Добавить урок |
| `GET` | `/api/courses/{courseId}/lessons/{lessonId}` | Получить урок |
| `PUT` | `/api/courses/{courseId}/lessons/{lessonId}` | Обновить урок |
| `DELETE` | `/api/courses/{courseId}/lessons/{lessonId}` | Удалить урок |
| `GET` | `/api/courses/{courseId}/enrollments` | Записи на курс |
| `POST` | `/api/courses/{courseId}/enrollments` | Записать студента на курс |
| `GET` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Получить запись |
| `PUT` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Обновить запись |
| `DELETE` | `/api/courses/{courseId}/enrollments/{enrollmentId}` | Удалить запись |
| `GET` | `/api/enrollments` | Все записи по всем курсам |

Параметры `GET /api/courses`:

| Параметр | Тип | По умолчанию | Описание |
| --- | --- | --- | --- |
| `page` | int | `1` | Номер страницы |
| `pageSize` | int | `10` | Размер страницы (1–100) |
| `search` | string | — | Поиск по названию, описанию и преподавателю |
| `category` | `CourseCategory` | — | Фильтр по категории |
| `isActive` | bool | — | Фильтр по активности |

Коды ответов: `201 Created` (с заголовком `Location`), `200 OK`, `204 No Content` (удаление), `400 Bad Request` (валидация), `404 Not Found`, `503 Service Unavailable` (нет БД).

---

## Примеры запросов и ответов

### Проверка работоспособности API

```
curl http://localhost:8080/
```

```json
{
  "message": "Course Tracker API is running",
  "version": "2.0",
  "storage": "PostgreSQL",
  "documentation": "/swagger",
  "health": "/health",
  "endpoints": ["GET /api/courses", "... всего 18 маршрутов"]
}
```

### Получение списка учебных курсов

```
curl http://localhost:8080/api/courses
```

```json
{
  "items": [
    {
      "id": 1,
      "title": "Основы C# и .NET",
      "description": "Вводный курс по языку C# и платформе .NET",
      "instructor": "Иван Петров",
      "category": "Programming",
      "credits": 6,
      "startDate": "2026-09-01",
      "isActive": true,
      "createdAt": "2026-10-16T11:53:55.2082777+00:00",
      "updatedAt": "2026-10-16T11:53:55.2082777+00:00",
      "lessonCount": 3,
      "enrollmentCount": 1,
      "averageProgress": 40
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1,
  "hasNextPage": false
}
```

### Получение курса по идентификатору

```
curl http://localhost:8080/api/courses/1
```

Если курс не найден, API возвращает HTTP-статус `404 Not Found`:

```json
{ "message": "Курс с id=999999 не найден." }
```

### Создание курса

```
curl -X POST http://localhost:8080/api/courses \
  -H 'Content-Type: application/json' \
  -d '{
        "title": "Основы C# и .NET",
        "description": "Вводный курс по языку C# и платформе .NET",
        "instructor": "Иван Петров",
        "category": "Programming",
        "credits": 6,
        "startDate": "2026-09-01",
        "isActive": true
      }'
```

### Структура объекта курса

| Поле | Тип | Описание |
| --- | --- | --- |
| `id` | int | Уникальный идентификатор курса |
| `title` | string | Название курса |
| `description` | string | Описание |
| `instructor` | string | Преподаватель |
| `category` | string | Категория курса |
| `credits` | int | Количество учебных кредитов |
| `startDate` | date | Дата старта |
| `isActive` | bool | Активен ли курс |
| `createdAt` / `updatedAt` | datetime | Даты аудита |
| `lessonCount` | int | Количество уроков |
| `enrollmentCount` | int | Количество записей |
| `averageProgress` | double | Средний прогресс студентов, 0–100 |

---

## Изменения версии 2.0 относительно 1.0

| | Версия 1.0 | Версия 2.0 |
| --- | --- | --- |
| Хранилище | Массив в коде | PostgreSQL 16, EF Core, миграции |
| Запуск | `docker run` | `docker compose up` |
| Операции | Только чтение | Полный CRUD |
| Курс | `name`, `totalHours`, `completedHours`, `completed` | `title`, `instructor`, `category`, `credits`, `startDate`, `isActive` + агрегаты |
| Ответ `GET /api/courses` | Массив `[...]` | Объект `{items, page, pageSize, totalItems, totalPages, hasNextPage}` |
| Связанные сущности | Нет | Уроки и записи студентов с каскадным удалением |
| Валидация | Нет | DataAnnotations, ответ `400` с ошибками по полям |
| Документация | Нет | Swagger UI `/swagger`, health-check `/health` |
| Сохранность данных | Терялась при перезапуске | Volume `db_data`, переживает `down`/`up` |

**Важно:** формат ответа `GET /api/courses` изменился — вместо массива возвращается объект с пагинацией, а поля курса расширены. Поле `name` заменено на `title`, `completedHours`/`completed` больше не отдаются напрямую: прогресс теперь хранится в записях студентов (`progressPercent`, `status`), а агрегированные значения приходят в `averageProgress` и `isActive`.

Маршруты `GET /` и `GET /api/courses/{id}` сохранены и работают.

---

## Проверка CRUD-запросов

### Автоматически

В репозитории есть скрипт, который прогоняет полный набор операций:

```bash
./scripts/verify-crud.sh
```

Фактический результат прогона:

```
== 0. Health-check
  OK   GET /health -> 200                                         [200]
== 1. Courses: Create (POST)
  OK   POST /api/courses -> 201                                   [201]
== 2. Courses: Read (GET)
  OK   GET /api/courses/2 -> 200                                  [200]
  OK   GET /api/courses (список) -> 200                           [200]
  OK   GET /api/courses/999999 -> 404                             [404]
== 3. Courses: Update (PUT)
  OK   PUT /api/courses/2 -> 200                                  [200]
  OK   PATCH /api/courses/2/status -> 200                         [200]
== 4. Courses: Validation (400)
  OK   POST /api/courses с title='X' -> 400                      [400]
  OK   POST /api/courses без instructor -> 400                   [400]
  OK   POST /api/courses с credits=999 -> 400                    [400]
== 5. Lessons (вложенный CRUD)
  OK   POST /api/courses/2/lessons -> 201                         [201]
  OK   GET /api/courses/2/lessons -> 200                          [200]
  OK   PUT lesson 1 -> 200                                        [200]
== 6. Enrollments (вложенный CRUD)
  OK   POST /api/courses/2/enrollments -> 201                     [201]
  OK   PUT enrollment 1 -> 200                                    [200]
== 7. Фильтрация и поиск
  OK   GET /api/courses?category=Programming -> 200               [200]
== 8. Courses: Delete (DELETE)
  OK   DELETE /api/courses/2 -> 204                               [204]
== 9. Swagger / OpenAPI
  OK   GET /swagger/v1/swagger.json -> 200                        [200]

===== ИТОГ: 31 успешно, 0 неуспешно =====
```

### Вручную

```bash
# CREATE — создать курс
curl -X POST http://localhost:8080/api/courses \
  -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET","description":"Вводный курс",
       "instructor":"Иван Петров","category":"Programming",
       "credits":6,"startDate":"2026-09-01","isActive":true}'

# READ — список с фильтром и пагинацией
curl 'http://localhost:8080/api/courses?page=1&pageSize=10&category=Programming&search=C%23'
curl http://localhost:8080/api/courses/1

# UPDATE — полное обновление
curl -X PUT http://localhost:8080/api/courses/1 \
  -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET (2-е издание)","description":"Обновлённый курс",
       "instructor":"Иван Петров","category":"Programming","credits":8,
       "startDate":"2026-10-01","isActive":true}'

# UPDATE — деактивировать курс
curl -X PATCH 'http://localhost:8080/api/courses/1/status?isActive=false'

# Вложенные сущности: уроки и записи студентов
curl -X POST http://localhost:8080/api/courses/1/lessons \
  -H 'Content-Type: application/json' \
  -d '{"title":"Введение и настройка среды","durationMinutes":45,"content":"Установка .NET SDK"}'
curl http://localhost:8080/api/courses/1/lessons

curl -X POST http://localhost:8080/api/courses/1/enrollments \
  -H 'Content-Type: application/json' \
  -d '{"studentName":"Анна Смирнова","studentEmail":"anna@example.com",
       "status":"Active","progressPercent":40}'
curl http://localhost:8080/api/courses/1/enrollments

# DELETE — запись, урок, затем курс
curl -X DELETE http://localhost:8080/api/courses/1/enrollments/1
curl -X DELETE http://localhost:8080/api/courses/1/lessons/1
curl -X DELETE http://localhost:8080/api/courses/1
```

---

## Проверка сохранения данных после перезапуска

```bash
./scripts/verify-persistence.sh
```

Скрипт создаёт эталонные данные, проверяет их после `docker compose restart` и после полного `docker compose down` + `up` (контейнеры пересоздаются, volume `db_data` сохраняется), затем подтверждает каскадное удаление напрямую в PostgreSQL через `psql`.

```
== 1. Создаём эталонные данные ==
   courseId=3 title=persistence-115703
== 2. docker compose restart ==
  OK   после restart: курс #3 и все поля сохранены
  OK   после restart: урок #3 сохранён
  OK   после restart: запись #3 сохранена
== 3. docker compose down + up ==
  OK   после down/up: курс #3 сохранён в volume db_data
  OK   после down/up: урок #3 доступен
  OK   после down/up: запись #3 доступна
== 4. Удаление через API и проверка каскада в БД ==
  OK   DELETE курса каскадно удалил уроки
  OK   DELETE курса каскадно удалил записи
  OK   в PostgreSQL строка lessons с id=3 отсутствует (ON DELETE CASCADE)

===== ИТОГ: 9 успешно, 0 неуспешно =====
```

### Проверка вручную

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

Прямой доступ к базе:

```bash
docker compose exec db psql -U training_user -d training_courses -c 'SELECT * FROM courses;'
docker compose exec db psql -U training_user -d training_courses -c '\d lessons'
```

---

## Локальный запуск без Docker

### 1. Перейти в корень проекта

```
cd training-course-api
```

### 2. Восстановить зависимости

```
dotnet restore
```

### 3. Убедиться, что база создана

См. [команды для создания базы данных](#команды-для-создания-базы-данных), вариант 2.

### 4. Запустить приложение

```
export ConnectionStrings__TrainingDb="Host=localhost;Port=5432;Database=training_courses;Username=training_user;Password=training_password"
dotnet run --project src/TrainingCourseApi
```

API доступен по адресу <http://localhost:5287>, Swagger — <http://localhost:5287/swagger>.

### 5. Проверить работу API

Выполни команды в другом терминале:

```
curl http://localhost:5287/
curl http://localhost:5287/health
curl http://localhost:5287/api/courses
curl http://localhost:5287/api/courses/1
```

Для остановки приложения нажми `Ctrl + C`. Миграции применяются автоматически при старте.

---

## Запуск в Docker

Основной способ — Docker Compose, он поднимает API и БД вместе:

```
docker compose up -d --build
```

Запуск только API-образа (как в версии 1.0) тоже поддерживается, но **без базы** — приложение будет повторно пытаться подключиться к БД и не стартует:

```
docker build -t training-course-api:latest .

docker run -d \
  --name training-api \
  -p 8080:8080 \
  -e ConnectionStrings__TrainingDb="Host=хост;Port=5432;Database=training_courses;Username=training_user;Password=training_password" \
  training-course-api:latest
```

Описание параметров:

- `-d` — запуск контейнера в фоновом режиме.
- `--name training-api` — имя контейнера.
- `-p 8080:8080` — проброс порта хоста `8080` на порт контейнера `8080`.
- `-e ConnectionStrings__TrainingDb=...` — настройки подключения через environment.
- `training-course-api:latest` — имя и тег Docker-образа.

---

## Проверка и управление контейнерами

### Просмотр состояния

```
docker compose ps
docker ps -a
```

### Просмотр логов

```
docker compose logs -f api
docker compose logs -f db
docker logs -f training-api
docker logs -f training-db
```

Для завершения просмотра логов нажми `Ctrl + C`. Это не останавливает контейнер.

### Перезапуск и остановка

```
docker compose restart              # перезапустить оба сервиса
docker compose restart api          # только API
docker compose stop                 # остановить, volume сохраняется
docker compose down                 # удалить контейнеры и сеть, volume СОХРАНЯЕТСЯ
docker compose down -v              # удалить и volume — данные будут потеряны
```

---

## Полный сценарий демонстрации

### Шаг 1. Подготовка

```
docker compose down -v
```

### Шаг 2. Запуск в Docker Compose

```
docker compose up -d --build
```

### Шаг 3. Проверка контейнеров

```
docker compose ps
```

Ожидается: `training-db` в статусе `healthy`, `training-api` — `Up`.

### Шаг 4. Проверка создания базы данных

```
docker compose exec db psql -U training_user -d training_courses -c '\dt'
```

Ожидаются таблицы `courses`, `lessons`, `enrollments`, `__EFMigrationsHistory`.

### Шаг 5. Проверка API

```
curl http://localhost:8080/health
curl http://localhost:8080/
curl http://localhost:8080/api/courses
```

### Шаг 6. Полный CRUD

```
./scripts/verify-crud.sh
```

### Шаг 7. Проверка сохранности данных

```
./scripts/verify-persistence.sh
```

### Шаг 8. Проверка логов

```
docker compose logs api | grep -i 'миграции'
```

### Шаг 9. Очистка

```
docker compose down
```

---

## Работа с миграциями

`dotnet-ef` подключён как локальный инструмент (`dotnet-tools.json`).

```bash
# Восстановить инструмент после клонирования
dotnet tool restore

# Новая миграция
dotnet ef migrations add <ИмяМиграции> \
  --project src/TrainingCourseApi \
  --output-dir Data/Migrations

# Применить вручную (в контейнере применяется автоматически при старте)
dotnet ef database update --project src/TrainingCourseApi

# Откатить последнюю миграцию
dotnet ef migrations remove --project src/TrainingCourseApi

# Посмотреть применённые миграции
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
│   ├── verify-crud.sh              # автопроверка всех CRUD-операций
│   └── verify-persistence.sh       # проверка сохранности данных при перезапуске
└── src/TrainingCourseApi/
    ├── Program.cs                  # конфигурация DI, БД, Swagger, health-check
    ├── AppDbContextFactory.cs      # design-time фабрика для dotnet ef
    ├── TrainingCourseApi.csproj
    ├── appsettings.json            # строки подключения и настройки
    ├── appsettings.Development.json
    ├── Models/                     # Course, Lesson, Enrollment, перечисления
    ├── Data/
    │   ├── AppDbContext.cs         # контекст EF и конфигурация сущностей
    │   ├── DatabaseSeeder.cs       # опциональные демо-данные
    │   └── Migrations/             # миграции EF Core
    ├── Contracts/Contracts.cs      # DTO запросов и ответов
    ├── Mapping/MappingExtensions.cs
    └── Endpoints/                  # CourseEndpoints, LessonEndpoints, EnrollmentEndpoints
```

Каталоги `bin/` и `obj/` создаются автоматически при сборке и запуске .NET-проекта. Их не следует добавлять в Git — они исключены в `.gitignore`.

---

## Решение проблем

**Порт 8080 или 55432 уже занят**

```
ss -ltn | grep -E ':8080|:55432'
```

Поменять порты: `API_HOST_PORT=9090 POSTGRES_HOST_PORT=55433 docker compose up -d`

**API перезапускается по кругу (в логах `Npgsql.NpgsqlException`)**

Проверьте, что БД healthy: `docker compose ps`. Если у `db` статус `unhealthy`, смотрите `docker compose logs db`. Чаще всего причина — неверные `POSTGRES_PASSWORD` в `.env`: параметры кластера применяются только при первом создании volume. После смены пароля выполните `docker compose down -v && docker compose up -d`.

**`docker compose down` удалил базу**

Нет — `down` удаляет контейнеры и сеть, но volume `db_data` остаётся. Данные исчезают только при `docker compose down -v`.

**База создалась, но таблиц нет**

Таблицы создаёт приложение при старте. Проверьте логи: `docker compose logs api | grep -i миграции`. Если в БД есть таблицы со старой схемой, пересоздайте volume: `docker compose down -v && docker compose up -d`.

**Ошибка сборки `error MSB3552: Resource file "**/*.resx" cannot be found`**

В каталоге проекта появилась папка с обратным слэшем в имени (например `bin\Debug`) — её создаёт инструмент `dotnet-ef` на Linux. Такой каталог ломает glob-разбор MSBuild. Удалите его:

```bash
rm -rf 'src/TrainingCourseApi/bin\Debug'
```

В `.gitignore` и `.dockerignore` такие пути уже исключены (`bin\\*/`), поэтому в git-сборку и Docker-контекст они не попадут.

**Порт 5432 занят локальным PostgreSQL**

Это учтено: на хосте по умолчанию используется `55432`. Внутри сети compose БД всегда доступна как `db:5432`.

---

## Статус

- **Версия 1.0** — начальная реализация, данные в памяти приложения.
- **Версия 2.0** — текущая реализация: PostgreSQL, полный CRUD, Docker Compose, Swagger, автоматические миграции.

### Проверено

| Проверка | Результат |
| --- | --- |
| `docker compose up -d --build` с чистого volume | БД `training_courses` и роль `training_user` созданы |
| Таблицы после старта API | `courses`, `lessons`, `enrollments`, `__EFMigrationsHistory` |
| Миграции | `Applying migration '20261016115536_InitialCreate'` в логах API |
| `./scripts/verify-crud.sh` | 31 успешно, 0 неуспешно |
| `./scripts/verify-persistence.sh` | 9 успешно, 0 неуспешно |
| `GET /` | `{"message":"Course Tracker API is running","version":"2.0",...}` |
| Команды `CREATE USER/DATABASE` | выполнены в тестовой БД, синтаксис подтверждён |