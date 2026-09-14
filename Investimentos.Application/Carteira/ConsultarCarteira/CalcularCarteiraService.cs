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
                        x.Nome,
                        x.TipoAtivoCodigo,
                        x.TipoAtivoNome
                    })
                    .Select(grupo =>
                    {
                        decimal quantidade = 0;
                        decimal custoTotal = 0;
                        decimal resultadoRealizado = 0;

                        DateTime? dataPrimeiraCompra =
                            grupo
                                .Where(x =>
                                    x.TipoOperacao == "COMPRA")
                                .Select(x =>
                                    (DateTime?)x.Data)
                                .OrderBy(x => x)
                                .FirstOrDefault();

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
                                    throw new InvalidOperationException(
                                        $"Não existe posição disponível de {grupo.Key.Ticker} para venda.");
                                }

                                if (operacao.Quantidade > quantidade)
                                {
                                    throw new InvalidOperationException(
                                        $"Quantidade de venda de {grupo.Key.Ticker} é maior que a posição disponível. " +
                                        $"Disponível: {quantidade}. Venda: {operacao.Quantidade}.");
                                }

                                var precoMedio =
                                    custoTotal / quantidade;

                                var custoQuantidadeVendida =
                                    operacao.Quantidade *
                                    precoMedio;

                                var valorVenda =
                                    operacao.Quantidade *
                                    operacao.PrecoUnitario;

                                resultadoRealizado +=
                                    valorVenda
                                    - custoQuantidadeVendida
                                    - operacao.Taxas;

                                custoTotal -=
                                    custoQuantidadeVendida;

                                quantidade -=
                                    operacao.Quantidade;

                                if (quantidade == 0)
                                {
                                    custoTotal = 0;
                                }
                            }
                        }

                        var precoMedioFinal =
                            quantidade > 0
                                ? custoTotal / quantidade
                                : 0;

                        return new PosicaoAtivoDto(
                            grupo.Key.Ticker,
                            grupo.Key.Nome,
                            grupo.Key.TipoAtivoCodigo,
                            grupo.Key.TipoAtivoNome,
                            quantidade,
                            precoMedioFinal,
                            custoTotal,
                            resultadoRealizado,
                            dataPrimeiraCompra);
                    })
                    .Where(x =>
                        x.Quantidade > 0)
                    .OrderBy(x =>
                        x.Ticker)
                    .ToList();

            return posicoes;
        }
    }
}