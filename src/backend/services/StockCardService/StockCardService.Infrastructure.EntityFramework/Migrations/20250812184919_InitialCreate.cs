using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockCardService.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BondCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MaturityPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BondCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    EBITDA = table.Column<decimal>(type: "numeric", nullable: false),
                    NetProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    CAPEX = table.Column<decimal>(type: "numeric", nullable: false),
                    FCF = table.Column<decimal>(type: "numeric", nullable: false),
                    Debt = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShareCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttOffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_BondCards_ParentId",
                        column: x => x.ParentId,
                        principalTable: "BondCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dividends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttOffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dividends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dividends_ShareCards_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ShareCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Multipliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Multipliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Multipliers_ShareCards_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ShareCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ParentId",
                table: "Coupons",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dividends_ParentId",
                table: "Dividends",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Multipliers_ParentId",
                table: "Multipliers",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "CryptoCards");

            migrationBuilder.DropTable(
                name: "Dividends");

            migrationBuilder.DropTable(
                name: "FinancialReports");

            migrationBuilder.DropTable(
                name: "Multipliers");

            migrationBuilder.DropTable(
                name: "BondCards");

            migrationBuilder.DropTable(
                name: "ShareCards");
        }
    }
}
