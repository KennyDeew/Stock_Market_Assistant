# Настройка секретов для Analytics Service

## Обзор

Analytics Service поддерживает получение паролей и других секретных данных из нескольких источников с приоритетом:

1. **User Secrets** (для разработки)
2. **Переменные окружения**
3. **appsettings.json** (не рекомендуется для секретов)

## Настройка User Secrets

### 1. Инициализация User Secrets

```bash
# В директории AnalyticsService.WebApi
dotnet user-secrets init
```

### 2. Добавление секретов

```bash
# Пароль базы данных
dotnet user-secrets set "Database:Password" "your-secure-password"

# Kafka SASL учетные данные
dotnet user-secrets set "Kafka:SaslUsername" "your-kafka-username"
dotnet user-secrets set "Kafka:SaslPassword" "your-kafka-password"

# Redis пароль
dotnet user-secrets set "Redis:Password" "your-redis-password"

# API ключи
dotnet user-secrets set "ApiKeys:ExternalServiceKey" "your-api-key"
```

### 3. Просмотр секретов

```bash
dotnet user-secrets list
```

### 4. Удаление секретов

```bash
dotnet user-secrets remove "Database:Password"
dotnet user-secrets clear  # удалить все секреты
```

## Настройка переменных окружения

### Windows (PowerShell)

```powershell
$env:ANALYTICS_DB_PASSWORD="your-secure-password"
$env:KAFKA_SASL_USERNAME="your-kafka-username"
$env:KAFKA_SASL_PASSWORD="your-kafka-password"
$env:REDIS_PASSWORD="your-redis-password"
$env:API_KEY_EXTERNALSERVICEKEY="your-api-key"
```

### Windows (Command Prompt)

```cmd
set ANALYTICS_DB_PASSWORD=your-secure-password
set KAFKA_SASL_USERNAME=your-kafka-username
set KAFKA_SASL_PASSWORD=your-kafka-password
set REDIS_PASSWORD=your-redis-password
set API_KEY_EXTERNALSERVICEKEY=your-api-key
```

### Linux/macOS

```bash
export ANALYTICS_DB_PASSWORD="your-secure-password"
export KAFKA_SASL_USERNAME="your-kafka-username"
export KAFKA_SASL_PASSWORD="your-kafka-password"
export REDIS_PASSWORD="your-redis-password"
export API_KEY_EXTERNALSERVICEKEY="your-api-key"
```

## Структура секретов

### База данных

```json
{
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Name": "analytics-db",
    "Username": "postgres",
    "Password": "your-secure-password"
  }
}
```

### Kafka

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "analytics-service-group",
    "Topic": "portfolio-transactions",
    "AutoOffsetReset": "Earliest",
    "EnableAutoCommit": false,
    "SaslUsername": "your-kafka-username",
    "SaslPassword": "your-kafka-password",
    "SaslMechanism": "PLAIN",
    "SecurityProtocol": "PLAINTEXT"
  }
}
```

### Redis

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "KeyPrefix": "asset-prices:",
    "Password": "your-redis-password",
    "Host": "localhost",
    "Port": 6379
  }
}
```

### API ключи

```json
{
  "ApiKeys": {
    "ExternalServiceKey": "your-api-key"
  }
}
```

## Docker и переменные окружения

### Docker Compose

```yaml
version: '3.8'
services:
  analytics-service:
    build: .
    environment:
      - ANALYTICS_DB_PASSWORD=your-secure-password
      - KAFKA_SASL_USERNAME=your-kafka-username
      - KAFKA_SASL_PASSWORD=your-kafka-password
      - REDIS_PASSWORD=your-redis-password
      - API_KEY_EXTERNALSERVICEKEY=your-api-key
```

### Docker run

```bash
docker run -e ANALYTICS_DB_PASSWORD="your-secure-password" \
           -e KAFKA_SASL_USERNAME="your-kafka-username" \
           -e KAFKA_SASL_PASSWORD="your-kafka-password" \
           -e REDIS_PASSWORD="your-redis-password" \
           -e API_KEY_EXTERNALSERVICEKEY="your-api-key" \
           analytics-service
```

## Проверка конфигурации

### 1. Логирование

Сервис автоматически логирует загруженную конфигурацию (без паролей):

```
info: Конфигурация базы данных загружена: Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=***
info: Конфигурация Kafka загружена: localhost:9092, SASL: False
info: Конфигурация Redis загружена: localhost:6379,defaultDatabase=0
```

### 2. Валидация секретов

При запуске сервис проверяет наличие необходимых секретов:

```
info: Все необходимые секреты загружены успешно
```

или

```
warn: Некоторые секреты не настроены
```

## Безопасность

### ✅ Рекомендуется

- Использовать User Secrets для разработки
- Использовать переменные окружения для продакшена
- Использовать менеджеры секретов (Azure Key Vault, AWS Secrets Manager)
- Регулярно ротировать пароли

### ❌ Не рекомендуется

- Хранить пароли в appsettings.json
- Коммитить секреты в репозиторий
- Использовать простые пароли
- Передавать секреты через командную строку

## Примеры использования

### Получение конфигурации в коде

```csharp
public class SomeService
{
    private readonly SecretsService _secretsService;
    
    public SomeService(SecretsService secretsService)
    {
        _secretsService = secretsService;
    }
    
    public void DoSomething()
    {
        var dbConfig = _secretsService.GetDatabaseConfiguration();
        var kafkaConfig = _secretsService.GetKafkaConfiguration();
        var redisConfig = _secretsService.GetRedisConfiguration();
        
        var apiKey = _secretsService.GetApiKey("ExternalServiceKey");
    }
}
```

## Устранение неполадок

### Проблема: "Секреты не найдены"

**Решение:**
1. Проверьте, что User Secrets инициализированы
2. Проверьте правильность ключей секретов
3. Проверьте переменные окружения

### Проблема: "Ошибка подключения к базе данных"

**Решение:**
1. Проверьте пароль базы данных
2. Проверьте права доступа пользователя
3. Проверьте настройки сети

### Проблема: "Kafka SASL ошибка аутентификации"

**Решение:**
1. Проверьте SASL учетные данные
2. Проверьте механизм SASL
3. Проверьте протокол безопасности

## Поддержка

При возникновении проблем с секретами:

1. Проверьте логи сервиса
2. Убедитесь, что все необходимые секреты настроены
3. Проверьте приоритеты источников секретов
4. Обратитесь к команде разработки
