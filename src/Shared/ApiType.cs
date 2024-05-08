namespace Shared;

public enum ApiType
{
    MinimalApis,
    Controllers,
}

public static class ApiTypeExtensions
{
    public static string ToDisplayString(this ApiType apiType) => apiType switch
    {
        ApiType.MinimalApis => "Minimal APIs",
        ApiType.Controllers => "Controllers",
        _ => throw new NotSupportedException(),
    };
}
