# Вспомогательный скрипт для обновления docker-compose.yml на основе appsettings.json
# Этот скрипт вызывается из start_analytics_service.ps1

param(
    [string]$AppSettingsPath,
    [string]$DockerComposePath
)

if (-not (Test-Path $AppSettingsPath)) {
    Write-Error "Файл appsettings.json не найден: $AppSettingsPath"
    exit 1
}

try {
    $appsettings = Get-Content $AppSettingsPath -Raw | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.'analytics-db'

    if (-not $connectionString) {
        Write-Warning "Строка подключения 'analytics-db' не найдена в appsettings.json"
        exit 0
    }

    # Парсим строку подключения
    $params = @{}
    $connectionString -split ';' | ForEach-Object {
        if ($_ -match '(\w+)=(.+)') {
            $params[$matches[1]] = $matches[2]
        }
    }

    $dbHost = $params['Host'] ?? 'localhost'
    $dbPort = $params['Port'] ?? '5432'
    $dbName = $params['Database'] ?? 'analytics-db'
    $dbUser = $params['Username'] ?? 'postgres'
    $dbPassword = $params['Password'] ?? 'postgres'

    # Читаем docker-compose.yml
    $dockerComposeContent = Get-Content $DockerComposePath -Raw

    # Обновляем параметры PostgreSQL
    $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_USER:\s*\S+', "POSTGRES_USER: $dbUser"
    $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_PASSWORD:\s*\S+', "POSTGRES_PASSWORD: $dbPassword"
    $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_DB:\s*\S+', "POSTGRES_DB: $dbName"
    $dockerComposeContent = $dockerComposeContent -replace '"(\d+):5432"', "`"$dbPort`:5432`""

    # Сохраняем обновленный файл
    Set-Content -Path $DockerComposePath -Value $dockerComposeContent -Encoding UTF8

    Write-Host "✅ docker-compose.yml обновлен параметрами из appsettings.json" -ForegroundColor Green
    Write-Host "   Database: $dbName, User: $dbUser, Port: $dbPort" -ForegroundColor Gray

} catch {
    Write-Error "Ошибка при обновлении docker-compose.yml: $_"
    exit 1
}

