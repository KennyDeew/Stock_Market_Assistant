# Исправление проблемы таймаута подключения к PostgreSQL

## Дата: 2024-12-19

## Проблема

При запуске AppHost на `https://localhost:17095` возникала ошибка:

```
Npgsql.NpgsqlException: Exception while reading from stream
Inner Exception: TimeoutException: Timeout during reading attempt
```

Ошибка происходила в `AuthService` при попытке выполнить миграции базы данных PostgreSQL.

## Причины проблемы

1. **PostgreSQL контейнер не готов** - Docker контейнер PostgreSQL еще не полностью инициализирован и не готов принимать подключения
2. **Недостаточные таймауты** - Таймауты подключения Npgsql по умолчанию слишком короткие (15 секунд)
3. **Отсутствие задержки** - Сервис пытается подключиться сразу после запуска, не дожидаясь готовности PostgreSQL
4. **Нет retry механизма с достаточными задержками** - Retry policy есть, но задержки могут быть недостаточными

## Внесенные исправления

### 1. Увеличение таймаутов подключения в Npgsql

**Файл:** `src/backend/services/AuthService/src/AuthService.Infrastructure.Postgres/DependencyInjection.cs`

- Увеличены параметры `EnableRetryOnFailure`:
  - `maxRetryCount: 5`
  - `maxRetryDelay: TimeSpan.FromSeconds(30)`
- Добавлен `CommandTimeout(60)` для команд БД

### 2. Добавление таймаутов в строку подключения

**Файл:** `src/StockMarketAssistant.AppHost/Program.cs`

- Добавлены таймауты в строку подключения через `WithEnvironment`:
  ```csharp
  .WithEnvironment("ConnectionStrings__Database",
      $"{pgAuthDb.Resource.ConnectionStringExpression};Timeout=30;Command Timeout=60")
  ```

### 3. Задержка перед миграцией

**Файл:** `src/backend/services/AuthService/src/AuthService.WebApi/Program.cs`

- Добавлена задержка перед миграцией (5 секунд по умолчанию):
  ```csharp
  var startupDelay = builder.Configuration["ASPNETCORE_STARTUP_DELAY"];
  if (!string.IsNullOrEmpty(startupDelay) && int.TryParse(startupDelay, out var delaySeconds) && delaySeconds > 0)
  {
      logger.LogInformation("⏳ Ожидание {Delay} секунд перед миграцией...", delaySeconds);
      await Task.Delay(TimeSpan.FromSeconds(delaySeconds), CancellationToken.None);
  }
  ```

**Файл:** `src/StockMarketAssistant.AppHost/Program.cs`

- Установлена переменная окружения для задержки:
  ```csharp
  .WithEnvironment("ASPNETCORE_STARTUP_DELAY", "5")
  ```

### 4. Улучшение обработки подключения в MigrationManager

**Файл:** `src/backend/services/AuthService/src/AuthService.Infrastructure.Postgres/MigrationManager.cs`

- Улучшена логика установки таймаутов:
  - Проверка, что таймауты не заданы (равны 0)
  - Установка таймаутов: `Timeout = 30`, `CommandTimeout = 60`
- Добавлено логирование таймаутов при подключении
- Улучшена обработка ошибок с безопасным логированием (без пароля)

### 5. Дополнительная задержка в MigrationManager

- Добавлена задержка 1 секунда перед попыткой подключения (в дополнение к задержке в Program.cs)

## Результат

После внесения изменений:

1. ✅ Увеличены таймауты подключения до 30 секунд
2. ✅ Увеличены таймауты выполнения команд до 60 секунд
3. ✅ Добавлена задержка 5 секунд перед миграцией для готовности PostgreSQL
4. ✅ Улучшено логирование для диагностики проблем
5. ✅ Улучшена обработка ошибок подключения

## Рекомендации

1. **При запуске AppHost:**
   - Убедитесь, что Docker Desktop запущен
   - Дождитесь полной инициализации контейнеров PostgreSQL
   - Проверьте логи на наличие ошибок подключения

2. **Если проблема сохраняется:**
   - Увеличьте `ASPNETCORE_STARTUP_DELAY` до 10-15 секунд
   - Проверьте, что порт 14051 (pg-auth-db) не занят другим процессом
   - Проверьте логи Docker контейнера PostgreSQL: `docker logs <container_id>`

3. **Для production:**
   - Используйте health checks для PostgreSQL
   - Настройте readiness probes в Kubernetes
   - Используйте connection pooling с правильными параметрами

## Проверка исправления

После применения изменений:

1. Перезапустите AppHost
2. Проверьте логи AuthService - должно быть сообщение "✅ Подключение к PostgreSQL установлено"
3. Проверьте, что миграции применяются успешно
4. Убедитесь, что сервис доступен на `https://localhost:17095`

