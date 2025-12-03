# Анализ плана презентации Stock Market Assistant

## 📊 Анализ целостности и полноты плана презентации

### ✅ Сильные стороны плана

1. **Структура презентации** - логичная последовательность слайдов (15-17 слайдов)
2. **Покрытие основных разделов** - есть разделы по проблеме, решению, архитектуре, безопасности
3. **User Stories** - детализированы по фазам разработки
4. **Технологический стек** - указан, но требует корректировки

### ⚠️ Выявленные проблемы и несоответствия

#### 1. **Критические несоответствия с реальной реализацией**

**Технологический стек:**
- ❌ План указывает: **Node.js 18+, Express.js/NestJS, TypeScript**
- ✅ Реальная реализация: **.NET 8+, ASP.NET Core, C#**

**Backend Framework:**
- ❌ План: Express.js / NestJS
- ✅ Реальность: ASP.NET Core Web API

**Архитектура:**
- ❌ План: упоминает микросервисы, но не детализирует
- ✅ Реальность: **Clean Architecture** с микросервисами

#### 2. **Пропущенные слайды**

- **Слайд 3** - отсутствует (после "Проблема и решение" идет сразу слайд 4)
- **Слайд 10** - раздел "NON-FUNCTIONAL REQUIREMENTS (NFR)" пустой
- **Слайд 11** - отсутствует
- **Слайд 16** - отсутствует (после "Monitoring & Observability" идет сразу слайд 17)

#### 3. **Незаполненные разделы**

**Слайд 4 - Доменная модель:**
- ❌ "Основные сущности" - не указаны
- ❌ "Ключевые связи" - не указаны

**Слайд 9 - Database Schema:**
- ❌ "Основные таблицы" - не указаны
- ❌ "Key Optimizations" - не указаны

**Слайд 10 - NFR:**
- ❌ Полностью пустой раздел

#### 4. **Недостающие компоненты в плане**

**Сервисы:**
- ❌ Не упоминают **AnalyticsService** (аналитика транзакций, рейтинги активов)
- ❌ Не упоминают **NotificationService** (уведомления через Email)
- ✅ Упомянуты: Auth, Portfolio, Quote, Alert

**Базы данных:**
- ✅ Упомянуты: PostgreSQL, Redis, MongoDB, OpenSearch
- ❌ TimescaleDB не используется (удалено из плана)

**Архитектурные паттерны:**
- ❌ Не упоминают **Clean Architecture** (Domain, Application, Infrastructure, Presentation слои)
- ❌ Не упоминают **Event-Driven Architecture** (Kafka для событий)
- ❌ Не упоминают **CQRS** (если используется)

**Инфраструктура:**
- ❌ Не упоминают **.NET Aspire** (для оркестрации микросервисов)
- ❌ Не упоминают **Docker Compose** (для локальной разработки)

#### 5. **Неточности в описании**

**WebSocket:**
- План указывает Socket.IO
- В .NET обычно используется SignalR

**Frontend:**
- ✅ React 18 + TypeScript - соответствует
- ✅ Vite - соответствует

**Message Queue:**
- ✅ Kafka - соответствует
- ❌ Bull - не используется (это для Node.js)

#### 6. **Отсутствующие важные разделы**

- **Микросервисная архитектура** - детальное описание сервисов
- **Event-Driven Communication** - как сервисы взаимодействуют через Kafka
- **Data Consistency** - как обеспечивается консистентность между сервисами
- **API Gateway** - роль и функции Gateway сервиса
- **Clean Architecture** - описание слоев и их ответственности

---

## 🔧 Рекомендации по улучшению плана

### 1. Исправить технологический стек

**Backend:**
- .NET 8+ (вместо Node.js)
- ASP.NET Core Web API (вместо Express/NestJS)
- C# (вместо TypeScript для backend)
- Entity Framework Core (вместо ORM для Node.js)
- SignalR (вместо Socket.IO)

**Infrastructure:**
- .NET Aspire (для оркестрации)
- Docker + Docker Compose
- Kubernetes (для production)

### 2. Добавить описание всех сервисов

**6 микросервисов:**
1. **Gateway** - API Gateway, маршрутизация запросов
2. **AuthService** - аутентификация и авторизация (RBAC)
3. **StockCardService** - управление карточками активов (акции, облигации, криптовалюты)
4. **PortfolioService** - управление портфелями пользователей и транзакциями
5. **AnalyticsService** - аналитика транзакций, расчет рейтингов активов
6. **NotificationService** - отправка уведомлений (Email)

### 3. Заполнить доменную модель

**Основные сущности:**

