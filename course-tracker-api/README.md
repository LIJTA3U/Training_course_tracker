# Course Tracker API

Учебный проект для колледжа: REST API для трекера учебных курсов.

## Стек

- C#
- ASP.NET Core Minimal API
- .NET 10
- Docker

## Реализованные endpoints

- `GET /` — проверка, что API запущено.
- `GET /api/courses` — получить список учебных курсов.
- `GET /api/courses/{id}` — получить один курс по ID.

## Локальный запуск без Docker

Перейти в каталог проекта:

```bash
cd ~/course-tracker-api
```

Восстановить зависимости:

```bash
dotnet restore
```

Запустить API:

```bash
dotnet run
```

API доступно на `http://localhost:8080`.

Проверка:

```bash
curl http://localhost:8080/
curl http://localhost:8080/api/courses
curl http://localhost:8080/api/courses/1
```

Остановить приложение: `Ctrl+C`.

## Создание Docker-образа

Из каталога проекта:

```bash
docker build -t course-tracker-api:1.0 .
```

Проверить образ:

```bash
docker images
```

## Запуск контейнера

```bash
docker run -d --name course-tracker -p 8080:8080 course-tracker-api:1.0
```

## Проверка контейнера

```bash
docker ps
```

Также можно проверить все контейнеры:

```bash
docker ps -a
```

## Проверка API в контейнере

Через `curl`:

```bash
curl http://localhost:8080/
curl http://localhost:8080/api/courses
curl http://localhost:8080/api/courses/1
```

Через браузер:

- `http://localhost:8080/`
- `http://localhost:8080/api/courses`
- `http://localhost:8080/api/courses/1`

## Проверка логов контейнера

```bash
docker logs course-tracker
```

Для просмотра последних логов в режиме реального времени:

```bash
docker logs -f course-tracker
```

Остановить просмотр: `Ctrl+C`.

## Остановка и удаление контейнера

```bash
docker stop course-tracker
docker rm course-tracker
```

## Полный сценарий для демонстрации

1. `dotnet run`
2. `curl http://localhost:8080/api/courses`
3. `docker build -t course-tracker-api:1.0 .`
4. `docker run -d --name course-tracker -p 8080:8080 course-tracker-api:1.0`
5. `docker ps`
6. `curl http://localhost:8080/api/courses`
7. `docker logs course-tracker`
