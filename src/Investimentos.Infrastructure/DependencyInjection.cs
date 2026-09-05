using Investimentos.Application.Interfaces;
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
                    "DefaultConnection");

            services.AddDbContext<InvestimentosDbContext>(
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

            return services;
        }
    }
}