# Скрипт для проверки работы Kafka Consumer в AnalyticsService

param(
    [string]$BootstrapServer = "localhost:9092",
    [string]$Topic = "portfolio.transactions",
    [string]$ConsumerGroup = "analytics-service-transactions"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Диагностика Kafka Consumer" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Проверка доступности Kafka
Write-Host "1. Проверка доступности Kafka..." -ForegroundColor Yellow
$kafkaCheck = Test-NetConnection localhost -Port 9092 -InformationLevel Quiet -WarningAction SilentlyContinue
if ($kafkaCheck) {
    Write-Host "   ✅ Kafka доступен на $BootstrapServer" -ForegroundColor Green
} else {
    Write-Host "   ❌ Kafka недоступен на $BootstrapServer" -ForegroundColor Red
    Write-Host "   Запустите Kafka перед проверкой" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# 2. Проверка процессов AnalyticsService
Write-Host "2. Проверка запуска AnalyticsService..." -ForegroundColor Yellow
$analyticsProcess = Get-Process | Where-Object {$_.ProcessName -like "*AnalyticsService*" -or $_.ProcessName -like "*Analytics*"}
if ($analyticsProcess) {
    Write-Host "   ✅ AnalyticsService запущен (PID: $($analyticsProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "   ❌ AnalyticsService не запущен" -ForegroundColor Red
    Write-Host "   Запустите AnalyticsService:" -ForegroundColor Yellow
    Write-Host "   cd src\backend\services\AnalyticsService\AnalyticsService.WebApi" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    Write-Host ""
}
Write-Host ""

# 3. Проверка Consumer Group (если доступен Kafka CLI)
Write-Host "3. Проверка Consumer Group..." -ForegroundColor Yellow
$kafkaConsumerGroups = Get-Command kafka-consumer-groups -ErrorAction SilentlyContinue
if ($kafkaConsumerGroups) {
    try {
        $groupInfo = kafka-consumer-groups --bootstrap-server $BootstrapServer --group $ConsumerGroup --describe 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Consumer Group '$ConsumerGroup' найден" -ForegroundColor Green
            Write-Host "   Информация о группе:" -ForegroundColor Gray
            $groupInfo | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        } else {
            Write-Host "   ⚠️ Consumer Group '$ConsumerGroup' не найден или не активен" -ForegroundColor Yellow
            Write-Host "   Это может означать, что Consumer еще не подключился к Kafka" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ⚠️ Не удалось получить информацию о Consumer Group" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️ kafka-consumer-groups не найден в PATH" -ForegroundColor Yellow
    Write-Host "   Установите Kafka CLI для проверки Consumer Group" -ForegroundColor Gray
}
Write-Host ""

# 4. Проверка топика (если доступен Kafka CLI)
Write-Host "4. Проверка топика..." -ForegroundColor Yellow
$kafkaTopics = Get-Command kafka-topics -ErrorAction SilentlyContinue
if ($kafkaTopics) {
    try {
        $topics = kafka-topics --bootstrap-server $BootstrapServer --list 2>&1
        if ($topics -match $Topic) {
            Write-Host "   ✅ Топик '$Topic' существует" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Топик '$Topic' не найден" -ForegroundColor Red
            Write-Host "   Создайте топик:" -ForegroundColor Yellow
            Write-Host "   kafka-topics --create --bootstrap-server $BootstrapServer --topic $Topic --partitions 3 --replication-factor 1" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ⚠️ Не удалось проверить топик" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️ kafka-topics не найден в PATH" -ForegroundColor Yellow
}
Write-Host ""

# 5. Рекомендации
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Рекомендации для диагностики:" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Проверьте логи AnalyticsService на наличие:" -ForegroundColor Yellow
Write-Host "   - 'Запуск Kafka Consumer для топика: portfolio.transactions'" -ForegroundColor Gray
Write-Host "   - 'Успешно подписались на топик portfolio.transactions'" -ForegroundColor Gray
Write-Host "   - 'Получен батч из X сообщений'" -ForegroundColor Gray
Write-Host "   - 'Батч транзакций сохранен: X транзакций'" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Проверьте конфигурацию в appsettings.json:" -ForegroundColor Yellow
Write-Host "   - Kafka.BootstrapServers: $BootstrapServer" -ForegroundColor Gray
Write-Host "   - Kafka.Topic: $Topic" -ForegroundColor Gray
Write-Host "   - Kafka.ConsumerGroup: $ConsumerGroup" -ForegroundColor Gray
Write-Host "   - ConnectionStrings.analytics-db: (должна быть заполнена)" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Проверьте базу данных:" -ForegroundColor Yellow
Write-Host "   SELECT COUNT(*) FROM asset_transactions;" -ForegroundColor Gray
Write-Host "   SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 5;" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Отправьте тестовое сообщение:" -ForegroundColor Yellow
Write-Host "   .\scripts\send_test_kafka_message.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Проверьте Consumer Lag (если доступен Kafka CLI):" -ForegroundColor Yellow
Write-Host "   kafka-consumer-groups --bootstrap-server $BootstrapServer --group $ConsumerGroup --describe" -ForegroundColor Gray
Write-Host ""

