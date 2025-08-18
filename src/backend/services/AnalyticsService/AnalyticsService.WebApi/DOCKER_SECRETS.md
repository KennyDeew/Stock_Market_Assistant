# Настройка Docker секретов для AnalyticsService

## Обзор

AnalyticsService поддерживает получение паролей и других секретных данных из Docker секретов, переменных окружения и конфигурации приложения с приоритетом:

1. **Docker секреты** (файлы в `/run/secrets/`) - высший приоритет
2. **Переменные окружения** - средний приоритет
3. **Конфигурация приложения** (appsettings.json) - низший приоритет

## Способы передачи секретов

### 1. Docker секреты (рекомендуется для продакшена)

Создайте секреты в Docker Swarm:

```bash
# Создание секретов
echo "your_secure_password" | docker secret create analytics_db_password -
echo "your_kafka_password" | docker secret create kafka_sasl_password -
echo "your_redis_password" | docker secret create redis_password -

# Использование в docker-compose.yml
version: '3.8'
services:
  analytics-service:
    image: analytics-service:latest
    secrets:
      - analytics_db_password
      - kafka_sasl_password
      - redis_password
    environment:
      - ANALYTICS_DB_HOST=postgres
      - KAFKA_BOOTSTRAP_SERVERS=kafka:9092
      - REDIS_HOST=redis

secrets:
  analytics_db_password:
    external: true
  kafka_sasl_password:
    external: true
  redis_password:
    external: true
```

### 2. Переменные окружения

```bash
# База данных
ANALYTICS_DB_HOST=postgres
ANALYTICS_DB_PORT=5432
ANALYTICS_DB_NAME=analytics-db
ANALYTICS_DB_USERNAME=postgres
ANALYTICS_DB_PASSWORD=your_secure_password

# Kafka
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
KAFKA_GROUP_ID=analytics-service-group
KAFKA_TOPIC=portfolio-transactions
KAFKA_SASL_USERNAME=your_kafka_username
KAFKA_SASL_PASSWORD=your_kafka_password

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_DATABASE=0
REDIS_PASSWORD=your_redis_password
```

### 3. Альтернативные имена переменных

Для совместимости поддерживаются стандартные имена переменных:

```bash
# PostgreSQL
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=analytics-db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# Kafka
KAFKA_SERVERS=kafka:9092
KAFKA_USERNAME=your_kafka_username
KAFKA_PASSWORD=your_kafka_password

# Redis
REDIS_SERVER=redis
REDIS_AUTH=your_redis_password
```

## Маппинг секретов

| Секрет | Docker секрет | Переменная окружения | Описание |
|--------|---------------|---------------------|----------|
| Database:Password | `analytics_db_password` | `ANALYTICS_DB_PASSWORD` | Пароль PostgreSQL |
| Kafka:SaslPassword | `kafka_sasl_password` | `KAFKA_SASL_PASSWORD` | Пароль Kafka SASL |
| Redis:Password | `redis_password` | `REDIS_PASSWORD` | Пароль Redis |

## Проверка конфигурации

При запуске сервиса проверьте логи на наличие сообщений:

```
Конфигурация базы данных загружена: Host=postgres;Port=5432;Database=analytics-db;Username=postgres;Password=...
Конфигурация Kafka загружена: kafka:9092, SASL: True
Конфигурация Redis загружена: postgres:6379,password=...,defaultDatabase=0
```

## Безопасность

- **Никогда не храните пароли в appsettings.json**
- Используйте Docker секреты для продакшена
- Переменные окружения подходят для разработки и тестирования
- Регулярно ротируйте пароли
- Используйте сложные пароли (минимум 16 символов)
