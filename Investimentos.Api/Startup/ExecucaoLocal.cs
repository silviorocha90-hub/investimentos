using Microsoft.Data.SqlClient;

namespace Investimentos.Api.Startup;

public static class ExecucaoLocal
{
    public const string Ambiente = "Local";

    public static void Configurar(WebApplicationBuilder builder, string[] args)
    {
        if (!builder.Environment.IsEnvironment(Ambiente)) return;

        // O SDK só carrega User Secrets automaticamente em Development.
        // Local usa o mesmo cofre, preservando precedência de ambiente e CLI.
        builder.Configuration.AddUserSecrets(typeof(ExecucaoLocal).Assembly, optional: true)
            .AddEnvironmentVariables().AddCommandLine(args);
        ValidarBanco(builder.Configuration.GetConnectionString("InvestimentosDb"));

        if (string.IsNullOrWhiteSpace(builder.Configuration["DATA_PROTECTION_KEYS_PATH"]))
        {
            builder.Configuration["DATA_PROTECTION_KEYS_PATH"] = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Aportiva", "DataProtection-Keys");
        }
    }

    public static void ValidarBanco(string? connectionString)
    {
        SqlConnectionStringBuilder conexao;
        try { conexao = new SqlConnectionStringBuilder(connectionString ?? ""); }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("A conexão do perfil Local é inválida. Configure o SQL Server local em User Secrets.");
        }
        var servidor = conexao.DataSource.Trim();
        if (servidor.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase)) servidor = servidor[4..];
        var host = servidor.Split('\\')[0].Split(',')[0];
        var local = host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host is "." or "127.0.0.1" or "[::1]" ||
            host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) ||
            servidor.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase);
        if (!local || !conexao.InitialCatalog.Equals("InvestimentosDb", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "O perfil Local exige o banco InvestimentosDb em localhost, SQLEXPRESS, LocalDB ou nesta máquina. " +
                "Uma conexão remota/Azure herdada foi bloqueada antes de aplicar migrations. Ajuste ConnectionStrings:InvestimentosDb em User Secrets.");
    }
}
