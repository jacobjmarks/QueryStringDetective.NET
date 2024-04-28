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
};
