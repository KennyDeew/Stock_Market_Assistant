# Управление тестовыми данными через API

API предоставляет эндпоинты для управления тестовыми данными базы данных аналитики через Swagger UI.

## Доступные эндпоинты

### 1. Очистка базы данных

**POST** `/api/test-data/clear`

Удаляет все транзакции и рейтинги из базы данных аналитики.

**Ответ:**
```json
{
  "success": true,
  "message": "База данных успешно очищена. Удалено транзакций: 150, рейтингов: 25",
  "transactionsDeleted": 150,
  "ratingsDeleted": 25
}
```

### 2. Заполнение базы данных тестовыми данными

**POST** `/api/test-data/seed`

Заполняет базу данных тестовыми данными.

**Тело запроса (опционально):**
```json
{
  "portfolioCount": 4,
  "overlapCount": 3,
  "daysBack": 90
}
```

**Параметры:**
- `portfolioCount` (int, по умолчанию: 4) - Количество портфелей для создания (1-10)
- `overlapCount` (int, по умолчанию: 3) - Количество портфелей с пересекающимися активами (0-`portfolioCount`)
- `daysBack` (int, по умолчанию: 90) - Количество дней назад для генерации транзакций (1-365)

**Ответ:**
```json
{
  "success": true,
  "message": "База данных успешно заполнена тестовыми данными",
  "companiesCreated": 7,
  "portfoliosCreated": 4,
  "transactionsCreated": 180,
  "buyTransactionsCount": 126,
  "sellTransactionsCount": 54
}
```

### 3. Сброс базы данных (очистка + заполнение)

**POST** `/api/test-data/reset`

Комбинированная операция: сначала очищает базу данных, затем заполняет тестовыми данными.

**Тело запроса (опционально):**
```json
{
  "portfolioCount": 4,
  "overlapCount": 3,
  "daysBack": 90
}
```

**Ответ:**
```json
{
  "success": true,
  "message": "База данных сброшена и заполнена. Удалено транзакций: 150, рейтингов: 25. База данных успешно заполнена тестовыми данными",
  "companiesCreated": 7,
  "portfoliosCreated": 4,
  "transactionsCreated": 180,
  "buyTransactionsCount": 126,
  "sellTransactionsCount": 54,
  "transactionsDeleted": 150,
  "ratingsDeleted": 25
}
```

## Использование через Swagger UI

1. Запустите приложение AnalyticsService.WebApi
2. Откройте Swagger UI: `https://localhost:<port>/swagger` (или `http://localhost:<port>/swagger` в Development)
3. Найдите раздел **"Test Data Management"**
4. Нажмите на нужный эндпоинт
5. Нажмите **"Try it out"**
6. При необходимости заполните параметры запроса
7. Нажмите **"Execute"**

## Авторизация

Все эндпоинты требуют JWT авторизации. В Swagger UI:
1. Нажмите кнопку **"Authorize"** в верхней части страницы
2. Введите JWT токен в формате: `Bearer <your-token>`
3. Нажмите **"Authorize"**

## Генерируемые данные

### Компании (7 штук):
- SBER - ПАО Сбербанк
- ALFA - ПАО Альфа-Банк
- RSHB - ПАО Россельхозбанк
- TCSG - ПАО Тинькофф Банк
- GAZP - ПАО Газпром
- ROSN - ПАО Роснефть
- TATN - ПАО Татнефть

### Портфели:
- Создается указанное количество портфелей (по умолчанию: 4)
- В каждом портфеле 3-7 активов
- Указанное количество портфелей (по умолчанию: 3) имеют пересекающиеся активы

### Транзакции:
- По 5-15 транзакций на каждый актив
- За указанный период (по умолчанию: последние 90 дней)
- 70% покупок, 30% продаж
- Случайные цены в диапазоне компании
- Количество акций: 10-1000 штук на транзакцию

## Примеры использования

### Очистка базы данных
```bash
curl -X POST "https://localhost:5001/api/test-data/clear" \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json"
```

### Заполнение с параметрами по умолчанию
```bash
curl -X POST "https://localhost:5001/api/test-data/seed" \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json"
```

### Заполнение с кастомными параметрами
```bash
curl -X POST "https://localhost:5001/api/test-data/seed" \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "portfolioCount": 5,
    "overlapCount": 2,
    "daysBack": 60
  }'
```

### Сброс базы данных
```bash
curl -X POST "https://localhost:5001/api/test-data/reset" \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "portfolioCount": 4,
    "overlapCount": 3,
    "daysBack": 90
  }'
```


