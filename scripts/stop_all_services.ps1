# Скрипт для остановки всех запущенных сервисов проекта

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Остановка всех сервисов проекта" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Список процессов для остановки
$processNames = @(
    "AnalyticsService.WebApi",
    "Gateway.WebApi",
    "PortfolioService.WebApi",
    "StockCardService.WebApi",
    "AuthService.WebApi",
    "NotificationService",
    "StockMarketAssistant.AppHost"
)

$stoppedCount = 0
$notFoundCount = 0

foreach ($processName in $processNames) {
    $processes = Get-Process | Where-Object {
        $_.ProcessName -like "*$processName*" -or
        $_.MainWindowTitle -like "*$processName*" -or
        $_.CommandLine -like "*$processName*" -ErrorAction SilentlyContinue
    }

    if ($processes) {
        foreach ($process in $processes) {
            Write-Host "Остановка процесса: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Yellow
            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
                Write-Host "  ✅ Процесс остановлен" -ForegroundColor Green
                $stoppedCount++
            } catch {
                Write-Host "  ❌ Ошибка при остановке: $_" -ForegroundColor Red
            }
        }
    } else {
        $notFoundCount++
    }
}

# Дополнительная проверка по портам (если процессы все еще запущены)
Write-Host ""
Write-Host "Проверка процессов по портам..." -ForegroundColor Yellow

$commonPorts = @(5000, 5001, 5002, 5003, 5004, 5005, 7000, 7001, 7002, 7003, 7004, 7005, 7270, 8080, 8081, 8082, 8083)

foreach ($port in $commonPorts) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "Найден процесс на порту $port : $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Yellow
            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
                Write-Host "  ✅ Процесс остановлен" -ForegroundColor Green
                $stoppedCount++
            } catch {
                Write-Host "  ❌ Ошибка при остановке: $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Результат:" -ForegroundColor Cyan
Write-Host "  Остановлено процессов: $stoppedCount" -ForegroundColor Green
Write-Host "  Не найдено процессов: $notFoundCount" -ForegroundColor Gray
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Проверка оставшихся процессов
Write-Host "Проверка оставшихся процессов..." -ForegroundColor Yellow
$remaining = Get-Process | Where-Object {
    $_.ProcessName -like "*WebApi*" -or
    $_.ProcessName -like "*Service*" -or
    $_.ProcessName -like "*AppHost*"
} | Where-Object {
    $_.Path -like "*Stock_Market_Assistant*"
}

if ($remaining) {
    Write-Host "⚠️ Найдены оставшиеся процессы:" -ForegroundColor Yellow
    $remaining | ForEach-Object {
        Write-Host "  - $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
    }
    Write-Host ""
    $answer = Read-Host "Остановить оставшиеся процессы? (Y/N)"
    if ($answer -eq "Y" -or $answer -eq "y") {
        $remaining | ForEach-Object {
            try {
                Stop-Process -Id $_.Id -Force
                Write-Host "  ✅ Остановлен: $($_.ProcessName)" -ForegroundColor Green
            } catch {
                Write-Host "  ❌ Ошибка: $($_.ProcessName)" -ForegroundColor Red
            }
        }
    }
} else {
    Write-Host "✅ Все процессы остановлены" -ForegroundColor Green
}

Write-Host ""
Write-Host "💡 Теперь можно пересобрать проект в Visual Studio" -ForegroundColor Cyan

