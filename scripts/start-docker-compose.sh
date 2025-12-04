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

# Глобальные переменные для логов
LOG_FILE=""
ERROR_LOG_FILE=""

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

    # Создание директории с проверкой
    if [ ! -d "$LOG_DIR" ]; then
        if ! mkdir -p "$LOG_DIR" 2>/dev/null; then
            echo -e "${RED}❌ ОШИБКА: Не удалось создать директорию для логов: $LOG_DIR${NC}" >&2
            return 1
        fi
    fi

    # Проверка, что директория существует и доступна для записи
    if [ ! -d "$LOG_DIR" ] || [ ! -w "$LOG_DIR" ]; then
        echo -e "${RED}❌ ОШИБКА: Директория для логов недоступна для записи: $LOG_DIR${NC}" >&2
        return 1
    fi

    # Генерация имени файла лога
    TIMESTAMP=$(date +"%Y-%m-%d_%H%M%S")
    LOG_FILE="$LOG_DIR/log_${TIMESTAMP}.txt"
    ERROR_LOG_FILE="$LOG_DIR/error_logs_${TIMESTAMP}.txt"

    # Создание файлов логов с проверкой
    if ! touch "$LOG_FILE" 2>/dev/null; then
        echo -e "${RED}❌ ОШИБКА: Не удалось создать файл лога: $LOG_FILE${NC}" >&2
        return 1
    fi

    if ! touch "$ERROR_LOG_FILE" 2>/dev/null; then
        echo -e "${RED}❌ ОШИБКА: Не удалось создать файл ошибок: $ERROR_LOG_FILE${NC}" >&2
        return 1
    fi

    # Запись заголовка в лог
    {
        echo "========================================"
        echo "  Stock Market Assistant"
        echo "  Docker Compose Startup Script"
        echo "  Log Level: $LOG_LEVEL"
        echo "  Started: $(date '+%Y-%m-%d %H:%M:%S')"
        echo "========================================"
        echo ""
    } > "$LOG_FILE"

    # Запись заголовка в файл ошибок
    {
        echo "========================================"
        echo "  Stock Market Assistant"
        echo "  Docker Compose Startup Script"
        echo "  Error Log"
        echo "  Started: $(date '+%Y-%m-%d %H:%M:%S')"
        echo "========================================"
        echo ""
    } > "$ERROR_LOG_FILE"

    # Проверка, что файлы созданы
    if [ ! -f "$LOG_FILE" ]; then
        echo -e "${RED}❌ ОШИБКА: Файл лога не создан: $LOG_FILE${NC}" >&2
        return 1
    fi

    if [ ! -f "$ERROR_LOG_FILE" ]; then
        echo -e "${RED}❌ ОШИБКА: Файл ошибок не создан: $ERROR_LOG_FILE${NC}" >&2
        return 1
    fi

    echo -e "${GREEN}✓ Файлы логов созданы успешно${NC}"
    return 0
}

# Функция логирования
log() {
    local level=$1
    shift
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')

    # Проверка, что файл лога существует
    if [ -z "$LOG_FILE" ] || [ ! -f "$LOG_FILE" ]; then
        return 0
    fi

    # Проверка уровня логирования
    local current_level=${LOG_LEVELS[$LOG_LEVEL]}
    local message_level=${LOG_LEVELS[$level]}

    if [ $message_level -ge $current_level ]; then
        echo "[$timestamp] [$level] $message" >> "$LOG_FILE" 2>/dev/null || true
    fi
}

# Функция записи ошибок в отдельный файл
log_error() {
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')

    # Проверка, что файл ошибок существует
    if [ -n "$ERROR_LOG_FILE" ] && [ -f "$ERROR_LOG_FILE" ]; then
        echo "[$timestamp] [ERROR] $message" >> "$ERROR_LOG_FILE" 2>/dev/null || true
    fi

    # Также записываем в общий лог
    log "ERROR" "$message"
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
if ! init_logging; then
    echo -e "${YELLOW}⚠ ПРЕДУПРЕЖДЕНИЕ: Не удалось инициализировать логирование. Продолжение без файлов логов.${NC}"
    echo ""
fi

# Очистка экрана
clear

echo -e "${CYAN}========================================"
echo -e "  Stock Market Assistant"
echo -e "  Docker Compose Startup Script"
echo -e "========================================${NC}"
if [ -n "$LOG_FILE" ]; then
    echo -e "${CYAN}Лог файл: $LOG_FILE${NC}"
fi
if [ -n "$ERROR_LOG_FILE" ]; then
    echo -e "${CYAN}Файл ошибок: $ERROR_LOG_FILE${NC}"
fi
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

    # Функция для обработки вывода docker compose
    process_docker_output() {
        while IFS= read -r line; do
            # Выводим в консоль
            echo "$line"

            # Записываем в общий лог
            if [ -n "$LOG_FILE" ] && [ -f "$LOG_FILE" ]; then
                echo "$line" >> "$LOG_FILE" 2>/dev/null || true
            fi

            # Проверяем, является ли строка ошибкой
            # Исключаем ложные срабатывания
            if echo "$line" | grep -qiE "error|ERROR|failed|FAILED|exception|EXCEPTION|timeout|TIMEOUT"; then
                # Проверяем на ложные срабатывания
                if ! echo "$line" | grep -qiE "Executed DbCommand|0 Error\(s\)|Connection refused.*kafka|brokers are down|Coordinator load in progress|retrying|INFO.*JVM arguments|\[INF\]|\[WRN\]|handshake timed out.*kafka-ui"; then
                    # Записываем в файл ошибок
                    log_error "$line"
                fi
            fi
        done
    }

    if [ ${#COMPOSE_ARGS[@]} -gt 0 ]; then
        docker compose up "${COMPOSE_ARGS[@]}" 2>&1 | process_docker_output
        EXIT_CODE=${PIPESTATUS[0]}
    else
        docker compose up 2>&1 | process_docker_output
        EXIT_CODE=${PIPESTATUS[0]}
    fi

    if [ $EXIT_CODE -eq 0 ]; then
        log "INFO" "Docker Compose завершен успешно"
    else
        log_error "Docker Compose завершен с ошибкой (код: $EXIT_CODE)"
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

