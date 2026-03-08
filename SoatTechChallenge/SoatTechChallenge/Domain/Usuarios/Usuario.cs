using SoatTechChallenge.Domain.Usuarios.Roles;

namespace SoatTechChallenge.Domain.Usuarios;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string SenhaHash { get; private set; }
    public List<UsuarioRole> Roles { get; private set; }

    public Usuario() { }

    public Usuario(string nome, string senhaHash, List<UsuarioRole> roles)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        SenhaHash = senhaHash;
        Roles = roles;
    }
}