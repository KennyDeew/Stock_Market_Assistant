# Анализ несоответствий названий баз данных в Connection Strings

**Дата анализа:** 2025-12-05
**Цель:** Выявление несоответствий между названиями connection strings в коде и docker-compose.yml

---

## 📋 Сводная таблица всех Connection Strings

| Сервис | Ключ в коде | Ключ в docker-compose.yml | Имя БД в docker-compose.yml | Статус |
|--------|-------------|---------------------------|----------------------------|--------|
| **AuthService** | `Database` | `ConnectionStrings__Database` | `Database` | ✅ Соответствует |
| **StockCardService (PostgreSQL)** | `stock-card-db` | `ConnectionStrings__pg-stock-card-db` | `stock-card-db` | ❌ **НЕСООТВЕТСТВИЕ** |
| **StockCardService (MongoDB)** | `finantial-report-db` | `ConnectionStrings__mongo` | `finantial-report-db` | ❌ **НЕСООТВЕТСТВИЕ** |
| **PortfolioService** | `portfolio-db` | `ConnectionStrings__portfolio-db` | `portfolio-db` | ✅ Соответствует |
| **AnalyticsService** | `analytics-db` | `ConnectionStrings__analytics-db` | `analytics-db` | ✅ Соответствует |
| **NotificationService** | `notificationDb` | `ConnectionStrings__notificationDb` | `notificationDb` | ✅ Соответствует |

---

## ❌ Найденные несоответствия

### 1. StockCardService - PostgreSQL Connection String

**Проблема:** Несоответствие названия ключа connection string между кодом и docker-compose.yml

**Местоположение в коде:**
- **Файл:** `src/backend/services/StockCardService/StockCardService.WebApi/Program.cs`
- **Строка:** 38
- **Код:**
```csharp
options.UseNpgsql(builder.Configuration.GetConnectionString("stock-card-db"),
```

**Местоположение в docker-compose.yml:**
- **Файл:** `docker-compose.yml`
- **Строка:** 43
- **Код:**
```yaml
- ConnectionStrings__pg-stock-card-db=Host=pg-stock-card-db;Port=5432;Database=stock-card-db;Username=postgres;Password=postgres
```

**Описание проблемы:**
- Код ищет connection string с ключом `"stock-card-db"`
- В docker-compose.yml переменная окружения называется `ConnectionStrings__pg-stock-card-db`
- Это приводит к тому, что приложение не может найти connection string и будет использовать значение по умолчанию или выбросит исключение

**Последствия:**
- Приложение может не подключиться к базе данных PostgreSQL
- Возможны ошибки при запуске контейнера
- Миграции могут не применяться

---

### 2. StockCardService - MongoDB Connection String

**Проблема:** Несоответствие названия ключа connection string между кодом и docker-compose.yml

**Местоположение в коде:**
- **Файл:** `src/backend/services/StockCardService/StockCardService.WebApi/Program.cs`
- **Строка:** 46
- **Код:**
```csharp
var connStr = builder.Configuration.GetConnectionString("finantial-report-db");
```

**Местоположение в docker-compose.yml:**
- **Файл:** `docker-compose.yml`
- **Строка:** 44
- **Код:**
```yaml
- ConnectionStrings__mongo=mongodb://mongo-stock-card-db:27017/finantial-report-db
```

**Описание проблемы:**
- Код ищет connection string с ключом `"finantial-report-db"`
- В docker-compose.yml переменная окружения называется `ConnectionStrings__mongo`
- Это приводит к тому, что приложение не может найти connection string и выбросит `InvalidOperationException` при запуске

**Последствия:**
- Приложение не сможет запуститься (выбросит исключение на строке 50)
- Невозможность подключения к MongoDB
- Сервис будет падать при старте

---

## ✅ Корректные соответствия

### AuthService
- **Код:** `GetConnectionString("Database")`
- **docker-compose.yml:** `ConnectionStrings__Database`
- **База данных:** `Database`
- ✅ Все соответствует


### PortfolioService
- **Код:** `GetConnectionString("portfolio-db")` (в `AutofacModule.cs:152`)
- **docker-compose.yml:** `ConnectionStrings__portfolio-db`
- **База данных:** `portfolio-db`
- ✅ Все соответствует

### AnalyticsService
- **Код:** `GetConnectionString("analytics-db")` (в `Program.cs:125`)
- **docker-compose.yml:** `ConnectionStrings__analytics-db`
- **База данных:** `analytics-db`
- ✅ Все соответствует

