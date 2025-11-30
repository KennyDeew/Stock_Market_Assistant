# Параметры подключения к PostgreSQL AnalyticsService

## 📋 Учетные данные

| Параметр | Значение |
|----------|----------|
| **Host** | `localhost` |
| **Port** | `5432` |
| **Database** | `analytics-db` |
| **Username** | `postgres` |
| **Password** | `postgres` |

## 📋 Строка подключения (Connection String)

```
Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
```

## 📋 Команды для подключения

### Через psql (если установлен локально)

```bash
psql -h localhost -p 5432 -U postgres -d analytics-db
```

При запросе пароля введите: `postgres`

### Через Docker (рекомендуется)

```bash
docker exec -it analytics-postgres psql -U postgres -d analytics-db
```

### Через pgAdmin или другой клиент

- **Host/Server**: `localhost`
- **Port**: `5432`
- **Database**: `analytics-db`
- **Username**: `postgres`
- **Password**: `postgres`

## 🔍 Проверка подключения

### Проверка статуса контейнера

```powershell
docker ps --filter "name=analytics-postgres"
```

### Проверка доступности порта

```powershell
Test-NetConnection -ComputerName localhost -Port 5432
```

### Проверка подключения через Docker

```bash
docker exec analytics-postgres psql -U postgres -d analytics-db -c "SELECT version();"
```

## 🛠️ Решение проблем

### Контейнер не запущен

```bash
docker start analytics-postgres
```

или

```bash
docker-compose -f scripts/docker-compose-analytics.yml up -d postgres
```

### Порт 5432 занят

Проверьте, не запущен ли другой PostgreSQL:

```powershell
netstat -ano | findstr :5432
```

Если порт занят другим процессом, можно изменить порт в `docker-compose-analytics.yml`:

```yaml
ports:
  - "5433:5432"  # Используйте порт 5433 на хосте
```

И обновите строку подключения:

```
Host=localhost;Port=5433;Database=analytics-db;Username=postgres;Password=postgres
```

### База данных не существует

Создайте базу данных:

```bash
docker exec analytics-postgres psql -U postgres -c "CREATE DATABASE analytics-db;"
```

## 📝 Файлы конфигурации

- **appsettings.json**: `src/backend/services/AnalyticsService/AnalyticsService.WebApi/appsettings.json`
- **docker-compose.yml**: `scripts/docker-compose-analytics.yml`

