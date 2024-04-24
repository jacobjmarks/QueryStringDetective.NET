namespace Shared;

public static class Constants
{
    public static readonly EndpointDescriptor[] Endpoints =
    [
        new("string", typeof(string)),
        new("string[]", typeof(string[])),
        new("char", typeof(char)),
        new("char[]", typeof(char[])),
        new("int", typeof(int)),
        new("int[]", typeof(int[])),
        new("double", typeof(double)),
        new("double[]", typeof(double[])),
        new("float", typeof(float)),
        new("float[]", typeof(float[])),
        new("decimal", typeof(decimal)),
        new("decimal[]", typeof(decimal[])),
        new("bool", typeof(bool)),
        new("bool[]", typeof(bool[])),
        new("uint", typeof(uint)),
        new("uint[]", typeof(uint[])),
        new("long", typeof(long)),
        new("long[]", typeof(long[])),
        new("ulong", typeof(ulong)),
        new("ulong[]", typeof(ulong[])),
        new("sbyte", typeof(sbyte)),
        new("sbyte[]", typeof(sbyte[])),
        new("short", typeof(short)),
        new("short[]", typeof(short[])),
        new("ushort", typeof(ushort)),
        new("ushort[]", typeof(ushort[])),
        new("byte", typeof(byte)),
        new("byte[]", typeof(byte[])),
    ];
}
