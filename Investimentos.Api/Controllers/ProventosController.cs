using Investimentos.Application.Interfaces;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Domain.Entities;
using Investimentos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Controllers
{
    [ApiController]
    [Route("api/proventos")]
    public class ProventosController :
        ControllerBase
    {
        private readonly
            IProventoRepository _repository;

        private readonly
            ConsultarProventosHandler _consultarHandler;

        private readonly
            InvestimentosDbContext _context;

        public ProventosController(
            IProventoRepository repository,
            ConsultarProventosHandler consultarHandler,
            InvestimentosDbContext context)
        {
            _repository = repository;
            _consultarHandler =
                consultarHandler;

            _context = context;
        }

        [HttpGet("{investidorId:guid}")]
        public async Task<ActionResult<
            IReadOnlyList<ProventoDto>>> Listar(
                Guid investidorId,
                CancellationToken cancellationToken)
        {
            try
            {
                var proventos =
                    await _consultarHandler
                        .HandleAsync(
                            investidorId,
                            cancellationToken);

                return Ok(proventos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        mensagem =
                            ex.Message
                    });
            }
        }

        [HttpPost("ratear")]
        public async Task<ActionResult>
            CriarRateado(
                [FromBody]
                SalvarProventoTotalRequest request,
                CancellationToken cancellationToken)
        {
            try
            {
                if (request.ValorTotal <= 0)
                    return BadRequest(new { mensagem = "O valor total deve ser maior que zero." });

                var ativo =
                    await _repository.ObterAtivoPorTickerAsync(
                        request.Ticker,
                        cancellationToken);

                if (ativo is null)
                    return NotFound(new { mensagem = "Ativo não encontrado." });

                var dataBase =
                    (request.DataCom ?? request.DataPagamento).Date;

                var operacoes =
                    await _context.Operacoes
                        .AsNoTracking()
                        .Where(x =>
                            x.AtivoId == ativo.Id &&
                            x.Data.Date <= dataBase)
                        .Select(x => new
                        {
                            x.InvestidorId,
                            Tipo = x.TipoOperacao.Codigo,
                            x.Quantidade
                        })
                        .ToListAsync(cancellationToken);

                var posicoes =
                    operacoes
                        .GroupBy(x => x.InvestidorId)
                        .Select(grupo => new
                        {
                            InvestidorId = grupo.Key,
                            Quantidade = grupo.Sum(x =>
                                x.Tipo == "COMPRA"
                                    ? x.Quantidade
                                    : x.Tipo == "VENDA"
                                        ? -x.Quantidade
                                        : 0)
                        })
                        .Where(x => x.Quantidade > 0)
                        .ToList();

                var quantidadeTotal =
                    posicoes.Sum(x => x.Quantidade);

                if (quantidadeTotal <= 0)
                    return BadRequest(new
                    {
                        mensagem =
                            $"Não existem posições de {request.Ticker.ToUpperInvariant()} na data-base {dataBase:dd/MM/yyyy}."
                    });

                var investidores =
                    await _context.Investidores
                        .Where(x =>
                            posicoes.Select(p => p.InvestidorId)
                                .Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, cancellationToken);

                var valorUnitario =
                    request.ValorTotal / quantidadeTotal;

                decimal distribuido = 0;

                for (var indice = 0; indice < posicoes.Count; indice++)
                {
                    var posicao = posicoes[indice];

                    var valorRecebido =
                        indice == posicoes.Count - 1
                            ? request.ValorTotal - distribuido
                            : Math.Round(
                                request.ValorTotal *
                                posicao.Quantidade /
                                quantidadeTotal,
                                2,
                                MidpointRounding.AwayFromZero);

                    distribuido += valorRecebido;

                    var provento =
                        new Provento(
                            investidores[posicao.InvestidorId],
                            ativo,
                            request.Tipo,
                            request.Descricao,
                            request.DataCom,
                            request.DataPagamento,
                            posicao.Quantidade,
                            valorUnitario,
                            valorRecebido);

                    _context.Proventos.Add(provento);
                }

                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    quantidadeTotal,
                    valorUnitario,
                    investidores = posicoes.Count,
                    valorTotal = request.ValorTotal
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult>
            Criar(
                [FromBody]
                SalvarProventoRequest request,
                CancellationToken cancellationToken)
        {
            try
            {
                var investidor =
                    await _repository
                        .ObterInvestidorAsync(
                            request.InvestidorId,
                            cancellationToken);

                if (investidor is null)
                {
                    return NotFound(
                        new
                        {
                            mensagem =
                                "Investidor não encontrado."
                        });
                }

                var ativo =
                    await _repository
                        .ObterAtivoPorTickerAsync(
                            request.Ticker,
                            cancellationToken);

                if (ativo is null)
                {
                    return NotFound(
                        new
                        {
                            mensagem =
                                "Ativo não encontrado."
                        });
                }

                var provento =
                    new Provento(
                        investidor,
                        ativo,
                        request.Tipo,
                        request.Descricao,
                        request.DataCom,
                        request.DataPagamento,
                        request.QuantidadeBase,
                        request.ValorPorUnidade,
                        request.ValorRecebido);

                await _repository
                    .AdicionarAsync(
                        provento,
                        cancellationToken);

                return Created(
                    $"/api/proventos/{provento.Id}",
                    new
                    {
                        provento.Id
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        mensagem =
                            ex.Message
                    });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult>
            Atualizar(
                Guid id,
                [FromBody]
                AtualizarProventoRequest request,
                CancellationToken cancellationToken)
        {
            try
            {
                var provento =
                    await _repository
                        .ObterPorIdAsync(
                            id,
                            cancellationToken);

                if (provento is null)
                {
                    return NotFound(
                        new
                        {
                            mensagem =
                                "Provento não encontrado."
                        });
                }

                var ativo =
                    await _repository
                        .ObterAtivoPorTickerAsync(
                            request.Ticker,
                            cancellationToken);

                if (ativo is null)
                {
                    return NotFound(
                        new
                        {
                            mensagem =
                                "Ativo não encontrado."
                        });
                }

                provento.Atualizar(
                    ativo,
                    request.Tipo,
                    request.Descricao,
                    request.DataCom,
                    request.DataPagamento,
                    request.QuantidadeBase,
                    request.ValorPorUnidade,
                    request.ValorRecebido);

                await _repository
                    .SalvarAlteracoesAsync(
                        cancellationToken);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new
                    {
                        mensagem =
                            ex.Message
                    });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult>
            Excluir(
                Guid id,
                CancellationToken cancellationToken)
        {
            var provento =
                await _repository
                    .ObterPorIdAsync(
                        id,
                        cancellationToken);

            if (provento is null)
            {
                return NotFound(
                    new
                    {
                        mensagem =
                            "Provento não encontrado."
                    });
            }

            _repository.Excluir(
                provento);

            await _repository
                .SalvarAlteracoesAsync(
                    cancellationToken);

            return NoContent();
        }
    }

    public record SalvarProventoTotalRequest(
        string Ticker,
        string Tipo,
        string? Descricao,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal ValorTotal);

    public record SalvarProventoRequest(
        Guid InvestidorId,
        string Ticker,
        string Tipo,
        string? Descricao,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorRecebido);

    public record AtualizarProventoRequest(
        string Ticker,
        string Tipo,
        string? Descricao,
        DateTime? DataCom,
        DateTime DataPagamento,
        decimal QuantidadeBase,
        decimal ValorPorUnidade,
        decimal ValorRecebido);
}