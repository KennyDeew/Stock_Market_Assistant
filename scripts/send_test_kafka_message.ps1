# Скрипт PowerShell для отправки тестового сообщения в Kafka
# Автоматически выбирает метод: .NET SDK (предпочтительно), Python с kafka-python, или kafka-console-producer

param(
    [string]$BootstrapServer = "localhost:9092",
    [string]$Topic = "portfolio.transactions",
    [int]$Count = 1,
    [int]$TransactionType = 1,  # 1=Buy, 2=Sell
    [int]$AssetType = 1,  # 1=Share, 2=Bond, 3=Crypto
    [string]$Method = "auto"  # auto, dotnet, python, kafka-cli
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Отправка тестового сообщения в Kafka" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Bootstrap Server: $BootstrapServer"
Write-Host "Topic: $Topic"
Write-Host "Количество сообщений: $Count"
Write-Host ""

# Определение метода отправки
$useDotNet = $false
$usePython = $false
$useKafkaCli = $false

if ($Method -eq "auto") {
    # Сначала проверяем .NET SDK (предпочтительно для этого проекта)
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        $programCs = Join-Path $PSScriptRoot "Program.cs"
        $csprojPath = Join-Path $PSScriptRoot "SendKafkaMessage.csproj"
        if ((Test-Path $programCs) -or (Test-Path $csprojPath)) {
            $useDotNet = $true
            Write-Host "✅ Найден .NET SDK, будет использована C# утилита" -ForegroundColor Green
        }
    }

    # Если .NET не доступен, проверяем Python с kafka-python
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

    # Если ни .NET, ни Python не доступны, проверяем Kafka CLI
    if (-not $useDotNet -and -not $usePython) {
        $kafkaProducer = Get-Command kafka-console-producer -ErrorAction SilentlyContinue
        if ($kafkaProducer) {
            $useKafkaCli = $true
            Write-Host "✅ Найден kafka-console-producer" -ForegroundColor Green
        }
    }
} elseif ($Method -eq "dotnet") {
    $useDotNet = $true
} elseif ($Method -eq "python") {
    $usePython = $true
} elseif ($Method -eq "kafka-cli") {
    $useKafkaCli = $true
}

# Если ничего не найдено
if (-not $useDotNet -and -not $usePython -and -not $useKafkaCli) {
    Write-Host "❌ Не найден ни .NET SDK, ни Python с kafka-python, ни kafka-console-producer" -ForegroundColor Red
    Write-Host ""
    Write-Host "Варианты решения:" -ForegroundColor Yellow
    Write-Host "1. Установите .NET SDK (рекомендуется для этого проекта):" -ForegroundColor Yellow
    Write-Host "   https://dotnet.microsoft.com/download" -ForegroundColor White
    Write-Host ""
    Write-Host "2. Или установите Python и библиотеку kafka-python:" -ForegroundColor Yellow
    Write-Host "   pip install kafka-python" -ForegroundColor White
    Write-Host ""
    Write-Host "3. Или установите Kafka и добавьте kafka-console-producer в PATH" -ForegroundColor Yellow
    exit 1
}

# Генерация тестовых сообщений
$messages = @()
for ($i = 1; $i -le $Count; $i++) {
    $transactionId = [guid]::NewGuid().ToString()
    $portfolioId = [guid]::NewGuid().ToString()
    $stockCardId = [guid]::NewGuid().ToString()
    $transactionTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

    $message = @{
        id = $transactionId
        portfolioId = $portfolioId
        stockCardId = $stockCardId
        assetType = $AssetType
        transactionType = $TransactionType
        quantity = 100
        pricePerUnit = 250.75
        totalAmount = 25075.00
        transactionTime = $transactionTime
        currency = "RUB"
        metadata = $null
    }

    $messages += @{
        Message = $message
        TransactionId = $transactionId
        PortfolioId = $portfolioId
        StockCardId = $stockCardId
    }
}

