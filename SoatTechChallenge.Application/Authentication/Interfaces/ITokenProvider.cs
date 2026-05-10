using SoatTechChallenge.Domain.Usuarios;

namespace SoatTechChallenge.Application.Authentication.Interfaces;

public interface ITokenProvider
{
    string GerarToken(Usuario usuario);
}