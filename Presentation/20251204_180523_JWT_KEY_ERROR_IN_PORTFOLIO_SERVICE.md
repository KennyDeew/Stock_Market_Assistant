# Анализ ошибки JWT ключа в PortfolioService

## Дата: 2025-12-04

## Проблема

PortfolioService падает с ошибкой при запуске:

```
System.ArgumentException: IDX10703: Cannot create a 'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey', key length is zero.
   at Microsoft.IdentityModel.Tokens.SymmetricSecurityKey..ctor(Byte[] key)
   at StockMarketAssistant.PortfolioService.WebApi.Program.<>c__DisplayClass1_0.<Main>b__1(JwtBearerOptions options) in /src/src/backend/services/PortfolioService/PortfolioService.WebApi/Program.cs:line 54
```

## Причина

В файле `PortfolioService.WebApi/Program.cs` на строке 63 происходит попытка создать `SymmetricSecurityKey` из пустого или отсутствующего JWT ключа:

```csharp
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
```

Проблемы:
1. **Пустой ключ в конфигурации** - в `appsettings.json` поле `Jwt:Key` пустое: `"Key": ""`
2. **Отсутствие проверки** - код не проверяет, что ключ не пустой перед использованием
3. **Использование null-forgiving оператора** - `jwtSettings["Key"]!` скрывает потенциальную проблему

## Сравнение с AnalyticsService

В `AnalyticsService.WebApi/Program.cs` реализована защита от этой проблемы:

```csharp
var jwtKey = jwtSettings["Key"];

// Если ключ не указан, используем дефолтный ключ для разработки (только для Development)
if (string.IsNullOrEmpty(jwtKey) && builder.Environment.IsDevelopment())
{
    jwtKey = "DevelopmentKey-AtLeast32CharactersLongForHS256Algorithm";
}

// Регистрируем JWT только если ключ указан
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // ... настройка JWT
        });
}
else
{
    logger.LogWarning("JWT ключ не указан. JWT авторизация отключена.");
}
```

## Дополнительные проблемы в логах

1. **StockCardService недоступен** - gateway не может найти `stockcardservice-api:8080`:
   ```
   System.Net.Http.HttpRequestException: Name or service not known (stockcardservice-api:8080)
   ```
   Это может быть нормально, если сервис не запущен или не требуется.

2. **AnalyticsService работает нормально** - health checks возвращают 200 OK.

## Решение

### Вариант 1: Добавить проверку и дефолтный ключ (рекомендуется)

Применить тот же подход, что используется в AnalyticsService:

**Файл:** `src/backend/services/PortfolioService/PortfolioService.WebApi/Program.cs`

Заменить код на строках 48-65:

```csharp
// Add services to the container.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];

// Если ключ не указан, используем дефолтный ключ для разработки (только для Development)
if (string.IsNullOrEmpty(jwtKey) && builder.Environment.IsDevelopment())
{
    jwtKey = "DevelopmentKey-AtLeast32CharactersLongForHS256Algorithm";
}

// Регистрируем JWT только если ключ указан
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = !string.IsNullOrEmpty(jwtSettings["Issuer"]),
                ValidateAudience = !string.IsNullOrEmpty(jwtSettings["Audience"]),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "AuthService",
                ValidAudience = jwtSettings["Audience"] ?? "AuthServiceClients",
                RoleClaimType = "Role",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
}
else
{
    // Если ключ не указан и не Development режим, логируем предупреждение
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
    logger.LogWarning("JWT ключ не указан. JWT авторизация отключена.");
}
```

**Плюсы:**
- ✅ Защита от падения при пустом ключе
- ✅ Автоматический дефолтный ключ для Development
- ✅ Единообразный подход с AnalyticsService
- ✅ Логирование предупреждения вместо падения

**Минусы:**
- ⚠️ В Production режиме без ключа JWT авторизация будет отключена (но это лучше, чем падение)

### Вариант 2: Установить JWT ключ через переменные окружения

Установить переменную окружения `Jwt__Key` при запуске сервиса.

**Плюсы:**
- ✅ Безопасно (ключ не в коде)
- ✅ Гибко (можно менять без пересборки)

**Минусы:**
- ⚠️ Не решает проблему полностью (если забыли установить, сервис упадет)
- ⚠️ Требует настройки в каждом окружении

### Вариант 3: Комбинированный подход (оптимальный)

Применить Вариант 1 + добавить проверку в Production с понятным сообщением об ошибке:

```csharp
if (string.IsNullOrEmpty(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = "DevelopmentKey-AtLeast32CharactersLongForHS256Algorithm";
        logger.LogWarning("Используется дефолтный JWT ключ для разработки. Для Production установите Jwt:Key в конфигурации.");
    }
    else
    {
        throw new InvalidOperationException(
            "JWT Key не настроен. Установите значение 'Jwt:Key' в конфигурации или переменной окружения.");
    }
}
```

**Плюсы:**
- ✅ Защита от падения в Development
- ✅ Явная ошибка в Production (лучше, чем тихое отключение авторизации)
- ✅ Понятное сообщение об ошибке

**Минусы:**
- ⚠️ Требует настройки ключа в Production

## Рекомендации

1. **Немедленное исправление:** Применить Вариант 1 для быстрого исправления проблемы
2. **Долгосрочное решение:** Использовать Вариант 3 для более надежной конфигурации
3. **Унификация:** Привести все сервисы к единому подходу обработки JWT ключей
4. **Документация:** Добавить в README инструкции по настройке JWT ключей для каждого окружения

## Проверка исправления

После применения исправления:

1. Перезапустить PortfolioService
2. Проверить логи - не должно быть ошибки `IDX10703`
3. Проверить health check - должен возвращать 200 OK
4. Проверить, что JWT авторизация работает (если требуется)

## Файлы для изменения

⚠️ **ВНИМАНИЕ:** Согласно правилам проекта, PortfolioService находится вне области изменений по умолчанию. Перед внесением изменений необходимо получить подтверждение.

**Файл для изменения:**
- `src/backend/services/PortfolioService/PortfolioService.WebApi/Program.cs` (строки 48-65)

## Статус

- ✅ Проблема идентифицирована
- ✅ Исправление применено в PortfolioService.WebApi/Program.cs
- ✅ Ошибки линтера исправлены
- ⏳ Требуется перезапуск сервиса для проверки

## Примененные изменения

**Файл:** `src/backend/services/PortfolioService/PortfolioService.WebApi/Program.cs`

1. Добавлен using `Microsoft.Extensions.Logging`
2. Добавлена проверка на пустой JWT ключ
3. Добавлен дефолтный ключ для Development режима
4. Улучшена обработка пустых значений Issuer и Audience
5. Добавлено логирование предупреждения при отсутствии ключа

**Изменения:**
- Строки 48-85: Заменена логика настройки JWT с защитой от пустого ключа

