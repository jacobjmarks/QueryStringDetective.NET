using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using MySharedClassLib;
using Microsoft.AspNetCore.Components;

namespace MyWebApp.Pages;

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

    private IEnumerable<BindingResult> bindingResults = [];
    private bool showErroneous = true;

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
        var qs = QueryString.Create("q", "?q=" + inputValue);
        using var response = await httpClient.GetAsync(AppConfig.AzureFunctionUrl + qs);
        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<IEnumerable<BindingResult>>() ?? [];
        bindingResults = results.OrderByDescending(r => !r.IsErroneous);
    }

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}