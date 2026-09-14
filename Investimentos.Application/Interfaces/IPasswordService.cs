namespace Investimentos.Application.Interfaces
{
    public interface IPasswordService
    {
        string GerarHash(string senha);

        bool Verificar(
            string senhaHash,
            string senha);
    }
}