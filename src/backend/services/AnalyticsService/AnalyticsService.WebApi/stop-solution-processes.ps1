# Скрипт для остановки всех процессов, связанных с проектами решения StockMarketAssistant

$ErrorActionPreference = "SilentlyContinue"

$processNames = @(
    "Gateway.WebApi",
    "AuthService.WebApi",
    "StockCardService.WebApi",
    "PortfolioService.WebApi",
    "AnalyticsService.WebApi",
    "NotificationService.WebApi",
    "StockMarketAssistant.AppHost"
)

$solutionPath = "Stock_Market_Assistant"
$stoppedCount = 0

Write-Host "Остановка процессов решения StockMarketAssistant..." -ForegroundColor Yellow

# Останавливаем процессы по именам exe файлов
foreach ($processName in $processNames) {
    $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            try {
                $processPath = $process.Path
                if ($processPath -and $processPath -like "*$solutionPath*") {
                    Write-Host "Остановка процесса: $($process.Name) (PID: $($process.Id))" -ForegroundColor Cyan
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                    Start-Sleep -Milliseconds 100
                    $stoppedCount++
                }
            }
            catch {
                # Игнорируем ошибки
            }
        }
    }
}

# Останавливаем процессы dotnet, которые могут запускать проекты решения
try {
    $allDotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    if ($allDotnetProcesses) {
        foreach ($process in $allDotnetProcesses) {
            try {
                # Получаем командную строку процесса через WMI
                $wmiProcess = Get-WmiObject Win32_Process -Filter "ProcessId = $($process.Id)" -ErrorAction SilentlyContinue
                if ($wmiProcess) {
                    $commandLine = $wmiProcess.CommandLine
                    if ($commandLine -and (
                        $commandLine -like "*$solutionPath*" -or
                        $commandLine -like "*Gateway.WebApi*" -or
                        $commandLine -like "*AuthService.WebApi*" -or
                        $commandLine -like "*StockCardService.WebApi*" -or
                        $commandLine -like "*PortfolioService.WebApi*" -or
                        $commandLine -like "*AnalyticsService.WebApi*" -or
                        $commandLine -like "*NotificationService.WebApi*" -or
                        $commandLine -like "*AppHost*"
                    )) {
                        Write-Host "Остановка процесса: dotnet (PID: $($process.Id))" -ForegroundColor Cyan
                        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                        Start-Sleep -Milliseconds 100
                        $stoppedCount++
                    }
                }
            }
            catch {
                # Игнорируем ошибки
            }
        }
    }
}
catch {
    # Игнорируем ошибки
}

# Даем время процессам завершиться
if ($stoppedCount -gt 0) {
    Start-Sleep -Milliseconds 500
    Write-Host "Остановлено процессов: $stoppedCount" -ForegroundColor Green
}

Write-Host "Готово." -ForegroundColor Green

