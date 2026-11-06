-- Somente leitura. Executar na origem Azure e na copia local e comparar os resultados.
SELECT @@SERVERNAME AS Servidor, DB_NAME() AS Banco;

SELECT i.Id, i.Nome,
    (SELECT COUNT_BIG(*) FROM Operacao o WHERE o.InvestidorId = i.Id) AS Operacoes,
    (SELECT MAX(o.Data) FROM Operacao o WHERE o.InvestidorId = i.Id) AS UltimaOperacao,
    (SELECT COUNT_BIG(*) FROM OperacaoOpcao o WHERE o.InvestidorId = i.Id) AS Opcoes,
    (SELECT COUNT_BIG(*) FROM Provento p WHERE p.InvestidorId = i.Id) AS Proventos,
    (SELECT COUNT_BIG(*) FROM MovimentacaoFinanceira m WHERE m.InvestidorId = i.Id) AS Movimentacoes,
    (SELECT COUNT_BIG(*) FROM HistoricoPatrimonio h WHERE h.InvestidorId = i.Id) AS HistoricosPatrimonio
FROM Investidor i
ORDER BY i.Nome;

SELECT COUNT_BIG(*) AS Usuarios FROM Usuario;
SELECT COUNT_BIG(*) AS RelatoriosIa FROM RelatorioDiarioIa;
-- As duas tabelas abaixo existem apos as migrations de 28/10/2026.
IF OBJECT_ID(N'HistoricoEstimativaIr', N'U') IS NOT NULL
    EXEC(N'SELECT Ano, COUNT_BIG(*) AS Versoes, MAX(CriadoEmUtc) AS UltimaVersaoUtc FROM HistoricoEstimativaIr GROUP BY Ano ORDER BY Ano;');
IF OBJECT_ID(N'RetencaoDefinitivaIr', N'U') IS NOT NULL
    EXEC(N'SELECT COUNT_BIG(*) AS RetencoesDefinitivas FROM RetencaoDefinitivaIr;');
