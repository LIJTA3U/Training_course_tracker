# Training Course Tracker

Микросервисная система отслеживания учебных курсов: каталог курсов, регистрация/вход (JWT), запись на курс и прогресс обучения.

---

## История этапов разработки

| Этап | Версия | Что сделано |
|------|--------|-------------|
| **Этап 1** | 1.0 | Начальный REST API на .NET (только чтение, данные в памяти) |
| **Этап 2** | 2.0 | Полный CRUD, PostgreSQL 16, EF Core миграции, Swagger, Docker Compose, volume `db_data` |
| **Этап 3** | 3.0 | Разделение на 3 микросервиса (course, auth, progress), балансировщик nginx, веб-интерфейс HTML/CSS/JS |

---

## Идея и пользовательский сценарий

**Идея:** пользователь просматривает каталог образовательных курсов, записывается на них и отслеживает свой прогресс обучения в личном кабинете.

**Минимальный сценарий:**
1. Открыть `http://localhost:8080` — веб-интерфейс через балансировщик.
2. Зарегистрироваться / войти → получить JWT-токен.
3. Раздел «Курсы» → записаться на интересующий курс.
4. Раздел «Мой прогресс» → видеть процент прохождения по каждому курсу.

---

## Архитектура (3+ микросервиса)

| Сервис | Папка | Ответственность | Стек |
|--------|-------|-----------------|------|
| **course-service** | `services/course-service/` | Каталог курсов (`GET /api/courses`, `{id}`) | .NET 10, ASP.NET Core, EF Core + PostgreSQL |
| **auth-service** | `services/auth-service/` | Регистрация, логин, JWT | Node.js + Express |
| **progress-service** | `services/progress-service/` | Запись на курс, прогресс уроков | Node.js + Express |
| **web-frontend** | `web-frontend/` | Статический сайт HTML/CSS/JS | Nginx |
| **nginx-lb** | `nginx-lb/` | Балансировщик + reverse proxy на порту 8080 | Nginx |

Каждый сервис — отдельная папка, own API, own `Dockerfile`. Взаимодействие через общий балансировщик `nginx-lb`.

---

## C4-Диаграммы

### Level 1 — Context

![C4 Level 1 — Context](docs/c4-context.png)

### Level 2 — Container

![C4 Level 2 — Container](docs/c4-container.png)

### Level 3 — Component

![C4 Level 3 — Component](docs/c4-component.png)

### Level 4 — Code

См. подробную внутреннюю структуру компонента Course Service в файле [`docs/c4-code.md`](docs/c4-code.md) (Mermaid диаграмма классов).

### 4. Схема развёртывания (Deployment)

```
[Пользователь / браузер]
         | HTTP :8080
         v
+-----------------------------------+
|   nginx-lb (порт 8080 хоста)      |
|  /api/courses*  -> course-service |
|  /api/auth*     -> auth-service   |
|  /api/progress* -> progress-service|
|  /              -> web-frontend   |
+-----------------------------------+
         | Docker сеть training_net
   +-------------------------------+
   | course-service  :8080 (.NET)  |
   | auth-service    :3000 (Node)  |
   | progress-service:3000 (Node)  |
   | web-frontend    :80 (Nginx)   |
   +-------------------------------+
         |
   +-------------------------------+
   | PostgreSQL 16 :5432 (внутри)  |
   | volume: db_data               |
   +-------------------------------+
```

---

## Быстрый старт

Требуются: Docker + Docker Compose.

```bash
# 1. Создать .env (скопировать пример)
cp .env.example .env

# 2. Запустить все сервисы
docker compose up -d --build

# 3. Проверить состояние всех сервисов
docker compose ps
```

Открыть в браузере: `http://localhost:8080`

---

## Настройки (.env)

| Переменная | По умолчанию |
|------------|--------------|
| `POSTGRES_DB` | `coursetracker` |
| `POSTGRES_USER` | `postgres` |
| `POSTGRES_PASSWORD` | (обязательно) |
| `POSTGRES_HOST_PORT` | `5441` |
| `JWT_SECRET` | (обязательно) |

База данных: образ `postgres:16`, данные хранятся в volume `db_data` (сохраняются после перезапуска).

---

## Проверка API (curl)

Через балансировщик `http://localhost:8080`:

```bash
# Список курсов
curl http://localhost:8080/api/courses

# Курс по ID
curl http://localhost:8080/api/courses/1

# Регистрация
curl -X POST http://localhost:8080/api/auth/register \
  -H 'Content-Type: application/json' -d '{"username":"ivan","password":"secret"}'

# Логин (возвращает JWT)
curl -X POST http://localhost:8080/api/auth/login \
  -H 'Content-Type: application/json' -d '{"username":"ivan","password":"secret"}'

# Запись на курс (нужен Bearer-токен)
curl -X POST http://localhost:8080/api/progress/enroll \
  -H 'Content-Type: application/json' -H "Authorization: Bearer <TOKEN>" \
  -d '{"userId":1,"courseId":1}'
```

Подробные примеры: `curl-examples.txt`

---

## Сохранение данных

При `docker compose down` volume `db_data` остаётся — данные в PostgreSQL не теряются. Удаляются только при `docker compose down -v`.

Остановить: `docker compose down`

---

## Технологии

- **Балансировщик:** nginx
- **course-service:** .NET 10, ASP.NET Core Minimal API, EF Core, Npgsql
- **auth-service / progress-service:** Node.js + Express
- **web-frontend:** HTML5, CSS3, vanilla JS
- **База:** PostgreSQL 16 (Docker-образ)
- **Контейнеризация:** Docker + Docker Compose
- **C4-схемы:** `docs/c4-*.png` (Context, Container, Component)