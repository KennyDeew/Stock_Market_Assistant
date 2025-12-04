#!/bin/bash

# Скрипт запуска Docker Compose для Stock Market Assistant
# Проверяет готовность Docker и запускает все сервисы

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Параметры по умолчанию
BUILD=false
DETACHED=false
FORCE=false
LOG_LEVEL="ERROR"  # DEBUG, INFO, WARNING, ERROR, CRITICAL

# Уровни логирования (по возрастанию важности)
declare -A LOG_LEVELS=(
    ["DEBUG"]=0
    ["INFO"]=1
    ["WARNING"]=2
    ["ERROR"]=3
    ["CRITICAL"]=4
)

# Инициализация логирования
init_logging() {
    # Создание директории для логов
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
    LOG_DIR="$PROJECT_ROOT/build_log"

    if [ ! -d "$LOG_DIR" ]; then
        mkdir -p "$LOG_DIR"
    fi

    # Генерация имени файла лога
    TIMESTAMP=$(date +"%Y-%m-%d_%H%M%S")
    LOG_FILE="$LOG_DIR/log_${TIMESTAMP}.txt"

    # Запись заголовка в лог
    echo "========================================" >> "$LOG_FILE"
    echo "  Stock Market Assistant" >> "$LOG_FILE"
    echo "  Docker Compose Startup Script" >> "$LOG_FILE"
    echo "  Log Level: $LOG_LEVEL" >> "$LOG_FILE"
    echo "  Started: $(date '+%Y-%m-%d %H:%M:%S')" >> "$LOG_FILE"
    echo "========================================" >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
}

# Функция логирования
log() {
    local level=$1
    shift
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')

    # Проверка уровня логирования
    local current_level=${LOG_LEVELS[$LOG_LEVEL]}
    local message_level=${LOG_LEVELS[$level]}

    if [ $message_level -ge $current_level ]; then
        echo "[$timestamp] [$level] $message" >> "$LOG_FILE"
    fi
}

# Парсинг аргументов
while [[ $# -gt 0 ]]; do
    case $1 in
        --build|-b)
            BUILD=true
            shift
            ;;
        --detached|-d)
            DETACHED=true
            shift
            ;;
        --force|-f)
            FORCE=true
            shift
            ;;
        --log-level|-l)
            LOG_LEVEL="$2"
            if [[ ! -v LOG_LEVELS[$LOG_LEVEL] ]]; then
                echo "Ошибка: Неверный уровень логирования: $LOG_LEVEL"
                echo "Допустимые значения: DEBUG, INFO, WARNING, ERROR, CRITICAL"
                exit 1
            fi
            shift 2
            ;;
        *)
            echo "Неизвестный параметр: $1"
            echo "Использование: $0 [--build] [--detached] [--force] [--log-level LEVEL]"
            echo "  Уровни логирования: DEBUG, INFO, WARNING, ERROR, CRITICAL"
            echo "  По умолчанию: ERROR"
            exit 1
            ;;
    esac
done

# Инициализация логирования
init_logging

# Очистка экрана
clear

echo -e "${CYAN}========================================"
echo -e "  Stock Market Assistant"
echo -e "  Docker Compose Startup Script"
echo -e "========================================${NC}"
echo -e "${CYAN}Лог файл: $LOG_FILE${NC}"
echo ""
log "INFO" "Скрипт запущен. Уровень логирования: $LOG_LEVEL"

# Функция проверки команды
check_command() {
    command -v "$1" >/dev/null 2>&1
}

# Функция проверки Docker
check_docker_ready() {
    echo -e "${YELLOW}Проверка готовности Docker...${NC}"

    # Проверка наличия Docker
    if ! check_command docker; then
        local error_msg="Docker не установлен или не найден в PATH"
        echo -e "${RED}❌ ОШИБКА: $error_msg${NC}"
        echo -e "${YELLOW}   Установите Docker: https://www.docker.com/get-started${NC}"
        log "CRITICAL" "$error_msg"
        exit 1
    fi

    echo -e "${GREEN}✓ Docker найден${NC}"
    log "INFO" "Docker найден в системе"

    # Проверка наличия docker-compose
    if ! check_command docker; then
        local error_msg="docker compose не доступен"
        echo -e "${RED}❌ ОШИБКА: $error_msg${NC}"
        log "CRITICAL" "$error_msg"
        exit 1
    fi

    echo -e "${GREEN}✓ docker compose найден${NC}"
    log "INFO" "docker compose найден"

    # Проверка, что Docker daemon запущен
    if ! docker version >/dev/null 2>&1; then
        local error_msg="Docker daemon не запущен"
        echo -e "${RED}❌ ОШИБКА: $error_msg${NC}"
        echo -e "${YELLOW}   Запустите Docker и повторите попытку${NC}"
        log "CRITICAL" "$error_msg"
        exit 1
    fi

    DOCKER_VERSION=$(docker version --format '{{.Server.Version}}' 2>/dev/null)
    echo -e "${GREEN}✓ Docker daemon запущен (версия: $DOCKER_VERSION)${NC}"
    log "INFO" "Docker daemon запущен (версия: $DOCKER_VERSION)"

    # Проверка доступности Docker API
    if ! docker info >/dev/null 2>&1; then
        local error_msg="Docker API недоступен"
        echo -e "${RED}❌ ОШИБКА: $error_msg${NC}"
        log "CRITICAL" "$error_msg"
        exit 1
    fi

    echo -e "${GREEN}✓ Docker API доступен${NC}"
    log "INFO" "Docker API доступен"
    echo ""
}

