using Investimentos.Domain.Entities;

namespace Investimentos.Application.Interfaces
{
    public interface IOperacaoOpcaoCrudRepository
    {
        Task<OperacaoOpcao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken);

        void Excluir(
            OperacaoOpcao operacaoOpcao);
    }
}