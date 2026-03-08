using Microsoft.AspNetCore.Mvc;
using SoatTechChallenge.Authorization.Services;
using LoginRequest = SoatTechChallenge.Authorization.Controllers.DTOs.LoginRequest;

namespace SoatTechChallenge.Authorization.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    
    public AuthorizationController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await _authorizationService.Login(request);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new { token });
    }
}