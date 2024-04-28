namespace Shared;

public static class Constants
{
    public static readonly EndpointDescriptor[] Endpoints =
    [
        new("string", "/string"),
        new("string[]", "/string-array"),
        new("char", "/char"),
        new("char[]", "/char-array"),
        new("int", "/int"),
        new("int[]", "/int-array"),
        new("double", "/double"),
        new("double[]", "/double-array"),
        new("float", "/float"),
        new("float[]", "/float-array"),
        new("decimal", "/decimal"),
        new("decimal[]", "/decimal-array"),
        new("bool", "/bool"),
        new("bool[]", "/bool-array"),
        new("uint", "/uint"),
        new("uint[]", "/uint-array"),
        new("long", "/long"),
        new("long[]", "/long-array"),
        new("ulong", "/ulong"),
        new("ulong[]", "/ulong-array"),
        new("sbyte", "/sbyte"),
        new("sbyte[]", "/sbyte-array"),
        new("short", "/short"),
        new("short[]", "/short-array"),
        new("ushort", "/ushort"),
        new("ushort[]", "/ushort-array"),
        new("byte", "/byte"),
        new("byte[]", "/byte-array"),
    ];
}
