using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderBranchAndMigrateInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BranchId",
                table: "Orders",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Branches_BranchId",
                table: "Orders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            // Ensure every product has a main branch before migrating stock.
            migrationBuilder.Sql("""
    IF EXISTS
    (
        SELECT 1
        FROM Products p
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM Branches b
            WHERE b.BusinessId = p.BusinessId
              AND b.IsMain = 1
        )
    )
    BEGIN
        THROW 51000,
            'Cannot migrate inventory: a product business has no main branch.',
            1;
    END;
    """);

            // Migrate parent product stock, including products that have variants.
            // Parent stock remains independent from variant stock.
            migrationBuilder.Sql("""
    INSERT INTO BranchInventories
    (
        Id,
        BranchId,
        ProductId,
        VariantId,
        Quantity,
        ReservedQuantity,
        UpdatedAt
    )
    SELECT
        NEWID(),
        b.Id,
        p.Id,
        NULL,
        p.StockQuantity,
        0,
        SYSUTCDATETIME()
    FROM Products p
    INNER JOIN Branches b
        ON b.BusinessId = p.BusinessId
       AND b.IsMain = 1
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM BranchInventories bi
        WHERE bi.BranchId = b.Id
          AND bi.ProductId = p.Id
          AND bi.VariantId IS NULL
    );
    """);

            // Migrate every product variant stock independently.
            migrationBuilder.Sql("""
    INSERT INTO BranchInventories
    (
        Id,
        BranchId,
        ProductId,
        VariantId,
        Quantity,
        ReservedQuantity,
        UpdatedAt
    )
    SELECT
        NEWID(),
        b.Id,
        pv.ProductId,
        pv.Id,
        pv.StockQuantity,
        0,
        SYSUTCDATETIME()
    FROM ProductVariants pv
    INNER JOIN Products p
        ON p.Id = pv.ProductId
    INNER JOIN Branches b
        ON b.BusinessId = p.BusinessId
       AND b.IsMain = 1
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM BranchInventories bi
        WHERE bi.BranchId = b.Id
          AND bi.VariantId = pv.Id
    );
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Branches_BranchId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BranchId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Orders");
        }
    }
}
