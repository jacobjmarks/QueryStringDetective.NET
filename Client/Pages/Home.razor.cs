using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Shared;
using Microsoft.AspNetCore.Components;

namespace Client.Pages;

public partial class Home : IDisposable
{
    [Inject]
    private AppConfig AppConfig { get; set; } = null!;

    private string CurrentInputValue { get; set; } = string.Empty;

    private readonly HttpClient httpClient = new();

    private bool isLoading = false;

    private IEnumerable<BindingResults> bindingResults = [];
    private bool showErroneous = false;
    private bool showDetailedErrorMessages = false;

    private Task InputOnChange(string value)
    {
        CurrentInputValue = value ?? string.Empty;
        isLoading = true;

        return Task.CompletedTask;
    }

    private async Task InputOnDebounce(string value)
    {
        CurrentInputValue = value ?? string.Empty;
        isLoading = true;

        try
        {
            await GetBindingResultsAsync(CurrentInputValue);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task GetBindingResultsAsync(string inputValue)
    {
        var qs = QueryString.Create("qs", "?q=" + inputValue);
        using var response = await httpClient.GetAsync(AppConfig.AzureFunctionUrl + qs);
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<IEnumerable<BindingResults>>() ?? [];
        bindingResults = results.OrderBy(r => r.Results.Count(r => r.Value.IsErroneous));
    }

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}