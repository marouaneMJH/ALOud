using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ALOud.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Notes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Families",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Accords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Accords");
        }
    }
}
