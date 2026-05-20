using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleParts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vendors");
        }
    }
}
