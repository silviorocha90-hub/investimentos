using Investimentos.Domain.Entities;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorOperacoes
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorOperacoes(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<int> ImportarAsync(
            IReadOnlyCollection<OperacaoImportacao> operacoes,
            IReadOnlyDictionary<string, Investidor> investidores,
            IReadOnlyDictionary<string, Ativo> ativos)
        {
            if (operacoes is null)
            {
                throw new ArgumentNullException(
                    nameof(operacoes));
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

            var tiposOperacao =
                await _context.TiposOperacoes
                    .ToDictionaryAsync(
                        x => x.Codigo,
                        StringComparer.OrdinalIgnoreCase);

            if (!tiposOperacao.ContainsKey("COMPRA"))
            {
                throw new InvalidOperationException(
                    "O TipoOperacao COMPRA não está cadastrado.");
            }

            if (!tiposOperacao.ContainsKey("VENDA"))
            {
                throw new InvalidOperationException(
                    "O TipoOperacao VENDA não está cadastrado.");
            }

            var sequencias =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            var entidades =
                new List<Operacao>();

            var operacoesOrdenadas =
                operacoes
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.LinhaExcel)
                    .ToList();

            foreach (var item in operacoesOrdenadas)
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

                if (!ativos.TryGetValue(
                        item.Ativo,
                        out var ativo))
                {
                    throw new InvalidOperationException(
                        $"Ativo '{item.Ativo}' " +
                        $"não encontrado. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                if (!tiposOperacao.TryGetValue(
                        item.TipoOperacao,
                        out var tipoOperacao))
                {
                    throw new InvalidOperationException(
                        $"Tipo de operação " +
                        $"'{item.TipoOperacao}' inválido. " +
                        $"Linha Excel: {item.LinhaExcel}.");
                }

                var chaveSequencia =
                    $"{item.Investidor.Trim().ToUpperInvariant()}" +
                    $"|{item.Data:yyyyMMdd}";

                if (!sequencias.TryGetValue(
                        chaveSequencia,
                        out var sequenciaAtual))
                {
                    sequenciaAtual = 0;
                }

                var proximaSequencia =
                    sequenciaAtual + 1;

                sequencias[chaveSequencia] =
                    proximaSequencia;

                var operacao =
                    new Operacao(
                        item.Data,
                        proximaSequencia,
                        investidor,
                        ativo,
                        tipoOperacao,
                        item.Quantidade,
                        item.PrecoUnitario,
                        item.Taxas);

                entidades.Add(operacao);
            }

            await _context.Operacoes
                .AddRangeAsync(entidades);

            await _context.SaveChangesAsync();

            return entidades.Count;
        }
    }
}