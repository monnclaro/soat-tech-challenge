using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Usuarios.Roles;

namespace SoatTechChallenge.Domain.Usuarios;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<UsuarioRole> Roles { get; private init; } = new();

    public Usuario() { }

    public Usuario(string nome, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome é obrigatório");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O email é obrigatório");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("A senha é obrigatória");

        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        DataCriacao = DateTime.UtcNow;
    }

    public void AdicionarRoles(List<UsuarioRole> roles)
    {
        var rolesNovas = roles
            .Where(r => Roles.All(l => l.Id != r.Id))
            .ToList();

        Roles.AddRange(rolesNovas);
    }
}