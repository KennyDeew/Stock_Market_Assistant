# Скрипт проверки состояния Docker Desktop
# Использование: .\scripts\check_docker_health.ps1

Write-Host "🔍 Проверка состояния Docker Desktop..." -ForegroundColor Cyan
Write-Host ""

# Проверка 1: Docker CLI доступен
Write-Host "1. Проверка Docker CLI..." -ForegroundColor Yellow
$dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCmd) {
    Write-Host "   ❌ Docker CLI не найден в PATH" -ForegroundColor Red
    Write-Host "   Установите Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host "   ✅ Docker CLI найден: $($dockerCmd.Source)" -ForegroundColor Green

# Проверка 2: Docker Desktop процесс
Write-Host ""
Write-Host "2. Проверка процесса Docker Desktop..." -ForegroundColor Yellow
$dockerProcess = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue
if (-not $dockerProcess) {
    Write-Host "   ❌ Docker Desktop не запущен" -ForegroundColor Red
    Write-Host "   Запустите Docker Desktop и подождите полной инициализации (30-60 секунд)" -ForegroundColor Yellow
    exit 1
}
Write-Host "   ✅ Docker Desktop запущен (PID: $($dockerProcess.Id))" -ForegroundColor Green

# Проверка 3: Docker daemon доступен
Write-Host ""
Write-Host "3. Проверка Docker daemon..." -ForegroundColor Yellow
$dockerVersion = docker version --format "{{.Server.Version}}" 2>&1
if ($LASTEXITCODE -ne 0 -or $dockerVersion -match "Error|500") {
    Write-Host "   ❌ Docker daemon недоступен" -ForegroundColor Red
    Write-Host "   Ошибка: $dockerVersion" -ForegroundColor Red
    Write-Host ""
    Write-Host "   💡 Решения:" -ForegroundColor Cyan
    Write-Host "   1. Перезапустите Docker Desktop:" -ForegroundColor Yellow
    Write-Host "      - Закройте Docker Desktop полностью (через системный трей)" -ForegroundColor Gray
    Write-Host "      - Подождите 10 секунд" -ForegroundColor Gray
    Write-Host "      - Запустите Docker Desktop снова" -ForegroundColor Gray
    Write-Host "      - Дождитесь зеленого индикатора (30-60 секунд)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   2. Проверьте, что WSL 2 установлен и обновлен:" -ForegroundColor Yellow
    Write-Host "      - wsl --update" -ForegroundColor Gray
    Write-Host "      - wsl --set-default-version 2" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   3. Если проблема сохраняется:" -ForegroundColor Yellow
    Write-Host "      - Перезагрузите компьютер" -ForegroundColor Gray
    Write-Host "      - Проверьте логи Docker Desktop: Settings → Troubleshoot → View logs" -ForegroundColor Gray
    exit 1
}
Write-Host "   ✅ Docker daemon доступен (версия: $dockerVersion)" -ForegroundColor Green

# Проверка 4: Docker info
Write-Host ""
Write-Host "4. Проверка информации Docker..." -ForegroundColor Yellow
$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -ne 0 -or $dockerInfo -match "Error|500") {
    Write-Host "   ⚠️  Не удалось получить информацию Docker" -ForegroundColor Yellow
    Write-Host "   Ошибка: $($dockerInfo | Select-Object -First 3)" -ForegroundColor Gray
} else {
    Write-Host "   ✅ Docker info получен успешно" -ForegroundColor Green
}

# Проверка 5: Контейнеры
Write-Host ""
Write-Host "5. Проверка контейнеров..." -ForegroundColor Yellow
$containers = docker ps -a 2>&1
if ($LASTEXITCODE -ne 0 -or $containers -match "Error|500") {
    Write-Host "   ⚠️  Не удалось получить список контейнеров" -ForegroundColor Yellow
} else {
    $runningCount = (docker ps --format "{{.Names}}" 2>&1 | Where-Object { $_ -notmatch "Error" }).Count
    $totalCount = (docker ps -a --format "{{.Names}}" 2>&1 | Where-Object { $_ -notmatch "Error" }).Count
    Write-Host "   ✅ Контейнеры: $runningCount запущено из $totalCount всего" -ForegroundColor Green
}

# Проверка 6: Kubernetes (если включен)
Write-Host ""
Write-Host "6. Проверка Kubernetes..." -ForegroundColor Yellow
$kubectl = Get-Command kubectl -ErrorAction SilentlyContinue
if ($kubectl) {
    $k8sContext = kubectl config current-context 2>&1
    if ($k8sContext -match "docker-desktop") {
        Write-Host "   ✅ Kubernetes включен (контекст: $k8sContext)" -ForegroundColor Green

        $k8sNodes = kubectl get nodes 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Kubernetes кластер доступен" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Kubernetes кластер не готов" -ForegroundColor Yellow
            Write-Host "   Подождите еще немного или перезапустите Docker Desktop" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ℹ️  Kubernetes не используется (контекст: $k8sContext)" -ForegroundColor Gray
    }
} else {
    Write-Host "   ℹ️  kubectl не найден (Kubernetes может быть не включен)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "✅ Проверка завершена" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Если все проверки пройдены, но Aspire все еще не работает:" -ForegroundColor Cyan
Write-Host "   - Подождите еще 30-60 секунд для полной инициализации" -ForegroundColor Yellow
Write-Host "   - Перезапустите AppHost" -ForegroundColor Yellow
Write-Host "   - Проверьте логи Aspire в Visual Studio Output" -ForegroundColor Yellow

