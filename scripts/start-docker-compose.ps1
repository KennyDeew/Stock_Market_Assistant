# Скрипт запуска Docker Compose для Stock Market Assistant
# Проверяет готовность Docker и запускает все сервисы

param(
    [switch]$Build,
    [switch]$Detached,
    [switch]$Force,
    [ValidateSet("DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL")]
    [string]$LogLevel = "ERROR"
)

# Уровни логирования (по возрастанию важности)
$script:LogLevels = @{
    "DEBUG" = 0
    "INFO" = 1
    "WARNING" = 2
    "ERROR" = 3
    "CRITICAL" = 4
}

# Глобальные переменные для логирования
$script:LogFile = $null
$script:ErrorLogFile = $null
$script:CurrentLogLevel = $script:LogLevels[$LogLevel]

# Функция инициализации логирования
function Initialize-Logging {
    try {
        # Определение пути к скрипту (работает в разных контекстах)
        if ($PSScriptRoot) {
            $scriptPath = $PSScriptRoot
        }
        elseif ($MyInvocation.MyCommand.Path) {
            $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
        }
        else {
            $scriptPath = Split-Path -Parent $PSCommandPath
        }

        $projectRoot = Split-Path -Parent $scriptPath
        $logDir = Join-Path $projectRoot "build_log"

        # Создание директории для логов с проверкой
        if (-not (Test-Path $logDir)) {
            $null = New-Item -ItemType Directory -Path $logDir -Force -ErrorAction Stop
        }

        # Проверка, что директория создана
        if (-not (Test-Path $logDir)) {
            throw "Не удалось создать директорию для логов: $logDir"
        }

        # Генерация имени файла лога
        $timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
        $script:LogFile = Join-Path $logDir "log_${timestamp}.txt"
        $script:ErrorLogFile = Join-Path $logDir "error_logs_${timestamp}.txt"

        # Создаем пустые файлы логов перед стартом с проверкой
        $null = New-Item -ItemType File -Path $script:LogFile -Force -ErrorAction Stop
        $null = New-Item -ItemType File -Path $script:ErrorLogFile -Force -ErrorAction Stop

        # Проверка, что файлы созданы
        if (-not (Test-Path $script:LogFile)) {
            throw "Не удалось создать файл лога: $script:LogFile"
        }
        if (-not (Test-Path $script:ErrorLogFile)) {
            throw "Не удалось создать файл ошибок: $script:ErrorLogFile"
        }

        # Запись заголовка в лог
        $header = @"
========================================
  Stock Market Assistant
  Docker Compose Startup Script
  Log Level: $LogLevel
  Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
========================================

"@
        Set-Content -Path $script:LogFile -Value $header -ErrorAction Stop

        # Запись заголовка в файл ошибок
        $errorHeader = @"
========================================
  Stock Market Assistant
  Docker Compose Startup Script
  Error Log
  Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
========================================

"@
        Set-Content -Path $script:ErrorLogFile -Value $errorHeader -ErrorAction Stop

        Write-Host "✓ Файлы логов созданы успешно" -ForegroundColor Green
    }
    catch {
        # Если не удалось инициализировать логирование, продолжаем без файла лога
        $errorMsg = "Не удалось инициализировать файл лога: $($_.Exception.Message)"
        Write-Warning $errorMsg
        Write-Warning "Логирование будет только в консоль."
        $script:LogFile = $null
        $script:ErrorLogFile = $null
    }
}

# Функция логирования
function Write-Log {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL")]
        [string]$Level,

        [Parameter(Mandatory=$true)]
        [string]$Message
    )

    if ($null -eq $script:LogFile) {
        return
    }

    $messageLevel = $script:LogLevels[$Level]

    if ($messageLevel -ge $script:CurrentLogLevel) {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $logEntry = "[$timestamp] [$Level] $Message"
        Add-Content -Path $script:LogFile -Value $logEntry
    }
}

# Инициализация логирования
Initialize-Logging

# Очистка экрана
Clear-Host

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Stock Market Assistant" -ForegroundColor Cyan
Write-Host "  Docker Compose Startup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Лог файл: $script:LogFile" -ForegroundColor Cyan
if ($null -ne $script:ErrorLogFile) {
    Write-Host "Файл ошибок: $script:ErrorLogFile" -ForegroundColor Cyan
}
Write-Host ""
Write-Log "INFO" "Скрипт запущен. Уровень логирования: $LogLevel"

