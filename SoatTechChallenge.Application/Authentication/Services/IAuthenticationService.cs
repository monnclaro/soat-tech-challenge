using SharedKernel;
using SoatTechChallenge.Application.Authentication.DTOs.Requests;
using SoatTechChallenge.Application.Authentication.DTOs.Responses;

namespace SoatTechChallenge.Application.Authentication.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> Login(LoginRequest request);
}