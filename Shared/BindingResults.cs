using System.Text.Json.Serialization;

namespace Shared;

public record BindingResults(
    [property: JsonPropertyName("t")]
    string Type,

    [property: JsonPropertyName("r")]
    Dictionary<ApiType, BindingResult> Results)
{
    [JsonIgnore]
    public bool IsDiscrepant { get => Results.DistinctBy(r => r.Value.ToString()).Count() > 1; }

    [JsonIgnore]
    public bool AllErroneous { get => Results.All(r => r.Value.IsErroneous); }

    public bool AreEquivalentTo(BindingResults other)
    {
        if (Results.Count != other.Results.Count) return false;
        foreach (var entry in Results)
        {
            if (!other.Results.TryGetValue(entry.Key, out var otherValue) || otherValue != entry.Value)
                return false;
        }
        return true;
    }
};
