using Application.Authentication.DTOs.Requests;
using Application.Authentication.DTOs.Responses;

namespace Application.Authentication.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> Login(LoginRequest request);
}