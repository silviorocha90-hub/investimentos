using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;

namespace Investimentos.Api.Startup
{
    public static class BootstrapAdminService
    {
        public static async Task ExecutarAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var nome =
                configuration[
                    "BootstrapAdmin:Nome"];

            var email =
                configuration[
                    "BootstrapAdmin:Email"];

            var senha =
                configuration[
                    "BootstrapAdmin:Senha"];

            if (string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(senha))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                throw new InvalidOperationException(
                    "A configuração BootstrapAdmin está incompleta.");
            }

            using var scope =
                serviceProvider.CreateScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<
                        IUsuarioRepository>();

            if (await repository.ExistePorEmailAsync(
                email,
                cancellationToken))
            {
                return;
            }

            var passwordService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IPasswordService>();

            var senhaHash =
                passwordService.GerarHash(
                    senha);

            var admin =
                Usuario.CriarAdmin(
                    nome,
                    email,
                    senhaHash);

            admin.DefinirPermissoes(
                Enum.GetValues<
                    PermissaoSistema>());

            await repository.AdicionarAsync(
                admin,
                cancellationToken);
        }
    }
}