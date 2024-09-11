using EventSourcingCosmosDB.Domain;
using EventSourcingCosmosDB.Events;

namespace EventSourcingCosmosDB.Stores;

public sealed class InMemoryStore : IEventStore<Customer>
{
    private readonly Dictionary<Guid, SortedList<DateTime, Event>> _events = new();
    private readonly Dictionary<Guid, Customer> _customers = new();

    public async Task Append<TE>(TE @event) where TE : Event
    {
        var stream = _events!.GetValueOrDefault(@event.StreamId, null);

        if (stream is null)
        {
            _events[@event.StreamId] = new SortedList<DateTime, Event>();
        }

        // *********  These two should be atomic
        _events[@event.StreamId].Add(@event.CreatedAtUtc, @event);
        // Synchronous Projection - depends on whether we are in a read/write heavy scenario and what type of consistency is acceptable
        _customers[@event.StreamId] = (await Get(@event.StreamId))!;
        // **********************************************
    }

    public Task<Customer?> Get(Guid streamId)
    {
        var studentStream = _events!.GetValueOrDefault(streamId, null);
        if (studentStream is null)
            return Task.FromResult<Customer?>(null);

        Customer student = new();
        foreach (var keyValuePair in studentStream)
        {
            student.Apply(keyValuePair.Value);
        }

        return Task.FromResult(student)!;
    }

    public Task<Customer?> GetSnapshot(Guid streamId)
    {
        return Task.FromResult(_customers!.GetValueOrDefault(streamId, null));
    }
}
