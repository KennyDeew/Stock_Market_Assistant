# Скрипт для просмотра логов ошибок из Docker контейнеров

param(
    [string]$ContainerName = "",
    [switch]$SaveToFile
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Просмотр логов ошибок" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Получаем список контейнеров с ошибками
$errorContainers = docker ps -a --filter "status=exited" --format "{{.Names}}" 2>$null

if ($null -eq $errorContainers -or $errorContainers.Count -eq 0) {
    Write-Host "✅ Нет контейнеров с ошибками" -ForegroundColor Green
    exit 0
}

Write-Host "Контейнеры с ошибками:" -ForegroundColor Yellow
$errorContainers | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
Write-Host ""

# Если указан конкретный контейнер
if ($ContainerName) {
    $containers = @($ContainerName)
}
else {
    $containers = $errorContainers
}

# Создаем временный файл для логов, если нужно сохранить
$logFile = $null
if ($SaveToFile) {
    $logDir = Join-Path $PSScriptRoot ".." "build_log"
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    $timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
    $logFile = Join-Path $logDir "error_logs_${timestamp}.txt"
    Write-Host "📄 Логи будут сохранены в: $logFile" -ForegroundColor Cyan
    Write-Host ""
}

# Функция для записи в файл и консоль
function Write-LogOutput {
    param([string]$Message, [string]$Color = "White")

    if ($logFile) {
        Add-Content -Path $logFile -Value $Message
    }

    Write-Host $Message -ForegroundColor $Color
}

# Просматриваем логи каждого контейнера
foreach ($container in $containers) {
    Write-LogOutput "========================================" "Cyan"
    Write-LogOutput "Контейнер: $container" "Yellow"
    Write-LogOutput "========================================" "Cyan"
    Write-LogOutput ""

    # Получаем последние 100 строк логов
    $logs = docker logs $container --tail 100 2>&1

    if ($LASTEXITCODE -eq 0) {
        # Ищем ошибки в логах
        $errorLines = $logs | Select-String -Pattern "error|exception|failed|fatal" -CaseSensitive:$false

        if ($errorLines) {
            Write-LogOutput "Найдено ошибок: $($errorLines.Count)" "Red"
            Write-LogOutput ""
            Write-LogOutput "Последние ошибки:" "Yellow"
            $errorLines | Select-Object -Last 20 | ForEach-Object {
                Write-LogOutput $_.Line "Red"
            }
        }
        else {
            Write-LogOutput "Ошибки не найдены в последних 100 строках" "Gray"
        }

        Write-LogOutput ""
        Write-LogOutput "Последние 30 строк логов:" "Cyan"
        $logs | Select-Object -Last 30 | ForEach-Object {
            Write-LogOutput $_ "White"
        }
    }
    else {
        Write-LogOutput "❌ Не удалось получить логи для контейнера $container" "Red"
    }

    Write-LogOutput ""
    Write-LogOutput ""
}

# Открываем файл, если он был создан
if ($logFile -and (Test-Path $logFile)) {
    Write-Host "📂 Открываю файл с логами..." -ForegroundColor Green
    code $logFile
}

