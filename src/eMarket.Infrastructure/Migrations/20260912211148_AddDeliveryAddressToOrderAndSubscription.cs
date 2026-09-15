using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryAddressToOrderAndSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId1",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CartId1",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_AddressLine",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_ApartmentNumber",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_BuildingNumber",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_City",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_District",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_FullName",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryAddress_Latitude",
                table: "Subscriptions",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryAddress_Longitude",
                table: "Subscriptions",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Neighborhood",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_PhoneNumber",
                table: "Subscriptions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_PostalCode",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Street",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_AddressLine",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_ApartmentNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_BuildingNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_City",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_District",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_FullName",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryAddress_Latitude",
                table: "Orders",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryAddress_Longitude",
                table: "Orders",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Neighborhood",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_PhoneNumber",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_PostalCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Street",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress_AddressLine",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_ApartmentNumber",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_BuildingNumber",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_City",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_District",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_FullName",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Latitude",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Longitude",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Neighborhood",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_PhoneNumber",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_PostalCode",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Street",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_AddressLine",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_ApartmentNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_BuildingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_District",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_FullName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Latitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Longitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Neighborhood",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_PhoneNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_PostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Street",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "CartId1",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId1",
                table: "CartItems",
                column: "CartId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId1",
                table: "CartItems",
                column: "CartId1",
                principalTable: "Carts",
                principalColumn: "Id");
        }
    }
}
