# Training Course Tracker

Система отслеживания учебных курсов, построена по микросервисной архитектуре.

---

## 1. Идея системы

**Training Course Tracker** позволяет пользователям просматривать каталог образовательных курсов, записываться на них и отслеживать свой прогресс обучения. Административная часть (каталог курсов) управляет списком курсов и объёмом учебных часов.

Цель — дать пользователю личный кабинет, где он видит, на какие курсы записан, и какую часть материала уже прошёл.

---

## 2. Минимальный пользовательский сценарий

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

## C4 Диаграммы

Архитектура dokumentирована в нотации C4. Изображения в репозитории:

| Уровень | Файл | Описание |
|---------|------|----------|
| **Level 1 — Context** | `C4 Level 1 — Context.png` | Пользователь, веб-интерфейс, 3 микросервиса и внешняя база данных |
| **Level 2 — Container** | `C4 Level 2 — Container.png` | Контейнеры: nginx-lb, веб-интерфейс, course-service, auth-service, progress-service, PostgreSQL |
| **Level 3 — Component** | `C4 Level 3 — Component.png` | Внутренние компоненты Course Service: API, сервис, репозиторий, общие модели |


---

## 4. Структура репозитория

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
```

---

## 5. Запуск

Требуются: Docker + Docker Compose.

Создайте `.env` (или проверьте существующий):

```env
POSTGRES_PASSWORD=root
JWT_SECRET=your-super-secret-key
```

Запустите все сервисы:

```bash
docker compose up --build
```

Откройте в браузере:

```
http://localhost
```

Балансировщик (`nginx-lb`) принимает трафик на порт 80 и маршрутизирует:
- `/` → веб-интерфейс (`web-frontend`)
- `/api/courses*` → `course-service`
- `/api/auth*` → `auth-service`
- `/api/progress*` → `progress-service`

Остановить и удалить контейнеры:

```bash
docker compose down
```

### Проверка API напрямую (через балансировщик)

```bash
# Список курсов
curl http://localhost/api/courses

# Конкретный курс
curl http://localhost/api/courses/1

# Регистрация
curl -X POST http://localhost/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"username":"ivan","password":"secret"}'

# Логин
curl -X POST http://localhost/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"ivan","password":"secret"}'
```

---

## 6. Технологии

- **Балансировщик / прокси:** nginx
- **Фронтенд:** HTML5 + CSS3 + vanilla JS (без фреймворков)
- **course-service:** ASP.NET Core Minimal API (.NET 10) + PostgreSQL (EF Core, Npgsql)
- **auth-service:** Node.js + Express + bcryptjs + jsonwebtoken
- **progress-service:** Node.js + Express
- **shared:** C# библиотека моделей (Course, Enrollment, AuthResponse, ApiError) на .NET 10
- **api-client:** Типизированный C# HttpClient-клиент (CourseTrackerClient) для вызовов API через балансировщик
- **Контейнеризация:** Docker + Docker Compose
- **Диаграммы:** C4 (Mermaid, .md-файлы в корне репозитория)
- **Языки:** C#, JavaScript
