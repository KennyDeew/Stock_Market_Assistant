# PowerShell скрипт для создания файлов секретов
# Запустите этот скрипт от имени администратора

Write-Host "Создание файлов секретов для AnalyticsService..." -ForegroundColor Green

# Создаем директорию для секретов
$secretsDir = ".\secrets"
if (!(Test-Path $secretsDir)) {
    New-Item -ItemType Directory -Path $secretsDir -Force
    Write-Host "Создана директория: $secretsDir" -ForegroundColor Yellow
}

# Функция для создания файла секрета
function Create-SecretFile {
    param(
        [string]$FileName,
        [string]$Description
    )

    $filePath = Join-Path $secretsDir $FileName
    if (!(Test-Path $filePath)) {
        $securePassword = Read-Host "Введите $Description" -AsSecureString
        $password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))
        $password | Out-File -FilePath $filePath -Encoding UTF8 -NoNewline
        Write-Host "Создан файл секрета: $filePath" -ForegroundColor Green
    } else {
        Write-Host "Файл секрета уже существует: $filePath" -ForegroundColor Yellow
    }
}

# Создаем файлы секретов
Create-SecretFile "analytics_db_password.txt" "пароль для базы данных PostgreSQL"
Create-SecretFile "kafka_sasl_password.txt" "пароль для Kafka SASL"
Create-SecretFile "redis_password.txt" "пароль для Redis"

Write-Host "`nВсе файлы секретов созданы!" -ForegroundColor Green
Write-Host "Теперь вы можете запустить docker-compose up -d" -ForegroundColor Cyan
Write-Host "`nВНИМАНИЕ: Файлы секретов содержат пароли в открытом виде!" -ForegroundColor Red
Write-Host "Убедитесь, что они не попадут в систему контроля версий!" -ForegroundColor Red
Write-Host "Добавьте папку secrets/ в .gitignore" -ForegroundColor Red
