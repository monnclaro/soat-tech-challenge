namespace Application.Authentication.Interfaces;

public interface IPasswordHasher
{
    bool Verificar(string senha, string hash);
}