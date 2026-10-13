# SaumiWeb — тонкий клиент SAUMI (React + Python)

**Продуктовая цель:** веб-клиент, привычный пользователям SAUMI (синие тона, лёгкий UI); Oracle-процедуры не меняем — вызываем через средний слой.  
**Стек:** React (слои) + FastAPI/Python (слои) → Oracle.  
**Статус:** декомпиляция + каркас логина (2026-09-10).

---

## Ключевые цели

### 1. Исследование exe → логика, не только имена

Через декомпиляцию (.NET IL + Delphi `mmp32`) **восстанавливать поведение клиента**: порядок экранов, SQL/пакеты, параметры, ветки UI → API → БД. Списки `sm.*` — указатели; эталон — логика exe. Схема БД (`db/saumi485_schema_*.sql`) даёт типы полей и сигнатуры процедур.

### 2. Архитектура поставки

```
React (UI SAUMI-look)
    → FastAPI средний слой (JWT access + refresh, все методы кроме login/refresh закрыты)
        → Repository (сейчас MOCK; Oracle oracledb — код есть, закомментирован/флаг)
            → SM.* процедуры / SQL как в exe
```

Пока **БД нет** — repository возвращает моки; вызовы Oracle пишем рядом и включаем флагом/`USE_ORACLE=1`.

### 3. Паттерны кода (обязательно)

**Frontend**
- Слои: `api` | `services` (логика) | `pages/components` (вёрстка) | `styles` — раздельно
- Ничего секретного/URL API не зашивать → `.env` (`VITE_*`)
- Стили SAUMI: светлый UI, синие акценты, привычная форма входа

**Backend**
- Слои: `api` → `services` → `repositories` → (Oracle/mock)
- Access token + refresh token; защищённые роуты
- Конфиг из env; CORS allowlist; security headers; rate limit на login
- Без хардкода секретов; пароли не логировать

---

## Важные находки (exe)

### `mmp32.exe` — Delphi + DOA
- `TOracleSession` / `TOracleLogon` / `LoginMod.TLoginPrompt`
- Логин: **username / password / database (TNS)**
- История: `Software\Allround Automations\DOA\LogonHistory`
- После connect: `MMP_SessionAfterLogOn` → `AfterLogonInit` → .NET `OracleProvider` / `Inflex_Init`

### .NET-плагины
| Сборка | Роль |
|--------|------|
| `OracleUtils` | TNS |
| `InflexENV` | `SaumiPascalSQL` |
| `InflexData` | `StoredProcedureAccessor` |
| `InflexCore` | права |

Каталоги строк: `docs/mmp32_*.txt`. Логика входа: `docs/login_flow.md`. Главная панель/меню: `docs/menu_flow.md`.

---

## План работ

### Сделано
- [x] Декомпил .NET, строковые карты Oracle
- [x] Схема 4.8.5 + docker-заготовка в `db/`
- [x] Черновик логики логина
- [x] Каркас `backend/` + `frontend/` (логин, JWT, моки)

### Дальше
- [x] Снять меню главной панели из DFM + SQL `mmp32` (`docs/menu_flow.md`, `GET /menu`)
- [ ] Добить post-logon по дизасму/трассировке
- [ ] Пул Oracle-сессии после login → исполнять SQL меню (сейчас мок: статика DFM, динамика пустая)
- [ ] Дизасм `TCheckRole`, `PrepareMenuCreateNewItem`, `PrepareMenuHistory`, `PrepareMenuReports` (склейка деревьев)
- [ ] Экраны по приоритету: UI → service → repo (mock→Oracle)
- [ ] Включить Oracle (`USE_ORACLE=1`) на стенде

---

## Структура

```
Astra1860/SaumiWeb/
  PLAN.md
  docs/           login_flow.md, карты вызовов
  decompile/      bin + src
  db/             схема + docker
  backend/        FastAPI (слои, JWT, mock Oracle)
  frontend/       React+Vite (слои, env, стили SAUMI)
```

## Локальный запуск (пилот)

```bash
# Docker (рекомендуется)
cd Astra1860/SaumiWeb
docker compose up -d --build
# UI: http://localhost:8080
# API через тот же origin: /api/v1/...

# Или без Docker
cd backend && python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
uvicorn app.main:app --reload --port 8000

cd frontend && npm install
cp .env.example .env
npm run dev
```

Логин (мок): любой non-empty user/password/database → токены.  
Включить Oracle позже: `USE_ORACLE=1` + DSN в `backend/.env`.
