using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrivateFieldToPortfolioEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_private",
                table: "portfolio",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_private",
                table: "portfolio");
        }
    }
}
