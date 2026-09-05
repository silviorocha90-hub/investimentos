using Investimentos.Importador.BancoDados;
using Investimentos.Importador.Importacao;

Console.OutputEncoding =
    System.Text.Encoding.UTF8;

Console.WriteLine(
    "========================================");

Console.WriteLine(
    " IMPORTADOR DE INVESTIMENTOS");

Console.WriteLine(
    "========================================");

Console.WriteLine();

if (args.Length == 0)
{
    Console.WriteLine(
        "Informe o caminho do arquivo Investimentos.xlsx.");

    AguardarEncerramento();
    return;
}

var caminhoArquivo =
    args[0];

try
{
    var leitor =
        new LeitorExcelInvestimentos();

    Console.WriteLine(
        $"Arquivo: {caminhoArquivo}");

    Console.WriteLine();

    Console.WriteLine(
        "Extraindo e validando dados...");

    Console.WriteLine();

    var dados =
        leitor.Ler(caminhoArquivo);

    var validadorHistorico =
        new ValidadorHistoricoOperacoes();

    var inconsistenciasHistoricas =
        validadorHistorico.Validar(
            dados.Operacoes);

    var compras =
        dados.Operacoes.Count(
            x => x.TipoOperacao == "COMPRA");

    var vendas =
        dados.Operacoes.Count(
            x => x.TipoOperacao == "VENDA");

    var puts =
        dados.Opcoes.Count(
            x => x.TipoOpcao == "PUT");

    var calls =
        dados.Opcoes.Count(
            x => x.TipoOpcao == "CALL");

    var cadastroPorTicker =
        dados.CadastroAtivos
            .GroupBy(
                x => x.Ticker,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.First(),
                StringComparer.OrdinalIgnoreCase);

    string? ObterTipoAtivoCodigo(
        string ticker)
    {
        if (cadastroPorTicker.TryGetValue(
                ticker,
                out var cadastro) &&
            cadastro.Classificado)
        {
            return cadastro.TipoAtivoCodigo;
        }

        return MapeamentoAtivosHistoricos
            .ObterTipoAtivoCodigo(ticker);
    }

    var ativosClassificados =
        dados.Ativos
            .Where(ticker =>
                ObterTipoAtivoCodigo(ticker) is not null)
            .OrderBy(x => x)
            .ToList();

    var ativosSemClassificacao =
        dados.Ativos
            .Where(ticker =>
                ObterTipoAtivoCodigo(ticker) is null)
            .OrderBy(x => x)
            .ToList();

    /*
     * Validações dos snapshots.
     */

    var cotacoesDuplicadas =
        dados.Cotacoes
            .GroupBy(
                x => x.Ativo,
                StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .ToList();

    var saldosDuplicados =
        dados.SaldosDisponiveis
            .GroupBy(
                x => x.Investidor,
                StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .ToList();

    var historicosDuplicados =
        dados.HistoricosPatrimonio
            .GroupBy(x => new
            {
                Investidor =
                    x.Investidor.ToUpperInvariant(),

                Data =
                    x.DataReferencia.Date
            })
            .Where(x => x.Count() > 1)
            .ToList();

    /*
     * OPERAÇÕES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " OPERAÇÕES VÁLIDAS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.Operacoes.Count}");

    Console.WriteLine(
        $"Compras............: {compras}");

    Console.WriteLine(
        $"Vendas..............: {vendas}");

    Console.WriteLine();

    /*
     * PROVENTOS
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " PROVENTOS VÁLIDOS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.Proventos.Count}");

    Console.WriteLine();

    /*
     * OPÇÕES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " OPÇÕES VÁLIDAS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.Opcoes.Count}");

    Console.WriteLine(
        $"PUTs...............: {puts}");

    Console.WriteLine(
        $"CALLs..............: {calls}");

    Console.WriteLine();

    /*
     * COTAÇÕES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " COTAÇÕES VÁLIDAS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.Cotacoes.Count}");

    Console.WriteLine(
        $"Duplicadas.........: {cotacoesDuplicadas.Count}");

    if (cotacoesDuplicadas.Count > 0)
    {
        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "COTAÇÕES DUPLICADAS:");

        Console.ResetColor();

        foreach (var grupo in
                 cotacoesDuplicadas
                     .OrderBy(x => x.Key))
        {
            Console.WriteLine(
                $"  ! {grupo.Key}: {grupo.Count()} registros");
        }
    }

    Console.WriteLine();

    /*
     * SALDOS
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " SALDOS DISPONÍVEIS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.SaldosDisponiveis.Count}");

    Console.WriteLine(
        $"Duplicados.........: {saldosDuplicados.Count}");

    if (saldosDuplicados.Count > 0)
    {
        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "SALDOS DUPLICADOS:");

        Console.ResetColor();

        foreach (var grupo in
                 saldosDuplicados
                     .OrderBy(x => x.Key))
        {
            Console.WriteLine(
                $"  ! {grupo.Key}: {grupo.Count()} registros");
        }
    }

    Console.WriteLine();

    /*
     * HISTÓRICO PATRIMONIAL
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " HISTÓRICO PATRIMONIAL");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.HistoricosPatrimonio.Count}");

    Console.WriteLine(
        $"Duplicados.........: {historicosDuplicados.Count}");

    if (historicosDuplicados.Count > 0)
    {
        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "HISTÓRICOS DUPLICADOS:");

        Console.ResetColor();

        foreach (var grupo in
                 historicosDuplicados
                     .OrderBy(x => x.Key.Investidor)
                     .ThenBy(x => x.Key.Data))
        {
            Console.WriteLine(
                $"  ! {grupo.Key.Investidor} " +
                $"{grupo.Key.Data:dd/MM/yyyy}: " +
                $"{grupo.Count()} registros");
        }
    }

    Console.WriteLine();

    /*
     * INVESTIDORES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " INVESTIDORES");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Total..............: {dados.Investidores.Count}");

    foreach (var investidor in
             dados.Investidores.OrderBy(x => x))
    {
        Console.WriteLine(
            $"  - {investidor}");
    }

    Console.WriteLine();

    /*
     * SITUAÇÃO DAS OPÇÕES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " SITUAÇÃO DAS OPÇÕES");

    Console.WriteLine(
        "----------------------------------------");

    foreach (var grupo in
             dados.Opcoes
                 .GroupBy(x => x.Situacao)
                 .OrderBy(x => x.Key))
    {
        Console.WriteLine(
            $"{grupo.Key,-20}: {grupo.Count()}");
    }

    Console.WriteLine();

    /*
     * ENCERRADA exige DataFinalizacao.
     *
     * EXERCIDA não exige DataFinalizacao.
     */

    var opcoesEncerradas =
        dados.Opcoes
            .Where(x =>
                x.Situacao == "ENCERRADA")
            .ToList();

    var encerradasComData =
        opcoesEncerradas.Count(
            x => x.DataFinalizacao.HasValue);

    var encerradasSemData =
        opcoesEncerradas.Count -
        encerradasComData;

    var opcoesExercidas =
        dados.Opcoes
            .Where(x =>
                x.Situacao == "EXECUTADA")
            .ToList();

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " DATAS DE FINALIZAÇÃO DAS OPÇÕES");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Encerradas.............: {opcoesEncerradas.Count}");

    Console.WriteLine(
        $"Encerradas com data....: {encerradasComData}");

    Console.WriteLine(
        $"Encerradas sem data....: {encerradasSemData}");

    Console.WriteLine(
        $"Exercidas..............: {opcoesExercidas.Count}");

    Console.WriteLine(
        "Data em exercidas......: não obrigatória");

    if (encerradasSemData > 0)
    {
        Console.WriteLine();

        Console.WriteLine(
            "OPÇÕES ENCERRADAS SEM DATA:");

        foreach (var opcao in
                 opcoesEncerradas
                     .Where(x =>
                         !x.DataFinalizacao.HasValue))
        {
            Console.WriteLine(
                $"  ! Linha {opcao.LinhaExcel}: " +
                $"{opcao.TickerOpcao}");
        }
    }

    Console.WriteLine();

    /*
     * HISTÓRICO DAS OPERAÇÕES
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " VALIDAÇÃO DO HISTÓRICO");

    Console.WriteLine(
        "----------------------------------------");

    if (inconsistenciasHistoricas.Count == 0)
    {
        Console.ForegroundColor =
            ConsoleColor.Green;

        Console.WriteLine(
            "Todas as vendas possuem posição anterior suficiente.");

        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            $"Inconsistências encontradas: " +
            $"{inconsistenciasHistoricas.Count}");

        Console.ResetColor();

        Console.WriteLine();

        foreach (var inconsistencia in
                 inconsistenciasHistoricas)
        {
            Console.WriteLine(
                $"! {inconsistencia}");
        }
    }

    Console.WriteLine();

    /*
     * CLASSIFICAÇÃO DOS ATIVOS
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " CLASSIFICAÇÃO DOS ATIVOS");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Ativos utilizados.....: {dados.Ativos.Count}");

    Console.WriteLine(
        $"Classificados.........: {ativosClassificados.Count}");

    Console.WriteLine(
        $"Sem classificação.....: {ativosSemClassificacao.Count}");

    Console.WriteLine();

    if (ativosClassificados.Count > 0)
    {
        Console.WriteLine(
            "Classificados:");

        foreach (var ticker in
                 ativosClassificados)
        {
            var tipoAtivoCodigo =
                ObterTipoAtivoCodigo(ticker);

            Console.WriteLine(
                $"  {ticker,-12} " +
                $"→ {tipoAtivoCodigo}");
        }

        Console.WriteLine();
    }

    if (ativosSemClassificacao.Count > 0)
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "ATIVOS QUE PRECISAM DE CLASSIFICAÇÃO:");

        Console.ResetColor();

        foreach (var ticker in
                 ativosSemClassificacao)
        {
            if (cadastroPorTicker.TryGetValue(
                    ticker,
                    out var cadastro))
            {
                Console.WriteLine(
                    $"  ! {ticker,-12} " +
                    $"Tipo Excel='{cadastro.TipoOriginal}'");
            }
            else
            {
                Console.WriteLine(
                    $"  ! {ticker,-12} " +
                    $"não consta em 'Cotação Ativos'");
            }
        }

        Console.WriteLine();
    }

    /*
     * ATIVOS-BASE DAS OPÇÕES
     */

    var resolvedorAtivoBase =
        new ResolvedorAtivoBaseOpcao();

    var resolucoesAtivosBase =
        dados.Opcoes
            .Select(opcao =>
                new
                {
                    Opcao = opcao,

                    Resolucao =
                        resolvedorAtivoBase.Resolver(
                            opcao.TickerOpcao,
                            dados.Ativos)
                })
            .ToList();

    var opcoesBaseResolvida =
        resolucoesAtivosBase
            .Where(x =>
                x.Resolucao.Resolvido)
            .ToList();

    var opcoesBaseAmbigua =
        resolucoesAtivosBase
            .Where(x =>
                x.Resolucao.Ambiguo)
            .ToList();

    var opcoesBaseNaoResolvida =
        resolucoesAtivosBase
            .Where(x =>
                x.Resolucao.NaoResolvido)
            .ToList();

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " ATIVOS-BASE DAS OPÇÕES");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Opções................: {dados.Opcoes.Count}");

    Console.WriteLine(
        $"Resolvidas............: {opcoesBaseResolvida.Count}");

    Console.WriteLine(
        $"Ambíguas..............: {opcoesBaseAmbigua.Count}");

    Console.WriteLine(
        $"Não resolvidas........: {opcoesBaseNaoResolvida.Count}");

    Console.WriteLine();

    if (opcoesBaseResolvida.Count > 0)
    {
        Console.WriteLine(
            "Resolvidas:");

        foreach (var item in
                 opcoesBaseResolvida
                     .OrderBy(x =>
                         x.Opcao.LinhaExcel))
        {
            Console.WriteLine(
                $"  Linha {item.Opcao.LinhaExcel,-4} " +
                $"{item.Opcao.TickerOpcao,-12} " +
                $"→ {item.Resolucao.AtivoBase}");
        }

        Console.WriteLine();
    }

    if (opcoesBaseAmbigua.Count > 0)
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "AMBÍGUAS:");

        Console.ResetColor();

        foreach (var item in
                 opcoesBaseAmbigua
                     .OrderBy(x =>
                         x.Opcao.LinhaExcel))
        {
            Console.WriteLine(
                $"  ! Linha {item.Opcao.LinhaExcel}: " +
                $"{item.Opcao.TickerOpcao} → " +
                $"{string.Join(", ", item.Resolucao.Candidatos)}");
        }

        Console.WriteLine();
    }

    if (opcoesBaseNaoResolvida.Count > 0)
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            "NÃO RESOLVIDAS:");

        Console.ResetColor();

        foreach (var item in
                 opcoesBaseNaoResolvida
                     .OrderBy(x =>
                         x.Opcao.LinhaExcel))
        {
            Console.WriteLine(
                $"  ! Linha {item.Opcao.LinhaExcel}: " +
                $"{item.Opcao.TickerOpcao}");
        }

        Console.WriteLine();
    }

    /*
     * PENDÊNCIAS
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " PENDÊNCIAS DO EXCEL");

    Console.WriteLine(
        "----------------------------------------");

    if (dados.Pendencias.Count == 0)
    {
        Console.WriteLine(
            "Nenhuma pendência encontrada.");
    }
    else
    {
        foreach (var pendencia in
                 dados.Pendencias)
        {
            Console.WriteLine(
                $"! {pendencia}");
        }
    }

    Console.WriteLine();

    /*
     * Pendências auxiliares não bloqueiam a importação.
     *
     * Inconsistências efetivas e duplicidades bloqueiam.
     */

    var prontoParaImportar =
        inconsistenciasHistoricas.Count == 0 &&
        ativosSemClassificacao.Count == 0 &&
        encerradasSemData == 0 &&
        opcoesBaseAmbigua.Count == 0 &&
        opcoesBaseNaoResolvida.Count == 0 &&
        cotacoesDuplicadas.Count == 0 &&
        saldosDuplicados.Count == 0 &&
        historicosDuplicados.Count == 0;

    Console.WriteLine(
        "========================================");

    if (prontoParaImportar)
    {
        Console.ForegroundColor =
            ConsoleColor.Green;

        Console.WriteLine(
            " DADOS PRONTOS PARA IMPORTAÇÃO");

        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor =
            ConsoleColor.Yellow;

        Console.WriteLine(
            " DADOS AINDA POSSUEM PENDÊNCIAS");

        Console.ResetColor();
    }

    Console.WriteLine();

    /*
     * VERIFICAÇÃO DO BANCO
     */

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " VERIFICAÇÃO DO BANCO DE DADOS");

    Console.WriteLine(
        "----------------------------------------");

    await using var context =
        ConfiguracaoBancoDados.CriarContexto();

    var verificadorBanco =
        new VerificadorBancoDados(context);

    var resultadoBanco =
        await verificadorBanco.VerificarAsync();

    Console.WriteLine(
        $"Banco.................: " +
        $"{resultadoBanco.Banco ?? "(não identificado)"}");

    Console.WriteLine(
        $"Status................: " +
        $"{(resultadoBanco.Valido ? "OK" : "INVÁLIDO")}");

    Console.WriteLine(
        $"Detalhes..............: " +
        $"{resultadoBanco.Mensagem}");

    Console.WriteLine();

    /*
     * BLOQUEIOS ANTES DE QUALQUER ALTERAÇÃO.
     */

    if (!resultadoBanco.Valido)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            " IMPORTAÇÃO BLOQUEADA");

        Console.WriteLine();

        Console.WriteLine(
            " O banco de dados não passou na validação.");

        Console.WriteLine(
            " Nenhuma alteração foi realizada.");

        Console.WriteLine(
            "========================================");

        Console.ResetColor();

        Environment.ExitCode = 1;

        AguardarEncerramento();
        return;
    }

    if (!prontoParaImportar)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            " IMPORTAÇÃO BLOQUEADA");

        Console.WriteLine();

        Console.WriteLine(
            " O Excel possui inconsistências bloqueantes.");

        Console.WriteLine(
            " Nenhuma alteração foi realizada.");

        Console.WriteLine(
            "========================================");

        Console.ResetColor();

        Environment.ExitCode = 1;

        AguardarEncerramento();
        return;
    }

    /*
     * IMPORTAÇÃO REAL
     *
     * A partir deste ponto o CoordenadorImportacao:
     *
     * 1. abre uma transação;
     * 2. limpa os dados importáveis;
     * 3. recria investidores e ativos;
     * 4. importa operações;
     * 5. importa proventos;
     * 6. importa opções;
     * 7. importa cotações;
     * 8. importa saldos;
     * 9. importa histórico patrimonial;
     * 10. valida Excel x SQL;
     * 11. realiza COMMIT.
     *
     * Qualquer exceção provoca ROLLBACK.
     */

    Console.WriteLine(
        "========================================");

    Console.WriteLine(
        " INICIANDO IMPORTAÇÃO REAL");

    Console.WriteLine(
        "========================================");

    Console.WriteLine();

    Console.WriteLine(
        "O banco será reconstruído a partir do Excel.");

    Console.WriteLine(
        "A operação será executada dentro de uma transação.");

    Console.WriteLine();

    var coordenador =
        new CoordenadorImportacao(context);

    var resultadoImportacao =
        await coordenador.ExecutarAsync(dados);

    /*
     * VERIFICAÇÃO PÓS-IMPORTAÇÃO
     */

    var verificacaoFinal =
        await verificadorBanco.VerificarAsync();

    if (!verificacaoFinal.Valido)
    {
        throw new InvalidOperationException(
            "A importação terminou, mas a verificação final " +
            "do banco de dados falhou. " +
            verificacaoFinal.Mensagem);
    }

    Console.WriteLine();

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " RESULTADO DA IMPORTAÇÃO");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Investidores..........: {resultadoImportacao.Investidores}");

    Console.WriteLine(
        $"Ativos................: {resultadoImportacao.Ativos}");

    Console.WriteLine(
        $"Operações..............: {resultadoImportacao.Operacoes}");

    Console.WriteLine(
        $"Proventos..............: {resultadoImportacao.Proventos}");

    Console.WriteLine(
        $"Opções.................: {resultadoImportacao.Opcoes}");

    Console.WriteLine(
        $"Cotações...............: {resultadoImportacao.Cotacoes}");

    Console.WriteLine(
        $"Saldos disponíveis.....: {resultadoImportacao.SaldosDisponiveis}");

    Console.WriteLine(
        $"Históricos patrimoniais: {resultadoImportacao.HistoricosPatrimonio}");

    Console.WriteLine();

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        " VERIFICAÇÃO FINAL DO BANCO");

    Console.WriteLine(
        "----------------------------------------");

    Console.WriteLine(
        $"Banco.................: " +
        $"{verificacaoFinal.Banco ?? "(não identificado)"}");

    Console.WriteLine(
        $"Status................: " +
        $"{(verificacaoFinal.Valido ? "OK" : "INVÁLIDO")}");

    Console.WriteLine(
        $"Detalhes..............: " +
        $"{verificacaoFinal.Mensagem}");

    Console.WriteLine();

    /*
     * Conferência adicional das quantidades que vêm
     * diretamente do Excel.
     *
     * O total de Ativos não é hardcoded porque o importador
     * também pode precisar criar ativos-base encontrados
     * exclusivamente nas opções.
     */

    if (resultadoImportacao.Investidores !=
            dados.Investidores.Count ||
        resultadoImportacao.Operacoes !=
            dados.Operacoes.Count ||
        resultadoImportacao.Proventos !=
            dados.Proventos.Count ||
        resultadoImportacao.Opcoes !=
            dados.Opcoes.Count ||
        resultadoImportacao.Cotacoes !=
            dados.Cotacoes.Count ||
        resultadoImportacao.SaldosDisponiveis !=
            dados.SaldosDisponiveis.Count ||
        resultadoImportacao.HistoricosPatrimonio !=
            dados.HistoricosPatrimonio.Count)
    {
        throw new InvalidOperationException(
            "A conferência final encontrou divergência " +
            "entre o Excel e o resultado da importação.");
    }

    Console.ForegroundColor =
        ConsoleColor.Green;

    Console.WriteLine(
        "========================================");

    Console.WriteLine(
        " IMPORTAÇÃO CONCLUÍDA COM SUCESSO");

    Console.WriteLine();

    Console.WriteLine(
        " Excel e SQL Server estão consistentes.");

    Console.WriteLine(
        "========================================");

    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor =
        ConsoleColor.Red;

    Console.WriteLine();

    Console.WriteLine(
        "========================================");

    Console.WriteLine(
        " ERRO DURANTE A IMPORTAÇÃO");

    Console.WriteLine(
        "========================================");

    Console.WriteLine();

    Console.WriteLine(
        ex.ToString());

    Console.WriteLine();

    Console.WriteLine(
        "Se a falha ocorreu dentro da carga transacional, " +
        "o CoordenadorImportacao executou rollback.");

    Console.ResetColor();

    Environment.ExitCode = 1;
}

AguardarEncerramento();

static void AguardarEncerramento()
{
    Console.WriteLine();

    Console.WriteLine(
        "Pressione ENTER para encerrar...");

    Console.ReadLine();
}
