namespace SoatTechChallenge.Application.Authentication.Services.DTOs.Requests;

public class LoginRequest
{
    public string Email { get; set; }
    public string Senha { get; set; }

    public LoginRequest(string email, string senha)
    {
        Email = email;
        Senha = senha;
    }
}