# Скрипт для исправления проблем с Docker build

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Исправление проблем Docker Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Проверка Docker
Write-Host "1. Проверка Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>&1
    Write-Host "   ✓ Docker установлен: $dockerVersion" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ Docker не найден" -ForegroundColor Red
    exit 1
}

# Проверка Docker daemon
Write-Host ""
Write-Host "2. Проверка Docker daemon..." -ForegroundColor Yellow
try {
    docker info 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Docker daemon работает" -ForegroundColor Green
    }
    else {
        Write-Host "   ✗ Docker daemon не отвечает" -ForegroundColor Red
        Write-Host ""
        Write-Host "   💡 Решения:" -ForegroundColor Cyan
        Write-Host "      1. Перезапустите Docker Desktop" -ForegroundColor Gray
        Write-Host "      2. Проверьте, что Docker Desktop запущен" -ForegroundColor Gray
        Write-Host "      3. Попробуйте: docker system prune -a" -ForegroundColor Gray
        exit 1
    }
}
catch {
    Write-Host "   ✗ Ошибка при проверке Docker daemon" -ForegroundColor Red
    Write-Host "      $($_.Exception.Message)" -ForegroundColor Gray
    exit 1
}

# Очистка кэша сборки
Write-Host ""
Write-Host "3. Очистка кэша сборки..." -ForegroundColor Yellow
Write-Host "   Выполняется: docker builder prune -f" -ForegroundColor Gray
docker builder prune -f 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Кэш сборки очищен" -ForegroundColor Green
}
else {
    Write-Host "   ⚠ Не удалось очистить кэш (может быть не критично)" -ForegroundColor Yellow
}

# Проверка свободного места
Write-Host ""
Write-Host "4. Проверка использования диска Docker..." -ForegroundColor Yellow
$dockerDiskUsage = docker system df 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host $dockerDiskUsage -ForegroundColor Gray
}
else {
    Write-Host "   ⚠ Не удалось получить информацию о диске" -ForegroundColor Yellow
}

# Рекомендации
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Рекомендации" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Если сборка все еще не работает:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Перезапустите Docker Desktop:" -ForegroundColor Cyan
Write-Host "   - Закройте Docker Desktop" -ForegroundColor Gray
Write-Host "   - Подождите 10 секунд" -ForegroundColor Gray
Write-Host "   - Запустите Docker Desktop снова" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Очистите все неиспользуемые ресурсы:" -ForegroundColor Cyan
Write-Host "   docker system prune -a --volumes" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Пересоберите только проблемный сервис:" -ForegroundColor Cyan
Write-Host "   docker compose build --no-cache portfolioservice-api" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Проверьте логи Docker Desktop:" -ForegroundColor Cyan
Write-Host "   - Откройте Docker Desktop" -ForegroundColor Gray
Write-Host "   - Перейдите в Settings > Troubleshoot" -ForegroundColor Gray
Write-Host "   - Проверьте логи" -ForegroundColor Gray
Write-Host ""

