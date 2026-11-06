-- Aportiva: atualizacao idempotente da estrutura local. Gerado pelo EF Core 10.0.11.
-- Origem: migrations de main incorporadas em develop em 06/11/2026.
-- No SSMS: conectar ao SQL Server LOCAL e selecionar InvestimentosDb. Fazer backup antes.
-- Cada lote e protegido; o script NAO copia dados do Azure nem recria operacoes.

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE TABLE [ClasseAtivo] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Ativo] bit NOT NULL,
        CONSTRAINT [PK_ClasseAtivo] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE TABLE [TipoAtivo] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Ativo] bit NOT NULL,
        [ClasseAtivoId] int NOT NULL,
        CONSTRAINT [PK_TipoAtivo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TipoAtivo_ClasseAtivo_ClasseAtivoId] FOREIGN KEY ([ClasseAtivoId]) REFERENCES [ClasseAtivo] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE TABLE [Ativo] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(150) NOT NULL,
        [TipoAtivoId] int NOT NULL,
        [Ticker] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Ativo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ativo_TipoAtivo_TipoAtivoId] FOREIGN KEY ([TipoAtivoId]) REFERENCES [TipoAtivo] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[ClasseAtivo]'))
        SET IDENTITY_INSERT [ClasseAtivo] ON;
    EXEC(N'INSERT INTO [ClasseAtivo] ([Id], [Ativo], [Codigo], [Nome])
    VALUES (1, CAST(1 AS bit), N''RENDA_FIXA'', N''Renda Fixa''),
    (2, CAST(1 AS bit), N''RENDA_VARIAVEL'', N''Renda Variável''),
    (3, CAST(1 AS bit), N''FUNDOS'', N''Fundos''),
    (4, CAST(1 AS bit), N''ALTERNATIVOS'', N''Alternativos'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[ClasseAtivo]'))
        SET IDENTITY_INSERT [ClasseAtivo] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'ClasseAtivoId', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[TipoAtivo]'))
        SET IDENTITY_INSERT [TipoAtivo] ON;
    EXEC(N'INSERT INTO [TipoAtivo] ([Id], [Ativo], [ClasseAtivoId], [Codigo], [Nome])
    VALUES (1, CAST(1 AS bit), 2, N''ACAO'', N''Ação''),
    (2, CAST(1 AS bit), 3, N''FII'', N''Fundo Imobiliário''),
    (3, CAST(1 AS bit), 3, N''ETF'', N''ETF''),
    (4, CAST(1 AS bit), 2, N''BDR'', N''BDR''),
    (5, CAST(1 AS bit), 1, N''TESOURO_DIRETO'', N''Tesouro Direto''),
    (6, CAST(1 AS bit), 1, N''CDB'', N''CDB''),
    (7, CAST(1 AS bit), 1, N''LCI'', N''LCI''),
    (8, CAST(1 AS bit), 1, N''LCA'', N''LCA''),
    (9, CAST(1 AS bit), 1, N''DEBENTURE'', N''Debênture''),
    (10, CAST(1 AS bit), 1, N''POUPANCA'', N''Poupança''),
    (11, CAST(1 AS bit), 3, N''FUNDO_RENDA_FIXA'', N''Fundo de Renda Fixa''),
    (12, CAST(1 AS bit), 3, N''FUNDO_ACOES'', N''Fundo de Ações''),
    (13, CAST(1 AS bit), 3, N''FUNDO_MULTIMERCADO'', N''Fundo Multimercado''),
    (14, CAST(1 AS bit), 3, N''FUNDO_CAMBIAL'', N''Fundo Cambial''),
    (15, CAST(1 AS bit), 4, N''CRIPTOMOEDA'', N''Criptomoeda''),
    (16, CAST(1 AS bit), 4, N''CAMBIO'', N''Câmbio''),
    (17, CAST(1 AS bit), 4, N''COMMODITY'', N''Commodity''),
    (18, CAST(1 AS bit), 4, N''OURO'', N''Ouro'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'ClasseAtivoId', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[TipoAtivo]'))
        SET IDENTITY_INSERT [TipoAtivo] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE INDEX [IX_Ativo_TipoAtivoId] ON [Ativo] ([TipoAtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ativo_Ticker] ON [Ativo] ([Ticker]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ClasseAtivo_Codigo] ON [ClasseAtivo] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE INDEX [IX_TipoAtivo_ClasseAtivoId] ON [TipoAtivo] ([ClasseAtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TipoAtivo_Codigo] ON [TipoAtivo] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904200647_CriacaoInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904200647_CriacaoInicial', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904202412_AdicionarInvestidor'
)
BEGIN
    CREATE TABLE [Investidor] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(150) NOT NULL,
        CONSTRAINT [PK_Investidor] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904202412_AdicionarInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904202412_AdicionarInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE TABLE [TipoOperacao] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Ativo] bit NOT NULL,
        CONSTRAINT [PK_TipoOperacao] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE TABLE [Operacao] (
        [Id] uniqueidentifier NOT NULL,
        [Data] datetime2 NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [TipoOperacaoId] int NOT NULL,
        [Quantidade] decimal(18,8) NOT NULL,
        [PrecoUnitario] decimal(18,8) NOT NULL,
        [Taxas] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Operacao] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Operacao_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operacao_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operacao_TipoOperacao_TipoOperacaoId] FOREIGN KEY ([TipoOperacaoId]) REFERENCES [TipoOperacao] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[TipoOperacao]'))
        SET IDENTITY_INSERT [TipoOperacao] ON;
    EXEC(N'INSERT INTO [TipoOperacao] ([Id], [Ativo], [Codigo], [Nome])
    VALUES (1, CAST(1 AS bit), N''COMPRA'', N''Compra''),
    (2, CAST(1 AS bit), N''VENDA'', N''Venda'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Ativo', N'Codigo', N'Nome') AND [object_id] = OBJECT_ID(N'[TipoOperacao]'))
        SET IDENTITY_INSERT [TipoOperacao] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE INDEX [IX_Operacao_AtivoId] ON [Operacao] ([AtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE INDEX [IX_Operacao_InvestidorId] ON [Operacao] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE INDEX [IX_Operacao_TipoOperacaoId] ON [Operacao] ([TipoOperacaoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TipoOperacao_Codigo] ON [TipoOperacao] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904203259_AdicionarOperacoes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904203259_AdicionarOperacoes', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904223325_AdicionarUnicidadeInvestidor'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Investidor_Nome] ON [Investidor] ([Nome]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904223325_AdicionarUnicidadeInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904223325_AdicionarUnicidadeInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904231654_AjustarMapeamentoTicker'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904231654_AjustarMapeamentoTicker', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905025111_AdicionarSequenciaOperacao'
)
BEGIN
    DROP INDEX [IX_Operacao_InvestidorId] ON [Operacao];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905025111_AdicionarSequenciaOperacao'
)
BEGIN
    ALTER TABLE [Operacao] ADD [Sequencia] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905025111_AdicionarSequenciaOperacao'
)
BEGIN
    WITH OperacoesOrdenadas AS
    (
        SELECT
            Id,
            ROW_NUMBER() OVER
            (
                PARTITION BY InvestidorId, Data
                ORDER BY
                    CASE
                        WHEN PrecoUnitario = 40.00 THEN 1
                        WHEN PrecoUnitario = 42.00 THEN 2
                        WHEN PrecoUnitario = 45.00 THEN 3
                        ELSE 4
                    END,
                    Id
            ) AS NovaSequencia
        FROM Operacao
    )
    UPDATE Operacao
    SET Sequencia = OperacoesOrdenadas.NovaSequencia
    FROM Operacao
    INNER JOIN OperacoesOrdenadas
        ON Operacao.Id = OperacoesOrdenadas.Id;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905025111_AdicionarSequenciaOperacao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Operacao_InvestidorId_Data_Sequencia] ON [Operacao] ([InvestidorId], [Data], [Sequencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905025111_AdicionarSequenciaOperacao'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905025111_AdicionarSequenciaOperacao', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE TABLE [OperacaoOpcao] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [TickerOpcao] nvarchar(30) NOT NULL,
        [TipoOpcao] nvarchar(10) NOT NULL,
        [Natureza] nvarchar(10) NOT NULL,
        [DataOperacao] datetime2 NOT NULL,
        [Vencimento] datetime2 NOT NULL,
        [Strike] decimal(18,8) NOT NULL,
        [Contratos] int NOT NULL,
        [Quantidade] decimal(18,8) NOT NULL,
        [PremioUnitario] decimal(18,8) NOT NULL,
        [Taxas] decimal(18,2) NOT NULL,
        [Situacao] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_OperacaoOpcao] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OperacaoOpcao_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperacaoOpcao_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE TABLE [Provento] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(30) NOT NULL,
        [DataCom] datetime2 NOT NULL,
        [DataPagamento] datetime2 NOT NULL,
        [QuantidadeBase] decimal(18,8) NOT NULL,
        [ValorPorUnidade] decimal(18,8) NOT NULL,
        [ValorTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Provento] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Provento_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Provento_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE INDEX [IX_OperacaoOpcao_AtivoId] ON [OperacaoOpcao] ([AtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE INDEX [IX_OperacaoOpcao_InvestidorId_TickerOpcao_DataOperacao] ON [OperacaoOpcao] ([InvestidorId], [TickerOpcao], [DataOperacao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE INDEX [IX_Provento_AtivoId] ON [Provento] ([AtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    CREATE INDEX [IX_Provento_InvestidorId_DataPagamento] ON [Provento] ([InvestidorId], [DataPagamento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905033824_AdicionarProventosEOpcoes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905033824_AdicionarProventosEOpcoes', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905045734_AmpliarHistoricoOperacoesOpcoes'
)
BEGIN
    ALTER TABLE [OperacaoOpcao] ADD [DataFinalizacao] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905045734_AmpliarHistoricoOperacoesOpcoes'
)
BEGIN
    ALTER TABLE [OperacaoOpcao] ADD [ValorRecompra] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905045734_AmpliarHistoricoOperacoesOpcoes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905045734_AmpliarHistoricoOperacoesOpcoes', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905053907_AjustarHistoricoProventos'
)
BEGIN
    EXEC sp_rename N'[Provento].[ValorTotal]', N'ValorRecebido', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905053907_AjustarHistoricoProventos'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Provento]') AND [c].[name] = N'DataCom');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Provento] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Provento] ALTER COLUMN [DataCom] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905053907_AjustarHistoricoProventos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905053907_AjustarHistoricoProventos', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    DROP INDEX [IX_OperacaoOpcao_InvestidorId_TickerOpcao_DataOperacao] ON [OperacaoOpcao];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    EXEC sp_rename N'[OperacaoOpcao].[ValorRecompra]', N'PrecoRecompraUnitario', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OperacaoOpcao]') AND [c].[name] = N'PrecoRecompraUnitario');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [OperacaoOpcao] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [OperacaoOpcao] ALTER COLUMN [PrecoRecompraUnitario] decimal(18,8) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    ALTER TABLE [OperacaoOpcao] ADD [ValorExecucao] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    CREATE INDEX [IX_OperacaoOpcao_InvestidorId_DataOperacao] ON [OperacaoOpcao] ([InvestidorId], [DataOperacao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    CREATE INDEX [IX_OperacaoOpcao_TickerOpcao] ON [OperacaoOpcao] ([TickerOpcao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905054424_CorrigirHistoricoOperacoesOpcoes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905054424_CorrigirHistoricoOperacoesOpcoes', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905063519_AdicionarTiposRendaFixaEPrevidencia'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM TipoAtivo
        WHERE Codigo = 'RENDA_FIXA'
    )
    BEGIN
        INSERT INTO TipoAtivo
        (
            Codigo,
            Nome,
            Ativo,
            ClasseAtivoId
        )
        SELECT
            'RENDA_FIXA',
            'Renda Fixa',
            1,
            Id
        FROM ClasseAtivo
        WHERE Codigo = 'RENDA_FIXA';
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905063519_AdicionarTiposRendaFixaEPrevidencia'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM TipoAtivo
        WHERE Codigo = 'PREVIDENCIA'
    )
    BEGIN
        INSERT INTO TipoAtivo
        (
            Codigo,
            Nome,
            Ativo,
            ClasseAtivoId
        )
        SELECT
            'PREVIDENCIA',
            'Previdência',
            1,
            Id
        FROM ClasseAtivo
        WHERE Codigo = 'FUNDOS';
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905063519_AdicionarTiposRendaFixaEPrevidencia'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905063519_AdicionarTiposRendaFixaEPrevidencia', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905141050_PreservarDadosHistoricosExcel'
)
BEGIN
    ALTER TABLE [Provento] ADD [Descricao] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905141050_PreservarDadosHistoricosExcel'
)
BEGIN
    ALTER TABLE [OperacaoOpcao] ADD [ResultadoInformado] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905141050_PreservarDadosHistoricosExcel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905141050_PreservarDadosHistoricosExcel', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142102_AdicionarCotacaoAtivo'
)
BEGIN
    CREATE TABLE [CotacaoAtivo] (
        [Id] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [DataReferencia] datetime2 NOT NULL,
        [Preco] decimal(18,8) NOT NULL,
        CONSTRAINT [PK_CotacaoAtivo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CotacaoAtivo_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142102_AdicionarCotacaoAtivo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CotacaoAtivo_AtivoId_DataReferencia] ON [CotacaoAtivo] ([AtivoId], [DataReferencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142102_AdicionarCotacaoAtivo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905142102_AdicionarCotacaoAtivo', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142714_AdicionarSaldoDisponivel'
)
BEGIN
    CREATE TABLE [SaldoDisponivel] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [DataReferencia] datetime2 NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SaldoDisponivel] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SaldoDisponivel_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142714_AdicionarSaldoDisponivel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SaldoDisponivel_InvestidorId_DataReferencia] ON [SaldoDisponivel] ([InvestidorId], [DataReferencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905142714_AdicionarSaldoDisponivel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905142714_AdicionarSaldoDisponivel', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905143022_AdicionarHistoricoPatrimonio'
)
BEGIN
    CREATE TABLE [HistoricoPatrimonio] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [DataReferencia] datetime2 NOT NULL,
        [ValorCarteira] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_HistoricoPatrimonio] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HistoricoPatrimonio_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905143022_AdicionarHistoricoPatrimonio'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HistoricoPatrimonio_InvestidorId_DataReferencia] ON [HistoricoPatrimonio] ([InvestidorId], [DataReferencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905143022_AdicionarHistoricoPatrimonio'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905143022_AdicionarHistoricoPatrimonio', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905170000_AdicionarDescontosFiscais'
)
BEGIN
    CREATE TABLE [DescontoFiscal] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(10) NOT NULL,
        [DataPagamento] date NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [Descricao] nvarchar(250) NULL,
        CONSTRAINT [PK_DescontoFiscal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DescontoFiscal_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905170000_AdicionarDescontosFiscais'
)
BEGIN
    CREATE INDEX [IX_DescontoFiscal_InvestidorId] ON [DescontoFiscal] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905170000_AdicionarDescontosFiscais'
)
BEGIN
    CREATE INDEX [IX_DescontoFiscal_InvestidorId_DataPagamento_Tipo] ON [DescontoFiscal] ([InvestidorId], [DataPagamento], [Tipo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905170000_AdicionarDescontosFiscais'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905170000_AdicionarDescontosFiscais', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    ALTER TABLE [DescontoFiscal] DROP CONSTRAINT [FK_DescontoFiscal_Investidor_InvestidorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    DROP INDEX [IX_DescontoFiscal_InvestidorId] ON [DescontoFiscal];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    DROP INDEX [IX_DescontoFiscal_InvestidorId_DataPagamento_Tipo] ON [DescontoFiscal];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DescontoFiscal]') AND [c].[name] = N'InvestidorId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DescontoFiscal] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [DescontoFiscal] DROP COLUMN [InvestidorId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    CREATE INDEX [IX_DescontoFiscal_DataPagamento_Tipo] ON [DescontoFiscal] ([DataPagamento], [Tipo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910013903_DescontoFiscalPorCarteira'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910013903_DescontoFiscalPorCarteira', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE TABLE [Usuario] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(150) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [SenhaHash] nvarchar(500) NOT NULL,
        [Perfil] nvarchar(30) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [DataCadastro] datetime2 NOT NULL,
        [DataAprovacao] datetime2 NULL,
        [AprovadoPorUsuarioId] uniqueidentifier NULL,
        [UltimoLogin] datetime2 NULL,
        CONSTRAINT [PK_Usuario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuario_Usuario_AprovadoPorUsuarioId] FOREIGN KEY ([AprovadoPorUsuarioId]) REFERENCES [Usuario] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE TABLE [TokenRecuperacaoSenha] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [DataCriacao] datetime2 NOT NULL,
        [DataExpiracao] datetime2 NOT NULL,
        [DataUtilizacao] datetime2 NULL,
        CONSTRAINT [PK_TokenRecuperacaoSenha] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TokenRecuperacaoSenha_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE TABLE [UsuarioInvestidor] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UsuarioInvestidor] PRIMARY KEY ([UsuarioId], [InvestidorId]),
        CONSTRAINT [FK_UsuarioInvestidor_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UsuarioInvestidor_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE TABLE [UsuarioPermissao] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [Permissao] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_UsuarioPermissao] PRIMARY KEY ([UsuarioId], [Permissao]),
        CONSTRAINT [FK_UsuarioPermissao_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TokenRecuperacaoSenha_TokenHash] ON [TokenRecuperacaoSenha] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE INDEX [IX_TokenRecuperacaoSenha_UsuarioId_DataExpiracao] ON [TokenRecuperacaoSenha] ([UsuarioId], [DataExpiracao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE INDEX [IX_Usuario_AprovadoPorUsuarioId] ON [Usuario] ([AprovadoPorUsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuario_Email] ON [Usuario] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    CREATE INDEX [IX_UsuarioInvestidor_InvestidorId] ON [UsuarioInvestidor] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914203708_AdicionarIdentidadeUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914203708_AdicionarIdentidadeUsuarios', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919160000_AdicionarValorPatrimonialAtivo'
)
BEGIN
    CREATE TABLE [ValorPatrimonialAtivo] (
        [Id] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [DataReferencia] datetime2 NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ValorPatrimonialAtivo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ValorPatrimonialAtivo_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919160000_AdicionarValorPatrimonialAtivo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ValorPatrimonialAtivo_AtivoId_DataReferencia] ON [ValorPatrimonialAtivo] ([AtivoId], [DataReferencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919160000_AdicionarValorPatrimonialAtivo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919160000_AdicionarValorPatrimonialAtivo', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921150000_AdicionarPermissoesPorInvestidor'
)
BEGIN
    CREATE TABLE [UsuarioInvestidorPermissao] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [Permissao] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_UsuarioInvestidorPermissao] PRIMARY KEY ([UsuarioId], [InvestidorId], [Permissao]),
        CONSTRAINT [FK_UsuarioInvestidorPermissao_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UsuarioInvestidorPermissao_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921150000_AdicionarPermissoesPorInvestidor'
)
BEGIN
    CREATE INDEX [IX_UsuarioInvestidorPermissao_InvestidorId] ON [UsuarioInvestidorPermissao] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921150000_AdicionarPermissoesPorInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921150000_AdicionarPermissoesPorInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    ALTER TABLE [ValorPatrimonialAtivo] ADD [InvestidorId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN

    UPDATE V
    SET InvestidorId = COALESCE(
        (SELECT TOP 1 O.InvestidorId FROM Operacao O
         WHERE O.AtivoId = V.AtivoId
         GROUP BY O.InvestidorId ORDER BY COUNT(*) DESC),
        (SELECT TOP 1 I.Id FROM Investidor I ORDER BY I.Nome))
    FROM ValorPatrimonialAtivo V;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ValorPatrimonialAtivo]') AND [c].[name] = N'InvestidorId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ValorPatrimonialAtivo] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [ValorPatrimonialAtivo] ALTER COLUMN [InvestidorId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    DROP INDEX [IX_ValorPatrimonialAtivo_AtivoId_DataReferencia] ON [ValorPatrimonialAtivo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ValorPatrimonialAtivo_AtivoId_InvestidorId_DataReferencia] ON [ValorPatrimonialAtivo] ([AtivoId], [InvestidorId], [DataReferencia]) WHERE [AtivoId] IS NOT NULL AND [InvestidorId] IS NOT NULL AND [DataReferencia] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    CREATE INDEX [IX_ValorPatrimonialAtivo_InvestidorId] ON [ValorPatrimonialAtivo] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    ALTER TABLE [ValorPatrimonialAtivo] ADD CONSTRAINT [FK_ValorPatrimonialAtivo_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924210000_ValorPatrimonialPorInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924210000_ValorPatrimonialPorInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925023000_AdicionarMovimentacaoFinanceira'
)
BEGIN
    CREATE TABLE [MovimentacaoFinanceira] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [Data] date NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [Descricao] nvarchar(200) NULL,
        CONSTRAINT [PK_MovimentacaoFinanceira] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimentacaoFinanceira_Investidores_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925023000_AdicionarMovimentacaoFinanceira'
)
BEGIN
    CREATE INDEX [IX_MovimentacaoFinanceira_InvestidorId_Data] ON [MovimentacaoFinanceira] ([InvestidorId], [Data]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925023000_AdicionarMovimentacaoFinanceira'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925023000_AdicionarMovimentacaoFinanceira', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925205931_AdicionarMetasAtivos'
)
BEGIN
    CREATE TABLE [MetaAtivo] (
        [Id] uniqueidentifier NOT NULL,
        [InvestidorId] uniqueidentifier NOT NULL,
        [AtivoId] uniqueidentifier NOT NULL,
        [QuantidadeDesejada] decimal(18,4) NOT NULL,
        CONSTRAINT [PK_MetaAtivo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MetaAtivo_Ativo_AtivoId] FOREIGN KEY ([AtivoId]) REFERENCES [Ativo] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MetaAtivo_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925205931_AdicionarMetasAtivos'
)
BEGIN
    CREATE INDEX [IX_MetaAtivo_AtivoId] ON [MetaAtivo] ([AtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925205931_AdicionarMetasAtivos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MetaAtivo_InvestidorId_AtivoId] ON [MetaAtivo] ([InvestidorId], [AtivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925205931_AdicionarMetasAtivos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925205931_AdicionarMetasAtivos', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925232219_AdicionarInvestidorDescontoFiscal'
)
BEGIN
    ALTER TABLE [DescontoFiscal] ADD [InvestidorId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925232219_AdicionarInvestidorDescontoFiscal'
)
BEGIN
    CREATE INDEX [IX_DescontoFiscal_InvestidorId] ON [DescontoFiscal] ([InvestidorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925232219_AdicionarInvestidorDescontoFiscal'
)
BEGIN
    ALTER TABLE [DescontoFiscal] ADD CONSTRAINT [FK_DescontoFiscal_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925232219_AdicionarInvestidorDescontoFiscal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925232219_AdicionarInvestidorDescontoFiscal', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006194241_AdicionarRelatorioDiarioIa'
)
BEGIN
    CREATE TABLE [RelatorioDiarioIa] (
        [Id] uniqueidentifier NOT NULL,
        [DataReferencia] date NOT NULL,
        [DataGeracao] datetime2 NOT NULL,
        [Conteudo] nvarchar(max) NOT NULL,
        [Modelo] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_RelatorioDiarioIa] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006194241_AdicionarRelatorioDiarioIa'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RelatorioDiarioIa_DataReferencia] ON [RelatorioDiarioIa] ([DataReferencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006194241_AdicionarRelatorioDiarioIa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006194241_AdicionarRelatorioDiarioIa', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    DROP INDEX [IX_RelatorioDiarioIa_DataReferencia] ON [RelatorioDiarioIa];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    ALTER TABLE [RelatorioDiarioIa] ADD [Escopo] nvarchar(20) NOT NULL DEFAULT N'TODOS';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    ALTER TABLE [RelatorioDiarioIa] ADD [InvestidorId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    ALTER TABLE [Investidor] ADD [FrequenciaRelatorioIa] nvarchar(20) NOT NULL DEFAULT N'DIARIO';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    ALTER TABLE [Investidor] ADD [ReceberRelatorioIa] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    ALTER TABLE [Investidor] ADD [WhatsApp] nvarchar(15) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RelatorioDiarioIa_DataReferencia_Escopo_InvestidorId] ON [RelatorioDiarioIa] ([DataReferencia], [Escopo], [InvestidorId]) WHERE [InvestidorId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006204558_AdicionarPreferenciasRelatorioIaInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006205707_GarantirUnicidadeRelatoriosIa'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RelatorioDiarioIa_DataReferencia_Escopo] ON [RelatorioDiarioIa] ([DataReferencia], [Escopo]) WHERE [InvestidorId] IS NULL AND [Escopo] = ''TODOS''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006205707_GarantirUnicidadeRelatoriosIa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006205707_GarantirUnicidadeRelatoriosIa', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin'
)
BEGIN
    ALTER TABLE [Usuario] ADD [FrequenciaRelatorioIa] nvarchar(20) NOT NULL DEFAULT N'DIARIO';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin'
)
BEGIN
    ALTER TABLE [Usuario] ADD [ReceberRelatorioIa] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin'
)
BEGIN
    ALTER TABLE [Usuario] ADD [UltimoEnvioRelatorioIa] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin'
)
BEGIN
    ALTER TABLE [Usuario] ADD [WhatsApp] nvarchar(15) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006210327_AdicionarPreferenciasRelatorioIaAdmin', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    IF COL_LENGTH('Usuario', 'WhatsApp') IS NOT NULL ALTER TABLE [Usuario] DROP COLUMN [WhatsApp];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    IF COL_LENGTH('Investidor', 'FrequenciaRelatorioIa') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [FrequenciaRelatorioIa];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    IF COL_LENGTH('Investidor', 'ReceberRelatorioIa') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [ReceberRelatorioIa];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    IF COL_LENGTH('Investidor', 'WhatsApp') IS NOT NULL ALTER TABLE [Investidor] DROP COLUMN [WhatsApp];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    IF COL_LENGTH('Usuario', 'TelegramChatId') IS NULL ALTER TABLE [Usuario] ADD [TelegramChatId] nvarchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261007224058_SincronizarModeloAtual'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261007224058_SincronizarModeloAtual', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013160000_AdicionarLogsAplicacao'
)
BEGIN
    CREATE TABLE [LogsAplicacao] (
        [Id] uniqueidentifier NOT NULL,
        [DataUtc] datetime2 NOT NULL,
        [Ambiente] nvarchar(40) NOT NULL,
        [Modulo] nvarchar(80) NOT NULL,
        [Operacao] nvarchar(120) NOT NULL,
        [Nivel] nvarchar(20) NOT NULL,
        [Mensagem] nvarchar(1500) NOT NULL,
        [Detalhes] nvarchar(max) NULL,
        [CorrelacaoId] nvarchar(100) NULL,
        CONSTRAINT [PK_LogsAplicacao] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013160000_AdicionarLogsAplicacao'
)
BEGIN
    CREATE INDEX [IX_LogsAplicacao_DataUtc] ON [LogsAplicacao] ([DataUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013160000_AdicionarLogsAplicacao'
)
BEGIN
    CREATE INDEX [IX_LogsAplicacao_CorrelacaoId] ON [LogsAplicacao] ([CorrelacaoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013160000_AdicionarLogsAplicacao'
)
BEGIN
    CREATE INDEX [IX_LogsAplicacao_Modulo_Nivel_DataUtc] ON [LogsAplicacao] ([Modulo], [Nivel], [DataUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013160000_AdicionarLogsAplicacao'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013160000_AdicionarLogsAplicacao', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030000_AdicionarCotacaoMercadoOperacaoOpcao'
)
BEGIN
    IF COL_LENGTH(N'dbo.OperacaoOpcao', N'PrecoMercadoAtual') IS NULL ALTER TABLE [dbo].[OperacaoOpcao] ADD [PrecoMercadoAtual] decimal(18,8) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030000_AdicionarCotacaoMercadoOperacaoOpcao'
)
BEGIN
    IF COL_LENGTH(N'dbo.OperacaoOpcao', N'DataAtualizacaoCotacao') IS NULL ALTER TABLE [dbo].[OperacaoOpcao] ADD [DataAtualizacaoCotacao] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014030000_AdicionarCotacaoMercadoOperacaoOpcao'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014030000_AdicionarCotacaoMercadoOperacaoOpcao', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022050000_CalendarioDividendos'
)
BEGIN
    CREATE TABLE [CalendariosDividendos] (
        [InvestidorId] uniqueidentifier NOT NULL,
        [Ticker] nvarchar(24) NOT NULL,
        [Meses] nvarchar(48) NOT NULL,
        [AtualizadoEmUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_CalendariosDividendos] PRIMARY KEY ([InvestidorId], [Ticker]),
        CONSTRAINT [FK_CalendariosDividendos_Investidor_InvestidorId] FOREIGN KEY ([InvestidorId]) REFERENCES [Investidor] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022050000_CalendarioDividendos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022050000_CalendarioDividendos', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184747_EstimativaIrPorInvestidor'
)
BEGIN
    CREATE TABLE [HistoricoEstimativaIr] (
        [Id] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [CriadoEmUtc] datetime2 NOT NULL,
        [CriadoPor] nvarchar(100) NOT NULL,
        [Conteudo] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_HistoricoEstimativaIr] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184747_EstimativaIrPorInvestidor'
)
BEGIN
    CREATE TABLE [RetencaoDefinitivaIr] (
        [Id] uniqueidentifier NOT NULL,
        [OperacaoId] uniqueidentifier NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [Observacao] nvarchar(1000) NOT NULL,
        [CriadoEmUtc] datetime2 NOT NULL,
        [CriadoPor] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_RetencaoDefinitivaIr] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184747_EstimativaIrPorInvestidor'
)
BEGIN
    CREATE INDEX [IX_HistoricoEstimativaIr_Ano_CriadoEmUtc] ON [HistoricoEstimativaIr] ([Ano], [CriadoEmUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184747_EstimativaIrPorInvestidor'
)
BEGIN
    CREATE INDEX [IX_RetencaoDefinitivaIr_OperacaoId_CriadoEmUtc] ON [RetencaoDefinitivaIr] ([OperacaoId], [CriadoEmUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184747_EstimativaIrPorInvestidor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028184747_EstimativaIrPorInvestidor', N'10.0.11');
END;

COMMIT;
GO

IF DB_NAME() <> N'InvestimentosDb' OR CONVERT(int, SERVERPROPERTY(N'EngineEdition')) IN (5, 8)
    THROW 51000, N'Execute este script somente em InvestimentosDb no SQL Server LOCAL. Azure SQL e outros bancos foram bloqueados.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184800_RegistrarRetencaoSimoneLlft11'
)
BEGIN
    DECLARE @Candidatas TABLE (Id uniqueidentifier);
    INSERT INTO @Candidatas
    SELECT o.Id FROM Operacao o
    JOIN Investidor i ON i.Id = o.InvestidorId
    JOIN Ativo a ON a.Id = o.AtivoId
    JOIN TipoOperacao t ON t.Id = o.TipoOperacaoId
    WHERE UPPER(LTRIM(RTRIM(i.Nome))) = 'SIMONE'
      AND a.Ticker = 'LLFT11' AND t.Codigo = 'VENDA' AND o.Quantidade = 499
      AND o.Data >= '20261001' AND o.Data < '20261029';
    IF (SELECT COUNT(*) FROM @Candidatas) = 1
    BEGIN
        INSERT INTO RetencaoDefinitivaIr (Id, OperacaoId, Valor, Observacao, CriadoEmUtc, CriadoPor)
        SELECT NEWID(), c.Id, 62.28,
            N'IR definitivo de R$ 62,28 confirmado pelo titular em 28/10/2026 para venda de 499 cotas de LLFT11 da Simone. Conferir com nota de corretagem.',
            SYSUTCDATETIME(), 'confirmacao-titular-20261028'
        FROM @Candidatas c
        WHERE NOT EXISTS (SELECT 1 FROM RetencaoDefinitivaIr r WHERE r.OperacaoId = c.Id);
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184800_RegistrarRetencaoSimoneLlft11'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028184800_RegistrarRetencaoSimoneLlft11', N'10.0.11');
END;

COMMIT;
GO
