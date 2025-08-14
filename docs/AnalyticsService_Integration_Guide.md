# Руководство по интеграции с сервисом анализа рейтинга активов

## Обзор

Сервис анализа рейтинга активов (Analytics Service) предоставляет функциональность для анализа и ранжирования активов на основе данных о транзакциях. Сервис получает данные через Kafka от Portfolio Service и актуальные цены через Redis.

## Архитектура

### Компоненты сервиса

1. **Domain Layer** - доменные сущности и бизнес-логика
2. **Application Layer** - сервисы приложения и DTO
3. **Infrastructure Layer** - Entity Framework, репозитории, Kafka consumer
4. **WebApi Layer** - REST API контроллеры и Swagger документация

### База данных

- **PostgreSQL** с схемой `public` (по умолчанию)
- **Таблицы:**
  - `asset_transactions` - транзакции с активами
  - `asset_ratings` - рейтинги активов

## Интеграция с внешними сервисами

### 1. Portfolio Service (Kafka)

#### Топик: `portfolio-transactions`

**Формат сообщения:**
```json
{
  "id": "uuid",
  "portfolioId": "uuid",
  "stockCardId": "uuid",
  "assetType": "Share|Bond|Crypto",
  "transactionType": "Buy|Sell",
  "quantity": 100,
  "pricePerUnit": 150.50,
  "totalAmount": 15050.00,
  "transactionTime": "2024-01-01T10:00:00Z",
  "currency": "RUB",
  "metadata": "Дополнительная информация"
}
```

**Обязательные поля:**
- `id` - уникальный идентификатор транзакции
- `portfolioId` - идентификатор портфеля
- `stockCardId` - идентификатор актива
- `assetType` - тип актива
- `transactionType` - тип транзакции
- `quantity` - количество активов
- `pricePerUnit` - цена за единицу
- `totalAmount` - общая стоимость
- `transactionTime` - время транзакции
- `currency` - валюта

**Типы активов:**
- `Share` - Акция
- `Bond` - Облигация
- `Crypto` - Криптовалюта

**Типы транзакций:**
- `Buy` - Покупка
- `Sell` - Продажа

### 2. Stock Card Service (Redis)

#### Ключи: `asset-prices:{stockCardId}`

**Формат значения:**
```json
{
  "price": 150.50,
  "currency": "RUB",
  "lastUpdated": "2024-01-01T10:00:00Z",
  "source": "market-data"
}
```

**Обязательные поля:**
- `price` - актуальная цена актива
- `currency` - валюта цены
- `lastUpdated` - время последнего обновления

## API Endpoints

### Базовый URL: `/api/assetrating`

#### 1. Получение рейтинга активов

**POST** `/get-ratings`

**Тело запроса:**
```json
{
  "periodStart": "2024-01-01T00:00:00Z",
  "periodEnd": "2024-01-31T23:59:59Z",
  "assetType": "Share",
  "portfolioId": "uuid",
  "limit": 100,
  "offset": 0,
  "sortBy": "TransactionCount",
  "sortDirection": "Descending"
}
```

**Параметры сортировки:**
- `TransactionCount` - по количеству транзакций
- `TransactionAmount` - по стоимости транзакций
- `BuyCount` - по количеству покупок
- `SellCount` - по количеству продаж
- `BuyAmount` - по стоимости покупок
- `SellAmount` - по стоимости продаж

#### 2. Топ покупаемых активов

**GET** `/top-buying?periodStart=2024-01-01&periodEnd=2024-01-31&limit=10&portfolioId=uuid`

#### 3. Топ продаваемых активов

**GET** `/top-selling?periodStart=2024-01-01&periodEnd=2024-01-31&limit=10&portfolioId=uuid`

#### 4. Пересчет рейтингов

**POST** `/recalculate?periodStart=2024-01-01&periodEnd=2024-01-31&portfolioId=uuid`

#### 5. Статистика

