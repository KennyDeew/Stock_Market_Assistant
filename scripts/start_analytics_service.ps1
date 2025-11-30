# Скрипт для запуска AnalyticsService с автоматическим запуском БД и Kafka через Docker

param(
    [switch]$SkipDbCheck = $false,
    [switch]$SkipKafkaCheck = $false,
    [switch]$SkipMigrations = $false,
    [switch]$SkipDockerStart = $false,
    [string]$KafkaBootstrapServer = "localhost:9092",
    [string]$DbHost = "localhost",
    [int]$DbPort = 5432,
    [string]$DbName = "analytics-db",
    [string]$DbUser = "postgres",
    [string]$DbPassword = "postgres"
)

$ErrorActionPreference = "Continue"

# Очистка экрана
Clear-Host

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Запуск AnalyticsService" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Определяем пути
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptRoot
$analyticsApiPath = Join-Path $projectRoot "src\backend\services\AnalyticsService\AnalyticsService.WebApi"
$infrastructurePath = Join-Path $projectRoot "src\backend\services\AnalyticsService\AnalyticsService.Infrastructure.EntityFramework"
$appsettingsPath = Join-Path $analyticsApiPath "appsettings.json"
$dockerComposePath = Join-Path $scriptRoot "docker-compose-analytics.yml"

