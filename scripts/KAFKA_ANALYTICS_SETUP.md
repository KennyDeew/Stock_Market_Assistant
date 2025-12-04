# Настройка Kafka для приема сообщений AnalyticsService

## Текущая конфигурация

### 1. Kafka (docker-compose-analytics.yml)

```yaml
kafka:
  ports:
    - "9092:9092"  # Внешний доступ для приложений на хосте
  environment:
    KAFKA_AUTO_CREATE_TOPICS_ENABLE: 'true'  # Автоматическое создание топиков
    KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092,PLAINTEXT_INTERNAL://kafka:29092
```

### 2. AnalyticsService (appsettings.json)

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ConsumerGroup": "analytics-service-transactions",
    "Topic": "portfolio.transactions",
    "BatchSize": 100
  }
}
```

### 3. Consumer настройки (Program.cs)

- **AutoOffsetReset**: `Earliest` - читает с начала, если нет сохраненного offset
- **EnableAutoCommit**: `false` - ручной коммит после успешной обработки
- **Topic**: `portfolio.transactions`

### 4. Формат сообщения

Скрипт `send_test_kafka_message.ps1` отправляет сообщение в формате:

```json
{
  "id": "guid",
  "portfolioId": "guid",
  "stockCardId": "guid",
  "assetType": 1,
  "transactionType": 1,
  "quantity": 100,
  "pricePerUnit": 250.75,
  "totalAmount": 25075.00,
  "transactionTime": "2024-01-01T12:00:00.000Z",
  "currency": "RUB",
  "metadata": null
}
```

Этот формат **полностью соответствует** ожидаемому формату `TransactionMessage` в AnalyticsService.

## Проверка настройки

### Шаг 1: Убедитесь, что Kafka запущен

```powershell
# Проверка контейнеров
docker ps | Select-String "kafka|zookeeper"

# Или используйте скрипт запуска
.\scripts\start_analytics_service.ps1
```

### Шаг 2: Проверьте, что топик создан

```powershell
# Через AKHQ (если запущен)
# Откройте http://localhost:8080 и проверьте наличие топика portfolio.transactions

# Или через Kafka CLI (если установлен)
kafka-topics --bootstrap-server localhost:9092 --list
```

### Шаг 3: Проверьте Consumer Group

```powershell
# Если установлен Kafka CLI
kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --describe
```

**Что проверить:**
- Consumer Group существует
- `LAG` (отставание) показывает количество необработанных сообщений
- Если `LAG = 0`, все сообщения обработаны

### Шаг 4: Отправьте тестовое сообщение

```powershell
# Отправка одного сообщения
.\scripts\send_test_kafka_message.ps1

# Отправка 5 сообщений
.\scripts\send_test_kafka_message.ps1 -Count 5
```

### Шаг 5: Проверьте логи AnalyticsService

```powershell
# Если AnalyticsService запущен, проверьте логи на наличие:
# - "Получен батч из X сообщений"
# - "Батч транзакций сохранен: X транзакций"
```

### Шаг 6: Проверьте базу данных

```sql
-- Проверьте последние записи
SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 10;

-- Проверьте количество записей
SELECT COUNT(*) FROM asset_transactions;
```

## Решение проблем

### Проблема 1: Consumer не читает сообщения

**Причина:** Consumer уже закоммитил offset и не читает новые сообщения.

**Решение:**
1. Сбросьте Consumer Group offset:
   ```powershell
   kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --reset-offsets --to-earliest --topic portfolio.transactions --execute
   ```
2. Перезапустите AnalyticsService

### Проблема 2: Сообщения не доставляются

**Причина:** Топик не создан или Kafka недоступен.

**Решение:**
1. Убедитесь, что Kafka запущен:
   ```powershell
   docker ps | Select-String "kafka"
   ```
2. Проверьте, что топик создан:
   ```powershell
   kafka-topics --bootstrap-server localhost:9092 --list
   ```
3. Создайте топик вручную, если нужно:
   ```powershell
   kafka-topics --bootstrap-server localhost:9092 --create --topic portfolio.transactions --partitions 1 --replication-factor 1
   ```

### Проблема 3: Ошибка десериализации

**Причина:** Формат сообщения не соответствует ожидаемому.

**Решение:**
1. Проверьте формат сообщения в AKHQ (http://localhost:8080)
2. Убедитесь, что все поля присутствуют:
   - `id`, `portfolioId`, `stockCardId` - должны быть GUID
   - `assetType`, `transactionType` - должны быть числами
   - `transactionTime` - должен быть в формате ISO 8601

### Проблема 4: Consumer не подписывается на топик

**Причина:** Kafka недоступен при старте Consumer.

**Решение:**
1. Убедитесь, что Kafka запущен перед запуском AnalyticsService
2. Используйте скрипт `start_analytics_service.ps1`, который автоматически запускает Kafka

## Рекомендуемый порядок действий

1. **Запустите Kafka и зависимые сервисы:**
   ```powershell
   .\scripts\start_analytics_service.ps1
   ```
   (Скрипт автоматически запустит Kafka, PostgreSQL и AKHQ)

2. **Дождитесь готовности всех сервисов** (проверьте логи)

3. **Отправьте тестовое сообщение:**
   ```powershell
   .\scripts\send_test_kafka_message.ps1 -Count 1
   ```

4. **Проверьте в AKHQ:**
   - Откройте http://localhost:8080
   - Выберите кластер "analytics-kafka"
   - Перейдите в топик "portfolio.transactions"
   - Убедитесь, что сообщение появилось

5. **Проверьте логи AnalyticsService:**
   - Должно появиться сообщение "Получен батч из 1 сообщений"
   - Должно появиться сообщение "Батч транзакций сохранен"

6. **Проверьте базу данных:**
   ```sql
   SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 1;
   ```

## Дополнительная информация

- **Топик:** `portfolio.transactions`
- **Consumer Group:** `analytics-service-transactions`
- **Bootstrap Servers:** `localhost:9092`
- **Auto Offset Reset:** `Earliest` (читает с начала, если нет сохраненного offset)
- **Auto Commit:** `false` (ручной коммит после успешной обработки)

## Проверка через AKHQ

1. Откройте http://localhost:8080
2. Выберите кластер "analytics-kafka"
3. Перейдите в раздел "Topics"
4. Найдите топик "portfolio.transactions"
5. Нажмите на топик для просмотра сообщений
6. Проверьте Consumer Groups - должна быть группа "analytics-service-transactions"




