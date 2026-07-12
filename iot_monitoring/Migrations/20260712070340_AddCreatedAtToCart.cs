using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iot_monitoring.Migrations
{
    public partial class AddCreatedAtToCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Carts",
                newName: "CreatedAt");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Carts",
                newName: "CreateAt");
        }
    }
}