# Проверка существования путей
if (-not (Test-Path $analyticsApiPath)) {
    Write-Host "❌ Ошибка: Не найден путь к AnalyticsService.WebApi" -ForegroundColor Red
    Write-Host "   Ожидаемый путь: $analyticsApiPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Путь к проекту: $analyticsApiPath" -ForegroundColor Gray
Write-Host ""

# Читаем параметры PostgreSQL из appsettings.json AnalyticsService
if (Test-Path $appsettingsPath) {
    try {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $connectionString = $appsettings.ConnectionStrings.'analytics-db'

        if ($connectionString) {
            # Парсим строку подключения: Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=postgres
            $connectionParams = @{}
            $connectionString -split ';' | ForEach-Object {
                if ($_ -match '(\w+)=(.+)') {
                    $connectionParams[$matches[1]] = $matches[2]
                }
            }

            # Обновляем параметры по умолчанию из appsettings.json
            if ($connectionParams.ContainsKey('Host')) { $DbHost = $connectionParams['Host'] }
            if ($connectionParams.ContainsKey('Port')) { $DbPort = [int]$connectionParams['Port'] }
            if ($connectionParams.ContainsKey('Database')) { $DbName = $connectionParams['Database'] }
            if ($connectionParams.ContainsKey('Username')) { $DbUser = $connectionParams['Username'] }
            if ($connectionParams.ContainsKey('Password')) { $DbPassword = $connectionParams['Password'] }

            Write-Host "📋 Параметры PostgreSQL из appsettings.json AnalyticsService:" -ForegroundColor Gray
            Write-Host "   Host: $DbHost, Port: $DbPort, Database: $DbName, User: $DbUser" -ForegroundColor Gray
            Write-Host ""

            # Обновляем docker-compose.yml с параметрами из appsettings.json
            Write-Host "🔄 Обновление docker-compose.yml параметрами из appsettings.json..." -ForegroundColor Gray
            $updateScript = Join-Path $scriptRoot "update_docker_compose_from_appsettings.ps1"
            if (Test-Path $updateScript) {
                & $updateScript -AppSettingsPath $appsettingsPath -DockerComposePath $dockerComposePath 2>&1 | Out-Null
            } else {
                # Если скрипт обновления недоступен, обновляем напрямую
                $dockerComposeContent = Get-Content $dockerComposePath -Raw
                $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_USER:\s*\S+', "POSTGRES_USER: $DbUser"
                $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_PASSWORD:\s*\S+', "POSTGRES_PASSWORD: $DbPassword"
                $dockerComposeContent = $dockerComposeContent -replace 'POSTGRES_DB:\s*\S+', "POSTGRES_DB: $DbName"
                $dockerComposeContent = $dockerComposeContent -replace '"(\d+):5432"', "`"$DbPort`:5432`""
                Set-Content -Path $dockerComposePath -Value $dockerComposeContent -Encoding UTF8 -NoNewline
                Write-Host "   ✅ docker-compose.yml обновлен" -ForegroundColor Green
            }
            Write-Host ""
        }
    } catch {
        Write-Host "   ⚠️ Не удалось прочитать параметры из appsettings.json, используем значения по умолчанию" -ForegroundColor Yellow
        Write-Host ""
    }
} else {
    Write-Host "   ⚠️ Файл appsettings.json не найден, используем значения по умолчанию" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================
# Проверка Docker
# ============================================
$docker = Get-Command docker -ErrorAction SilentlyContinue
if (-not $docker) {
    Write-Host "❌ Docker не найден в PATH" -ForegroundColor Red
    Write-Host "   Установите Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Проверка docker-compose
$dockerCompose = Get-Command docker-compose -ErrorAction SilentlyContinue
if (-not $dockerCompose) {
    Write-Host "⚠️ docker-compose не найден, используем 'docker compose' (новый синтаксис)" -ForegroundColor Yellow
    $useDockerCompose = $false
} else {
    $useDockerCompose = $true
}

# ============================================
# Шаг 1: Проверка и запуск PostgreSQL в Docker
# ============================================
if (-not $SkipDbCheck) {
    Write-Host "1. Проверка PostgreSQL в Docker..." -ForegroundColor Yellow

    if (-not $SkipDockerStart) {
        if (-not (Test-Path $dockerComposePath)) {
            Write-Host "   ❌ Файл docker-compose-analytics.yml не найден" -ForegroundColor Red
            Write-Host "   Ожидаемый путь: $dockerComposePath" -ForegroundColor Yellow
            exit 1
        }

        # Проверяем, запущен ли контейнер analytics-postgres
        $runningPostgres = docker ps --filter "name=analytics-postgres" --format "{{.Names}}" 2>&1
        $existingPostgres = docker ps -a --filter "name=analytics-postgres" --format "{{.Names}}" 2>&1

        if ($runningPostgres -and $runningPostgres -notmatch "Error") {
            Write-Host "   ✅ Контейнер PostgreSQL уже запущен" -ForegroundColor Green
            $dbReady = $true
        } else {
            Write-Host "   🐳 Запуск PostgreSQL через Docker..." -ForegroundColor Yellow

            # Если контейнер существует, но не запущен - удаляем его для чистого запуска
            if ($existingPostgres -and $existingPostgres -notmatch "Error") {
                Write-Host "   Удаление старого контейнера..." -ForegroundColor Gray
                Push-Location $scriptRoot
                try {
                    if ($useDockerCompose) {
                        docker-compose -f docker-compose-analytics.yml rm -f postgres 2>&1 | Out-Null
                    } else {
                        docker compose -f docker-compose-analytics.yml rm -f postgres 2>&1 | Out-Null
                    }
                } catch {
                    # Игнорируем ошибки
                } finally {
                    Pop-Location
                }
            }

            # Запускаем PostgreSQL из docker-compose
            Push-Location $scriptRoot
            try {
                Write-Host "   Запуск контейнера..." -ForegroundColor Gray
                if ($useDockerCompose) {
                    $dockerOutput = docker-compose -f docker-compose-analytics.yml up -d postgres 2>&1
                } else {
                    $dockerOutput = docker compose -f docker-compose-analytics.yml up -d postgres 2>&1
                }

                if ($LASTEXITCODE -eq 0) {
                    Write-Host "   ✅ PostgreSQL запущен в Docker" -ForegroundColor Green
                    Write-Host "   ⏳ Ожидание готовности PostgreSQL (до 30 секунд)..." -ForegroundColor Yellow

                    # Ждем готовности PostgreSQL
                    $maxAttempts = 30
                    $attempt = 0
                    $dbReady = $false
                    while ($attempt -lt $maxAttempts -and -not $dbReady) {
                        Start-Sleep -Seconds 1
                        $dbPortCheck = Test-NetConnection -ComputerName $DbHost -Port $DbPort -InformationLevel Quiet -WarningAction SilentlyContinue
                        if ($dbPortCheck) {
                            # Дополнительная проверка - убеждаемся, что это наш контейнер
                            $containerCheck = docker ps --filter "name=analytics-postgres" --filter "status=running" --format "{{.Names}}" 2>&1
                            if ($containerCheck -and $containerCheck -notmatch "Error") {
                                $dbReady = $true
                            }
                        }
                        $attempt++
                        Write-Host "   ." -NoNewline -ForegroundColor Gray
                    }
                    Write-Host ""

                    if ($dbReady) {
                        Write-Host "   ✅ PostgreSQL готов" -ForegroundColor Green
                    } else {
                        Write-Host "   ⚠️ PostgreSQL запущен, но еще не готов. Продолжаем..." -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "   ❌ Не удалось запустить PostgreSQL через Docker" -ForegroundColor Red
                    Write-Host "   Ошибка Docker:" -ForegroundColor Yellow
                    $dockerOutput | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
                    Write-Host ""
                    Write-Host "   Возможные причины:" -ForegroundColor Yellow
                    Write-Host "   - Порт 5432 уже занят другим процессом" -ForegroundColor Gray
                    Write-Host "   - Docker не запущен или недоступен" -ForegroundColor Gray
                    Write-Host ""
                    Write-Host "   Проверьте:" -ForegroundColor Cyan
                    Write-Host "   - docker ps (проверьте запущенные контейнеры)" -ForegroundColor Gray
                    Write-Host "   - netstat -ano | findstr :5432 (проверьте порт PostgreSQL)" -ForegroundColor Gray
                    Write-Host "   - Остановите локальный PostgreSQL, если он запущен: Stop-Service postgresql-x64-*" -ForegroundColor Gray
                    exit 1
                }
            } finally {
                Pop-Location
            }
        }
    } else {
        # Если пропущен автоматический запуск, просто проверяем доступность
        $dbPortCheck = Test-NetConnection -ComputerName $DbHost -Port $DbPort -InformationLevel Quiet -WarningAction SilentlyContinue
        if ($dbPortCheck) {
            Write-Host "   ✅ PostgreSQL доступен на $DbHost`:$DbPort" -ForegroundColor Green
        } else {
            Write-Host "   ❌ PostgreSQL недоступен на $DbHost`:$DbPort" -ForegroundColor Red
            Write-Host "   Запустите PostgreSQL вручную или уберите флаг -SkipDockerStart" -ForegroundColor Yellow
            exit 1
        }
    }

    # Проверка существования базы данных
    Write-Host "   Проверка существования базы данных..." -ForegroundColor Gray

    $psql = Get-Command psql -ErrorAction SilentlyContinue
    $dbExists = $false

    if ($psql) {
        # Используем psql, если доступен
        $env:PGPASSWORD = $DbPassword
        $dbCheck = psql -h $DbHost -p $DbPort -U $DbUser -lqt 2>&1 | Select-String $DbName
        if ($dbCheck) {
            $dbExists = $true
        }
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    } else {
        # Если psql недоступен, используем Docker для проверки и создания БД
        Write-Host "   psql не найден, используем Docker для проверки БД..." -ForegroundColor Gray
        $dbCheckDocker = docker exec analytics-postgres psql -U $DbUser -lqt 2>&1 | Select-String $DbName
        if ($dbCheckDocker) {
            $dbExists = $true
        }
    }

    if ($dbExists) {
        Write-Host "   ✅ База данных '$DbName' существует" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️ База данных '$DbName' не найдена" -ForegroundColor Yellow
        Write-Host "   Создание базы данных через Docker..." -ForegroundColor Yellow

        if ($psql) {
            # Используем psql, если доступен
            $env:PGPASSWORD = $DbPassword
            $createDb = psql -h $DbHost -p $DbPort -U $DbUser -d postgres -c "CREATE DATABASE $DbName;" 2>&1
            Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ База данных '$DbName' создана" -ForegroundColor Green
            } else {
                Write-Host "   ⚠️ Не удалось создать базу данных через psql" -ForegroundColor Yellow
                Write-Host "   Пробуем через Docker..." -ForegroundColor Gray
                $createDbDocker = docker exec analytics-postgres psql -U $DbUser -d postgres -c "CREATE DATABASE $DbName;" 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "   ✅ База данных '$DbName' создана через Docker" -ForegroundColor Green
                } else {
                    Write-Host "   ⚠️ Не удалось создать базу данных" -ForegroundColor Yellow
                    Write-Host "   Создайте базу вручную: CREATE DATABASE $DbName;" -ForegroundColor Gray
                }
            }
        } else {
            # Используем Docker для создания БД
            $createDbDocker = docker exec analytics-postgres psql -U $DbUser -d postgres -c "CREATE DATABASE $DbName;" 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ База данных '$DbName' создана через Docker" -ForegroundColor Green
            } else {
                Write-Host "   ⚠️ Не удалось создать базу данных через Docker" -ForegroundColor Yellow
                Write-Host "   Ошибка: $createDbDocker" -ForegroundColor Gray
                Write-Host "   Создайте базу вручную: docker exec -it analytics-postgres psql -U $DbUser -d postgres -c 'CREATE DATABASE $DbName;'" -ForegroundColor Gray
            }
        }
    }
} else {
    Write-Host "1. Проверка PostgreSQL пропущена (-SkipDbCheck)" -ForegroundColor Gray
}
Write-Host ""

# ============================================
# Шаг 2: Применение миграций
# ============================================
if (-not $SkipMigrations) {
    Write-Host "2. Применение миграций..." -ForegroundColor Yellow

    Push-Location $analyticsApiPath
    try {
        # Указываем строку подключения явно для миграций (используем localhost вместо postgres)
        # Это необходимо, так как миграции запускаются с хоста, а не из контейнера
        # Используем Set-Item для переменных окружения с дефисом в имени
        Set-Item -Path "env:ConnectionStrings__analytics-db" -Value "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"

        # Указываем контекст явно, так как в проекте несколько DbContext
        $migrationResult = dotnet ef database update --project $infrastructurePath --startup-project . --context AnalyticsDbContext 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Миграции применены успешно" -ForegroundColor Green
        } else {
            # Фильтруем ошибки OpenSearch (они не критичны для миграций)
            $criticalErrors = $migrationResult | Where-Object {
                $_ -notmatch "OpenSearch" -and
                $_ -notmatch "Failed to discover" -and
                $_ -notmatch "подключение не установлено"
            }

            if ($criticalErrors) {
                Write-Host "   ⚠️ Ошибка при применении миграций:" -ForegroundColor Yellow
                $criticalErrors | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
            } else {
                Write-Host "   ⚠️ Предупреждения при применении миграций (OpenSearch недоступен, но это не критично)" -ForegroundColor Yellow
            }
            Write-Host "   Продолжаем запуск..." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ⚠️ Не удалось применить миграции: $_" -ForegroundColor Yellow
        Write-Host "   Продолжаем запуск..." -ForegroundColor Yellow
    } finally {
        # Очищаем переменную окружения
        Remove-Item -Path "env:ConnectionStrings__analytics-db" -ErrorAction SilentlyContinue
        Pop-Location
    }
} else {
    Write-Host "2. Применение миграций пропущено (-SkipMigrations)" -ForegroundColor Gray
}
Write-Host ""

# ============================================
# Шаг 3: Проверка и запуск Kafka
# ============================================
if (-not $SkipKafkaCheck) {
    Write-Host "3. Проверка Kafka..." -ForegroundColor Yellow

    $kafkaParts = $KafkaBootstrapServer.Split(':')
    $kafkaHost = $kafkaParts[0]
    $kafkaPort = if ($kafkaParts.Length -gt 1) { [int]$kafkaParts[1] } else { 9092 }

    $kafkaCheck = Test-NetConnection -ComputerName $kafkaHost -Port $kafkaPort -InformationLevel Quiet -WarningAction SilentlyContinue
    if (-not $kafkaCheck) {
        Write-Host "   ❌ Kafka недоступен на $KafkaBootstrapServer" -ForegroundColor Red

        if (-not $SkipDockerStart) {
            Write-Host "   🐳 Запуск Kafka через Docker..." -ForegroundColor Yellow

            if (-not (Test-Path $dockerComposePath)) {
                Write-Host "   ❌ Файл docker-compose-analytics.yml не найден" -ForegroundColor Red
                Write-Host "   Ожидаемый путь: $dockerComposePath" -ForegroundColor Yellow
                exit 1
            }

            # Инициализируем переменные
            $useExistingZookeeper = $false
            $servicesToStart = "zookeeper kafka"
            $zookeeperHostPort = 2183  # Порт на хосте для Zookeeper (внутри контейнера остается 2181)

            # Проверяем, запущен ли уже наш контейнер Zookeeper
            $existingZookeeper = docker ps --filter "name=analytics-zookeeper" --format "{{.Names}}" 2>&1
            if ($existingZookeeper -and $existingZookeeper -notmatch "Error") {
                Write-Host "   ℹ️ Контейнер Zookeeper уже запущен" -ForegroundColor Yellow
                $useExistingZookeeper = $true
                $servicesToStart = "kafka"
            } else {
                # Проверяем занятость порта 2182 (наш порт для Zookeeper)
                $zookeeperPortCheck = Test-NetConnection -ComputerName localhost -Port $zookeeperHostPort -InformationLevel Quiet -WarningAction SilentlyContinue
                if ($zookeeperPortCheck) {
                    Write-Host "   ⚠️ Порт $zookeeperHostPort (Zookeeper) уже занят" -ForegroundColor Yellow
                    Write-Host "   Проверяем, не запущен ли уже Zookeeper в другом контейнере..." -ForegroundColor Gray

                    # Проверяем все контейнеры с Zookeeper
                    $allZookeeper = docker ps -a --filter "ancestor=confluentinc/cp-zookeeper" --format "{{.Names}}" 2>&1
                    if ($allZookeeper -and $allZookeeper -notmatch "Error") {
                        Write-Host "   ℹ️ Найден Zookeeper в другом контейнере: $allZookeeper" -ForegroundColor Yellow
                        Write-Host "   Используем существующий Zookeeper" -ForegroundColor Green
                        $useExistingZookeeper = $true
                        $servicesToStart = "kafka"
                    } else {
                        Write-Host "   ⚠️ Порт $zookeeperHostPort занят, но Zookeeper не найден в контейнерах" -ForegroundColor Yellow
                        Write-Host "   Пытаемся запустить Zookeeper на порту $zookeeperHostPort..." -ForegroundColor Yellow
                        # Продолжаем запуск - Docker сам покажет ошибку, если порт действительно занят
                    }
                } else {
                    Write-Host "   ✅ Порт $zookeeperHostPort свободен для Zookeeper" -ForegroundColor Green
                }
            }

            # Проверяем, не запущены ли уже наши контейнеры
            $existingKafka = docker ps -a --filter "name=analytics-kafka" --format "{{.Names}}" 2>&1
            $existingZookeeper = docker ps -a --filter "name=analytics-zookeeper" --format "{{.Names}}" 2>&1

            if ($existingKafka -or ($existingZookeeper -and -not $useExistingZookeeper)) {
                Write-Host "   ℹ️ Найдены существующие контейнеры, перезапускаем..." -ForegroundColor Yellow
                Push-Location $scriptRoot
                try {
                    if ($useDockerCompose) {
                        docker-compose -f docker-compose-analytics.yml down 2>&1 | Out-Null
                    } else {
                        docker compose -f docker-compose-analytics.yml down 2>&1 | Out-Null
                    }
                } catch {
                    # Игнорируем ошибки при остановке
                } finally {
                    Pop-Location
                }
            }

            # Запускаем контейнеры из docker-compose
            Push-Location $scriptRoot
            try {
                Write-Host "   Запуск контейнеров ($servicesToStart)..." -ForegroundColor Gray

                # Разбиваем строку сервисов на массив для правильной передачи в docker-compose
                $servicesArray = $servicesToStart -split '\s+' | Where-Object { $_ -ne '' }

                # Формируем команду с правильными аргументами (явно указываем элементы массива)
                if ($useDockerCompose) {
                    if ($servicesArray.Count -eq 2) {
                        $dockerOutput = & docker-compose -f docker-compose-analytics.yml up -d $servicesArray[0] $servicesArray[1] 2>&1
                    } else {
                        $dockerOutput = & docker-compose -f docker-compose-analytics.yml up -d $servicesArray[0] 2>&1
                    }
                } else {
                    if ($servicesArray.Count -eq 2) {
                        $dockerOutput = & docker compose -f docker-compose-analytics.yml up -d $servicesArray[0] $servicesArray[1] 2>&1
                    } else {
                        $dockerOutput = & docker compose -f docker-compose-analytics.yml up -d $servicesArray[0] 2>&1
                    }
                }

                if ($LASTEXITCODE -eq 0) {
                    Write-Host "   ✅ Kafka и Zookeeper запущены в Docker" -ForegroundColor Green
                    Write-Host "   ⏳ Ожидание готовности Kafka (до 30 секунд)..." -ForegroundColor Yellow

                    # Ждем готовности Kafka
                    $maxAttempts = 30
                    $attempt = 0
                    $kafkaReady = $false
                    while ($attempt -lt $maxAttempts -and -not $kafkaReady) {
                        Start-Sleep -Seconds 1
                        $kafkaCheck = Test-NetConnection -ComputerName $kafkaHost -Port $kafkaPort -InformationLevel Quiet -WarningAction SilentlyContinue
                        if ($kafkaCheck) {
                            $kafkaReady = $true
                        }
                        $attempt++
                        Write-Host "   ." -NoNewline -ForegroundColor Gray
                    }
                    Write-Host ""

                    if ($kafkaReady) {
                        Write-Host "   ✅ Kafka готов" -ForegroundColor Green
                    } else {
                        Write-Host "   ⚠️ Kafka запущен, но еще не готов. Продолжаем..." -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "   ❌ Не удалось запустить Kafka через Docker" -ForegroundColor Red
                    Write-Host "   Ошибка Docker:" -ForegroundColor Yellow
                    $dockerOutput | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
                    Write-Host ""
                    Write-Host "   Возможные причины:" -ForegroundColor Yellow
                    Write-Host "   - Порт 2181 или 9092 уже занят другим процессом" -ForegroundColor Gray
                    Write-Host "   - Docker не запущен или недоступен" -ForegroundColor Gray
                    Write-Host "   - Недостаточно ресурсов для запуска контейнеров" -ForegroundColor Gray
                    Write-Host ""
                    Write-Host "   Проверьте:" -ForegroundColor Cyan
                    Write-Host "   - docker ps (проверьте запущенные контейнеры)" -ForegroundColor Gray
                    Write-Host "   - netstat -ano | findstr :2181 (проверьте порт Zookeeper)" -ForegroundColor Gray
                    Write-Host "   - netstat -ano | findstr :9092 (проверьте порт Kafka)" -ForegroundColor Gray
                    exit 1
                }
            } finally {
                Pop-Location
            }
        } else {
            Write-Host "   Убедитесь, что Kafka запущен" -ForegroundColor Yellow
            Write-Host ""
            $answer = Read-Host "Продолжить запуск без Kafka? (Y/N)"
            if ($answer -ne "Y" -and $answer -ne "y") {
                exit 1
            }
        }
    } else {
        Write-Host "   ✅ Kafka доступен на $KafkaBootstrapServer" -ForegroundColor Green
    }
} else {
    Write-Host "3. Проверка Kafka пропущена (-SkipKafkaCheck)" -ForegroundColor Gray
}
Write-Host ""

# ============================================
# Шаг 4: Обновление appsettings.json
# ============================================
Write-Host "4. Проверка конфигурации..." -ForegroundColor Yellow

$appsettingsPath = Join-Path $analyticsApiPath "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json

    # Проверка строки подключения
    if ([string]::IsNullOrWhiteSpace($appsettings.ConnectionStrings.'analytics-db')) {
        Write-Host "   ⚠️ Строка подключения к БД пустая, обновляем..." -ForegroundColor Yellow
        $connectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"
        $appsettings.ConnectionStrings.'analytics-db' = $connectionString
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
        Write-Host "   ✅ Строка подключения обновлена" -ForegroundColor Green
    } else {
        Write-Host "   ✅ Строка подключения к БД настроена" -ForegroundColor Green
    }

    # Проверка Kafka конфигурации
    if ($appsettings.Kafka.BootstrapServers -ne $KafkaBootstrapServer) {
        Write-Host "   ⚠️ Обновляем адрес Kafka..." -ForegroundColor Yellow
        $appsettings.Kafka.BootstrapServers = $KafkaBootstrapServer
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
        Write-Host "   ✅ Адрес Kafka обновлен: $KafkaBootstrapServer" -ForegroundColor Green
    } else {
        Write-Host "   ✅ Конфигурация Kafka настроена" -ForegroundColor Green
    }
} else {
    Write-Host "   ⚠️ Файл appsettings.json не найден" -ForegroundColor Yellow
}
Write-Host ""

# ============================================
# Шаг 5: Запуск AnalyticsService
# ============================================
Write-Host "5. Запуск AnalyticsService..." -ForegroundColor Yellow
Write-Host ""

Push-Location $analyticsApiPath
try {
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Запуск AnalyticsService" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "💡 Для остановки нажмите Ctrl+C" -ForegroundColor Gray
    Write-Host "💡 Логи будут отображаться ниже" -ForegroundColor Gray
    Write-Host "💡 Docker контейнеры будут продолжать работать после остановки" -ForegroundColor Gray
    Write-Host ""

    # Запускаем dotnet run
    dotnet run
} catch {
    Write-Host ""
    Write-Host "❌ Ошибка при запуске: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}
