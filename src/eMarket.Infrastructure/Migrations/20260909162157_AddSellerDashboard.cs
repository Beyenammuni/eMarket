using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessMembers_BusinessId",
                table: "BusinessMembers");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_BusinessId_Role_IsActive",
                table: "BusinessMembers",
                columns: new[] { "BusinessId", "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_BusinessId_UserId",
                table: "BusinessMembers",
                columns: new[] { "BusinessId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessMembers_BusinessId_Role_IsActive",
                table: "BusinessMembers");

            migrationBuilder.DropIndex(
                name: "IX_BusinessMembers_BusinessId_UserId",
                table: "BusinessMembers");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Businesses",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_BusinessId",
                table: "BusinessMembers",
                column: "BusinessId");
        }
    }
}
