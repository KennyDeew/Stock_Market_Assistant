# Быстрый старт Stock Market Assistant

## Предварительные требования

- Docker Desktop установлен и запущен
- Git (для клонирования репозитория)

## Запуск проекта

### Windows

```powershell
# Перейти в директорию проекта
cd Stock_Market_Assistant

# Запустить все сервисы
.\scripts\start-docker-compose.ps1 -Build
```

### Linux/Mac

```bash
# Перейти в директорию проекта
cd Stock_Market_Assistant

# Сделать скрипт исполняемым (первый раз)
chmod +x scripts/start-docker-compose.sh

# Запустить все сервисы
./scripts/start-docker-compose.sh --build
```

## Доступ к сервисам

После успешного запуска сервисы будут доступны по следующим адресам:

- **Frontend:** http://localhost:5273
- **API Gateway:** http://localhost:8080
- **Swagger Gateway:** http://localhost:8080/swagger
- **OpenSearch Dashboards:** http://localhost:5601
- **Kafka UI:** http://localhost:9100

## Остановка сервисов

```bash
docker compose down
```

## Дополнительная информация

Подробная документация по скриптам запуска: [scripts/README_DOCKER_COMPOSE.md](scripts/README_DOCKER_COMPOSE.md)

