# 🚀 Запуск AnalyticsService

## Быстрый старт

```powershell
# Просто запустите скрипт - он выполнит все проверки и запустит сервис
.\scripts\start_analytics_service.ps1
```

## Что делает скрипт

1. ✅ **Проверяет и автоматически запускает PostgreSQL через Docker**
   - Проверяет порт 5432
   - Если PostgreSQL недоступен - запускает через Docker Compose
   - Проверяет существование базы данных `analytics-db`
   - Создает базу, если её нет (если доступен psql)
   - Ожидает готовности PostgreSQL перед продолжением

2. ✅ **Применяет миграции**
   - Автоматически выполняет `dotnet ef database update`
   - Создает таблицы в базе данных

3. ✅ **Проверяет и автоматически запускает Kafka через Docker**
   - Проверяет порт 9092 (или указанный)
   - Если Kafka недоступен - запускает через Docker Compose (Zookeeper + Kafka)
   - Ожидает готовности Kafka перед продолжением

4. ✅ **Обновляет конфигурацию**
   - Проверяет и обновляет строку подключения к БД
   - Проверяет и обновляет адрес Kafka

5. ✅ **Запускает AnalyticsService**
   - Выполняет `dotnet run`
   - Отображает логи в консоли

## Параметры

```powershell
# Пропустить проверку БД (не запускать через Docker)
.\scripts\start_analytics_service.ps1 -SkipDbCheck

# Пропустить проверку Kafka (не запускать через Docker)
.\scripts\start_analytics_service.ps1 -SkipKafkaCheck

# Пропустить применение миграций
.\scripts\start_analytics_service.ps1 -SkipMigrations

# Пропустить автоматический запуск через Docker (только проверка)
.\scripts\start_analytics_service.ps1 -SkipDockerStart

# Указать другой адрес Kafka
.\scripts\start_analytics_service.ps1 -KafkaBootstrapServer "localhost:29091"

# Указать другие параметры БД
.\scripts\start_analytics_service.ps1 -DbHost "localhost" -DbPort 5432 -DbName "analytics-db" -DbUser "postgres" -DbPassword "postgres"

# Комбинация параметров
.\scripts\start_analytics_service.ps1 -SkipDbCheck -KafkaBootstrapServer "kafka-server:9092"
```

## Примеры использования

### Пример 1: Полный запуск с проверками

```powershell
.\scripts\start_analytics_service.ps1
```

### Пример 2: Запуск без проверок (если всё уже настроено)

```powershell
.\scripts\start_analytics_service.ps1 -SkipDbCheck -SkipKafkaCheck -SkipMigrations
```

### Пример 2.1: Запуск без автоматического запуска Docker (только проверка)

```powershell
.\scripts\start_analytics_service.ps1 -SkipDockerStart
```

### Пример 3: Запуск с Kafka на другом порту (Docker Compose)

```powershell
.\scripts\start_analytics_service.ps1 -KafkaBootstrapServer "localhost:29091"
```

### Пример 4: Запуск с удаленной БД

```powershell
.\scripts\start_analytics_service.ps1 -DbHost "db-server" -DbUser "analytics_user" -DbPassword "password123"
```

## Требования

### Обязательные:
- ✅ .NET 8.0 SDK или выше
- ✅ Docker Desktop (для автоматического запуска PostgreSQL и Kafka)

### Опциональные:
- `psql` (для автоматического создания БД, если PostgreSQL уже запущен)
- Локальный PostgreSQL (если не хотите использовать Docker)
- Локальный Kafka (если не хотите использовать Docker)

## Что проверить перед запуском

1. **PostgreSQL запущен:**
   ```powershell
   Test-NetConnection localhost -Port 5432
   ```

2. **Kafka запущен:**
   ```powershell
   Test-NetConnection localhost -Port 9092
   ```

3. **База данных существует:**
   ```sql
   CREATE DATABASE analytics-db;
   ```

## После запуска

1. **Проверьте логи** - должны появиться записи:
   ```
   [Info] Запуск Kafka Consumer для топика: portfolio.transactions
   [Info] Успешно подписались на топик portfolio.transactions
   ```

2. **Отправьте тестовое сообщение:**
   ```powershell
   .\scripts\send_test_kafka_message.ps1
   ```

3. **Проверьте базу данных:**
   ```sql
   SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 5;
   ```

## Устранение проблем

### Проблема: PostgreSQL недоступен

**Решение:**
```powershell
# Запустите PostgreSQL локально или через Docker
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres

# Или пропустите проверку
.\scripts\start_analytics_service.ps1 -SkipDbCheck
```

### Проблема: Kafka недоступен

**Решение:**
```powershell
# Запустите Kafka через Aspire
cd src\StockMarketAssistant.AppHost
dotnet run

# Или через Docker Compose
cd src\backend\services\StockCardService
docker-compose -f docker-compose_StockCard.yml up -d kafka zookeeper

# Или пропустите проверку
.\scripts\start_analytics_service.ps1 -SkipKafkaCheck
```

### Проблема: Ошибка миграций

**Решение:**
```powershell
# Примените миграции вручную
cd src\backend\services\AnalyticsService\AnalyticsService.WebApi
dotnet ef database update --project ..\AnalyticsService.Infrastructure.EntityFramework --startup-project .

# Или пропустите миграции
.\scripts\start_analytics_service.ps1 -SkipMigrations
```

## Связанные скрипты

- `send_test_kafka_message.ps1` - Отправка тестового сообщения в Kafka
- `check_kafka_consumer.ps1` - Диагностика Kafka Consumer
- `stop_all_services.ps1` - Остановка всех сервисов

## Дополнительная информация

- 📖 [DIAGNOSTICS.md](DIAGNOSTICS.md) - Подробная диагностика проблем
- 📖 [FIX_DATABASE_CONNECTION.md](FIX_DATABASE_CONNECTION.md) - Исправление проблем с БД
- 📖 [KAFKA_SETUP.md](KAFKA_SETUP.md) - Настройка Kafka

