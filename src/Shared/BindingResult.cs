using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Shared;

public record BindingResult(
    [property: JsonPropertyName("r"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Result = null,

    [property: JsonPropertyName("e"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BindingResult.ProblemDetails? Error = null)
{
    public record ProblemDetails(
        [property: JsonPropertyName("m")]
        string Message,

        [property: JsonPropertyName("d"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Detail)
    {
        public string ToDisplayString(bool detailed = false)
        {
            var displayString = Message;
            if (detailed && !string.IsNullOrEmpty(Detail))
                displayString += ": " + Detail;
            return displayString;
        }
    };

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Result))]
    public bool IsErroneous { get => Error != null; }
}
