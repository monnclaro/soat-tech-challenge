namespace SharedKernel;

public interface IDomainEvent
{
    DateTime OcurredAt { get; }
};
