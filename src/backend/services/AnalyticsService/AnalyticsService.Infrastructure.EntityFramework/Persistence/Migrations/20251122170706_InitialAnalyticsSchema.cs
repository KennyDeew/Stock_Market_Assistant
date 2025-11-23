using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticsService.Infrastructure.EntityFramework.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnalyticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            // Проверяем существование таблицы asset_ratings перед созданием
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public'
                        AND table_name = 'asset_ratings'
                    ) THEN
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
                            total_buy_amount numeric(18,2) NOT NULL,
                            total_sell_amount numeric(18,2) NOT NULL,
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
                    END IF;
                END $$;
            ");

            // Проверяем существование таблицы asset_transactions перед созданием
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'public'
                        AND table_name = 'asset_transactions'
                    ) THEN
                        CREATE TABLE public.asset_transactions (
                            id uuid NOT NULL,
                            portfolio_id uuid NOT NULL,
                            stock_card_id uuid NOT NULL,
                            asset_type integer NOT NULL,
                            transaction_type integer NOT NULL,
                            quantity integer NOT NULL,
                            price_per_unit numeric(18,2) NOT NULL,
                            total_amount numeric(18,2) NOT NULL,
                            transaction_time timestamp with time zone NOT NULL,
                            currency character varying(10) NOT NULL,
                            metadata text,
                            created_at timestamp with time zone NOT NULL,
                            updated_at timestamp with time zone NOT NULL,
                            CONSTRAINT pk_asset_transactions PRIMARY KEY (id)
                        );
                    END IF;
                END $$;
            ");

            // Создаём индексы для asset_ratings с проверкой существования
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_asset_type_period_start_period_end_context') THEN
                        CREATE INDEX ix_asset_ratings_asset_type_period_start_period_end_context
                        ON public.asset_ratings (asset_type, period_start, period_end, context);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_context_portfolio_id') THEN
                        CREATE INDEX ix_asset_ratings_context_portfolio_id
                        ON public.asset_ratings (context, portfolio_id);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_context_portfolio_id_period_start_period_end') THEN
                        CREATE INDEX ix_asset_ratings_context_portfolio_id_period_start_period_end
                        ON public.asset_ratings (context, portfolio_id, period_start, period_end);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_period_start_period_end') THEN
                        CREATE INDEX ix_asset_ratings_period_start_period_end
                        ON public.asset_ratings (period_start, period_end);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_stock_card_id') THEN
                        CREATE INDEX ix_asset_ratings_stock_card_id
                        ON public.asset_ratings (stock_card_id);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_stock_card_id_period_start_period_end_context_portfolio_id') THEN
                        CREATE INDEX ix_asset_ratings_stock_card_id_period_start_period_end_context_portfolio_id
                        ON public.asset_ratings (stock_card_id, period_start, period_end, context, portfolio_id);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_transaction_amount_rank_context_period_start_period_end') THEN
                        CREATE INDEX ix_asset_ratings_transaction_amount_rank_context_period_start_period_end
                        ON public.asset_ratings (transaction_amount_rank, context, period_start, period_end);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_ratings_transaction_count_rank_context_period_start_period_end') THEN
                        CREATE INDEX ix_asset_ratings_transaction_count_rank_context_period_start_period_end
                        ON public.asset_ratings (transaction_count_rank, context, period_start, period_end);
                    END IF;
                END $$;
            ");

            // Создаём индексы для asset_transactions с проверкой существования
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_asset_type_transaction_time') THEN
                        CREATE INDEX ix_asset_transactions_asset_type_transaction_time
                        ON public.asset_transactions (asset_type, transaction_time);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_portfolio_id') THEN
                        CREATE INDEX ix_asset_transactions_portfolio_id
                        ON public.asset_transactions (portfolio_id);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_portfolio_id_transaction_time') THEN
                        CREATE INDEX ix_asset_transactions_portfolio_id_transaction_time
                        ON public.asset_transactions (portfolio_id, transaction_time);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_stock_card_id') THEN
                        CREATE INDEX ix_asset_transactions_stock_card_id
                        ON public.asset_transactions (stock_card_id);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_stock_card_id_transaction_time') THEN
                        CREATE INDEX ix_asset_transactions_stock_card_id_transaction_time
                        ON public.asset_transactions (stock_card_id, transaction_time);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_transaction_time') THEN
                        CREATE INDEX ix_asset_transactions_transaction_time
                        ON public.asset_transactions (transaction_time);
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_asset_transactions_transaction_type_transaction_time') THEN
                        CREATE INDEX ix_asset_transactions_transaction_type_transaction_time
                        ON public.asset_transactions (transaction_type, transaction_time);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_ratings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "asset_transactions",
                schema: "public");
        }
    }
}
