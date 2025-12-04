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
$script:CurrentLogLevel = $script:LogLevels[$LogLevel]

# Функция инициализации логирования
function Initialize-Logging {
    try {
        # Создание директории для логов
        $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
        $projectRoot = Split-Path -Parent $scriptPath
        $logDir = Join-Path $projectRoot "build_log"

        if (-not (Test-Path $logDir)) {
            New-Item -ItemType Directory -Path $logDir -Force | Out-Null
        }

        # Генерация имени файла лога
        $timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
        $script:LogFile = Join-Path $logDir "log_${timestamp}.txt"

        # Запись заголовка в лог
        $header = @"
========================================
  Stock Market Assistant
  Docker Compose Startup Script
  Log Level: $LogLevel
  Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
========================================

"@
        Add-Content -Path $script:LogFile -Value $header -ErrorAction SilentlyContinue
    }
    catch {
        # Если не удалось инициализировать логирование, продолжаем без файла лога
        Write-Warning "Не удалось инициализировать файл лога: $($_.Exception.Message). Логирование будет только в консоль."
        $script:LogFile = $null
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

    # Проверка наличия docker-compose
    $dockerComposeCmd = "docker compose"
    if (-not (Test-Command "docker")) {
        $errorMsg = "docker compose не доступен"
        Write-Host "❌ ОШИБКА: $errorMsg" -ForegroundColor Red
        Write-Log "CRITICAL" $errorMsg
        exit 1
    }

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

    try {
        if ($composeArgs.Count -gt 0) {
            if ($null -ne $script:LogFile) {
                docker compose up $composeArgs 2>&1 | Tee-Object -FilePath $script:LogFile -Append
            }
            else {
                docker compose up $composeArgs
            }
        }
        else {
            if ($null -ne $script:LogFile) {
                docker compose up 2>&1 | Tee-Object -FilePath $script:LogFile -Append
            }
            else {
                docker compose up
            }
        }

        if ($LASTEXITCODE -eq 0) {
            Write-Log "INFO" "Docker Compose завершен успешно"
        }
        else {
            Write-Log "ERROR" "Docker Compose завершен с ошибкой (код: $LASTEXITCODE)"
        }
    }
    catch {
        Write-Log "ERROR" "Ошибка при выполнении Docker Compose: $($_.Exception.Message)"
        throw
    }
}
catch {
    Write-Host ""
    Write-Host "❌ КРИТИЧЕСКАЯ ОШИБКА:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Log "CRITICAL" "Критическая ошибка в скрипте: $($_.Exception.Message)"
    exit 1
}

