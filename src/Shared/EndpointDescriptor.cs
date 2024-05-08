namespace Shared;

public record EndpointDescriptor(string Type)
{
    public string Route { get; } = Type.ToBase64();
}
