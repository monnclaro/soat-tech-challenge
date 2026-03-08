using SoatTechChallenge.Authorization.Controllers.DTOs;

namespace SoatTechChallenge.Authorization.Services;

public interface IAuthorizationService
{
    Task<string?> Login(LoginRequest request);
}