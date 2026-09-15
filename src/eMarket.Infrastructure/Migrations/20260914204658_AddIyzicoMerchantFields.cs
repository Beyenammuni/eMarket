using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIyzicoMerchantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PlatformCommissionAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PlatformCommissionCurrency",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformCommissionRate",
                table: "Payments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SellerCurrency",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE Orders
                SET BusinessId = '44E6E339-853E-4C97-B626-AF2CD1CF7522'
                WHERE BusinessId = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IyzicoSubMerchantKey",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IyzicoSubMerchantStatus",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Businesses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantAddress",
                table: "Businesses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantCity",
                table: "Businesses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantCountry",
                table: "Businesses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantEmail",
                table: "Businesses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantIban",
                table: "Businesses",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantPhoneNumber",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MerchantType",
                table: "Businesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformCommissionRate",
                table: "Businesses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

     migrationBuilder.Sql("""
           UPDATE Businesses
           SET
               LegalName = 'Reyan Alsayed',
               MerchantEmail = 'rayan@gmail.com',
               MerchantPhoneNumber = '+902222222222',
               MerchantCountry = 'Turkey',
               PlatformCommissionRate = 10.00
           WHERE Id = '44E6E339-853E-4C97-B626-AF2CD1CF7522';
           """);

migrationBuilder.Sql("""
    UPDATE Businesses
    SET
        MerchantType = 1,
        LegalName = 'Reyan Alsayed',
        IdentityNumber = '11111111111',
        MerchantAddress = 'Test Address',
        MerchantCity = 'Istanbul',
        MerchantCountry = 'Turkey',
        MerchantEmail = 'rayan@gmail.com',
        MerchantIban = 'TR000000000000000000000000',
        MerchantPhoneNumber = '+902222222222',
        PlatformCommissionRate = 10.00
    WHERE Id = '44E6E339-853E-4C97-B626-AF2CD1CF7522';
    """);



            migrationBuilder.AddColumn<string>(
                name: "TaxOffice",
                table: "Businesses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionCurrency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionRate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SellerAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SellerCurrency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "IyzicoSubMerchantKey",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "IyzicoSubMerchantStatus",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantAddress",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantCity",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantCountry",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantEmail",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantIban",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantPhoneNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MerchantType",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionRate",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TaxOffice",
                table: "Businesses");
        }
    }
}
