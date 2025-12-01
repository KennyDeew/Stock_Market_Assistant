# Скрипт для проверки сообщений в топике Kafka
# Работает без установки Kafka CLI - использует Python или .NET

param(
    [string]$BootstrapServer = "localhost:9092",
    [string]$Topic = "portfolio.transactions",
    [int]$MaxMessages = 10,
    [string]$Method = "auto", # auto, dotnet, python, docker
    [switch]$FromBeginning = $false # Читать с начала топика (все сообщения) или только новые
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Проверка сообщений в топике Kafka" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Bootstrap Server: $BootstrapServer" -ForegroundColor Yellow
Write-Host "Topic: $Topic" -ForegroundColor Yellow
Write-Host "Max Messages: $MaxMessages" -ForegroundColor Yellow
Write-Host ""

# Определение метода
$useDotNet = $false
$usePython = $false
$useDocker = $false

if ($Method -eq "auto") {
    # Сначала проверяем .NET SDK
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        $csFile = Join-Path $PSScriptRoot "CheckKafkaTopic.cs"
        if (Test-Path $csFile) {
            $useDotNet = $true
            Write-Host "✅ Найден .NET SDK, будет использована C# утилита" -ForegroundColor Green
        }
    }

    # Если .NET не доступен, проверяем Python
    if (-not $useDotNet) {
        $python = Get-Command python -ErrorAction SilentlyContinue
        if ($python) {
            try {
                $kafkaPythonCheck = python -c "import kafka; print('ok')" 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $usePython = $true
                    Write-Host "✅ Найден Python с библиотекой kafka-python" -ForegroundColor Green
                }
            } catch {
                # Игнорируем ошибки
            }
        }
    }

    # Если ни .NET, ни Python не доступны, проверяем Docker
    if (-not $useDotNet -and -not $usePython) {
        $docker = Get-Command docker -ErrorAction SilentlyContinue
        if ($docker) {
            $useDocker = $true
            Write-Host "✅ Найден Docker, будет использован Kafka CLI из контейнера" -ForegroundColor Green
        }
    }
} elseif ($Method -eq "dotnet") {
    $useDotNet = $true
} elseif ($Method -eq "python") {
    $usePython = $true
} elseif ($Method -eq "docker") {
    $useDocker = $true
}

# Выполнение
if ($useDotNet) {
    Write-Host "`nИспользование .NET утилиты..." -ForegroundColor Cyan
    $csFile = Join-Path $PSScriptRoot "CheckKafkaTopic.cs"
    $csprojFile = Join-Path $PSScriptRoot "CheckKafkaTopic.csproj"

    if (-not (Test-Path $csprojFile)) {
        Write-Host "Создание проекта..." -ForegroundColor Gray
        $csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Confluent.Kafka" Version="2.3.0" />
  </ItemGroup>
</Project>
"@
        Set-Content -Path $csprojFile -Value $csprojContent
    }

    Push-Location $PSScriptRoot
    try {
        $fromBeginningArg = if ($FromBeginning) { "--from-beginning" } else { "" }
        if ($fromBeginningArg) {
            dotnet run --project CheckKafkaTopic.csproj -- --bootstrap-server $BootstrapServer --topic $Topic --max-messages $MaxMessages --from-beginning
        } else {
            dotnet run --project CheckKafkaTopic.csproj -- --bootstrap-server $BootstrapServer --topic $Topic --max-messages $MaxMessages
        }
    } finally {
        Pop-Location
    }
}
elseif ($usePython) {
    Write-Host "`nИспользование Python..." -ForegroundColor Cyan
    $pythonScript = @"
from kafka import KafkaConsumer
import json
import sys

bootstrap_server = sys.argv[1] if len(sys.argv) > 1 else 'localhost:9092'
topic = sys.argv[2] if len(sys.argv) > 2 else 'portfolio.transactions'
max_messages = int(sys.argv[3]) if len(sys.argv) > 3 else 10

print(f'Подключение к Kafka: {bootstrap_server}')
print(f'Топик: {topic}')
print(f'Максимум сообщений: {max_messages}')
print('=' * 50)

try:
    consumer = KafkaConsumer(
        topic,
        bootstrap_servers=[bootstrap_server],
        auto_offset_reset='earliest',
        enable_auto_commit=False,
        consumer_timeout_ms=5000
    )

    count = 0
    for message in consumer:
        count += 1
        print(f'\nСообщение #{count}:')
        print(f'  Partition: {message.partition}')
        print(f'  Offset: {message.offset}')
        print(f'  Key: {message.key}')
        print(f'  Value: {message.value.decode("utf-8") if message.value else "null"}')

        if count >= max_messages:
            break

    if count == 0:
        print('Сообщений в топике не найдено')
    else:
        print(f'\nВсего получено сообщений: {count}')

except Exception as e:
    print(f'Ошибка: {e}')
    sys.exit(1)
"@

    $tempScript = Join-Path $env:TEMP "check_kafka_topic_$(Get-Date -Format 'yyyyMMddHHmmss').py"
    Set-Content -Path $tempScript -Value $pythonScript
    try {
        python $tempScript $BootstrapServer $Topic $MaxMessages
    } finally {
        Remove-Item $tempScript -ErrorAction SilentlyContinue
    }
}
elseif ($useDocker) {
    Write-Host "`nИспользование Docker (Kafka CLI)..." -ForegroundColor Cyan
    Write-Host "Поиск контейнера Kafka..." -ForegroundColor Gray

    $kafkaContainer = docker ps --filter "name=kafka" --format "{{.Names}}" 2>&1 | Select-Object -First 1
    if (-not $kafkaContainer -or $kafkaContainer -match "Error") {
        Write-Host "❌ Контейнер Kafka не найден" -ForegroundColor Red
        Write-Host "Запустите Kafka через Docker Compose или используйте другой метод" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Использование контейнера: $kafkaContainer" -ForegroundColor Green
    docker exec $kafkaContainer kafka-console-consumer `
        --bootstrap-server localhost:9092 `
        --topic $Topic `
        --from-beginning `
        --max-messages $MaxMessages
}
else {
    Write-Host "❌ Не найден ни один доступный метод для проверки топика" -ForegroundColor Red
    Write-Host ""
    Write-Host "Установите один из вариантов:" -ForegroundColor Yellow
    Write-Host "  1. Python с kafka-python: pip install kafka-python" -ForegroundColor Gray
    Write-Host "  2. Docker для использования Kafka CLI из контейнера" -ForegroundColor Gray
    Write-Host "  3. Kafka CLI инструменты в PATH" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Или используйте скрипт отправки тестового сообщения:" -ForegroundColor Cyan
    Write-Host "  .\scripts\send_test_kafka_message.ps1" -ForegroundColor Green
    exit 1
}

