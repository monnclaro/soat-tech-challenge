namespace SoatTechChallenge.Application.Authentication.Services.DTOs;

public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public int ExpirationHours { get; set; } = 2;
}