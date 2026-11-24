using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillMainBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        INSERT INTO [Branches]
        (
            [Id],
            [BusinessId],
            [Name],
            [Address],
            [City],
            [District],
            [PhoneNumber],
            [Latitude],
            [Longitude],
            [IsMain],
            [IsActive],
            [CreatedAt],
            [UpdatedAt]
        )
        SELECT
            NEWID(),
            b.[Id],
            b.[Name],
            ISNULL(b.[MerchantAddress], ''),
            ISNULL(b.[MerchantCity], ''),
            '',
            ISNULL(b.[MerchantPhoneNumber], ''),
            NULL,
            NULL,
            1,
            1,
            SYSUTCDATETIME(),
            NULL
        FROM [Businesses] AS b
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [Branches] AS br
            WHERE br.[BusinessId] = b.[Id]
              AND br.[IsMain] = 1
        );
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
