using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionBillingPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingCycle",
                table: "Subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentPeriodStart",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentPeriodEnd",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
        UPDATE Subscriptions
        SET
            BillingCycle = 1,
            CurrentPeriodStart = CAST(NextDeliveryDate AS date),
            CurrentPeriodEnd = DATEADD(month, 1, CAST(NextDeliveryDate AS date))
        WHERE
            BillingCycle IS NULL
            OR CurrentPeriodStart IS NULL
            OR CurrentPeriodEnd IS NULL;
        """);

            migrationBuilder.AlterColumn<int>(
                name: "BillingCycle",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CurrentPeriodStart",
                table: "Subscriptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CurrentPeriodEnd",
                table: "Subscriptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CurrentPeriodEnd",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CurrentPeriodStart",
                table: "Subscriptions");
        }
    }
}
