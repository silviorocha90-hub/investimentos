namespace Investimentos.Domain.Usuarios
{
    public class TokenRecuperacaoSenha
    {
        public Guid Id { get; private set; }

        public Guid UsuarioId { get; private set; }

        public string TokenHash { get; private set; }

        public DateTime DataCriacao { get; private set; }

        public DateTime DataExpiracao { get; private set; }

        public DateTime? DataUtilizacao { get; private set; }

        public Usuario Usuario { get; private set; }

        private TokenRecuperacaoSenha()
        {
            TokenHash = null!;
            Usuario = null!;
        }

        public TokenRecuperacaoSenha(
            Guid usuarioId,
            string tokenHash,
            DateTime dataExpiracao)
        {
            if (usuarioId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O usuário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException(
                    "O hash do token é obrigatório.");
            }

            if (dataExpiracao <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "A data de expiração deve ser futura.");
            }

            Id = Guid.NewGuid();

            UsuarioId = usuarioId;

            TokenHash = tokenHash.Trim();

            DataCriacao = DateTime.UtcNow;

            DataExpiracao = dataExpiracao;

            Usuario = null!;
        }

        public bool EstaValido(DateTime dataAtual)
        {
            return
                DataUtilizacao is null &&
                DataExpiracao > dataAtual;
        }

        public void MarcarComoUtilizado()
        {
            if (DataUtilizacao is not null)
            {
                return;
            }

            DataUtilizacao = DateTime.UtcNow;
        }
    }
}