**PortfolioService:**
- Portfolio (Портфель)
- PortfolioAsset (Актив портфеля)
- PortfolioAssetTransaction (Транзакция)

**StockCardService:**
- ShareCard (Акция)
- BondCard (Облигация)
- CryptoCard (Криптовалюта)
- Multiplier, Dividend, Coupon (связанные данные)

**AnalyticsService:**
- AssetTransaction (Транзакция для аналитики)
- AssetRating (Рейтинг актива)
- Period (Период анализа)

**AuthService:**
- User (Пользователь)
- Role (Роль)
- Permission (Разрешение)

**Ключевые связи:**
- Portfolio → PortfolioAsset (1:N)
- PortfolioAsset → PortfolioAssetTransaction (1:N)
- PortfolioAsset → StockCard (N:1 через StockCardId)
- User → Portfolio (1:N)

### 4. Заполнить схему БД

**Основные таблицы по сервисам:**

**PortfolioService (PostgreSQL):**
- `portfolio` (id, user_id, name, currency)
- `portfolio_asset` (id, portfolio_id, stock_card_id, asset_type)
- `portfolio_asset_transaction` (id, portfolio_asset_id, transaction_type, quantity, price_per_unit, total_amount, transaction_date, currency)

**StockCardService (PostgreSQL + MongoDB):**
- `ShareCards` (id, ticker, name, currency, current_price)
- `BondCards` (id, ticker, name, maturity_period, rating, face_value)
- `CryptoCards` (id, ticker, name)
- `Multipliers`, `Dividends`, `Coupons` (связанные таблицы)
- MongoDB: `FinancialReports` (финансовые отчеты)

**AnalyticsService (PostgreSQL):**
- `asset_transactions` (id, transaction_id, portfolio_id, stock_card_id, asset_type, transaction_type, quantity, price_per_unit, total_amount, transaction_time, currency)
- `asset_ratings` (id, stock_card_id, period_start, period_end, context, portfolio_id, buy_count, sell_count, buy_amount, sell_amount, rank)

**AuthService (PostgreSQL):**
- `users` (через Identity)
- `roles` (через Identity)
- `role_permissions` (кастомная)
- `permissions` (кастомная)

**Key Optimizations:**
- Индексы на внешние ключи (portfolio_id, user_id, stock_card_id)
- Индексы на даты (transaction_date, period_start, period_end)
- Индексы для поиска (ticker, name)

### 5. Заполнить NFR (Non-Functional Requirements)

**Производительность:**
- API response time: p95 < 200ms, p99 < 500ms
- WebSocket latency: < 100ms
- Database query time: < 50ms (p95)
- Поддержка 1000+ concurrent users

**Масштабируемость:**
- Горизонтальное масштабирование микросервисов
- Автоматическое масштабирование в Kubernetes
- Кэширование через Redis
- Асинхронная обработка через Kafka

**Надежность:**
- Availability: 99.9% (SLA)
- Retry механизмы для внешних API
- Circuit Breaker для защиты от каскадных сбоев
- Graceful degradation

**Безопасность:**
- TLS 1.3 для всех соединений
- JWT токены с коротким временем жизни (1 час)
- Refresh token rotation
- Rate limiting (100 req/min на пользователя)
- Валидация входных данных
- SQL injection protection (EF Core)

**Поддерживаемость:**
- Clean Architecture для изоляции слоев
- Unit test coverage: 80%+
- Integration tests для критических путей
- Структурированное логирование (OpenSearch)
- Мониторинг и алертинг (OpenTelemetry + OpenSearch)

---

## 🎯 ПРОМПТ ДЛЯ ГЕНЕРАЦИИ ПОЛНОЙ ПРЕЗЕНТАЦИИ

