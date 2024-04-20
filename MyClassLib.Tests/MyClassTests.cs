using System.Text.Json;
using Snapshooter;

namespace MyClassLib.Tests;

public class MyClassTests
{
    [Theory]
    [InlineData("foo")]
    [InlineData("a&q=b&q=c")]
    [InlineData("null")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("1&q=2&q=3")]
    [InlineData("1&q=2&q=.3")]
    [InlineData("-1&q=0&q=1")]
    [InlineData("2147483647")]
    [InlineData("-2147483648")]
    [InlineData("9223372036854775807")]
    [InlineData("-9223372036854775808")]
    [InlineData("18446744073709551615")]
    public async Task AssertBinding(string queryString)
    {
        var results = await MyClass.TestQueryStringBindingAsync("?q=" + queryString);
        var assertions = results.Where(r => !r.IsErroneous).Select(r => r.ToString());
        assertions.Should().MatchSnapshot(SnapshotNameExtension.Create(ToValidFileName(queryString)));
    }

    private static string ToValidFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value;
    }
}