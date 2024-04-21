namespace Server.Core.Tests;

public class QueryBindingEvaluatorTests
{
    public static readonly TheoryData<string> TestCases = new()
    {
        "c",
        "foo",
        "a&q=b&q=c",
        "null",
        "0",
        "1",
        "-1",
        "1&q=2&q=3",
        "1&q=2&q=.3",
        "-1&q=0&q=1",
        $"{sbyte.MinValue}",
        $"{sbyte.MaxValue}",
        $"{byte.MaxValue}",
        $"{short.MinValue}",
        $"{short.MaxValue}",
        $"{ushort.MaxValue}",
        $"{int.MinValue}",
        $"{int.MaxValue}",
        $"{long.MinValue}",
        $"{long.MaxValue}",
        $"{ulong.MaxValue}",
        Uri.EscapeDataString($"{double.MinValue}"),
        Uri.EscapeDataString($"{double.MaxValue}"),
        $"{decimal.MinValue}",
        $"{decimal.MaxValue}",
        Uri.EscapeDataString($"{float.MinValue}"),
        Uri.EscapeDataString($"{float.MaxValue}"),
    };

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task AssertBinding(string queryString)
    {
        using var bindingEvaluator = new QueryBindingEvaluator();
        var results = await bindingEvaluator.EvaluateAsync("?q=" + queryString);

        var content = "Input: " + queryString
            + "\n\nBindings:\n" + string.Join("\n", results.Where(r => !r.IsErroneous)
                    .Select(r => $"{r.Type,-10} {r.Result}"))
            + "\n\nErroneous:\n" + string.Join("\n", results.Where(r => r.IsErroneous)
                    .Select(r => $"{r.Type,-10} {r.Error!.Message}"));

        content.Should().MatchSnapshot(SnapshotNameExtension.Create(ToValidFileName(queryString)));
    }

    private static string ToValidFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value;
    }
}