```
Создай полную презентацию для проекта "Stock Market Assistant" на основе следующего плана.

# Stock Market Assistant
## Платформа для анализа биржевых котировок с многопользовательским доступом

---

## СЛАЙД 1: ТИТУЛЬНЫЙ СЛАЙД
- **Название:** Stock Market Assistant
- **Подзаголовок:** Платформа для анализа биржевых котировок с многопользовательским доступом
- **Авторы:**
  - Дубровский Никита Владимирович
  - Заворотный Александр Александрович
  - Мельников Игорь Евгеньевич
  - Шадрин Максим Александрович
  - Павлов Константин Петрович
- **Дата:** [Дата презентации]

---

## СЛАЙД 2: ПРОБЛЕМА И РЕШЕНИЕ
**Проблема:**
- Инвесторам нужна единая платформа для мониторинга котировок в реальном времени
- Существующие решения: сложные, дорогие или ограниченные в функционале
- Нет удобного многопользовательского доступа с кастомизацией

**Решение:**
- Веб-платформа с real-time котировками через WebSocket
- Простой интерфейс для портфелей и оповещений
- Многопользовательский доступ с правами доступа (RBAC)
- Аналитика транзакций и рейтинги активов

**Целевая аудитория:**
- Розничные инвесторы
- Профессиональные трейдеры
- Финансовые аналитики
- Инвестиционные команды

---

## СЛАЙД 3: ОБЗОР СИСТЕМЫ
**Основные возможности:**
- Real-time мониторинг котировок акций, облигаций, криптовалют
- Управление инвестиционными портфелями
- Трекинг транзакций (покупка/продажа)
- Система оповещений (Email) при достижении целевых цен
- Аналитика и рейтинги активов
- Многопользовательский доступ с разделением прав

**Типы активов:**
- Акции (Shares)
- Облигации (Bonds)
- Криптовалюты (Crypto)

---

## СЛАЙД 4: ДОМЕННАЯ МОДЕЛЬ
**Основные сущности:**

**PortfolioService:**
- **Portfolio** (Портфель) - коллекция активов пользователя
- **PortfolioAsset** (Актив портфеля) - связь портфеля с активом
- **PortfolioAssetTransaction** (Транзакция) - покупка/продажа актива

**StockCardService:**
- **ShareCard** (Акция) - карточка акции с тикером, ценой, дивидендами
- **BondCard** (Облигация) - карточка облигации с купонами, рейтингом
- **CryptoCard** (Криптовалюта) - карточка криптовалюты
- Multiplier, Dividend, Coupon (связанные данные)

**AnalyticsService:**
- **AssetTransaction** (Транзакция для аналитики) - копия транзакции для анализа
- **AssetRating** (Рейтинг актива) - агрегированные метрики по периоду
- Period (Период анализа)

**AuthService:**
- **User** (Пользователь) - учетная запись
- **Role** (Роль) - ADMIN, USER
- **Permission** (Разрешение) - гранулярные права доступа

**Ключевые связи:**
- User → Portfolio (1:N) - пользователь может иметь несколько портфелей
- Portfolio → PortfolioAsset (1:N) - портфель содержит множество активов
- PortfolioAsset → PortfolioAssetTransaction (1:N) - актив имеет историю транзакций
- PortfolioAsset → StockCard (N:1) - актив ссылается на карточку через StockCardId
- PortfolioAssetTransaction → AssetTransaction (1:1 через Kafka) - транзакция реплицируется в аналитику

---

## СЛАЙД 5: ОСНОВНЫЕ БИЗНЕС-ПРОЦЕССЫ

1. **Мониторинг котировок в реальном времени**
   - Пользователь открывает платформу
   - WebSocket подключение (SignalR PriceHub) к источнику котировок
   - Подписка на тикеры через `Subscribe(tickers)`
   - PriceStreamingService опрашивает MOEX API каждые 1.5 секунды
   - Обновления цен через SignalR `PriceUpdate` событие
   - Визуализация изменений в интерфейсе (цена, изменение, процент изменения)

2. **Управление портфелем**
   - Создание портфелей
   - Добавление/удаление активов
   - Регистрация транзакций (покупка/продажа)
   - Расчет стоимости и PnL (прибыль/убыток)
   - История транзакций

3. **Система оповещений**
   - Пользователь устанавливает целевую цену
   - Система мониторит в реальном времени
   - При достижении цены → Email уведомление через NotificationService
   - Пользователь может управлять оповещениями

4. **Аналитика и рейтинги**
   - Транзакции реплицируются в AnalyticsService через Kafka (Outbox Pattern)
   - Batch processing транзакций (100 сообщений за раз)
   - Расчет рейтингов активов (Global и Portfolio контексты)
   - Агрегация по периодам (день, неделя, месяц, произвольный)
   - Ранжирование активов по популярности
   - Получение транзакций с фильтрацией (Today, Week, Month, Custom)
   - Топ активов по покупкам/продажам за период

5. **Многопользовательский доступ**
   - RBAC (Role-Based Access Control): ADMIN, USER
   - Каждая роль имеет разные права
   - Логирование действий для аудита
   - Приватные портфели (IsPrivate флаг)

---

## СЛАЙД 6: USER STORIES (ФАЗЫ РАЗРАБОТКИ)

**Phase 1 (MVP — неделя 1-3):**
- Real-time quote display
- Portfolio CRUD (Create, Read, Update, Delete)
- Basic transaction tracking
- Authentication и авторизация

**Phase 2 (WebSocket & Alerts — неделя 4-6):**
- SignalR для real-time quotes
- Email оповещения через NotificationService
- Advanced alert management
- 100 concurrent users

**Phase 3 (Analytics — неделя 7-9):**
- AnalyticsService: рейтинги активов
- Агрегация данных по периодам
- Топ активов по покупкам/продажам
- 1000 concurrent users

**Phase 4 (Production hardening — неделя 10-11):**
- Security audit & penetration testing
- Monitoring & observability (OpenTelemetry, OpenSearch)
- Complete documentation
- User acceptance testing

---

## СЛАЙД 7: ТЕХНОЛОГИЧЕСКИЙ СТЕК

**Frontend:**
- React 18 + TypeScript
- Redux Toolkit / Zustand (state management)
- Tailwind CSS + Material-UI
- Chart.js / Recharts (графики)
- SignalR client (WebSocket)
- Vite (build tool)

**Backend:**
- .NET 8+ LTS
- ASP.NET Core Web API
- C# 12
- Entity Framework Core (ORM)
- SignalR (WebSocket server)
- JWT (authentication)
- Autofac (DI container для PortfolioService)
- NSwag (Swagger/OpenAPI генерация)
- Serilog (структурированное логирование)

**Database:**
- PostgreSQL 17+ (OLTP для всех сервисов)
- Redis 7+ (cache)
- MongoDB 8+ (финансовые отчеты в StockCardService)

**Message Queue & Events:**
- Apache Kafka (event-driven communication)
- Confluent Kafka .NET client
- Kafka UI (веб-интерфейс для управления топиками)

**Infrastructure:**
- .NET Aspire (оркестрация микросервисов)
- Docker + Docker Compose (локальная разработка)
- Kubernetes (production)
- GitHub Actions (CI/CD)
- AWS / Azure / Yandex.Cloud
- OpenTelemetry (мониторинг и трейсинг)
- OpenSearch (логирование и визуализация)
- OpenSearch Dashboards (дашборды)

**External APIs:**
- MOEX (Московская биржа) - котировки акций и облигаций
  - `https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities.json` (акции)
  - `https://iss.moex.com/iss/securities.json` (облигации ОФЗ и корпоративные)
  - `https://iss.moex.com/iss/engines/stock/markets/{market}/boards/{board}/securities/{ticker}.json` (текущая цена)
