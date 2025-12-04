# Анализ проблем при запуске Docker контейнеров

## Дата: 2025-12-04
## Обновлено: 2025-12-04 20:08

## Обзор проблем

При запуске Docker Compose обнаружено несколько проблем, которые не критичны для работы системы, но требуют внимания:

1. **NotificationService**: Ошибка подписки на несуществующий Kafka топик ✅ **ИСПРАВЛЕНО**
2. **OpenSearch Dashboards**: Ошибка подключения к OpenSearch ✅ **ИСПРАВЛЕНО**
3. **StockCardService**: Критическая ошибка - несоответствие имен connection strings ✅ **ИСПРАВЛЕНО**
4. **Kafka Coordinator**: Нормальное поведение при старте (не проблема)

## Детальный анализ

### 1. NotificationService - Ошибка подписки на Kafka топик

#### Проблема

```
[15:53:40 ERR] Error consuming message from Kafka
Confluent.Kafka.ConsumeException: Subscribed topic not available: notifications_send: Broker: Unknown topic or partition
```

**Локация:** `NotificationService/Notification.Infrastructure/Messaging/KafkaConsumer.cs:33`

**Причина:**
- Consumer пытается подписаться на топик `notifications_send` при старте
- Топик еще не создан в Kafka
- Kafka не создает топики автоматически при подписке (только при записи, если включен `auto.create.topics.enable`)

**Текущая реализация:**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("Starting Kafka notification worker...");
    _consumer.Subscribe(_topic);  // Строка 33 - подписка на несуществующий топик

    try
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(stoppingToken);
            // ...
        }
    }
}
```

#### Решение

**Вариант 1: Создание топика при старте (рекомендуется)**

Добавить создание топика перед подпиской:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("Starting Kafka notification worker...");

    // Создание топика, если не существует
    await EnsureTopicExistsAsync(_topic, stoppingToken);

    _consumer.Subscribe(_topic);

    // ... остальной код
}

private async Task EnsureTopicExistsAsync(string topicName, CancellationToken cancellationToken)
{
    try
    {
        using var adminClient = _consumer.GetAdminClient();
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

        var topicExists = metadata.Topics.Any(t => t.Topic == topicName);

        if (!topicExists)
        {
            _logger.LogInformation("Creating Kafka topic: {Topic}", topicName);
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            }, cancellationToken: cancellationToken);
            _logger.LogInformation("Kafka topic created: {Topic}", topicName);
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Could not ensure topic exists, will retry: {Topic}", topicName);
        // Продолжаем - Kafka может создать топик автоматически при первой записи
    }
}
```

**Плюсы:**
- ✅ Гарантирует существование топика
- ✅ Явное создание с нужными параметрами
- ✅ Логирование процесса

**Минусы:**
- ⚠️ Требует AdminClient (дополнительная зависимость)
- ⚠️ Нужны права на создание топиков

**Вариант 2: Обработка ошибки с повторными попытками**

Улучшить обработку ошибки подписки:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("Starting Kafka notification worker...");

    int retryCount = 0;
    const int maxRetries = 10;
    const int retryDelayMs = 5000;

    while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
    {
        try
        {
            _consumer.Subscribe(_topic);
            _logger.LogInformation("Successfully subscribed to topic: {Topic}", _topic);
            break; // Успешная подписка
        }
        catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
        {
            retryCount++;
            _logger.LogWarning(
                "Topic {Topic} not found, retry {Retry}/{MaxRetries} in {Delay}ms",
                _topic, retryCount, maxRetries, retryDelayMs);

            if (retryCount >= maxRetries)
            {
                _logger.LogError("Failed to subscribe to topic {Topic} after {MaxRetries} retries",
                    _topic, maxRetries);
                throw;
            }

            await Task.Delay(retryDelayMs, stoppingToken);
        }
    }

    // ... остальной код обработки сообщений
}
```

**Плюсы:**
- ✅ Не требует AdminClient
- ✅ Автоматические повторные попытки
- ✅ Работает, если топик создается автоматически при первой записи

**Минусы:**
- ⚠️ Зависит от настройки `auto.create.topics.enable` в Kafka
- ⚠️ Может занять время до успешной подписки

**Вариант 3: Создание топика через docker-compose или init скрипт**

Создать топик при инициализации Kafka:

```yaml
# docker-compose.yml
kafka-init:
  image: confluentinc/cp-kafka:latest
  depends_on:
    - kafka
  command: >
    sh -c "
    echo 'Waiting for Kafka...' &&
    sleep 30 &&
    kafka-topics --create --if-not-exists
      --bootstrap-server kafka:9092
      --topic notifications_send
      --partitions 1
      --replication-factor 1
    "
  networks:
    - aspire-net
