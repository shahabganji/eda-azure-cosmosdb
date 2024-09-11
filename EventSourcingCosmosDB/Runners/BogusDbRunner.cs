using Bogus;
using EventSourcingCosmosDB.Domain;
using EventSourcingCosmosDB.Events;
using EventSourcingCosmosDB.Stores;

namespace EventSourcingCosmosDB.Runners;

public class BogusDbRunner : IApplicationRunner
{
    // Step 1: Create a Faker for generating the EmailUpdated events
    private static readonly Faker<EmailUpdated> EmailUpdatedFaker = new Faker<EmailUpdated>()
        .RuleFor(e => e.CustomerId, f => Guid.NewGuid())
        .RuleFor(e => e.Email, f => f.Internet.Email());

    // Step 2: Create a Faker for generating the CustomerCreated events
    private static readonly Faker<CustomerCreated> CustomerCreatedFaker = new Faker<CustomerCreated>()
        .RuleFor(c => c.CustomerId, f => Guid.NewGuid())
        .RuleFor(c => c.FullName, f => f.Name.FullName())
        .RuleFor(c => c.DateOfBirth, f => f.Date.Past())
        .RuleFor(c => c.Email, f => f.Internet.Email());

    public async Task Run()
    {
        IEventStore<Customer> eventStore = await CosmosEventStore.InitCosmos();

        for (var i = 0; i < 1000; i++)
        {
            var customerId = Guid.NewGuid(); // Common CustomerId for both events

            var customerCreated = CustomerCreatedFaker.RuleFor(x => x.StreamId, _ => customerId).Generate();
            await eventStore.Append(customerCreated);
    
            var emailUpdates = EmailUpdatedFaker.RuleFor(x => x.StreamId, _ => customerId).Generate();
            await eventStore.Append(emailUpdates);

        }

        Console.WriteLine("Done!");

    }
}