- SendGrid (email)

---

## СЛАЙД 8: МИКРОСЕРВИСНАЯ АРХИТЕКТУРА

**6 микросервисов:**

1. **Gateway Service**
- API Gateway для маршрутизации запросов (базовая реализация)
- Единая точка входа для клиентов
- CORS настройки

2. **AuthService**
- Аутентификация (JWT токены)
- Авторизация (RBAC)
- Управление пользователями и ролями
- Refresh token rotation

3. **StockCardService**
- Управление карточками активов (акции, облигации, криптовалюты)
- Интеграция с MOEX для получения котировок
- Real-time котировки через SignalR (PriceHub, PriceStreamingService)
- Кэширование данных в Redis
- Хранение финансовых отчетов в MongoDB
- Публикация событий создания финансовых отчетов в Kafka

4. **PortfolioService**
- Управление портфелями пользователей
- Управление активами в портфелях
- Регистрация транзакций
- Публикация событий транзакций в Kafka

5. **AnalyticsService**
- Потребление событий транзакций из Kafka (batch processing)
- Расчет рейтингов активов (Global и Portfolio контексты)
- Агрегация данных по периодам
- API для получения аналитики
- Тестовое заполнение данных (TestDataController, TestDataGenerator)
- Получение транзакций с фильтрацией по периоду и типу

6. **NotificationService**
- Потребление событий из Kafka
- Отправка Email уведомлений (SendGrid)
- Интеграция с OpenSearch для логирования

**Архитектурный стиль:**
- Clean Architecture (Domain, Application, Infrastructure, Presentation)
- Event-Driven Architecture (Kafka для асинхронной коммуникации)
- RESTful API для синхронной коммуникации
- Database per Service (каждый сервис имеет свою БД)

---

## СЛАЙД 9: АРХИТЕКТУРА СИСТЕМЫ (LAYERS)

**Clean Architecture Layers:**

**1. Presentation Layer (WebApi)**
- ASP.NET Core Controllers
- DTOs (Data Transfer Objects)
- Middleware (error handling, validation)
- SignalR Hubs (WebSocket)

**2. Application Layer**
- Use Cases (бизнес-логика оркестрации)
- Application Services
- DTOs и маппинг
- Валидаторы (FluentValidation)

