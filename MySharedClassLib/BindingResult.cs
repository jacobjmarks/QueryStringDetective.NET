using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MySharedClassLib;

public record BindingResult(
    [property: JsonPropertyName("t")]
    string Type,

    [property: JsonPropertyName("r"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Result = null,

    [property: JsonPropertyName("e"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BindingResult.ProblemDetails? Error = null)
{
    public record ProblemDetails(
        [property: JsonPropertyName("m")]
        string Message,

        [property: JsonPropertyName("d"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Detail);

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Result))]
    public bool IsErroneous { get => Error != null; }
}
