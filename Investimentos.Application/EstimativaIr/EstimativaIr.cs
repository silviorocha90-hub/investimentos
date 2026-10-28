namespace Investimentos.Application.EstimativaIr;

public record MovimentoIr(Guid Id, Guid InvestidorId, string Investidor, string Ticker,
    string Categoria, string Tipo, DateTime Data, int Sequencia, decimal Quantidade,
    decimal Preco, decimal Taxas, decimal? ResultadoInformado = null, string? Pendencia = null);
public record ConfirmacaoIr(Guid OperacaoId, decimal Valor, string Observacao);
public record ItemIr(Guid OperacaoId, Guid InvestidorId, string Investidor, string Ticker,
    DateTime Data, string Tipo, string Categoria, decimal Quantidade, decimal ValorOperacao,
    decimal? Resultado, decimal Estimativa, decimal Retido, string Situacao, string Observacao);
public record ResumoInvestidorIr(Guid InvestidorId, string Investidor, decimal Resultado,
    decimal Estimativa, decimal Retido, decimal Total, int Pendencias);
public record RelatorioEstimativaIr(int Ano, DateTime CalculadoEmUtc, string Versao,
    IReadOnlyList<ResumoInvestidorIr> Investidores, IReadOnlyList<ItemIr> Operacoes,
    IReadOnlyList<string> Avisos);

