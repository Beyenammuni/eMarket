using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessMemberPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessMemberPermissions",
                columns: table => new
                {
                    BusinessMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessMemberPermissions", x => new { x.BusinessMemberId, x.Permission });
                    table.ForeignKey(
                        name: "FK_BusinessMemberPermissions_BusinessMembers_BusinessMemberId",
                        column: x => x.BusinessMemberId,
                        principalTable: "BusinessMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMemberPermissions_BusinessMemberId",
                table: "BusinessMemberPermissions",
                column: "BusinessMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessMemberPermissions");
        }
    }
}
