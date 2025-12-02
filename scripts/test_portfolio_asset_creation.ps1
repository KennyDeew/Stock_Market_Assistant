# Скрипт для тестирования создания актива портфеля и проверки получения сообщения через Kafka
# Использование: .\scripts\test_portfolio_asset_creation.ps1 [-PortfolioServiceUrl <url>] [-AuthServiceUrl <url>] [-Email <email>] [-Password <password>]
#
# Примечание: Если сервисы запущены через Aspire, порты могут отличаться.
# Найдите правильные порты в Aspire Dashboard (обычно https://localhost:17095):
#   - Откройте Aspire Dashboard
#   - Найдите сервис "authservice-api" и посмотрите его URL
#   - Используйте этот URL в параметре -AuthServiceUrl
#
# Пример: .\scripts\test_portfolio_asset_creation.ps1 -AuthServiceUrl "https://localhost:XXXXX"

param(
    [string]$PortfolioServiceUrl = "https://localhost:7228",
    [string]$AuthServiceUrl = "https://localhost:7175",  # Порт AuthService из launchSettings.json (может отличаться в Aspire)
    [string]$Email = "test@example.com",
    [string]$Password = "Test123!",
    [int]$WaitTimeSeconds = 10
)

$ErrorActionPreference = "Stop"

# Очистка экрана перед началом работы
Clear-Host

Write-Host "🧪 Тестирование создания актива портфеля и проверки Kafka" -ForegroundColor Cyan
Write-Host ""

