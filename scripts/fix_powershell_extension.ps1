# Скрипт для исправления проблем с PowerShell Extension

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Исправление PowerShell Extension" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Проверка процессов PowerShell
Write-Host "1. Проверка процессов PowerShell..." -ForegroundColor Yellow
$pwshProcesses = Get-Process -Name "pwsh","powershell" -ErrorAction SilentlyContinue
if ($pwshProcesses) {
    Write-Host "   Найдено процессов PowerShell: $($pwshProcesses.Count)" -ForegroundColor Gray
    Write-Host "   ⚠ Много процессов PowerShell может вызывать проблемы" -ForegroundColor Yellow
}
else {
    Write-Host "   ✓ Процессы PowerShell не найдены" -ForegroundColor Green
}

# Очистка старых PSES процессов
Write-Host ""
Write-Host "2. Поиск процессов PSES..." -ForegroundColor Yellow
$psesProcesses = Get-Process | Where-Object { $_.ProcessName -like "*pses*" -or $_.MainWindowTitle -like "*PowerShell*Editor*" }
if ($psesProcesses) {
    Write-Host "   Найдено процессов PSES: $($psesProcesses.Count)" -ForegroundColor Gray
    Write-Host "   💡 Рекомендуется перезапустить Cursor/VS Code для очистки PSES" -ForegroundColor Cyan
}
else {
    Write-Host "   ✓ Процессы PSES не найдены" -ForegroundColor Green
}

# Проверка named pipes
Write-Host ""
Write-Host "3. Проверка named pipes PSES..." -ForegroundColor Yellow
$pipes = [System.IO.Directory]::GetFiles("\\.\pipe\") | Where-Object { $_ -like "*PSES*" }
if ($pipes) {
    Write-Host "   Найдено named pipes PSES: $($pipes.Count)" -ForegroundColor Gray
    $pipes | ForEach-Object { Write-Host "     - $_" -ForegroundColor Gray }
}
else {
    Write-Host "   ✓ Named pipes PSES не найдены" -ForegroundColor Green
}

# Рекомендации
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Рекомендации" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Для исправления ошибки PSES:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Перезапустите Cursor/VS Code:" -ForegroundColor Cyan
Write-Host "   - Закройте все окна Cursor/VS Code" -ForegroundColor Gray
Write-Host "   - Подождите 5-10 секунд" -ForegroundColor Gray
Write-Host "   - Запустите Cursor/VS Code снова" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Перезагрузите PowerShell Extension:" -ForegroundColor Cyan
Write-Host "   - Нажмите Ctrl+Shift+P" -ForegroundColor Gray
Write-Host "   - Выполните: 'PowerShell: Restart Current Session'" -ForegroundColor Gray
Write-Host "   - Или: 'Developer: Reload Window'" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Проверьте настройки PowerShell Extension:" -ForegroundColor Cyan
Write-Host "   - Откройте Settings (Ctrl+,)" -ForegroundColor Gray
Write-Host "   - Найдите 'powershell'" -ForegroundColor Gray
Write-Host "   - Проверьте 'PowerShell: Use Legacy Console'" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Альтернатива - используйте tasks.json вместо launch.json:" -ForegroundColor Cyan
Write-Host "   - Задачи в tasks.json используют 'shell' тип" -ForegroundColor Gray
Write-Host "   - Они не требуют PowerShell Extension для запуска" -ForegroundColor Gray
Write-Host ""

