namespace CivilConstruction.Domain.Events;

public interface IDomainEvent { }

public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public Guid Id { get; protected set; }
    public long Version { get; private set; }
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents;

    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var domainEvent in history)
        {
            When(domainEvent);
            Version++;
        }

        _uncommittedEvents.Clear();
    }

    public void MarkChangesCommitted()
    {
        Version += _uncommittedEvents.Count;
        _uncommittedEvents.Clear();
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        When(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    protected abstract void When(IDomainEvent domainEvent);
}
