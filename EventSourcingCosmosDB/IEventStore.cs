using EventSourcingCosmosDB.Events;

namespace EventSourcingCosmosDB;

public interface IEventStore<T>
{
    Task Append<TE>(TE @event) where TE : Event;
    Task<T?> Get(Guid streamId);
    Task<T?> GetSnapshot(Guid streamId);
}
