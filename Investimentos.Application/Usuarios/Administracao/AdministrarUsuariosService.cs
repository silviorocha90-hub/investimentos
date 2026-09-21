using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;

namespace Investimentos.Application.Usuarios.Administracao
{
    public class AdministrarUsuariosService
    {
        private readonly IUsuarioRepository
            _usuarioRepository;

        public AdministrarUsuariosService(
            IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository =
                usuarioRepository;
        }

        public async Task<
            IReadOnlyList<UsuarioAdministracaoDto>>
            ListarAsync(
                CancellationToken cancellationToken = default)
        {
            var usuarios =
                await _usuarioRepository.ListarAsync(
                    cancellationToken);

            return usuarios
                .Select(Mapear)
                .ToList();
        }

        public async Task AprovarAsync(
            Guid usuarioId,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {
            var usuario =
                await ObterUsuarioAsync(
                    usuarioId,
                    cancellationToken);

            usuario.Aprovar(
                administradorId);

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);
        }

        public async Task RejeitarAsync(
            Guid usuarioId,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {
            ValidarOutroUsuario(
                usuarioId,
                administradorId,
                "rejeitar");

            var usuario =
                await ObterUsuarioAsync(
                    usuarioId,
                    cancellationToken);

            await ValidarPreservacaoAdminAsync(
                usuario,
                cancellationToken);

            usuario.Rejeitar();

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);
        }

        public async Task BloquearAsync(
            Guid usuarioId,
            Guid administradorId,
            CancellationToken cancellationToken = default)
        {
            ValidarOutroUsuario(
                usuarioId,
                administradorId,
                "bloquear");

            var usuario =
                await ObterUsuarioAsync(
                    usuarioId,
                    cancellationToken);

            await ValidarPreservacaoAdminAsync(
                usuario,
                cancellationToken);

            usuario.Bloquear();

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);
        }

        public async Task DesbloquearAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            var usuario =
                await ObterUsuarioAsync(
                    usuarioId,
                    cancellationToken);

            usuario.Desbloquear();

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);
        }

        public async Task AlterarAcessosAsync(
            Guid usuarioId,
            Guid administradorId,
            PerfilUsuario perfil,
            IReadOnlyCollection<PermissaoSistema>
                permissoes,
            IReadOnlyCollection<Guid>
                investidoresIds,
            CancellationToken cancellationToken = default)
        {
            var usuario =
                await ObterUsuarioAsync(
                    usuarioId,
                    cancellationToken);

            if (usuarioId == administradorId &&
                perfil != PerfilUsuario.Admin)
            {
                throw new InvalidOperationException(
                    "O administrador não pode remover o próprio perfil de administrador.");
            }

            if (usuario.Perfil ==
                    PerfilUsuario.Admin &&
                perfil != PerfilUsuario.Admin)
            {
                await ValidarPreservacaoAdminAsync(
                    usuario,
                    cancellationToken);
            }

            usuario.AlterarPerfil(
                perfil);

            if (perfil ==
                PerfilUsuario.Admin)
            {
                usuario.DefinirPermissoes(
                    Enum.GetValues<
                        PermissaoSistema>());

                usuario.DefinirInvestidores(
                    Array.Empty<Guid>());
            }
            else
            {
                var permissoesVisualizacao =
                    new[]
                    {
                        PermissaoSistema.Dashboard,
                        PermissaoSistema.Carteira,
                        PermissaoSistema.Opcoes,
                        PermissaoSistema.Proventos
                    };

                usuario.DefinirPermissoes(
                    permissoesVisualizacao);

                usuario.DefinirInvestidores(
                    investidoresIds);
            }

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);
        }

        private async Task<Usuario>
            ObterUsuarioAsync(
                Guid usuarioId,
                CancellationToken cancellationToken)
        {
            var usuario =
                await _usuarioRepository
                    .ObterPorIdAsync(
                        usuarioId,
                        cancellationToken);

            if (usuario is null)
            {
                throw new KeyNotFoundException(
                    "Usuário não encontrado.");
            }

            return usuario;
        }

        private async Task
            ValidarPreservacaoAdminAsync(
                Usuario usuario,
                CancellationToken cancellationToken)
        {
            if (usuario.Perfil !=
                    PerfilUsuario.Admin ||
                usuario.Status !=
                    StatusUsuario.Ativo)
            {
                return;
            }

            var quantidadeAdmins =
                await _usuarioRepository
                    .ContarAdminsAtivosAsync(
                        cancellationToken);

            if (quantidadeAdmins <= 1)
            {
                throw new InvalidOperationException(
                    "A operação não pode ser realizada porque o sistema precisa manter pelo menos um administrador ativo.");
            }
        }

        private static void ValidarOutroUsuario(
            Guid usuarioId,
            Guid administradorId,
            string operacao)
        {
            if (usuarioId ==
                administradorId)
            {
                throw new InvalidOperationException(
                    $"O administrador não pode {operacao} o próprio usuário.");
            }
        }

        private static UsuarioAdministracaoDto
            Mapear(
                Usuario usuario)
        {
            return new UsuarioAdministracaoDto(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil.ToString(),
                usuario.Status.ToString(),

                /*
                 * Datas persistidas no banco estão
                 * em UTC.
                 *
                 * SQL Server datetime2 não preserva
                 * DateTimeKind. Ao materializar a
                 * entidade, o EF devolve
                 * DateTimeKind.Unspecified.
                 *
                 * Marcamos explicitamente como UTC
                 * antes de serializar para que o
                 * frontend possa converter
                 * corretamente para o horário local.
                 */
                ComoUtc(
                    usuario.DataCadastro),

                ComoUtc(
                    usuario.DataAprovacao),

                usuario.AprovadoPorUsuarioId,

                ComoUtc(
                    usuario.UltimoLogin),

                usuario.Permissoes
                    .Select(
                        x =>
                            x.Permissao
                                .ToString())
                    .OrderBy(x => x)
                    .ToArray(),

                usuario.Investidores
                    .Select(
                        x =>
                            x.InvestidorId)
                    .Distinct()
                    .ToArray());
        }

        private static DateTime ComoUtc(
            DateTime data)
        {
            return data.Kind switch
            {
                DateTimeKind.Utc =>
                    data,

                DateTimeKind.Local =>
                    data.ToUniversalTime(),

                _ =>
                    DateTime.SpecifyKind(
                        data,
                        DateTimeKind.Utc)
            };
        }

        private static DateTime? ComoUtc(
            DateTime? data)
        {
            if (!data.HasValue)
            {
                return null;
            }

            return ComoUtc(
                data.Value);
        }
    }

    public record UsuarioAdministracaoDto(
        Guid Id,
        string Nome,
        string Email,
        string Perfil,
        string Status,
        DateTime DataCadastro,
        DateTime? DataAprovacao,
        Guid? AprovadoPorUsuarioId,
        DateTime? UltimoLogin,
        IReadOnlyCollection<string> Permissoes,
        IReadOnlyCollection<Guid> InvestidoresIds);
}