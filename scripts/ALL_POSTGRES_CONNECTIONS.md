# Все строки подключения к PostgreSQL в решении

## 📋 Сводная таблица

| Сервис | Ключ ConnectionString | Host | Port | Database | Username | Password | Файл |
|--------|----------------------|------|------|----------|----------|----------|------|
| **AnalyticsService** | `analytics-db` | localhost | 5432 | analytics-db | postgres | postgres | appsettings.json |
| **AnalyticsService** | `analytics-db` | localhost | 5432 | analytics-db | postgres | postgres | TestDataGenerator/appsettings.json |
| **AnalyticsService** | - | - | 5432 | analytics-db | postgres | postgres | docker-compose-analytics.yml |
| **StockCardService** | `stock-card-db` | postgres-stockcard | 5432 | stock-card-db | postgres | password | docker-compose_StockCard.yml |
| **StockCardService** | - | - | 5432 | stock-card-db | postgres | password | docker-compose_StockCard.yml |
| **PortfolioService** | `portfolio-db` | - | - | - | - | - | appsettings.json (пусто) |
| **AuthService** | `Database` | - | - | - | - | - | appsettings.json (пусто) |
| **NotificationService** | `notificationDb` | - | - | - | - | - | appsettings.json (пусто) |

---

## 1. AnalyticsService

### appsettings.json
**Файл**: `src/backend/services/AnalyticsService/AnalyticsService.WebApi/appsettings.json`

```json
{
  "ConnectionStrings": {
    "analytics-db": "Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres"
  }
}
```

**Строка подключения:**
```
Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
```

**Параметры:**
- Host: `localhost`
- Port: `5432`
- Database: `analytics-db`
- Username: `postgres`
- Password: `postgres`

### TestDataGenerator/appsettings.json
**Файл**: `src/backend/services/AnalyticsService/AnalyticsService.TestDataGenerator/appsettings.json`

```json
{
  "ConnectionStrings": {
    "analytics-db": "Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres"
  }
}
```

**Строка подключения:**
```
Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
```

### docker-compose-analytics.yml
**Файл**: `scripts/docker-compose-analytics.yml`

```yaml
postgres:
  image: postgres:16-alpine
  container_name: analytics-postgres
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
    POSTGRES_DB: analytics-db
  ports:
    - "5432:5432"
```

**Параметры:**
- Host: `localhost` (снаружи) / `analytics-postgres` (внутри Docker сети)
- Port: `5432`
- Database: `analytics-db`
- Username: `postgres`
- Password: `postgres`

**Строка подключения (снаружи Docker):**
```
Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
```

**Строка подключения (внутри Docker сети):**
```
Host=analytics-postgres;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
```

---

## 2. StockCardService

### docker-compose_StockCard.yml
**Файл**: `src/backend/services/StockCardService/docker-compose_StockCard.yml`

#### Environment переменная для сервиса:
```yaml
environment:
  - ConnectionStrings__stock-card-db=Host=postgres-stockcard;Database=stock-card-db;Username=postgres;Password=password;Port=5432
```

**Строка подключения:**
```
Host=postgres-stockcard;Database=stock-card-db;Username=postgres;Password=password;Port=5432
```

**Параметры:**
- Host: `postgres-stockcard` (имя контейнера)
- Port: `5432`
- Database: `stock-card-db`
- Username: `postgres`
- Password: `password`

#### Конфигурация контейнера PostgreSQL:
```yaml
postgres-stockcard:
  image: postgres:latest
  environment:
    - POSTGRES_USER=postgres
    - POSTGRES_PASSWORD=password
    - POSTGRES_DB=stock-card-db
  ports:
    - "5432:5432"
```

**Параметры:**
- Host: `localhost` (снаружи) / `postgres-stockcard` (внутри Docker сети)
- Port: `5432`
- Database: `stock-card-db`
- Username: `postgres`
- Password: `password`

**Строка подключения (снаружи Docker):**
```
Host=localhost;Port=5432;Database=stock-card-db;Username=postgres;Password=password
```

**Строка подключения (внутри Docker сети):**
```
Host=postgres-stockcard;Port=5432;Database=stock-card-db;Username=postgres;Password=password
```

#### pgweb (PostgreSQL Web UI):
```yaml
pgweb:
  environment:
    PGWEB_DATABASE_URL: "postgres://postgres:password@postgres-stockcard:5432/stock-card-db?sslmode=disable"
```

**URL подключения:**
```
postgres://postgres:password@postgres-stockcard:5432/stock-card-db?sslmode=disable
```

---

## 3. PortfolioService

### appsettings.json
**Файл**: `src/backend/services/PortfolioService/PortfolioService.WebApi/appsettings.json`

```json
{
  "ConnectionStrings": {
    "portfolio-db": ""
  }
}
```

**Статус**: ⚠️ Пустая строка подключения

