using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ALOud.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfumePriceStockDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Perfumes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Perfumes",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Perfumes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Perfumes");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Perfumes");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Perfumes");
        }
    }
}
