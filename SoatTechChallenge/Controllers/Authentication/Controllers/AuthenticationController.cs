using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Application.Authentication.Services;
using LoginRequest = SoatTechChallenge.Application.Authentication.Services.DTOs.LoginRequest;

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
        var token = await _authenticationService.Login(request);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new { token });
    }
}