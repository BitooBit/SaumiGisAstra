# SaumiWeb backend

Слои: `api` → `services` → `repositories` (mock | oracle).

```bash
cd backend
python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env   # задайте JWT_SECRET_KEY
uvicorn app.main:app --reload --port 8000
```

| Метод | Auth | Описание |
|-------|------|----------|
| `POST /api/v1/auth/login` | нет | user / password / database → access+refresh |
| `POST /api/v1/auth/refresh` | нет | ротация refresh |
| `GET /api/v1/auth/me` | Bearer | текущий пользователь |
| `GET /api/v1/health` | нет | liveness |

`USE_ORACLE=0` (по умолчанию) — mock. `USE_ORACLE=1` — `OracleAuthRepository` (код готов).
