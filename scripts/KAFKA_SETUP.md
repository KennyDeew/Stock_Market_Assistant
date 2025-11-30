# Настройка и запуск Kafka для тестирования

## 🔍 Проблема: Kafka недоступен

Если вы видите ошибки:
```
Connect to ipv4#127.0.0.1:9092 failed: Unknown error
Connect to ipv6#[::1]:9092 failed: Unknown error
```

Это означает, что **Kafka не запущен** или недоступен на порту 9092.

## ✅ Решения

### Вариант 1: Docker Compose (для StockCardService)

В проекте есть `docker-compose_StockCard.yml` для StockCardService:

```powershell
# Перейдите в директорию StockCardService
cd src\backend\services\StockCardService

# Запустите Kafka и связанные сервисы
docker-compose -f docker-compose_StockCard.yml up -d

# Проверьте, что Kafka запущен
docker ps | Select-String "kafka"
```

**⚠️ Важно:** В этом docker-compose Kafka настроен на порт **29091** (не 9092)!
Если используете этот вариант, запустите скрипт с правильным портом:
```powershell
.\scripts\send_test_kafka_message.ps1 -BootstrapServer "localhost:29091"
```

### Вариант 2: Локальная установка Kafka

#### Шаг 1: Скачайте Kafka
1. Перейдите на https://kafka.apache.org/downloads
2. Скачайте последнюю версию (например, `kafka_2.13-3.6.1.tgz`)
3. Распакуйте архив

#### Шаг 2: Запустите Zookeeper
```powershell
# В директории Kafka
.\bin\windows\zookeeper-server-start.bat .\config\zookeeper.properties
```

#### Шаг 3: Запустите Kafka
```powershell
# В новом окне терминала, в директории Kafka
.\bin\windows\kafka-server-start.bat .\config\server.properties
```

### Вариант 3: Использование .NET Aspire (РЕКОМЕНДУЕТСЯ для этого проекта)

В этом проекте настроен **.NET Aspire** с Kafka. Это самый простой способ:

```powershell
# Перейдите в директорию AppHost
cd src\StockMarketAssistant.AppHost

# Запустите Aspire Host (это запустит Kafka и все сервисы)
dotnet run
```

После запуска:
- Kafka будет доступен на порту, который Aspire назначит автоматически
- Откройте браузер на адресе, который покажет Aspire (обычно `https://localhost:15000`)
- В Aspire Dashboard вы увидите все сервисы, включая Kafka

**⚠️ Важно:** Если используете Aspire, порт Kafka может отличаться от 9092.
Проверьте настройки в Aspire Dashboard или используйте адрес, который показывает Aspire.

### Вариант 4: Проверка существующего Kafka

```powershell
# Проверьте, запущен ли Kafka
Get-Process | Where-Object {$_.ProcessName -like "*kafka*"}

# Проверьте доступность порта
Test-NetConnection localhost -Port 9092

# Если Kafka запущен на другом порту, укажите его в скрипте:
.\scripts\send_test_kafka_message.ps1 -BootstrapServer "localhost:9093"
```

## 🔧 Создание топика (если нужно)

Если топик `portfolio.transactions` не существует:

```powershell
# Используя Kafka CLI (если установлен)
kafka-topics --create `
  --bootstrap-server localhost:9092 `
  --topic portfolio.transactions `
  --partitions 3 `
  --replication-factor 1

# Проверка топика
kafka-topics --list --bootstrap-server localhost:9092
```

## 📋 Проверка после запуска

1. **Проверьте процессы:**
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -like "*kafka*" -or $_.ProcessName -like "*zookeeper*"}
   ```

2. **Проверьте порт:**
   ```powershell
   Test-NetConnection localhost -Port 9092
   ```

3. **Проверьте логи Kafka** (если запущен локально):
   - Zookeeper: `logs/zookeeper.out`
   - Kafka: `logs/server.log`

## 🚀 После запуска Kafka

Запустите скрипт отправки сообщения:

```powershell
.\scripts\send_test_kafka_message.ps1
```

Скрипт автоматически проверит доступность Kafka перед отправкой сообщения.

## ⚠️ Важные замечания

1. **Zookeeper должен быть запущен перед Kafka**
2. **Порт 9092 должен быть свободен** (или используйте другой порт)
3. **Если используете Docker**, убедитесь, что порты проброшены правильно
4. **Если Kafka на удаленном сервере**, укажите правильный адрес:
   ```powershell
   .\scripts\send_test_kafka_message.ps1 -BootstrapServer "kafka-server:9092"
   ```

## 🔗 Полезные ссылки

- [Kafka Quick Start](https://kafka.apache.org/quickstart)
- [Kafka Docker Images](https://hub.docker.com/r/apache/kafka)
- [Confluent Platform](https://www.confluent.io/get-started/)