# Отправка сообщений
if ($useDotNet) {
    # Используем C# утилиту через dotnet run
    Write-Host "Отправка через .NET утилиту..." -ForegroundColor Yellow

    $scriptDir = $PSScriptRoot
    $csprojPath = Join-Path $scriptDir "SendKafkaMessage.csproj"
    $programCs = Join-Path $scriptDir "Program.cs"

    Push-Location $scriptDir
    try {
        # Проверяем наличие Program.cs
        if (-not (Test-Path $programCs)) {
            Write-Host "❌ Ошибка: файл Program.cs не найден в $scriptDir" -ForegroundColor Red
            Write-Host "Убедитесь, что файл SendKafkaMessage.cs был переименован в Program.cs" -ForegroundColor Yellow
            exit 1
        }

        # Проверяем, нужно ли создать проект
        if (-not (Test-Path $csprojPath)) {
            Write-Host "Создание проекта..." -ForegroundColor Gray
            $csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Confluent.Kafka" Version="2.3.0" />
  </ItemGroup>
</Project>
"@
            $csprojContent | Out-File -FilePath $csprojPath -Encoding UTF8
        }

        # Запускаем через dotnet run
        $argsList = @(
            "run",
            "--project", "SendKafkaMessage.csproj",
            "--",
            "--bootstrap-server", $BootstrapServer,
            "--topic", $Topic,
            "--count", $Count.ToString(),
            "--transaction-type", $TransactionType.ToString(),
            "--asset-type", $AssetType.ToString()
        )

        & dotnet $argsList
    } finally {
        Pop-Location
    }
} elseif ($usePython) {
    # Используем Python скрипт
    $scriptPath = Join-Path $PSScriptRoot "send_test_kafka_message.py"
    if (Test-Path $scriptPath) {
        Write-Host "Отправка через Python..." -ForegroundColor Yellow
        python $scriptPath --bootstrap-server $BootstrapServer --topic $Topic --count $Count --transaction-type $TransactionType --asset-type $AssetType
    } else {
        # Встроенная отправка через Python
        Write-Host "Отправка через Python (встроенный метод)..." -ForegroundColor Yellow

        $pythonScript = @"
import json
import sys
from kafka import KafkaProducer
from kafka.errors import KafkaError

bootstrap_servers = "$BootstrapServer"
topic = "$Topic"
messages = $($messages | ConvertTo-Json -Depth 10)

try:
    producer = KafkaProducer(
        bootstrap_servers=bootstrap_servers,
        value_serializer=lambda v: json.dumps(v).encode('utf-8'),
        key_serializer=lambda k: k.encode('utf-8') if k else None
    )

    success_count = 0
    for msg_data in messages:
        message = msg_data['Message']
        key = msg_data['PortfolioId']

        try:
            future = producer.send(topic, key=key, value=message)
            record_metadata = future.get(timeout=10)
            print(f"✅ Сообщение отправлено: TransactionId={message['id']}, Offset={record_metadata.offset}")
            success_count += 1
        except Exception as e:
            print(f"❌ Ошибка отправки: {e}")

    producer.close()
    print(f"\nОтправлено: {success_count}/{len(messages)}")
    sys.exit(0 if success_count == len(messages) else 1)
except Exception as e:
    print(f"❌ Критическая ошибка: {e}")
    sys.exit(1)
"@

        $pythonScript | python
    }
} elseif ($useKafkaCli) {
    # Используем kafka-console-producer
    Write-Host "Отправка через kafka-console-producer..." -ForegroundColor Yellow

    for ($i = 0; $i -lt $messages.Count; $i++) {
        $msg = $messages[$i]
        $messageJson = $msg.Message | ConvertTo-Json -Compress

        Write-Host "[$($i+1)/$Count] Отправка сообщения..." -ForegroundColor Yellow
        Write-Host "   Transaction ID: $($msg.TransactionId)" -ForegroundColor Gray
        Write-Host "   Portfolio ID: $($msg.PortfolioId)" -ForegroundColor Gray
        Write-Host "   Stock Card ID: $($msg.StockCardId)" -ForegroundColor Gray

        try {
            $messageJson | kafka-console-producer --bootstrap-server $BootstrapServer --topic $Topic

            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Сообщение успешно отправлено!" -ForegroundColor Green
            } else {
                Write-Host "   ❌ Ошибка при отправке сообщения (код: $LASTEXITCODE)" -ForegroundColor Red
            }
        } catch {
            Write-Host "   ❌ Ошибка: $_" -ForegroundColor Red
        }

        Write-Host ""

        if ($i -lt ($messages.Count - 1)) {
            Start-Sleep -Milliseconds 500
        }
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Отправка завершена" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 Проверьте логи AnalyticsService для подтверждения обработки" -ForegroundColor Yellow
Write-Host "💡 Проверьте базу данных: SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 10;" -ForegroundColor Yellow

