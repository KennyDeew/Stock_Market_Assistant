# 🚀 Быстрый запуск Kafka для тестирования

## Самый простой способ (через .NET Aspire)

```powershell
# 1. Запустите Aspire Host
cd src\StockMarketAssistant.AppHost
dotnet run

# 2. В новом терминале проверьте доступность Kafka
# (Aspire покажет адрес Kafka в консоли или в Dashboard)

# 3. Запустите скрипт отправки сообщения
# Если Kafka на стандартном порту:
.\scripts\send_test_kafka_message.ps1

# Если Aspire использует другой порт, укажите его:
.\scripts\send_test_kafka_message.ps1 -BootstrapServer "localhost:[PORT]"
```

## Альтернатива: Docker Compose (StockCardService)

```powershell
# 1. Запустите Kafka через Docker Compose
cd src\backend\services\StockCardService
docker-compose -f docker-compose_StockCard.yml up -d kafka zookeeper

# 2. Подождите несколько секунд, пока Kafka запустится
Start-Sleep -Seconds 10

# 3. Запустите скрипт с правильным портом (29091)
cd ..\..\..\..
.\scripts\send_test_kafka_message.ps1 -BootstrapServer "localhost:29091"
```

## Проверка

После запуска Kafka проверьте:

```powershell
# Проверьте порт (замените на ваш порт)
Test-NetConnection localhost -Port 9092
# или
Test-NetConnection localhost -Port 29091

# Проверьте процессы Docker
docker ps | Select-String "kafka"
```

## 📖 Подробные инструкции

См. [KAFKA_SETUP.md](KAFKA_SETUP.md) для всех вариантов запуска.

