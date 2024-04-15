using System.Text.Json.Serialization;

namespace MySharedClassLib;

public record BindingResult(
    [property: JsonPropertyName("t")]
    string Type,

    [property: JsonPropertyName("r"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    object? Result = null,

    [property: JsonPropertyName("e"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Error = null);
