using ClosedXML.Excel;
using System.Globalization;

namespace Investimentos.Importador.Importacao
{
    public class LeitorExcelInvestimentos
    {
        public DadosImportacao Ler(
            string caminhoArquivo)
        {
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
            {
                throw new ArgumentException(
                    "O caminho do arquivo é obrigatório.");
            }

            if (!File.Exists(caminhoArquivo))
            {
                throw new FileNotFoundException(
                    "O arquivo de investimentos não foi encontrado.",
                    caminhoArquivo);
            }

            var dados =
                new DadosImportacao();

            using var workbook =
                new XLWorkbook(caminhoArquivo);

            LerOperacoes(
                workbook,
                dados);

            LerProventos(
                workbook,
                dados);

            LerOpcoes(
                workbook,
                dados);

            LerCadastroAtivos(
                workbook,
                dados);

            LerSaldosDisponiveis(
                workbook,
                dados);

            LerHistoricoPatrimonio(
                workbook,
                dados);

            return dados;
        }

        private static void LerOperacoes(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Qtde Ativos (bkp)";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var data =
                    ObterData(linha.Cell(1));

                var ativo =
                    NormalizarTexto(linha.Cell(2));

                var investidor =
                    NormalizarTexto(linha.Cell(3));

                var tipoOperacao =
                    NormalizarTexto(linha.Cell(4))
                        .ToUpperInvariant();

                if (data is null &&
                    string.IsNullOrWhiteSpace(ativo))
                {
                    continue;
                }

                if (data is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Operação sem data.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(ativo))
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Operação sem ativo.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(investidor) ||
                    investidor == "-")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Operação sem investidor válido. " +
                        $"Data={data.Value:dd/MM/yyyy}, " +
                        $"Ativo={ativo}, " +
                        $"Transação={tipoOperacao}, " +
                        $"Investidor='{investidor}'.");

