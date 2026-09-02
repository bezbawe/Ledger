# Ledger

Учебный проект: банковский счёт на **Event Sourcing + CQRS**. Баланс счёта — не первичная запись,
а проекция истории событий (`AccountOpened`, `MoneyDeposited`, `MoneyWithdrawn`); полная история
операций доступна как встроенный аудит.

Полное ТЗ и обоснование архитектурных решений — [`docs/tz.md`](docs/tz.md).
Статус реализации по этапам — [`docs/plan.md`](docs/plan.md).

## Архитектура

```
src/
  Ledger.Domain          — агрегат Account, события, инварианты (без EF)
  Ledger.EventStore      — EventRecord, IEventStreamRepository (без EF)
  Ledger.Projections     — AccountBalance, IAccountBalanceRepository (без EF)
  Ledger.Application     — MediatR-команды/запросы + LedgerDbContext, EF-репозитории
  Ledger.Web             — HTTP API (ASP.NET Core Minimal API)
  Ledger.Tests           — xUnit, EF Core InMemory
```

Write side принимает команды, восстанавливает агрегат реплеем событий из `Events` (append-only,
PostgreSQL) и пишет новое событие с optimistic concurrency. Read side — проекция `AccountBalance`,
обновляется синхронно, в той же транзакции, что и запись события.

Стек: .NET 9, ASP.NET Core Minimal API, EF Core + Npgsql, MediatR, PostgreSQL, xUnit.

## Запуск

Нужны .NET 9 SDK и Docker.

```bash
docker compose up -d          # поднимает PostgreSQL
dotnet run --project src/Ledger.Web
```

Таблицы (`Events`, `AccountBalances`) создаются автоматически при старте (`EnsureCreated`).
API слушает `http://localhost:5277`.

Тесты (не требуют Postgres, EF InMemory):

```bash
dotnet test src/Ledger.Tests
```

## Демо-сценарий

Готовые запросы — в [`src/Ledger.Web/Ledger.Web.http`](src/Ledger.Web/Ledger.Web.http)
(открывается в Rider/VS Code REST Client). Или через curl:

```bash
# Открыть счёт
curl -s -X POST http://localhost:5277/accounts
# → {"accountId":"...","balance":0,"version":1}

# Пополнить
curl -s -X POST http://localhost:5277/accounts/{accountId}/deposits \
  -H "Content-Type: application/json" -d '{"amount":100}'

# Снять
curl -s -X POST http://localhost:5277/accounts/{accountId}/withdrawals \
  -H "Content-Type: application/json" -d '{"amount":40}'

# Текущий баланс
curl -s http://localhost:5277/accounts/{accountId}/balance

# Полная история событий — то, что демонстрирует идею event sourcing
curl -s http://localhost:5277/accounts/{accountId}/history
```

`GET /health/db` — проверка связности с БД.

## API

| Метод | Путь | Что делает |
|---|---|---|
| `POST` | `/accounts` | Открыть новый счёт |
| `POST` | `/accounts/{id}/deposits` | Пополнить (`{"amount": N}`) |
| `POST` | `/accounts/{id}/withdrawals` | Снять (`{"amount": N}`) |
| `GET` | `/accounts/{id}/balance` | Текущий баланс (проекция) |
| `GET` | `/accounts/{id}/history` | Полная история событий по счёту |
| `GET` | `/health/db` | Проверка подключения к БД |

Ошибки бизнес-правил (недостаточно средств, отрицательная сумма, счёт не найден/уже существует) —
`400`. Конфликт версий при параллельной записи в один стрим — `409`.

## Статус

MVP реализован и проверен (unit-тесты + сквозной прогон на PostgreSQL) — детали и критерии
приёмки см. [`docs/plan.md`](docs/plan.md). Snapshots, upcasting, temporal queries и
асинхронные проекции — в roadmap, сознательно вне MVP.
