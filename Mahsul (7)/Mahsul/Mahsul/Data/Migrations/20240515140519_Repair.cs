using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahsul.Data.Migrations
{
    public partial class Repair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedCategoryId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedCategoryId",
                table: "Product");
        }
    }
}