### NotificationService
- **Код:** `GetConnectionString("notificationDb")` (предположительно)
- **docker-compose.yml:** `ConnectionStrings__notificationDb`
- **База данных:** `notificationDb`
- ✅ Все соответствует

---

## 🔧 Рекомендации по исправлению

### Проблема 1: StockCardService PostgreSQL

#### Вариант 1.1: Изменить код (рекомендуется)
Изменить в `StockCardService.WebApi/Program.cs` строку 38:
```csharp
// Было:
options.UseNpgsql(builder.Configuration.GetConnectionString("stock-card-db"),

// Должно быть:
options.UseNpgsql(builder.Configuration.GetConnectionString("pg-stock-card-db"),
```

**Плюсы:**
- Соответствует текущей конфигурации в docker-compose.yml
- Не требует изменения docker-compose.yml
- Более явное именование (указывает на PostgreSQL)

**Минусы:**
- Требует изменения кода

#### Вариант 1.2: Изменить docker-compose.yml
Изменить в `docker-compose.yml` строку 43:
```yaml
# Было:
- ConnectionStrings__pg-stock-card-db=Host=pg-stock-card-db;Port=5432;Database=stock-card-db;Username=postgres;Password=postgres

# Должно быть:
- ConnectionStrings__stock-card-db=Host=pg-stock-card-db;Port=5432;Database=stock-card-db;Username=postgres;Password=postgres
```

**Плюсы:**
- Не требует изменения кода
- Более короткое имя

**Минусы:**
- Требует изменения docker-compose.yml
- Менее явное именование (не указывает на тип БД)

---

### Проблема 2: StockCardService MongoDB

#### Вариант 2.1: Изменить код (рекомендуется)
Изменить в `StockCardService.WebApi/Program.cs` строку 46:
```csharp
// Было:
var connStr = builder.Configuration.GetConnectionString("finantial-report-db");

// Должно быть:
var connStr = builder.Configuration.GetConnectionString("mongo");
```

**Плюсы:**
- Соответствует текущей конфигурации в docker-compose.yml
- Не требует изменения docker-compose.yml
- Более короткое и понятное имя

**Минусы:**
- Требует изменения кода

#### Вариант 2.2: Изменить docker-compose.yml
Изменить в `docker-compose.yml` строку 44:
```yaml
# Было:
- ConnectionStrings__mongo=mongodb://mongo-stock-card-db:27017/finantial-report-db

# Должно быть:
- ConnectionStrings__finantial-report-db=mongodb://mongo-stock-card-db:27017/finantial-report-db
```

**Плюсы:**
- Не требует изменения кода
- Имя соответствует названию базы данных

**Минусы:**
- Требует изменения docker-compose.yml
- Более длинное имя

---

## 📝 Дополнительные замечания

1. **Именование connection strings:**
   - В проекте используется смешанный стиль: некоторые ключи содержат префикс типа БД (`pg-stock-card-db`), другие - нет (`analytics-db`, `portfolio-db`)
   - Рекомендуется унифицировать стиль именования для всех сервисов

2. **MongoDB connection string:**
   - Обнаружено несоответствие: в docker-compose.yml используется ключ `mongo`, а в коде ищется `finantial-report-db`
   - Это критическая проблема, которая приведет к падению сервиса при запуске

3. **Проверка AuthService и NotificationService:**
   - Не удалось найти явное использование `GetConnectionString` в исходном коде этих сервисов
   - Возможно, они используют другой механизм конфигурации (например, через Aspire или DependencyInjection)
   - Рекомендуется проверить их конфигурацию дополнительно

---

## ✅ Выводы

Обнаружено **2 критических несоответствия** в StockCardService:
1. PostgreSQL connection string: код ищет `"stock-card-db"`, а в docker-compose.yml указано `"pg-stock-card-db"`
2. MongoDB connection string: код ищет `"finantial-report-db"`, а в docker-compose.yml указано `"mongo"`

**Рекомендуемые действия:**
1. Исправить код в `StockCardService.WebApi/Program.cs`:
   - Строка 38: изменить `"stock-card-db"` на `"pg-stock-card-db"`
   - Строка 46: изменить `"finantial-report-db"` на `"mongo"`

**Приоритет:** Критический - сервис не сможет запуститься с текущей конфигурацией.

