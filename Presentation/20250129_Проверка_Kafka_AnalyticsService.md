# Проверка получения сообщений из Kafka для AnalyticsService

**Дата:** 2025-01-29
**Сервис:** AnalyticsService
**Топик:** `portfolio.transactions`

---

## 1. Подготовка

### 1.1 Проверка конфигурации

**Файл:** `appsettings.json`
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

### 1.2 Убедитесь, что запущены:
- ✅ Kafka брокер (localhost:9092)
- ✅ AnalyticsService
- ✅ PostgreSQL база данных

---

## 2. Способы проверки

### 2.1 Способ 1: Проверка логов приложения

**Что искать в логах:**

1. **Успешная подписка на топик:**
   ```
   [Info] Успешно подписались на топик portfolio.transactions
   ```

2. **Получение батча сообщений:**
   ```
   [Info] Получен батч из {Count} сообщений
   ```

3. **Сохранение транзакций:**
   ```
   [Info] Батч транзакций сохранен: {Count} транзакций за один запрос к БД
   ```

4. **Публикация событий:**
   ```
   [Info] Опубликовано событий TransactionReceivedEvent: {Count}
   ```

5. **Коммит в Kafka:**
   ```
   [Info] Успешно обработано и закоммичено {ProcessedCount} из {TotalCount} сообщений в Kafka
   ```

**Где смотреть логи:**
- Консоль приложения
- OpenSearch (если настроен): `analytics-service-{дата}`

---

### 2.2 Способ 2: Проверка в базе данных

**SQL запросы для проверки:**

```sql
-- Проверка количества транзакций
SELECT COUNT(*) FROM asset_transactions;

-- Последние транзакции
SELECT
    id,
    portfolio_id,
    stock_card_id,
    transaction_type,
    quantity,
    price_per_unit,
    total_amount,
    transaction_time
FROM asset_transactions
ORDER BY transaction_time DESC
LIMIT 10;

-- Транзакции за последний час
SELECT COUNT(*)
FROM asset_transactions
WHERE transaction_time >= NOW() - INTERVAL '1 hour';

-- Статистика по типам транзакций
SELECT
    transaction_type,
    COUNT(*) as count,
    SUM(total_amount) as total_amount
FROM asset_transactions
GROUP BY transaction_type;
```

---

### 2.3 Способ 3: Отправка тестового сообщения через Kafka CLI

**Использование kafka-console-producer:**

```bash
# Подключение к Kafka
kafka-console-producer --bootstrap-server localhost:9092 --topic portfolio.transactions

# Затем введите JSON сообщение:
```

**Пример тестового сообщения:**
```json
{"id":"123e4567-e89b-12d3-a456-426614174000","portfolioId":"123e4567-e89b-12d3-a456-426614174001","stockCardId":"123e4567-e89b-12d3-a456-426614174002","assetType":1,"transactionType":1,"quantity":10,"pricePerUnit":100.50,"totalAmount":1005.00,"transactionTime":"2025-01-29T12:00:00Z","currency":"RUB"}
```

**Или через файл:**
```bash
echo '{"id":"123e4567-e89b-12d3-a456-426614174000","portfolioId":"123e4567-e89b-12d3-a456-426614174001","stockCardId":"123e4567-e89b-12d3-a456-426614174002","assetType":1,"transactionType":1,"quantity":10,"pricePerUnit":100.50,"totalAmount":1005.00,"transactionTime":"2025-01-29T12:00:00Z","currency":"RUB"}' | kafka-console-producer --bootstrap-server localhost:9092 --topic portfolio.transactions
```

---

### 2.4 Способ 4: Использование Python скрипта

См. файл `scripts/send_test_kafka_message.py` (будет создан ниже)

---

### 2.5 Способ 5: Проверка через PortfolioService

Если PortfolioService настроен и работает:

1. Создайте транзакцию через API PortfolioService
2. Транзакция автоматически отправится в Kafka
3. AnalyticsService получит и обработает сообщение

---

## 3. Мониторинг в реальном времени

### 3.1 Kafka Consumer Groups

```bash
# Проверка статуса consumer group
kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --describe

# Проверка lag (отставание)
kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --describe | grep LAG
```

### 3.2 Просмотр сообщений в топике

```bash
# Чтение последних сообщений из топика
kafka-console-consumer --bootstrap-server localhost:9092 --topic portfolio.transactions --from-beginning --max-messages 10
```

---

## 4. Диагностика проблем

### 4.1 Consumer не получает сообщения

**Проверьте:**
- ✅ Kafka брокер доступен
- ✅ Топик `portfolio.transactions` существует
- ✅ Consumer подписан на топик (см. логи)
- ✅ Нет ошибок в логах

**Создание топика вручную:**
```bash
kafka-topics --create --bootstrap-server localhost:9092 --topic portfolio.transactions --partitions 1 --replication-factor 1
```

### 4.2 Сообщения не сохраняются в БД

**Проверьте:**
- ✅ Подключение к PostgreSQL
- ✅ Таблица `asset_transactions` существует
- ✅ Нет ошибок в логах при сохранении

**Проверка подключения:**
```sql
SELECT version();
```

### 4.3 Ошибки десериализации

**Признаки:**
```
[Warning] Не удалось десериализовать сообщение: {Value}
```

**Решение:**
- Проверьте формат JSON сообщения
- Убедитесь, что все обязательные поля присутствуют
- Проверьте типы данных

---

## 5. Тестовые данные

### 5.1 Пример валидного сообщения

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "portfolioId": "550e8400-e29b-41d4-a716-446655440001",
  "stockCardId": "550e8400-e29b-41d4-a716-446655440002",
  "assetType": 1,
  "transactionType": 1,
  "quantity": 100,
  "pricePerUnit": 250.75,
  "totalAmount": 25075.00,
  "transactionTime": "2025-01-29T14:30:00Z",
  "currency": "RUB",
  "metadata": null
}
```

### 5.2 Типы данных

- **AssetType:** 1 = Share, 2 = Bond, 3 = Crypto
- **TransactionType:** 1 = Buy, 2 = Sell
- **Currency:** "RUB", "USD", "EUR" и т.д.

---

## 6. Метрики для мониторинга

### 6.1 Ключевые метрики

- Количество обработанных сообщений в секунду
- Размер батча
- Время обработки батча
- Количество ошибок
- Lag consumer group

### 6.2 Алерты

Настройте алерты на:
- Lag > 1000 сообщений
- Ошибки обработки > 5%
- Время обработки батча > 5 секунд

---

**Автор:** AI Assistant
**Дата:** 2025-01-29

