using SoatTechChallenge.Application.Authentication.Services.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;

namespace SoatTechChallenge.Application.Authentication.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> Login(LoginRequest request);
}