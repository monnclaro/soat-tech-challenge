using Domain.Common.Events;
using SharedKernel;

namespace Application.Common.Interfaces;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken);
}