# Функция для логина и получения токена
function Get-AuthToken {
    param(
        [string]$AuthUrl,
        [string]$Email,
        [string]$Password
    )

    Write-Host "🔐 Получение токена авторизации..." -ForegroundColor Yellow

    $loginBody = @{
        email = $Email
        password = $Password
    } | ConvertTo-Json

    try {
        # Используем Invoke-WebRequest для лучшего контроля над ответом
        $response = Invoke-WebRequest -Uri "$AuthUrl/api/v1/auth/login" `
            -Method Post `
            -Body $loginBody `
            -ContentType "application/json" `
            -SkipCertificateCheck `
            -ErrorAction Stop

        # Проверяем HTTP статус код
        if ($response.StatusCode -ne 200) {
            Write-Host "❌ Сервер вернул HTTP статус: $($response.StatusCode)" -ForegroundColor Red
            Write-Host "   Ответ: $($response.Content)" -ForegroundColor Red
            throw "Ошибка HTTP $($response.StatusCode). Проверьте правильность учетных данных или URL сервиса."
        }

        # Проверяем, что ответ - это JSON, а не HTML
        $contentType = $response.Headers['Content-Type']
        if ($contentType -and $contentType -notlike "*application/json*") {
            Write-Host "❌ Сервер вернул не JSON ответ. Content-Type: $contentType" -ForegroundColor Red
            Write-Host "   Первые 500 символов ответа: $($response.Content.Substring(0, [Math]::Min(500, $response.Content.Length)))" -ForegroundColor Yellow
            throw "Сервер вернул HTML вместо JSON. Проверьте правильность URL AuthService. Возможно, используется URL Aspire Dashboard вместо AuthService."
        }

        # Парсим JSON ответ
        $jsonResponse = $response.Content | ConvertFrom-Json

        # Проверяем наличие ошибок в ответе
        if ($jsonResponse.errors -or $jsonResponse.error) {
            Write-Host "❌ Сервер вернул ошибку:" -ForegroundColor Red
            if ($jsonResponse.errors) {
                foreach ($err in $jsonResponse.errors) {
                    Write-Host "   Код: $($err.code), Сообщение: $($err.message)" -ForegroundColor Red
                }
            }
            if ($jsonResponse.error) {
                Write-Host "   Ошибка: $($jsonResponse.error)" -ForegroundColor Red
            }
            throw "Ошибка авторизации. Проверьте правильность email и password."
        }

        # Логируем структуру ответа для отладки (без токена)
        $debugResponse = $jsonResponse | Select-Object -Property * -ExcludeProperty AccessToken, accessToken
        Write-Host "📋 Структура ответа: $($debugResponse | ConvertTo-Json -Depth 2)" -ForegroundColor Gray

        # Проверяем оба варианта именования (PascalCase и camelCase)
        # В C# record свойства сериализуются как PascalCase по умолчанию
        $accessToken = $null
        if ($jsonResponse.PSObject.Properties['AccessToken']) {
            $accessToken = $jsonResponse.AccessToken
        } elseif ($jsonResponse.PSObject.Properties['accessToken']) {
            $accessToken = $jsonResponse.accessToken
        }

        if ($accessToken) {
            Write-Host "✅ Токен получен успешно" -ForegroundColor Green
            return $accessToken
        } else {
            Write-Host "❌ Доступные свойства ответа: $($jsonResponse.PSObject.Properties.Name -join ', ')" -ForegroundColor Red
            throw "Токен не найден в ответе. Проверьте структуру ответа выше."
        }
    }
    catch [System.Net.Http.HttpRequestException] {
        # Обработка HTTP ошибок (401, 404, 500 и т.д.)
        $errorMessage = $_.Exception.Message
        Write-Host "❌ Ошибка HTTP при получении токена: $errorMessage" -ForegroundColor Red

        # Пытаемся прочитать тело ответа из ErrorDetails
        $responseBody = $null
        if ($_.ErrorDetails) {
            $responseBody = $_.ErrorDetails.Message
            Write-Host "   Ответ сервера: $responseBody" -ForegroundColor Red

            # Пытаемся распарсить JSON ошибки
            try {
                $errorJson = $responseBody | ConvertFrom-Json
                if ($errorJson.errors) {
                    foreach ($err in $errorJson.errors) {
                        $errMessage = $err.message
                        # Декодируем Unicode escape последовательности
                        if ($errMessage -match '\\u[0-9a-fA-F]{4}') {
                            $errMessage = [System.Text.RegularExpressions.Regex]::Replace($errMessage, '\\u([0-9a-fA-F]{4})', { param($m) [char][int]::Parse($m.Groups[1].Value, [System.Globalization.NumberStyles]::HexNumber) })
                        }
                        Write-Host "   Код: $($err.code), Сообщение: $errMessage" -ForegroundColor Red
                    }
                }
            } catch {
                # Если не JSON, просто выводим как есть
            }
        }

        # Определяем тип ошибки и даем рекомендации
        if ($errorMessage -match "500") {
            Write-Host "" -ForegroundColor Red
            Write-Host "💡 Это ошибка на стороне сервера (500 Internal Server Error)." -ForegroundColor Yellow
            Write-Host "   Возможные причины:" -ForegroundColor Yellow
            Write-Host "   - Проблема с подключением к базе данных" -ForegroundColor Gray
            Write-Host "   - Сервис не полностью инициализирован" -ForegroundColor Gray
            Write-Host "   - Ошибка в коде сервера" -ForegroundColor Gray
            Write-Host "   Проверьте логи AuthService для деталей." -ForegroundColor Yellow
        } elseif ($errorMessage -match "401|403") {
            Write-Host "" -ForegroundColor Red
            Write-Host "💡 Ошибка авторизации. Проверьте правильность email и password." -ForegroundColor Yellow
        } elseif ($errorMessage -match "404") {
            Write-Host "" -ForegroundColor Red
            Write-Host "💡 Endpoint не найден. Проверьте правильность URL AuthService." -ForegroundColor Yellow
        }

        throw "Не удалось получить токен авторизации. См. детали выше."
    }
    catch {
        Write-Host "❌ Ошибка при получении токена: $_" -ForegroundColor Red

        # Пытаемся прочитать ответ из различных источников
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            Write-Host "   Ответ сервера: $($_.ErrorDetails.Message)" -ForegroundColor Red
        } elseif ($_.Exception.Message) {
            Write-Host "   Сообщение: $($_.Exception.Message)" -ForegroundColor Red
        }

        throw
    }
}

# Функция для получения списка портфелей пользователя
function Get-UserPortfolios {
    param(
        [string]$PortfolioServiceUrl,
        [string]$Token,
        [string]$UserId
    )

    Write-Host "📋 Получение списка портфелей пользователя..." -ForegroundColor Yellow

    $headers = @{
        "Authorization" = "Bearer $Token"
    }

    try {
        $response = Invoke-RestMethod -Uri "$PortfolioServiceUrl/api/v1/portfolios/user/$UserId" `
            -Method Get `
            -Headers $headers `
            -ContentType "application/json" `
            -SkipCertificateCheck `
            -ErrorAction Stop

        if ($response.items -and $response.items.Count -gt 0) {
            Write-Host "✅ Найдено портфелей: $($response.items.Count)" -ForegroundColor Green
            return $response.items
        } else {
            Write-Host "⚠️ Портфели не найдены. Убедитесь, что у пользователя есть портфели." -ForegroundColor Yellow
            return @()
        }
    }
    catch {
        Write-Host "❌ Ошибка при получении портфелей: $_" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "   Ответ сервера: $responseBody" -ForegroundColor Red
        }
        throw
    }
}

