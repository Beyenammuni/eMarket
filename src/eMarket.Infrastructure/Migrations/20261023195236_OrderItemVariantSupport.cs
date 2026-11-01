using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderItemVariantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.AddColumn<Guid>(
     name: "Id",
     table: "OrderItems",
     type: "uniqueidentifier",
     nullable: true);

            migrationBuilder.Sql(
                "UPDATE [OrderItems] SET [Id] = NEWID() WHERE [Id] IS NULL;");
            migrationBuilder.AlterColumn<Guid>(
    name: "Id",
    table: "OrderItems",
    type: "uniqueidentifier",
    nullable: false,
    oldClrType: typeof(Guid),
    oldType: "uniqueidentifier",
    oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                columns: new[] { "OrderId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "OrderItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                columns: new[] { "OrderId", "ProductId" });
        }
    }
}
