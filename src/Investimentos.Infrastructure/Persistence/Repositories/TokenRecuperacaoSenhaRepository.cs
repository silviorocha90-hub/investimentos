using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class TokenRecuperacaoSenhaRepository
        : ITokenRecuperacaoSenhaRepository
    {
        private readonly InvestimentosDbContext _context;

        public TokenRecuperacaoSenhaRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<TokenRecuperacaoSenha?>
            ObterPorHashAsync(
                string tokenHash,
                CancellationToken cancellationToken = default)
        {
            return await _context
                .TokensRecuperacaoSenha
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash,
                    cancellationToken);
        }

        public async Task AdicionarAsync(
            TokenRecuperacaoSenha token,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);

            await _context
                .TokensRecuperacaoSenha
                .AddAsync(
                    token,
                    cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task InvalidarTokensAtivosAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            var agora = DateTime.UtcNow;

            var tokens =
                await _context
                    .TokensRecuperacaoSenha
                    .Where(
                        x =>
                            x.UsuarioId == usuarioId &&
                            x.DataUtilizacao == null &&
                            x.DataExpiracao > agora)
                    .ToListAsync(
                        cancellationToken);

            foreach (var token in tokens)
            {
                token.MarcarComoUtilizado();
            }

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}