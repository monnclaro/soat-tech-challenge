using Application.Authentication.DTOs.Requests;
using Application.Authentication.DTOs.Responses;
using Application.Authentication.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Authentication.Controllers;

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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