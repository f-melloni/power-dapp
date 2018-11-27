using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PowerDapp_UserArea.Migrations
{
    public partial class BlockNumberCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinConfirmations",
                table: "ExchangeRates",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BlockNumberCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BlockHash = table.Column<string>(maxLength: 100, nullable: true),
                    BlockNumber = table.Column<int>(nullable: true),
                    CurrencyCode = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockNumberCache", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockNumberCache");

            migrationBuilder.DropColumn(
                name: "MinConfirmations",
                table: "ExchangeRates");
        }
    }
}
