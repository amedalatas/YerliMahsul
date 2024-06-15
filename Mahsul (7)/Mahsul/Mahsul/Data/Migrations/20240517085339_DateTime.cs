using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mahsul.Data.Migrations
{
    public partial class DateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Product",
                type: "datetime2",
                nullable: false,
                defaultValue: System.DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Product",
                type: "datetime2",
                nullable: false,
                defaultValue: System.DateTime.UtcNow.AddMonths(1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Product");
        }
    }
}
