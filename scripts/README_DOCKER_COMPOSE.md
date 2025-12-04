# Скрипты запуска Docker Compose

## Описание

Скрипты для автоматического запуска всех сервисов проекта Stock Market Assistant через Docker Compose с предварительной проверкой готовности Docker.

## Доступные скрипты

### Windows (PowerShell)

**Файл:** `start-docker-compose.ps1`

**Использование:**
```powershell
# Базовый запуск (интерактивный режим)
.\scripts\start-docker-compose.ps1

# С пересборкой образов
.\scripts\start-docker-compose.ps1 -Build

# В фоновом режиме (detached)
.\scripts\start-docker-compose.ps1 -Detached

# С пересборкой и в фоновом режиме
.\scripts\start-docker-compose.ps1 -Build -Detached

# Принудительная остановка существующих контейнеров перед запуском
.\scripts\start-docker-compose.ps1 -Force
```

### Linux/Mac (Bash)

**Файл:** `start-docker-compose.sh`

**Использование:**
```bash
# Сделать скрипт исполняемым (первый раз)
chmod +x scripts/start-docker-compose.sh

# Базовый запуск (интерактивный режим)
./scripts/start-docker-compose.sh

# С пересборкой образов
./scripts/start-docker-compose.sh --build

# В фоновом режиме (detached)
./scripts/start-docker-compose.sh --detached

# С пересборкой и в фоновом режиме
./scripts/start-docker-compose.sh --build --detached

# Принудительная остановка существующих контейнеров перед запуском
./scripts/start-docker-compose.sh --force
```

## Функциональность

### Проверки перед запуском

1. **Проверка наличия Docker CLI**
   - Проверяет, что команда `docker` доступна в PATH
   - Выводит ошибку с инструкцией по установке, если Docker не найден

2. **Проверка Docker daemon**
   - Проверяет, что Docker daemon запущен и доступен
   - Проверяет версию Docker
   - Проверяет доступность Docker API

3. **Проверка портов**
   - Проверяет доступность портов: 8080, 8081, 8082, 8083, 8084, 8085, 5273, 6379, 9092, 9200, 5601
   - Предупреждает о занятых портах
   - Запрашивает подтверждение перед продолжением (если не используется `--force`)

4. **Остановка существующих контейнеров** (опционально)
   - При использовании флага `--force` автоматически останавливает существующие контейнеры

### Параметры запуска

| Параметр | Описание |
|----------|----------|
| `--build` / `-Build` | Пересобрать Docker образы перед запуском |
| `--detached` / `-Detached` | Запустить контейнеры в фоновом режиме (не блокировать терминал) |
| `--force` / `-Force` | Принудительно остановить существующие контейнеры перед запуском |

## Примеры использования

### Первый запуск проекта

```powershell
# Windows
.\scripts\start-docker-compose.ps1 -Build
```

```bash
# Linux/Mac
./scripts/start-docker-compose.sh --build
```

### Обычный запуск (после первого раза)

```powershell
# Windows
.\scripts\start-docker-compose.ps1
```

```bash
# Linux/Mac
./scripts/start-docker-compose.sh
```

### Запуск в фоновом режиме для разработки

```powershell
# Windows
.\scripts\start-docker-compose.ps1 -Detached
```

```bash
# Linux/Mac
./scripts/start-docker-compose.sh --detached
```

### Пересборка после изменений в коде

```powershell
# Windows
.\scripts\start-docker-compose.ps1 -Build -Force
```

```bash
# Linux/Mac
./scripts/start-docker-compose.sh --build --force
```

## Структура запускаемых сервисов

После успешного запуска будут доступны следующие сервисы:

| Сервис | Порт | Описание |
|--------|------|----------|
| Gateway | 8080 | API Gateway (YARP) - единая точка входа |
| AuthService | 8081 | Сервис аутентификации |
| StockCardService | 8082 | Сервис карточек активов |
| PortfolioService | 8083 | Сервис портфелей |
| AnalyticsService | 8084 | Сервис аналитики |
| NotificationService | 8085 | Сервис уведомлений |
| Frontend | 5273 | React приложение |
| Redis | 6379 | Кэш |
| Kafka | 9092 | Message broker |
| OpenSearch | 9200 | Поиск и логирование |
| OpenSearch Dashboards | 5601 | Дашборды |

## Управление контейнерами

### Просмотр логов

```bash
# Все сервисы
docker compose logs -f

# Конкретный сервис
docker compose logs -f gateway-api
docker compose logs -f portfolioservice-api
```

### Остановка сервисов

```bash
# Остановить все сервисы
docker compose down

# Остановить с удалением volumes
docker compose down -v
```

### Перезапуск конкретного сервиса

```bash
docker compose restart gateway-api
```

## Устранение проблем

### Docker не запускается

1. Убедитесь, что Docker Desktop запущен
2. Проверьте, что Docker daemon доступен: `docker version`
3. Перезапустите Docker Desktop

### Порт занят

1. Используйте флаг `--force` для принудительной остановки
2. Или вручную остановите процесс, использующий порт:
   ```powershell
   # Windows - найти процесс на порту 8080
   netstat -ano | findstr :8080
   ```

### Ошибки при сборке

1. Убедитесь, что все зависимости установлены
2. Проверьте логи сборки: `docker compose build`
3. Очистите кэш Docker: `docker system prune -a`

## Дополнительные команды

### Просмотр статуса контейнеров

```bash
docker compose ps
```

### Просмотр использования ресурсов

```bash
docker stats
```

### Вход в контейнер

```bash
docker compose exec gateway-api sh
```

## Примечания

- Скрипты автоматически определяют корневую директорию проекта
- Все проверки выполняются перед запуском docker-compose
- При ошибках скрипт завершается с кодом выхода 1
- В интерактивном режиме логи выводятся в реальном времени
- В фоновом режиме (`-d`) контейнеры запускаются без блокировки терминала