```

**Плюсы:**
- ✅ Топик создается до запуска сервисов
- ✅ Централизованное управление топиками

**Минусы:**
- ⚠️ Требует дополнительный контейнер или init скрипт
- ⚠️ Нужно синхронизировать с кодом

#### Рекомендация

**Комбинированный подход:**
1. Использовать Вариант 2 (обработка с повторными попытками) как основное решение
2. Добавить Вариант 3 (создание через docker-compose) для production окружения
3. Это обеспечит надежность и гибкость

### 2. OpenSearch Dashboards - Ошибка подключения к OpenSearch ✅ ИСПРАВЛЕНО

#### Проблема

```
{"type":"log","@timestamp":"2025-12-04T15:54:12Z","tags":["error","opensearch","data"],"pid":1,"message":"[ConnectionError]: connect ECONNREFUSED 172.18.0.3:9200"}
{"type":"log","@timestamp":"2025-12-04T15:54:12Z","tags":["error","savedobjects-service"],"pid":1,"message":"Unable to retrieve version information from OpenSearch nodes."}
```

**Причина:**
- OpenSearch Dashboards пытается подключиться к OpenSearch при старте
- OpenSearch еще не полностью готов к работе (загружается)
- `depends_on` в docker-compose только ждет запуска контейнера, но не готовности сервиса
- Отсутствует health check для OpenSearch

#### Решение (применено)

**Файл:** `docker-compose.yml`

**Изменения:**
1. Добавлен health check для OpenSearch:
```yaml
opensearch:
  # ... существующая конфигурация
  healthcheck:
    test: ["CMD-SHELL", "curl -f http://localhost:9200/_cluster/health || exit 1"]
    interval: 10s
    timeout: 5s
    retries: 10
    start_period: 30s
```

2. Обновлен `depends_on` для OpenSearch Dashboards:
```yaml
opensearch-dashboards:
  # ... существующая конфигурация
  depends_on:
    opensearch:
      condition: service_healthy
```

**Результат:**
- ✅ OpenSearch Dashboards теперь ждет полной готовности OpenSearch перед запуском
- ✅ Устранена ошибка `ECONNREFUSED` при подключении
- ✅ Health check проверяет доступность API OpenSearch через `/cluster/health`

### 3. StockCardService - Сервис недоступен для Gateway ✅ ИСПРАВЛЕНО

#### Проблема

```
warn: Yarp.ReverseProxy.Health.ActiveHealthCheckMonitor[17]
      Probing destination `destination1` on cluster `stockcard-cluster` failed.
      System.Net.Http.HttpRequestException: Name or service not known (stockcardservice-api:8080)
      System.Net.Sockets.SocketException (0xFFFDFFFF): Name or service not known
```

**Причина:**
- Gateway пытается проверить health check для `stockcardservice-api:8080`
- Сервис либо не запущен, либо не может быть найден по DNS имени в Docker сети
- Сервис определен в `docker-compose.yml`, но может не запускаться из-за ошибок

#### Проверка конфигурации

**docker-compose.yml:**
```yaml
stockcardservice-api:
  build:
    context: .
    dockerfile: src/backend/services/StockCardService/StockCardService.WebApi/Dockerfile
  ports:
    - "8082:8080"
  environment:
    - ASPNETCORE_HTTP_PORTS=8080
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__pg-stock-card-db=Host=pg-stock-card-db;Port=5432;Database=stock-card-db;Username=postgres;Password=postgres
    - ConnectionStrings__mongo=mongodb://mongo-stock-card-db:27017/finantial-report-db
    - FRONTEND_ORIGIN=http://localhost:5273
  depends_on:
    - pg-stock-card-db
    - mongo-stock-card-db
    - cache
  networks:
    - aspire-net
