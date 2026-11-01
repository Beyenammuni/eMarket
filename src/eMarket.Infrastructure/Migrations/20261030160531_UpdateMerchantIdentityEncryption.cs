using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMerchantIdentityEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdentityNumber",
                table: "Businesses",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeFullName",
                table: "Businesses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeIdentityNumber",
                table: "Businesses",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SellerPlanPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerPlanSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProviderToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerPlanPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerPlanPayments_SellerPlanSubscriptions_SellerPlanSubscriptionId",
                        column: x => x.SellerPlanSubscriptionId,
                        principalTable: "SellerPlanSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellerPlanPayments_SellerPlans_SellerPlanId",
                        column: x => x.SellerPlanId,
                        principalTable: "SellerPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellerPlanPayments_BusinessId",
                table: "SellerPlanPayments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerPlanPayments_ProviderToken",
                table: "SellerPlanPayments",
                column: "ProviderToken",
                unique: true,
                filter: "[ProviderToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SellerPlanPayments_SellerPlanId",
                table: "SellerPlanPayments",
                column: "SellerPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerPlanPayments_SellerPlanSubscriptionId",
                table: "SellerPlanPayments",
                column: "SellerPlanSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerPlanPayments");

            migrationBuilder.DropColumn(
                name: "RepresentativeFullName",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "RepresentativeIdentityNumber",
                table: "Businesses");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityNumber",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }
    }
}
