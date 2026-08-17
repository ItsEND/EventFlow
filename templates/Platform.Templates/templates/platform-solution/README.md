# PlatformTemplate

Каркас распределённой .NET-платформы. Решение содержит общие Building Blocks, локальные Redis/Kafka и полный набор наблюдаемости. Сервисы добавляются независимо по мере появления предметных границ.

## Добавление сервиса

По умолчанию создаются PostgreSQL, Redis, Kafka, Outbox и Inbox:

```powershell
.\eng\Add-Service.ps1 -Name Catalog
```

Другие варианты:

```powershell
# SQL Server
.\eng\Add-Service.ps1 -Name Orders -Database sqlserver

# Сервис без БД и брокера
.\eng\Add-Service.ps1 -Name Gateway -Database none -NoKafka -NoRedis
```

Каждый сервис получает:

```text
src/Services/<Name>/
├── PlatformTemplate.<Name>.Api
├── PlatformTemplate.<Name>.Application
├── PlatformTemplate.<Name>.Domain
└── PlatformTemplate.<Name>.Infrastructure

Tests/
├── Unit/PlatformTemplate.<Name>.UnitTests
└── Integration/PlatformTemplate.<Name>.IntegrationTests
```

API подключает общие возможности явно:

```csharp
builder.AddPlatformAuthentication();
builder.AddPlatformObservability();
builder.AddPlatformApi();
```

`Authentication` валидирует JWT, но не выдаёт токены. Источник токенов можно реализовать отдельным сервисом или заменить внешним OIDC-провайдером.

## Локальная инфраструктура

```powershell
docker compose up -d
```

Будут запущены PostgreSQL, Redis, Kafka, Jaeger, Prometheus и Grafana. SQL Server включается отдельным профилем:

```powershell
docker compose --profile sqlserver up -d
```

Адреса:

- Jaeger: `http://localhost:16686`;
- Prometheus: `http://localhost:9090`;
- Grafana: `http://localhost:3000` (`admin` / `admin`);
- Kafka: `localhost:9092`;
- Redis: `localhost:6379`.

Сгенерированный API запускается на назначенном ему порту. `Add-Service.ps1` автоматически добавляет этот адрес в Prometheus file discovery.

## База данных

Шаблон намеренно не применяет миграции при старте приложения. После определения модели создайте первую миграцию и применяйте её отдельным шагом развёртывания.

## Секреты

Значение `Jwt:Secret` в `appsettings.json` предназначено только для локальной разработки. Для настоящего окружения используйте переменные окружения, Secret Manager или хранилище секретов.
