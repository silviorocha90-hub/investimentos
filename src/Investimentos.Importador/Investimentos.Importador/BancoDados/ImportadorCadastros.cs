using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Investimentos.Importador.Importacao;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Importador.BancoDados
{
    public class ImportadorCadastros
    {
        private readonly InvestimentosDbContext _context;

        public ImportadorCadastros(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoImportacaoCadastros>
            ImportarAsync(
                DadosImportacao dados)
        {
            if (dados is null)
            {
                throw new ArgumentNullException(
                    nameof(dados));
            }

            var tiposAtivos =
                await _context.TiposAtivos
                    .ToDictionaryAsync(
                        x => x.Codigo,
                        StringComparer.OrdinalIgnoreCase);

            var investidores =
                new Dictionary<string, Investidor>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var nome in
                     dados.Investidores
                         .OrderBy(x => x))
            {
                var nomeNormalizado =
                    nome.Trim();

                if (string.IsNullOrWhiteSpace(
                        nomeNormalizado))
                {
                    throw new InvalidOperationException(
                        "Foi encontrado um investidor sem nome.");
                }

                if (investidores.ContainsKey(
                        nomeNormalizado))
                {
                    continue;
                }

                var investidor =
                    new Investidor(
                        nomeNormalizado);

                investidores.Add(
                    nomeNormalizado,
                    investidor);
            }

            var tickersNecessarios =
                new HashSet<string>(
                    dados.Ativos,
                    StringComparer.OrdinalIgnoreCase);

            var resolvedor =
                new ResolvedorAtivoBaseOpcao();

            foreach (var opcao in dados.Opcoes)
            {
                var resolucao =
                    resolvedor.Resolver(
                        opcao.TickerOpcao,
                        dados.Ativos);

                if (!resolucao.Resolvido ||
                    string.IsNullOrWhiteSpace(
                        resolucao.AtivoBase))
                {
                    throw new InvalidOperationException(
                        $"Não foi possível determinar " +
                        $"o ativo-base da opção " +
                        $"'{opcao.TickerOpcao}'. " +
                        $"Linha Excel: {opcao.LinhaExcel}.");
                }

                tickersNecessarios.Add(
                    resolucao.AtivoBase);
            }

            var cadastroPorTicker =
                dados.CadastroAtivos
                    .GroupBy(
                        x => x.Ticker,
                        StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        x => x.Key,
                        x => x.First(),
                        StringComparer.OrdinalIgnoreCase);

            var ativos =
                new Dictionary<string, Ativo>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var tickerOriginal in
                     tickersNecessarios
                         .OrderBy(x => x))
            {
                var ticker =
                    tickerOriginal
                        .Trim()
                        .ToUpperInvariant();

                var tipoAtivoCodigo =
                    ObterTipoAtivoCodigo(
                        ticker,
                        cadastroPorTicker);

                if (tipoAtivoCodigo is null)
                {
                    throw new InvalidOperationException(
                        $"O ativo '{ticker}' não possui " +
                        $"classificação de TipoAtivo.");
                }

                if (!tiposAtivos.TryGetValue(
                        tipoAtivoCodigo,
                        out var tipoAtivo))
                {
                    throw new InvalidOperationException(
                        $"O TipoAtivo " +
                        $"'{tipoAtivoCodigo}' necessário " +
                        $"para o ativo '{ticker}' " +
                        $"não existe no banco.");
                }

                var nome =
                    ObterNomeAtivo(
                        ticker,
                        cadastroPorTicker);

                var ativo =
                    new Ativo(
                        new Ticker(ticker),
                        nome,
                        tipoAtivo);

                ativos.Add(
                    ticker,
                    ativo);
            }

            await _context.Investidores
                .AddRangeAsync(
                    investidores.Values);

            await _context.Ativos
                .AddRangeAsync(
                    ativos.Values);

            await _context.SaveChangesAsync();

            return new ResultadoImportacaoCadastros(
                investidores,
                ativos);
        }

        private static string? ObterTipoAtivoCodigo(
            string ticker,
            IReadOnlyDictionary<string, AtivoImportacao>
                cadastroPorTicker)
        {
            if (ticker.Equals(
                    "AXIA3",
                    StringComparison.OrdinalIgnoreCase) ||
                ticker.Equals(
                    "SANB4",
                    StringComparison.OrdinalIgnoreCase) ||
                ticker.Equals(
                    "ALOS3",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "ACAO";
            }

            if (cadastroPorTicker.TryGetValue(
                    ticker,
                    out var cadastro) &&
                cadastro.Classificado &&
                !string.IsNullOrWhiteSpace(
                    cadastro.TipoAtivoCodigo))
            {
                return cadastro.TipoAtivoCodigo;
            }

            return MapeamentoAtivosHistoricos
                .ObterTipoAtivoCodigo(
                    ticker);
        }

        private static string ObterNomeAtivo(
            string ticker,
            IReadOnlyDictionary<string, AtivoImportacao>
                cadastroPorTicker)
        {
            return ticker;
        }
    }

    public record ResultadoImportacaoCadastros(
        IReadOnlyDictionary<string, Investidor> Investidores,
        IReadOnlyDictionary<string, Ativo> Ativos);
}