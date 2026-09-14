using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;

namespace Investimentos.Application.Usuarios.Autenticacao
{
    public class AutenticacaoService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;

        public AutenticacaoService(
            IUsuarioRepository usuarioRepository,
            IPasswordService passwordService)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
        }

        public async Task<Guid> CadastrarAsync(
            string nome,
            string email,
            string senha,
            CancellationToken cancellationToken = default)
        {
            if (await _usuarioRepository.ExistePorEmailAsync(
                email,
                cancellationToken))
            {
                throw new ArgumentException(
                    "Já existe um usuário cadastrado com este e-mail.");
            }

            var senhaHash =
                _passwordService.GerarHash(senha);

            var usuario =
                new Usuario(
                    nome,
                    email,
                    senhaHash);

            await _usuarioRepository.AdicionarAsync(
                usuario,
                cancellationToken);

            return usuario.Id;
        }

        public async Task<ResultadoLogin> LoginAsync(
            string email,
            string senha,
            CancellationToken cancellationToken = default)
        {
            var usuario =
                await _usuarioRepository.ObterPorEmailAsync(
                    email,
                    cancellationToken);

            if (usuario is null ||
                !_passwordService.Verificar(
                    usuario.SenhaHash,
                    senha))
            {
                return ResultadoLogin.Falha(
                    "E-mail ou senha inválidos.");
            }

            if (usuario.Status ==
                StatusUsuario.Pendente)
            {
                return ResultadoLogin.Falha(
                    "Seu cadastro está aguardando aprovação do administrador.");
            }

            if (usuario.Status ==
                StatusUsuario.Rejeitado)
            {
                return ResultadoLogin.Falha(
                    "Seu cadastro não está autorizado.");
            }

            if (usuario.Status ==
                StatusUsuario.Bloqueado)
            {
                return ResultadoLogin.Falha(
                    "Seu usuário está bloqueado.");
            }

            if (usuario.Status !=
                StatusUsuario.Ativo)
            {
                return ResultadoLogin.Falha(
                    "Seu usuário não está autorizado.");
            }

            usuario.RegistrarLogin();

            await _usuarioRepository
                .SalvarAlteracoesAsync(
                    cancellationToken);

            return ResultadoLogin.Sucesso(
                usuario);
        }
    }

    public record ResultadoLogin(
        bool Autenticado,
        string? Mensagem,
        Usuario? Usuario)
    {
        public static ResultadoLogin Sucesso(
            Usuario usuario)
        {
            return new ResultadoLogin(
                true,
                null,
                usuario);
        }

        public static ResultadoLogin Falha(
            string mensagem)
        {
            return new ResultadoLogin(
                false,
                mensagem,
                null);
        }
    }
}