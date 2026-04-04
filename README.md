# EventFlow
Учебный проект ASP.NET Core Web API для управления мероприятиями с CRUD-операциями, DI и Swagger.

## Что реализовано

- CRUD-операции для событий
- хранение данных в памяти приложения
- бизнес-логика вынесена в сервис
- сервис подключён через Dependency Injection
- валидация входных данных
- Swagger для тестирования API

## Стек

- C#
- .NET 10
- ASP.NET Core Web API
- Swagger / Swashbuckle

## Структура сущности Event

Событие содержит поля:

- `Id` - идентификатор события (`Guid`)
- `Title` - название события
- `Description` - описание события
- `StartAt` - дата и время начала
- `EndAt` - дата и время окончания

## Валидация

Проверяется:
- обязательность `Title`
- максимальная длина `Description`
- обязательность `StartAt`
- обязательность `EndAt`
- `EndAt` должно быть позже `StartAt`

## Как запустить проект

### 1. Клонировать репозиторий
```bash
git clone https://github.com/ItsEND/EventFlow.git
cd .\EventFlow.Api\
```
### 2. Собрать проект
```bash
dotnet build
dotnet restore
```
### 3. Запустить проект
```bash
dotnet run
```

## Swagger
После запуска API Swagger доступен по адресу:
```url
https://localhost:xxxx/swagger
```
или
```url
http://localhost:xxxx/swagger
```
Точный порт будет показан в консоли при запуске приложения.

## Эндпоинты API
### Получить все события
```http
GET /events
```
### Получить событие по id
```http
GET /events/{id}
```
### Создать событие
```http
POST /events
Content-Type: application/json
```
Пример тела запроса:
```JSON
{
  "title": "Team Meeting",
  "description": "Sprint planning",
  "startAt": "2026-04-01T10:00:00",
  "endAt": "2026-04-01T11:00:00"
}
```
### Обновить событие
```http
PUT /events/{id}
Content-Type: application/json
```
Пример тела запроса:
```JSON
{
  "title": "Updated Meeting",
  "description": "Updated description",
  "startAt": "2026-04-01T12:00:00",
  "endAt": "2026-04-01T13:00:00"
}
```
### Удалить событие
```http
DELETE /events/{id}
```
## HTTP-статусы
- `200 OK` - успешное получение или обновление
- `201 Created` - успешное создание
- `204 No Content` - успешное удаление
- `400 Bad Request` - ошибка валидации
- `404 Not Found` событие не найдено

## Особенности реализации
Данные хранятся в памяти приложения (`List<Event>`), поэтому после перезапуска приложения все созданные события удаляются.