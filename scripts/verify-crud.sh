#!/usr/bin/env bash
# Проверка CRUD-операций API трекера учебных курсов.
# Использование: ./scripts/verify-crud.sh [BASE_URL]
set -euo pipefail

BASE="${1:-http://localhost:8080}"
PASS=0
FAIL=0

hdr() { printf '\n\033[1m== %s\033[0m\n' "$1"; }

# check <описание> <ожидаемый_код> <фактический_код>
check() {
  if [[ "$2" == "$3" ]]; then
    printf '  \033[32mOK\033[0m   %-58s [%s]\n' "$1" "$3"
    PASS=$((PASS + 1))
  else
    printf '  \033[31mFAIL\033[0m %-58s [expected %s, got %s]\n' "$1" "$2" "$3"
    FAIL=$((FAIL + 1))
  fi
}

show() { jq . 2>/dev/null || cat; }

code() { curl -s -o /dev/null -w '%{http_code}' "$@"; }
body() { curl -s "$@"; }

################################################################################
hdr "0. Health-check"
r=$(body "$BASE/health"); check "GET /health -> 200" 200 "$(code "$BASE/health")"
echo "     $r"

################################################################################
hdr "1. Courses: Create (POST)"
COURSE_JSON=$(body -X POST "$BASE/api/courses" \
  -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET","description":"Вводный курс по C# и .NET","instructor":"Иван Петров","category":"Programming","credits":6,"startDate":"2026-09-01","isActive":true}')
check "POST /api/courses -> 201" 201 "$(code -X POST "$BASE/api/courses" -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET","description":"Вводный курс по C# и .NET","instructor":"Иван Петров","category":"Programming","credits":6,"startDate":"2026-09-01","isActive":true}')"
echo "$COURSE_JSON" | jq -c '{id, title, instructor, category, credits}' &>/dev/null && echo "     $(echo "$COURSE_JSON" | jq -c '{id,title,instructor,category,credits}')"

CID=$(echo "$COURSE_JSON" | jq -r '.id')
if [[ "$CID" == "null" || -z "$CID" ]]; then echo "  FATAL: не удалось создать курс"; exit 1; fi

################################################################################
hdr "2. Courses: Read (GET)"
r=$(body "$BASE/api/courses/$CID")
check "GET /api/courses/$CID -> 200" 200 "$(code "$BASE/api/courses/$CID")"
echo "     $(echo "$r" | jq -c '{id,title,instructor,credits,isActive}')"

r=$(body "$BASE/api/courses")
check "GET /api/courses (список) -> 200" 200 "$(code "$BASE/api/courses")"
echo "     totalItems=$(echo "$r" | jq -r '.totalItems')  page=$(echo "$r" | jq -r '.page')  pageSize=$(echo "$r" | jq -r '.pageSize')"

check "GET /api/courses/999999 -> 404" 404 "$(code "$BASE/api/courses/999999")"
echo "     $(body "$BASE/api/courses/999999" | jq -c '.message')"

