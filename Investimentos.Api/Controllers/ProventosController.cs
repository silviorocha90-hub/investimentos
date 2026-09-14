using Investimentos.Application.Interfaces;
using Investimentos.Application.Proventos.ConsultarProventos;
using Investimentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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

        public ProventosController(
            IProventoRepository repository,
            ConsultarProventosHandler consultarHandler)
        {
            _repository = repository;
            _consultarHandler =
                consultarHandler;
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