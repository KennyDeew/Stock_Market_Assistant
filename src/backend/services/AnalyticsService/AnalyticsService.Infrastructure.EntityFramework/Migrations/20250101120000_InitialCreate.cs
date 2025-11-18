using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_transactions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<int>(type: "integer", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "numeric(18,0)", nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_ratings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<int>(type: "integer", nullable: false),
                    ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    buy_transaction_count = table.Column<int>(type: "integer", nullable: false),
                    sell_transaction_count = table.Column<int>(type: "integer", nullable: false),
                    total_buy_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_sell_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_buy_quantity = table.Column<int>(type: "numeric(18,0)", nullable: false),
                    total_sell_quantity = table.Column<int>(type: "numeric(18,0)", nullable: false),
                    transaction_count_rank = table.Column<int>(type: "integer", nullable: false),
                    transaction_amount_rank = table.Column<int>(type: "integer", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    context = table.Column<int>(type: "integer", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_ratings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asset_transactions_portfolio_id_transaction_time",
                table: "asset_transactions",
                columns: new[] { "portfolio_id", "transaction_time" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_transactions_stock_card_id_transaction_time",
                table: "asset_transactions",
                columns: new[] { "stock_card_id", "transaction_time" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_transactions_asset_type_transaction_time",
                table: "asset_transactions",
                columns: new[] { "asset_type", "transaction_time" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_transactions_transaction_type_transaction_time",
                table: "asset_transactions",
                columns: new[] { "transaction_type", "transaction_time" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_transactions_transaction_time",
                table: "asset_transactions",
                column: "transaction_time");

            migrationBuilder.CreateIndex(
                name: "ix_asset_ratings_stock_card_id_period_start_period_end_context_portfolio_id",
                table: "asset_ratings",
                columns: new[] { "stock_card_id", "period_start", "period_end", "context", "portfolio_id" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_ratings_asset_type_period_start_period_end_context",
                table: "asset_ratings",
                columns: new[] { "asset_type", "period_start", "period_end", "context" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_ratings_context_portfolio_id_period_start_period_end",
                table: "asset_ratings",
                columns: new[] { "context", "portfolio_id", "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_ratings_transaction_count_rank_context_period_start_period_end",
                table: "asset_ratings",
                columns: new[] { "transaction_count_rank", "context", "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_ratings_transaction_amount_rank_context_period_start_period_end",
                table: "asset_ratings",
                columns: new[] { "transaction_amount_rank", "context", "period_start", "period_end" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_transactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "asset_ratings",
                schema: "public");
        }
    }
}