**Использование**: Строка подключения настраивается через Aspire или переменные окружения.

**В коде** (`Program.cs`):
```csharp
var connectionString = builder.Configuration.GetConnectionString("portfolio-db");
```

---

## 4. AuthService

### appsettings.json
**Файл**: `src/backend/services/AuthService/src/AuthService.WebApi/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Database": ""
  }
}
```

**Статус**: ⚠️ Пустая строка подключения

**Использование**: Строка подключения настраивается через Aspire или переменные окружения.

**В коде** (`DependencyInjection.cs`):
```csharp
var cs = sp.GetRequiredService<IOptionsMonitor<ConnectionStringsOptions>>()
           .CurrentValue.Database;
opt.UseNpgsql(cs, ...);
```

---

## 5. NotificationService

### appsettings.json
**Файл**: `src/backend/services/NotificationService/Notification/appsettings.json`

```json
{
  "ConnectionStrings": {
    "notificationDb": ""
  }
}
```

**Статус**: ⚠️ Пустая строка подключения

**Использование**: Строка подключения настраивается через Aspire или переменные окружения.

---

## 6. AppHost (Aspire)

### Program.cs
**Файл**: `src/StockMarketAssistant.AppHost/Program.cs`

Использует .NET Aspire для управления базами данных PostgreSQL:

```csharp
// AuthService Database
var pgAuthDb = builder.AddPostgres("pg-auth-db")
    .WithImage("postgres:17.5")
    .WithDataVolume("auth-pg-data")
    .WithHostPort(14053)
    .AddDatabase("Database");

// StockCardService Database
var pgStockCardDb = builder.AddPostgres("pg-stock-card-db")
    .WithImage("postgres:17.5")
    .WithDataVolume("stock-card-pg-data")
    .WithHostPort(14054)
    .WithPgWeb(n => n.WithHostPort(5000))
    .AddDatabase("notificationDb");

// AnalyticsService Database
var pgAnalyticsDb = builder.AddPostgres("pg-analytics-db")
    .WithImage("postgres:17.5")
    .WithDataVolume("analytics-pg-data")
    .WithHostPort(14055)
    .WithEnvironment("POSTGRES_USER", "postgres")
    .AddDatabase("analytics-db");
```

**Параметры Aspire:**
- **AuthService**: Host `localhost`, Port `14053`
- **StockCardService/NotificationService**: Host `localhost`, Port `14054`
- **AnalyticsService**: Host `localhost`, Port `14055`, User `postgres`

**Примечание**: Aspire автоматически генерирует строки подключения и передает их через `ConnectionStringExpression`.

---

## 📝 Команды для подключения

### AnalyticsService (localhost:5432)
```bash
# Через psql
psql -h localhost -p 5432 -U postgres -d analytics-db

# Через Docker
docker exec -it analytics-postgres psql -U postgres -d analytics-db
```

### StockCardService (localhost:5432)
```bash
# Через psql
psql -h localhost -p 5432 -U postgres -d stock-card-db

# Через Docker
docker exec -it postgres-stockcard psql -U postgres -d stock-card-db
```

### Aspire Databases
```bash
# AuthService (порт 14053)
psql -h localhost -p 14053 -U postgres -d <database_name>

# StockCardService/NotificationService (порт 14054)
psql -h localhost -p 14054 -U postgres -d <database_name>

# AnalyticsService (порт 14055)
psql -h localhost -p 14055 -U postgres -d analytics-db
```

---

## ⚠️ Важные замечания

1. **Конфликт портов**:
   - `AnalyticsService` и `StockCardService` оба используют порт `5432` на хосте
   - При одновременном запуске может возникнуть конфликт
   - Используйте разные порты или запускайте сервисы по отдельности

2. **Пустые строки подключения**:
   - `PortfolioService`, `AuthService`, `NotificationService` имеют пустые строки в `appsettings.json`
   - Они полагаются на Aspire или переменные окружения для настройки подключения

3. **Aspire vs Docker Compose**:
   - Aspire использует порты `14053`, `14054`, `14055`
   - Docker Compose использует стандартный порт `5432`
   - Убедитесь, что используете правильный порт в зависимости от способа запуска

4. **Пароли**:
   - `AnalyticsService`: `postgres`
   - `StockCardService`: `password`
   - Aspire: автоматически генерируется

---

## 🔍 Поиск строк подключения в коде

Для поиска всех использований строк подключения:

```powershell
# Поиск в appsettings.json
Get-ChildItem -Recurse -Filter "appsettings*.json" | Select-String "ConnectionString"

# Поиск в docker-compose
Get-ChildItem -Recurse -Filter "docker-compose*.yml" | Select-String "POSTGRES_|ConnectionString"

# Поиск в коде
Get-ChildItem -Recurse -Filter "*.cs" | Select-String "GetConnectionString|UseNpgsql"
```

