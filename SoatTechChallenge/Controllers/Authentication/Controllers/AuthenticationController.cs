using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Authentication.Services;
using SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;
using LoginRequest = SoatTechChallenge.Application.Authentication.Services.DTOs.Requests.LoginRequest;

namespace SoatTechChallenge.Controllers.Authentication.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    
    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var resultado = await _authenticationService.Login(request);

        return resultado.Status switch
        {
            LoginResponseStatusResultado.UsuarioNaoEncontrado => NotFound(),
            LoginResponseStatusResultado.SenhaInvalida => Unauthorized(),
            _ => Ok(new { resultado.Token })
        };
    }
}