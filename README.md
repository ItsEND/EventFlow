# EventFlow

EventFlow — учебная событийно-ориентированная система управления мероприятиями и бронированиями на ASP.NET Core. Монолит из предыдущих спринтов разделён на три самостоятельных микросервиса. Каждый сервис владеет своей базой PostgreSQL, а Bookings и Events обмениваются событием `BookingConfirmed` через Apache Kafka.

Проект использует .NET 10, PostgreSQL, Entity Framework Core, Kafka, Redis, JWT, Docker и xUnit. Kafka работает в режиме KRaft, поэтому ZooKeeper не требуется.

## Состав системы

| Компонент | Ответственность | База данных | Адрес при запуске в Docker |
|---|---|---|---|
| Users API | регистрация, вход и выдача JWT | `eventflow_users` | `http://localhost:5056` |
| Events API | CRUD мероприятий и учёт доступных мест | `eventflow_events` | `http://localhost:5265` |
| Bookings API | создание, фоновое подтверждение и отмена броней | `eventflow_bookings` | `http://localhost:5176` |
| Kafka | доставка `BookingConfirmed` и DLQ | отдельный volume | `localhost:9092` |
| Redis | кеш событий и топ-10 популярных событий | in-memory | `localhost:6379` |
| pgAdmin | просмотр баз данных | отдельный volume | `http://localhost:5050` |

PostgreSQL доступен с хоста на следующих портах:

| Сервис | Порт | База данных |
|---|---:|---|
| Users | `5433` | `eventflow_users` |
| Events | `5434` | `eventflow_events` |
| Bookings | `5435` | `eventflow_bookings` |

Для локальной учебной среды используются учётные данные PostgreSQL `postgres` / `postgres`. Данные pgAdmin: `admin@eventflow.dev` / `admin`.

## Архитектура

Каждый микросервис разделён на слои чистой архитектуры:

- `Domain` — сущности, перечисления и бизнес-правила;
- `Application` — сценарии использования, контракты и абстракции;
- `Infrastructure` — EF Core, PostgreSQL, Redis, Kafka и фоновые сервисы;
- `Api` — Presentation-слой: контроллеры, HTTP-контракты, middleware и конфигурация приложения.

Структура решения:

```text
src/
├── Services/
│   ├── Users/
│   │   ├── EventFlow.Users.Domain
│   │   ├── EventFlow.Users.Application
│   │   ├── EventFlow.Users.Infrastructure
│   │   └── EventFlow.Users.Api
│   ├── Events/
│   │   ├── EventFlow.Events.Domain
│   │   ├── EventFlow.Events.Application
│   │   ├── EventFlow.Events.Infrastructure
│   │   └── EventFlow.Events.Api
│   └── Bookings/
│       ├── EventFlow.Bookings.Domain
│       ├── EventFlow.Bookings.Application
│       ├── EventFlow.Bookings.Infrastructure
│       └── EventFlow.Bookings.Api
└── Shared/
    └── EventFlow.Contracts
```

Сервисы не используют общую схему БД и не вызывают друг друга по HTTP. В брони хранятся только идентификаторы `EventId` и `UserId`. Общий проект `EventFlow.Contracts` содержит контракт `BookingConfirmed` и имена Kafka-топиков.

Каждый сервис имеет собственные EF Core migrations. При старте API миграции автоматически применяются к соответствующей базе данных.

## Стратегия кеширования

Redis используется в Events API как необязательный ускоряющий слой. Источником истины остаётся PostgreSQL: недоступность или очистка Redis не приводит к потере данных и не должна делать API недоступным.

### Событие по идентификатору

Ответ `GET /events/{id}` кешируется по ключу `event:{id}` с TTL 10 минут. Для чтения используется паттерн Cache-Aside:

1. Events API пытается получить `EventDto` из Redis.
2. При попадании значение сразу возвращается клиенту, репозиторий не вызывается.
3. При промахе событие читается из PostgreSQL, преобразуется в DTO и сохраняется в Redis с TTL.

