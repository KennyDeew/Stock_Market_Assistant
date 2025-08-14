-- Скрипт для переноса таблиц из схемы analytics в схему public
-- и удаления схемы analytics

-- 1. Создаем таблицы в схеме public
CREATE TABLE public.asset_transactions (
    id uuid NOT NULL,
    portfolio_id uuid NOT NULL,
    stock_card_id uuid NOT NULL,
    asset_type integer NOT NULL,
    transaction_type integer NOT NULL,
    quantity integer NOT NULL,
    price_per_unit numeric(18,4) NOT NULL,
    total_amount numeric(18,4) NOT NULL,
    transaction_time timestamp with time zone NOT NULL,
    currency character varying(3) NOT NULL,
    metadata text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_asset_transactions PRIMARY KEY (id)
);

CREATE TABLE public.asset_ratings (
    id uuid NOT NULL,
    stock_card_id uuid NOT NULL,
    asset_type integer NOT NULL,
    ticker character varying(20) NOT NULL,
    name character varying(255) NOT NULL,
    period_start timestamp with time zone NOT NULL,
    period_end timestamp with time zone NOT NULL,
    buy_transaction_count integer NOT NULL,
    sell_transaction_count integer NOT NULL,
    total_buy_amount numeric(18,4) NOT NULL,
    total_sell_amount numeric(18,4) NOT NULL,
    total_buy_quantity integer NOT NULL,
    total_sell_quantity integer NOT NULL,
    transaction_count_rank integer NOT NULL,
    transaction_amount_rank integer NOT NULL,
    last_updated timestamp with time zone NOT NULL,
    context integer NOT NULL,
    portfolio_id uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_asset_ratings PRIMARY KEY (id)
);

-- 2. Создаем индексы для транзакций в схеме public
CREATE INDEX ix_asset_transactions_portfolio_id_transaction_time ON public.asset_transactions (portfolio_id, transaction_time);
CREATE INDEX ix_asset_transactions_stock_card_id_transaction_time ON public.asset_transactions (stock_card_id, transaction_time);
CREATE INDEX ix_asset_transactions_asset_type_transaction_time ON public.asset_transactions (asset_type, transaction_time);
CREATE INDEX ix_asset_transactions_transaction_type_transaction_time ON public.asset_transactions (transaction_type, transaction_time);
CREATE INDEX ix_asset_transactions_transaction_time ON public.asset_transactions (transaction_time);

-- 3. Создаем индексы для рейтингов в схеме public
CREATE INDEX ix_asset_ratings_stock_card_id_period_start_period_end_context_portfolio_id ON public.asset_ratings (stock_card_id, period_start, period_end, context, portfolio_id);
CREATE INDEX ix_asset_ratings_asset_type_period_start_period_end_context ON public.asset_ratings (asset_type, period_start, period_end, context);
CREATE INDEX ix_asset_ratings_context_portfolio_id_period_start_period_end ON public.asset_ratings (context, portfolio_id, period_start, period_end);
CREATE INDEX ix_asset_ratings_transaction_count_rank_context_period_start_period_end ON public.asset_ratings (transaction_count_rank, context, period_start, period_end);
CREATE INDEX ix_asset_ratings_transaction_amount_rank_context_period_start_period_end ON public.asset_ratings (transaction_amount_rank, context, period_start, period_end);

-- 4. Копируем данные из схемы analytics в схему public (если таблицы существуют)
-- Проверяем существование таблиц перед копированием
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'analytics' AND table_name = 'asset_transactions') THEN
        INSERT INTO public.asset_transactions 
        SELECT * FROM analytics.asset_transactions;
    END IF;
    
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'analytics' AND table_name = 'asset_ratings') THEN
        INSERT INTO public.asset_ratings 
        SELECT * FROM analytics.asset_ratings;
    END IF;
END $$;

-- 5. Удаляем таблицы из схемы analytics (если они существуют)
DROP TABLE IF EXISTS analytics.asset_transactions CASCADE;
DROP TABLE IF EXISTS analytics.asset_ratings CASCADE;

-- 6. Удаляем схему analytics (если она существует и пуста)
DROP SCHEMA IF EXISTS analytics CASCADE;

-- Проверяем результат
SELECT 
    schemaname,
    tablename,
    tableowner
FROM pg_tables 
WHERE schemaname IN ('public', 'analytics')
ORDER BY schemaname, tablename;
