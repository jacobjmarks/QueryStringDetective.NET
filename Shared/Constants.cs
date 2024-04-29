namespace Shared;

public static class Constants
{
    public static EndpointDescriptor[] Endpoints { get; } =
        new string[]
        {
            "string",
            "char",
            "int",
            "double",
            "float",
            "decimal",
            "bool",
            "uint",
            "long",
            "ulong",
            "sbyte",
            "short",
            "ushort",
            "byte",
        }
        .SelectMany(t => new EndpointDescriptor[]
        {
            new(t),
            new(t + "?"),
            new(t + "[]"),
        })
        .ToArray();
}
