using System.Text.Json.Serialization;
using EventSourcingCosmosDB.Events;

namespace EventSourcingCosmosDB.Domain;

public sealed class Customer
{
    public Guid CustomerId { get; set; }

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public DateTime DateOfBirth { get; set; }
    
    public void Apply(Event @event)
    {
        ((dynamic)this).Apply((dynamic)@event);
    }

    private void Apply(CustomerCreated @event)
    {
        CustomerId = @event.CustomerId;
        FullName = @event.FullName;
        Email = @event.Email;
        DateOfBirth = @event.DateOfBirth;
    }

    private void Apply(EmailUpdated @event)
    {
        Email = @event.Email;
    }

    public string StreamId => CustomerId.ToString();
    [JsonPropertyName("pk")] public string Pk => CustomerId.ToString();
    [JsonPropertyName("id")] public string Id => CustomerId.ToString();
}
