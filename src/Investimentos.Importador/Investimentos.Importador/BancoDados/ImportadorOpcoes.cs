using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorOpcoes
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorOpcoes(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<OpcaoImportacao> opcoes,
            IReadOnlyDictionary<string, Investidor> investidores,
            IReadOnlyDictionary<string, Ativo> ativos,
            IReadOnlyCollection<string> ativosHistoricos)
        {
            if (opcoes is null)
            {
                throw new ArgumentNullException(
                    nameof(opcoes));
            }

            if (investidores is null)
            {
                throw new ArgumentNullException(
                    nameof(investidores));
            }

            if (ativos is null)
            {
                throw new ArgumentNullException(
                    nameof(ativos));
            }

            if (ativosHistoricos is null)
            {
                throw new ArgumentNullException(
                    nameof(ativosHistoricos));
            }

            var entidades =
                new List<OperacaoOpcao>();

            var resolvedor =
                new ResolvedorAtivoBaseOpcao();

            var opcoesOrdenadas =
                opcoes
                    .OrderBy(x => x.DataOperacao)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in opcoesOrdenadas)
            {
                if (!investidores.TryGetValue(
                        item.Investidor,
                        out var investidor))
                {
                    throw new InvalidOperationException(
                        $"Investidor '{item.Investidor}' " +
                        $"não encontrado. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var resolucao =
                    resolvedor.Resolver(
                        item.TickerOpcao,
                        ativosHistoricos);

                if (!resolucao.Resolvido ||
                    string.IsNullOrWhiteSpace(
                        resolucao.AtivoBase))
                {
                    throw new InvalidOperationException(
                        $"Não foi possível determinar " +
                        $"o ativo-base da opção " +
                        $"'{item.TickerOpcao}'. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (!ativos.TryGetValue(
                        resolucao.AtivoBase,
                        out var ativo))
                {
                    throw new InvalidOperationException(
                        $"O ativo-base " +
                        $"'{resolucao.AtivoBase}' da opção " +
                        $"'{item.TickerOpcao}' não foi cadastrado. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (item.Quantidade <= 0)
                {
                    throw new InvalidOperationException(
                        $"Quantidade inválida na opção " +
                        $"'{item.TickerOpcao}'. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (item.Quantidade % 100m != 0)
                {
                    throw new InvalidOperationException(
                        $"A quantidade da opção " +
                        $"'{item.TickerOpcao}' não é " +
                        $"múltipla de 100. " +
                        $"Quantidade: {item.Quantidade}. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var contratosDecimal =
                    item.Quantidade / 100m;

                if (contratosDecimal > int.MaxValue)
                {
                    throw new InvalidOperationException(
                        $"Quantidade de contratos excede " +
                        $"o limite permitido. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var contratos =
                    checked((int)contratosDecimal);

                const decimal taxas = 0m;

                var entidade =
                    new OperacaoOpcao(
                        investidor,
                        ativo,
                        item.TickerOpcao,
                        item.TipoOpcao,
                        item.Natureza,
                        item.DataOperacao,
                        item.Vencimento,
                        item.Strike,
                        contratos,
                        item.Quantidade,
                        item.PremioUnitario,
                        taxas,
                        item.ResultadoInformado);

                AplicarSituacao(
                    entidade,
                    item);

                entidades.Add(
                    entidade);
            }

            await _context.OperacoesOpcoes
                .AddRangeAsync(entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }

        private static void AplicarSituacao(
            OperacaoOpcao entidade,
            OpcaoImportacao item)
        {
            var situacao =
                item.Situacao
                    .Trim()
                    .ToUpperInvariant();

            switch (situacao)
            {
                case "ABERTA":
                    return;

                case "ENCERRADA":
                    if (!item.DataFinalizacao.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"A opção encerrada " +
                            $"'{item.TickerOpcao}' não possui " +
                            $"data de finalização. " +
                            $"Linha Excel: {item.LinhaExcel}.");
                    }

                    if (!item.PrecoRecompraUnitario.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"A opção encerrada " +
                            $"'{item.TickerOpcao}' não possui " +
                            $"preço de recompra. " +
                            $"Linha Excel: {item.LinhaExcel}.");
                    }

                    entidade.Encerrar(
                        item.DataFinalizacao.Value,
                        item.PrecoRecompraUnitario.Value);

                    return;

                case "EXECUTADA":
                    entidade.MarcarExercida(
                        item.ValorExecucao);

                    return;

                case "EXPIRADA":
                    if (!item.DataFinalizacao.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"A opção expirada " +
                            $"'{item.TickerOpcao}' não possui " +
                            $"data de finalização. " +
                            $"Linha Excel: {item.LinhaExcel}.");
                    }

                    entidade.MarcarExpirada(
                        item.DataFinalizacao.Value);

                    return;

                default:
                    throw new InvalidOperationException(
                        $"Situação '{item.Situacao}' inválida " +
                        $"para a opção '{item.TickerOpcao}'. " +
                        $"Linha Excel: {item.LinhaExcel}.");
            }
        }
    }
}