Для отдельного события выбрана инвалидация при записи. После успешного обновления или удаления в PostgreSQL ключ `event:{id}` удаляется. При создании индивидуальная инвалидация не требуется: событие получает новый идентификатор, для которого кеш ещё не мог существовать. Следующий запрос после инвалидации снова прогревает кеш актуальными данными.

Порядок операций — сначала сохранение в PostgreSQL, затем удаление ключа. Поэтому сбой между операциями не ставит под угрозу источник истины. Если удалить устаревший ключ не удалось, возможная несогласованность ограничена TTL индивидуального события.

### Топ-10 популярных событий

Ответ `GET /events/top` кешируется по единому ключу `events:top10` с TTL 1 минута. Популярность определяется долей проданных мест:

```text
(total_seats - available_seats) / total_seats
```

Сортировка и ограничение десятью строками выполняются в PostgreSQL. Для рейтингового агрегата допустимо кратковременное устаревание, поэтому топ обновляется только по TTL. Явная инвалидация после каждого создания, изменения или бронирования создавала бы лишние обращения к Redis и почти лишила бы этот кеш пользы.

### Изменения через Kafka

Обработчик `BookingConfirmed` уменьшает `AvailableSeats` внутри транзакции PostgreSQL. После успешного commit он инвалидирует `event:{id}`. Кеш топа при этом не удаляется и обновится после окончания собственного короткого TTL.

### Недоступность Redis

`IConnectionMultiplexer` зарегистрирован в DI как Singleton и переиспользуется на протяжении жизни приложения. `AbortOnConnectFail` отключён, поэтому Events API может стартовать, когда Redis временно недоступен. Ошибки чтения, записи и удаления кеша логируются внутри Redis-реализации и не передаются клиенту: чтение деградирует до обращения в PostgreSQL.

Значения TTL находятся в конфигурации отдельно для двух сценариев:

- `Cache:EventTtl` — 10 минут;
- `Cache:TopEventsTtl` — 1 минута.

## Надёжный поток BookingConfirmed

Основной топик — `booking-confirmed`, топик необработанных сообщений — `booking-confirmed-dlq`. Событие `BookingConfirmed` содержит:

- `BookingId` — идентификатор брони и идентификатор Inbox/Outbox-сообщения;
- `EventId` — идентификатор мероприятия и ключ Kafka-сообщения;
- `UserId` — идентификатор пользователя;
- `SeatCount` — количество резервируемых мест;
- `ConfirmedAt` — время подтверждения брони.

Поток обработки:

1. Авторизованный пользователь создаёт бронь в Bookings API.
2. Бронь сохраняется со статусом `Pending`, а её идентификатор помещается во внутреннюю очередь фоновой обработки.
3. Фоновый обработчик переводит бронь в `Confirmed` и добавляет событие в таблицу `outbox_messages` одним вызовом `SaveChangesAsync`. Эти изменения фиксируются в одной транзакции БД.
4. `OutboxPublisherBackgroundService` читает неопубликованные сообщения пакетами, отправляет их в Kafka и только после успешной отправки устанавливает `PublishedAt`.
5. Events API читает `BookingConfirmed`, создавая отдельный DI scope для каждого сообщения.
6. `BookingConfirmedInboxHandler` в одной транзакции добавляет запись в `inbox_messages` и уменьшает `AvailableSeats` мероприятия.
7. Kafka offset фиксируется вручную только после завершения обработки сообщения.

### Outbox в Bookings

Таблица `outbox_messages` хранит топик, ключ, JSON payload, время возникновения и публикации, количество попыток и последнюю ошибку. Если Kafka временно недоступна, сообщение остаётся неопубликованным и будет отправлено повторно.

