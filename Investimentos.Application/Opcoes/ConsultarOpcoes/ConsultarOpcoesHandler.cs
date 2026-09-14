using Investimentos.Application.Interfaces;

namespace Investimentos.Application.Opcoes.ConsultarOpcoes
{
    public class ConsultarOpcoesHandler
    {
        private readonly IOperacaoOpcaoRepository _repository;

        private readonly ICotacaoAtivoRepository?
            _cotacaoRepository;

        /*
         * Mantido para compatibilidade com os testes
         * existentes que instanciam o handler diretamente.
         */
        public ConsultarOpcoesHandler(
            IOperacaoOpcaoRepository repository)
        {
            _repository = repository;
            _cotacaoRepository = null;
        }

        /*
         * Construtor utilizado pela aplicação/API.
         *
         * Permite complementar as opções com a cotação
         * atual do ativo-base.
         */
        public ConsultarOpcoesHandler(
            IOperacaoOpcaoRepository repository,
            ICotacaoAtivoRepository cotacaoRepository)
        {
            _repository = repository;
            _cotacaoRepository = cotacaoRepository;
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

            IReadOnlyDictionary<string, decimal>
                cotacoes =
                    new Dictionary<string, decimal>(
                        StringComparer.OrdinalIgnoreCase);

            if (_cotacaoRepository is not null)
            {
                cotacoes =
                    await _cotacaoRepository
                        .ObterUltimasPorTickerAsync(
                            cancellationToken);
            }

            return operacoes
                .Select(
                    x =>
                    {
                        decimal? valorAcaoAtual = null;

                        if (cotacoes.TryGetValue(
                                x.Ativo.Ticker.Codigo,
                                out var cotacao))
                        {
                            valorAcaoAtual = cotacao;
                        }

                        return new OperacaoOpcaoDto(
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
                            x.Situacao)
                        {
                            ValorAcaoAtual =
                                valorAcaoAtual
                        };
                    })
                .ToList();
        }
    }
}