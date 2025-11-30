#!/usr/bin/env python3
"""
Скрипт для отправки тестового сообщения в Kafka топик portfolio.transactions
для проверки работы AnalyticsService Consumer

Использование:
    python scripts/send_test_kafka_message.py

Или с параметрами:
    python scripts/send_test_kafka_message.py --bootstrap-server localhost:9092 --topic portfolio.transactions
"""

import json
import uuid
import argparse
from datetime import datetime, timezone
from kafka import KafkaProducer
from kafka.errors import KafkaError


def create_test_transaction_message():
    """Создает тестовое сообщение о транзакции"""
    transaction_id = str(uuid.uuid4())
    portfolio_id = str(uuid.uuid4())
    stock_card_id = str(uuid.uuid4())

    message = {
        "id": transaction_id,
        "portfolioId": portfolio_id,
        "stockCardId": stock_card_id,
        "assetType": 1,  # Share
        "transactionType": 1,  # Buy
        "quantity": 100,
        "pricePerUnit": 250.75,
        "totalAmount": 25075.00,
        "transactionTime": datetime.now(timezone.utc).isoformat(),
        "currency": "RUB",
        "metadata": None
    }

    return message, portfolio_id


def send_message(bootstrap_servers, topic, message, key):
    """Отправляет сообщение в Kafka"""
    try:
        producer = KafkaProducer(
            bootstrap_servers=bootstrap_servers,
            value_serializer=lambda v: json.dumps(v).encode('utf-8'),
            key_serializer=lambda k: k.encode('utf-8') if k else None
        )

        future = producer.send(topic, key=key, value=message)

        # Ждем подтверждения
        record_metadata = future.get(timeout=10)

        print(f"✅ Сообщение успешно отправлено!")
        print(f"   Топик: {record_metadata.topic}")
        print(f"   Partition: {record_metadata.partition}")
        print(f"   Offset: {record_metadata.offset}")
        print(f"   Transaction ID: {message['id']}")
        print(f"   Portfolio ID: {message['portfolioId']}")
        print(f"   Stock Card ID: {message['stockCardId']}")

        producer.close()
        return True

    except KafkaError as e:
        print(f"❌ Ошибка Kafka: {e}")
        return False
    except Exception as e:
        print(f"❌ Ошибка: {e}")
        return False


def main():
    parser = argparse.ArgumentParser(
        description='Отправка тестового сообщения в Kafka для AnalyticsService'
    )
    parser.add_argument(
        '--bootstrap-server',
        default='localhost:9092',
        help='Адрес Kafka брокера (по умолчанию: localhost:9092)'
    )
    parser.add_argument(
        '--topic',
        default='portfolio.transactions',
        help='Название топика (по умолчанию: portfolio.transactions)'
    )
    parser.add_argument(
        '--count',
        type=int,
        default=1,
        help='Количество сообщений для отправки (по умолчанию: 1)'
    )
    parser.add_argument(
        '--transaction-type',
        type=int,
        choices=[1, 2],
        default=1,
        help='Тип транзакции: 1=Buy, 2=Sell (по умолчанию: 1)'
    )
    parser.add_argument(
        '--asset-type',
        type=int,
        choices=[1, 2, 3],
        default=1,
        help='Тип актива: 1=Share, 2=Bond, 3=Crypto (по умолчанию: 1)'
    )

    args = parser.parse_args()

    print("=" * 60)
    print("Отправка тестового сообщения в Kafka")
    print("=" * 60)
    print(f"Bootstrap Server: {args.bootstrap_server}")
    print(f"Topic: {args.topic}")
    print(f"Количество сообщений: {args.count}")
    print()

    success_count = 0
    fail_count = 0

    for i in range(args.count):
        message, key = create_test_transaction_message()
        message['transactionType'] = args.transaction_type
        message['assetType'] = args.asset_type

        print(f"[{i+1}/{args.count}] Отправка сообщения...")

        if send_message(args.bootstrap_server, args.topic, message, key):
            success_count += 1
        else:
            fail_count += 1

        print()

    print("=" * 60)
    print(f"Результат: {success_count} успешно, {fail_count} ошибок")
    print("=" * 60)

    if success_count > 0:
        print("\n💡 Проверьте логи AnalyticsService для подтверждения обработки сообщения")
        print("💡 Проверьте базу данных: SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 10;")


if __name__ == '__main__':
    main()

