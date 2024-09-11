using EventSourcingCosmosDB.Domain;
using EventSourcingCosmosDB.Events;
using EventSourcingCosmosDB.Serializers;
using Microsoft.Azure.Cosmos;

namespace EventSourcingCosmosDB.Stores;

public sealed class CosmosEventStore : IEventStore<Customer>
{
    
    #region private members: 
    
    private readonly Container _container;

    private const string ConnectionString = "<your-cosmos-db-connecting-string>";

    #endregion

    public static async Task<CosmosEventStore> InitCosmos()
    {
        var cosmosClient = new CosmosClient(ConnectionString, new CosmosClientOptions
        {
            ApplicationName = "EventSourcingCosmosDB - Demo",
            EnableContentResponseOnWrite = false,

            Serializer = new CosmosSystemTextJsonSerializer(),

            ApplicationPreferredRegions = ["West Europe"],
        });
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("Demo");
        var container = await database.Database.CreateContainerIfNotExistsAsync("Customers", "/StreamId");

        return new CosmosEventStore(container.Container);
    }

    private CosmosEventStore(Container container)
    {
        _container = container;
    }

    /// <summary>
    /// Appends an event to the end of the stream
    /// </summary>
    /// <param name="event">The event to be stored in the stream</param>
    /// <typeparam name="TE">Generic type, because of serialization and inheritance</typeparam>
    public async Task Append<TE>(TE @event) where TE : Event
    {
        Customer customer = await Get(@event.StreamId) ?? new();
        customer.Apply(@event);

        var transactionalBatch = _container.CreateTransactionalBatch(new PartitionKey(@event.StreamId.ToString()));

        // transactionalBatch.UpsertItem(customer);
        transactionalBatch.UpsertItem<Event>(@event);

        var transactionResult = await transactionalBatch.ExecuteAsync(CancellationToken.None);

        Console.WriteLine($"Number of items written: {transactionResult.Count}");
        Console.WriteLine($"transaction Status Code: {transactionResult.StatusCode}");

        // await _container.UpsertItemAsync<Event>(@event, new PartitionKey(@event.StreamId.ToString()));
    }

    /// <summary>
    /// Gets the snapshot by applying the events in the stream
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream</param>
    /// <returns></returns>
    public async Task<Customer?> Get(Guid streamId)
    {
        var streamIterator =
            _container.GetItemQueryIterator<Event>(
                new QueryDefinition($"SELECT * FROM c WHERE c.StreamId = '{streamId}' AND c.id <> '{streamId}'"));

        if (!streamIterator.HasMoreResults)
            return null;

        Customer customer = new();

        while (streamIterator.HasMoreResults)
        {
            var readNext = await streamIterator.ReadNextAsync();

            foreach (var @event in readNext.Resource)
            {
                customer.Apply(@event);
            }
        }

        return customer;
    }

    /// <summary>
    /// Gets the snapshot using Point Read mechanism from ACD SDK
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream</param>
    /// <returns></returns>
    public async Task<Customer?> GetSnapshot(Guid streamId)
    {
        try
        {
            var snapshot = await _container.ReadItemAsync<Customer>(
                id: streamId.ToString(), new PartitionKey(streamId.ToString()), new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false,
                });

            return snapshot.Resource;
        }
        catch
        {
            return null;
        }
    }

    public FeedIterator<Customer> GetChangeFeedIteratorFor(Guid streamId)
    {
        FeedIterator<Customer> iteratorForPartitionKey = _container.GetChangeFeedIterator<Customer>(
            ChangeFeedStartFrom.Beginning(FeedRange.FromPartitionKey(new PartitionKey(streamId.ToString()))),
            ChangeFeedMode.LatestVersion);

        return iteratorForPartitionKey;
    }
}