```

**Gateway конфигурация:**
- Ищет сервис по имени `stockcardservice-api:8080`
- Сервис должен быть в той же Docker сети `aspire-net`

#### Возможные причины

1. **Сервис не запускается** - ошибки при старте (миграции, подключение к БД)
2. **Сервис не в той же сети** - неправильная конфигурация сети
3. **Сервис запускается медленно** - health check происходит до готовности сервиса
4. **Порт не совпадает** - сервис слушает другой порт

#### Решение

**Вариант 1: Проверить логи StockCardService**

```bash
docker logs stock_market_assistant-stockcardservice-api-1
```

**Вариант 2: Добавить health check в docker-compose**

```yaml
stockcardservice-api:
  # ... существующая конфигурация
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 10s
    timeout: 5s
    retries: 5
    start_period: 30s
```

**Вариант 3: Улучшить depends_on с условиями**

```yaml
stockcardservice-api:
  # ... существующая конфигурация
  depends_on:
    pg-stock-card-db:
      condition: service_healthy
    mongo-stock-card-db:
      condition: service_started
    cache:
      condition: service_started
```

**Вариант 4: Настроить Gateway для обработки недоступных сервисов**

Настроить Gateway, чтобы он не падал при недоступности StockCardService (если сервис опционален).

#### Решение (применено)

**Файл:** `docker-compose.yml`

**Изменения:**
1. Добавлен health check для StockCardService:
```yaml
stockcardservice-api:
  # ... существующая конфигурация
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 10s
    timeout: 5s
    retries: 10
    start_period: 30s
```

2. Обновлен `depends_on` для Gateway:
```yaml
gateway-api:
  # ... существующая конфигурация
  depends_on:
    authservice-api:
      condition: service_started
    stockcardservice-api:
      condition: service_healthy
    portfolioservice-api:
      condition: service_started
    analyticsservice-api:
      condition: service_started
