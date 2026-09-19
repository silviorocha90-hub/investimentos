using Investimentos.Application.Interfaces;
using Investimentos.Infrastructure.Identity;
using Investimentos.Infrastructure.Persistence;
using Investimentos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Investimentos.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString(
                    "InvestimentosDb");

            if (string.IsNullOrWhiteSpace(
                connectionString))
            {
                throw new InvalidOperationException(
                    "A connection string 'InvestimentosDb' não foi configurada.");
            }

            services.AddDbContext<
                InvestimentosDbContext>(
                options =>
                    options.UseSqlServer(
                        connectionString));

            services.AddScoped<
                IInvestidorRepository,
                InvestidorRepository>();

            services.AddScoped<
                IAtivoRepository,
                AtivoRepository>();

            services.AddScoped<
                IOperacaoRepository,
                OperacaoRepository>();

            services.AddScoped<
                ICarteiraRepository,
                CarteiraRepository>();

            services.AddScoped<
                IProventoRepository,
                ProventoRepository>();

            services.AddScoped<
                IOperacaoOpcaoRepository,
                OperacaoOpcaoRepository>();

            services.AddScoped<
                ISaldoDisponivelRepository,
                SaldoDisponivelRepository>();

            services.AddScoped<
                IDescontoFiscalRepository,
                DescontoFiscalRepository>();

            services.AddScoped<
                IHistoricoPatrimonioRepository,
                HistoricoPatrimonioRepository>();

            services.AddScoped<
                ICotacaoAtivoRepository,
                CotacaoAtivoRepository>();

            services.AddScoped<
                IValorPatrimonialAtivoRepository,
                ValorPatrimonialAtivoRepository>();

            services.AddScoped<
                IUsuarioRepository,
                UsuarioRepository>();

            services.AddScoped<
                ITokenRecuperacaoSenhaRepository,
                TokenRecuperacaoSenhaRepository>();

            services.AddScoped<
                IPasswordService,
                PasswordService>();

            return services;
        }
    }
}