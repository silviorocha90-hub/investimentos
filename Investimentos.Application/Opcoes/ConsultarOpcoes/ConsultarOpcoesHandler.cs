using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Opcoes.ConsultarOpcoes
{
    public class ConsultarOpcoesHandler
    {
        private readonly IOperacaoOpcaoRepository _repository;

        public ConsultarOpcoesHandler(
            IOperacaoOpcaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<OperacaoOpcaoDto>> HandleAsync(
            Guid investidorId,
            CancellationToken cancellationToken = default)
        {
            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            var operacoes =
                await _repository.ListarAsync(
                    investidorId,
                    cancellationToken);

            return operacoes
                .Select(x =>
                    new OperacaoOpcaoDto(
                        x.Id,
                        x.Ativo.Ticker.Codigo,
                        x.TickerOpcao,
                        x.TipoOpcao,
                        x.Natureza,
                        x.DataOperacao,
                        x.Vencimento,
                        x.DataFinalizacao,
                        x.Strike,
                        x.Contratos,
                        x.Quantidade,
                        x.PremioUnitario,
                        x.PremioTotal,
                        x.Taxas,
                        x.PrecoRecompraUnitario,
                        x.ValorRecompraTotal,
                        x.ValorExecucao,
                        x.ResultadoInformado,
                        x.ResultadoFinal,
                        x.Situacao))
                .ToList();
        }
    }
}
