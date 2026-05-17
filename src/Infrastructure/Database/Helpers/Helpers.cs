using SharedKernel;
using SoatTechChallenge.Infrastucture.DomainEvents;

namespace SoatTechChallenge.Infrastucture.Database.Helpers;

public class NoopDomainEventsDispatcher : IDomainEventsDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
}