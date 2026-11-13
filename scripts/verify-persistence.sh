#!/usr/bin/env bash
# Проверка сохранения данных в PostgreSQL после перезапуска контейнеров.
# Использование: ./scripts/verify-persistence.sh
set -euo pipefail

BASE="${1:-http://localhost:8080}"
PASS=0
FAIL=0
STAMP="persistence-$(date +%H%M%S)"

ok()   { printf '  \033[32mOK\033[0m   %s\n' "$1"; PASS=$((PASS + 1)); }
bad()  { printf '  \033[31mFAIL\033[0m %s\n' "$1"; FAIL=$((FAIL + 1)); }

# Ждём готовности API.
wait_for_api() {
  for _ in $(seq 1 60); do
    if [[ "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/health")" == "200" ]]; then return 0; fi
    sleep 2
  done
  echo "API не поднялся"; exit 1
}

echo "== 1. Создаём эталонные данные =="
COURSE=$(curl -s -X POST "$BASE/api/courses" -H 'Content-Type: application/json' -d "{
  \"title\":\"$STAMP\",\"description\":\"проверка сохранности\",\"instructor\":\"Тест Тестов\",
  \"category\":\"DataScience\",\"credits\":5,\"startDate\":\"2026-11-11\",\"isActive\":true}")
CID=$(jq -r '.id' <<<"$COURSE")
echo "   courseId=$CID title=$STAMP"

LESSON=$(curl -s -X POST "$BASE/api/courses/$CID/lessons" -H 'Content-Type: application/json' \
  -d '{"title":"Урок для проверки","durationMinutes":77,"content":"не потерять"}')
LID=$(jq -r '.id' <<<"$LESSON")

ENR=$(curl -s -X POST "$BASE/api/courses/$CID/enrollments" -H 'Content-Type: application/json' \
  -d '{"studentName":"Студент Студентов","studentEmail":"student@example.com","status":"Active","progressPercent":42}')
EID=$(jq -r '.id' <<<"$ENR")
echo "   lessonId=$LID enrollmentId=$EID"

echo
echo "== 2. docker compose restart =="
docker compose restart >/dev/null 2>&1
wait_for_api
GOT=$(curl -s "$BASE/api/courses/$CID")
if [[ "$(jq -r '.title' <<<"$GOT")" == "$STAMP" && "$(jq -r '.instructor' <<<"$GOT")" == "Тест Тестов" \
   && "$(jq -r '.credits' <<<"$GOT")" == "5" && "$(jq -r '.category' <<<"$GOT")" == "DataScience" \
   && "$(jq -r '.lessonCount' <<<"$GOT")" == "1" && "$(jq -r '.enrollmentCount' <<<"$GOT")" == "1" ]]; then
  ok "после restart: курс #${CID} и все поля сохранены"
else
  bad "после restart: данные курса #${CID} повреждены/потеряны -> $GOT"
fi

L=$(curl -s "$BASE/api/courses/$CID/lessons/$LID")
if [[ "$(jq -r '.title' <<<"$L")" == "Урок для проверки" && "$(jq -r '.durationMinutes' <<<"$L")" == "77" ]]; then
  ok "после restart: урок #${LID} сохранён"
else
  bad "после restart: урок #${LID} потерян -> $L"
fi

E=$(curl -s "$BASE/api/courses/$CID/enrollments/$EID")
if [[ "$(jq -r '.studentName' <<<"$E")" == "Студент Студентов" && "$(jq -r '.progressPercent' <<<"$E")" == "42" ]]; then
  ok "после restart: запись #${EID} сохранена"
else
  bad "после restart: запись #${EID} потеряна -> $E"
fi

echo
echo "== 3. docker compose down + up (контейнеры пересоздаются, volume db_data сохраняется) =="
docker compose down >/dev/null 2>&1
docker compose up -d >/dev/null 2>&1
wait_for_api

GOT=$(curl -s "$BASE/api/courses/$CID")
if [[ "$(jq -r '.title' <<<"$GOT")" == "$STAMP" && "$(jq -r '.lessonCount' <<<"$GOT")" == "1" ]]; then
  ok "после down/up: курс #${CID} сохранён в volume db_data"
else
  bad "после down/up: курс #${CID} потерян -> $GOT"
fi

if [[ "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/api/courses/$CID/lessons/$LID")" == "200" ]]; then
  ok "после down/up: урок #${LID} доступен"
else
  bad "после down/up: урок #${LID} недоступен"
fi

if [[ "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/api/courses/$CID/enrollments/$EID")" == "200" ]]; then
  ok "после down/up: запись #${EID} доступна"
else
  bad "после down/up: запись #${EID} недоступна"
fi

echo
echo "== 4. Удаление через API и проверка каскада в БД =="
curl -s -o /dev/null -X DELETE "$BASE/api/courses/$CID"
if [[ "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/api/courses/$CID/lessons/$LID")" == "404" ]]; then
  ok "DELETE курса каскадно удалил уроки"
else
  bad "урок остался после удаления курса"
fi
if [[ "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/api/courses/$CID/enrollments/$EID")" == "404" ]]; then
  ok "DELETE курса каскадно удалил записи"
else
  bad "запись осталась после удаления курса"
fi

LEFT=$(docker compose exec -T db psql -U training_user -d training_courses -tAc \
  "SELECT count(*) FROM lessons WHERE id=$LID;" | tr -d '[:space:]')
if [[ "$LEFT" == "0" ]]; then
  ok "в PostgreSQL строка lessons с id=$LID отсутствует (ON DELETE CASCADE)"
else
  bad "в PostgreSQL осталась осиротевшая запись lessons id=$LID"
fi

printf '\n\033[1m===== ИТОГ: %d успешно, %d неуспешно =====\033[0m\n' "$PASS" "$FAIL"
[[ "$FAIL" -eq 0 ]] || exit 1