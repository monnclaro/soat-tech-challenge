using SoatTechChallenge.Application.Authentication.Services.DTOs;

namespace SoatTechChallenge.Application.Authentication.Services;

public interface IAuthenticationService
{
    Task<string?> Login(LoginRequest request);
}