# Быстрая проверка получения сообщений из Kafka

## Вариант 1: Через PowerShell (Windows) ⚡

```powershell
# 1. Отправьте тестовое сообщение
.\scripts\send_test_kafka_message.ps1

# 2. Проверьте логи AnalyticsService - должны появиться записи:
#    - "Получен батч из 1 сообщений"
#    - "Батч транзакций сохранен: 1 транзакций за один запрос к БД"

# 3. Проверьте базу данных
psql -U postgres -d analytics-db -c "SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 5;"
```

## Вариант 2: Через Kafka CLI ⚡

```bash
# 1. Отправьте сообщение
echo '{"id":"550e8400-e29b-41d4-a716-446655440000","portfolioId":"550e8400-e29b-41d4-a716-446655440001","stockCardId":"550e8400-e29b-41d4-a716-446655440002","assetType":1,"transactionType":1,"quantity":100,"pricePerUnit":250.75,"totalAmount":25075.00,"transactionTime":"2025-01-29T14:30:00Z","currency":"RUB"}' | kafka-console-producer --bootstrap-server localhost:9092 --topic portfolio.transactions

# 2. Проверьте Consumer Group
kafka-consumer-groups --bootstrap-server localhost:9092 --group analytics-service-transactions --describe
```

## Вариант 3: Через Python скрипт ⚡

```bash
# 1. Установите зависимости (если еще не установлены)
pip install kafka-python

# 2. Запустите скрипт
python scripts/send_test_kafka_message.py

# 3. Проверьте результат в логах и БД
```

## Что должно произойти:

1. ✅ **В логах AnalyticsService:**
   ```
   [Info] Получен батч из 1 сообщений
   [Info] Батч транзакций сохранен: 1 транзакций за один запрос к БД
   [Info] Опубликовано событий TransactionReceivedEvent: 1
   [Info] Успешно обработано и закоммичено 1 из 1 сообщений в Kafka
   ```

2. ✅ **В базе данных:**
   - Новая запись в таблице `asset_transactions`
   - Обновленные рейтинги в таблице `asset_ratings`

3. ✅ **Consumer Group:**
   - Lag должен быть 0 или близок к 0
   - Consumer должен быть активен

## Если не работает:

1. Проверьте, что Kafka запущен: `kafka-broker-api-versions --bootstrap-server localhost:9092`
2. Проверьте, что AnalyticsService запущен и подписан на топик (см. логи)
3. Проверьте подключение к PostgreSQL
4. Проверьте формат JSON сообщения

