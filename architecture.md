# Трекер учебных курсов — проектная документация (Markdown)

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
<mxfile host="app.diagrams.net" modified="2026-11-06T12:00:00Z" agent="Mozilla/5.0" version="22.1.7" type="device">
  <diagram name="C4 Level 1 - Context" id="level1">
    <mxGraphModel dx="1442" dy="790" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <!-- Студент / слушатель -->
        <mxCell id="s1" value="Студент / слушатель - Просматривает каталог курсов, записывается на курс, отслеживает прогресс" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="40" y="80" width="240" height="80" as="geometry" />
        </mxCell>

        <!-- Преподаватель / куратор -->
        <mxCell id="s2" value="Преподаватель / куратор - Создаёт и редактирует курсы и уроки, управляет записями" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="40" y="220" width="240" height="80" as="geometry" />
        </mxCell>

        <!-- Система -->
        <mxCell id="s3" value="Трекер учебных курсов (REST API) - CRUD-веб-сервис для учёта курсов, уроков и прогресса обучения. Хранилище - PostgreSQL 16" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="420" y="120" width="300" height="160" as="geometry" />
        </mxCell>

        <!-- Swagger UI / OpenAPI -->
        <mxCell id="s4" value="Swagger UI / OpenAPI - Документация и тестирование эндпоинтов API" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;strokeWidth=2;dashed=1;" vertex="1" parent="1">
          <mxGeometry x="900" y="60" width="220" height="70" as="geometry" />
        </mxCell>

        <!-- Скрипты проверки -->
        <mxCell id="s5" value="Скрипты проверки (verify-crud, verify-persistence) - Внешние клиенты, сценарии CRUD и сохранности данных по HTTP" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;strokeWidth=2;dashed=1;" vertex="1" parent="1">
          <mxGeometry x="900" y="180" width="240" height="90" as="geometry" />
        </mxCell>

        -- psql / pgAdmin -->
        <mxCell id="s6" value="psql / pgAdmin - Прямое администрирование и отладка базы системы" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;strokeWidth=2;dashed=1;" vertex="1" parent="1">
          <mxGeometry x="900" y="310" width="220" height="70" as="geometry" />
        </mxCell>

        <!-- Ребра -->
        <mxCell id="e1" value="REST API (JSON) HTTPS" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="s1" target="s3">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="280" y="120" as="sourcePoint" />
            <mxPoint x="420" y="130" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e2" value="REST API (JSON) HTTPS" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="s2" target="s3">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="280" y="260" as="sourcePoint" />
            <mxPoint x="420" y="200" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e3" value="OpenAPI HTTP" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="s3" target="s4">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="720" y="140" as="sourcePoint" />
            <mxPoint x="900" y="95" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e4" value="Проверочные запросы HTTPS" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="s5" target="s3">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="900" y="225" as="sourcePoint" />
            <mxPoint x="720" y="190" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e5" value="TCP/5432" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="s6" target="s3">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="900" y="345" as="sourcePoint" />
            <mxPoint x="720" y="270" as="targetPoint" />
          </mxGeometry>
        </mxCell>

      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```

**Направления взаимодействия:** Студент и Преподаватель → система (REST API, HTTPS); система → Swagger UI (отдаёт спецификацию); Внешние скрипты → система (проверочные HTTP-запросы); Инструмент администрирования БД ↔ данные системы (TCP/5432).

---

## 4. C4 Level 2 — Container

Контейнеры: шлюз/балансировщик, API-сервис, PostgreSQL и дополнительный компонент. Уровень не смешивается с контекстом и компонентами.

```mermaid
<mxfile host="app.diagrams.net" modified="2026-11-06T12:00:00Z" agent="Mozilla/5.0" version="22.1.7" type="device">
  <diagram name="C4 Level 2 - Container" id="level2">
    <mxGraphModel dx="1442" dy="790" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <!-- Студент / слушатель -->
        <mxCell id="p1" value="Студент / слушатель - Просматривает курсы, записывается, отслеживает прогресс" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="40" y="80" width="220" height="70" as="geometry" />
        </mxCell>

        <!-- Преподаватель / куратор -->
        <mxCell id="p2" value="Преподаватель / куратор - Создаёт и редактирует курсы и уроки" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="40" y="220" width="220" height="70" as="geometry" />
        </mxCell>

        <!-- Граница: API Gateway / Load Balancer -->
        <mxCell id="gw_boundary" value="API Gateway / Load Balancer" style="group=1;dashed=1;whiteSpace=wrap;html=1;fillColor=#f5f5f5;strokeColor=#666666;strokeWidth=2;pointerEvents=0;" vertex="1" parent="1">
          <mxGeometry x="320" y="40" width="300" height="340" as="geometry" />
        </mxCell>

        <!-- API Gateway / балансировщик (внутри границы) -->
        <mxCell id="gw" value="Шлюз / балансировщик (Nginx или Traefik) - Точка входа: терминация TLS, роутинг запросов на API, ограничение частоты (rate limiting)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="gw_boundary">
          <mxGeometry x="40" y="40" width="220" height="260" as="geometry" />
        </mxCell>

        <!-- Training Course API -->
        <mxCell id="api" value="Training Course API (ASP.NET Core Minimal API) - REST API: курсы, уроки, записи; валидация, маппинг, EF Core, миграции, Swagger UI, health-check" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="720" y="80" width="300" height="200" as="geometry" />
        </mxCell>

        <!-- PostgreSQL 16 -->
        <mxCell id="db" value="PostgreSQL 16 - Реляционное хранилище: курсы, уроки, записи со связями (ON DELETE CASCADE); том db_data сохраняет данные между перезапусками" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="1120" y="80" width="220" height="140" as="geometry" />
        </mxCell>

        <!-- Redis (дополнительный компонент) -->
        <mxCell id="cache" value="Redis - Дополнительный компонент: кэш списков курсов и агрегатов (lessonCount, averageProgress)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="1120" y="280" width="220" height="80" as="geometry" />
        </mxCell>

        <!-- Ребра -->
        <mxCell id="e1" value="REST API (JSON) HTTPS" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="p1" target="gw">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="260" y="115" as="sourcePoint" />
            <mxPoint x="320" y="120" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e2" value="REST API (JSON) HTTPS" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="p2" target="gw">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="260" y="255" as="sourcePoint" />
            <mxPoint x="320" y="250" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e3" value="Пересылает запросы HTTP" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="gw" target="api">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="560" y="210" as="sourcePoint" />
            <mxPoint x="720" y="180" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e4" value="EF Core / Npgsql TCP/5432" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="api" target="db">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="1020" y="180" as="sourcePoint" />
            <mxPoint x="1120" y="150" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e5" value="Кэш списков и агрегатов TCP/6379" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="api" target="cache">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="1020" y="270" as="sourcePoint" />
            <mxPoint x="1120" y="320" as="targetPoint" />
          </mxGeometry>
        </mxCell>

      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
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
<mxfile host="app.diagrams.net" modified="2026-11-06T12:00:00Z" agent="Mozilla/5.0" version="22.1.7" type="device">
  <diagram name="C4 Level 3 - Component API" id="level3">
    <mxGraphModel dx="1442" dy="790" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />

        <!-- Клиент API -->
        <mxCell id="client" value="Клиент API (студент, куратор или шлюз) - Вызывает эндпоинты REST API" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="40" y="200" width="220" height="70" as="geometry" />
        </mxCell>

        <!-- Граница: Training Course API (ASP.NET Core) -->
        <mxCell id="api_boundary" value="Training Course API (ASP.NET Core)" style="group=1;dashed=1;whiteSpace=wrap;html=1;fillColor=#f5f5f5;strokeColor=#666666;strokeWidth=2;pointerEvents=0;" vertex="1" parent="1">
          <mxGeometry x="320" y="30" width="900" height="500" as="geometry" />
        </mxCell>

        <!-- CourseEndpoints -->
        <mxCell id="ep_courses" value="CourseEndpoints - Контроллер курсов (Minimal API endpoints)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="40" y="80" width="200" height="60" as="geometry" />
        </mxCell>

        <!-- LessonEndpoints -->
        <mxCell id="ep_lessons" value="LessonEndpoints - Контроллер уроков (Minimal API endpoints)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="280" y="80" width="200" height="60" as="geometry" />
        </mxCell>

        <!-- EnrollmentEndpoints -->
        <mxCell id="ep_enrollments" value="EnrollmentEndpoints - Контроллер записей студентов (Minimal API endpoints)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="520" y="80" width="200" height="60" as="geometry" />
        </mxCell>

        <!-- ValidationHelper -->
        <mxCell id="validation" value="ValidationHelper - Валидатор: проверка DTO, ответ 400 со списком ошибок" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="40" y="180" width="200" height="50" as="geometry" />
        </mxCell>

        <!-- MappingExtensions -->
        <mxCell id="mapping" value="MappingExtensions - Маппер: сущность EF Core <-> DTO контрактов" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="280" y="180" width="200" height="50" as="geometry" />
        </mxCell>

        <!-- AppDbContext -->
        <mxCell id="dbctx" value="AppDbContext - Репозиторий доступа к данным (EF Core DbContext), миграции при старте" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="520" y="180" width="200" height="50" as="geometry" />
        </mxCell>

        <!-- Модели -->
        <mxCell id="models" value="Модели (Course, Lesson, Enrollment, CourseCategory) - Доменные модели, таблицы БД snake_case" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="280" y="280" width="200" height="60" as="geometry" />
        </mxCell>

        <!-- Контракты -->
        <mxCell id="contracts" value="Контракты (Contracts.cs, DTO) - Схемы входных и выходных данных API" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=2;" vertex="1" parent="api_boundary">
          <mxGeometry x="40" y="280" width="200" height="60" as="geometry" />
        </mxCell>

        <!-- PostgreSQL 16 (вне контейнера API) -->
        <mxCell id="pg" value="PostgreSQL 16 - База данных системы" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;strokeWidth=2;" vertex="1" parent="1">
          <mxGeometry x="1300" y="200" width="220" height="80" as="geometry" />
        </mxCell>

        <!-- Ребра от клиента к эндпоинтам -->
        <mxCell id="e1" value="Маршруты курсов HTTP/JSON" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="client" target="ep_courses">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="260" y="235" as="sourcePoint" />
            <mxPoint x="360" y="110" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e2" value="Маршруты уроков HTTP/JSON" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="client" target="ep_lessons">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="260" y="235" as="sourcePoint" />
            <mxPoint x="380" y="150" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <mxCell id="e3" value="Маршруты записей HTTP/JSON" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="client" target="ep_enrollments">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="260" y="235" as="sourcePoint" />
            <mxPoint x="620" y="190" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- Эндпоинты -> ValidationHelper -->
        <mxCell id="e4" value="Валидация DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_courses" target="validation">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="140" y="140" as="sourcePoint" />
            <mxPoint x="140" y="180" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e5" value="Валидация DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_lessons" target="validation">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="380" y="140" as="sourcePoint" />
            <mxPoint x="380" y="180" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e6" value="Валидация DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_enrollments" target="validation">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="620" y="140" as="sourcePoint" />
            <mxPoint x="620" y="180" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- Эндпоинты -> MappingExtensions -->
        <mxCell id="e7" value="Маппинг сущность <-> DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_courses" target="mapping">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="140" y="140" as="sourcePoint" />
            <mxPoint x="380" y="205" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e8" value="Маппинг сущность <-> DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_lessons" target="mapping">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="380" y="140" as="sourcePoint" />
            <mxPoint x="380" y="205" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e9" value="Маппинг сущность <-> DTO in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_enrollments" target="mapping">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="620" y="140" as="sourcePoint" />
            <mxPoint x="380" y="230" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- Эндпоинты -> Контракты -->
        <mxCell id="e10" value="Входной/выходной контракт in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_courses" target="contracts">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="140" y="140" as="sourcePoint" />
            <mxPoint x="140" y="310" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e11" value="Входной/выходной контракт in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_lessons" target="contracts">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="380" y="140" as="sourcePoint" />
            <mxPoint x="260" y="340" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e12" value="Входной/выходной контракт in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_enrollments" target="contracts">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="620" y="140" as="sourcePoint" />
            <mxPoint x="380" y="340" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- Эндпоинты -> AppDbContext -->
        <mxCell id="e13" value="Сохранение и загрузка курсов EF Core / Npgsql" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_courses" target="dbctx">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="140" y="140" as="sourcePoint" />
            <mxPoint x="620" y="205" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e14" value="Сохранение и загрузка уроков EF Core / Npgsql" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_lessons" target="dbctx">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="380" y="140" as="sourcePoint" />
            <mxPoint x="620" y="205" as="targetPoint" />
          </mxGeometry>
        </mxCell>
        <mxCell id="e15" value="Сохранение и загрузка записей EF Core / Npgsql" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="ep_enrollments" target="dbctx">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="620" y="140" as="sourcePoint" />
            <mxPoint x="620" y="205" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- AppDbContext -> Модели -->
        <mxCell id="e16" value="Работа с доменными сущностями in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="dbctx" target="models">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="620" y="230" as="sourcePoint" />
            <mxPoint x="380" y="310" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- AppDbContext -> PostgreSQL -->
        <mxCell id="e17" value="SQL и применение миграций TCP/5432" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="1" source="dbctx" target="pg">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="720" y="230" as="sourcePoint" />
            <mxPoint x="1300" y="240" as="targetPoint" />
          </mxGeometry>
        </mxCell>

        <!-- MappingExtensions -> Модели -->
        <mxCell id="e18" value="Преобразование моделей in-process" style="endArrow=classic;html=1;strokeColor=#333333;" edge="1" parent="api_boundary" source="mapping" target="models">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="380" y="230" as="sourcePoint" />
            <mxPoint x="380" y="280" as="targetPoint" />
          </mxGeometry>
        </mxCell>

      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
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

**Редактируемый исходник:** три текстовых файла в формате Mermaid (любой текстовый редактор, mermaid.lite, IDE с плагином Mermaid):

- `c4-context.mmd` — схема уровня 1 (Context)
- `c4-container.mmd` — схема уровня 2 (Container)
- `c4-component-api.mmd` — схема уровня 3 (Component для API)

Уровни не смешаны — каждая схема в отдельном файле, все подписи простые, стрелки показывают направление взаимодействия.

**Экспорт в .png / .pdf** (выполнить самостоятельно, ничего на диске создавать не требуется):

```bash
# через mermaid-cli
npx @mermaid-js/mmdc -i c4-context.mmd -o c4-context.png
npx @mermaid-js/mmdc -i c4-container.mmd -o c4-container.png
npx @mermaid-js/mmdc -i c4-component-api.mmd -o c4-component-api.png

# или в PDF
npx @mermaid-js/mmdc -i c4-context.mmd -o c4-context.pdf
```

Альтернатива без установки: скопировать код любого блока выше в [mermaid.live](https://mermaid.live) → кнопка «Download PNG/PDF».