**3. Domain Layer**
- Entities (доменные сущности)
- Value Objects
- Domain Services (бизнес-логика)
- Domain Events
- Enums и константы

**4. Infrastructure Layer**
- Entity Framework Core (data access)
- Repositories (реализация)
- Kafka Consumers/Producers
- HTTP Clients (для межсервисной коммуникации)
- External API integrations (MOEX, SendGrid)
- Caching (Redis)
- Background Services (PriceStreamingService, TransactionConsumer)
- Outbox Pattern (для гарантированной доставки событий в Kafka)

**Data Flow:**
- Browser → Gateway → Backend Services
- Services → PostgreSQL / Redis / MongoDB
- PortfolioService → Kafka → AnalyticsService / NotificationService
- StockCardService → Kafka (financial.report.created) → другие сервисы
- Background jobs → Email notifications
- SignalR → Real-time quote broadcasting (PriceStreamingService)
- PriceStreamingService → MOEX API → SignalR Hub → Frontend

---

## СЛАЙД 10: DATABASE SCHEMA (ER-модель)

**Основные таблицы по сервисам:**

**PortfolioService (PostgreSQL):**
- `portfolio` (id, user_id, name, currency)
- `portfolio_asset` (id, portfolio_id, stock_card_id, asset_type)
- `portfolio_asset_transaction` (id, portfolio_asset_id, transaction_type, quantity, price_per_unit, total_amount, transaction_date, currency)

**StockCardService (PostgreSQL + MongoDB):**
- `ShareCards` (id, ticker, name, currency, current_price)
- `BondCards` (id, ticker, name, maturity_period, rating, face_value)
- `CryptoCards` (id, ticker, name)
- `Multipliers`, `Dividends`, `Coupons` (связанные таблицы)
- MongoDB: `FinancialReports` (коллекция)

**AnalyticsService (PostgreSQL):**
- `asset_transactions` (id, transaction_id, portfolio_id, stock_card_id, asset_type, transaction_type, quantity, price_per_unit, total_amount, transaction_time, currency)
- `asset_ratings` (id, stock_card_id, period_start, period_end, context, portfolio_id, buy_count, sell_count, buy_amount, sell_amount, rank)

**AuthService (PostgreSQL):**
- `users` (через ASP.NET Identity)
- `roles` (через ASP.NET Identity)
- `role_permissions` (кастомная)
- `permissions` (кастомная)
- `refresh_sessions` (для refresh tokens)

**Key Optimizations:**
- Индексы на внешние ключи (portfolio_id, user_id, stock_card_id)
- Индексы на даты (transaction_date, period_start, period_end) для быстрого поиска
- Индексы для поиска (ticker, name)
- Кэширование часто запрашиваемых данных в Redis

---

## СЛАЙД 11: NON-FUNCTIONAL REQUIREMENTS (NFR)

**Производительность:**
- API response time: p95 < 200ms, p99 < 500ms
- WebSocket latency: < 100ms
- Database query time: < 50ms (p95)
- Поддержка 1000+ concurrent users
- Throughput: 1000+ requests/second

**Масштабируемость:**
- Горизонтальное масштабирование микросервисов
- Автоматическое масштабирование в Kubernetes (HPA)
- Кэширование через Redis для снижения нагрузки на БД
- Асинхронная обработка через Kafka (decoupling)
- Database per Service для независимого масштабирования

**Надежность:**
- Availability: 99.9% (SLA)
- Retry механизмы для внешних API (Polly)
- Circuit Breaker для защиты от каскадных сбоев
- Graceful degradation (fallback на кэш при недоступности БД)
- Health checks для всех сервисов

**Безопасность:**
- TLS 1.3 для всех соединений
- JWT токены с коротким временем жизни (1 час)
- Refresh token rotation
- Rate limiting (100 req/min на пользователя)
- Валидация входных данных (FluentValidation)
- SQL injection protection (EF Core parameterized queries)
- CORS restricted to registered domains
- Secrets management (environment variables, Azure Key Vault)

**Поддерживаемость:**
- Clean Architecture для изоляции слоев
- Unit test coverage: 80%+
- Integration tests для критических путей (TestContainers)
- Структурированное логирование (Serilog + OpenSearch)
- Мониторинг и алертинг (OpenTelemetry + OpenSearch Dashboards)
- API documentation (Swagger/OpenAPI через NSwag)
- Автоматические миграции БД при старте приложения
- TestDataGenerator для генерации тестовых данных

---

## СЛАЙД 12: SECURITY ARCHITECTURE