// Rateio gerencial: perdas pertencem à carteira, mas o limite de vendas de ações é do CPF.
// Não representa a apuração fiscal única do titular, que pode compensar carteiras entre si.
public static class CalculadoraEstimativaIr
{
    public const string Versao = "1.0";
    public static RelatorioEstimativaIr Calcular(int ano, IEnumerable<MovimentoIr> movimentos,
        IEnumerable<ConfirmacaoIr>? confirmacoes = null)
    {
        var todos = movimentos.Where(x => x.Data.Year <= ano).OrderBy(x => x.Data)
            .ThenBy(x => x.Sequencia).ThenBy(x => x.Id).ToList();
        var confirmados = (confirmacoes ?? []).ToDictionary(x => x.OperacaoId);
        var itens = new List<ItemIr>();
        var vendasCpf = todos.Where(x => x.Categoria == "ACAO" && x.Tipo == "VENDA")
            .GroupBy(x => (x.Data.Year, x.Data.Month))
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantidade * y.Preco));
        var intradia = todos.Where(x => x.Tipo is "COMPRA" or "VENDA")
            .GroupBy(x => (x.InvestidorId, x.Ticker, x.Data.Date))
            .Where(x => x.Any(y => y.Tipo == "COMPRA") && x.Any(y => y.Tipo == "VENDA"))
            .Select(x => x.Key).ToHashSet();
        foreach (var grupo in todos.GroupBy(x => (x.InvestidorId, x.Ticker)))
        {
            decimal quantidade = 0, custo = 0;
            bool custoPendente = false;
            foreach (var m in grupo)
            {
                if (m.Tipo == "COMPRA")
                {
                    quantidade += m.Quantidade;
                    custo += m.Quantidade * m.Preco + m.Taxas;
                    continue;
                }
                decimal? resultado = null;
                var pendencia = m.Pendencia;
                var categoria = m.Categoria;
                if (m.Tipo == "VENDA")
                {
                    if (intradia.Contains((m.InvestidorId, m.Ticker, m.Data.Date)))
                        custoPendente = true;
                    if (m.Quantidade > quantidade || quantidade <= 0)
                    {
                        pendencia = "Compra/custo histórico insuficiente. Conferir notas e exercícios de opções.";
                        custoPendente = true;
                    }
                    else
                    {
                        var vendido = custo / quantidade * m.Quantidade;
                        resultado = m.Quantidade * m.Preco - m.Taxas - vendido;
                        quantidade -= m.Quantidade;
                        custo -= vendido;
                    }
                    if (custoPendente)
                        pendencia = "Custo exige conferência: há operação intradia ou histórico insuficiente neste ativo.";
                    if (categoria == "ACAO" && resultado > 0 &&
                        vendasCpf.GetValueOrDefault((m.Data.Year, m.Data.Month)) <= 20000)
                        categoria = "ISENTO";
                    if (categoria is not ("ACAO" or "ISENTO" or "FII" or "ETF_RV" or "BDR"))
                        pendencia ??= "Tributação não automatizada. Informe o IR definitivo retido na nota para esta operação.";
                }
                else
                {
                    resultado = m.ResultadoInformado;
                    if (resultado is null) pendencia ??= "Resultado ou data fiscal pendente de conferência.";
                }
                var retido = confirmados.GetValueOrDefault(m.Id);
                // Valor confirmado é retenção definitiva, nunca somado novamente à estimativa.
                itens.Add(new(m.Id, m.InvestidorId, m.Investidor, m.Ticker, m.Data, m.Tipo,
                    categoria, m.Quantidade, m.Quantidade * m.Preco, resultado, 0,
                    retido?.Valor ?? 0, retido is not null ? "RETIDO CONFIRMADO" :
                    pendencia is not null ? "PENDENTE" : categoria == "ISENTO" ? "ISENTO" : "ESTIMADO",
                    retido?.Observacao ?? pendencia ?? "Rateio mensal gerencial; não inclui IRRF antecipado nem DARF pago."));
            }
        }
        // Compensa perdas anteriores por carteira e grupo. Mantém perdas de anos anteriores.
        foreach (var grupo in itens.Where(x => x.Situacao == "ESTIMADO")
            .GroupBy(x => (x.InvestidorId, Grupo: x.Categoria == "FII" ? "FII" : x.Categoria == "DAY_TRADE" ? "DAY_TRADE" : "COMUM")))
        {
            decimal prejuizo = 0;
            foreach (var mes in grupo.GroupBy(x => (x.Data.Year, x.Data.Month)).OrderBy(x => x.Key))
            {
                var resultado = mes.Sum(x => x.Resultado ?? 0);
                var baseTributavel = Math.Max(0, resultado - prejuizo);
                prejuizo = Math.Max(0, prejuizo - resultado);
                var imposto = decimal.Round(baseTributavel * (grupo.Key.Grupo == "COMUM" ? .15m : .20m), 2, MidpointRounding.AwayFromZero);
                var positivos = mes.Where(x => x.Resultado > 0).OrderBy(x => x.OperacaoId).ToList();
                var soma = positivos.Sum(x => x.Resultado!.Value);
                decimal distribuido = 0;
                for (int i = 0; i < positivos.Count; i++)
                {
                    var item = positivos[i];
                    var valor = i == positivos.Count - 1 ? imposto - distribuido :
                        Math.Min(imposto - distribuido, decimal.Round(imposto * item.Resultado!.Value / soma, 2, MidpointRounding.AwayFromZero));
                    distribuido += valor;
                    itens[itens.IndexOf(item)] = item with { Estimativa = valor };
                }
            }
        }
        var anoItens = itens.Where(x => x.Data.Year == ano).OrderByDescending(x => x.Data).ThenBy(x => x.Investidor).ToList();
        var investidores = todos.Select(x => (x.InvestidorId, x.Investidor)).Distinct()
            .OrderBy(x => x.Investidor).Select(x =>
            {
                var lista = anoItens.Where(y => y.InvestidorId == x.InvestidorId).ToList();
                return new ResumoInvestidorIr(x.InvestidorId, x.Investidor, lista.Sum(y => y.Resultado ?? 0),
                    lista.Sum(y => y.Estimativa), lista.Sum(y => y.Retido),
                    lista.Sum(y => y.Estimativa + y.Retido), lista.Count(y => y.Situacao == "PENDENTE"));
            }).ToList();
        return new(ano, DateTime.UtcNow, Versao, investidores, anoItens, new[] {
            "Controle interno para acerto entre investidores; não é DARF nem imposto adicional da declaração anual. A apuração oficial é mensal e única por CPF.",
            "A isenção de ações usa as vendas mensais de todas as carteiras cadastradas. Operações externas ao sistema podem alterar o resultado.",
            "Perdas são compensadas por carteira desde o histórico disponível; a apuração oficial do CPF pode compensar carteiras entre si e divergir deste rateio.",
            "IRRF antecipado e DARFs pagos não são abatidos aqui. Retenção definitiva confirmada substitui a estimativa da operação, evitando duplicidade.",
            "Compras não geram imposto nesta tela. Intradia de ações, exercícios de opções e ativos sem regra definida exigem conferência; pendências não significam imposto zero." });
    }
}