# Функция проверки команды
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Функция проверки Docker
function Test-DockerReady {
    Write-Host "Проверка готовности Docker..." -ForegroundColor Yellow

    # Проверка наличия Docker
    if (-not (Test-Command "docker")) {
        $errorMsg = "Docker не установлен или не найден в PATH"
        Write-Host "❌ ОШИБКА: $errorMsg" -ForegroundColor Red
        Write-Host "   Установите Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
        Write-Log "CRITICAL" $errorMsg
        exit 1
    }

    Write-Host "✓ Docker найден" -ForegroundColor Green
    Write-Log "INFO" "Docker найден в системе"

    # Проверка наличия docker-compose (встроен в Docker)
    Write-Host "✓ docker compose найден" -ForegroundColor Green
    Write-Log "INFO" "docker compose найден"

    # Проверка, что Docker daemon запущен
    try {
        $dockerVersion = docker version --format '{{.Server.Version}}' 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Docker daemon не запущен"
        }
        Write-Host "✓ Docker daemon запущен (версия: $dockerVersion)" -ForegroundColor Green
        Write-Log "INFO" "Docker daemon запущен (версия: $dockerVersion)"
    }
    catch {
        $errorMsg = "Docker daemon не запущен"
        Write-Host "❌ ОШИБКА: $errorMsg" -ForegroundColor Red
        Write-Host "   Запустите Docker Desktop и повторите попытку" -ForegroundColor Yellow
        Write-Log "CRITICAL" $errorMsg
        exit 1
    }

    # Проверка доступности Docker API
    try {
        docker info | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker API недоступен"
        }
        Write-Host "✓ Docker API доступен" -ForegroundColor Green
        Write-Log "INFO" "Docker API доступен"
    }
    catch {
        $errorMsg = "Docker API недоступен"
        Write-Host "❌ ОШИБКА: $errorMsg" -ForegroundColor Red
        Write-Log "CRITICAL" $errorMsg
        exit 1
    }

    Write-Host ""
}

# Функция проверки портов
function Test-PortsAvailable {
    Write-Host "Проверка доступности портов..." -ForegroundColor Yellow

    $ports = @(8080, 8081, 8082, 8083, 8084, 8085, 5273, 6379, 9092, 9200, 5601)
    $occupiedPorts = @()

    foreach ($port in $ports) {
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
        if ($connection) {
            $occupiedPorts += $port
        }
    }

    if ($occupiedPorts.Count -gt 0) {
        $occupiedPortsList = $occupiedPorts -join ", "
        Write-Host "⚠ ПРЕДУПРЕЖДЕНИЕ: Следующие порты заняты:" -ForegroundColor Yellow
        foreach ($port in $occupiedPorts) {
            Write-Host "   - Порт $port" -ForegroundColor Yellow
            Write-Log "WARNING" "Порт $port занят"
        }
        Write-Host ""

        if (-not $Force) {
            $response = Read-Host "Продолжить запуск? (y/N)"
            if ($response -ne "y" -and $response -ne "Y") {
                Write-Host "Запуск отменен" -ForegroundColor Yellow
                Write-Log "INFO" "Запуск отменен пользователем"
                exit 0
            }
        }
        Write-Log "WARNING" "Следующие порты заняты: $occupiedPortsList - продолжение запуска"
    }
    else {
        Write-Host "✓ Все необходимые порты свободны" -ForegroundColor Green
        Write-Log "INFO" "Все необходимые порты свободны"
    }

    Write-Host ""
}

# Функция остановки существующих контейнеров
function Stop-ExistingContainers {
    if ($Force) {
        Write-Host "Остановка существующих контейнеров..." -ForegroundColor Yellow
        Write-Log "INFO" "Остановка существующих контейнеров (режим -Force)"
        docker compose down 2>&1 | Out-Null
        Write-Host "✓ Существующие контейнеры остановлены" -ForegroundColor Green
        Write-Log "INFO" "Существующие контейнеры остановлены"
        Write-Host ""
    }
}