Если Bookings завершится после отправки в Kafka, но до установки `PublishedAt`, событие может быть опубликовано повторно. Поэтому доставка имеет семантику **at least once**, а не exactly once. Kafka producer настроен с `Acks.All` и `EnableIdempotence`, но основную защиту от повторного бизнес-эффекта обеспечивает Inbox на стороне Events.

При старте Bookings все оставшиеся брони со статусом `Pending` снова помещаются во внутреннюю очередь. Это защищает ещё не подтверждённые брони от потери при перезапуске процесса.

### Inbox в Events

В таблице `inbox_messages` поле `MessageId`, равное `BookingId`, является первичным ключом. Обработка события и изменение количества мест выполняются в одной транзакции PostgreSQL. Повторная доставка того же события обнаруживается по `MessageId` и не уменьшает количество мест второй раз.

Запись Inbox получает один из статусов:

- `Processed` — места успешно зарезервированы;
- `IgnoredEventNotFound` — мероприятие не найдено;
- `IgnoredNotEnoughSeats` — свободных мест недостаточно.

Ожидаемые бизнес-ситуации сохраняются как `Ignored` и считаются обработанными. Техническая ошибка обрабатывается с тремя попытками. Если обработка по-прежнему невозможна, исходное сообщение и сведения об ошибке отправляются в `booking-confirmed-dlq`, после чего offset фиксируется.

Итоговая модель даёт доставку at least once и effectively-once изменение базы Events для каждого `BookingId`.

## JWT и права доступа

JWT выдаёт только Users API через `POST /auth/login`. Все три API используют одинаковые значения секрета, издателя и аудитории.

- `POST /auth/register` и `POST /auth/login` доступны без токена;
- `GET /events`, `GET /events/{id}` и `GET /events/top` доступны без токена;
- создание, изменение и удаление мероприятий разрешено только роли `Admin`;
- все endpoints Bookings требуют JWT;
- Bookings читает `UserId` из claim `NameIdentifier`;
- отменить бронь может её владелец или пользователь с ролью `Admin`.

Swagger каждого сервиса поддерживает JWT. В окне `Authorize` достаточно вставить сам токен без префикса `Bearer`.

## Запуск в Docker

Требования:

- Docker Desktop или Docker Engine;
- Docker Compose v2.

Из корня репозитория выполните:

```bash
docker compose up --build -d
```

Compose соберёт Dockerfile каждого API и запустит три базы PostgreSQL, Kafka в режиме KRaft, Redis, три API и pgAdmin. API стартуют после успешных healthcheck зависимых компонентов.

Swagger будет доступен по адресам:

- Users: `http://localhost:5056/swagger`;
- Events: `http://localhost:5265/swagger`;
- Bookings: `http://localhost:5176/swagger`.

Полезные команды:

```bash
# состояние контейнеров
docker compose ps

# логи сервисов, Kafka и Redis
docker compose logs -f users-api events-api bookings-api kafka redis

# остановка с сохранением данных
docker compose down

# остановка и удаление локальных данных
docker compose down -v
```

## Проверка сквозного сценария

### 1. Зарегистрировать администратора

```http
POST http://localhost:5056/auth/register
Content-Type: application/json

{
  "login": "admin",
  "password": "Admin123!",
  "role": "Admin"
}
```

Успешный ответ — `204 No Content`.

### 2. Получить JWT

```http
POST http://localhost:5056/auth/login
Content-Type: application/json

{
  "login": "admin",
  "password": "Admin123!"
}
```

Скопируйте поле `token` из ответа. В следующих защищённых запросах передавайте его так:

```http
Authorization: Bearer <token>
```

### 3. Создать мероприятие

```http
POST http://localhost:5265/events
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Backend Conference",
  "description": "EventFlow Kafka scenario",
  "totalSeats": 3,
  "startAt": "2030-07-01T10:00:00Z",
  "endAt": "2030-07-01T18:00:00Z"
}
```

Сохраните `id` созданного мероприятия.

### 4. Создать бронь

```http
POST http://localhost:5176/bookings/<eventId>/book
Authorization: Bearer <token>
```

