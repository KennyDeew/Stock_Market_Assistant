# Миграция: Перенос таблиц из схемы analytics в схему public

## Описание

Эта миграция переносит все таблицы и данные из схемы `analytics` в схему `public` (схема по умолчанию PostgreSQL) и удаляет схему `analytics`.

## Что происходит

1. **Создание таблиц в схеме public:**
   - `asset_transactions` - транзакции с активами
   - `asset_ratings` - рейтинги активов

2. **Создание всех необходимых индексов** для оптимизации запросов

3. **Копирование данных** из схемы `analytics` в схему `public`

4. **Удаление таблиц** из схемы `analytics`

5. **Удаление схемы** `analytics`

## Выполнение миграции

### Способ 1: Через Entity Framework (рекомендуется)

```bash
# В директории AnalyticsService.Infrastructure.EntityFramework
dotnet ef database update --startup-project ../AnalyticsService.WebApi
```

### Способ 2: Ручное выполнение SQL

Если миграция через EF не работает, выполните SQL-скрипт `MoveTablesToPublicSchema.sql` в базе данных.

## Откат миграции

### Через Entity Framework

```bash
dotnet ef database update 20250101000000_InitialCreate --startup-project ../AnalyticsService.WebApi
```

### Ручной откат

Выполните команды в обратном порядке:
1. Восстановите схему `analytics`
2. Создайте таблицы в схеме `analytics`
3. Скопируйте данные обратно
4. Удалите таблицы из схемы `public`

## Проверка результата

После выполнения миграции проверьте:

```sql
-- Проверка таблиц в схеме public
SELECT schemaname, tablename 
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('asset_transactions', 'asset_ratings');

-- Проверка отсутствия схемы analytics
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name = 'analytics';

-- Проверка данных
SELECT COUNT(*) FROM public.asset_transactions;
SELECT COUNT(*) FROM public.asset_ratings;
```

## Важные замечания

1. **Резервное копирование:** Перед выполнением миграции обязательно создайте резервную копию базы данных
2. **Время выполнения:** Миграция может занять время в зависимости от объема данных
3. **Блокировки:** Во время миграции таблицы могут быть заблокированы для записи
4. **Тестирование:** Протестируйте миграцию на тестовой базе данных перед применением на продакшене

## Возможные проблемы

1. **Ошибка "schema does not exist":** Схема `analytics` уже удалена - это нормально
2. **Ошибка "table does not exist":** Таблицы в схеме `analytics` уже удалены - это нормально
3. **Ошибка "duplicate key":** Возможно, данные уже существуют в схеме `public`

## Поддержка

При возникновении проблем обратитесь к команде разработки или создайте issue в репозитории.
