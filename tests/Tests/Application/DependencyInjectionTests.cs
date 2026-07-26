using Application;
using Application.Common.Interfaces;
using Domain.OrdensServico.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistraImplementacoesDeIDomainEventHandler()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        // Regressão: IDomainEventHandler<T> não era coberto por nenhum dos
        // Scan() existentes (só IUseCase/IScoped), então handlers como o que
        // decrementa estoque ao finalizar uma OS nunca eram resolvidos pelo
        // DomainEventsDispatcher e silenciosamente não executavam.
        services
            .Should()
            .Contain(d => d.ServiceType == typeof(IDomainEventHandler<OrdemServicoFinalizadaDomainEvent>),
                because: "handlers de eventos de domínio precisam ser resolvíveis pelo DomainEventsDispatcher via DI");

        services
            .Should()
            .Contain(d => d.ServiceType == typeof(IDomainEventHandler<OrdemServicoStatusAlteradoDomainEvent>),
                because: "o handler que loga a duração por status (dashboard de observabilidade) também precisa ser resolvível");
    }
}