**Authentication:**
- JWT tokens с коротким временем жизни (15 минут для access token)
- Refresh token rotation (автоматическая смена, 30 дней)
- Password policy: min 8 chars, complexity requirements
- ASP.NET Identity для управления пользователями

**Authorization:**
- RBAC: ADMIN, USER roles
- Row-level security для user data (фильтрация по user_id)
- Policy-based authorization в ASP.NET Core
- Приватные портфели (IsPrivate флаг)

**Data Protection:**
- TLS 1.3 for transit (HTTPS)
- AES-256 for data at rest (если требуется шифрование)
- Password hashing (bcrypt через ASP.NET Identity)
- Secrets management (environment variables, Azure Key Vault)

**API Security:**
- Rate limiting per user (100 req/min)
- CORS restricted to registered domains
- API key validation для внешних интеграций
- Request signing для sensitive operations
- Input validation (FluentValidation)
- SQL injection protection (EF Core)

---

## СЛАЙД 13: TESTING STRATEGY

**Unit Tests (80% coverage):**
- xUnit / NUnit (test framework)
- Moq / NSubstitute (mocking)
- FluentAssertions (assertions)
- Domain services, application services, utilities
- Component logic tests

**Integration Tests:**
- API endpoints с database (TestContainers для PostgreSQL)
- External API mocking (MOEX, SendGrid)
- Kafka integration tests (Testcontainers)
- HTTP client testing (WebApplicationFactory)

**Performance Tests:**
- Load testing (k6, NBomber)
- Stress testing для определения пределов
- Database performance tests

**E2E Tests:**
- Playwright / Cypress для frontend
- API E2E tests через Gateway

---

## СЛАЙД 14: DEPLOYMENT & DEVOPS

**CI/CD Pipeline (GitHub Actions):**
1. Code push to Git
2. Run linting & formatting checks (dotnet format)
3. Run unit tests (xUnit)
4. Run integration tests с test DB (TestContainers)
5. Build Docker image
6. Push to container registry (Docker Hub / Azure Container Registry)
7. Deploy to staging
8. Run E2E tests
9. Manual approval
10. Deploy to production (blue-green deployment)

**Infrastructure:**
- Docker containers для всех сервисов
- .NET Aspire для локальной разработки и оркестрации
- Kubernetes для orchestration (production)
- Database: Managed PostgreSQL (AWS RDS / Azure Database)
- Cache: Redis cluster (managed service)
- Load balancer: ALB (AWS) / Azure LB
- CDN: Cloudflare / AWS CloudFront
- Monitoring: OpenTelemetry (метрики и трейсинг)
- Logging: OpenSearch (централизованное логирование)
- Dashboards: OpenSearch Dashboards (визуализация)
- Kafka UI (веб-интерфейс для управления топиками, порт 9100)
- Mongo Express (веб-интерфейс для MongoDB, порт 5005)
- PgWeb (веб-интерфейс для PostgreSQL, порты 5000, 5001)

**Deployment Strategy:**
- Blue-Green deployment для zero-downtime
- Canary releases для постепенного rollout
- Database migrations через EF Core Migrations (автоматические при старте приложения)
- Health checks для всех сервисов (`/health`, `/alive`)

---

## СЛАЙД 15: MONITORING & OBSERVABILITY

**Metrics & Tracing (OpenTelemetry):**
- API response time (p50, p95, p99)
- Database query time
- Cache hit ratio (Redis)
- WebSocket connections (SignalR)
- Kafka message processing rate
- Error rates по сервисам
- Distributed tracing между сервисами

**Dashboards (OpenSearch Dashboards):**
- System health (CPU, memory, disk)
- API performance (latency, throughput)
- Database metrics (connections, slow queries)
- Business metrics (active users, transactions, portfolios)
- Error rates и logs
- Kafka topics monitoring

**Logging (OpenSearch):**
- Structured JSON logs
- Log levels: ERROR, WARN, INFO, DEBUG
- Centralized search и analysis
- Correlation IDs для трейсинга запросов
- Интеграция с OpenTelemetry для контекста

**Observability:**
- OpenTelemetry для метрик, трейсинга и логов
- OpenSearch для хранения и анализа логов
- OpenSearch Dashboards для визуализации

---

## СЛАЙД 16: EVENT-DRIVEN COMMUNICATION

**Kafka Topics:**

**1. portfolio.transactions**
- Producer: PortfolioService
- Consumers: AnalyticsService, NotificationService
- Consumer Group: `analytics-service-transactions`
- Payload: Transaction event (transactionId, portfolioId, portfolioAssetId, stockCardId, assetType, transactionType, quantity, pricePerUnit, totalAmount, transactionTime, currency)
- Batch processing: 100 сообщений за раз
- Manual offset commit после успешной обработки

