#!/bin/bash
# Скрипт для проверки статуса Kafka Consumer для AnalyticsService

BOOTSTRAP_SERVER="${1:-localhost:9092}"
CONSUMER_GROUP="${2:-analytics-service-transactions}"
TOPIC="${3:-portfolio.transactions}"

echo "=========================================="
echo "Проверка статуса Kafka Consumer"
echo "=========================================="
echo "Bootstrap Server: $BOOTSTRAP_SERVER"
echo "Consumer Group: $CONSUMER_GROUP"
echo "Topic: $TOPIC"
echo ""

# Проверка доступности Kafka
echo "1. Проверка доступности Kafka..."
if kafka-broker-api-versions --bootstrap-server "$BOOTSTRAP_SERVER" &>/dev/null; then
    echo "   ✅ Kafka доступен"
else
    echo "   ❌ Kafka недоступен. Убедитесь, что брокер запущен на $BOOTSTRAP_SERVER"
    exit 1
fi
echo ""

# Проверка существования топика
echo "2. Проверка существования топика..."
if kafka-topics --bootstrap-server "$BOOTSTRAP_SERVER" --list | grep -q "^${TOPIC}$"; then
    echo "   ✅ Топик '$TOPIC' существует"

    # Показываем информацию о топике
    echo "   Информация о топике:"
    kafka-topics --bootstrap-server "$BOOTSTRAP_SERVER" --describe --topic "$TOPIC" | sed 's/^/      /'
else
    echo "   ⚠️  Топик '$TOPIC' не существует"
    echo "   Создание топика..."
    kafka-topics --create \
        --bootstrap-server "$BOOTSTRAP_SERVER" \
        --topic "$TOPIC" \
        --partitions 1 \
        --replication-factor 1
    echo "   ✅ Топик создан"
fi
echo ""

# Проверка consumer group
echo "3. Проверка Consumer Group..."
if kafka-consumer-groups --bootstrap-server "$BOOTSTRAP_SERVER" --list | grep -q "^${CONSUMER_GROUP}$"; then
    echo "   ✅ Consumer Group '$CONSUMER_GROUP' существует"
    echo ""
    echo "   Статус Consumer Group:"
    kafka-consumer-groups \
        --bootstrap-server "$BOOTSTRAP_SERVER" \
        --group "$CONSUMER_GROUP" \
        --describe | sed 's/^/      /'

    echo ""
    echo "   Lag (отставание):"
    kafka-consumer-groups \
        --bootstrap-server "$BOOTSTRAP_SERVER" \
        --group "$CONSUMER_GROUP" \
        --describe | grep -E "LAG|TOPIC" | sed 's/^/      /'
else
    echo "   ⚠️  Consumer Group '$CONSUMER_GROUP' не найдена"
    echo "   Это нормально, если AnalyticsService еще не запущен или не подписался на топик"
fi
echo ""

# Количество сообщений в топике
echo "4. Статистика топика..."
echo "   Последние сообщения в топике (первые 5):"
timeout 5 kafka-console-consumer \
    --bootstrap-server "$BOOTSTRAP_SERVER" \
    --topic "$TOPIC" \
    --from-beginning \
    --max-messages 5 \
    --timeout-ms 5000 2>/dev/null | head -5 | sed 's/^/      /' || echo "      (нет сообщений или таймаут)"
echo ""

echo "=========================================="
echo "Проверка завершена"
echo "=========================================="