                    continue;
                }

                if (tipoOperacao != "COMPRA" &&
                    tipoOperacao != "VENDA")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Operação sem classificação. " +
                        $"Ativo={ativo}, " +
                        $"Investidor={investidor}, " +
                        $"Transação={tipoOperacao}");

                    continue;
                }

                var quantidade =
                    ObterDecimal(linha.Cell(5));

                var precoUnitario =
                    ObterDecimal(linha.Cell(6));

                var taxas =
                    ObterDecimal(linha.Cell(7));

                var custoTotal =
                    ObterDecimalOpcional(
                        linha.Cell(8));

                var valorRecebido =
                    ObterDecimalOpcional(
                        linha.Cell(9));

                if (quantidade <= 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Quantidade inválida para {ativo}.");

                    continue;
                }

                if (precoUnitario < 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Preço inválido para {ativo}.");

                    continue;
                }

                var ticker =
                    ativo.ToUpperInvariant();

                dados.Operacoes.Add(
                    new OperacaoImportacao(
                        linha.RowNumber(),
                        data.Value,
                        ticker,
                        investidor,
                        tipoOperacao,
                        quantidade,
                        precoUnitario,
                        taxas,
                        custoTotal,
                        valorRecebido));

                dados.Ativos.Add(ticker);
                dados.Investidores.Add(investidor);
            }
        }

        private static void LerProventos(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Proventos Ativos";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var data =
                    ObterData(linha.Cell(1));

                var ativo =
                    NormalizarTexto(linha.Cell(2));

                if (data is null &&
                    string.IsNullOrWhiteSpace(ativo))
                {
                    continue;
                }

                var descricao =
                    NormalizarTexto(linha.Cell(3));

                var investidor =
                    NormalizarTexto(linha.Cell(4));

                var quantidade =
                    ObterDecimal(linha.Cell(5));

                var valorPorUnidade =
                    ObterDecimal(linha.Cell(6));

                var valorRecebido =
                    ObterDecimal(linha.Cell(7));

                var tipoOriginal =
                    NormalizarTexto(linha.Cell(8));

                var tipo =
                    NormalizarTipoProvento(
                        tipoOriginal);

                if (data is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Provento sem data.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(ativo))
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Provento sem ativo.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(investidor) ||
                    investidor == "-")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Provento sem investidor válido.");

                    continue;
                }

                if (tipo is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Tipo de provento desconhecido: " +
                        $"'{tipoOriginal}'.");

                    continue;
                }

                if (quantidade <= 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Quantidade base inválida para {ativo}.");

                    continue;
                }

                if (valorPorUnidade < 0 ||
                    valorRecebido < 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Valor inválido no provento de {ativo}.");

                    continue;
                }

                var ticker =
                    ativo.ToUpperInvariant();

                dados.Proventos.Add(
                    new ProventoImportacao(
                        linha.RowNumber(),
                        data.Value,
                        ticker,
                        descricao,
                        investidor,
                        quantidade,
                        valorPorUnidade,
                        valorRecebido,
                        tipo));

                dados.Ativos.Add(ticker);
                dados.Investidores.Add(investidor);
            }
        }

        private static void LerOpcoes(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Opções Ativos";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var dataOperacao =
                    ObterData(linha.Cell(1));

                var dataFinalizacao =
                    ObterData(linha.Cell(2));

                var investidor =
                    NormalizarTexto(linha.Cell(3));

                var tickerOpcao =
                    NormalizarTexto(linha.Cell(4));

                var tipoOpcao =
                    NormalizarTexto(linha.Cell(5))
                        .ToUpperInvariant();

                var natureza =
                    NormalizarTexto(linha.Cell(6))
                        .ToUpperInvariant();

                if (dataOperacao is null &&
                    string.IsNullOrWhiteSpace(tickerOpcao))
                {
                    continue;
                }

                var quantidade =
                    ObterDecimal(linha.Cell(7));

                var strike =
                    ObterDecimal(linha.Cell(8));

                var vencimento =
                    ObterData(linha.Cell(10));

                var premioUnitario =
                    ObterDecimal(linha.Cell(11));

                var precoRecompra =
                    ObterDecimalOpcional(
                        linha.Cell(12));

                var resultadoInformado =
                    ObterDecimalOpcional(
                        linha.Cell(13));

                var statusOriginal =
                    NormalizarTexto(linha.Cell(14));

                var valorExecucao =
                    ObterDecimalOpcional(
                        linha.Cell(16));

                if (dataOperacao is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Opção sem data de operação.");

                    continue;
                }

                if (vencimento is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Opção {tickerOpcao} sem vencimento.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(investidor) ||
                    investidor == "-")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Opção {tickerOpcao} sem investidor válido.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(tickerOpcao))
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Opção sem ticker.");

                    continue;
                }

                if (tipoOpcao != "PUT" &&
                    tipoOpcao != "CALL")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Tipo inválido para {tickerOpcao}: " +
                        $"'{tipoOpcao}'.");

                    continue;
                }

                if (natureza != "COMPRA" &&
                    natureza != "VENDA")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Natureza inválida para {tickerOpcao}: " +
                        $"'{natureza}'.");

                    continue;
                }

                if (quantidade <= 0 ||
                    strike <= 0 ||
                    premioUnitario < 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Valores inválidos para a opção " +
                        $"{tickerOpcao}.");

                    continue;
                }

                var situacao =
                    NormalizarSituacaoOpcao(
                        statusOriginal,
                        precoRecompra);

                if (situacao is null)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Situação desconhecida para " +
                        $"{tickerOpcao}: '{statusOriginal}'.");

                    continue;
                }

                dados.Opcoes.Add(
                    new OpcaoImportacao(
                        linha.RowNumber(),
                        dataOperacao.Value,
                        dataFinalizacao,
                        investidor,
                        tickerOpcao.ToUpperInvariant(),
                        tipoOpcao,
                        natureza,
                        quantidade,
                        strike,
                        vencimento.Value,
                        premioUnitario,
                        precoRecompra,
                        resultadoInformado,
                        valorExecucao,
                        situacao));

                dados.Investidores.Add(investidor);
            }
        }

        private static void LerCadastroAtivos(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Cotação Ativos";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var ticker =
                    NormalizarTexto(linha.Cell(1))
                        .ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(ticker))
                {
                    continue;
                }

                var tipoOriginal =
                    NormalizarTexto(linha.Cell(2));

                /*
                 * A planilha possui uma linha auxiliar:
                 *
                 * VALOR | Disponível | ...
                 *
                 * Ela representa valor disponível,
                 * não um ativo financeiro.
                 */
                if (tipoOriginal.Equals(
                        "Disponível",
                        StringComparison.OrdinalIgnoreCase) ||
                    tipoOriginal.Equals(
                        "Disponivel",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var precoAtual =
                    ObterDecimalOpcional(
                        linha.Cell(3));

                var tipoAtivoCodigo =
                    NormalizarTipoAtivo(
                        tipoOriginal);

                dados.CadastroAtivos.Add(
                    new AtivoImportacao(
                        ticker,
                        tipoOriginal,
                        tipoAtivoCodigo,
                        precoAtual));

                dados.Ativos.Add(ticker);

                if (!precoAtual.HasValue)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Cotação não informada para {ticker}.");

                    continue;
                }

                if (precoAtual.Value < 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Cotação inválida para {ticker}: " +
                        $"{precoAtual.Value}.");

                    continue;
                }

                dados.Cotacoes.Add(
                    new CotacaoAtivoImportacao(
                        linha.RowNumber(),
                        ticker,
                        precoAtual.Value));
            }
        }

        private static void LerSaldosDisponiveis(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Valores Disponíveis";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var investidor =
                    NormalizarTexto(linha.Cell(1));

                if (string.IsNullOrWhiteSpace(investidor))
                {
                    continue;
                }

                if (investidor == "-")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Saldo disponível sem investidor válido.");

                    continue;
                }

                var valor =
                    ObterDecimalOpcional(
                        linha.Cell(2));

                if (!valor.HasValue)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Saldo disponível não informado para " +
                        $"{investidor}.");

                    continue;
                }

                dados.SaldosDisponiveis.Add(
                    new SaldoDisponivelImportacao(
                        linha.RowNumber(),
                        investidor,
                        valor.Value));

                dados.Investidores.Add(investidor);
            }
        }

        private static void LerHistoricoPatrimonio(
            XLWorkbook workbook,
            DadosImportacao dados)
        {
            const string nomePlanilha =
                "Linha do Tempo";

            if (!workbook.Worksheets.Contains(nomePlanilha))
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' não encontrada.");

                return;
            }

            var worksheet =
                workbook.Worksheet(nomePlanilha);

            var range =
                worksheet.RangeUsed();

            if (range is null)
            {
                dados.Pendencias.Add(
                    $"Planilha '{nomePlanilha}' está vazia.");

                return;
            }

            foreach (var linha in
                     range.RowsUsed().Skip(1))
            {
                var investidor =
                    NormalizarTexto(linha.Cell(1));

                var dataReferencia =
                    ObterData(linha.Cell(2));

                var valorCarteira =
                    ObterDecimalOpcional(
                        linha.Cell(3));

                if (string.IsNullOrWhiteSpace(investidor) &&
                    dataReferencia is null &&
                    !valorCarteira.HasValue)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(investidor) ||
                    investidor == "-")
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        "Histórico patrimonial sem investidor válido.");

                    continue;
                }

                if (!dataReferencia.HasValue)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Histórico patrimonial de {investidor} " +
                        $"sem data.");

                    continue;
                }

                if (!valorCarteira.HasValue)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Histórico patrimonial de {investidor} " +
                        $"sem valor da carteira.");

                    continue;
                }

                if (valorCarteira.Value < 0)
                {
                    AdicionarPendencia(
                        dados,
                        linha,
                        $"Valor patrimonial inválido para " +
                        $"{investidor}: {valorCarteira.Value}.");

                    continue;
                }

                dados.HistoricosPatrimonio.Add(
                    new HistoricoPatrimonioImportacao(
                        linha.RowNumber(),
                        investidor,
                        dataReferencia.Value,
                        valorCarteira.Value));

                dados.Investidores.Add(investidor);
            }
        }

        private static string? NormalizarTipoAtivo(
            string tipo)
        {
            var valor =
                tipo.Trim()
                    .ToUpperInvariant();

            return valor switch
            {
                "AÇÃO" => "ACAO",
                "ACAO" => "ACAO",
                "AÇÕES" => "ACAO",
                "ACOES" => "ACAO",

                "FII" => "FII",
                "FUNDO IMOBILIÁRIO" => "FII",
                "FUNDO IMOBILIARIO" => "FII",

                "ETF" => "ETF",

                "BDR" => "BDR",

                "TESOURO DIRETO" => "TESOURO_DIRETO",

                "CDB" => "CDB",
                "LCI" => "LCI",
                "LCA" => "LCA",

                "DEBÊNTURE" => "DEBENTURE",
                "DEBENTURE" => "DEBENTURE",

                "POUPANÇA" => "POUPANCA",
                "POUPANCA" => "POUPANCA",

                "FUNDO DE RENDA FIXA" =>
                    "FUNDO_RENDA_FIXA",

                "FUNDO DE AÇÕES" =>
                    "FUNDO_ACOES",

                "FUNDO DE ACOES" =>
                    "FUNDO_ACOES",

                "FUNDO MULTIMERCADO" =>
                    "FUNDO_MULTIMERCADO",

                "FUNDO CAMBIAL" =>
                    "FUNDO_CAMBIAL",

                "CRIPTOMOEDA" =>
                    "CRIPTOMOEDA",

                "CÂMBIO" =>
                    "CAMBIO",

                "CAMBIO" =>
                    "CAMBIO",

                "COMMODITY" =>
                    "COMMODITY",

                "OURO" =>
                    "OURO",

                _ => null
            };
        }

        private static string? NormalizarTipoProvento(
            string tipo)
        {
            return tipo
                .Trim()
                .ToUpperInvariant() switch
            {
                "DIVIDENDO" => "DIVIDENDO",
                "RENDIMENTO" => "RENDIMENTO",
                "JUROS" => "JCP",
                "JCP" => "JCP",
                _ => null
            };
        }

        private static string? NormalizarSituacaoOpcao(
            string situacao,
            decimal? precoRecompra)
        {
            var valor =
                situacao.Trim()
                    .ToUpperInvariant();

            return valor switch
            {
                "EXECUTADA" => "EXECUTADA",
                "EXERCIDA" => "EXECUTADA",
                "EXPIRADA" => "EXPIRADA",
                "ABERTA" => "ABERTA",

                "ENCERRADA" =>
                    precoRecompra.HasValue
                        ? "ENCERRADA"
                        : "EXPIRADA",

                _ => null
            };
        }

        private static string NormalizarTexto(
            IXLCell cell)
        {
            return cell
                .GetFormattedString()
                .Trim();
        }

        private static DateTime? ObterData(
            IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return null;
            }

            if (cell.TryGetValue<DateTime>(
                    out var data))
            {
                return data;
            }

            var texto =
                cell.GetFormattedString()
                    .Trim();

            if (DateTime.TryParse(
                    texto,
                    new CultureInfo("pt-BR"),
                    DateTimeStyles.None,
                    out data))
            {
                return data;
            }

            return null;
        }

        private static decimal ObterDecimal(
            IXLCell cell)
        {
            return ObterDecimalOpcional(cell) ?? 0m;
        }

        private static decimal? ObterDecimalOpcional(
            IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return null;
            }

            if (cell.TryGetValue<decimal>(
                    out var valor))
            {
                return valor;
            }

            var texto =
                cell.GetFormattedString()
                    .Trim();

            if (string.IsNullOrWhiteSpace(texto) ||
                texto.Equals(
                    "NA",
                    StringComparison.OrdinalIgnoreCase) ||
                texto.Equals(
                    "N/A",
                    StringComparison.OrdinalIgnoreCase) ||
                texto == "-")
            {
                return null;
            }

            if (decimal.TryParse(
                    texto,
                    NumberStyles.Any,
                    new CultureInfo("pt-BR"),
                    out valor))
            {
                return valor;
            }

            return null;
        }

        private static void AdicionarPendencia(
            DadosImportacao dados,
            IXLRangeRow linha,
            string mensagem)
        {
            dados.Pendencias.Add(
                $"Linha {linha.RowNumber()}: {mensagem}");
        }
    }
}
