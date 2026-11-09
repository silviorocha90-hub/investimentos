using System.Net;
using System.Text;
using Investimentos.Api.AssistenteIa;
using Investimentos.Api.ExceptionHandling;
using Investimentos.Domain.Entities;
using Investimentos.Domain.ValueObjects;
using Investimentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Investimentos.Api.Tests;

public class AtualizacaoManualCotacoesTests
{
    [Fact]
    public async Task CliqueManualAtualizaAtivosDeBolsaMesmoSemInvestidoresOuPosicoes()
    {
        await using var db = CriarBanco();
        var classe = new ClasseAtivo("TESTE", "Teste");
        foreach (var (ticker, tipo) in new[] { ("PETR4", "ACAO"), ("BTLG11", "FII"), ("LLFT11", "ETF"), ("PREVIDENCIA", "PREVIDENCIA") })
            db.Ativos.Add(new Ativo(new Ticker(ticker), ticker, new TipoAtivo(tipo, tipo, classe)));
        await db.SaveChangesAsync();
        var fonte = new FonteTeste();
        var service = CriarServico(db, fonte);

        var resultado = await service.AtualizarMercadoUmaVezAoDiaAsync(new DateTime(2026, 11, 9, 12, 0, 0), "manual", default);

        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Ativos.Count);
        Assert.Equal(3, await db.CotacoesAtivos.CountAsync());
        Assert.Equal(new[] { "BTLG11", "LLFT11", "PETR4" }, fonte.Tickers.Order().ToArray());
    }

    [Fact]
    public async Task NovoCliqueNoMesmoDiaConsultaFonteEPreservaHistoricoAnterior()
    {
        await using var db = CriarBanco();
        db.Ativos.Add(new Ativo(new Ticker("PETR4"), "Petrobras", new TipoAtivo("ACAO", "Ação", new ClasseAtivo("TESTE", "Teste"))));
        await db.SaveChangesAsync();
        var fonte = new FonteTeste();
        var service = CriarServico(db, fonte);
        var instante = new DateTime(2026, 11, 9, 12, 0, 0);

        await service.AtualizarMercadoUmaVezAoDiaAsync(instante, "manual", default);
        await service.AtualizarMercadoUmaVezAoDiaAsync(instante.AddMinutes(1), "manual", default);

        var precos = await db.CotacoesAtivos.OrderBy(x => x.DataReferencia).Select(x => x.Preco).ToListAsync();
        Assert.Equal(new[] { 31m, 32m }, precos);
        Assert.Equal(2, fonte.Tickers.Count);
        Assert.Null(await service.AtualizarMercadoUmaVezAoDiaAsync(instante.AddMinutes(2), "automatica", default));
        Assert.Equal(2, fonte.Tickers.Count);
    }

    private static InvestimentosDbContext CriarBanco() => new(new DbContextOptionsBuilder<InvestimentosDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AssistenteIaService CriarServico(InvestimentosDbContext db, FonteTeste fonte)
    {
        var config = new ConfigurationBuilder().Build();
        var logs = new LogAplicacaoService(null!, null!, NullLogger<LogAplicacaoService>.Instance);
        var mercado = new MercadoBrapiService(db, fonte, config, NullLogger<MercadoBrapiService>.Instance, logs);
        // Não há investidores: o fluxo exercitado consulta diretamente os ativos cadastrados.
        return new AssistenteIaService(db, null!, fonte, config, mercado, NullLogger<AssistenteIaService>.Instance);
    }

    private sealed class FonteTeste : HttpMessageHandler, IHttpClientFactory
    {
        public List<string> Tickers { get; } = [];
        public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ticker = Uri.UnescapeDataString(request.RequestUri!.Query.Split("symbols=")[1]);
            Tickers.Add(ticker);
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                results = new[] { new { symbol = ticker, regularMarketPrice = 30 + Tickers.Count } }
            });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
