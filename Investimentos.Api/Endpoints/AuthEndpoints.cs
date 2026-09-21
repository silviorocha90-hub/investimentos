using System.Security.Claims;
using Investimentos.Application.Interfaces;
using Investimentos.Application.Usuarios.Autenticacao;
using Investimentos.Domain.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Investimentos.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static WebApplication MapAuthEndpoints(
            this WebApplication app)
        {
            app.MapPost(
                "/api/auth/cadastro",
                CadastrarAsync);

            app.MapPost(
                "/api/auth/login",
                LoginAsync);

            /*
             * Logout propositalmente não exige autorização.
             *
             * A operação é idempotente:
             * mesmo que a sessão já tenha expirado,
             * o cookie local deve ser removido.
             */
            app.MapPost(
                "/api/auth/logout",
                LogoutAsync);

            app.MapGet(
                    "/api/auth/me",
                    ObterUsuarioAtualAsync)
                .RequireAuthorization();

            return app;
        }

        private static async Task<IResult> CadastrarAsync(
            CadastroRequest request,
            AutenticacaoService service,
            CancellationToken cancellationToken)
        {
            try
            {
                var id =
                    await service.CadastrarAsync(
                        request.Nome,
                        request.Email,
                        request.Senha,
                        cancellationToken);

                return Results.Created(
                    $"/api/usuarios/{id}",
                    new
                    {
                        id,
                        mensagem =
                            "Cadastro realizado com sucesso. Aguarde a aprovação do administrador."
                    });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(
                    new
                    {
                        detail = ex.Message
                    });
            }
        }

        private static async Task<IResult> LoginAsync(
            LoginRequest request,
            AutenticacaoService service,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            var resultado =
                await service.LoginAsync(
                    request.Email,
                    request.Senha,
                    cancellationToken);

            if (!resultado.Autenticado ||
                resultado.Usuario is null)
            {
                return Results.Json(
                    new
                    {
                        detail = resultado.Mensagem
                    },
                    statusCode:
                        StatusCodes.Status401Unauthorized);
            }

            var usuario =
                resultado.Usuario;

            var claims =
                CriarClaims(usuario);

            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc =
                        DateTimeOffset.UtcNow
                            .AddHours(8)
                });

            return Results.Ok(
                CriarUsuarioAtual(usuario));
        }

        private static async Task<IResult> LogoutAsync(
            HttpContext httpContext)
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return Results.NoContent();
        }

        private static async Task<IResult>
            ObterUsuarioAtualAsync(
                ClaimsPrincipal principal,
                IUsuarioRepository repository,
                CancellationToken cancellationToken)
        {
            var idClaim =
                principal.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                idClaim,
                out var usuarioId))
            {
                return Results.Unauthorized();
            }

            var usuario =
                await repository.ObterPorIdAsync(
                    usuarioId,
                    cancellationToken);

            if (usuario is null ||
                usuario.Status != StatusUsuario.Ativo)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(
                CriarUsuarioAtual(usuario));
        }

        private static IEnumerable<Claim> CriarClaims(
            Usuario usuario)
        {
            var claims =
                new List<Claim>
                {
                    new(
                        ClaimTypes.NameIdentifier,
                        usuario.Id.ToString()),

                    new(
                        ClaimTypes.Name,
                        usuario.Nome),

                    new(
                        ClaimTypes.Email,
                        usuario.Email),

                    new(
                        ClaimTypes.Role,
                        usuario.Perfil.ToString())
                };

            foreach (
                var permissao in
                usuario.Permissoes)
            {
                claims.Add(
                    new Claim(
                        "permissao",
                        permissao.Permissao
                            .ToString()));
            }

            foreach (
                var investidor in
                usuario.Investidores)
            {
                claims.Add(
                    new Claim(
                        "investidor",
                        investidor.InvestidorId
                            .ToString()));
            }

            return claims;
        }

        private static object CriarUsuarioAtual(
            Usuario usuario)
        {
            var permissoes =
                usuario.Perfil ==
                PerfilUsuario.Admin
                    ? Enum
                        .GetValues<
                            PermissaoSistema>()
                        .Select(
                            x =>
                                x.ToString())
                        .ToArray()
                    : usuario.Permissoes
                        .Select(
                            x =>
                                x.Permissao
                                    .ToString())
                        .Distinct()
                        .OrderBy(x => x)
                        .ToArray();

            var investidores =
                usuario.Investidores
                    .Select(
                        x =>
                            x.InvestidorId)
                    .Distinct()
                    .ToArray();

            return new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,

                perfil =
                    usuario.Perfil
                        .ToString(),

                status =
                    usuario.Status
                        .ToString(),

                permissoes,

                investidoresIds =
                    investidores,

                acessosInvestidores =
                    usuario.InvestidoresPermissoes
                        .GroupBy(x => x.InvestidorId)
                        .Select(x => new
                        {
                            investidorId = x.Key,
                            permissoes = x
                                .Select(p => p.Permissao.ToString())
                                .Distinct()
                                .OrderBy(p => p)
                                .ToArray()
                        })
                        .ToArray()
            };
        }
    }

    public record CadastroRequest(
        string Nome,
        string Email,
        string Senha);

    public record LoginRequest(
        string Email,
        string Senha);
}