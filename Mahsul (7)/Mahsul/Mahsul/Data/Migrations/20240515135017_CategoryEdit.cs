using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mahsul.Data.Migrations
{
    public partial class CategoryEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_Product_Productid",
                table: "categories");

            migrationBuilder.AlterColumn<int>(
                name: "Productid",
                table: "categories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_Product_Productid",
                table: "categories",
                column: "Productid",
                principalTable: "Product",
                principalColumn: "ProductID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_Product_Productid",
                table: "categories");

            migrationBuilder.AlterColumn<int>(
                name: "Productid",
                table: "categories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_categories_Product_Productid",
                table: "categories",
                column: "Productid",
                principalTable: "Product",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
