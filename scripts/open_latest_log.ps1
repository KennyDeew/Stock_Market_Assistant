# Скрипт для открытия последнего лог файла из build_log

$logDir = Join-Path $PSScriptRoot ".." "build_log"

if (-not (Test-Path $logDir)) {
    Write-Host "❌ Директория build_log не найдена" -ForegroundColor Red
    Write-Host "   Логи будут созданы при запуске скрипта start-docker-compose.ps1" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Для просмотра логов Docker контейнеров используйте:" -ForegroundColor Cyan
    Write-Host "   docker logs <container_name>" -ForegroundColor Gray
    exit 1
}

$latestLog = Get-ChildItem -Path $logDir -Filter "*.txt" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $latestLog) {
    Write-Host "❌ Лог файлы не найдены в $logDir" -ForegroundColor Red
    exit 1
}

Write-Host "📄 Открываю лог файл: $($latestLog.Name)" -ForegroundColor Green
Write-Host "   Путь: $($latestLog.FullName)" -ForegroundColor Gray
Write-Host "   Размер: $([math]::Round($latestLog.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "   Изменен: $($latestLog.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

# Открываем файл в VS Code/Cursor
code $latestLog.FullName