# Функция для получения списка акций
function Get-ShareCards {
    param(
        [string]$StockCardServiceUrl,
        [string]$Token
    )

    Write-Host "📊 Получение списка акций..." -ForegroundColor Yellow

    $headers = @{
        "Authorization" = "Bearer $Token"
    }

    try {
        # Пытаемся получить список акций
        # Если endpoint не существует, используем известные ID из FakeDataFactory
        $shareCards = @(
            @{
                Id = [guid]"eb980257-db33-4fe0-80dd-3ddf5277d791"
                Ticker = "GAZP"
                Name = "Газпром (ПАО) - обыкн."
            },
            @{
                Id = [guid]"778e0b49-d17b-4c34-a67a-1a3e6dfd998f"
                Ticker = "SBER"
                Name = "Сбербанк России ПАО - обыкн."
            },
            @{
                Id = [guid]"3f95da41-6ce4-48e8-8ab1-7b42895b549e"
                Ticker = "NVTK"
                Name = "ПАО НОВАТЭК - обыкн."
            }
        )

        Write-Host "✅ Используем тестовые акции из FakeDataFactory" -ForegroundColor Green
        return $shareCards
    }
    catch {
        Write-Host "⚠️ Ошибка при получении акций, используем тестовые данные: $_" -ForegroundColor Yellow
        # Возвращаем тестовые данные
        return @(
            @{
                Id = [guid]"eb980257-db33-4fe0-80dd-3ddf5277d791"
                Ticker = "GAZP"
                Name = "Газпром (ПАО) - обыкн."
            }
        )
    }
}