**2. financial.report.created**
- Producer: StockCardService
- Payload: FinancialReportCreatedMessage (событие создания финансового отчета)
- Хранение отчетов в MongoDB

**Схема взаимодействия:**
1. Пользователь создает транзакцию в PortfolioService
2. PortfolioService сохраняет транзакцию в БД
3. PortfolioService публикует событие в Outbox таблицу (Outbox Pattern)
4. Background Service обрабатывает Outbox и публикует в Kafka topic `portfolio.transactions`
5. AnalyticsService потребляет событие (batch processing) и обновляет рейтинги
6. NotificationService потребляет событие и проверяет триггеры оповещений

**Преимущества:**
- Асинхронная обработка (decoupling)
- Масштабируемость (несколько consumers)
- Надежность (persistent messages, Outbox Pattern)
- Возможность replay событий
- Batch processing для оптимизации производительности

---

## СЛАЙД 17: API ENDPOINTS

**PortfolioService:**
- `GET /api/v1/portfolios` - список портфелей
- `POST /api/v1/portfolios` - создание портфеля
- `GET /api/v1/portfolios/{id}` - детали портфеля
- `PUT /api/v1/portfolios/{id}` - обновление портфеля
- `DELETE /api/v1/portfolios/{id}` - удаление портфеля
- `GET /api/v1/portfolio-assets` - список активов портфеля
- `POST /api/v1/portfolio-assets` - добавление актива
- `GET /api/v1/portfolio-assets/{id}/transactions` - транзакции актива
- `POST /api/v1/portfolio-assets/{id}/transactions` - создание транзакции

**StockCardService:**
- `GET /api/stockcard/shares` - список акций
- `GET /api/stockcard/bonds` - список облигаций
- `GET /api/stockcard/crypto` - список криптовалют
- `GET /api/stockcard/{ticker}` - детали актива
- `GET /api/stockcard/{ticker}/price` - текущая цена
- SignalR Hub: `/priceHub` - подписка на real-time котировки

**AnalyticsService:**
- `GET /api/analytics/transactions` - все транзакции с фильтрацией
- `GET /api/analytics/assets/top-bought` - топ активов по покупкам
- `GET /api/analytics/assets/top-sold` - топ активов по продажам
- `GET /api/analytics/portfolios/{id}/history` - история портфеля
- `POST /api/analytics/portfolios/compare` - сравнение портфелей


**AuthService:**
- `POST /api/auth/register` - регистрация
- `POST /api/auth/login` - вход
- `POST /api/auth/refresh` - обновление токена
- `POST /api/auth/logout` - выход

**NotificationService:**
- `POST /api/notifications/send` - отправка уведомления
- Потребление событий из Kafka для автоматических уведомлений

---

## СЛАЙД 18: ВЫВОДЫ И ПЕРСПЕКТИВЫ

**Технические результаты:**
✓ Масштабируемая микросервисная архитектура с Clean Architecture
✓ Real-time infrastructure с SignalR WebSocket
✓ Event-Driven Architecture с Kafka
✓ Оптимизированные databases с индексами
✓ Multi-layer security (JWT, encryption)
✓ Comprehensive monitoring и observability

**Бизнес-результаты:**
✓ Единая платформа для мониторинга котировок
✓ Удобное управление портфелями
✓ Автоматические оповещения
✓ Аналитика и рейтинги активов
✓ Многопользовательский доступ с RBAC

**Смотрим в будущее:**
1. Масштабирование в Kubernetes для production
2. Использование сервисов Искусственного Интеллекта для аналитики и прогнозирования
3. Расширение типов активов (фьючерсы, опционы)
4. Мобильное приложение (React Native)
5. Расширенная аналитика с ML моделями

---

## ТРЕБОВАНИЯ К ПРЕЗЕНТАЦИИ

**Формат:**
- PowerPoint (.pptx) на основе шаблона `StockMarketAssistant_Presentation_template.pptx`
- 18 слайдов
- Профессиональный корпоративный дизайн

**Дизайн (на основе шаблона):**

**Цветовая схема:**
- Основной фон: темный (черный или темно-серый)
- Акцентный цвет: #38bdf8 (cyan/голубой) для заголовков, иконок, акцентов
- Дополнительные цвета:
  - Белый/светло-серый для основного текста
  - Зеленый для положительных метрик/успешных операций
  - Красный для предупреждений/ошибок
  - Желтый/оранжевый для важных уведомлений