# Функция проверки портов
check_ports_available() {
    echo -e "${YELLOW}Проверка доступности портов...${NC}"

    PORTS=(8080 8081 8082 8083 8084 8085 5273 6379 9092 9200 5601)
    OCCUPIED_PORTS=()

    for port in "${PORTS[@]}"; do
        if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1 || netstat -an 2>/dev/null | grep -q ":$port.*LISTEN"; then
            OCCUPIED_PORTS+=($port)
        fi
    done

    if [ ${#OCCUPIED_PORTS[@]} -gt 0 ]; then
        local warning_msg="Следующие порты заняты: ${OCCUPIED_PORTS[*]}"
        echo -e "${YELLOW}⚠ ПРЕДУПРЕЖДЕНИЕ: Следующие порты заняты:${NC}"
        for port in "${OCCUPIED_PORTS[@]}"; do
            echo -e "${YELLOW}   - Порт $port${NC}"
            log "WARNING" "Порт $port занят"
        done
        echo ""

        if [ "$FORCE" = false ]; then
            read -p "Продолжить запуск? (y/N): " response
            if [ "$response" != "y" ] && [ "$response" != "Y" ]; then
                echo -e "${YELLOW}Запуск отменен${NC}"
                log "INFO" "Запуск отменен пользователем"
                exit 0
            fi
        fi
        log "WARNING" "$warning_msg - продолжение запуска"
    else
        echo -e "${GREEN}✓ Все необходимые порты свободны${NC}"
        log "INFO" "Все необходимые порты свободны"
    fi

    echo ""
}

# Функция остановки существующих контейнеров
stop_existing_containers() {
    if [ "$FORCE" = true ]; then
        echo -e "${YELLOW}Остановка существующих контейнеров...${NC}"
        log "INFO" "Остановка существующих контейнеров (режим --force)"
        docker compose down >/dev/null 2>&1
        echo -e "${GREEN}✓ Существующие контейнеры остановлены${NC}"
        log "INFO" "Существующие контейнеры остановлены"
        echo ""
    fi
}

# Основная логика
main() {
    # Проверка Docker
    check_docker_ready

    # Проверка портов
    check_ports_available

    # Остановка существующих контейнеров при необходимости
    stop_existing_containers

    # Определение параметров запуска
    COMPOSE_ARGS=()

    if [ "$BUILD" = true ]; then
        COMPOSE_ARGS+=("--build")
        echo -e "${CYAN}Режим: Пересборка образов${NC}"
        log "INFO" "Режим: Пересборка образов"
    fi

    if [ "$DETACHED" = true ]; then
        COMPOSE_ARGS+=("-d")
        echo -e "${CYAN}Режим: Запуск в фоновом режиме${NC}"
        log "INFO" "Режим: Запуск в фоновом режиме"
    else
        echo -e "${CYAN}Режим: Запуск в интерактивном режиме${NC}"
        log "INFO" "Режим: Запуск в интерактивном режиме"
    fi

    echo ""
    echo -e "${GREEN}Запуск Docker Compose...${NC}"
    echo -e "${CYAN}========================================${NC}"
    echo ""
    log "INFO" "Запуск Docker Compose с параметрами: ${COMPOSE_ARGS[*]}"

    # Переход в корневую директорию проекта
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
    cd "$PROJECT_ROOT"

    # Запуск docker compose
    log "INFO" "Выполнение команды: docker compose up ${COMPOSE_ARGS[*]}"
    if [ ${#COMPOSE_ARGS[@]} -gt 0 ]; then
        docker compose up "${COMPOSE_ARGS[@]}" 2>&1 | tee -a "$LOG_FILE"
        EXIT_CODE=${PIPESTATUS[0]}
    else
        docker compose up 2>&1 | tee -a "$LOG_FILE"
        EXIT_CODE=${PIPESTATUS[0]}
    fi

    if [ $EXIT_CODE -eq 0 ]; then
        log "INFO" "Docker Compose завершен успешно"
    else
        log "ERROR" "Docker Compose завершен с ошибкой (код: $EXIT_CODE)"
    fi

    return $EXIT_CODE
}

# Обработка ошибок
trap 'if [ $? -ne 0 ]; then log "CRITICAL" "Критическая ошибка в скрипте (код: $?)"; echo -e "\n${RED}❌ КРИТИЧЕСКАЯ ОШИБКА${NC}"; fi' ERR

# Запуск основной функции
main
EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
    log "ERROR" "Скрипт завершен с ошибкой (код: $EXIT_CODE)"
else
    log "INFO" "Скрипт завершен успешно"
fi

exit $EXIT_CODE

