using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<short>(type: "smallint", nullable: false),
                    asset_ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    asset_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_price = table.Column<decimal>(type: "numeric", nullable: false),
                    asset_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    condition = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    triggered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    last_checked = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alert", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Active_AssetType_Ticker",
                table: "alert",
                columns: ["is_active", "asset_type", "asset_ticker"]);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AssetType_Ticker",
                table: "alert",
                columns: ["asset_type", "asset_ticker"]);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_IsActive",
                table: "alert",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_StockCardId",
                table: "alert",
                column: "stock_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                table: "alert",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Processed_Created",
                table: "outbox_message",
                columns: ["processed_at", "created_at"]);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Topic",
                table: "outbox_message",
                column: "topic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert");

            migrationBuilder.DropTable(
                name: "outbox_message");
        }
    }
}