```

**Результат:**
- ✅ Gateway теперь ждет готовности StockCardService перед проверкой health check
- ✅ Health check проверяет доступность API через `/health` endpoint
- ✅ Улучшена обработка зависимостей между сервисами

**Примечание:**
- StockCardService использует `AddServiceDefaults()` и `MapDefaultEndpoints()`, поэтому health check endpoint доступен
- Если проблема сохраняется, необходимо проверить логи: `docker logs stock_market_assistant-stockcardservice-api-1`

### 3. Kafka Coordinator - Нормальное поведение

#### Сообщение

```
analyticsservice-api-1 | %4|1764863619.832|GETPID|rdkafka#producer-2| [thrd:main]:
Failed to acquire idempotence PID from broker kafka:9092/1:
Broker: Coordinator load in progress: retrying
```

**Статус:** ✅ Это нормальное поведение

**Объяснение:**
- При старте Kafka координатор загружается
- Producer пытается получить idempotence PID (идентификатор для идемпотентности)
- Координатор еще не готов, поэтому возвращает "Coordinator load in progress"
- Producer автоматически повторяет попытку (retrying)

**Действие:** Никаких действий не требуется. Это временное состояние при инициализации Kafka.

## Приоритет исправления

### Высокий приоритет
1. **NotificationService** - Ошибка подписки на топик (влияет на функциональность)
   - Рекомендуется: Вариант 2 (обработка с повторными попытками)

### Средний приоритет
2. **StockCardService** - Недоступность сервиса ✅ ИСПРАВЛЕНО
   - Применено: Добавлен health check и обновлен depends_on для Gateway

### Низкий приоритет
3. **Kafka Coordinator** - Нормальное поведение, не требует исправления

## Статус

- ✅ Проблемы идентифицированы
- ✅ Решения подготовлены
- ✅ Исправления применены

## Примененные исправления

### 1. NotificationService - Обработка ошибки подписки на Kafka топик ✅

**Файл:** `src/backend/services/NotificationService/Notification.Infrastructure/Messaging/KafkaConsumer.cs`

**Изменения:**
- Добавлена логика повторных попыток подписки на топик (до 10 попыток с интервалом 5 секунд)
- Добавлена обработка ошибки `UnknownTopicOrPart` с переподпиской
- Улучшена обработка ошибок подключения к Kafka (`Local_Resolve`, `Local_AllBrokersDown`)
- Добавлено логирование всех этапов подписки и обработки ошибок

**Результат:**
- ✅ Consumer теперь ждет создания топика вместо немедленного падения
- ✅ Автоматическая переподписка при недоступности топика
- ✅ Улучшенная обработка ошибок подключения к Kafka
- ✅ **Подтверждено в логах:** `[17:02:56 INF] Resubscribed to topic notifications_send` - переподписка работает корректно

### 2. OpenSearch Dashboards - Проблема подключения к OpenSearch ✅

**Файл:** `docker-compose.yml`

**Изменения:**
- Добавлен health check для OpenSearch контейнера
- Обновлен `depends_on` для `opensearch-dashboards` с использованием `condition: service_healthy`

**Результат:**
- OpenSearch Dashboards теперь ждет полной готовности OpenSearch перед запуском
- Устранена ошибка `ECONNREFUSED` при подключении

### 3. StockCardService - Критическая ошибка: несоответствие имен connection strings ✅ ИСПРАВЛЕНО

#### Проблема

```
System.InvalidOperationException: The ConnectionString property has not been initialized.
An error occurred using the connection to database '' on server ''.
dependency failed to start: container stock_market_assistant-stockcardservice-api-1 exited (139)
```

**Причина:**
- Несоответствие имен connection strings между `docker-compose.yml` и `Program.cs`
- В docker-compose: `ConnectionStrings__pg-stock-card-db`, в коде: `GetConnectionString("stock-card-db")`
- В docker-compose: `ConnectionStrings__mongo`, в коде: `GetConnectionString("finantial-report-db")`
- Приложение падало при попытке выполнить миграции БД

#### Решение (применено)

**Файл:** `src/backend/services/StockCardService/StockCardService.WebApi/Program.cs`

**Изменения:**
1. Исправлено имя connection string для PostgreSQL:
   - Было: `GetConnectionString("stock-card-db")`
   - Стало: `GetConnectionString("pg-stock-card-db")`

2. Исправлено имя connection string для MongoDB:
   - Было: `GetConnectionString("finantial-report-db")`
   - Стало: `GetConnectionString("mongo")`

**Результат:**
- ✅ Connection strings теперь соответствуют переменным окружения в docker-compose.yml
- ✅ Приложение должно успешно подключаться к базам данных
- ✅ Миграции должны выполняться корректно

**Дополнительные исправления:**
- ✅ Добавлен health check для StockCardService в docker-compose.yml
- ✅ Обновлен depends_on для Gateway с использованием `condition: service_healthy`

## Файлы для изменения

✅ **ИСПРАВЛЕНИЯ ПРИМЕНЕНЫ:** Все исправления были применены после подтверждения пользователя.

**Измененные файлы:**
- ✅ `src/backend/services/NotificationService/Notification.Infrastructure/Messaging/KafkaConsumer.cs` - исправлено
- ✅ `src/backend/services/StockCardService/StockCardService.WebApi/Program.cs` - исправлено (connection strings)
- ✅ `docker-compose.yml` - исправлено (OpenSearch, StockCardService, Gateway)

**Разрешенные изменения:**
- ✅ Отчет создан в `Presentation` - разрешено

## Дополнительные рекомендации

1. **Мониторинг:** Настроить мониторинг доступности сервисов
2. **Логирование:** Улучшить логирование ошибок подключения
3. **Health Checks:** Добавить health checks для всех сервисов
4. **Retry Policies:** Настроить политики повторных попыток для всех внешних зависимостей

