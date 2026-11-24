BEGIN TRANSACTION;
ALTER TABLE [CartItems] ADD [Id] uniqueidentifier NULL;

UPDATE [CartItems] SET [Id] = NEWID() WHERE [Id] IS NULL;

ALTER TABLE [CartItems] DROP CONSTRAINT [PK_CartItems];

ALTER TABLE [CartItems] ADD [VariantId] uniqueidentifier NULL;

ALTER TABLE [CartItems] ADD [VariantName] nvarchar(100) NULL;

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CartItems]') AND [c].[name] = N'Id');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [CartItems] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [CartItems] ALTER COLUMN [Id] uniqueidentifier NOT NULL;

ALTER TABLE [CartItems] ADD CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]);

CREATE INDEX [IX_CartItems_CartId] ON [CartItems] ([CartId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261023174210_CartItemIndependentKey', N'10.0.2');

COMMIT;
GO

