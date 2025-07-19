using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portfolio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_asset",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<short>(type: "smallint", nullable: false),
                    stock_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    average_purchase_price = table.Column<decimal>(type: "numeric", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_asset", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolio_asset_portfolio_portfolio_id",
                        column: x => x.portfolio_id,
                        principalTable: "portfolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_asset_portfolio_id",
                table: "portfolio_asset",
                column: "portfolio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolio_asset");

            migrationBuilder.DropTable(
                name: "portfolio");
        }
    }
}
