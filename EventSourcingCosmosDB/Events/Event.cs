using System.Text.Json.Serialization;

namespace EventSourcingCosmosDB.Events;

public interface IAmStreamEvent
{
    // marker interface
}

public partial class Event : IAmStreamEvent
{
    public abstract Guid StreamId { get; }
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

    [JsonPropertyName("pk")] public string Pk => StreamId.ToString();
    [JsonPropertyName("id")] public string Id => CreatedAtUtc.ToString("O");
}
