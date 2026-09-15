/* eMarket MVP1 - Admin setup
   Run after the existing EF migrations.
   This creates persistent application roles for users and promotes one account to Admin.
*/

IF OBJECT_ID(N'dbo.UserRoleAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoleAssignments
    (
        UserId uniqueidentifier NOT NULL,
        RoleId int NOT NULL,
        CONSTRAINT PK_UserRoleAssignments PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoleAssignments_Users_UserId FOREIGN KEY (UserId)
            REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_UserRoleAssignments_RoleId ON dbo.UserRoleAssignments(RoleId);
END;
GO

-- Existing users are customers by default.
INSERT INTO dbo.UserRoleAssignments (UserId, RoleId)
SELECT u.Id, 1
FROM dbo.Users u
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.UserRoleAssignments r
    WHERE r.UserId = u.Id AND r.RoleId = 1
);
GO

/* Promote your account to Admin by replacing the email below. */
DECLARE @Email nvarchar(255) = N'YOUR-ADMIN-EMAIL@example.com';
DECLARE @UserId uniqueidentifier =
    (SELECT TOP (1) Id FROM dbo.Users WHERE Email = @Email);

IF @UserId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.UserRoleAssignments WHERE UserId = @UserId AND RoleId = 4)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId)
    VALUES (@UserId, 4);
END;
GO
