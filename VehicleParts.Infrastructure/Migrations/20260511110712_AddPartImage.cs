using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Parts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Parts");
        }
    }
}
