namespace Investimentos.Domain.Usuarios
{
    public class UsuarioInvestidorPermissao
    {
        public Guid UsuarioId { get; private set; }
        public Guid InvestidorId { get; private set; }
        public PermissaoSistema Permissao { get; private set; }
        public Usuario Usuario { get; private set; }

        private UsuarioInvestidorPermissao()
        {
            Usuario = null!;
        }

        internal UsuarioInvestidorPermissao(
            Guid usuarioId,
            Guid investidorId,
            PermissaoSistema permissao)
        {
            if (usuarioId == Guid.Empty) throw new ArgumentException("O usuário é obrigatório.");
            if (investidorId == Guid.Empty) throw new ArgumentException("O investidor é obrigatório.");
            UsuarioId = usuarioId;
            InvestidorId = investidorId;
            Permissao = permissao;
            Usuario = null!;
        }
    }
}