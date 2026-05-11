using Domain.Usuarios;

namespace Application.Authentication.Interfaces;

public interface ITokenProvider
{
    string GerarToken(Usuario usuario);
}