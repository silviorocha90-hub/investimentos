using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository
        : IUsuarioRepository
    {
        private readonly InvestimentosDbContext _context;

        public UsuarioRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .Include(x => x.Permissoes)
                .Include(x => x.Investidores)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<Usuario?> ObterPorEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var emailNormalizado =
                NormalizarEmail(email);

            return await _context.Usuarios
                .Include(x => x.Permissoes)
                .Include(x => x.Investidores)
                .FirstOrDefaultAsync(
                    x => x.Email == emailNormalizado,
                    cancellationToken);
        }

        public async Task<bool> ExistePorEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var emailNormalizado =
                NormalizarEmail(email);

            return await _context.Usuarios
                .AnyAsync(
                    x => x.Email == emailNormalizado,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Usuario>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(x => x.Permissoes)
                .Include(x => x.Investidores)
                .OrderBy(x => x.Nome)
                .ToListAsync(cancellationToken);
        }

        public async Task AdicionarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            await _context.Usuarios.AddAsync(
                usuario,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        public async Task SalvarAlteracoesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }

        private static string NormalizarEmail(
            string email)
        {
            return email
                .Trim()
                .ToLowerInvariant();
        }
    }
}