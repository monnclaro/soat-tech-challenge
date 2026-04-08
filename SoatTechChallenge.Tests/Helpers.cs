using SharedKernel;
using SoatTechChallenge.Infrastucture.DomainEvents;

namespace SoatTechChallenge.Tests;

public class NoopDomainEventsDispatcher : IDomainEventsDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
}