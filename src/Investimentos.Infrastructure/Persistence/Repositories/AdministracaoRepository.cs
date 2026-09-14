using Investimentos.Application.Administracao;
using Investimentos.Application.Carteira.ConsultarCarteira;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class AdministracaoRepository : IAdministracaoRepository
    {
        private readonly InvestimentosDbContext _context;
        private readonly CalcularCarteiraService
            _calcularCarteiraService;

        public AdministracaoRepository(
            InvestimentosDbContext context,
            CalcularCarteiraService calcularCarteiraService)
        {
            _context = context;
            _calcularCarteiraService =
                calcularCarteiraService;
        }

        public async Task<AdministracaoDto> ObterAsync(
            CancellationToken cancellationToken = default)
        {
            var classesBanco =
                await _context.ClassesAtivos
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync(cancellationToken);

            var tiposAtivoBanco =
                await _context.TiposAtivos
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync(cancellationToken);

            var tiposOperacaoBanco =
                await _context.TiposOperacoes
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync(cancellationToken);

            /*
             * Ticker é um Value Object.
             * O EF não consegue traduzir
             * OrderBy(x => x.Ticker.Codigo).
             *
             * Por isso primeiro materializamos
             * e depois ordenamos em memória.
             */
            var ativosBanco =
                (await _context.Ativos
                    .AsNoTracking()
                    .ToListAsync(cancellationToken))
                .OrderBy(x => x.Ticker.Codigo)
                .ToList();

            var cotacoesBanco =
                await _context.CotacoesAtivos
                    .AsNoTracking()
                    .OrderByDescending(
                        x => x.DataReferencia)
                    .ToListAsync(cancellationToken);

            var investidoresBanco =
                await _context.Investidores
                    .AsNoTracking()
                    .OrderBy(x => x.Nome)
                    .ToListAsync(cancellationToken);

            /*
             * Carregamos as operações necessárias para
             * calcular a posição atual de cada investidor
             * usando exatamente a mesma regra da Carteira.
             */
            var operacoesBanco =
                await _context.Operacoes
                    .AsNoTracking()
                    .OrderBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .Select(x =>
                        new
                        {
                            x.Id,
                            x.InvestidorId,
                            x.AtivoId,
                            Ticker =
                                x.Ativo.Ticker.Codigo,
                            Nome =
                                x.Ativo.Nome,
                            TipoAtivoCodigo =
                                x.Ativo
                                    .TipoAtivo
                                    .Codigo,
                            TipoAtivoNome =
                                x.Ativo
                                    .TipoAtivo
                                    .Nome,
                            TipoOperacao =
                                x.TipoOperacao.Codigo,
                            x.Quantidade,
                            x.PrecoUnitario,
                            x.Taxas,
                            x.Data,
                            x.Sequencia
                        })
                    .ToListAsync(cancellationToken);

            var saldosBanco =
                await _context.SaldosDisponiveis
                    .AsNoTracking()
                    .OrderByDescending(
                        x => x.DataReferencia)
                    .ToListAsync(cancellationToken);

            var historicosBanco =
                await _context.HistoricosPatrimonio
                    .AsNoTracking()
                    .OrderByDescending(
                        x => x.DataReferencia)
                    .ToListAsync(cancellationToken);

            /*
             * DescontoFiscal agora pertence à
             * carteira consolidada.
             *
             * Portanto, não existe mais filtro
             * por InvestidorId.
             */
            var descontosBanco =
                await _context.DescontosFiscais
                    .AsNoTracking()
                    .OrderByDescending(
                        x => x.DataPagamento)
                    .ThenBy(x => x.Tipo)
                    .ToListAsync(cancellationToken);

            var classesPorId =
                classesBanco.ToDictionary(
                    x => x.Id);

            var tiposAtivoPorId =
                tiposAtivoBanco.ToDictionary(
                    x => x.Id);

            var ultimaCotacaoPorAtivo =
                cotacoesBanco
                    .GroupBy(x => x.AtivoId)
                    .ToDictionary(
                        grupo => grupo.Key,
                        grupo => grupo.First());

            /*
             * Calcula a carteira atual individualmente
             * para cada investidor.
             *
             * Isso evita duplicar aqui as regras de
             * COMPRA/VENDA e garante que Administração
             * e Carteira interpretem as operações da
             * mesma maneira.
             */
            var posicoesPorTicker =
                new Dictionary<
                    string,
                    List<
                        PosicaoInvestidorAtivoAdministracaoDto>>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var investidor in investidoresBanco)
            {
                var operacoesInvestidor =
                    operacoesBanco
                        .Where(x =>
                            x.InvestidorId ==
                            investidor.Id)
                        .Select(x =>
                            new OperacaoCarteiraDto(
                                x.Id,
                                x.AtivoId,
                                x.Ticker,
                                x.Nome,
                                x.TipoOperacao,
                                x.Quantidade,
                                x.PrecoUnitario,
                                x.Taxas,
                                x.Data,
                                x.Sequencia)
                            {
                                TipoAtivoCodigo =
                                    x.TipoAtivoCodigo,

                                TipoAtivoNome =
                                    x.TipoAtivoNome
                            })
                        .ToList();

                var posicoesInvestidor =
                    _calcularCarteiraService.Calcular(
                        operacoesInvestidor);

                foreach (var posicao in
                         posicoesInvestidor)
                {
                    if (posicao.Quantidade <= 0)
                    {
                        continue;
                    }

                    if (!posicoesPorTicker.TryGetValue(
                            posicao.Ticker,
                            out var posicoes))
                    {
                        posicoes =
                            new List<
                                PosicaoInvestidorAtivoAdministracaoDto>();

                        posicoesPorTicker[
                            posicao.Ticker] =
                            posicoes;
                    }

                    posicoes.Add(
                        new PosicaoInvestidorAtivoAdministracaoDto(
                            investidor.Id,
                            investidor.Nome,
                            posicao.Quantidade));
                }
            }

            /*
             * ADMINISTRAÇÃO -> ATIVOS
             *
             * Todos os ativos continuam sendo retornados
             * pela API administrativa.
             *
             * Quantidade e PosicoesInvestidores refletem
             * a posição atual real.
             */
            var ativos =
                new List<AtivoAdministracaoDto>();

            foreach (var ativo in ativosBanco)
            {
                if (!tiposAtivoPorId.TryGetValue(
                        ativo.TipoAtivoId,
                        out var tipoAtivo))
                {
                    continue;
                }

                if (!classesPorId.TryGetValue(
                        tipoAtivo.ClasseAtivoId,
                        out var classeAtivo))
                {
                    continue;
                }

                ultimaCotacaoPorAtivo.TryGetValue(
                    ativo.Id,
                    out var cotacao);

                posicoesPorTicker.TryGetValue(
                    ativo.Ticker.Codigo,
                    out var posicoesInvestidores);

                posicoesInvestidores ??=
                    new List<
                        PosicaoInvestidorAtivoAdministracaoDto>();

                /*
                 * Quantidade administrativa consolidada:
                 * soma somente posições positivas.
                 *
                 * Não existe compensação entre
                 * investidores.
                 */
                var quantidade =
                    posicoesInvestidores.Sum(
                        x => x.Quantidade);

                ativos.Add(
                    new AtivoAdministracaoDto(
                        ativo.Id,
                        ativo.Ticker.Codigo,
                        ativo.Nome,
                        tipoAtivo.Id,
                        tipoAtivo.Codigo,
                        tipoAtivo.Nome,
                        classeAtivo.Id,
                        classeAtivo.Codigo,
                        classeAtivo.Nome,
                        quantidade,
                        posicoesInvestidores,
                        cotacao?.Preco,
                        cotacao?.DataReferencia));
            }

            /*
             * Investidores não possuem mais
             * DescontosFiscais.
             */
            var investidores =
                investidoresBanco
                    .Select(investidor =>
                    {
                        var saldoAtual =
                            saldosBanco
                                .FirstOrDefault(x =>
                                    x.InvestidorId ==
                                    investidor.Id);

                        var historicoInvestidorBanco =
                            historicosBanco
                                .Where(x =>
                                    x.InvestidorId ==
                                    investidor.Id)
                                .OrderByDescending(
                                    x =>
                                        x.DataReferencia)
                                .ToList();

                        var patrimonioAtual =
                            historicoInvestidorBanco
                                .FirstOrDefault();

                        var historicoInvestidor =
                            historicoInvestidorBanco
                                .Select(x =>
                                    new HistoricoPatrimonioAdministracaoDto(
                                        x.Id,
                                        x.DataReferencia,
                                        x.ValorCarteira))
                                .ToList();

                        return new InvestidorAdministracaoDto(
                            investidor.Id,
                            investidor.Nome,
                            saldoAtual?.Valor,
                            saldoAtual?.DataReferencia,
                            patrimonioAtual?.ValorCarteira,
                            patrimonioAtual?.DataReferencia,
                            historicoInvestidor);
                    })
                    .ToList();

            /*
             * Descontos fiscais da carteira.
             */
            var descontosFiscais =
                descontosBanco
                    .Select(x =>
                        new DescontoFiscalAdministracaoDto(
                            x.Id,
                            x.Tipo,
                            x.DataPagamento,
                            x.Valor,
                            x.Descricao))
                    .ToList();

            var classes =
                classesBanco
                    .Select(x =>
                        new ClasseAtivoAdministracaoDto(
                            x.Id,
                            x.Codigo,
                            x.Nome,
                            x.Ativo))
                    .ToList();

            var tiposAtivo =
                tiposAtivoBanco
                    .Where(x =>
                        classesPorId.ContainsKey(
                            x.ClasseAtivoId))
                    .Select(x =>
                    {
                        var classe =
                            classesPorId[
                                x.ClasseAtivoId];

                        return new TipoAtivoAdministracaoDto(
                            x.Id,
                            x.Codigo,
                            x.Nome,
                            x.Ativo,
                            classe.Id,
                            classe.Codigo,
                            classe.Nome);
                    })
                    .ToList();

            var tiposOperacao =
                tiposOperacaoBanco
                    .Select(x =>
                        new TipoOperacaoAdministracaoDto(
                            x.Id,
                            x.Codigo,
                            x.Nome,
                            x.Ativo))
                    .ToList();

            return new AdministracaoDto(
                ativos,
                investidores,
                classes,
                tiposAtivo,
                tiposOperacao,
                descontosFiscais);
        }

        public async Task<AtivoAdministracaoDto>
            CriarAtivoAsync(
                CriarAtivoAdministracaoRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            var tickerTexto =
                request.Ticker?
                    .Trim()
                    .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(
                    tickerTexto))
            {
                throw new ArgumentException(
                    "O ticker é obrigatório.");
            }

            /*
             * Ticker é Value Object.
             * Materializamos antes de comparar.
             */
            var ativosExistentes =
                await _context.Ativos
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

            var tickerExistente =
                ativosExistentes.Any(x =>
                    x.Ticker.Codigo.Equals(
                        tickerTexto,
                        StringComparison.OrdinalIgnoreCase));

            if (tickerExistente)
            {
                throw new InvalidOperationException(
                    $"O ativo {tickerTexto} já está cadastrado.");
            }

            var tipoAtivo =
                await _context.TiposAtivos
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            request.TipoAtivoId,
                        cancellationToken);

            if (tipoAtivo is null)
            {
                throw new ArgumentException(
                    "Tipo de ativo não encontrado.");
            }

            var classeAtivo =
                await _context.ClassesAtivos
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            tipoAtivo.ClasseAtivoId,
                        cancellationToken);

            if (classeAtivo is null)
            {
                throw new InvalidOperationException(
                    "A classe do tipo de ativo não foi encontrada.");
            }

            var nome =
                string.IsNullOrWhiteSpace(
                    request.Nome)
                    ? tickerTexto
                    : request.Nome.Trim();

            var ativo =
                new Ativo(
                    new Ticker(tickerTexto),
                    nome,
                    tipoAtivo);

            _context.Ativos.Add(
                ativo);

            CotacaoAtivo? cotacao = null;

            if (request.Cotacao.HasValue)
            {
                var dataCotacao =
                    (request.DataCotacao ??
                     DateTime.Today).Date;

                cotacao =
                    new CotacaoAtivo(
                        ativo,
                        dataCotacao,
                        request.Cotacao.Value);

                _context.CotacoesAtivos.Add(
                    cotacao);
            }

            await _context.SaveChangesAsync(
                cancellationToken);

            return new AtivoAdministracaoDto(
                ativo.Id,
                ativo.Ticker.Codigo,
                ativo.Nome,
                tipoAtivo.Id,
                tipoAtivo.Codigo,
                tipoAtivo.Nome,
                classeAtivo.Id,
                classeAtivo.Codigo,
                classeAtivo.Nome,
                0,
                Array.Empty<
                    PosicaoInvestidorAtivoAdministracaoDto>(),
                cotacao?.Preco,
                cotacao?.DataReferencia);
        }

        public async Task<AtivoAdministracaoDto?>
            AtualizarAtivoAsync(
                Guid ativoId,
                AtualizarAtivoAdministracaoRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            var ativo =
                await _context.Ativos
                    .FirstOrDefaultAsync(
                        x => x.Id == ativoId,
                        cancellationToken);

            if (ativo is null)
            {
                return null;
            }

            var tipoAtivo =
                await _context.TiposAtivos
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            request.TipoAtivoId,
                        cancellationToken);

            if (tipoAtivo is null)
            {
                throw new ArgumentException(
                    "Tipo de ativo não encontrado.");
            }

            var classeAtivo =
                await _context.ClassesAtivos
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                            tipoAtivo.ClasseAtivoId,
                        cancellationToken);

            if (classeAtivo is null)
            {
                throw new InvalidOperationException(
                    "A classe do tipo de ativo não foi encontrada.");
            }

            var nome =
                string.IsNullOrWhiteSpace(
                    request.Nome)
                    ? ativo.Ticker.Codigo
                    : request.Nome.Trim();

            ativo.Atualizar(
                nome,
                tipoAtivo);

            CotacaoAtivo? cotacao = null;

            if (request.Cotacao.HasValue)
            {
                var dataCotacao =
                    (request.DataCotacao ??
                     DateTime.Today).Date;

                var cotacoesAtivo =
                    await _context.CotacoesAtivos
                        .Where(x =>
                            x.AtivoId ==
                            ativo.Id)
                        .ToListAsync(
                            cancellationToken);

                cotacao =
                    cotacoesAtivo
                        .FirstOrDefault(x =>
                            x.DataReferencia.Date ==
                            dataCotacao);

                if (cotacao is null)
                {
                    cotacao =
                        new CotacaoAtivo(
                            ativo,
                            dataCotacao,
                            request.Cotacao.Value);

                    _context.CotacoesAtivos.Add(
                        cotacao);
                }
                else
                {
                    cotacao.Atualizar(
                        dataCotacao,
                        request.Cotacao.Value);
                }
            }
            else
            {
                var cotacoesAtivo =
                    await _context.CotacoesAtivos
                        .AsNoTracking()
                        .Where(x =>
                            x.AtivoId ==
                            ativo.Id)
                        .ToListAsync(
                            cancellationToken);

                cotacao =
                    cotacoesAtivo
                        .OrderByDescending(
                            x => x.DataReferencia)
                        .FirstOrDefault();
            }

            await _context.SaveChangesAsync(
                cancellationToken);

            return new AtivoAdministracaoDto(
                ativo.Id,
                ativo.Ticker.Codigo,
                ativo.Nome,
                tipoAtivo.Id,
                tipoAtivo.Codigo,
                tipoAtivo.Nome,
                classeAtivo.Id,
                classeAtivo.Codigo,
                classeAtivo.Nome,
                0,
                Array.Empty<
                    PosicaoInvestidorAtivoAdministracaoDto>(),
                cotacao?.Preco,
                cotacao?.DataReferencia);
        }
    }
}