**Типографика:**
- Заголовки слайдов: крупный шрифт (32-44pt), жирный, белый или акцентный цвет
- Подзаголовки: средний шрифт (24-28pt), полужирный
- Основной текст: 16-20pt, обычный, белый/светло-серый
- Код/технические термины: моноширинный шрифт (Consolas, Courier New), 14-16pt
- Шрифт по умолчанию: Calibri, Arial или аналогичный sans-serif

**Композиция слайдов:**
- Максимум 4-5 bullet points на слайд
- Отступы: достаточные поля (1-2 см от краев)
- Выравнивание: по левому краю для текста, по центру для заголовков
- Интерлиньяж: 1.2-1.5 для читаемости

**Элементы дизайна:**
- Логотип/брендинг в углу слайда (если есть в шаблоне)
- Номера слайдов в нижнем правом углу
- Разделительные линии между секциями (акцентный цвет)
- Иконки для визуализации концепций (микросервисы, базы данных, API)
- Градиенты или тени для глубины (если используются в шаблоне)

**Диаграммы и визуализация:**
- Диаграммы архитектуры: использовать draw.io, Lucidchart или аналоги
- Цветовое кодирование компонентов:
  - Сервисы: акцентный цвет (#38bdf8) или вариации
  - Базы данных: темно-синий или фиолетовый
  - Очереди сообщений: оранжевый или желтый
  - Внешние API: серый
- ER-диаграммы: светлые линии на темном фоне
- Sequence diagrams: четкие стрелки, подписи
- Infrastructure diagrams: иконки Docker, Kubernetes, облачных провайдеров
- Таблицы: чередующиеся строки (темный/светлее), заголовки с акцентным цветом
- Timeline: горизонтальная или вертикальная линия с точками-событиями

**Специальные слайды:**
- Титульный слайд: крупный заголовок, подзаголовок, список авторов, дата
- Слайды с кодом: темный фон, подсветка синтаксиса, моноширинный шрифт
- Слайды с диаграммами: минимум текста, максимум визуализации
- Заключительный слайд: краткое резюме, контакты (если нужно)

**Анимации и переходы:**
- Минимальные анимации (если используются в шаблоне)
- Плавные переходы между слайдами
- Появление элементов по клику для сложных диаграмм

**Язык:**
- Все тексты на русском языке
- Международно признанные термины на английском (API, RBAC, JWT, Kafka, etc.)
- Технические названия сервисов на английском (PortfolioService, AnalyticsService)

**Визуализация:**
- Диаграммы архитектуры микросервисов (Mermaid или draw.io)
- ER-диаграммы для основных сущностей
- Sequence diagrams для бизнес-процессов
- Infrastructure diagrams (Kubernetes, Docker)
- Схемы потоков данных (Kafka topics, event flow)
- Графики и метрики (если применимо)

**Структура слайдов:**
- Заголовок слайда (верх, акцентный цвет)
- Основной контент (центр, читаемый размер шрифта)
- Футер (низ, логотип/номер слайда, опционально)

---

## ИНСТРУКЦИИ ПО ИСПОЛЬЗОВАНИЮ ШАБЛОНА

**Шаблон:** `Presentation/StockMarketAssistant_Presentation_template.pptx`

**Рекомендации по работе с шаблоном:**

1. **Открыть шаблон в PowerPoint**
   - Использовать существующие макеты слайдов из шаблона
   - Сохранить цветовую схему и стили из шаблона
   - Применить шрифты и размеры из шаблона

2. **Создание новых слайдов**
   - Использовать макеты из шаблона (Title Slide, Content Slide, etc.)
   - Копировать стили заголовков и текста из существующих слайдов
   - Сохранять единообразие дизайна

3. **Вставка диаграмм**
   - Экспортировать диаграммы из draw.io/Mermaid в PNG/SVG
   - Использовать прозрачный фон для диаграмм
   - Масштабировать с сохранением пропорций
   - Добавлять подписи к диаграммам в стиле шаблона

4. **Таблицы**
   - Использовать стили таблиц из шаблона
   - Чередующиеся цвета строк для читаемости
   - Заголовки таблиц с акцентным цветом

5. **Код и технические термины**
   - Использовать моноширинный шрифт для кода
   - Темный фон для блоков кода
   - Подсветка синтаксиса (если возможно)

6. **Проверка перед презентацией**
   - Единообразие шрифтов и размеров
   - Корректность цветовой схемы
   - Читаемость текста на темном фоне
   - Правильность нумерации слайдов
   - Отсутствие опечаток

---

**Документ готов к использованию как промпт для генерации полной презентации.**
```