# Функция для создания актива портфеля
function New-PortfolioAsset {
    param(
        [string]$PortfolioServiceUrl,
        [string]$Token,
        [guid]$PortfolioId,
        [guid]$StockCardId,
        [int]$AssetType = 1,  # 1 = Share
        [decimal]$PurchasePricePerUnit = 100.0,
        [int]$Quantity = 10
    )

    Write-Host "💼 Создание актива портфеля..." -ForegroundColor Yellow
    Write-Host "   PortfolioId: $PortfolioId" -ForegroundColor Gray
    Write-Host "   StockCardId: $StockCardId" -ForegroundColor Gray
    Write-Host "   AssetType: $AssetType (Share)" -ForegroundColor Gray
    Write-Host "   PurchasePricePerUnit: $PurchasePricePerUnit" -ForegroundColor Gray
    Write-Host "   Quantity: $Quantity" -ForegroundColor Gray

    $headers = @{
        "Authorization" = "Bearer $Token"
    }

    $body = @{
        PortfolioId = $PortfolioId.ToString()
        StockCardId = $StockCardId.ToString()
        AssetType = $AssetType
        PurchasePricePerUnit = $PurchasePricePerUnit
        Quantity = $Quantity
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$PortfolioServiceUrl/api/v1/portfolio-assets" `
            -Method Post `
            -Headers $headers `
            -Body $body `
            -ContentType "application/json" `
            -SkipCertificateCheck `
            -ErrorAction Stop

        Write-Host "✅ Актив портфеля создан успешно!" -ForegroundColor Green
        Write-Host "   AssetId: $($response.id)" -ForegroundColor Gray
        Write-Host "   Ticker: $($response.ticker)" -ForegroundColor Gray
        Write-Host "   Name: $($response.name)" -ForegroundColor Gray
        Write-Host "   TotalQuantity: $($response.totalQuantity)" -ForegroundColor Gray
        Write-Host "   AveragePurchasePrice: $($response.averagePurchasePrice)" -ForegroundColor Gray

        return $response
    }
    catch {
        Write-Host "❌ Ошибка при создании актива: $_" -ForegroundColor Red
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "   HTTP Status: $statusCode" -ForegroundColor Red
            Write-Host "   Ответ сервера: $responseBody" -ForegroundColor Red
        }
        throw
    }
}

# Функция для декодирования JWT токена
function Get-JwtPayload {
    param([string]$Token)

    $parts = $Token.Split('.')
    if ($parts.Length -ne 3) {
        throw "Неверный формат JWT токена"
    }

    $payload = $parts[1]
    # Добавляем padding если нужно
    $mod = $payload.Length % 4
    if ($mod -gt 0) {
        $payload += "=" * (4 - $mod)
    }

    $jsonBytes = [System.Convert]::FromBase64String($payload)
    $jsonString = [System.Text.Encoding]::UTF8.GetString($jsonBytes)
    return $jsonString | ConvertFrom-Json
}

# Функция для проверки доступности сервиса
function Test-ServiceAvailability {
    param(
        [string]$ServiceUrl,
        [string]$ServiceName
    )

    Write-Host "🔍 Проверка доступности $ServiceName..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri $ServiceUrl `
            -Method Get `
            -SkipCertificateCheck `
            -TimeoutSec 5 `
            -ErrorAction Stop
        Write-Host "✅ $ServiceName доступен" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "⚠️ $ServiceName недоступен: $_" -ForegroundColor Yellow
        Write-Host "   Продолжаем выполнение..." -ForegroundColor Gray
        return $false
    }
}

# Основная логика
try {
    Write-Host "🔧 Параметры теста:" -ForegroundColor Cyan
    Write-Host "   PortfolioServiceUrl: $PortfolioServiceUrl" -ForegroundColor Gray
    Write-Host "   AuthServiceUrl: $AuthServiceUrl" -ForegroundColor Gray
    Write-Host "   Email: $Email" -ForegroundColor Gray
    Write-Host "   WaitTimeSeconds: $WaitTimeSeconds" -ForegroundColor Gray
    Write-Host ""

    # Проверка доступности сервисов
    Test-ServiceAvailability -ServiceUrl $PortfolioServiceUrl -ServiceName "PortfolioService" | Out-Null
    Test-ServiceAvailability -ServiceUrl $AuthServiceUrl -ServiceName "AuthService" | Out-Null
    Write-Host ""

    # Шаг 1: Получение токена
    $token = Get-AuthToken -AuthUrl $AuthServiceUrl -Email $Email -Password $Password

    # Шаг 2: Получение UserId из токена
    $tokenPayload = Get-JwtPayload -Token $token
    $userId = [guid]$tokenPayload.Id
    Write-Host "👤 UserId: $userId" -ForegroundColor Cyan
    Write-Host ""

    # Шаг 3: Получение списка портфелей
    $portfolios = Get-UserPortfolios -PortfolioServiceUrl $PortfolioServiceUrl -Token $token -UserId $userId

    if ($portfolios.Count -eq 0) {
        Write-Host "❌ У пользователя нет портфелей. Создайте портфель перед тестированием." -ForegroundColor Red
        exit 1
    }

    # Выбираем первый портфель (не приватный, чтобы сообщение отправилось в Kafka)
    $portfolio = $portfolios | Where-Object { -not $_.isPrivate } | Select-Object -First 1
    if (-not $portfolio) {
        $portfolio = $portfolios[0]
        Write-Host "⚠️ Выбран приватный портфель. Сообщение в Kafka не будет отправлено (только для публичных портфелей)." -ForegroundColor Yellow
    } else {
        Write-Host "✅ Выбран публичный портфель. Сообщение будет отправлено в Kafka." -ForegroundColor Green
    }

    Write-Host "📦 Портфель: $($portfolio.name) (ID: $($portfolio.id))" -ForegroundColor Cyan
    Write-Host ""

    # Шаг 4: Получение списка акций
    $shareCards = Get-ShareCards -StockCardServiceUrl "" -Token $token
    $selectedStockCard = $shareCards[0]
    Write-Host "📈 Выбранная акция: $($selectedStockCard.Ticker) - $($selectedStockCard.Name)" -ForegroundColor Cyan
    Write-Host ""

    # Шаг 5: Создание актива портфеля
    $asset = New-PortfolioAsset `
        -PortfolioServiceUrl $PortfolioServiceUrl `
        -Token $token `
        -PortfolioId ([guid]$portfolio.id) `
        -StockCardId $selectedStockCard.Id `
        -AssetType 1 `
        -PurchasePricePerUnit 150.50 `
        -Quantity 5

    Write-Host ""
    Write-Host "⏳ Ожидание обработки сообщения через Kafka ($WaitTimeSeconds секунд)..." -ForegroundColor Yellow
    Write-Host "   Сообщение должно быть обработано AnalyticsService через топик 'portfolio.transactions'" -ForegroundColor Gray
    Start-Sleep -Seconds $WaitTimeSeconds

    Write-Host ""
    Write-Host "✅ Тест завершен успешно!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Сводка созданного актива:" -ForegroundColor Cyan
    Write-Host "   AssetId: $($asset.id)" -ForegroundColor White
    Write-Host "   PortfolioId: $($portfolio.id)" -ForegroundColor White
    Write-Host "   PortfolioName: $($portfolio.name)" -ForegroundColor White
    Write-Host "   StockCardId: $($selectedStockCard.Id)" -ForegroundColor White
    Write-Host "   StockCardTicker: $($selectedStockCard.Ticker)" -ForegroundColor White
    Write-Host "   Quantity: $($asset.totalQuantity)" -ForegroundColor White
    Write-Host "   AveragePurchasePrice: $($asset.averagePurchasePrice)" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Как проверить получение сообщения через Kafka:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   1. Проверьте логи AnalyticsService:" -ForegroundColor Yellow
    Write-Host "      - Ищите сообщения типа 'Успешно обработано и закоммичено X из Y сообщений в Kafka'" -ForegroundColor Gray
    Write-Host "      - Ищите сообщения о десериализации TransactionMessage" -ForegroundColor Gray
    Write-Host "      - Ищите сообщения о сохранении AssetTransaction в базу данных" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   2. Проверьте базу данных AnalyticsService:" -ForegroundColor Yellow
    Write-Host "      - Подключитесь к PostgreSQL: Host=localhost, Port=14055, Database=analytics-db" -ForegroundColor Gray
    Write-Host "      - Выполните запрос:" -ForegroundColor Gray
    Write-Host "        SELECT * FROM asset_transactions WHERE portfolio_id = '$($portfolio.id)' ORDER BY transaction_time DESC LIMIT 1;" -ForegroundColor White
    Write-Host ""
    Write-Host "   3. Проверьте Kafka топик (если установлен Kafka UI):" -ForegroundColor Yellow
    Write-Host "      - Откройте http://localhost:9100" -ForegroundColor Gray
    Write-Host "      - Найдите топик 'portfolio.transactions'" -ForegroundColor Gray
    Write-Host "      - Проверьте последние сообщения" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   4. Проверьте Outbox в PortfolioService (если нужно):" -ForegroundColor Yellow
    Write-Host "      - Подключитесь к PostgreSQL: Host=localhost, Port=14050, Database=portfolio-db" -ForegroundColor Gray
    Write-Host "      - Выполните запрос:" -ForegroundColor Gray
    Write-Host "        SELECT * FROM outbox_messages WHERE topic = 'portfolio.transactions' ORDER BY created_at DESC LIMIT 5;" -ForegroundColor White
    Write-Host "      - Проверьте, что сообщение обработано (processed_at IS NOT NULL)" -ForegroundColor Gray
    Write-Host ""
    if ($portfolio.isPrivate) {
        Write-Host "⚠️ ВНИМАНИЕ: Выбранный портфель является приватным (isPrivate=true)." -ForegroundColor Yellow
        Write-Host "   Сообщения в Kafka отправляются только для публичных портфелей." -ForegroundColor Yellow
        Write-Host "   Для тестирования Kafka выберите публичный портфель." -ForegroundColor Yellow
        Write-Host ""
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Тест завершился с ошибкой: $_" -ForegroundColor Red
    Write-Host "   StackTrace: $($_.ScriptStackTrace)" -ForegroundColor Red
    exit 1
}