Bookings вернёт `202 Accepted` и бронь в статусе `Pending`. Сохраните её `id`.

### 5. Дождаться подтверждения

```http
GET http://localhost:5176/bookings/<bookingId>
Authorization: Bearer <token>
```

После фоновой обработки статус станет `Confirmed`. В базе Bookings появится запись Outbox, которая после публикации получит `PublishedAt`.

### 6. Проверить результат в Events

```http
GET http://localhost:5265/events/<eventId>
```

Поле `availableSeats` должно уменьшиться с `3` до `2`. В базе Events появится Inbox-запись со статусом `Processed`. Повторная доставка сообщения с тем же `BookingId` не уменьшит количество мест ещё раз.

## Основные endpoints

### Users API

| Метод | Маршрут | Доступ |
|---|---|---|
| `POST` | `/auth/register` | публичный |
| `POST` | `/auth/login` | публичный |

### Events API

| Метод | Маршрут | Доступ |
|---|---|---|
| `GET` | `/events` | публичный |
| `GET` | `/events/{id}` | публичный |
| `GET` | `/events/top` | публичный |
| `POST` | `/events` | `Admin` |
| `PUT` | `/events/{id}` | `Admin` |
| `DELETE` | `/events/{id}` | `Admin` |

### Bookings API

| Метод | Маршрут | Доступ |
|---|---|---|
| `POST` | `/bookings/{eventId}/book` | авторизованный пользователь |
| `GET` | `/bookings/{id}` | авторизованный пользователь |
| `DELETE` | `/bookings/{id}` | владелец брони или `Admin` |

## Конфигурация

Локальные значения находятся в `appsettings.json`. При запуске в Docker они переопределяются переменными окружения ASP.NET Core:

| Параметр | Назначение |
|---|---|
| `ConnectionStrings__UserConnection` | база Users |
| `ConnectionStrings__EventConnection` | база Events |
| `ConnectionStrings__BookingConnection` | база Bookings |
| `Kafka__BootstrapServers` | адрес Kafka |
| `Kafka__ConsumerGroup` | consumer group Events |
| `Redis__ConnectionString` | подключение Events API к Redis |
| `Cache__EventTtl` | TTL события по идентификатору |
| `Cache__TopEventsTtl` | TTL топ-10 популярных событий |
| `Jwt__Secret` | общий ключ подписи JWT |
| `Jwt__Issuer` | издатель JWT |
| `Jwt__Audience` | аудитория JWT |
| `Jwt__LifetimeMinutes` | срок действия JWT |

Секреты в `docker-compose.yml` предназначены только для локальной учебной среды. В production их следует получать из защищённого хранилища секретов.

## Локальная разработка

Для запуска API вне контейнеров сначала поднимите инфраструктуру:

```bash
docker compose up -d users-db events-db bookings-db kafka redis
```

Затем запустите API в отдельных терминалах:

```bash
dotnet run --project src/Services/Users/EventFlow.Users.Api
dotnet run --project src/Services/Events/EventFlow.Events.Api
dotnet run --project src/Services/Bookings/EventFlow.Bookings.Api
```

## Сборка и тесты

Сборка всего решения:

```bash
dotnet build EventFlow.slnx
```

Быстрые unit-тесты используют EF Core InMemory:

```bash
dotnet test Tests/EventFlow.Tests/EventFlow.Tests.csproj
```

Интеграционные тесты Inbox используют PostgreSQL Testcontainers, поэтому перед их запуском должен работать Docker:

```bash
dotnet test Tests/EventApi.IntegrationTests/EventApi.IntegrationTests.csproj
```

Тесты проверяют основные сценарии сервисов, попадание и промах кеша, TTL, инвалидацию после изменения события, сортировку топа по проценту проданных мест, создание Outbox-сообщения, атомарность подтверждения брони и Outbox, а также идемпотентную обработку повторных `BookingConfirmed` через Inbox.
