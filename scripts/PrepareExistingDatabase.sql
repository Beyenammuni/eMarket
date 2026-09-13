/*
Run this BEFORE applying the generated migration that introduces Users.Username
if the database already contains Users rows without a username.
It is safe to run more than once.
*/

IF COL_LENGTH('dbo.Users', 'Username') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Username nvarchar(50) NULL;
END;
GO

UPDATE dbo.Users
SET Username = LEFT('user_' + REPLACE(CONVERT(varchar(36), Id), '-', ''), 50)
WHERE Username IS NULL OR LTRIM(RTRIM(Username)) = '';
GO

/* Detect collisions before making Username NOT NULL + UNIQUE. */
SELECT Username, COUNT(*) AS UserCount
FROM dbo.Users
GROUP BY Username
HAVING COUNT(*) > 1 OR Username IS NULL OR LTRIM(RTRIM(Username)) = '';
GO
