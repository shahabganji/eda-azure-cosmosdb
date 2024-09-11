using EventSourcingCosmosDB.Domain;
using EventSourcingCosmosDB.Events;
using EventSourcingCosmosDB.Stores;

namespace EventSourcingCosmosDB.Runners;


public interface IApplicationRunner
{
    Task Run();
}

public class InMemoryRunner : IApplicationRunner
{
    public async Task Run()
    {
        IEventStore<Customer> eventStore = new InMemoryStore();

        var customerId = Guid.NewGuid();

        var customerCreated = new CustomerCreated
        {
            CustomerId = customerId,
            Email = "jane.doe@gmail.com",
            FullName = "Jane Doe",
            DateOfBirth = new DateTime(1989, 5, 20),
        };

        await eventStore.Append(customerCreated);

        var studentUpdated = new EmailUpdated
        {
            CustomerId = customerId,
            Email = "shahab@ganji.com",
        };


        await eventStore.Append(studentUpdated);

        var customer = await eventStore.Get(customerId);
        var snapshot = await eventStore.GetSnapshot(customerId);

        Console.WriteLine(customer!.FullName);
        Console.WriteLine(snapshot!.FullName);

    }
}
