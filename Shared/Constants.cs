namespace Shared;

public static class Constants
{
    public static readonly EndpointDescriptor[] Endpoints =
    [
        new("string", "/string", typeof(string)),
        new("string[]", "/string-array", typeof(string[])),
        new("char", "/char", typeof(char)),
        new("char[]", "/char-array", typeof(char[])),
        new("int", "/int", typeof(int)),
        new("int[]", "/int-array", typeof(int[])),
        new("double", "/double", typeof(double)),
        new("double[]", "/double-array", typeof(double[])),
        new("float", "/float", typeof(float)),
        new("float[]", "/float-array", typeof(float[])),
        new("decimal", "/decimal", typeof(decimal)),
        new("decimal[]", "/decimal-array", typeof(decimal[])),
        new("bool", "/bool", typeof(bool)),
        new("bool[]", "/bool-array", typeof(bool[])),
        new("uint", "/uint", typeof(uint)),
        new("uint[]", "/uint-array", typeof(uint[])),
        new("long", "/long", typeof(long)),
        new("long[]", "/long-array", typeof(long[])),
        new("ulong", "/ulong", typeof(ulong)),
        new("ulong[]", "/ulong-array", typeof(ulong[])),
        new("sbyte", "/sbyte", typeof(sbyte)),
        new("sbyte[]", "/sbyte-array", typeof(sbyte[])),
        new("short", "/short", typeof(short)),
        new("short[]", "/short-array", typeof(short[])),
        new("ushort", "/ushort", typeof(ushort)),
        new("ushort[]", "/ushort-array", typeof(ushort[])),
        new("byte", "/byte", typeof(byte)),
        new("byte[]", "/byte-array", typeof(byte[])),
    ];
}
