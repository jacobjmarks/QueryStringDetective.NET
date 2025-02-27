namespace Server.Core.Tests;

public class QueryBindingEvaluatorTests
{
    public static readonly TheoryData<string> TestCases = new()
    {
        "",
        "q=",
        "q=c",
        "q=foo",
        "q=a&q=b&q=c",
        "q=null",
        "q=0",
        "q=1",
        "q=-1",
        "q=true",
        "q=false",
        "q=yes",
        "q=no",
        "q=1&q=2&q=3",
        "q=1&q=2&q=.3",
        "q=-1&q=0&q=1",
        $"q={sbyte.MinValue}",
        $"q={sbyte.MaxValue}",
        $"q={byte.MaxValue}",
        $"q={short.MinValue}",
        $"q={short.MaxValue}",
        $"q={ushort.MaxValue}",
        $"q={int.MinValue}",
        $"q={int.MaxValue}",
        $"q={long.MinValue}",
        $"q={long.MaxValue}",
        $"q={ulong.MaxValue}",
        $"q={Uri.EscapeDataString($"{double.MinValue}")}",
        $"q={Uri.EscapeDataString($"{double.MaxValue}")}",
        $"q={decimal.MinValue}",
        $"q={decimal.MaxValue}",
        $"q={Uri.EscapeDataString($"{float.MinValue}")}",
        $"q={Uri.EscapeDataString($"{float.MaxValue}")}",
        "q=2025-01-13T15:25:00",
        "q=2025-01-13T05:25:00Z",
        "q=2025-01-13",
        "q=15:25:00",
    };

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task AssertBinding(string queryString)
    {
        using var bindingEvaluator = new QueryBindingEvaluator();
        var results = await bindingEvaluator.EvaluateAsync("?" + queryString);

        var content = $"Input: {queryString}"
            + "\n\nBindings:\n";

        foreach (var result in results)
        {
            content += "\n" + result.Type;
            content += "\n  " + string.Join("\n  ", result.Results
                .Select(r => $"{r.Key}\n    {(
                    r.Value.IsErroneous
                    ? $"e = {r.Value.Error.Message} ({r.Value.Error.Detail})"
                    : $"r = {r.Value.Result}")}"));

            content += "\n";
        }

        // "mock" the current date
        content = content.Replace(DateTime.Now.ToString("yyyy-MM-dd"), "2020-03-12");

        content.Should().MatchSnapshot(SnapshotNameExtension.Create(ToValidFileName(queryString)));
    }

    private static string ToValidFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value;
    }
}