# Link Shortener

REST API для создания коротких ссылок на ASP.NET Core.

## Возможности

- создание коротких Base62-кодов;
- перенаправление на исходный URL;
- подсчёт переходов и получение статистики;
- удаление ссылок;
- идемпотентность через заголовок `Idempotency-Key`;
- LRU-кэш с TTL;
- SQLite и EF Core migrations;
- unit- и integration-тесты.

## Технологии

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger
- xUnit

## Запуск

Восстановить зависимости:

```powershell
dotnet restore
```

Применить миграции:

```powershell
dotnet ef database update --project LinkShortener
```

Запустить API:

```powershell
dotnet run --project LinkShortener
```

Swagger будет доступен по адресу:

```text
https://localhost:<port>/swagger
```

Порт отображается в терминале после запуска.

## Тесты

```powershell
dotnet test
```

## Примеры запросов

В примерах используется адрес `https://localhost:7110`. При необходимости замени порт.

### Создать короткую ссылку

```powershell
curl.exe -k -X POST "https://localhost:7110/api/links" -H "Content-Type: application/json" -H "Idempotency-Key: request-123" -d "{\"url\":\"https://example.com\"}"
```

Повторный запрос с тем же `Idempotency-Key` вернёт тот же короткий код и не создаст новую запись.

### Перейти по короткой ссылке

```powershell
curl.exe -k -i "https://localhost:7110/abc123"
```

При успешном запросе API вернёт `302 Found` и исходный URL в заголовке `Location`.

### Получить статистику

```powershell
curl.exe -k "https://localhost:7110/api/links/abc123"
```

### Удалить ссылку

```powershell
curl.exe -k -i -X DELETE "https://localhost:7110/api/links/abc123"
```

Успешное удаление возвращает `204 No Content`.