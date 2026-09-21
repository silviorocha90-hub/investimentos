using Investimentos.Application.Interfaces;
using Investimentos.Application.Operacoes.AtualizarOperacao;
using Investimentos.Application.Operacoes.ExcluirOperacao;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Tests.Operacoes;

public class AtualizarExcluirOperacaoHandlerTests
{
    [Fact]
    public async Task Atualizar_DeveAlterarDadosESalvar()
    {
        var operacao = CriarOperacao();
        var repository = new FakeRepository(operacao);
        var handler = new AtualizarOperacaoHandler(repository);
        var data = new DateTime(2026, 9, 21);

        await handler.HandleAsync(
            new AtualizarOperacaoCommand(
                operacao.Id, data, 200, 41.25m, 12.50m));

        Assert.Equal(data, operacao.Data);
        Assert.Equal(200, operacao.Quantidade);
        Assert.Equal(41.25m, operacao.PrecoUnitario);
        Assert.Equal(12.50m, operacao.Taxas);
        Assert.Equal(1, repository.Salvamentos);
    }

    [Fact]
    public async Task Atualizar_ComIdVazio_DeveFalhar()
    {
        var handler = new AtualizarOperacaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                new AtualizarOperacaoCommand(
                    Guid.Empty, DateTime.Today, 1, 1, 0)));
    }

    [Fact]
    public async Task Atualizar_OperacaoInexistente_DeveFalhar()
    {
        var handler = new AtualizarOperacaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(
                new AtualizarOperacaoCommand(
                    Guid.NewGuid(), DateTime.Today, 1, 1, 0)));
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(-1, 10, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 10, -1)]
    public async Task Atualizar_DadosInvalidos_DeveFalhar(
        decimal quantidade,
        decimal preco,
        decimal taxas)
    {
        var operacao = CriarOperacao();
        var repository = new FakeRepository(operacao);
        var handler = new AtualizarOperacaoHandler(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                new AtualizarOperacaoCommand(
                    operacao.Id,
                    DateTime.Today,
                    quantidade,
                    preco,
                    taxas)));

        Assert.Equal(0, repository.Salvamentos);
    }

    [Fact]
    public async Task Excluir_DeveRemoverQuandoHistoricoPermaneceValido()
    {
        var operacao = CriarOperacao();
        var repository = new FakeRepository(operacao)
        {
            HistoricoValido = true
        };
        var handler = new ExcluirOperacaoHandler(repository);

        await handler.HandleAsync(operacao.Id);

        Assert.True(repository.Removida);
        Assert.Equal(1, repository.Salvamentos);
    }

    [Fact]
    public async Task Excluir_ComIdVazio_DeveFalhar()
    {
        var handler = new ExcluirOperacaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(Guid.Empty));
    }

    [Fact]
    public async Task Excluir_OperacaoInexistente_DeveFalhar()
    {
        var handler = new ExcluirOperacaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Excluir_QuandoHistoricoFicariaInvalido_NaoDeveRemover()
    {
        var operacao = CriarOperacao();
        var repository = new FakeRepository(operacao)
        {
            HistoricoValido = false
        };
        var handler = new ExcluirOperacaoHandler(repository);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(operacao.Id));

        Assert.Contains("histórico", exception.Message);
        Assert.False(repository.Removida);
        Assert.Equal(0, repository.Salvamentos);
    }

    private static Operacao CriarOperacao()
    {
        var classe = new ClasseAtivo("RENDA_VARIAVEL", "Renda Variável");
        var tipoAtivo = new TipoAtivo("ACAO", "Ação", classe);
        var ativo = new Ativo(new Ticker("ITUB4"), "Itaú Unibanco", tipoAtivo);
        var investidor = new Investidor("Silvio");
        var tipoOperacao = new TipoOperacao("COMPRA", "Compra");

        return new Operacao(
            new DateTime(2026, 9, 1),
            1,
            investidor,
            ativo,
            tipoOperacao,
            100,
            40m,
            0);
    }

    private sealed class FakeRepository : IOperacaoRepository
    {
        private readonly Operacao? _operacao;

        public bool HistoricoValido { get; set; } = true;
        public bool Removida { get; private set; }
        public int Salvamentos { get; private set; }

        public FakeRepository(Operacao? operacao = null)
        {
            _operacao = operacao;
        }

        public Task<Operacao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _operacao?.Id == id ? _operacao : null);

        public void Remover(Operacao operacao) => Removida = true;

        public Task SalvarAsync(
            CancellationToken cancellationToken = default)
        {
            Salvamentos++;
            return Task.CompletedTask;
        }

        public Task<bool> HistoricoPermaneceValidoSemOperacaoAsync(
            Guid operacaoId,
            Guid investidorId,
            Guid ativoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HistoricoValido);

        public Task AdicionarAsync(Operacao operacao, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Investidor?> ObterInvestidorPorIdAsync(Guid investidorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Investidor?>(null);

        public Task<Ativo?> ObterAtivoPorTickerAsync(string ticker, CancellationToken cancellationToken = default) =>
            Task.FromResult<Ativo?>(null);

        public Task<TipoOperacao?> ObterTipoOperacaoPorCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
            Task.FromResult<TipoOperacao?>(null);

        public Task<int> ObterProximaSequenciaAsync(Guid investidorId, DateTime data, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task<decimal> ObterQuantidadeDisponivelAsync(Guid investidorId, Guid ativoId, DateTime data, int sequencia, CancellationToken cancellationToken = default) =>
            Task.FromResult(0m);
    }
}
