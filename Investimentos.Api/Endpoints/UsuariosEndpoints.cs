using System.Security.Claims;
using Investimentos.Application.Usuarios.Administracao;
using Investimentos.Domain.Usuarios;

namespace Investimentos.Api.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static WebApplication
            MapUsuariosEndpoints(
                this WebApplication app)
        {
            var grupo =
                app.MapGroup(
                        "/api/usuarios")
                    .RequireAuthorization(
                        policy =>
                            policy.RequireRole(
                                PerfilUsuario.Admin
                                    .ToString()));

            grupo.MapGet(
                "/",
                (Delegate)ListarAsync);

            grupo.MapGet(
                "/pendentes",
                (Delegate)ListarPendentesAsync);

            grupo.MapPut(
                "/{id:guid}/aprovar",
                (Delegate)AprovarAsync);

            grupo.MapPut(
                "/{id:guid}/rejeitar",
                (Delegate)RejeitarAsync);

            grupo.MapPut(
                "/{id:guid}/bloquear",
                (Delegate)BloquearAsync);

            grupo.MapPut(
                "/{id:guid}/desbloquear",
                (Delegate)DesbloquearAsync);

            grupo.MapPut(
                "/{id:guid}/acessos",
                (Delegate)AlterarAcessosAsync);

            return app;
        }

        private static async Task<IResult>
            ListarAsync(
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var usuarios =
                await service.ListarAsync(
                    cancellationToken);

            return Results.Ok(
                usuarios);
        }

        private static async Task<IResult>
            ListarPendentesAsync(
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var usuarios =
                await service.ListarAsync(
                    cancellationToken);

            return Results.Ok(
                usuarios.Where(
                    x =>
                        x.Status ==
                        StatusUsuario.Pendente
                            .ToString()));
        }

        private static async Task<IResult>
            AprovarAsync(
                Guid id,
                ClaimsPrincipal principal,
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var administradorId =
                ObterUsuarioAtualId(
                    principal);

            await service.AprovarAsync(
                id,
                administradorId,
                cancellationToken);

            return Results.NoContent();
        }

        private static async Task<IResult>
            RejeitarAsync(
                Guid id,
                ClaimsPrincipal principal,
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var administradorId =
                ObterUsuarioAtualId(
                    principal);

            await service.RejeitarAsync(
                id,
                administradorId,
                cancellationToken);

            return Results.NoContent();
        }

        private static async Task<IResult>
            BloquearAsync(
                Guid id,
                ClaimsPrincipal principal,
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var administradorId =
                ObterUsuarioAtualId(
                    principal);

            await service.BloquearAsync(
                id,
                administradorId,
                cancellationToken);

            return Results.NoContent();
        }

        private static async Task<IResult>
            DesbloquearAsync(
                Guid id,
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            await service.DesbloquearAsync(
                id,
                cancellationToken);

            return Results.NoContent();
        }

        private static async Task<IResult>
            AlterarAcessosAsync(
                Guid id,
                AlterarAcessosUsuarioRequest request,
                ClaimsPrincipal principal,
                AdministrarUsuariosService service,
                CancellationToken cancellationToken)
        {
            var administradorId =
                ObterUsuarioAtualId(
                    principal);

            if (!Enum.TryParse<PerfilUsuario>(
                    request.Perfil,
                    true,
                    out var perfil))
            {
                return Results.BadRequest(
                    new
                    {
                        detail =
                            "Perfil de usuário inválido."
                    });
            }

            var permissoes =
                new List<PermissaoSistema>();

            foreach (
                var permissaoTexto in
                request.Permissoes ??
                Array.Empty<string>())
            {
                if (!Enum.TryParse<
                        PermissaoSistema>(
                        permissaoTexto,
                        true,
                        out var permissao))
                {
                    return Results.BadRequest(
                        new
                        {
                            detail =
                                $"Permissão inválida: {permissaoTexto}."
                        });
                }

                permissoes.Add(
                    permissao);
            }

            await service.AlterarAcessosAsync(
                id,
                administradorId,
                perfil,
                permissoes,
                request.InvestidoresIds ??
                    Array.Empty<Guid>(),
                request.AcessosInvestidores?
                    .Select(x => new AcessoInvestidorDto(
                        x.InvestidorId,
                        (x.Permissoes ?? Array.Empty<string>())
                            .Select(p => Enum.Parse<PermissaoSistema>(p, true))
                            .Where(p => p != PermissaoSistema.Administracao && p != PermissaoSistema.Operacoes)
                            .ToArray()))
                    .ToArray(),
                cancellationToken);

            return Results.NoContent();
        }

        private static Guid
            ObterUsuarioAtualId(
                ClaimsPrincipal principal)
        {
            var id =
                principal.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                    id,
                    out var usuarioId))
            {
                throw new InvalidOperationException(
                    "Não foi possível identificar o usuário autenticado.");
            }

            return usuarioId;
        }
    }

    public record AlterarAcessosUsuarioRequest(
        string Perfil,
        IReadOnlyCollection<string>?
            Permissoes,
        IReadOnlyCollection<Guid>?
            InvestidoresIds,
        IReadOnlyCollection<AcessoInvestidorRequest>?
            AcessosInvestidores);

    public record AcessoInvestidorRequest(
        Guid InvestidorId,
        IReadOnlyCollection<string>? Permissoes);
}