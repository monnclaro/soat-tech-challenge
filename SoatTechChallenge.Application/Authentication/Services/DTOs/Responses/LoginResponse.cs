namespace SoatTechChallenge.Application.Authentication.Services.DTOs.Responses;

public class LoginResponse
{
    public string? Token { get; set; }
    public LoginResponseStatusResultado Status { get; set; }

    public LoginResponse(string token)
    {
        Token = token;
        Status =  LoginResponseStatusResultado.Sucesso;
    }
    
    public LoginResponse(LoginResponseStatusResultado status)
    {
        Status =  status;
    }
}

public enum LoginResponseStatusResultado
{
    Sucesso = 0,
    UsuarioNaoEncontrado = 1,
    SenhaInvalida = 2,
}