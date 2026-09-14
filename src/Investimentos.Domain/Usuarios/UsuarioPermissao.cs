namespace Investimentos.Domain.Usuarios
{
    public class UsuarioPermissao
    {
        public Guid UsuarioId { get; private set; }

        public PermissaoSistema Permissao
        {
            get;
            private set;
        }

        public Usuario Usuario { get; private set; }

        private UsuarioPermissao()
        {
            Usuario = null!;
        }

        internal UsuarioPermissao(
            Guid usuarioId,
            PermissaoSistema permissao)
        {
            if (usuarioId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O usuário é obrigatório.");
            }

            UsuarioId = usuarioId;

            Permissao = permissao;

            Usuario = null!;
        }
    }
}