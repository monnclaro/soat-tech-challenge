namespace SoatTechChallenge.Domain.Usuarios.Roles;

public class UsuarioRole
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Role { get; private set; }

    public UsuarioRole() { }
    
    public UsuarioRole(string role)
    {
        Id = Guid.NewGuid();
        Role = role;
    }
}