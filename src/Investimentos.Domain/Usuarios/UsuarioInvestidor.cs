using Investimentos.Domain.Entities;

namespace Investimentos.Domain.Usuarios
{
    public class UsuarioInvestidor
    {
        public Guid UsuarioId { get; private set; }

        public Guid InvestidorId { get; private set; }

        public Usuario Usuario { get; private set; }

        public Investidor Investidor { get; private set; }

        private UsuarioInvestidor()
        {
            Usuario = null!;
            Investidor = null!;
        }

        internal UsuarioInvestidor(
            Guid usuarioId,
            Guid investidorId)
        {
            if (usuarioId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O usuário é obrigatório.");
            }

            if (investidorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O investidor é obrigatório.");
            }

            UsuarioId = usuarioId;

            InvestidorId = investidorId;

            Usuario = null!;

            Investidor = null!;
        }
    }
}