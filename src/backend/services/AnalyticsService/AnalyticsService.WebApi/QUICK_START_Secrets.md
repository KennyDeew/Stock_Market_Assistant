# Быстрый старт с секретами для Analytics Service

## 🚀 Быстрая настройка

### 1. Инициализация User Secrets

```bash
cd AnalyticsService.WebApi
dotnet user-secrets init
```

### 2. Добавление пароля базы данных

```bash
dotnet user-secrets set "Database:Password" "your-password"
```

### 3. Проверка секретов

```bash
dotnet user-secrets list
```

### 4. Запуск сервиса

```bash
dotnet run
```

## 🔑 Основные секреты

| Секрет | Команда | Описание |
|--------|---------|----------|
| `Database:Password` | `dotnet user-secrets set "Database:Password" "pass"` | Пароль PostgreSQL |
| `Kafka:SaslUsername` | `dotnet user-secrets set "Kafka:SaslUsername" "user"` | Имя пользователя Kafka |
| `Kafka:SaslPassword` | `dotnet user-secrets set "Kafka:SaslPassword" "pass"` | Пароль Kafka |
| `Redis:Password` | `dotnet user-secrets set "Redis:Password" "pass"` | Пароль Redis |

## 🌍 Переменные окружения

```bash
# Windows PowerShell
$env:ANALYTICS_DB_PASSWORD="your-password"

# Linux/macOS
export ANALYTICS_DB_PASSWORD="your-password"
```

## ✅ Проверка

При запуске сервиса в логах должно появиться:
```
info: Конфигурация базы данных загружена: Host=localhost;Port=5432;Database=analytics-db;Username=postgres;Password=***
info: Все необходимые секреты загружены успешно
```

## 📚 Подробная документация

См. [README_Secrets.md](README_Secrets.md) для полной документации.
