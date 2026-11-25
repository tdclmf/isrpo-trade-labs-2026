CREATE TABLE [Покупатели] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [ContactPerson] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Покупатели] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [СекретныеВопросы] (
    [Id] int NOT NULL IDENTITY,
    [Text] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_СекретныеВопросы] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Товары] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [WholesalePrice] decimal(18,2) NOT NULL,
    [RetailPrice] decimal(18,2) NOT NULL,
    [ReferenceInfo] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Товары] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Сделки] (
    [Id] int NOT NULL IDENTITY,
    [BuyerId] int NOT NULL,
    [BuyerName] nvarchar(max) NOT NULL,
    [BuyerAddress] nvarchar(max) NOT NULL,
    [BuyerPhone] nvarchar(max) NOT NULL,
    [BuyerContact] nvarchar(max) NOT NULL,
    [PurchaseDate] datetime2 NOT NULL,
    [Mode] nvarchar(max) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountRate] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Сделки] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Сделки_Покупатели_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Покупатели] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Пользователи] (
    [Id] int NOT NULL IDENTITY,
    [LastName] nvarchar(50) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [CodeWordHash] nvarchar(max) NOT NULL,
    [SecretQuestionId] int NOT NULL,
    [SecretAnswerHash] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Пользователи] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Пользователи_СекретныеВопросы_SecretQuestionId] FOREIGN KEY ([SecretQuestionId]) REFERENCES [СекретныеВопросы] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ПозицииСделок] (
    [Id] int NOT NULL IDENTITY,
    [SaleId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ПозицииСделок] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ПозицииСделок_Сделки_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Сделки] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ПозицииСделок_Товары_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Товары] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_ПозицииСделок_ProductId] ON [ПозицииСделок] ([ProductId]);
GO


CREATE INDEX [IX_ПозицииСделок_SaleId] ON [ПозицииСделок] ([SaleId]);
GO


CREATE UNIQUE INDEX [IX_Пользователи_Email] ON [Пользователи] ([Email]);
GO


CREATE INDEX [IX_Пользователи_SecretQuestionId] ON [Пользователи] ([SecretQuestionId]);
GO


CREATE INDEX [IX_Сделки_BuyerId] ON [Сделки] ([BuyerId]);
GO



