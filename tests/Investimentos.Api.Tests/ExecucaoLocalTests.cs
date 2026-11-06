using Investimentos.Api.AssistenteIa;
using Investimentos.Api.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Investimentos.Api.Tests;

public class ExecucaoLocalTests
{
    [Theory]
    [InlineData("Production", "true", true)]
    [InlineData("Local", "true", true)]
    [InlineData("Local", "false", false)]
    [InlineData("Local", null, false)]
    [InlineData("Development", "true", false)]
    [InlineData("Staging", "true", false)]
    [InlineData("Production", null, false)]
    public void AgendaExigeAmbienteEscolhidoEFlag(string ambiente, string? flag, bool esperado)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ASSISTENTE_IA_ATIVO"] = flag }).Build();
        Assert.Equal(esperado, AssistenteIaWorker.GeracaoHabilitada(config, new AmbienteTeste(ambiente)));
    }

    [Theory]
    [InlineData("Server=localhost;Database=InvestimentosDb;Trusted_Connection=True;")]
    [InlineData("Server=.\\SQLEXPRESS;Database=InvestimentosDb;Trusted_Connection=True;")]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=InvestimentosDb;Trusted_Connection=True;")]
    [InlineData("Server=tcp:127.0.0.1,1433;Database=InvestimentosDb;User Id=local;Password=teste;")]
    public void AceitaSomenteDestinoLocalConhecido(string conexao) => ExecucaoLocal.ValidarBanco(conexao);

    [Theory]
    [InlineData("Server=sql-aportiva-prod.database.windows.net;Database=aportiva;User Id=admin;Password=segredo;")]
    [InlineData("Server=localhost;Database=master;Trusted_Connection=True;")]
    [InlineData("Server=outro-servidor;Database=InvestimentosDb;Trusted_Connection=True;")]
    [InlineData(null)]
    [InlineData("texto-invalido")]
    public void BloqueiaDestinoRemotoOuBancoErradoSemExporCredenciais(string? conexao)
    {
        var erro = Assert.Throws<InvalidOperationException>(() => ExecucaoLocal.ValidarBanco(conexao));
        Assert.DoesNotContain("segredo", erro.Message);
    }

    [Fact]
    public void PerfilLocalValidaConexaoDeLinhaDeComandoAntesDeIniciarBanco()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Local", Args = [] });
        Assert.Throws<InvalidOperationException>(() => ExecucaoLocal.Configurar(builder,
            ["--ConnectionStrings:InvestimentosDb=Server=sql-remoto.database.windows.net;Database=aportiva;User Id=admin;Password=segredo;"]));
    }

    [Fact]
    public void PerfilLocalConfiguraPersistenciaDasChavesNoDiretorioDoUsuario()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Local", Args = [] });
        ExecucaoLocal.Configurar(builder,
            ["--ConnectionStrings:InvestimentosDb=Server=localhost;Database=InvestimentosDb;Trusted_Connection=True;", "--DATA_PROTECTION_KEYS_PATH="]);
        var caminho = builder.Configuration["DATA_PROTECTION_KEYS_PATH"]!;
        Assert.True(Path.IsPathFullyQualified(caminho));
        Assert.EndsWith(Path.Combine("Aportiva", "DataProtection-Keys"), caminho);
    }

    private sealed class AmbienteTeste(string nome) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = nome;
        public string ApplicationName { get; set; } = "Investimentos.Api";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
