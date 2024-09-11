namespace EventSourcingCosmosDB.Events;

public class CustomerCreated : Event
{
    public required Guid CustomerId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required DateTime DateOfBirth { get; set; }

    public override Guid StreamId => CustomerId;

}