# Функция остановки и удаления Docker контейнеров
function Stop-AndRemoveDockerContainers {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Остановка Docker контейнеров" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    try {
        # Переход в корневую директорию проекта
        $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
        $projectRoot = Split-Path -Parent $scriptPath
        Set-Location $projectRoot

        Write-Host "Остановка контейнеров..." -ForegroundColor Yellow
        Write-Log "INFO" "Остановка Docker контейнеров по запросу пользователя"

        # Останавливаем и удаляем контейнеры
        $stopOutput = docker compose down 2>&1
        $stopSuccess = $LASTEXITCODE -eq 0

        if ($stopSuccess) {
            Write-Host "✓ Контейнеры остановлены и удалены" -ForegroundColor Green
            Write-Log "INFO" "Контейнеры успешно остановлены и удалены"

            # Выводим результат
            foreach ($line in $stopOutput) {
                Write-Host $line -ForegroundColor Gray
            }
        }
        else {
            Write-Host "⚠ Предупреждение: Не удалось полностью остановить контейнеры" -ForegroundColor Yellow
            Write-Log "WARNING" "Не удалось полностью остановить контейнеры"

            foreach ($line in $stopOutput) {
                Write-Host $line -ForegroundColor Yellow
                if ($null -ne $script:LogFile) {
                    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                    Add-Content -Path $script:LogFile -Value "[$timestamp] [WARNING] $line" -ErrorAction SilentlyContinue
                }
            }
        }
    }
    catch {
        $errorMsg = "Ошибка при остановке контейнеров: $($_.Exception.Message)"
        Write-Host "❌ ОШИБКА: $errorMsg" -ForegroundColor Red
        Write-Log "ERROR" $errorMsg
        if ($null -ne $script:LogFile) {
            $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
            Add-Content -Path $script:LogFile -Value "[$timestamp] [ERROR] $errorMsg" -ErrorAction SilentlyContinue
        }
    }

    Write-Host ""
}

