using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIyzicoPricingPlanReferenceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IyzicoPricingPlanReferenceCode",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IyzicoPricingPlanReferenceCode",
                table: "Subscriptions");
        }
    }
}
