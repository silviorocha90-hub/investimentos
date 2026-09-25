using Investimentos.Application.Interfaces;
using Investimentos.Application.Opcoes.AtualizarOperacaoOpcao;
using Investimentos.Application.Opcoes.ExcluirOperacaoOpcao;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;

namespace Investimentos.Application.Tests.Opcoes;

public class AtualizarExcluirOperacaoOpcaoHandlerTests
{
    [Fact]
    public async Task Atualizar_ExecutadaParaEncerrada_DevePersistirResultado()
    {
        var opcao = CriarOpcaoExecutada();
        var repository = new FakeRepository(opcao);
        var handler = new AtualizarOperacaoOpcaoHandler(repository);
        var finalizacao = new DateTime(2026, 9, 15);

        await handler.HandleAsync(
            CriarCommand(
                opcao,
                situacao: "ENCERRADA",
                dataFinalizacao: finalizacao,
                precoRecompra: 0.40m,
                resultadoInformado: 75m),
            CancellationToken.None);

        Assert.Equal("ENCERRADA", opcao.Situacao);
        Assert.Equal(finalizacao, opcao.DataFinalizacao);
        Assert.Equal(0.40m, opcao.PrecoRecompraUnitario);
        Assert.Equal(75m, opcao.ResultadoInformado);
        Assert.Equal(75m, opcao.ResultadoFinal);
        Assert.Equal(1, repository.Salvamentos);
    }

    [Fact]
    public async Task Atualizar_ExecutadaSemDataFinalizacao_DeveFalhar()
    {
        var opcao = CriarOpcaoExecutada();
        var handler = new AtualizarOperacaoOpcaoHandler(new FakeRepository(opcao));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                CriarCommand(
                    opcao,
                    situacao: "ENCERRADA",
                    dataFinalizacao: null,
                    precoRecompra: 0.40m),
                CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_ExecutadaSemPrecoRecompra_DeveFalhar()
    {
        var opcao = CriarOpcaoExecutada();
        var handler = new AtualizarOperacaoOpcaoHandler(new FakeRepository(opcao));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(
                CriarCommand(
                    opcao,
                    situacao: "ENCERRADA",
                    dataFinalizacao: new DateTime(2026, 9, 15),
                    precoRecompra: null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_Aberta_DeveFalhar()
    {
        var opcao = CriarOpcao();
        var handler = new AtualizarOperacaoOpcaoHandler(new FakeRepository(opcao));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CriarCommand(
                    opcao,
                    situacao: "EXECUTADA"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_OpcaoFinalizadaNaoPodeMudarSituacao()
    {
        var opcao = CriarOpcao();
        opcao.MarcarExpirada();
        var handler = new AtualizarOperacaoOpcaoHandler(new FakeRepository(opcao));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CriarCommand(opcao, situacao: "EXECUTADA"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_OpcaoInexistente_DeveFalhar()
    {
        var handler = new AtualizarOperacaoOpcaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(
                new AtualizarOperacaoOpcaoCommand(
                    Guid.NewGuid(),
                    DateTime.Today,
                    DateTime.Today.AddDays(30),
                    10m, 1, 100, 1m, 0,
                    "ABERTA", null, null, null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Excluir_DeveRemoverESalvar()
    {
        var opcao = CriarOpcao();
        var repository = new FakeRepository(opcao);
        var handler = new ExcluirOperacaoOpcaoHandler(repository);

        await handler.HandleAsync(opcao.Id, CancellationToken.None);

        Assert.True(repository.Excluida);
        Assert.Equal(1, repository.Salvamentos);
    }

    [Fact]
    public async Task Excluir_Inexistente_DeveFalhar()
    {
        var handler = new ExcluirOperacaoOpcaoHandler(new FakeRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private static AtualizarOperacaoOpcaoCommand CriarCommand(
        OperacaoOpcao opcao,
        string situacao = "ABERTA",
        DateTime? dataFinalizacao = null,
        decimal? precoRecompra = null,
        decimal? valorExecucao = null,
        decimal? resultadoInformado = null) =>
        new(
            opcao.Id,
            opcao.DataOperacao,
            opcao.Vencimento,
            opcao.Strike,
            opcao.Contratos,
            opcao.Quantidade,
            opcao.PremioUnitario,
            opcao.Taxas,
            situacao,
            dataFinalizacao,
            precoRecompra,
            valorExecucao,
            resultadoInformado);

    private static OperacaoOpcao CriarOpcaoExecutada()
    {
        var opcao = CriarOpcao();
        opcao.MarcarExercida(500m);
        return opcao;
    }

    private static OperacaoOpcao CriarOpcao()
    {
        var classe = new ClasseAtivo("RENDA_VARIAVEL", "Renda Variável");
        var tipo = new TipoAtivo("ACAO", "Ação", classe);
        var ativo = new Ativo(new Ticker("VALE3"), "Vale", tipo);

        return new OperacaoOpcao(
            new Investidor("Silvio"),
            ativo,
            "VALEX790",
            "PUT",
            "VENDA",
            new DateTime(2026, 9, 1),
            new DateTime(2026, 10, 16),
            79m,
            5,
            500,
            1.41m,
            5m);
    }

    private sealed class FakeRepository : IOperacaoOpcaoCrudRepository
    {
        private readonly OperacaoOpcao? _opcao;
        public int Salvamentos { get; private set; }
        public bool Excluida { get; private set; }

        public FakeRepository(OperacaoOpcao? opcao = null) => _opcao = opcao;

        public Task<OperacaoOpcao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(_opcao?.Id == id ? _opcao : null);

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
        {
            Salvamentos++;
            return Task.CompletedTask;
        }

        public void Excluir(OperacaoOpcao operacaoOpcao) => Excluida = true;
    }
}
