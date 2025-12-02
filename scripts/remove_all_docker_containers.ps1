# Скрипт удаления всех контейнеров Docker
# Использование: .\scripts\remove_all_docker_containers.ps1 [-Force]

param(
    [switch]$Force = $false
)

Write-Host "🗑️  Удаление всех контейнеров Docker..." -ForegroundColor Cyan
Write-Host ""

# Проверка доступности Docker
$dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCmd) {
    Write-Host "❌ Docker CLI не найден в PATH" -ForegroundColor Red
    Write-Host "   Установите Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Получаем список всех контейнеров (включая остановленные)
Write-Host "📋 Получение списка контейнеров..." -ForegroundColor Yellow
$containers = docker ps -a --format "{{.ID}}|{{.Names}}|{{.Status}}" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Ошибка при получении списка контейнеров" -ForegroundColor Red
    Write-Host "   Ошибка: $containers" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Убедитесь, что Docker Desktop запущен и работает" -ForegroundColor Yellow
    exit 1
}

# Парсим список контейнеров
$containerList = @()
foreach ($line in $containers) {
    if ($line -match "^([a-f0-9]+)\|(.+)\|(.+)$") {
        $containerList += @{
            Id = $matches[1]
            Name = $matches[2]
            Status = $matches[3]
        }
    }
}

if ($containerList.Count -eq 0) {
    Write-Host "✅ Контейнеры не найдены" -ForegroundColor Green
    exit 0
}

Write-Host "   Найдено контейнеров: $($containerList.Count)" -ForegroundColor Gray
Write-Host ""

# Показываем список контейнеров
Write-Host "📦 Список контейнеров для удаления:" -ForegroundColor Yellow
foreach ($container in $containerList) {
    $statusColor = if ($container.Status -match "Up") { "Green" } else { "Gray" }
    Write-Host "   - $($container.Name) ($($container.Id.Substring(0, 12))) [$($container.Status)]" -ForegroundColor $statusColor
}
Write-Host ""

# Запрашиваем подтверждение, если не указан флаг -Force
if (-not $Force) {
    $confirmation = Read-Host "⚠️  Вы уверены, что хотите удалить все контейнеры? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "❌ Операция отменена" -ForegroundColor Yellow
        exit 0
    }
}

# Останавливаем все запущенные контейнеры
Write-Host ""
Write-Host "🛑 Остановка запущенных контейнеров..." -ForegroundColor Yellow
$runningContainers = docker ps -q 2>&1
if ($LASTEXITCODE -eq 0 -and $runningContainers) {
    foreach ($containerId in $runningContainers) {
        if ($containerId -match "^[a-f0-9]+$") {
            Write-Host "   Остановка контейнера: $($containerId.Substring(0, 12))..." -ForegroundColor Gray
            docker stop $containerId 2>&1 | Out-Null
        }
    }
    Write-Host "   ✅ Все запущенные контейнеры остановлены" -ForegroundColor Green
} else {
    Write-Host "   ℹ️  Запущенных контейнеров не найдено" -ForegroundColor Gray
}

# Удаляем все контейнеры
Write-Host ""
Write-Host "🗑️  Удаление контейнеров..." -ForegroundColor Yellow
$removedCount = 0
$errorCount = 0

foreach ($container in $containerList) {
    Write-Host "   Удаление: $($container.Name) ($($container.Id.Substring(0, 12)))..." -ForegroundColor Gray -NoNewline
    $result = docker rm -f $container.Id 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " ✅" -ForegroundColor Green
        $removedCount++
    } else {
        Write-Host " ❌" -ForegroundColor Red
        Write-Host "      Ошибка: $result" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "📊 Результат:" -ForegroundColor Cyan
Write-Host "   ✅ Успешно удалено: $removedCount" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "   ❌ Ошибок: $errorCount" -ForegroundColor Red
}

Write-Host ""
Write-Host "✅ Операция завершена" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Дополнительные команды для очистки Docker:" -ForegroundColor Cyan
Write-Host "   - docker system prune -a          # Удалить все неиспользуемые образы, контейнеры, сети" -ForegroundColor Gray
Write-Host "   - docker volume prune              # Удалить все неиспользуемые volumes" -ForegroundColor Gray
Write-Host "   - docker network prune            # Удалить все неиспользуемые сети" -ForegroundColor Gray

