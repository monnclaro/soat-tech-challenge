using Application.Login.UseCases;
using Application.Login.UseCases.DTOs;
using Application.Login.UseCases.Interfaces;
using Domain.Usuarios;
using Domain.Usuarios.Gateways;

namespace Tests.Authentication.Unit;

public class LoginUseCaseTests
{
    [Fact]
    public async Task Execute_QuandoUsuarioNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeUsuarioGateway();
        var hasher    = new FakePasswordHasher(valido: false);
        var token     = new FakeTokenProvider();
        var presenter = new FakeLoginPresenter();
        var useCase   = new LoginUseCase(gateway, hasher, token, presenter);

        await useCase.Execute(new LoginInput("naoexiste@email.com", "senha"), CancellationToken.None);

        Assert.True(presenter.UsuarioNaoEncontradoChamado);
        Assert.False(presenter.LoginRealizadoChamado);
    }

    [Fact]
    public async Task Execute_QuandoSenhaInvalida_ChamaSenhaInvalida()
    {
        var usuario = CriarUsuario("email@email.com");
        var gateway   = new FakeUsuarioGateway(usuario);
        var hasher    = new FakePasswordHasher(valido: false);
        var token     = new FakeTokenProvider();
        var presenter = new FakeLoginPresenter();
        var useCase   = new LoginUseCase(gateway, hasher, token, presenter);

        await useCase.Execute(new LoginInput(usuario.Email, "senhaErrada"), CancellationToken.None);

        Assert.True(presenter.SenhaInvalidaChamado);
        Assert.False(presenter.LoginRealizadoChamado);
    }

    [Fact]
    public async Task Execute_QuandoCredenciaisValidas_ChamaLoginRealizado()
    {
        var usuario = CriarUsuario("email@email.com");
        var gateway   = new FakeUsuarioGateway(usuario);
        var hasher    = new FakePasswordHasher(valido: true);
        var token     = new FakeTokenProvider("token-gerado");
        var presenter = new FakeLoginPresenter();
        var useCase   = new LoginUseCase(gateway, hasher, token, presenter);

        await useCase.Execute(new LoginInput(usuario.Email, "senha123"), CancellationToken.None);

        Assert.True(presenter.LoginRealizadoChamado);
        Assert.Equal("token-gerado", presenter.Output?.Token);
    }

    [Fact]
    public async Task Execute_QuandoSenhaInvalida_NaoGeraToken()
    {
        var usuario = CriarUsuario("email@email.com");
        var gateway   = new FakeUsuarioGateway(usuario);
        var hasher    = new FakePasswordHasher(valido: false);
        var token     = new FakeTokenProvider();
        var presenter = new FakeLoginPresenter();
        var useCase   = new LoginUseCase(gateway, hasher, token, presenter);

        await useCase.Execute(new LoginInput(usuario.Email, "senhaErrada"), CancellationToken.None);

        Assert.False(token.GerarTokenFoiChamado);
    }

    private static Usuario CriarUsuario(string email) =>
        new("Usuário Teste", email, "hash-qualquer", "52998224725");
}

file class FakeUsuarioGateway : IUsuarioGateway
{
    private readonly Usuario? _usuario;
    public FakeUsuarioGateway(Usuario? usuario = null) => _usuario = usuario;
    
    public Task<Usuario?> BuscarPorEmail(string email, CancellationToken ct) => Task.FromResult(_usuario?.Email == email ? _usuario : null);
    public Task Salvar(Usuario usuario, CancellationToken ct) => Task.CompletedTask;
}

file class FakePasswordHasher : IPasswordHasher
{
    private readonly bool _valido;
    public FakePasswordHasher(bool valido) => _valido = valido;
    public bool Verificar(string senha, string hash) => _valido;
    public string Hash(string senha) => "hash";
}

file class FakeTokenProvider : ITokenProvider
{
    private readonly string _token;
    public bool GerarTokenFoiChamado { get; private set; }

    public FakeTokenProvider(string token = "token") => _token = token;

    public string GerarToken(Usuario usuario)
    {
        GerarTokenFoiChamado = true;
        return _token;
    }
}

file class FakeLoginPresenter : ILoginOutputPort
{
    public bool UsuarioNaoEncontradoChamado { get; private set; }
    public bool SenhaInvalidaChamado { get; private set; }
    public bool LoginRealizadoChamado { get; private set; }
    public LoginOutput? Output { get; private set; }

    public void UsuarioNaoEncontrado() => UsuarioNaoEncontradoChamado = true;
    public void SenhaInvalida() => SenhaInvalidaChamado = true;
    public void LoginRealizado(LoginOutput output) { LoginRealizadoChamado = true; Output = output; }
}