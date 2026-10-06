Training Course Tracker

REST API для учебного проекта Training Course Tracker.

Проект предназначен для отслеживания учебных курсов, количества часов обучения и прогресса прохождения курсов.

API реализовано на C# с использованием ASP.NET Core Minimal API и запускается как локальное приложение или Docker-контейнер.

Технологии

C#

ASP.NET Core Minimal API

.NET 10

Docker

Ubuntu Linux

API
GET /

Проверка, что API запущено.

curl http://localhost:8080/


Пример ответа:

{
  "message": "Course Tracker API is running",
  "endpoints": [
    "/api/courses",
    "/api/courses/{id}"
  ]
}

GET /api/courses

Получить список всех учебных курсов.

curl http://localhost:8080/api/courses


Пример ответа:

[
  {
    "id": 1,
    "name": "C# и .NET",
    "totalHours": 72,
    "completedHours": 35,
    "completed": false
  },
  {
    "id": 2,
    "name": "Базы данных",
    "totalHours": 48,
    "completedHours": 20,
    "completed": false
  },
  {
    "id": 3,
    "name": "Docker",
    "totalHours": 24,
    "completedHours": 24,
    "completed": true
  }
]

GET /api/courses/{id}

Получить учебный курс по его идентификатору.

curl http://localhost:8080/api/courses/1


Если курс с указанным идентификатором не найден, API возвращает HTTP 404 Not Found.

Локальный запуск без Docker

Перейти в корень проекта:

cd ~/Training_course_tracker


Восстановить зависимости:

dotnet restore


Запустить API:

dotnet run


API запускается по адресу:

http://localhost:8080


Проверить API:

curl http://localhost:8080/
curl http://localhost:8080/api/courses
curl http://localhost:8080/api/courses/1


Для остановки приложения нажать Ctrl+C.

Docker
Сборка Docker-образа

Из корня проекта:

cd ~/Training_course_tracker


Собрать Docker-образ:

docker build -t course-tracker-api:1.0 .


Проверить созданный образ:

docker images

Запуск контейнера

Запустить контейнер с пробросом порта:

docker run -d \
  --name course-tracker \
  -p 8080:8080 \
  course-tracker-api:1.0


Порт 8080 хоста перенаправляется на порт 8080 контейнера.

Проверка контейнера

Проверить запущенные контейнеры:

docker ps


Также можно посмотреть все контейнеры:

docker ps -a

Проверка API в контейнере

Проверить главную страницу API:

curl http://localhost:8080/


Проверить список курсов:

curl http://localhost:8080/api/courses


Проверить отдельный курс:

curl http://localhost:8080/api/courses/1


API также можно открыть в браузере:

http://localhost:8080/
http://localhost:8080/api/courses
http://localhost:8080/api/courses/1

Проверка логов контейнера

Просмотреть логи:

docker logs course-tracker


Для просмотра логов в режиме реального времени:

docker logs -f course-tracker


Для остановки просмотра нажать Ctrl+C.

Остановка и удаление контейнера

Остановить контейнер:

docker stop course-tracker


Удалить контейнер:

docker rm course-tracker

Полный сценарий демонстрации
1. Проверка API без Docker

Запустить:

dotnet run


В другом терминале выполнить:

curl http://localhost:8080/api/courses


Остановить API:

Ctrl+C

2. Сборка Docker-образа
docker build -t course-tracker-api:1.0 .

3. Запуск контейнера
docker run -d \
  --name course-tracker \
  -p 8080:8080 \
  course-tracker-api:1.0

4. Проверка контейнера
docker ps

5. Проверка API
curl http://localhost:8080/api/courses

6. Проверка логов
docker logs course-tracker

Соответствие этапам задания
Этап	Выполнение
1. Продолжить проектную тему	Training Course Tracker
2. Создать API-приложение	C# / ASP.NET Core Minimal API
3. Добавить endpoint	GET /api/courses, GET /api/courses/{id}
4. Проверить API без Docker	dotnet run + curl
5. Написать Dockerfile	Dockerfile в корне проекта
6. Собрать Docker-образ	docker build
7. Запустить контейнер	docker run -p
8. Проверить контейнер	docker ps
9. Проверить API	curl / браузер
10. Проверить логи	docker logs
11. Описать команды	Данный README
Структура проекта
Training_course_tracker/
├── CourseTrackerApi.csproj
├── Dockerfile
├── Program.cs
├── README.md
├── git-conflictmd
├── .gitignore
├── bin/
└── obj/

Папки bin/ и obj/ создаются автоматически при работе с .NET и не должны добавляться в Git благодаря .gitignore.
