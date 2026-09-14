using Investimentos.Application.Interfaces;
using Investimentos.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace Investimentos.Infrastructure.Identity
{
    public class PasswordService
        : IPasswordService
    {
        private readonly PasswordHasher<Usuario>
            _passwordHasher = new();

        public string GerarHash(string senha)
        {
            ValidarSenha(senha);

            return _passwordHasher.HashPassword(
                null!,
                senha);
        }

        public bool Verificar(
            string senhaHash,
            string senha)
        {
            if (string.IsNullOrWhiteSpace(senhaHash))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(senha))
            {
                return false;
            }

            var resultado =
                _passwordHasher.VerifyHashedPassword(
                    null!,
                    senhaHash,
                    senha);

            return
                resultado ==
                    PasswordVerificationResult.Success ||
                resultado ==
                    PasswordVerificationResult
                        .SuccessRehashNeeded;
        }

        private static void ValidarSenha(
            string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
            {
                throw new ArgumentException(
                    "A senha é obrigatória.");
            }

            if (senha.Length < 8)
            {
                throw new ArgumentException(
                    "A senha deve possuir pelo menos 8 caracteres.");
            }

            if (!senha.Any(char.IsUpper))
            {
                throw new ArgumentException(
                    "A senha deve possuir pelo menos uma letra maiúscula.");
            }

            if (!senha.Any(char.IsLower))
            {
                throw new ArgumentException(
                    "A senha deve possuir pelo menos uma letra minúscula.");
            }

            if (!senha.Any(char.IsDigit))
            {
                throw new ArgumentException(
                    "A senha deve possuir pelo menos um número.");
            }
        }
    }
}