################################################################################
hdr "3. Courses: Update (PUT)"
r=$(body -X PUT "$BASE/api/courses/$CID" -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET (обновлено)","description":"Вводный курс по C# и .NET, 2-е издание","instructor":"Иван Петров","category":"Programming","credits":8,"startDate":"2026-10-01","isActive":true}')
check "PUT /api/courses/$CID -> 200" 200 "$(code -X PUT "$BASE/api/courses/$CID" -H 'Content-Type: application/json' \
  -d '{"title":"Основы C# и .NET (обновлено)","description":"Вводный курс по C# и .NET, 2-е издание","instructor":"Иван Петров","category":"Programming","credits":8,"startDate":"2026-10-01","isActive":true}')"
echo "     $(echo "$r" | jq -c '{title,credits,startDate}')"

r=$(body -X PATCH "$BASE/api/courses/$CID/status?isActive=false" -H 'Content-Type: application/json' -d '')
check "PATCH /api/courses/$CID/status -> 200" 200 "$(code -X PATCH "$BASE/api/courses/$CID/status?isActive=false" -H 'Content-Type: application/json' -d '')"
echo "     isActive=$(echo "$r" | jq -r '.isActive')"

body -X PATCH "$BASE/api/courses/$CID/status?isActive=true" -H 'Content-Type: application/json' -d '' >/dev/null

################################################################################
hdr "4. Courses: Validation (400)"
check "POST /api/courses с title='X' -> 400" 400 "$(code -X POST "$BASE/api/courses" -H 'Content-Type: application/json' \
  -d '{"title":"X","instructor":"A"}')"
check "POST /api/courses без instructor -> 400" 400 "$(code -X POST "$BASE/api/courses" -H 'Content-Type: application/json' \
  -d '{"title":"Нормальный заголовок"}')"
check "POST /api/courses с credits=999 -> 400" 400 "$(code -X POST "$BASE/api/courses" -H 'Content-Type: application/json' \
  -d '{"title":"Нормальный заголовок","instructor":"Автор","credits":999}')"
echo "     $(body -X POST "$BASE/api/courses" -H 'Content-Type: application/json' -d '{"title":"X","instructor":"A"}' | jq -c '.errors')"

################################################################################
hdr "5. Lessons (вложенный CRUD)"
L1=$(body -X POST "$BASE/api/courses/$CID/lessons" -H 'Content-Type: application/json' \
  -d '{"title":"Введение и настройка среды","durationMinutes":45,"content":"Установка .NET SDK"}')
check "POST /api/courses/$CID/lessons -> 201" 201 "$(code -X POST "$BASE/api/courses/$CID/lessons" -H 'Content-Type: application/json' \
  -d '{"title":"Типы данных и переменные","durationMinutes":60}')"
LID=$(echo "$L1" | jq -r '.id')
echo "     lessonId=$LID order=$(echo "$L1" | jq -r '.order')"

r=$(body "$BASE/api/courses/$CID/lessons")
check "GET /api/courses/$CID/lessons -> 200" 200 "$(code "$BASE/api/courses/$CID/lessons")"
echo "     count=$(echo "$r" | jq 'length')"

check "GET lesson $LID -> 200" 200 "$(code "$BASE/api/courses/$CID/lessons/$LID")"

r=$(body -X PUT "$BASE/api/courses/$CID/lessons/$LID" -H 'Content-Type: application/json' \
  -d '{"title":"Введение и настройка среды (v2)","durationMinutes":55,"content":"Установка .NET SDK и редактора","order":1}')
check "PUT lesson $LID -> 200" 200 "$(code -X PUT "$BASE/api/courses/$CID/lessons/$LID" -H 'Content-Type: application/json' \
  -d '{"title":"Введение и настройка среды (v2)","durationMinutes":55,"content":"Установка .NET SDK и редактора","order":1}')"
echo "     $(echo "$r" | jq -c '{title,durationMinutes}')"

check "POST lesson с durationMinutes=0 -> 400" 400 "$(code -X POST "$BASE/api/courses/$CID/lessons" -H 'Content-Type: application/json' \
  -d '{"title":"Плохой урок","durationMinutes":0}')"
check "GET lesson 999999 -> 404" 404 "$(code "$BASE/api/courses/$CID/lessons/999999")"

################################################################################
hdr "6. Enrollments (вложенный CRUD)"
E1=$(body -X POST "$BASE/api/courses/$CID/enrollments" -H 'Content-Type: application/json' \
  -d '{"studentName":"Анна Смирнова","studentEmail":"anna.smirnova@example.com","status":"Active","progressPercent":10}')
check "POST /api/courses/$CID/enrollments -> 201" 201 "$(code -X POST "$BASE/api/courses/$CID/enrollments" -H 'Content-Type: application/json' \
  -d '{"studentName":"Пётр Волков","studentEmail":"petr.volkov@example.com","status":"Active","progressPercent":0}')"
EID=$(echo "$E1" | jq -r '.id')
echo "     enrollmentId=$EID"

r=$(body -X PUT "$BASE/api/courses/$CID/enrollments/$EID" -H 'Content-Type: application/json' \
  -d '{"studentName":"Анна Смирнова","studentEmail":"anna.smirnova@example.com","status":"Active","progressPercent":75}')
check "PUT enrollment $EID -> 200" 200 "$(code -X PUT "$BASE/api/courses/$CID/enrollments/$EID" -H 'Content-Type: application/json' \
  -d '{"studentName":"Анна Смирнова","studentEmail":"anna.smirnova@example.com","status":"Active","progressPercent":75}')"
echo "     $(echo "$r" | jq -c '{studentName,progressPercent,status}')"

check "POST enrollment с email=bad -> 400" 400 "$(code -X POST "$BASE/api/courses/$CID/enrollments" -H 'Content-Type: application/json' \
  -d '{"studentName":"Тест","studentEmail":"not-an-email"}')"
check "POST enrollment progressPercent=150 -> 400" 400 "$(code -X POST "$BASE/api/courses/$CID/enrollments" -H 'Content-Type: application/json' \
  -d '{"studentName":"Тест","studentEmail":"t@example.com","progressPercent":150}')"

r=$(body "$BASE/api/courses/$CID")
check "Агрегаты в курсе -> 200" 200 "$(code "$BASE/api/courses/$CID")"
echo "     lessonCount=$(echo "$r" | jq -r '.lessonCount') enrollmentCount=$(echo "$r" | jq -r '.enrollmentCount') avgProgress=$(echo "$r" | jq -r '.averageProgress')"

################################################################################
hdr "7. Фильтрация и поиск"
check "GET /api/courses?category=Programming -> 200" 200 "$(code "$BASE/api/courses?category=Programming")"
echo "     total=$(body "$BASE/api/courses?category=Programming" | jq -r '.totalItems')"
check "GET /api/courses?search=Основы -> 200" 200 "$(code "$BASE/api/courses?search=%D0%9E%D1%81%D0%BD%D0%BE%D0%B2%D1%8B")"
echo "     total=$(body "$BASE/api/courses?search=%D0%9E%D1%81%D0%BD%D0%BE%D0%B2%D1%8B" | jq -r '.totalItems')"
check "GET /api/courses?page=1&pageSize=5 -> 200" 200 "$(code "$BASE/api/courses?page=1&pageSize=5")"

################################################################################
hdr "8. Courses: Delete (DELETE)"
check "DELETE lesson -> 204" 204 "$(code -X DELETE "$BASE/api/courses/$CID/lessons/$LID")"
check "DELETE enrollment -> 204" 204 "$(code -X DELETE "$BASE/api/courses/$CID/enrollments/$EID")"
check "GET удалённого lesson -> 404" 404 "$(code "$BASE/api/courses/$CID/lessons/$LID")"
check "DELETE /api/courses/$CID -> 204" 204 "$(code -X DELETE "$BASE/api/courses/$CID")"
check "GET удалённого курса -> 404" 404 "$(code "$BASE/api/courses/$CID")"

################################################################################
hdr "9. Swagger / OpenAPI"
check "GET /swagger/v1/swagger.json -> 200" 200 "$(code "$BASE/swagger/v1/swagger.json")"
check "GET /swagger -> 200" 200 "$(code "$BASE/swagger/index.html")"

################################################################################
printf '\n\033[1m===== ИТОГ: %d успешно, %d неуспешно =====\033[0m\n' "$PASS" "$FAIL"
[[ "$FAIL" -eq 0 ]] || exit 1