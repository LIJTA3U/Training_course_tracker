# Training Course Tracker

REST API для учебного проекта Training Course Tracker.

## О проекте

Проект предназначен для отслеживания учебных курсов,
количества часов обучения и прогресса прохождения курсов.

API реализовано на C# с использованием ASP.NET Core Minimal API.

## Технологии

- C#
- ASP.NET Core Minimal API
- .NET 10
- Docker
- Ubuntu Linux

## API

### GET /

Проверка, что API запущено.

```bash
curl http://localhost:8080/

GET /api/courses
Получить список всех учебных курсов.

curl http://localhost:8080/api/courses

GET /api/courses/{id}
Получить учебный курс по его идентификатору.

curl http://localhost:8080/api/courses/1

Локальный запуск без Docker
Перейти в корень проекта:

cd ~/Training_course_tracker

Восстановить зависимости:

dotnet restore

Запустить API:

dotnet run

API доступно по адресу:

http://localhost:8080

Проверить:

curl http://localhost:8080/
curl http://localhost:8080/api/courses
curl http://localhost:8080/api/courses/1

Для остановки приложения нажать Ctrl+C.

Docker
Сборка Docker-образа
Из корня проекта:

docker build -t course-tracker-api:1.0 .

Проверить созданный образ:

docker images

Запуск контейнера
docker run -d \
  --name course-tracker \
  -p 8080:8080 \
  course-tracker-api:1.0

Проверка контейнера
docker ps

Также можно посмотреть все контейнеры:

docker ps -a

Проверка API в контейнере
curl http://localhost:8080/
curl http://localhost:8080/api/courses
curl http://localhost:8080/api/courses/1

API также можно открыть в браузере:

http://localhost:8080/
http://localhost:8080/api/courses
http://localhost:8080/api/courses/1

Проверка логов
docker logs course-tracker

Для просмотра логов в реальном времени:

docker logs -f course-tracker

Остановить просмотр:

Ctrl+C

Остановка и удаление контейнера
docker stop course-tracker
docker rm course-tracker

Полный сценарий демонстрации
1. Запуск API без Docker
dotnet run

В другом терминале:

curl http://localhost:8080/api/courses

2. Сборка Docker-образа
docker build -t course-tracker-api:1.0 .

3. Запуск контейнера
docker run -d --name course-tracker -p 8080:8080 course-tracker-api:1.0

4. Проверка контейнера
docker ps

5. Проверка API
curl http://localhost:8080/api/courses

6. Проверка логов
docker logs course-tracker

## исправлен конфликт в файле
Был исправлен конфликт в файле database.db: в 2 ветках было разное содержимое. Конфликт был исправлен путём изменения содержимого файла на одинаковое везде.
<img width="1920" height="1080" alt="Снимок экрана (3)" src="https://github.com/user-attachments/assets/16683889-7e90-4e9f-93c8-ce72438eeb38" />