**GET** `/statistics?periodStart=2024-01-01&periodEnd=2024-01-31&portfolioId=uuid`

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "analytics-db": ""
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "analytics-service-group",
    "Topic": "portfolio-transactions",
    "AutoOffsetReset": "Earliest",
    "EnableAutoCommit": false
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "KeyPrefix": "asset-prices:"
  }
}
```

### Переменные окружения

```bash
# База данных
ANALYTICS_DB_HOST=localhost
ANALYTICS_DB_PORT=15432
ANALYTICS_DB_NAME=analytics-db
ANALYTICS_DB_USER=postgres
ANALYTICS_DB_PASSWORD=password

# Kafka
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
KAFKA_GROUP_ID=analytics-service-group
KAFKA_TOPIC=portfolio-transactions

# Redis
REDIS_CONNECTION_STRING=localhost:6379
REDIS_DATABASE=0
REDIS_KEY_PREFIX=asset-prices:
```

## Развертывание

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AnalyticsService.WebApi/AnalyticsService.WebApi.csproj", "AnalyticsService.WebApi/"]
COPY ["AnalyticsService.Application/AnalyticsService.Application.csproj", "AnalyticsService.Application/"]
COPY ["AnalyticsService.Domain/AnalyticsService.Domain.csproj", "AnalyticsService.Domain/"]
COPY ["AnalyticsService.Infrastructure.EntityFramework/AnalyticsService.Infrastructure.EntityFramework.csproj", "AnalyticsService.Infrastructure.EntityFramework/"]
COPY ["AnalyticsService.Infrastructure.Repositories/AnalyticsService.Infrastructure.Repositories.csproj", "AnalyticsService.Infrastructure.Repositories/"]

RUN dotnet restore "AnalyticsService.WebApi/AnalyticsService.WebApi.csproj"
COPY . .
WORKDIR "/src/AnalyticsService.WebApi"
RUN dotnet build "AnalyticsService.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalyticsService.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalyticsService.WebApi.dll"]
```

### Docker Compose

```yaml
version: '3.8'
services:
  analytics-service:
    build: .
    ports:
      - "5003:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__AnalyticsDb=Host=postgres;Database=analytics-db;Username=postgres;Password=password
      - Kafka__BootstrapServers=kafka:9092
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - kafka
      - redis

  postgres:
    image: postgres:17.5
    environment:
      POSTGRES_DB: analytics-db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "15432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  kafka:
    image: confluentinc/cp-kafka:latest
    ports:
      - "9092:9092"
    environment:
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1

  zookeeper:
    image: confluentinc/cp-zookeeper:latest
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

## Мониторинг и логирование

### Логи

Сервис использует структурированное логирование с уровнями:
- `Information` - информационные сообщения
- `Warning` - предупреждения
- `Error` - ошибки

### Метрики

Основные метрики для мониторинга:
- Количество обработанных транзакций
- Время обработки запросов
- Количество ошибок
- Размер базы данных

## Безопасность

### Аутентификация

Для продакшена рекомендуется добавить:
- JWT токены
- API ключи
- OAuth 2.0

### Авторизация

Контроль доступа к:
- Данным портфелей
- Административным функциям
- API endpoints

## Тестирование

### Unit тесты

```bash
dotnet test AnalyticsService.Tests.Unit
```

### Integration тесты

```bash
dotnet test AnalyticsService.Tests.Integration
```

### API тесты

```bash
dotnet test AnalyticsService.Tests.Api
```

## Поддержка

### Контакты

- Email: support@stockmarketassistant.com
- Документация: `/swagger` (в режиме разработки)

### Известные проблемы

1. При большом объеме данных может потребоваться оптимизация запросов
2. Рекомендуется настроить партиционирование таблиц по времени
3. Для высоконагруженных систем рассмотрите использование кэширования

## Обновления

### Версия 1.0.0

- Базовая функциональность анализа рейтингов
- Поддержка акций, облигаций и криптовалют
- Kafka интеграция
- Redis интеграция
- Swagger документация
