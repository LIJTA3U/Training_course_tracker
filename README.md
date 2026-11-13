# Трекер учебных курсов (Training Course Tracker)

Система отслеживания учебных курсов, построена по микросервисной архитектуре.

**REST API для отслеживания учебных курсов, учебных часов и прогресса обучения.** Данные хранятся в **PostgreSQL 17** через Entity Framework Core, схема создаётся миграциями автоматически при старте. Запуск возможен в Docker-контейнерах через `docker compose up`.

---

## Содержание

1. [О проекте](#о-проекте)
2. [Идея системы и сценарий](#идея-системы-и-сценарий)
3. [Архитектура (3+ микросервиса)](#архитектура-3-микросервиса)
4. [C4 Диаграммы](#c4-диаграммы)
5. [Структура репозитория](#структура-репозитория)
6. [Быстрый старт](#быстрый-старт)
7. [Настройки подключения через environment](#настройки-подключения-через-environment)
8. [Модель данных](#модель-данных)
9. [API Endpoints](#api-endpoints)
10. [Примеры запросов](#примеры-запросов)
11. [Проверка CRUD-запросов](#проверка-crud-запросов)
12. [Проверка сохранения данных после перезапуска](#проверка-сохранения-данных-после-перезапуска)
13. [Локальный запуск без Docker](#локальный-запуск-без-docker)
14. [Запуск в Docker](#запуск-в-docker)
15. [Управление контейнерами](#управление-контейнерами)
16. [Работа с миграциями](#работа-с-миграциями)
17. [Решение проблем](#решение-проблем)
18. [Статус](#статус)
19. [Архитектурная документация](#архитектурная-документация)

---

## 1. О проекте

Training Course Tracker — учебный проект для отслеживания прохождения образовательных курсов. API позволяет управлять курсами, уроками и записями студентов: создавать, изменять, удалять и читать данные, отслеживать прогресс обучения, считать учебные часы и определять статус завершения курса.

Версия 2.0 заменяет начальную версию 1.0 (данные в памяти, только чтение): курсы теперь хранятся в PostgreSQL, реализован полный CRUD, добавлены уроки и записи студентов с каскадным удалением, валидация, Swagger UI и health-check. Маршруты `GET /` и `GET /api/courses/{id}` сохранены, но формат ответа `GET /api/courses` изменился — вместо массива возвращается объект с пагинацией.

---

## 2. Идея системы и сценарий

**Идея:** **Training Course Tracker** позволяет пользователям просматривать каталог образовательных курсов, записываться на них и отслеживать свой прогресс обучения. Административная часть (каталог курсов) управляет списком курсов и объёмом учебных часов. Цель — дать пользователю личный кабинет, где он видит, на какие курсы записан, и какую часть материала уже прошёл.

**Минимальный пользовательский сценарий:**

1. Пользователь открывает главную страницу (через балансировщик на порту 80).
2. Регистрируется или логинится → получает JWT-токен.
3. Открывает раздел «Курсы», видит список доступных курсов с прогрессом.
4. Жмёт «Записаться» на интересующий курс.
5. Переходит в «Мой прогресс», видит записи и процент прохождения по каждому курсу.
6. Может отмечать уроки как пройденные (через API сервиса прогресса).

---

## 3. Архитектура (3+ микросервиса)

Каждый сервис lebt в своей папке, имеет अपना API и свой `Dockerfile`.

| Сервис | Папка | Ответственность | API | Порт внутри сети |
|--------|-------|-----------------|-----|------------------|
| **course-service** | `services/course-service/` | Каталог курсов: получение списка и данных курса | `GET /api/courses`, `GET /api/courses/{id}` | 8080 (.NET) |
| **auth-service** | `services/auth-service/` | Регистрация, логин, проверка JWT | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me` | 3000 (Node.js) |
| **progress-service** | `services/progress-service/` | Запись на курс, отметка уроков, прогресс пользователя | `POST /api/progress/enroll`, `POST /api/progress/lesson`, `GET /api/progress/:userId` | 3000 (Node.js) |
| **web-frontend** | `web-frontend/` | Статический сайт HTML/CSS/JS | — (статика) | 8080 (nginx) |
| **nginx-lb** | `nginx-lb/` | **Балансировщик нагрузки** + обратный прокси, маршрутизация `/api/*` на сервисы | — | 80 |
| **shared** | `shared/` | Общая библиотека с моделями на C# (Course, Enrollment, AuthResponse, ApiError) | — | — |
| **api-client** | `api-client/` | Типизированный C# HttpClient-клиент для вызовов API через балансировщик | — | — |

Все микросервисы доступны извне через единую точку входа — `nginx-lb` (порт 80 хоста). Фронтенд и API разделяют один источник, поэтому CORS не требуется.

```
пользователь (браузер)
        |
     порт 80
        |
    +-----v-----+
    | nginx-lb  |  <-- балансировщик + reverse proxy
    +-----+-----+
          |
   +------+------+------+------+
   |      |      |      |      |
 web   course   auth   progress
-frontend service service service
```

---

## 4. C4 Диаграммы

Архитектура dokumentирована в нотации C4. Изображения в репозитории:

| Уровень | Файл | Описание |
|---------|------|----------|
| **Level 1 — Context** | `C4 Level 1 — Context.png` | Пользователь, веб-интерфейс, 3 микросервиса и внешняя база данных |
| **Level 2 — Container** | `C4 Level 2 — Container.png` | Контейнеры: nginx-lb, веб-интерфейс, course-service, auth-service, progress-service, PostgreSQL |
| **Level 3 — Component** | `C4 Level 3 — Component.png` | Внутренние компоненты Course Service: API, сервис, репозиторий, общие модели |

Подробная текстовая документация архитектуры см. в файле [`architecture.md`](architecture.md).

---

## 5. Структура репозитория

```
docker-compose.yml          # оркестрация всех сервисов
.env                        # секреты (POSTGRES_PASSWORD, JWT_SECRET)
C4 Level 1 — Context.png    # C4 Level 1 — Контекст
C4 Level 2 — Container.png  # C4 Level 2 — Контейнеры
C4 Level 3 — Component.png  # C4 Level 3 — Компоненты
nginx-lb/
  Dockerfile                # балансировщик + прокси
  nginx.conf                # конфигурация маршрутизации nginx
web-frontend/
  Dockerfile                # статический сайт (nginx)
  index.html
  style.css
  app.js
services/
  course-service/           # .NET каталог курсов
    Dockerfile
    Program.cs
    CourseTrackerApi.csproj
  auth-service/             # Node.js аутентификация
    Dockerfile
    package.json
    server.js
  progress-service/         # Node.js прогресс
    Dockerfile
    package.json
    server.js
shared/
  CourseTracker.Shared.csproj  # общая библиотека моделей на C# (.NET 10)
  Models/
    Course.cs
    Enrollment.cs
    AuthResponse.cs
    ApiError.cs
api-client/
  CourseTracker.Client.csproj  # типизированный C# HttpClient-клиент
  CourseTrackerClient.cs       # методы: GetCourses, Login, Enroll и т.д.
src/TrainingCourseApi/       # полный .NET-проект API (CRUD, миграции, Swagger)
  Program.cs
  TrainingCourseApi.csproj
  Models/
  Data/
  Endpoints/
  Contracts/
  Mapping/
architecture.md              # C4-диаграммы архитектуры (текстовая версия)
.env.example                 # пример переменных окружения
scripts/
  verify-crud.sh             # автопроверка CRUD-операций
  verify-persistence.sh      # проверка сохранности данных
```

Каталоги `bin/` и `obj/` создаются при сборке и исключены в `.gitignore`.

---

## 6. Быстрый старт

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

## 7. Настройки подключения через environment

Все настройки передаются через **environment**. Скопируйте пример и при необходимости отредактируйте:

```bash
cp .env.example .env
```

| Переменная | Назначение | По умолчанию |
|------------|------------|--------------|
| `POSTGRES_DB` | Имя базы данных | `coursetracker` |
| `POSTGRES_USER` | Пользователь PostgreSQL | `postgres` |
| `POSTGRES_PASSWORD` | Пароль PostgreSQL | (обязательно) |
| `POSTGRES_HOST_PORT` | Порт БД на хосте | `5440` |
| `JWT_SECRET` | Секрет для JWT-токенов | (обязательно) |

Строка подключения для приложения:

```
Host=db;Port=5432;Database=coursetracker;Username=postgres;Password=...
```

Внутри compose-сети PostgreSQL слушает на `db:5432`; на хост пробрасывается только `POSTGRES_HOST_PORT` (по умолчанию `5440`, так как `5432` часто занят локальным PostgreSQL).

Проверка, что настройки дошли до контейнеров:

```bash
docker compose exec course-service printenv ConnectionStrings__DefaultConnection
docker compose exec db printenv POSTGRES_DB POSTGRES_USER
```

Демо-данные (опционально, только если таблица `courses` пуста) заполняются автоматически при старте `course-service`.

---

## 8. Модель данных

```
courses 1 ────< lessons (ON DELETE CASCADE)
         └─────< enrollments   (ON DELETE CASCADE)
```

**`courses`** — учебный курс

| Поле | Тип | Описание |
|------|-----|----------|
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

## 9. API Endpoints

Базовый адрес: `http://localhost:8080`

| Метод | Endpoint | Описание |
|-------|----------|----------|
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

Микросервис `auth-service` (через балансировщик):

| Метод | Endpoint | Описание |
|-------|----------|----------|
| `POST` | `/api/auth/register` | Регистрация пользователя |
| `POST` | `/api/auth/login` | Вход, получение JWT |
| `GET` | `/api/auth/me` | Данные текущего пользователя |

Микросервис `progress-service` (через балансировщик):

| Метод | Endpoint | Описание |
|-------|----------|----------|
| `POST` | `/api/progress/enroll` | Записать пользователя на курс |
| `POST` | `/api/progress/lesson` | Отметить урок как пройденный |
| `GET` | `/api/progress/{userId}` | Прогресс пользователя |

Параметры `GET /api/courses`:

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|--------------|----------|
| `page` | int | `1` | Номер страницы |
| `pageSize` | int | `10` | Размер страницы (1–100) |
| `search` | string | — | Поиск по названию, описанию, преподавателю |
| `category` | `CourseCategory` | — | Фильтр по категории |
| `isActive` | bool | — | Фильтр по активности |

Коды ответов: `201 Created` (с `Location`), `200 OK`, `204 No Content` (удаление), `400 Bad Request` (валидация), `404 Not Found`, `503 Service Unavailable` (нет БД).

---

## 10. Примеры запросов

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

Регистрация пользователя:

```bash
curl -X POST http://localhost/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"username":"ivan","password":"secret"}'
```

Вход (получение JWT):

```bash
curl -X POST http://localhost/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"ivan","password":"secret"}'
```

Конкретный курс:

```bash
curl http://localhost/api/courses/1
```

Остальные примеры запросов и ответов см. в Swagger UI: <http://localhost:8080/swagger>

---

## 11. Проверка CRUD-запросов

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

## 12. Проверка сохранения данных после перезапуска

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
docker compose exec db psql -U postgres -d coursetracker -c 'SELECT * FROM courses;'
```

> Важно: `docker compose down` удаляет контейнеры и сеть, но volume `db_data` остаётся. Данные исчезают только при `docker compose down -v`.

---

## 13. Локальный запуск без Docker

```bash
cd src/TrainingCourseApi
dotnet restore
export ConnectionStrings__TrainingDb="Host=localhost;Port=5432;Database=coursetracker;Username=postgres;Password=your-password"
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

## 14. Запуск в Docker

Основной способ — Docker Compose (поднимает API и БД вместе):

```bash
docker compose up -d --build
```

Запуск только API-образа (без базы — приложение будет повторно пытаться подключиться к БД):

```bash
docker build -t training-course-api:latest .
docker run -d -p 8080:8080 \
  -e ConnectionStrings__TrainingDb="Host=хост;Port=5432;Database=coursetracker;Username=postgres;Password=your-password" \
  training-course-api:latest
```

---

## 15. Управление контейнерами

```bash
docker compose ps             # состояние сервисов
docker compose logs -f api    # логи API (Ctrl+C для выхода)
docker compose logs -f db     # логи БД
docker compose restart        # перезапуск всех сервисов
docker compose stop           # остановить, volume сохраняется
docker compose down           # удалить контейнеры и сеть, volume сохраняется
docker compose down -v        # удалить и volume — данные будут потеряны
```

---

## 16. Работа с миграциями

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

## 17. Решение проблем

**Порт 8080 или 5440 уже занят**

```bash
ss -ltn | grep -E ':8080|:5440'
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

## 18. Статус

- **Версия 1.0** — начальная реализация, данные в памяти, только чтение.
- **Версия 2.0** — текущая: PostgreSQL, полный CRUD, Docker Compose, Swagger, автоматические миграции, каскадное удаление.
- **Микросервисная версия** — course-service, auth-service, progress-service, web-frontend, nginx-lb, shared, api-client.

---

## 19. Архитектурная документация

Подробные C4-диаграммы системы (уровни Context, Container, Component) см. в файле [`architecture.md`](architecture.md).

- [C4 Level 1 — Context](architecture.md#c4-level-1-—-context) — пользователи, система и внешние системы
- [C4 Level 2 — Container](architecture.md#c4-level-2-—-container) — шлюз, API, PostgreSQL, Redis
- [C4 Level 3 — Component](architecture.md#c4-level-3-—-component) — внутренняя структура Training Course API