# Основная логика
try {
    # Проверка Docker
    Test-DockerReady

    # Проверка портов
    Test-PortsAvailable

    # Остановка существующих контейнеров при необходимости
    Stop-ExistingContainers

    # Определение параметров запуска
    $composeArgs = @()

    if ($Build) {
        $composeArgs += "--build"
        Write-Host "Режим: Пересборка образов" -ForegroundColor Cyan
        Write-Log "INFO" "Режим: Пересборка образов"
    }

    if ($Detached) {
        $composeArgs += "-d"
        Write-Host "Режим: Запуск в фоновом режиме" -ForegroundColor Cyan
        Write-Log "INFO" "Режим: Запуск в фоновом режиме"
    }
    else {
        Write-Host "Режим: Запуск в интерактивном режиме" -ForegroundColor Cyan
        Write-Log "INFO" "Режим: Запуск в интерактивном режиме"
    }

    Write-Host ""
    Write-Host "Запуск Docker Compose..." -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    $composeArgsString = $composeArgs -join " "
    Write-Log "INFO" "Запуск Docker Compose с параметрами: $composeArgsString"

    # Переход в корневую директорию проекта
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    $projectRoot = Split-Path -Parent $scriptPath
    Set-Location $projectRoot

    # Запуск docker compose
    $composeArgsString = $composeArgs -join " "
    Write-Log "INFO" "Выполнение команды: docker compose up $composeArgsString"

    # Файл для записи ошибок
    $errorLogFile = $script:ErrorLogFile

    try {
        # Используем скрипт-область для хранения ошибок
        $script:errors = @()

        # Функция для обработки строки в реальном времени
        function Write-OutputLine {
            param([string]$line)

            if ([string]::IsNullOrWhiteSpace($line)) {
                return
            }

            # Определяем, является ли строка ошибкой
            # Исключаем ложные срабатывания
            $falsePositives = @(
                "Executed DbCommand",           # Это не ошибка, а информационное сообщение
                "0 Error\(s\)",                 # Сообщение о количестве ошибок сборки (0 ошибок)
                "Connection refused.*kafka",    # Временная ошибка при старте Kafka
                "brokers are down",             # Временная ошибка при старте Kafka
                "Coordinator load in progress",  # Нормальное поведение при старте Kafka
                "retrying",                     # Повторные попытки - нормальное поведение
                "INFO.*JVM arguments",          # Информационные сообщения OpenSearch
                "INF\]",                        # Информационные сообщения (INF уровень)
                "WRN\]",                        # Предупреждения (WRN уровень)
                "handshake timed out.*kafka-ui" # SSL handshake timeout в kafka-ui (не критично)
            )

            $isFalsePositive = $falsePositives | Where-Object { $line -match $_ }

            $isError = -not $isFalsePositive -and (
                $line -is [System.Management.Automation.ErrorRecord] -or
                ($line -match "error|ERROR|Error|failed|FAILED|Failed|exception|EXCEPTION|Exception|timeout|TIMEOUT|Timeout" -and
                 $line -notmatch "retrying|retry|INFO|INF\]|WRN\]|Executed DbCommand|relation.*does not exist|Coordinator load|brokers are down|Connection refused.*kafka")
            )

            if ($isError) {
                $script:errors += $line
                # Выводим ошибку сразу с красным цветом
                Write-Host $line -ForegroundColor Red
                if ($null -ne $errorLogFile) {
                    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                    Add-Content -Path $errorLogFile -Value "[$timestamp] [ERROR] $line" -ErrorAction SilentlyContinue
                }
                # Также записываем в общий лог
                if ($null -ne $script:LogFile) {
                    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                    Add-Content -Path $script:LogFile -Value "[$timestamp] [ERROR] $line" -ErrorAction SilentlyContinue
                }
            }
            else {
                # Обычный вывод - показываем сразу
                Write-Host $line
                # Записываем только в общий лог, не в файл ошибок
                if ($null -ne $script:LogFile) {
                    Add-Content -Path $script:LogFile -Value $line -ErrorAction SilentlyContinue
                }
            }
        }

        # Запускаем docker compose с обработкой вывода в реальном времени
        if ($composeArgs.Count -gt 0) {
            docker compose up $composeArgs 2>&1 | ForEach-Object {
                Write-OutputLine $_
            }
        }
        else {
            docker compose up 2>&1 | ForEach-Object {
                Write-OutputLine $_
            }
        }

        # Если были ошибки, выводим сводку
        if ($script:errors.Count -gt 0) {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host "ОБНАРУЖЕНО ОШИБОК: $($script:errors.Count)" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
            if ($null -ne $errorLogFile) {
                Write-Host "Ошибки записаны в файл: $errorLogFile" -ForegroundColor Yellow
            }
            else {
                Write-Host "Внимание: Файл для записи ошибок не был создан" -ForegroundColor Yellow
            }
            Write-Host ""
        }

        if ($LASTEXITCODE -eq 0) {
            Write-Log "INFO" "Docker Compose завершен успешно"
        }
        else {
            $errorMsg = "Docker Compose завершен с ошибкой (код: $LASTEXITCODE)"
            Write-Log "ERROR" $errorMsg
            if ($null -ne $errorLogFile) {
                $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
                Add-Content -Path $errorLogFile -Value "[$timestamp] [ERROR] $errorMsg" -ErrorAction SilentlyContinue
            }
        }
    }
    catch {
        $errorMsg = "Ошибка при выполнении Docker Compose: $($_.Exception.Message)"
        Write-Log "ERROR" $errorMsg
        if ($null -ne $errorLogFile) {
            $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
            Add-Content -Path $errorLogFile -Value "[$timestamp] [ERROR] $errorMsg" -ErrorAction SilentlyContinue
            Add-Content -Path $errorLogFile -Value "[$timestamp] [ERROR] StackTrace: $($_.Exception.StackTrace)" -ErrorAction SilentlyContinue
        }
        throw
    }
}
catch {
    $errorMsg = $_.Exception.Message
    $errorStackTrace = $_.Exception.StackTrace

    Write-Host ""
    Write-Host "❌ КРИТИЧЕСКАЯ ОШИБКА:" -ForegroundColor Red
    Write-Host $errorMsg -ForegroundColor Red

    # Записываем ошибку в файл лога, если он доступен
    if ($null -ne $script:LogFile) {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Add-Content -Path $script:LogFile -Value "[$timestamp] [CRITICAL] Критическая ошибка в скрипте: $errorMsg" -ErrorAction SilentlyContinue
        if ($errorStackTrace) {
            Add-Content -Path $script:LogFile -Value "[$timestamp] [CRITICAL] StackTrace: $errorStackTrace" -ErrorAction SilentlyContinue
        }
        Write-Host ""
        Write-Host "Ошибка записана в лог: $script:LogFile" -ForegroundColor Yellow
    }
    else {
        Write-Log "CRITICAL" "Критическая ошибка в скрипте: $errorMsg"
    }

    # Спрашиваем перед выходом с ошибкой
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    $response = Read-Host "Остановить и удалить Docker контейнеры? (Д/Да/1/Y/y для подтверждения)"

    $positiveResponses = @("Д", "Да", "да", "1", "Y", "y", "yes", "Yes", "YES")
    if ($positiveResponses -contains $response) {
        Stop-AndRemoveDockerContainers
    }
    else {
        Write-Host "Контейнеры не остановлены" -ForegroundColor Gray
        Write-Log "INFO" "Пользователь отказался от остановки контейнеров"
    }

    exit 1
}

# Спрашиваем перед завершением работы (успешное или прерванное)
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
$response = Read-Host "Остановить и удалить Docker контейнеры? (Д/Да/1/Y/y для подтверждения)"

$positiveResponses = @("Д", "Да", "да", "1", "Y", "y", "yes", "Yes", "YES")
if ($positiveResponses -contains $response) {
    Stop-AndRemoveDockerContainers
}
else {
    Write-Host "Контейнеры не остановлены" -ForegroundColor Gray
    Write-Log "INFO" "Пользователь отказался от остановки контейнеров"
}

Write-Host ""
Write-Host "Скрипт завершен" -ForegroundColor Green
Write-Log "INFO" "Скрипт завершен"

