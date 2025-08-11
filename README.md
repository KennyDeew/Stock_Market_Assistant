# Stock Market Assistant

Система для анализа и управления портфелем ценных бумаг, включающая сервисы для работы с акциями, облигациями, криптовалютами и аналитикой.

## Архитектура

Система построена на основе микросервисной архитектуры с использованием:

- **.NET 9.0** - основной фреймворк
- **Entity Framework Core** - ORM для работы с базой данных
- **PostgreSQL** - основная база данных
- **Redis** - кэширование и хранение актуальных цен
- **Kafka** - асинхронная передача данных между сервисами
- **Swagger/OpenAPI** - документация API

## Сервисы

### 1. Gateway Service
API Gateway для маршрутизации запросов к микросервисам.

### 2. Auth Service
Сервис аутентификации и авторизации пользователей.

### 3. Stock Card Service
Управление карточками активов (акции, облигации, криптовалюты).

### 4. Portfolio Service
Управление портфелями пользователей и активами в них.

### 5. Analytics Service ⭐ **НОВЫЙ**
Сервис анализа рейтинга активов на основе транзакций.

**Основные возможности:**
- Анализ самых покупаемых/продаваемых активов
- Рейтинг по количеству транзакций и стоимости
- Анализ в контексте портфеля и глобально
- Получение данных через Kafka от Portfolio Service
- Актуальные цены через Redis

### 6. Notification Service
Сервис уведомлений пользователей.

## Быстрый старт

### Предварительные требования

- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 или VS Code

### Запуск

1. Клонируйте репозиторий:
```bash
git clone <repository-url>
cd Stock_Market_Assistant
```

2. Запустите приложение:
```bash
cd src/StockMarketAssistant.AppHost
dotnet run
```

3. Откройте браузер и перейдите по адресу:
   - Gateway API: http://localhost:5000
   - Analytics Service: http://localhost:5003
   - Swagger UI: http://localhost:5003/swagger

## Аналитический сервис

### API Endpoints

- `POST /api/assetrating/get-ratings` - получение рейтинга активов
- `GET /api/assetrating/top-buying` - топ покупаемых активов
- `GET /api/assetrating/top-selling` - топ продаваемых активов
- `POST /api/assetrating/recalculate` - пересчет рейтингов
- `GET /api/assetrating/statistics` - статистика

### Интеграция

Сервис получает данные о транзакциях через Kafka топик `portfolio-transactions` и актуальные цены через Redis с ключами `asset-prices:{stockCardId}`.

### База данных

PostgreSQL с схемой `analytics` и таблицами:
- `asset_transactions` - транзакции с активами
- `asset_ratings` - рейтинги активов

## Разработка

### Структура проекта

```
src/
├── backend/
│   ├── gateway/           # API Gateway
│   ├── services/
│   │   ├── AnalyticsService/     # Аналитический сервис
│   │   ├── AuthService/          # Сервис аутентификации
│   │   ├── PortfolioService/     # Сервис портфелей
│   │   ├── StockCardService/     # Сервис карточек активов
│   │   └── NotificationService/  # Сервис уведомлений
│   └── shared/            # Общие компоненты
├── StockMarketAssistant.AppHost/  # Хост приложения
└── StockMarketAssistant.ServiceDefaults/  # Общие настройки
```

### Добавление нового сервиса

1. Создайте папку в `src/backend/services/`
2. Добавьте проекты: Domain, Application, Infrastructure, WebApi
3. Обновите `StockMarketAssistant.AppHost/Program.cs`
4. Добавьте необходимые зависимости в `.csproj` файлы

## Тестирование

```bash
# Unit тесты
dotnet test

# Integration тесты
dotnet test --filter Category=Integration

# API тесты
dotnet test --filter Category=API
```

## Развертывание

### Docker

```bash
docker-compose up -d
```

### Kubernetes

```bash
kubectl apply -f k8s/
```

## Документация

- [Руководство по интеграции с Analytics Service](docs/AnalyticsService_Integration_Guide.md)
- API документация доступна через Swagger UI
- Архитектурные решения в папке `docs/`

## Поддержка

- Email: support@stockmarketassistant.com
- Issues: GitHub Issues
- Документация: `/swagger` в каждом сервисе

## Лицензия

MIT License - см. файл [LICENSE](LICENSE)
