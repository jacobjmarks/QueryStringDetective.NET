using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Shared;
using Microsoft.AspNetCore.Components;

namespace Client.Pages;

public partial class Home : IDisposable
{
    [Inject]
    private AppConfig AppConfig { get; set; } = null!;

    private string? CurrentInputValue { get; set; }

    private readonly HttpClient httpClient = new();
    private readonly TimeSpan minRequestInterval = TimeSpan.FromSeconds(1);
    private DateTime lastRequestAt = DateTime.MinValue;
    private string? lastRequestedInputValue;

    private bool isLoading = false;

    private IEnumerable<BindingResults> bindingResults = [];
    private bool showErroneous = false;

    private async Task InputOnChange()
    {
        if (CurrentInputValue == null)
            return;

        isLoading = true;

        try
        {
            if (DateTime.Now - lastRequestAt < minRequestInterval)
            {
                if (CurrentInputValue != lastRequestedInputValue)
                {
                    await Task.Delay(50);
                    await InputOnChange();
                }

                return;
            }

            lastRequestAt = DateTime.Now;
            await GetBindingResultsAsync(CurrentInputValue);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task GetBindingResultsAsync(string inputValue)
    {
        lastRequestedInputValue = inputValue;
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