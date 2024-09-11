namespace EventSourcingCosmosDB.Events;

public class EmailUpdated : Event
{
    public required Guid CustomerId { get; set; }
    public required string Email { get; set; }

    public override Guid StreamId => CustomerId;

}
