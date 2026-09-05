namespace Investimentos.Application.Carteira.ConsultarCarteira
{
    public class CalcularCarteiraService
    {
        public IReadOnlyList<PosicaoAtivoDto> Calcular(
            IEnumerable<OperacaoCarteiraDto> operacoes)
        {
            if (operacoes is null)
            {
                throw new ArgumentNullException(
                    nameof(operacoes));
            }

            var posicoes =
                operacoes
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .GroupBy(x => new
                    {
                        x.AtivoId,
                        x.Ticker,
                        x.Nome
                    })
                    .Select(grupo =>
                    {
                        decimal quantidade = 0;
                        decimal custoTotal = 0;
                        decimal resultadoRealizado = 0;

                        foreach (var operacao in grupo)
                        {
                            if (operacao.TipoOperacao == "COMPRA")
                            {
                                quantidade +=
                                    operacao.Quantidade;

                                custoTotal +=
                                    (operacao.Quantidade *
                                     operacao.PrecoUnitario)
                                    + operacao.Taxas;

                                continue;
                            }

                            if (operacao.TipoOperacao == "VENDA")
                            {
                                if (quantidade <= 0)
                                {
                                    continue;
                                }

                                var precoMedio =
                                    custoTotal / quantidade;

                                var quantidadeVendida =
                                    Math.Min(
                                        operacao.Quantidade,
                                        quantidade);

                                var valorVenda =
                                    quantidadeVendida *
                                    operacao.PrecoUnitario;

                                var custoQuantidadeVendida =
                                    quantidadeVendida *
                                    precoMedio;

                                resultadoRealizado +=
                                    valorVenda
                                    - custoQuantidadeVendida
                                    - operacao.Taxas;

                                custoTotal -=
                                    custoQuantidadeVendida;

                                quantidade -=
                                    quantidadeVendida;
                            }
                        }

                        var precoMedioFinal =
                            quantidade > 0
                                ? custoTotal / quantidade
                                : 0;

                        return new PosicaoAtivoDto(
                            grupo.Key.Ticker,
                            grupo.Key.Nome,
                            quantidade,
                            precoMedioFinal,
                            custoTotal,
                            resultadoRealizado);
                    })
                    .Where(x =>
                        x.Quantidade > 0 ||
                        x.ResultadoRealizado != 0)
                    .OrderBy(x =>
                        x.Ticker)
                    .ToList();

            return posicoes;
        }
    }
}