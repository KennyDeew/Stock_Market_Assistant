# Инструменты для тестирования Kafka интеграции AnalyticsService

## Быстрый старт

### 1. Проверка статуса Consumer

**Windows (PowerShell):**
```powershell
# Убедитесь, что Kafka CLI доступен в PATH
kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --describe
```

**Linux/Mac:**
```bash
bash scripts/check_kafka_consumer_status.sh
```

### 2. Отправка тестового сообщения

**Windows (PowerShell):**
```powershell
.\scripts\send_test_kafka_message.ps1
```

**Linux/Mac (Python):**
```bash
# Установите зависимости
pip install kafka-python

# Запустите скрипт
python scripts/send_test_kafka_message.py
```

**Через Kafka CLI:**
```bash
echo '{"id":"123e4567-e89b-12d3-a456-426614174000","portfolioId":"123e4567-e89b-12d3-a456-426614174001","stockCardId":"123e4567-e89b-12d3-a456-426614174002","assetType":1,"transactionType":1,"quantity":10,"pricePerUnit":100.50,"totalAmount":1005.00,"transactionTime":"2025-01-29T12:00:00Z","currency":"RUB"}' | kafka-console-producer --bootstrap-server localhost:9092 --topic portfolio.transactions
```

### 3. Проверка в базе данных

```sql
-- Последние транзакции
SELECT * FROM asset_transactions
ORDER BY transaction_time DESC
LIMIT 10;

-- Количество транзакций за последний час
SELECT COUNT(*)
FROM asset_transactions
WHERE transaction_time >= NOW() - INTERVAL '1 hour';
```

## Структура сообщения

```json
{
  "id": "UUID",
  "portfolioId": "UUID",
  "stockCardId": "UUID",
  "assetType": 1,        // 1=Share, 2=Bond, 3=Crypto
  "transactionType": 1,  // 1=Buy, 2=Sell
  "quantity": 100,
  "pricePerUnit": 250.75,
  "totalAmount": 25075.00,
  "transactionTime": "2025-01-29T14:30:00Z",
  "currency": "RUB",
  "metadata": null
}
```

## Что проверять

1. ✅ Логи AnalyticsService - должны появиться записи о получении и обработке
2. ✅ База данных - транзакции должны появиться в таблице `asset_transactions`
3. ✅ Рейтинги - должны обновиться в таблице `asset_ratings`
4. ✅ Consumer Group Lag - должен быть близок к 0

## Устранение проблем

### Consumer не получает сообщения
- Проверьте, что Kafka брокер доступен
- Проверьте, что топик существует
- Проверьте логи AnalyticsService на ошибки подписки

### Сообщения не сохраняются
- Проверьте подключение к PostgreSQL
- Проверьте логи на ошибки сохранения
- Проверьте права доступа к БД

### Ошибки десериализации
- Проверьте формат JSON сообщения
- Убедитесь, что все обязательные поля присутствуют

