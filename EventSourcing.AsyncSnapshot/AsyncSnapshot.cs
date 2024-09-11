using System.Text.Json;
using System.Text.Json.Nodes;
using EventSourcingCosmosDB.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EventSourcingCosmosDB.Events;
using Microsoft.Azure.Cosmos;

namespace EventSourcing.AsyncSnapshot;

public class AsyncSnapshot(ILogger<AsyncSnapshot> logger, Container container)
{
    [Function("AsyncSnapshot")]
    public async Task Run([CosmosDBTrigger(
            databaseName: "Demo",
            containerName: "Customers",
            Connection = "DatabaseConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true,
            StartFromBeginning = true, MaxItemsPerInvocation = 100)]
        IReadOnlyList<JsonObject> input)
    {
        foreach (var eventJson in input)
        {
            if (eventJson["Pk"]!.ToString() == eventJson["Id"]!.ToString())
                continue;

            var @event = eventJson.Deserialize<Event>();
            var customer = await GetSnapshot(@event!.StreamId);

            customer.Apply(@event);

            await container.UpsertItemAsync(customer, new PartitionKey(customer.StreamId));

            logger.LogInformation("Data is {@Data}", customer);
        }
    }

    private async Task<Customer> GetSnapshot(Guid streamId)
    {
        try
        {
            var snapshot = await container.ReadItemAsync<Customer>(
                id: streamId.ToString(), new PartitionKey(streamId.ToString()), new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false,
                });

            return snapshot.Resource;
        }
        catch
        {
            return new();
        }
    }
}
