using System.Text;

namespace Shared;

public static class StringExtensions
{
    public static string ToBase64(this string value, Encoding? encoding = null)
        => Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(value));
}
