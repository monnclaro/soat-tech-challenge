using Application.Login.UseCases.Interfaces;
using SharedKernel;

namespace SoatTechChallenge.Infrastucture.Security.BCrypt;

public class BCryptPasswordHasher : IPasswordHasher, IScoped
{
    public bool Verificar(string senha, string hash)
    {
        return global::BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}