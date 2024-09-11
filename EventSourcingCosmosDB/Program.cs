
using EventSourcingCosmosDB.Runners;

// IApplicationRunner runner = new InMemoryRunner();

IApplicationRunner runner = new CosmosDbRunner();


await runner.Run();
