using Application.Authentication.Interfaces;
using SharedKernel;

namespace SoatTechChallenge.Infrastucture.Authentication;

public class BCryptPasswordHasher : IPasswordHasher, IScopedService
{
    public bool Verificar(string senha, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}