using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioAssetTransactionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "average_purchase_price",
                table: "portfolio_asset");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "portfolio_asset");

            migrationBuilder.DropColumn(
                name: "last_updated",
                table: "portfolio_asset");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "portfolio_asset");

            migrationBuilder.CreateTable(
                name: "portfolio_asset_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<short>(type: "smallint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_asset_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolio_asset_transaction_portfolio_asset_portfolio_asset",
                        column: x => x.portfolio_asset_id,
                        principalTable: "portfolio_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_asset_transaction_portfolio_asset_id",
                table: "portfolio_asset_transaction",
                column: "portfolio_asset_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolio_asset_transaction");

            migrationBuilder.AddColumn<decimal>(
                name: "average_purchase_price",
                table: "portfolio_asset",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "portfolio_asset",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated",
                table: "portfolio_asset",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "portfolio_asset",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
